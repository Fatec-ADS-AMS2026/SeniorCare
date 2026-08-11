# SeniorCare

O SeniorCare é uma plataforma de gestão integrada para casas de cuidados e
Instituições de Longa Permanência para Pessoas Idosas (ILPIs). O produto tem como
centro a pessoa idosa e articula assistência cotidiana, acompanhamento
multidisciplinar de saúde, alimentação, convivência, gestão financeira, estoque,
doações, conformidade institucional e dashboards estatísticos.

O projeto é uma iniciativa de atuação da universidade junto à sociedade e tem
como público prioritário inicial ILPIs beneficentes e sem fins lucrativos. Seu
desenvolvimento combina extensão universitária, formação prática e produção de
tecnologia social sustentável.

O escopo conceitual e funcional canônico do projeto está documentado em
[`docs/escopo-do-projeto.md`](docs/escopo-do-projeto.md).

## Estado atual da implementação

A mudança OpenSpec `stabilize-existing-platform` (arquivada em
[`openspec/changes/archive/2026-08-11-stabilize-existing-platform/`](openspec/changes/archive/2026-08-11-stabilize-existing-platform/),
specs canônicas em [`openspec/specs/`](openspec/specs/)) entregou a **fundação
técnica e de segurança** da plataforma — não o núcleo assistencial em si
(residente, cuidado, prontuário — ver seção seguinte). O que já existe e está
testado hoje:

- **Autenticação e autorização institucional**: login por sessão (cookie
  `HttpOnly`+`Secure`), MFA obrigatório para conta administrativa, ativação e
  recuperação de conta, política de senha configurável. Controle de acesso por
  papéis, grupos de permissão, exceções individuais justificadas e políticas
  com escopo institucional/organizacional — a API é sempre a autoridade final
  de decisão, os front-ends só refletem.
- **Auditoria**: autenticações, MFA, ativações, mudanças de estado de conta,
  sessões e decisões de acesso protegido são registradas de forma imutável.
- **9 catálogos administrativos + Produto**: contrato uniforme (paginação,
  filtro, concorrência otimista via `RowVersion`), CRUD completo nos dois
  front-ends, contrato OpenAPI publicado e validado contra o código real dos
  dois clientes.
- **Baseline de acessibilidade WCAG 2.2 AA** nos componentes compartilhados e
  nas jornadas críticas (login, navegação, tabelas, formulários, modais) —
  verificação automatizada (`jest-axe`) + navegação por teclado mecanizada.
- **CI/CD real**: build, lint, teste com cobertura (backend e os dois
  front-ends), análise de segurança (CodeQL, SCA, secret scanning), migração
  de banco testada em banco vazio e sobre dado pré-existente, pré-validação de
  migração em produção antes de cada deploy, deploy pull-based com backup
  automático e rollback — ver [Arquitetura de CI/CD](docs/infra/ci-cd-arquitetura.md).

O que **ainda não existe**: residente, plano de cuidado, prontuário
multidisciplinar, alimentação, financeiro, doações, conformidade e dashboards
— o núcleo funcional descrito em `docs/escopo-do-projeto.md` continua como
trabalho futuro, deliberadamente fora do escopo desta mudança. Avaliação
detalhada requisito-a-requisito em
[docs/relatorio-avaliacao-requisitos-implementacao.md](docs/relatorio-avaliacao-requisitos-implementacao.md)
(seção 14 tem o resumo pós-`stabilize-existing-platform`).

## Componentes atuais

- [`SeniorCareManager-Backend`](SeniorCareManager-Backend/README.md): API ASP.NET Core e persistência PostgreSQL.
- [`SeniorCareManager-Frontend`](SeniorCareManager-Frontend/SeniorCareManagerFrontend/README.md): interface de gestão assistencial ("care").
- [`SeniorStockManager-Frontend`](SeniorStockManager-Frontend/SeniorStockManagerFrontend/README.md): interface de estoque e suprimentos ("stock").
- `infra`: execução local, publicação e operação com Docker Compose.
- `openspec`: especificações e planejamento das evoluções do produto.

## Documentação

- [Escopo do projeto](docs/escopo-do-projeto.md)
- [Avaliação dos requisitos frente à implementação](docs/relatorio-avaliacao-requisitos-implementacao.md)
- [Arquitetura de CI/CD](docs/infra/ci-cd-arquitetura.md)
- [Stack local para desenvolvimento](infra/docker-test/README.md)
- [Publicação em servidor](infra/deploy/README.md)
- [Bootstrap da instituição e do administrador inicial](infra/deploy/BOOTSTRAP.md)
