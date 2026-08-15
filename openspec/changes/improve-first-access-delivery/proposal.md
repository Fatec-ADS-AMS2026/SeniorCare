## Why

`stabilize-existing-platform` entregou identidade, MFA e controle de acesso
funcionais, mas dois pontos de fricção operacional ficaram deliberadamente
sem solução técnica, documentados como gap aceito (`tasks.md` do change
arquivado, tarefas 10.6 e 11.2; `design.md`, risco "Canal de ativação
indisponível em ILPI de baixo orçamento"; `infra/deploy/BOOTSTRAP.md`, seção
"Pendências conhecidas"):

1. **Nenhum token de ativação ou recuperação é entregue automaticamente.**
   A plataforma cria o token e o mostra só pra quem tem acesso ao
   log do processo (bootstrap) ou ao banco (contas administrativas
   seguintes) — a entrega até a pessoa nova depende inteiramente de um
   procedimento manual fora do sistema. Isso não escala pra uma ILPI com
   rotatividade de equipe, e o próprio requisito já promovido
   (`platform-authentication`, "Ativação e recuperação não distribuem senha
   conhecida", cenário "Solicitação de recuperação") já assume que a
   plataforma "envia instruções" — hoje ela não envia nada, só o token
   existe internamente.
2. **Cadastro de MFA não tem QR code.** `MfaEnrollPage` mostra só a chave em
   texto (`authenticatorKey`) — todo usuário precisa digitar manualmente
   num app autenticador, mais sujeito a erro de digitação do que escanear.

Esta mudança fecha as duas lacunas com implementação real, substituindo o
procedimento manual documentado por um canal técnico — sem inventar um
sistema de notificação genérico nem preparar terreno pra alertas clínicos
futuros (fora de escopo).

## What Changes

- Adicionar uma capacidade de envio de e-mail transacional (SMTP,
  configurável por variável de ambiente, opcional — implantação sem SMTP
  configurado continua no fluxo manual já documentado, sem regressão).
- Disparar e-mail automaticamente nos três pontos que hoje só geram token
  internamente: ativação de conta nova (bootstrap e criação administrativa
  via `AdminUserOverview`) e recuperação de senha (`POST /auth/recover`).
- Adicionar renderização de QR code (biblioteca só no front-end, sem
  dependência nova no backend — o `otpauth://` já é gerado hoje) na tela de
  cadastro de MFA, mantendo a chave manual como alternativa.
- Atualizar a resposta de criação de usuário administrativo pra indicar se o
  e-mail foi enviado com sucesso (sem nunca incluir o token em si — a spec
  já proíbe isso).
- Auditar o envio (sucesso/falha) sem registrar o conteúdo da mensagem nem o
  token.

## Capabilities

### New Capabilities

- `notification-delivery`: envio de e-mail transacional para eventos de
  identidade (ativação, recuperação), configurável, com falha segura
  (não bloqueia o fluxo que originou o envio) e sem dado sensível em log.

### Modified Capabilities

- `platform-authentication`: os requisitos "Ativação e recuperação não
  distribuem senha conhecida" e "Autenticação multifator protege contas
  privilegiadas" passam a refletir a entrega real (e-mail automático quando
  configurado; QR code no cadastro de MFA) em vez de só descrever o token
  existindo internamente.

## Impact

- **Domínios afetados:** identidade e acesso (ativação, recuperação, MFA);
  operação (configuração de SMTP por ambiente de implantação).
- **Atores afetados:** administradores institucionais (deixam de precisar
  entregar token manualmente), trabalhadores novos/recuperando senha
  (recebem instrução por e-mail em vez de canal informal), equipe de
  operação (nova variável de ambiente opcional a configurar por implantação).
- **Código:** API ASP.NET Core (novo serviço de envio + wiring nos
  controllers de Auth/AdminUser) e os três front-ends com cadastro de MFA
  (Senior Portal, care e stock), todos com QR code equivalente ao `otpAuthUri`.
- **Configuração:** novas variáveis de ambiente opcionais (`Smtp__*`) — ver
  `design.md`; ausência delas preserva o comportamento atual (token só
  interno), não é um requisito obrigatório de deploy.
- **Risco de regressão:** nenhum fluxo existente muda de comportamento
  quando SMTP não está configurado — o e-mail é estritamente aditivo sobre
  o procedimento manual já documentado, que continua funcionando como
  contorno em qualquer implantação sem SMTP disponível.
