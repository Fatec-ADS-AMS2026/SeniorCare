# SeniorStockManagerFrontend ("stock")

Front-end de estoque e suprimentos do SeniorCare — React 18 + TypeScript +
Vite, Tailwind CSS. Consome a API em
[`../../SeniorCareManager-Backend`](../../SeniorCareManager-Backend/README.md)
via `/api` (proxy same-origin — nunca aponta pra `localhost` fixo).

## O que tem hoje

- **Login e identidade**: sessão por cookie, MFA obrigatório para admin,
  ativação/recuperação de conta, troca de senha — mesmo modelo de identidade
  do app "care" (a API é uma só, compartilhada pelos dois front-ends).
- **Catálogos de estoque**: Transportadora, Fabricante, Fornecedor, Grupo de
  Produto, Tipo de Produto, Unidade de Medida — CRUD com concorrência
  otimista.
- **Produto**: cadastro completo (custo, estoque mínimo/atual, validade,
  alto custo), integrado com os catálogos acima.
- **Acessibilidade**: baseline WCAG 2.2 AA nos componentes compartilhados
  (`src/components/`) — foco visível, navegação por teclado, alto contraste e
  tamanho de fonte ajustáveis.

Este app **não** tem as telas de administração de acesso (papéis, grupos de
permissão etc.) — essas ficam só no app "care" (console único, decisão de
produto).

## Rodando localmente

```bash
npm install
npm run dev          # abre com proxy /api -> backend local
```

Precisa da API rodando (ver
[`../../infra/docker-test/README.md`](../../infra/docker-test/README.md) pra
subir a stack completa via Docker, ou `dotnet run` direto no backend).

## Scripts

| Comando | Uso |
|---|---|
| `npm run dev` | Servidor de desenvolvimento (Vite + HMR) |
| `npm run build` | `tsc -b` + build de produção |
| `npm run lint` | ESLint |
| `npm test` | Testes (Vitest + Testing Library) |
| `npm run test:coverage` | Testes com cobertura |

## Testes

Mock de `@/features/api` (nunca chama rede de verdade) +
`@testing-library/react`/`user-event`. Inclui testes de acessibilidade
(`jest-axe`, `src/test/accessibility.test.tsx`) e de navegação só por teclado
(`src/test/keyboardNavigation.test.tsx`). Já ligado ao CI
(`.github/workflows/ci.yml`, job `stock-web`), com cobertura publicada como
artefato.

## Documentação relacionada

- [README raiz do monorepo](../../README.md)
- [Bootstrap da instituição e do administrador inicial](../../infra/deploy/BOOTSTRAP.md)
