# Configuração — SeniorCareManager.WebAPI

A API lê configuração da forma padrão do ASP.NET Core: `appsettings.json` →
`appsettings.{ASPNETCORE_ENVIRONMENT}.json` → variáveis de ambiente (a última
camada sempre vence). Segredos e valores por ambiente **nunca** ficam nos
`appsettings.*.json` versionados — só variáveis de ambiente.

## Ambientes

| `ASPNETCORE_ENVIRONMENT` | Arquivo | Uso |
|---|---|---|
| `Development` (default se não definido) | `appsettings.Development.json` | Rodar localmente fora de container. Tem uma senha de banco local (`postdba`) — só funciona contra o Postgres do `infra/docker-test/`, nunca aponta pra um ambiente real. |
| `Test` | `appsettings.Test.json` | Usado pelos testes de integração (`PostgresWebApplicationFactory`). Não tem `ConnectionStrings` — a fábrica de testes substitui o `DbContext` por código, apontando pro contêiner Postgres efêmero do Testcontainers. |
| `Production` | `appsettings.Production.json` | Deploy real (`infra/deploy/`). Deliberadamente **sem** `ConnectionStrings` nem CORS — essas variáveis têm que vir de fora (ver tabela abaixo); se faltarem, o processo falha no boot (ver `Program.cs`). |

## Variáveis obrigatórias em produção

| Variável | Formato | Onde é setada |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | string de conexão Npgsql, ex.: `Host=postgres;Port=5432;Database=db_seniorcare;Username=postgres;Password=<senha>;` | `infra/deploy/docker-compose.yml`, montada a partir de `clients/<nome>/.env` (ver `infra/deploy/clients/exemplo/.env.example`) |
| `CORS_ALLOWED_ORIGINS` | lista separada por vírgula, ex.: `https://care.exemplo.com.br,https://estoque.exemplo.com.br` | idem — sem essa variável, o CORS cai no default de desenvolvimento (`localhost:3000`/`:3001`/`:5173`), que não funciona em produção |

O nome com `__` (duplo underscore) é a convenção do ASP.NET Core para mapear
uma variável de ambiente pra uma chave aninhada (`ConnectionStrings:DefaultConnection`).

## Bootstrap da instituição e do administrador inicial

| Variável | Formato | Obrigatório quando |
|---|---|---|
| `Bootstrap__InstitutionName` | nome da ILPI, ex.: `ILPI Exemplo` | nenhuma instituição existir ainda no banco |
| `Bootstrap__AdminEmail` | e-mail da primeira conta administrativa | idem |
| `Bootstrap__AdminDisplayName` | nome de exibição da primeira conta administrativa | idem |

As três variáveis só fazem sentido juntas — informar só uma ou duas é erro de
configuração e o processo falha no boot (ver "Validação de startup" abaixo).
No primeiro boot sem nenhuma instituição, se as três estiverem presentes, o
processo cria a instituição e uma conta administrativa `PROVISIONED` **sem
senha conhecida**. Com SMTP configurado, envia o link de ativação e não expõe
o token no console; sem SMTP ou quando a entrega falha, imprime o token uma
única vez para o procedimento manual. Reinícios seguintes (instituição já existente) são
no-op: nada é recriado nem redefinido silenciosamente, mesmo que as
variáveis continuem definidas.

## E-mail transacional e links de primeiro acesso

O canal SMTP é opcional para a implantação inteira. Se qualquer variável
`Smtp__*` for informada, as chaves obrigatórias condicionais precisam estar
completas; usuário e senha são opcionais, mas devem aparecer juntos.

| Variável | Obrigatoriedade | Exemplo |
|---|---|---|
| `Smtp__Host` | com SMTP habilitado | `mail.exemplo.com.br` |
| `Smtp__Port` | com SMTP habilitado | `587` |
| `Smtp__Username` | opcional, junto de `Smtp__Password` | `seniorcare` |
| `Smtp__Password` | opcional, junto de `Smtp__Username` | segredo injetado fora do repositório |
| `Smtp__FromAddress` | com SMTP habilitado | `nao-responda@exemplo.com.br` |
| `Smtp__FromDisplayName` | opcional | `SeniorCare` |
| `Smtp__UseStartTls` | opcional, default `true` | `true` |
| `Frontend__ActivationBaseUrl` | com SMTP habilitado | `https://portal.exemplo.com.br/ativar-conta` |

`Frontend__ActivationBaseUrl` aponta para o Senior Portal canônico. O link de
recuperação usa a mesma origem e a rota `/redefinir-senha`. Credenciais SMTP,
tokens e corpos de mensagem nunca são registrados em log ou auditoria.

Para contas administrativas criadas depois do bootstrap, `emailSent: false`
não autoriza leitura do banco ou exposição do token: corrija o SMTP e use
`POST /api/v1/AdminUser/{id}/resend-activation`. A operação exige
`AdminUser:write`, invalida ativações pendentes e retorna apenas `emailSent`.

## Sessão (cookie) entre os front-ends

| Variável | Formato | Obrigatório quando |
|---|---|---|
| `SessionCookieDomain` | domínio pai com ponto à frente, ex.: `.exemplo.com.br` | os front-ends ficarem em subdomínios distintos (`care.exemplo.com.br`/`estoque.exemplo.com.br`/`portal.exemplo.com.br`) e precisarem compartilhar sessão sem novo login |

Sem essa variável, o cookie de sessão é *host-only* (só volta pro host exato
que o emitiu) — correto em desenvolvimento, onde `localhost:3000`/`:3001`/`:3002`
já compartilham cookie por serem o mesmo host (cookies não são delimitados
por porta), e também correto no alvo de roteamento por caminho (§8.2/§9.7),
onde os três front-ends passam a viver sob a mesma origem. Parâmetros de
duração de acesso/renovação e limite de tentativas
são configuráveis por instituição via API administrativa
(`AdminInstitutionSecurityController`, §6), não por variável de ambiente —
têm default seguro quando a instituição não configurou nada (ver
`InstitutionSecurityPolicyService`).

## Validação de startup

O processo verifica, antes de subir, que `ConnectionStrings:DefaultConnection`
está presente e não vazia, que as três variáveis de bootstrap acima foram
informadas todas juntas ou nenhuma e que a configuração SMTP condicional está
completa e tipada — se algo faltar, encerra com uma mensagem
de erro que identifica a(s) chave(s) ausente(s)/incompleta(s), sem nunca
ecoar nenhum valor configurado. Ver `Program.cs`.
