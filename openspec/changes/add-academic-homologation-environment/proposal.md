## Why

O SeniorCare precisa de um ambiente institucional de homologação e ensino no servidor
da faculdade, operável por alunos sem domínio público e sem acesso a dados reais. A
infraestrutura atual já possui entrega por imagens, Docker Compose e componentes de
operação, mas ainda não estabelece um contrato reproduzível para TLS interno, dados
sintéticos, e-mail capturado, promoção do mesmo release e convivência segura com os
serviços existentes na VM universitária.

## What Changes

- Criar uma configuração versionada de homologação acadêmica que execute API,
  PostgreSQL, portal e módulos a partir de imagens imutáveis do mesmo release
  promovível para produção, sem compilar no servidor.
- Publicar a plataforma sob uma única origem HTTPS em hostname local configurável,
  com portal em `/`, API em `/api`, assistência em `/care` e estoque em `/stock`,
  preservando os contratos da mudança `introduce-senior-portal`.
- Manter API, front-ends e banco fora da exposição direta à LAN; somente a borda
  HTTPS será o ponto público da aplicação, em porta configurável que não conflite
  com o Apache existente.
- Incluir Mailpit exclusivamente na homologação para capturar ativação e
  recuperação de conta, com interface administrativa restrita ao loopback ou a
  canal administrativo equivalente.
- Persistir banco, chaves de proteção de dados da aplicação e artefatos de backup
  em volumes separados e identificáveis do ambiente acadêmico.
- Fornecer carga e reset seguros de dados explicitamente sintéticos, com barreiras
  que impeçam a execução do reset contra outro ambiente ou banco.
- Preservar `deploy.sh`, manifestos pinados por digest, backup pré-deploy,
  pré-validação de migração, healthchecks e rollback como fonte da verdade da
  publicação; Portainer permanecerá uma interface operacional independente.
- Tornar confiável a obtenção dos manifestos e scripts de migração publicados no
  GitHub Release, falhando antes do deploy quando os artefatos estiverem ausentes
  ou inconsistentes.
- Estender os gates automatizados para validar o Compose de homologação, a
  natureza sintética da carga acadêmica, o roteamento HTTPS, as exposições de
  rede e a promoção do mesmo conjunto de digests.
- Documentar instalação, hostname/CA interna, primeiro acesso, Mailpit, deploy,
  rollback, backup, restauração, reset e limites do ambiente didático.

### Objetivos

- permitir que estudantes validem releases completos em condições próximas à
  produção sem receber dados pessoais ou de saúde reais;
- tornar homologação e futura produção equivalentes no artefato executável,
  diferenciando-as somente por configuração, dados e integrações externas;
- manter uma operação simples e recuperável na VM atual da faculdade;
- gerar evidências reais para as tarefas de homologação pendentes do Senior
  Portal.

### Não objetivos

- operar dados reais, prontuários ou atendimento assistencial nesse ambiente;
- declarar o SeniorCare pronto para produção em ILPI ou para operação sem papel;
- substituir Docker Compose e `deploy.sh` por Portainer, Coolify ou outro PaaS;
- disponibilizar o ambiente na Internet ou adquirir domínio público;
- implantar alta disponibilidade, cluster, replicação do PostgreSQL ou
  recuperação de desastre de produção;
- implementar novos fluxos assistenciais ou alterar regras de cuidado, plano
  individual, estoque ou prontuário.

## Capabilities

### New Capabilities

- `academic-homologation-environment`: implantação didática reproduzível com
  releases imutáveis, HTTPS interno e mesma origem, isolamento de rede, Mailpit,
  persistência, dados sintéticos, operação por Compose e recuperação segura.

### Modified Capabilities

- `automated-quality-gates`: amplia os gates para cobrir a configuração de
  homologação, a carga acadêmica sintética, o contrato de exposição de rede e a
  identidade dos artefatos promovidos.

## Impact

- **Domínios afetados:** infraestrutura, identidade e acesso, configuração em
  runtime, entrega de e-mail de conta, governança de dados de ensino e operação.
  Nenhum domínio assistencial ou regra do plano individual é alterado.
- **Atores afetados:** estudantes e docentes que desenvolvem e homologam;
  administradores técnicos da universidade; gestores e trabalhadores da ILPI
  apenas futuramente, quando o mesmo release for promovido a um ambiente próprio
  e autorizado. Residentes, familiares e doadores não usarão o ambiente acadêmico.
- **Código e configuração:** workflows de release, Dockerfiles dos front-ends,
  Docker Compose e overlays, `deploy.sh`, configuração de Data Protection e
  forwarded headers da API, scripts de release/seed/reset/smoke test e runbooks.
- **Sistemas externos:** GitHub Actions, GHCR, GitHub Releases, Docker Engine,
  PostgreSQL, Caddy, Mailpit e o Portainer já instalado na VM.
- **Risco assistencial:** o ambiente não pode ser confundido com serviço apto ao
  cuidado real; identificação visual e documentação devem declarar seu caráter
  didático e impedir uso operacional com residentes.
- **Impacto regulatório:** reforça separação entre ensino e produção, minimização
  e privacidade por padrão conforme a governança da parceria universitária e a
  LGPD; não autoriza tratamento de dados reais nem reutilização para pesquisa.
- **Riscos operacionais:** CPU limitada da VM, confiança da CA interna nos
  clientes, colisão de portas com Apache/serviços existentes, perda de sessão por
  chaves efêmeras, exposição acidental do Mailpit e divergência entre manifests e
  imagens; o design deverá prever controles e validação para esses riscos.
