# Contratos pré-implementação — Senior Portal

> **Documento de referência**, produzido pela §1 (Pré-requisitos e contratos
> transversais) da mudança OpenSpec `introduce-senior-portal`. Trava, antes de
> qualquer código de §2 em diante, as decisões que teriam custo alto de reverter
> depois: rotas, contexto compartilhado entre aplicações e onde `/admin` mora.
> Consulte `openspec/changes/introduce-senior-portal/{proposal,design}.md` para a
> motivação e as alternativas descartadas — este documento só registra o contrato
> resultante, em versões (`v1`), para os front-ends consumirem.
>
> _Status: §1–§8 implementados e em produção; §9 (migração, contingência e
> aceite) em andamento — ver `openspec/changes/introduce-senior-portal/tasks.md`._
>
> **Não confundir com o portal futuro de residentes/famílias**: o "Senior
> Portal" descrito neste documento é uma aplicação **interna**, para a equipe
> já autenticada (assistência, estoque, administração) — não expõe dado
> clínico ou financeiro, só o catálogo de módulos e navegação. Um portal
> voltado a residentes e famílias é um produto futuro, distinto e ainda sem
> especificação própria — ver `docs/escopo-do-projeto.md` seção 12.3.

## 1. Contrato de rotas

### Alvo (design.md, decisão 2)

Todas as aplicações publicadas sob a mesma origem, por caminho:

```text
https://<domínio>/
├── /                 Senior Portal (entrada, catálogo, perfil, /admin)
├── /care             Assistência
├── /stock            Estoque
└── /api              API compartilhada
```

### Estado atual (não é meta — é o que existe hoje)

A produção real **não** roteia por caminho — usa **subdomínio** via Caddy
(`infra/deploy/ops/Caddyfile`): `care.$OPS_DOMAIN` → `seniorcare-care-web:80`,
`stock.$OPS_DOMAIN` → `seniorcare-stock-web:80`, `api.$OPS_DOMAIN` →
`seniorcare-api:8080`. O `docker-test` não tem nenhum ingress — cada container
expõe sua porta direto no host (3000/3001/8080). Nenhum roteamento por caminho
existe hoje em nenhum ambiente. Migrar o Caddyfile de roteamento por subdomínio
para roteamento por caminho é trabalho de implantação (§8) — este documento só
define o destino para que §4/§6/§7 já construam contra o contrato final, em vez
de contra o estado atual.

**Decisão explícita**: manter Caddy como a borda (já em uso, evita introduzir
nginx/outra ferramenta só para isso) reconfigurado para roteamento por caminho
via `handle_path` em vez de `subdomains` — `design.md` usa "borda nginx" de forma
genérica para "o proxy reverso da borda", não como exigência de software
específico.

### Parametrização de nome público e domínio

- **Domínio**: reaproveitar a variável `OPS_DOMAIN`, já usada pelo Caddyfile —
  nenhuma variável nova necessária.
- **Nome público de exibição** (ex.: "ILPI Jardim das Flores" no cabeçalho do
  portal): variável nova, **injetada em runtime, nunca no bundle** — consistente
  com o objetivo já declarado em `design.md` ("manter configuração de ambiente
  fora dos bundles imutáveis") e com o padrão já usado por
  `VITE_API_PROXY_TARGET` (dev-only, nunca embutido no build de produção). O
  mecanismo concreto (arquivo `public-config.json` servido ao lado do bundle e
  lido em runtime pelo portal, gerado por um entrypoint do container a partir de
  variável de ambiente) é decisão de implementação de `4.1` — aqui só travo o
  contrato: chave `publicName`, lida pelo portal antes de renderizar o
  cabeçalho, com fallback `"SeniorCare"` se ausente.

## 2. Contrato de contexto global

Reusa **tal como existe hoje**, sem endpoint novo:

- `GET /api/v1/auth/me` (`AuthController.cs:68`) → `CurrentIdentityDTO`
  (`UserId`, `InstitutionId`, `InstitutionName`, `DisplayName`, `Email`,
  `Roles`, `OrganizationalResponsibilities`, `EffectivePermissions`).

**Sobre status de MFA**: não existe (e não é necessário) um campo de "MFA
pendente" em `/me`. Uma sessão só é emitida depois do MFA satisfeito quando
exigido — `/me` responder implica MFA já resolvido. Se o portal receber 401 de
`/me`, o caminho correto é sempre reautenticar do zero (login → MFA se
aplicável), nunca tentar distinguir "sessão inválida" de "MFA pendente" nesse
endpoint.

## 3. Contrato de retorno seguro (`returnTo`)

- **Parâmetro**: `returnTo`, string.
- **Validação**: deve ser um caminho relativo (não pode começar com `//`, não
  pode conter esquema `http(s)://` nem `\`) E deve começar com um dos prefixos
  conhecidos: `/`, `/care`, `/stock`. Qualquer outro valor é descartado.
- **Fallback**: `/` (catálogo do portal).
- Reavaliado depois do login — um `returnTo` que aponta para um módulo sem
  permissão efetiva não é "corrigido" pelo front-end; o módulo/API nega
  normalmente e o usuário vê o catálogo sem esse item.
- O validador central (função única, reusada por portal/care/stock) é
  implementação de `4.5` — aqui só o contrato do parâmetro e das regras.

## 4. Contrato de perfil, segurança da conta e logout

- `/profile` e `/security` são rotas do **próprio Senior Portal** — não existem
  hoje em nenhum front-end. Assistência e estoque não duplicam essas telas;
  linkam de volta à navegação global do portal.
- **Logout**: qualquer aplicação chama `POST /api/v1/auth/logout` (já existe,
  compartilhado, `AuthController.cs:262`) e depois navega para `/`. Nenhuma
  regra de sessão é duplicada no cliente — o endpoint já revoga a sessão
  inteira.

## 5. Contrato de preferências (acessibilidade)

Os dois front-ends já persistem tema/contraste e tamanho de fonte em
`localStorage` sob as chaves `theme` e `fontSize`
(`SeniorCareManager-Frontend/.../contexts/ThemeContext.tsx`,
`SeniorStockManager-Frontend` tem o equivalente). Como `localStorage` é
particionado por **origem**, não por caminho, essas chaves passam a ser
automaticamente compartilhadas entre portal/`/care`/`/stock` assim que as três
aplicações estiverem sob a mesma origem — **sem mecanismo novo de
sincronização**.

**Contrato**: o Senior Portal e qualquer adaptação futura em care/stock DEVEM
reusar exatamente as chaves `theme` (valores `"light"`/`"high-contrast"`) e
`fontSize` (número, mesmos limites `10`–`44` já usados em `ThemeContext.tsx`).
Não introduzir chaves novas ou prefixadas por app — isso quebraria o
compartilhamento automático que a mesma origem já dá de graça.

## 6. Decisão: hospedagem de `/admin`

`design.md` deixou como "Open Question" se `/admin` seria hospedado dentro do
próprio Senior Portal ou em um artefato separado. **Decisão desta seção**:
`/admin` fica hospedado dentro do próprio Senior Portal, como rotas do mesmo
app React — não um quarto artefato/build/deploy separado.

**Racional**: o caso de uso primário de `/admin` nesta mudança é administrar o
catálogo de módulos (habilitação, ordem, estado operacional — API de `3.3`),
que já é uma responsabilidade do próprio portal. Criar um quarto app só para
isso contradiria o racional da própria decisão 1 do `design.md` ("limitar
impacto de release", "evitar monólito de interface") — um app inteiro com um
único caso de uso aumenta a superfície de release em vez de reduzi-la. A rota e
a permissão exigida permanecem as definidas no desenho; só a hospedagem física
fica resolvida aqui.
