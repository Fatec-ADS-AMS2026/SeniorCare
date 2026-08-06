#!/usr/bin/env python3
"""Sincroniza épicos do OpenSpec com issues do GitHub.

Direção única: `openspec/changes/<change>/tasks.md` é a fonte da verdade e a
issue é espelho. Marcar caixinha na issue não altera o repositório — a próxima
execução sobrescreve o corpo. O estado de trabalho continua versionado no git.

Cada seção `## N. Título` de um tasks.md vira uma issue. Quando o título traz um
id de épico (ex.: `CARE-EP03`), ele nomeia a issue; caso contrário usa-se o
número da seção. A reconciliação entre execuções é feita por um marcador HTML
no corpo, não pelo título — assim renomear um épico não duplica a issue.

Uso:
    sync_issues.py --change stabilize-existing-platform --dry-run
    sync_issues.py --change stabilize-existing-platform
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
CHANGES_DIR = REPO_ROOT / "openspec" / "changes"

SECTION_RE = re.compile(r"^## (\d+)\.\s+(.*)$")
TASK_RE = re.compile(r"^- \[([ xX])\]\s+([\d.]+)\s+(.*)$")
EPIC_ID_RE = re.compile(r"\b([A-Z]{2,}-EP\d+)\b")
MARKER_RE = re.compile(r"<!-- openspec-sync: change=(\S+) section=(\d+) -->")

BASE_LABEL = "openspec"


@dataclass
class Task:
    number: str
    done: bool
    text: str

    def render(self) -> str:
        return f"- [{'x' if self.done else ' '}] {self.number} {self.text}"


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
        """Título sem o prefixo do id do épico, que já vai entre colchetes."""
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


def parse_tasks(path: Path) -> list[Section]:
    sections: list[Section] = []
    current: Section | None = None
    for line in path.read_text(encoding="utf-8").splitlines():
        section_match = SECTION_RE.match(line)
        if section_match:
            current = Section(int(section_match.group(1)), section_match.group(2).strip())
            sections.append(current)
            continue
        task_match = TASK_RE.match(line)
        if task_match and current is not None:
            current.tasks.append(
                Task(
                    number=task_match.group(2),
                    done=task_match.group(1).lower() == "x",
                    text=task_match.group(3).strip(),
                )
            )
    return [section for section in sections if section.tasks]


def issue_title(change: str, section: Section) -> str:
    if section.epic_id:
        return f"[{section.epic_id}] {section.short_title}"
    return f"[{change}] §{section.number} {section.short_title}"


def issue_body(change: str, section: Section, tasks_path: Path) -> str:
    marker = f"<!-- openspec-sync: change={change} section={section.number} -->"
    rel = tasks_path.relative_to(REPO_ROOT)
    source = f"https://github.com/{REPO}/blob/main/{rel}"
    checklist = "\n".join(task.render() for task in section.tasks)
    return f"""{marker}

**Change OpenSpec:** `{change}`
**Seção:** §{section.number} — {section.title}
**Progresso:** {section.done_count}/{len(section.tasks)}

### Tarefas

{checklist}

---

Espelho de [`{rel}`]({source}), que é a fonte da verdade. Marcar caixinha aqui
não altera o repositório: o corpo é reescrito a cada sincronização. Para mudar o
estado, edite o `tasks.md` e faça merge na `main`.

Gerada por `infra/openspec-github/sync_issues.py`.
"""


def existing_issues() -> dict[tuple[str, int], dict]:
    raw = run_gh(
        [
            "issue", "list", "--repo", REPO, "--state", "all",
            "--limit", "500", "--json", "number,body,title,state",
        ]
    )
    found: dict[tuple[str, int], dict] = {}
    for issue in json.loads(raw):
        match = MARKER_RE.search(issue.get("body") or "")
        if match:
            found[(match.group(1), int(match.group(2)))] = issue
    return found


def ensure_labels(changes: set[str], dry_run: bool) -> None:
    raw = run_gh(["label", "list", "--repo", REPO, "--limit", "200", "--json", "name"])
    have = {label["name"] for label in json.loads(raw)}
    wanted = {
        BASE_LABEL: ("0e8a16", "Épico rastreado a partir do OpenSpec"),
    }
    for change in changes:
        wanted[f"change:{change}"] = ("1d76db", f"Change OpenSpec {change}")
    for name, (color, description) in wanted.items():
        if name in have:
            continue
        print(f"  + label {name}")
        if not dry_run:
            run_gh([
                "label", "create", name, "--repo", REPO,
                "--color", color, "--description", description,
            ])


def sync_change(change: str, include_completed: bool, dry_run: bool) -> list[str]:
    tasks_path = CHANGES_DIR / change / "tasks.md"
    if not tasks_path.is_file():
        raise SystemExit(f"tasks.md não encontrado para o change '{change}'")

    sections = parse_tasks(tasks_path)
    known = existing_issues()
    touched: list[str] = []

    for section in sections:
        if section.is_complete and not include_completed:
            if (change, section.number) not in known:
                continue
        title = issue_title(change, section)
        body = issue_body(change, section, tasks_path)
        labels = [BASE_LABEL, f"change:{change}"]
        current = known.get((change, section.number))

        if current is None:
            print(f"  + criar  {title}  ({section.done_count}/{len(section.tasks)})")
            touched.append(title)
            if not dry_run:
                url = run_gh([
                    "issue", "create", "--repo", REPO, "--title", title,
                    "--body", body, *sum((["--label", l] for l in labels), []),
                ]).strip()
                print(f"          {url}")
            continue

        number = current["number"]
        print(f"  ~ atualizar #{number}  {title}  ({section.done_count}/{len(section.tasks)})")
        touched.append(title)
        if not dry_run:
            run_gh([
                "issue", "edit", str(number), "--repo", REPO,
                "--title", title, "--body", body,
            ])
            # Épico concluído fecha a issue; reaberto reabre. Evita divergência
            # quando uma tarefa volta a ficar em aberto no tasks.md.
            if section.is_complete and current["state"] != "CLOSED":
                run_gh(["issue", "close", str(number), "--repo", REPO,
                        "--comment", "Todas as tarefas da seção concluídas no `tasks.md`."])
            elif not section.is_complete and current["state"] == "CLOSED":
                run_gh(["issue", "reopen", str(number), "--repo", REPO])

    return touched


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--change", action="append", dest="changes", default=[])
    parser.add_argument("--all", action="store_true", help="todos os changes ativos")
    parser.add_argument("--configured", action="store_true",
                        help="apenas os changes listados em synced-changes.txt")
    parser.add_argument("--include-completed", action="store_true",
                        help="também cria issue para seção 100%% concluída")
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    if args.all:
        changes = sorted(
            path.name for path in CHANGES_DIR.iterdir()
            if (path / "tasks.md").is_file()
        )
    elif args.configured:
        config = Path(__file__).with_name("synced-changes.txt")
        changes = [
            line.strip() for line in config.read_text(encoding="utf-8").splitlines()
            if line.strip() and not line.lstrip().startswith("#")
        ]
        if not changes:
            print("Nenhum change configurado em synced-changes.txt — nada a fazer.")
            return 0
    elif args.changes:
        changes = args.changes
    else:
        parser.error("informe --change <id>, --configured ou --all")

    print(f"Repositório: {REPO}")
    print(f"Modo: {'dry-run' if args.dry_run else 'aplicando'}")
    ensure_labels(set(changes), args.dry_run)

    for change in changes:
        print(f"\n{change}")
        sync_change(change, args.include_completed, args.dry_run)

    return 0


if __name__ == "__main__":
    sys.exit(main())
