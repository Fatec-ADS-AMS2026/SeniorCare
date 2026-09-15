## Purpose

Define um ambiente institucional de homologação e ensino reproduzível, isolado
de produção e seguro para estudantes validarem releases completos do SeniorCare
exclusivamente com dados sintéticos.

## ADDED Requirements

### Requirement: Homologação executa releases imutáveis promovíveis
O ambiente acadêmico SHALL executar imagens previamente construídas e identificadas
por digest em um manifesto de release, SHALL NOT compilar a aplicação no servidor e
SHALL permitir que o mesmo conjunto de digests seja promovido para produção sem
rebuild.

#### Scenario: Publicação de release na homologação
- **WHEN** um administrador publica uma versão válida no ambiente acadêmico
- **THEN** todos os componentes da aplicação são obtidos pelos digests declarados no manifesto e nenhum build ocorre no servidor

#### Scenario: Promoção posterior para produção
- **WHEN** uma versão homologada é selecionada para produção
- **THEN** o conjunto de digests da aplicação permanece idêntico e somente configurações, segredos, dados e integrações do ambiente mudam

#### Scenario: Manifesto ou artefato inconsistente
- **WHEN** um manifesto, imagem ou script de migração do release está ausente ou não corresponde à versão solicitada
- **THEN** a publicação falha antes de alterar os serviços em execução

### Requirement: Plataforma usa uma origem HTTPS interna
O ambiente acadêmico SHALL disponibilizar portal, API e módulos sob uma única
origem HTTPS configurável, sem exigir domínio público, com `/` para o portal,
`/api` para a API, `/care` para assistência e `/stock` para estoque.

#### Scenario: Acesso por hostname da rede acadêmica
- **WHEN** um computador que resolve o hostname interno e confia na autoridade certificadora do ambiente abre a URL de homologação
- **THEN** o navegador estabelece HTTPS sem trocar de origem ao navegar pelo portal, API, assistência e estoque

#### Scenario: Navegação autenticada entre módulos
- **WHEN** um usuário autenticado navega do portal para um módulo autorizado
- **THEN** a sessão segura continua válida e os assets do módulo são carregados sob seu caminho-base

#### Scenario: Cliente não confia na autoridade interna
- **WHEN** um computador ainda não confia na autoridade certificadora usada pelo ambiente
- **THEN** a documentação operacional identifica o pré-requisito e fornece procedimento verificável de instalação e validação da confiança

### Requirement: Serviços internos não são expostos diretamente à LAN
A API, os front-ends, o banco de dados e os serviços administrativos SHALL ser
alcançáveis entre si por rede privada da aplicação e SHALL NOT publicar suas
portas de serviço na LAN; somente a borda HTTPS SHALL ser o ponto de entrada da
aplicação. Acesso administrativo excepcional SHALL ser limitado ao loopback ou a
canal administrativo autenticado equivalente.

#### Scenario: Varredura das portas da VM pela LAN
- **WHEN** um cliente da rede acadêmica verifica as portas da aplicação
- **THEN** a borda HTTPS configurada está acessível e as portas diretas da API, banco, front-ends, Mailpit e Portainer não estão acessíveis

#### Scenario: Comunicação interna da aplicação
- **WHEN** a borda ou um componente autorizado encaminha uma requisição internamente
- **THEN** o destino é resolvido pela rede privada sem depender de porta publicada na LAN

### Requirement: Ensino e demonstração usam somente dados sintéticos
O ambiente acadêmico MUST utilizar base própria e separada de qualquer produção,
SHALL aceitar carga versionada somente com dados sintéticos e SHALL identificar de
forma visível seu caráter didático e a proibição de dados reais.

#### Scenario: Preparação inicial para uma turma
- **WHEN** o ambiente acadêmico é inicializado ou restaurado para atividades de ensino
- **THEN** recebe exclusivamente a carga sintética aprovada e apresenta a identificação de ambiente didático

#### Scenario: Tentativa de configurar origem de dados de outro ambiente
- **WHEN** a configuração acadêmica referencia banco, volume ou credencial reservada a outro ambiente
- **THEN** a validação falha antes de iniciar a aplicação ou executar carga de dados

#### Scenario: Dado pessoal real identificado
- **WHEN** revisão ou verificação detecta dado de residente, familiar, trabalhador ou doador que não é comprovadamente sintético
- **THEN** a carga ou publicação é bloqueada e o dado não é disponibilizado aos estudantes

### Requirement: Reset acadêmico possui barreiras contra destruição indevida
A operação de reset SHALL validar de forma redundante a identidade acadêmica do
ambiente e o alvo exato, SHALL exigir confirmação explícita para exclusão e SHALL
recusar qualquer alvo não reconhecido como acadêmico.

#### Scenario: Reset autorizado da homologação
- **WHEN** um administrador confirma o reset de um banco e volume identificados inequivocamente como acadêmicos
- **THEN** somente esse alvo é reinicializado e a carga sintética aprovada é reaplicada

#### Scenario: Nome ou ambiente divergente
- **WHEN** o comando recebe nome de cliente, banco, projeto ou volume diferente dos valores acadêmicos permitidos
- **THEN** ele termina sem excluir ou sobrescrever dados

### Requirement: E-mails acadêmicos são capturados e não entregues externamente
O ambiente acadêmico SHALL encaminhar mensagens de ativação e recuperação a um
capturador de e-mail local, SHALL NOT usar SMTP de produção e SHALL restringir a
interface de leitura por conter links e tokens temporários.

#### Scenario: Ativação de conta de estudante
- **WHEN** a aplicação envia a mensagem de ativação no ambiente acadêmico
- **THEN** a mensagem fica disponível no capturador local, não é entregue na Internet e seu link aponta para a origem HTTPS acadêmica

#### Scenario: Acesso comum pela LAN ao capturador
- **WHEN** um usuário sem canal administrativo tenta abrir a interface do capturador diretamente pela LAN
- **THEN** a conexão não é disponibilizada

### Requirement: Estado essencial sobrevive à recriação dos containers
Dados do PostgreSQL e chaves criptográficas necessárias para proteger a sessão
SHALL residir em armazenamento persistente específico do ambiente, de modo que uma
atualização normal dos containers não apague a base nem invalide sessões apenas por
troca da instância da API.

#### Scenario: Recriação da API durante atualização
- **WHEN** o container da API é substituído sem alteração incompatível de autenticação
- **THEN** as chaves persistidas são reutilizadas e uma sessão ainda válida continua verificável

#### Scenario: Recriação dos serviços sem reset
- **WHEN** a stack é recriada em uma publicação normal
- **THEN** a base acadêmica e os dados persistidos permanecem disponíveis

### Requirement: Publicação falha com segurança e admite recuperação
Toda publicação SHALL preservar backup recuperável do banco existente, validar a
migração antes da troca, aguardar prontidão dos serviços e registrar a versão
implantada. Falha de qualquer gate SHALL impedir a declaração de sucesso e SHALL
manter instrução operacional de rollback e restauração.

#### Scenario: Migração incompatível com a base atual
- **WHEN** a pré-validação da migração falha
- **THEN** a versão em execução não é substituída e a tentativa é reportada sem persistir a migração de validação

#### Scenario: Novo release não alcança prontidão
- **WHEN** um ou mais componentes não ficam prontos dentro do limite configurado
- **THEN** o deploy é marcado como falho e o operador consegue retornar ao release anterior registrado

#### Scenario: Primeiro deploy sem base anterior
- **WHEN** a versão é publicada em um ambiente acadêmico vazio
- **THEN** a ausência de backup anterior é tratada explicitamente e a instalação só é concluída após migrações e prontidão

### Requirement: Portainer não substitui a fonte versionada do deploy
O ambiente SHALL permanecer reproduzível a partir de configuração versionada e
manifestos de release. A interface de administração de containers SHALL ser tratada
como recurso de observação e resposta emergencial e SHALL NOT ser necessária para
criar ou atualizar a stack.

#### Scenario: Deploy sem interface administrativa
- **WHEN** a interface de administração está indisponível
- **THEN** um administrador ainda consegue publicar, verificar e reverter a aplicação pelo fluxo versionado

#### Scenario: Reconstrução do ambiente
- **WHEN** uma nova VM compatível precisa substituir a atual
- **THEN** a stack pode ser reconstruída a partir do repositório, dos artefatos de release, dos segredos externos e dos backups documentados

### Requirement: Ambiente documenta limites e procedimentos operacionais
O projeto SHALL fornecer um runbook acadêmico que enumere pré-requisitos,
configuração sem segredos, resolução do hostname, confiança no certificado,
primeiro acesso, publicação, Mailpit, backup, restauração, rollback, reset e
diagnóstico de recursos.

#### Scenario: Novo administrador prepara a VM
- **WHEN** um responsável técnico sem conhecimento implícito do ambiente segue o runbook
- **THEN** ele consegue validar os pré-requisitos e identificar todos os valores locais que precisam ser fornecidos fora do repositório

#### Scenario: Capacidade da VM é insuficiente
- **WHEN** saúde ou uso de recursos indica que a VM atual não sustenta a carga didática
- **THEN** o runbook orienta reduzir concorrência ou solicitar recursos sem remover controles de segurança, persistência ou recuperação
