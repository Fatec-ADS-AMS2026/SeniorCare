## ADDED Requirements

### Requirement: CI valida a topologia de homologação acadêmica
O CI SHALL materializar e validar a configuração efetiva do ambiente acadêmico,
incluindo sintaxe, variáveis obrigatórias, healthchecks, persistência, roteamento e
exposição de portas, antes que uma alteração de infraestrutura seja considerada
apta à entrega.

#### Scenario: Serviço interno é publicado na LAN
- **WHEN** uma alteração passa a publicar diretamente API, banco, front-end ou serviço administrativo no ambiente acadêmico
- **THEN** o gate de infraestrutura falha e identifica a exposição incompatível com o contrato

#### Scenario: Configuração acadêmica válida
- **WHEN** o CI combina a configuração-base, o overlay acadêmico e valores sintéticos de validação
- **THEN** a configuração resultante é válida e contém todos os serviços, redes, volumes e checks obrigatórios

### Requirement: CI comprova identidade dos artefatos promovíveis
O pipeline de release SHALL registrar por digest todos os componentes da aplicação
e SHALL verificar que os manifests destinados a homologação e produção podem
referenciar os mesmos artefatos sem rebuild.

#### Scenario: Release completo
- **WHEN** uma tag de release produz API, portal, assistência e estoque
- **THEN** um manifesto verificável associa a mesma versão e commit aos quatro digests e é publicado com o script de migração correspondente

#### Scenario: Componente ausente ou reconstruído
- **WHEN** o release omite um componente ou a promoção referencia digest diferente do homologado
- **THEN** o gate falha antes da publicação ou promoção

### Requirement: CI protege a carga sintética e as barreiras de reset
Os gates SHALL incluir os arquivos de carga acadêmica na verificação de dados
sintéticos e SHALL testar que a operação de reset recusa alvos não acadêmicos sem
executar exclusão.

#### Scenario: Identificador pessoal não aprovado na carga acadêmica
- **WHEN** uma fixture ou seed acadêmica contém dado que não satisfaz as convenções sintéticas versionadas
- **THEN** o gate falha e impede sua integração

#### Scenario: Teste negativo do reset
- **WHEN** a suíte invoca o reset com cliente, banco, projeto ou volume não acadêmico
- **THEN** o comando retorna falha e nenhuma operação destrutiva é chamada

### Requirement: Smoke test valida o acesso acadêmico fim a fim
O projeto SHALL possuir verificação automatizada que valide a origem HTTPS, a
prontidão, os caminhos públicos, a autenticação segura e a captura local de e-mail
em uma implantação representativa.

#### Scenario: Implantação acadêmica saudável
- **WHEN** a stack de teste equivalente à homologação alcança prontidão
- **THEN** o smoke test confirma portal, API, assistência, estoque, cookie seguro e captura de uma mensagem com URL acadêmica

#### Scenario: Caminho-base ou TLS regrede
- **WHEN** um bundle usa caminho-base incompatível ou a origem deixa de oferecer HTTPS corretamente
- **THEN** o smoke test falha antes que o release seja considerado homologável
