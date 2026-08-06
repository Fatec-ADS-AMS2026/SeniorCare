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

## Componentes atuais

- `SeniorCareManager-Backend`: API ASP.NET Core e persistência PostgreSQL.
- `SeniorCareManager-Frontend`: interface de gestão assistencial.
- `SeniorStockManager-Frontend`: interface de estoque e suprimentos.
- `infra`: execução local, publicação e operação com Docker Compose.
- `openspec`: especificações e planejamento das evoluções do produto.

## Documentação

- [Escopo do projeto](docs/escopo-do-projeto.md)
- [Arquitetura de CI/CD](docs/infra/ci-cd-arquitetura.md)
- [Stack local para desenvolvimento](infra/docker-test/README.md)
- [Publicação em servidor](infra/deploy/README.md)
