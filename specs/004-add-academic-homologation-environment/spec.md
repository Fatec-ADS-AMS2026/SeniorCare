# Feature Specification: Adicionar ambiente acadêmico de homologação

**Feature ID**: `004-add-academic-homologation-environment`
**Created**: 2026-09-26 (data da migração)
**Status**: Em andamento
**Input**: artefatos legados incorporados nesta feature durante a migração

> Este documento preserva requisitos e cenários verificáveis existentes. A migração
> não inventa prioridades P1/P2/P3 retroativas; os cenários associados a cada
> requisito continuam sendo os testes de aceitação autoritativos.

## User Scenarios & Testing

Os fluxos verificáveis estão expressos como cenários `WHEN`/`THEN` sob cada
requisito. Para trabalho novo, o fluxo Spec Kit deve acrescentar histórias
priorizadas e testes independentes sem enfraquecer estes contratos existentes.

## Contexto e escopo

### Why

O SeniorCare precisa de um ambiente institucional de homologação e ensino no servidor
da faculdade, operável por alunos sem domínio público e sem acesso a dados reais. A
infraestrutura atual já possui entrega por imagens, Docker Compose e componentes de
operação, mas ainda não estabelece um contrato reproduzível para TLS interno, dados
sintéticos, e-mail capturado, promoção do mesmo release e convivência segura com os
serviços existentes na VM universitária.

### What Changes

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

#### Objetivos

- permitir que estudantes validem releases completos em condições próximas à
  produção sem receber dados pessoais ou de saúde reais;
- tornar homologação e futura produção equivalentes no artefato executável,
  diferenciando-as somente por configuração, dados e integrações externas;
- manter uma operação simples e recuperável na VM atual da faculdade;
- gerar evidências reais para as tarefas de homologação pendentes do Senior
  Portal.

#### Não objetivos

- operar dados reais, prontuários ou atendimento assistencial nesse ambiente;
- declarar o SeniorCare pronto para produção em ILPI ou para operação sem papel;
- substituir Docker Compose e `deploy.sh` por Portainer, Coolify ou outro PaaS;
- disponibilizar o ambiente na Internet ou adquirir domínio público;
- implantar alta disponibilidade, cluster, replicação do PostgreSQL ou
  recuperação de desastre de produção;
- implementar novos fluxos assistenciais ou alterar regras de cuidado, plano
  individual, estoque ou prontuário.

### Capabilities

#### New Capabilities

- `academic-homologation-environment`: implantação didática reproduzível com
  releases imutáveis, HTTPS interno e mesma origem, isolamento de rede, Mailpit,
  persistência, dados sintéticos, operação por Compose e recuperação segura.

#### Modified Capabilities

- `automated-quality-gates`: amplia os gates para cobrir a configuração de
  homologação, a carga acadêmica sintética, o contrato de exposição de rede e a
  identidade dos artefatos promovidos.

### Impact

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

## Requirements

### Capacidade `academic-homologation-environment`

_Contrato de capacidade migrado sem alteração normativa._

### Purpose

Define um ambiente institucional de homologação e ensino reproduzível, isolado
de produção e seguro para estudantes validarem releases completos do SeniorCare
exclusivamente com dados sintéticos.

### Functional Requirements

#### Requirement: Homologação executa releases imutáveis promovíveis
O ambiente acadêmico SHALL executar imagens previamente construídas e identificadas
por digest em um manifesto de release, SHALL NOT compilar a aplicação no servidor e
SHALL permitir que o mesmo conjunto de digests seja promovido para produção sem
rebuild.

##### Scenario: Publicação de release na homologação
- **WHEN** um administrador publica uma versão válida no ambiente acadêmico
- **THEN** todos os componentes da aplicação são obtidos pelos digests declarados no manifesto e nenhum build ocorre no servidor

##### Scenario: Promoção posterior para produção
- **WHEN** uma versão homologada é selecionada para produção
- **THEN** o conjunto de digests da aplicação permanece idêntico e somente configurações, segredos, dados e integrações do ambiente mudam

##### Scenario: Manifesto ou artefato inconsistente
- **WHEN** um manifesto, imagem ou script de migração do release está ausente ou não corresponde à versão solicitada
- **THEN** a publicação falha antes de alterar os serviços em execução

#### Requirement: Plataforma usa uma origem HTTPS interna
O ambiente acadêmico SHALL disponibilizar portal, API e módulos sob uma única
origem HTTPS configurável, sem exigir domínio público, com `/` para o portal,
`/api` para a API, `/care` para assistência e `/stock` para estoque.

##### Scenario: Acesso por hostname da rede acadêmica
- **WHEN** um computador que resolve o hostname interno e confia na autoridade certificadora do ambiente abre a URL de homologação
- **THEN** o navegador estabelece HTTPS sem trocar de origem ao navegar pelo portal, API, assistência e estoque

##### Scenario: Navegação autenticada entre módulos
- **WHEN** um usuário autenticado navega do portal para um módulo autorizado
- **THEN** a sessão segura continua válida e os assets do módulo são carregados sob seu caminho-base

##### Scenario: Cliente não confia na autoridade interna
- **WHEN** um computador ainda não confia na autoridade certificadora usada pelo ambiente
- **THEN** a documentação operacional identifica o pré-requisito e fornece procedimento verificável de instalação e validação da confiança

#### Requirement: Serviços internos não são expostos diretamente à LAN
A API, os front-ends, o banco de dados e os serviços administrativos SHALL ser
alcançáveis entre si por rede privada da aplicação e SHALL NOT publicar suas
portas de serviço na LAN; somente a borda HTTPS SHALL ser o ponto de entrada da
aplicação. Acesso administrativo excepcional SHALL ser limitado ao loopback ou a
canal administrativo autenticado equivalente.

##### Scenario: Varredura das portas da VM pela LAN
- **WHEN** um cliente da rede acadêmica verifica as portas da aplicação
- **THEN** a borda HTTPS configurada está acessível e as portas diretas da API, banco, front-ends, Mailpit e Portainer não estão acessíveis

##### Scenario: Comunicação interna da aplicação
- **WHEN** a borda ou um componente autorizado encaminha uma requisição internamente
- **THEN** o destino é resolvido pela rede privada sem depender de porta publicada na LAN

#### Requirement: Ensino e demonstração usam somente dados sintéticos
O ambiente acadêmico MUST utilizar base própria e separada de qualquer produção,
SHALL aceitar carga versionada somente com dados sintéticos e SHALL identificar de
forma visível seu caráter didático e a proibição de dados reais.

##### Scenario: Preparação inicial para uma turma
- **WHEN** o ambiente acadêmico é inicializado ou restaurado para atividades de ensino
- **THEN** recebe exclusivamente a carga sintética aprovada e apresenta a identificação de ambiente didático

##### Scenario: Tentativa de configurar origem de dados de outro ambiente
- **WHEN** a configuração acadêmica referencia banco, volume ou credencial reservada a outro ambiente
- **THEN** a validação falha antes de iniciar a aplicação ou executar carga de dados

##### Scenario: Dado pessoal real identificado
- **WHEN** revisão ou verificação detecta dado de residente, familiar, trabalhador ou doador que não é comprovadamente sintético
- **THEN** a carga ou publicação é bloqueada e o dado não é disponibilizado aos estudantes

#### Requirement: Reset acadêmico possui barreiras contra destruição indevida
A operação de reset SHALL validar de forma redundante a identidade acadêmica do
ambiente e o alvo exato, SHALL exigir confirmação explícita para exclusão e SHALL
recusar qualquer alvo não reconhecido como acadêmico.

##### Scenario: Reset autorizado da homologação
- **WHEN** um administrador confirma o reset de um banco e volume identificados inequivocamente como acadêmicos
- **THEN** somente esse alvo é reinicializado e a carga sintética aprovada é reaplicada

##### Scenario: Nome ou ambiente divergente
- **WHEN** o comando recebe nome de cliente, banco, projeto ou volume diferente dos valores acadêmicos permitidos
- **THEN** ele termina sem excluir ou sobrescrever dados

#### Requirement: E-mails acadêmicos são capturados e não entregues externamente
O ambiente acadêmico SHALL encaminhar mensagens de ativação e recuperação a um
capturador de e-mail local, SHALL NOT usar SMTP de produção e SHALL restringir a
interface de leitura por conter links e tokens temporários.

##### Scenario: Ativação de conta de estudante
- **WHEN** a aplicação envia a mensagem de ativação no ambiente acadêmico
- **THEN** a mensagem fica disponível no capturador local, não é entregue na Internet e seu link aponta para a origem HTTPS acadêmica

##### Scenario: Acesso comum pela LAN ao capturador
- **WHEN** um usuário sem canal administrativo tenta abrir a interface do capturador diretamente pela LAN
- **THEN** a conexão não é disponibilizada

#### Requirement: Estado essencial sobrevive à recriação dos containers
Dados do PostgreSQL e chaves criptográficas necessárias para proteger a sessão
SHALL residir em armazenamento persistente específico do ambiente, de modo que uma
atualização normal dos containers não apague a base nem invalide sessões apenas por
troca da instância da API.

##### Scenario: Recriação da API durante atualização
- **WHEN** o container da API é substituído sem alteração incompatível de autenticação
- **THEN** as chaves persistidas são reutilizadas e uma sessão ainda válida continua verificável

##### Scenario: Recriação dos serviços sem reset
- **WHEN** a stack é recriada em uma publicação normal
- **THEN** a base acadêmica e os dados persistidos permanecem disponíveis

#### Requirement: Publicação falha com segurança e admite recuperação
Toda publicação SHALL preservar backup recuperável do banco existente, validar a
migração antes da troca, aguardar prontidão dos serviços e registrar a versão
implantada. Falha de qualquer gate SHALL impedir a declaração de sucesso e SHALL
manter instrução operacional de rollback e restauração.

##### Scenario: Migração incompatível com a base atual
- **WHEN** a pré-validação da migração falha
- **THEN** a versão em execução não é substituída e a tentativa é reportada sem persistir a migração de validação

##### Scenario: Novo release não alcança prontidão
- **WHEN** um ou mais componentes não ficam prontos dentro do limite configurado
- **THEN** o deploy é marcado como falho e o operador consegue retornar ao release anterior registrado

##### Scenario: Primeiro deploy sem base anterior
- **WHEN** a versão é publicada em um ambiente acadêmico vazio
- **THEN** a ausência de backup anterior é tratada explicitamente e a instalação só é concluída após migrações e prontidão

#### Requirement: Portainer não substitui a fonte versionada do deploy
O ambiente SHALL permanecer reproduzível a partir de configuração versionada e
manifestos de release. A interface de administração de containers SHALL ser tratada
como recurso de observação e resposta emergencial e SHALL NOT ser necessária para
criar ou atualizar a stack.

##### Scenario: Deploy sem interface administrativa
- **WHEN** a interface de administração está indisponível
- **THEN** um administrador ainda consegue publicar, verificar e reverter a aplicação pelo fluxo versionado

##### Scenario: Reconstrução do ambiente
- **WHEN** uma nova VM compatível precisa substituir a atual
- **THEN** a stack pode ser reconstruída a partir do repositório, dos artefatos de release, dos segredos externos e dos backups documentados

#### Requirement: Ambiente documenta limites e procedimentos operacionais
O projeto SHALL fornecer um runbook acadêmico que enumere pré-requisitos,
configuração sem segredos, resolução do hostname, confiança no certificado,
primeiro acesso, publicação, Mailpit, backup, restauração, rollback, reset e
diagnóstico de recursos.

##### Scenario: Novo administrador prepara a VM
- **WHEN** um responsável técnico sem conhecimento implícito do ambiente segue o runbook
- **THEN** ele consegue validar os pré-requisitos e identificar todos os valores locais que precisam ser fornecidos fora do repositório

##### Scenario: Capacidade da VM é insuficiente
- **WHEN** saúde ou uso de recursos indica que a VM atual não sustenta a carga didática
- **THEN** o runbook orienta reduzir concorrência ou solicitar recursos sem remover controles de segurança, persistência ou recuperação
### Capacidade `automated-quality-gates`

_Contrato de capacidade migrado sem alteração normativa._

### Functional Requirements

#### Requirement: CI valida a topologia de homologação acadêmica
O CI SHALL materializar e validar a configuração efetiva do ambiente acadêmico,
incluindo sintaxe, variáveis obrigatórias, healthchecks, persistência, roteamento e
exposição de portas, antes que uma alteração de infraestrutura seja considerada
apta à entrega.

##### Scenario: Serviço interno é publicado na LAN
- **WHEN** uma alteração passa a publicar diretamente API, banco, front-end ou serviço administrativo no ambiente acadêmico
- **THEN** o gate de infraestrutura falha e identifica a exposição incompatível com o contrato

##### Scenario: Configuração acadêmica válida
- **WHEN** o CI combina a configuração-base, o overlay acadêmico e valores sintéticos de validação
- **THEN** a configuração resultante é válida e contém todos os serviços, redes, volumes e checks obrigatórios

#### Requirement: CI comprova identidade dos artefatos promovíveis
O pipeline de release SHALL registrar por digest todos os componentes da aplicação
e SHALL verificar que os manifests destinados a homologação e produção podem
referenciar os mesmos artefatos sem rebuild.

##### Scenario: Release completo
- **WHEN** uma tag de release produz API, portal, assistência e estoque
- **THEN** um manifesto verificável associa a mesma versão e commit aos quatro digests e é publicado com o script de migração correspondente

##### Scenario: Componente ausente ou reconstruído
- **WHEN** o release omite um componente ou a promoção referencia digest diferente do homologado
- **THEN** o gate falha antes da publicação ou promoção

#### Requirement: CI protege a carga sintética e as barreiras de reset
Os gates SHALL incluir os arquivos de carga acadêmica na verificação de dados
sintéticos e SHALL testar que a operação de reset recusa alvos não acadêmicos sem
executar exclusão.

##### Scenario: Identificador pessoal não aprovado na carga acadêmica
- **WHEN** uma fixture ou seed acadêmica contém dado que não satisfaz as convenções sintéticas versionadas
- **THEN** o gate falha e impede sua integração

##### Scenario: Teste negativo do reset
- **WHEN** a suíte invoca o reset com cliente, banco, projeto ou volume não acadêmico
- **THEN** o comando retorna falha e nenhuma operação destrutiva é chamada

#### Requirement: Smoke test valida o acesso acadêmico fim a fim
O projeto SHALL possuir verificação automatizada que valide a origem HTTPS, a
prontidão, os caminhos públicos, a autenticação segura e a captura local de e-mail
em uma implantação representativa.

##### Scenario: Implantação acadêmica saudável
- **WHEN** a stack de teste equivalente à homologação alcança prontidão
- **THEN** o smoke test confirma portal, API, assistência, estoque, cookie seguro e captura de uma mensagem com URL acadêmica

##### Scenario: Caminho-base ou TLS regrede
- **WHEN** um bundle usa caminho-base incompatível ou a origem deixa de oferecer HTTPS corretamente
- **THEN** o smoke test falha antes que o release seja considerado homologável

## Success Criteria

A feature é aceita quando todos os cenários normativos acima são satisfeitos, as
tarefas marcadas como concluídas possuem implementação e evidência correspondentes
e os gates definidos na constituição do projeto passam. Tarefas ainda abertas em
`tasks.md` permanecem bloqueantes para a conclusão operacional da feature.

## Assumptions

- O escopo canônico e as decisões em aberto permanecem em
  [`docs/escopo-do-projeto.md`](../../docs/escopo-do-projeto.md).
- A API continua sendo a autoridade final de autorização.
- Nenhuma migração de formato altera o estado real das tarefas ou declara
  funcionalidades adicionais como entregues.
