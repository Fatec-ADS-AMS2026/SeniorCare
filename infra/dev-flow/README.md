# dev-flow — promoção dev → main com QA manual

Modelo de branches: features fazem PR contra `dev` (validado pelo `ci.yml` e
pelos workflows de segurança, igual a `main`). Ao mergear em `dev`, abre-se
automaticamente uma issue de teste manual; quando a equipe termina de testar,
alguém promove `dev` para `main` com o script deste diretório.

```
PR -> dev  (CI valida)
   │
   ▼ merge
issue [QA] criada automaticamente, com o Test plan do PR, no Project em "In review"
   │
   ▼ equipe testa, marca as caixinhas na issue, fecha a issue
promote_to_main.sh <issue> -> abre PR dev -> main (Closes #issue)
   │
   ▼ merge (main já tem branch protection própria)
release.yml dispara ao criar tag v* (build once, deploy many — ver docs/infra/ci-cd-arquitetura.md)
```

## Por que a issue de QA não é espelho (ao contrário das do openspec-sync)

As issues do `openspec-sync.yml` são espelho read-only de `tasks.md` — marcar
caixinha nelas não faz nada, porque a fonte da verdade é o git. A issue de QA
é diferente: **ela é a fonte da verdade do teste**. Não existe `tasks.md` de
teste manual no repo; o plano de teste vem do `## Test plan` que o autor
preencheu no PR (via `.github/pull_request_template.md`), copiado uma vez para
a issue. Marcar as caixinhas ali é o registro real.

## Componentes

- `.github/workflows/dev-merge-qa.yml` — dispara quando um PR é mergeado em
  `dev`; roda `create_qa_issue.py`.
- `create_qa_issue.py` — extrai o `## Test plan` do corpo do PR, cria a issue
  (label `qa`) e a posiciona no `SeniorCare Project` em "In review".
- `promote_to_main.sh` — roda localmente, manual. Abre o PR `dev -> main` com
  `Closes #<issue-de-qa>`. Avisa (mas não bloqueia) se a issue ainda estiver
  aberta.

## Por que precisa de um PAT além do GITHUB_TOKEN

Projects v2 (o board da organização) não aceita o `GITHUB_TOKEN` padrão do
Actions — só um PAT com escopo `project`. O secret `GH_PROJECT_TOKEN` cobre
só isso; tudo que é do repositório (criar issue, comentar no PR) usa o
`GITHUB_TOKEN` normal. Se o PAT expirar, gere outro (mesmo escopo `project`) e
atualize:

```
gh secret set GH_PROJECT_TOKEN --repo Fatec-ADS-AMS2026/SeniorCare
```

## Uso

```bash
# depois que a equipe termina de testar a issue #42:
./infra/dev-flow/promote_to_main.sh 42
```

## Limites conhecidos

- A issue de QA sempre entra em "In review" — não há lógica para diferenciar
  prioridade/urgência do PR. Se isso importar, ajuste manualmente no board
  depois de criada.
- `promote_to_main.sh` não valida que os testes marcados na issue realmente
  correspondem ao que foi feito — é um lembrete (`tem certeza que o teste
  terminou?`), não um gate técnico. O gate técnico continua sendo a branch
  protection de `main` (os 5 checks obrigatórios).
