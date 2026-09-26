#!/usr/bin/env python3
"""Sincroniza seções de tarefas do Spec Kit com issues do GitHub.

Direção única: `specs/<NNN-feature>/tasks.md` é a fonte da verdade e a issue é
um espelho. Marcar uma caixa na issue não altera o repositório; a sincronização
seguinte reescreve o corpo.

Uso:
    sync_issues.py --feature introduce-senior-portal --dry-run
    sync_issues.py --feature introduce-senior-portal
    sync_issues.py --all --include-completed
"""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from dataclasses import dataclass, field
from pathlib import Path

REPO = "Fatec-ADS-AMS2026/SeniorCare"
REPO_ROOT = Path(__file__).resolve().parents[2]
SPECS_DIR = REPO_ROOT / "specs"

SECTION_RE = re.compile(r"^## (\d+)\.\s+(.*)$")
TASK_RE = re.compile(
    r"^- \[([ xX])\]\s+(T\d+)\s+\(legado ([\d.]+)\)\s+(.*)$"
)
EPIC_ID_RE = re.compile(r"\b([A-Z]{2,}-EP\d+)\b")
MARKER_RE = re.compile(
    r"<!-- (?:openspec|speckit)-sync: (?:change|feature)=(\S+) section=(\d+) -->"
)

BASE_LABEL = "spec-kit"


@dataclass
class Task:
    task_id: str
    legacy_number: str
    done: bool
    text: str

    def render(self) -> str:
        checked = "x" if self.done else " "
        return (
            f"- [{checked}] {self.task_id} "
            f"(legado {self.legacy_number}) {self.text}"
        )


@dataclass
class Section:
    number: int
    title: str
    tasks: list[Task] = field(default_factory=list)

    @property
    def epic_id(self) -> str | None:
        match = EPIC_ID_RE.search(self.title)
        return match.group(1) if match else None

    @property
    def short_title(self) -> str:
        if self.epic_id:
            trimmed = self.title.replace(self.epic_id, "", 1)
            return trimmed.lstrip(" —-–:").strip()
        return self.title.strip()

    @property
    def done_count(self) -> int:
        return sum(1 for task in self.tasks if task.done)

    @property
    def is_complete(self) -> bool:
        return bool(self.tasks) and self.done_count == len(self.tasks)


def run_gh(args: list[str], check: bool = True) -> str:
    result = subprocess.run(
        ["gh", *args], capture_output=True, text=True, check=False
    )
    if check and result.returncode != 0:
        raise RuntimeError(f"gh {' '.join(args)} falhou: {result.stderr.strip()}")
    return result.stdout


def feature_slug(directory_name: str) -> str:
    return re.sub(r"^\d{3}-", "", directory_name)


def resolve_feature_dir(feature: str) -> Path:
    exact = SPECS_DIR / feature
    if (exact / "tasks.md").is_file():
        return exact

    matches = [
        path
        for path in SPECS_DIR.glob(f"[0-9][0-9][0-9]-{feature}")
        if (path / "tasks.md").is_file()
    ]
    if len(matches) != 1:
        raise SystemExit(
            f"feature '{feature}' não encontrada de forma inequívoca em {SPECS_DIR}"
        )
    return matches[0]


def parse_tasks(path: Path) -> list[Section]:
    sections: list[Section] = []
    current: Section | None = None
    for line in path.read_text(encoding="utf-8").splitlines():
        section_match = SECTION_RE.match(line)
        if section_match:
            current = Section(
                int(section_match.group(1)), section_match.group(2).strip()
            )
            sections.append(current)
            continue
        task_match = TASK_RE.match(line)
        if task_match and current is not None:
            current.tasks.append(
                Task(
                    task_id=task_match.group(2),
                    legacy_number=task_match.group(3),
                    done=task_match.group(1).lower() == "x",
                    text=task_match.group(4).strip(),
                )
            )
    return [section for section in sections if section.tasks]


def issue_title(feature: str, section: Section) -> str:
    if section.epic_id:
        return f"[{section.epic_id}] {section.short_title}"
    return f"[{feature}] §{section.number} {section.short_title}"


def issue_body(feature: str, section: Section, tasks_path: Path) -> str:
    marker = f"<!-- speckit-sync: feature={feature} section={section.number} -->"
    relative_path = tasks_path.relative_to(REPO_ROOT)
    source = f"https://github.com/{REPO}/blob/main/{relative_path}"
    checklist = "\n".join(task.render() for task in section.tasks)
    return f"""{marker}

**Feature Spec Kit:** `{feature}`
**Seção:** §{section.number} — {section.title}
**Progresso:** {section.done_count}/{len(section.tasks)}

### Tarefas

{checklist}

---

Espelho de [`{relative_path}`]({source}), que é a fonte da verdade. Marcar
caixinha aqui não altera o repositório: o corpo é reescrito a cada sincronização.
Para mudar o estado, edite o `tasks.md` e faça merge na `main`.

Gerada por `infra/speckit-github/sync_issues.py`.
"""


def existing_issues() -> dict[tuple[str, int], dict]:
    raw = run_gh(
        [
            "issue",
            "list",
            "--repo",
            REPO,
            "--state",
            "all",
            "--limit",
            "500",
            "--json",
            "number,body,title,state",
        ]
    )
    found: dict[tuple[str, int], dict] = {}
    for issue in json.loads(raw):
        match = MARKER_RE.search(issue.get("body") or "")
        if match:
            found[(match.group(1), int(match.group(2)))] = issue
    return found


def ensure_labels(features: set[str], dry_run: bool) -> None:
    raw = run_gh(
        ["label", "list", "--repo", REPO, "--limit", "200", "--json", "name"]
    )
    have = {label["name"] for label in json.loads(raw)}
    wanted = {
        BASE_LABEL: ("0e8a16", "Épico rastreado a partir do Spec Kit"),
    }
    for feature in features:
        wanted[f"feature:{feature}"] = (
            "1d76db",
            f"Feature Spec Kit {feature}",
        )
    for name, (color, description) in wanted.items():
        if name in have:
            continue
        print(f"  + label {name}")
        if not dry_run:
            run_gh(
                [
                    "label",
                    "create",
                    name,
                    "--repo",
                    REPO,
                    "--color",
                    color,
                    "--description",
                    description,
                ]
            )


def sync_feature(
    feature: str,
    include_completed: bool,
    dry_run: bool,
    known: dict[tuple[str, int], dict],
) -> list[str]:
    tasks_path = resolve_feature_dir(feature) / "tasks.md"
    sections = parse_tasks(tasks_path)
    touched: list[str] = []

    for section in sections:
        if section.is_complete and not include_completed:
            if (feature, section.number) not in known:
                continue
        title = issue_title(feature, section)
        body = issue_body(feature, section, tasks_path)
        labels = [BASE_LABEL, f"feature:{feature}"]
        current = known.get((feature, section.number))

        if current is None:
            print(
                f"  + criar  {title}  "
                f"({section.done_count}/{len(section.tasks)})"
            )
            touched.append(title)
            if not dry_run:
                url = run_gh(
                    [
                        "issue",
                        "create",
                        "--repo",
                        REPO,
                        "--title",
                        title,
                        "--body",
                        body,
                        *sum((["--label", label] for label in labels), []),
                    ]
                ).strip()
                print(f"          {url}")
            continue

        number = current["number"]
        print(
            f"  ~ atualizar #{number}  {title}  "
            f"({section.done_count}/{len(section.tasks)})"
        )
        touched.append(title)
        if not dry_run:
            run_gh(
                [
                    "issue",
                    "edit",
                    str(number),
                    "--repo",
                    REPO,
                    "--title",
                    title,
                    "--body",
                    body,
                    *sum((["--add-label", label] for label in labels), []),
                ]
            )
            if section.is_complete and current["state"] != "CLOSED":
                run_gh(
                    [
                        "issue",
                        "close",
                        str(number),
                        "--repo",
                        REPO,
                        "--comment",
                        "Todas as tarefas da seção foram concluídas no `tasks.md`.",
                    ]
                )
            elif not section.is_complete and current["state"] == "CLOSED":
                run_gh(["issue", "reopen", str(number), "--repo", REPO])

    return touched


def configured_features() -> list[str]:
    config = Path(__file__).with_name("synced-features.txt")
    return [
        line.strip()
        for line in config.read_text(encoding="utf-8").splitlines()
        if line.strip() and not line.lstrip().startswith("#")
    ]


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--feature", action="append", dest="features", default=[])
    parser.add_argument("--all", action="store_true", help="todas as features")
    parser.add_argument(
        "--configured",
        action="store_true",
        help="apenas as features listadas em synced-features.txt",
    )
    parser.add_argument(
        "--include-completed",
        action="store_true",
        help="também cria issue para seção 100%% concluída",
    )
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    if args.all:
        features = sorted(
            feature_slug(path.name)
            for path in SPECS_DIR.iterdir()
            if (path / "tasks.md").is_file()
        )
    elif args.configured:
        features = configured_features()
    elif args.features:
        features = args.features
    else:
        parser.error("informe --feature <id>, --configured ou --all")

    if not features:
        print("Nenhuma feature configurada — nada a fazer.")
        return 0

    print(f"Repositório: {REPO}")
    print(f"Modo: {'dry-run' if args.dry_run else 'aplicando'}")
    ensure_labels(set(features), args.dry_run)
    known = existing_issues()

    for feature in features:
        print(f"\n{feature}")
        sync_feature(
            feature,
            include_completed=args.include_completed,
            dry_run=args.dry_run,
            known=known,
        )

    return 0


if __name__ == "__main__":
    sys.exit(main())
