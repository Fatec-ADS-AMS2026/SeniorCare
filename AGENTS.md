## Code Review Rules

### Entrega e OpenSpec

- Determine o objetivo do PR pela change OpenSpec citada ou alterada, depois pela
  issue vinculada e, por último, pelo título e pela descrição. Não presuma
  requisitos ausentes a partir do diff.
- Abra o parecer com um veredito: **entrega**, **entrega parcial** (indicando o
  que falta) ou **diverge do que foi especificado**.
- Trate como bloqueante requisito `MUST`/`SHALL`, cenário `WHEN`/`THEN` ou tarefa
  OpenSpec marcada como concluída que não tenha implementação e teste
  correspondentes. Identifique o requisito, cenário ou item afetado.
- Aponte divergências entre código e especificação, regressões de cenários já
  publicados em `openspec/specs/` e alterações sem relação com o objetivo do PR.

### Autenticação e autorização

- A autenticação está sendo adotada progressivamente: não exija `[Authorize]`
  onde nenhum endpoint equivalente do mesmo controller ou módulo já o utiliza.
  Quando o módulo já estiver protegido, trate inconsistências novas como achado.
- Verifique autorização no recurso, instituição e escopo, não apenas papel ou
  permissão genérica. Endpoint que aceita identificador controlável pelo usuário
  e acessa recurso sem validar pertencimento pode introduzir IDOR.
- Mudanças em sessão, MFA, tokens, cookies ou credenciais devem preservar rotação,
  revogação, proteção contra enumeração e ausência de segredo em log ou resposta.

### LGPD e segurança

- Sinalize dados pessoais ou de saúde de residentes, trabalhadores ou terceiros
  em logs e erros sem mascaramento, campos pessoais retornados sem necessidade e
  exclusão física onde o módulo exige histórico ou exclusão lógica.
- Trate como bloqueantes SQL injection, path traversal, XSS, SSRF, desserialização
  insegura, segredo versionado e remoção de controles de segurança introduzidos
  ou agravados pelo PR.
- Para operações sensíveis, confirme auditoria suficiente sem registrar token,
  senha, segredo MFA, conteúdo clínico desnecessário ou credencial externa.

### Correção e qualidade do parecer

- Procure bugs concretos no fluxo alterado, tratamento de erro ausente, exceção
  descartada, N+1 em listagem e falta de transação em escrita composta.
- Reporte somente problemas reais introduzidos ou agravados pelo PR. Para cada
  achado, informe arquivo e linha, impacto concreto e correção segura; em risco de
  segurança, descreva o cenário de exploração.
- Separe bloqueadores de sugestões e omita elogios, resumo do diff, preferências
  de estilo e verificações já cobertas pelos gates determinísticos.
- Se não houver achado além do veredito de entrega, diga isso em uma linha e pare.

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

When the user types `/graphify`, use the installed graphify skill or instructions before doing anything else.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- Dirty graphify-out/ files are expected after hooks or incremental updates; dirty graph files are not a reason to skip graphify. Only skip graphify if the task is about stale or incorrect graph output, or the user explicitly says not to use it.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
