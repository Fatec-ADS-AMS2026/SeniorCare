# Constituição do SeniorCare

## Princípios fundamentais

### I. Pessoa idosa e escopo explícito

Toda feature MUST declarar domínios e atores afetados e permanecer coerente com
`docs/escopo-do-projeto.md`. Fluxos assistenciais MUST manter a pessoa idosa e o
plano individual no centro. Capacidade apenas planejada MUST NOT aparecer como
implementada ou disponível. Objetivos, não objetivos, riscos assistenciais e
impacto regulatório MUST constar na especificação.

### II. Privacidade e segurança por padrão

Dados pessoais e de saúde MUST ser minimizados, protegidos por instituição e
escopo e nunca expostos desnecessariamente em resposta, log ou erro. Senhas,
tokens, segredos MFA, credenciais externas e conteúdo clínico MUST NOT ser
registrados. Mudanças de autenticação MUST preservar rotação, revogação,
proteção contra enumeração e cookies protegidos. SQL injection, path traversal,
XSS, SSRF, desserialização insegura e segredo versionado são bloqueadores.

### III. Autorização e auditoria no recurso

A API é a autoridade final. Operações protegidas MUST validar pertencimento ao
recurso, instituição e escopo, não apenas papel ou permissão genérica. Operações
sensíveis MUST registrar auditoria suficiente para reconstruir ator, ação,
alvo, resultado, instante e correlação, sem incluir segredos ou dados clínicos
desnecessários. Prontuários MUST preservar autoria profissional, imutabilidade,
correções por adendo, controle de acesso e visão longitudinal.

### IV. Evidência verificável

Cada requisito `MUST`/`SHALL` e cenário `WHEN`/`THEN` MUST possuir implementação
e verificação proporcionais ao risco antes de a tarefa correspondente ser
concluída. Mudanças MUST exercitar o caminho real afetado; testes permanentes
devem cobrir comportamento, limites, invariantes, transições e falhas
observáveis. Build, lint, testes, segurança e documentação afetada MUST passar
pelos gates existentes.

### V. Solução simples e compatível

A implementação MUST reutilizar arquitetura, padrões e dependências existentes.
Código especulativo, abstração sem necessidade e segundo padrão concorrente
SHOULD NOT ser introduzidos. Alterações de contrato MUST declarar migração,
compatibilidade e rollback. Código obsoleto pela mudança MUST ser removido após
a migração de todos os consumidores.

## Restrições do produto

A stack atual é ASP.NET Core/.NET 8 com Entity Framework e PostgreSQL, três
front-ends React/TypeScript/Vite, Docker Compose e GitHub Actions. Obrigação
regulatória, regra institucional configurável e boa prática recomendada MUST
permanecer distintas. Indicadores e dashboards MUST especificar fórmula,
população, período, fonte, atualização, permissões, limitações e proteção contra
reidentificação. Assinatura eletrônica ou operação sem papel exige especificação
e validação próprias; o sistema MUST NOT custodiar chaves privadas.

## Fluxo de desenvolvimento e revisão

Cada evolução relevante usa `specs/<NNN-feature>/spec.md`, `plan.md` e
`tasks.md`. A especificação define comportamento e cenários; o plano registra
decisões, riscos e migração; as tarefas mantêm IDs `Tnnn`, caminhos concretos e
evidência. Revisões determinam o objetivo primeiro pela feature Spec Kit citada
ou alterada, depois pela issue vinculada e por fim pelo PR. Requisito normativo,
cenário ou tarefa concluída sem código e verificação correspondentes é
bloqueante. Descobertas de implementação que alterem o contrato MUST reconciliar
spec, plano e tarefas antes da conclusão.

## Governança

Esta constituição prevalece sobre templates e convenções locais incompatíveis.
Emendas exigem justificativa, impacto sobre features existentes e plano de
migração quando mudarem um contrato. O versionamento segue SemVer: princípio
removido ou incompatível incrementa MAJOR; princípio novo ou materialmente
ampliado incrementa MINOR; esclarecimento sem mudança normativa incrementa
PATCH. Toda revisão de PR MUST verificar conformidade com esta constituição.

**Version**: 1.0.0 | **Ratified**: 2026-09-26 | **Last Amended**: 2026-09-26
