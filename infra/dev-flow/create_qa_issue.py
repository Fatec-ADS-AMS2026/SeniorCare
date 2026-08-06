#!/usr/bin/env python3
"""Cria a issue de QA quando um PR é mergeado em `dev`.

Extrai a seção "## Test plan" do corpo do PR (preenchida pelo autor via
.github/pull_request_template.md) e cria uma issue rotulada `qa`, com esse
checklist, no repositório. Em seguida coloca a issue no GitHub Project em
"In review" — a etapa de teste manual da equipe antes de promover para `main`
via infra/dev-flow/promote_to_main.sh.

O comentário na issue não é sincronizado de volta a lugar nenhum (ao contrário
das issues de openspec-sync.yml): esta issue É a fonte de trabalho da equipe de
teste, não um espelho — marcar caixinha aqui é o registro real do progresso.

Requer dois tokens distintos (`gh` só usa um GH_TOKEN por vez):
  GH_TOKEN          — permissões normais do repo (issues, PRs). GITHUB_TOKEN
                       do Actions já serve.
  GH_PROJECT_TOKEN  — PAT classic com escopo `project` (Projects v2 não aceita
                       o GITHUB_TOKEN padrão do Actions). Ver infra/dev-flow/README.md.

Uso:
    create_qa_issue.py --pr 42
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys

REPO = "Fatec-ADS-AMS2026/SeniorCare"
PROJECT_OWNER = "Fatec-ADS-AMS2026"
PROJECT_NUMBER = "2"
STATUS_FIELD_NAME = "Status"
TARGET_STATUS_NAME = "In review"

TEST_PLAN_RE = re.compile(r"##\s*Test plan\s*\n(.*?)(?=\n##\s|\Z)", re.DOTALL | re.IGNORECASE)


def run_gh(args: list[str], token_env: str, check: bool = True) -> str:
    token = os.environ.get(token_env)
    if not token:
        raise SystemExit(f"variável de ambiente {token_env} não definida")
    env = {**os.environ, "GH_TOKEN": token}
    result = subprocess.run(["gh", *args], capture_output=True, text=True, env=env, check=False)
    if check and result.returncode != 0:
        raise RuntimeError(f"gh {' '.join(args)} falhou: {result.stderr.strip()}")
    return result.stdout


def repo_gh(args: list[str], check: bool = True) -> str:
    return run_gh(args, "GH_TOKEN", check=check)


def project_gh(args: list[str], check: bool = True) -> str:
    return run_gh(args, "GH_PROJECT_TOKEN", check=check)


def extract_test_plan(body: str) -> str:
    match = TEST_PLAN_RE.search(body or "")
    if not match:
        return "_PR não tinha seção `## Test plan` preenchida — defina o plano de teste nesta issue._"
    content = match.group(1).strip()
    if not content or content in {"- [ ]", "-[ ]"}:
        return "_Autor do PR não preencheu o plano de teste — defina o plano nesta issue._"
    return content


def ensure_label(name: str, color: str, description: str) -> None:
    raw = repo_gh(["label", "list", "--repo", REPO, "--limit", "200", "--json", "name"])
    have = {label["name"] for label in json.loads(raw)}
    if name in have:
        return
    repo_gh(["label", "create", name, "--repo", REPO, "--color", color, "--description", description])


def create_issue(pr: dict) -> str:
    title = f"[QA] {pr['title']} (#{pr['number']})"
    test_plan = extract_test_plan(pr.get("body") or "")
    body = f"""**PR mergeado em `dev`:** {pr['url']}

### Plano de teste

{test_plan}

---

Issue de teste manual — marcar as caixinhas aqui é o registro real do
progresso (ao contrário das issues de `openspec-sync.yml`, esta não é espelho
de nenhum `tasks.md`). Quando o teste terminar, promova para `main` com:

```
infra/dev-flow/promote_to_main.sh <número desta issue>
```

Gerada por `infra/dev-flow/create_qa_issue.py`.
"""
    url = repo_gh([
        "issue", "create", "--repo", REPO, "--title", title, "--body", body,
        "--label", "qa",
    ]).strip()
    return url


def resolve_status_option() -> tuple[str, str, str]:
    """Retorna (project_id, status_field_id, option_id) para TARGET_STATUS_NAME."""
    project_id = project_gh([
        "project", "view", PROJECT_NUMBER, "--owner", PROJECT_OWNER, "--format", "json", "--jq", ".id",
    ]).strip()
    raw = project_gh([
        "project", "field-list", PROJECT_NUMBER, "--owner", PROJECT_OWNER, "--format", "json", "--limit", "200",
    ])
    data = json.loads(raw)
    for f in data.get("fields", []):
        if f.get("name") == STATUS_FIELD_NAME:
            for opt in f.get("options", []):
                if opt.get("name") == TARGET_STATUS_NAME:
                    return project_id, f["id"], opt["id"]
    raise SystemExit(f"campo '{STATUS_FIELD_NAME}' ou opção '{TARGET_STATUS_NAME}' não encontrados no project")


def add_to_project(issue_url: str) -> None:
    project_id, status_field_id, option_id = resolve_status_option()
    item_id = project_gh([
        "project", "item-add", PROJECT_NUMBER, "--owner", PROJECT_OWNER,
        "--url", issue_url, "--format", "json", "--jq", ".id",
    ]).strip()
    project_gh([
        "project", "item-edit",
        "--project-id", project_id,
        "--id", item_id,
        "--field-id", status_field_id,
        "--single-select-option-id", option_id,
    ])


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--pr", required=True, type=int)
    args = parser.parse_args()

    pr_raw = repo_gh([
        "pr", "view", str(args.pr), "--repo", REPO,
        "--json", "number,title,url,body",
    ])
    pr = json.loads(pr_raw)

    ensure_label("qa", "fbca04", "Issue de teste manual gerada ao mergear PR em dev")

    print(f"Criando issue de QA para PR #{pr['number']} — {pr['title']}")
    issue_url = create_issue(pr)
    print(f"  {issue_url}")

    print(f"Adicionando ao project (owner={PROJECT_OWNER} #{PROJECT_NUMBER}), status={TARGET_STATUS_NAME}")
    add_to_project(issue_url)

    issue_number = issue_url.rstrip("/").rsplit("/", 1)[-1]
    repo_gh([
        "pr", "comment", str(pr["number"]), "--repo", REPO,
        "--body", f"Issue de QA aberta: #{issue_number}",
    ])

    return 0


if __name__ == "__main__":
    sys.exit(main())
