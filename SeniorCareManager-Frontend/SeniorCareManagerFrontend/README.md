# SeniorCareManagerFrontend ("care")

Front-end de gestão assistencial do SeniorCare — React 18 + TypeScript + Vite,
Tailwind CSS. Consome a API em [`../../SeniorCareManager-Backend`](../../SeniorCareManager-Backend/README.md)
via `/api` (proxy same-origin — nunca aponta pra `localhost` fixo).

## O que tem hoje

- **Login e identidade**: sessão por cookie, MFA obrigatório para admin,
  ativação/recuperação de conta, troca de senha.
- **Administração de acesso**: papéis, grupos de permissão, exceções
  individuais, políticas de acesso, atribuições organizacionais, usuários
  administrativos, sessões ativas, parâmetros de segurança da instituição —
  console único (o app "stock" não duplica essas telas).
- **Catálogos**: Religião, Cargo, Plano de Saúde (CRUD com concorrência
  otimista).
- **Acessibilidade**: baseline WCAG 2.2 AA nos componentes compartilhados
  (`src/components/`) — foco visível, navegação por teclado, alto contraste e
  tamanho de fonte ajustáveis (`AccessibilityPage`/`AccessibilityBar`).

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
(`.github/workflows/ci.yml`, job `care-web`), com cobertura publicada como
artefato.

## Documentação relacionada

- [README raiz do monorepo](../../README.md)
- [Bootstrap da instituição e do administrador inicial](../../infra/deploy/BOOTSTRAP.md)
