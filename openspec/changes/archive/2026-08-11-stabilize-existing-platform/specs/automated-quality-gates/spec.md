## Purpose

Cria proteção automatizada contra regressões na API e nos dois front-ends,
transformando build, contratos críticos, segurança e migrações em critérios
obrigatórios de entrega.

## ADDED Requirements

### Requirement: Backend possui testes automatizados representativos
A API SHALL ter testes unitários e de integração para autenticação, autorização,
validação, contratos CRUD, produto, concorrência, auditoria e respostas de erro.

#### Scenario: Testes com PostgreSQL compatível
- **WHEN** a suíte de integração é executada no CI
- **THEN** usa uma instância isolada e compatível com produção, aplica migrações desde banco vazio e descarta os dados ao final

#### Scenario: Regressão de autorização
- **WHEN** uma alteração permite que operador modifique cadastro administrativo
- **THEN** um teste falha e bloqueia a integração

### Requirement: Front-ends possuem testes de comportamento
Cada front-end SHALL testar componentes compartilhados e fluxos críticos, incluindo
login, proteção de rotas, tratamento de erro, CRUD representativo e acessibilidade.

#### Scenario: Falha de API no CRUD
- **WHEN** a API retorna validação, não encontrado, conflito ou indisponibilidade
- **THEN** o teste confirma que a interface apresenta mensagem apropriada e preserva o estado necessário do usuário

#### Scenario: Sessão expirada
- **WHEN** uma requisição protegida retorna HTTP 401
- **THEN** o teste confirma que a sessão local é encerrada e a autenticação é solicitada sem loop de navegação

### Requirement: CI bloqueia mudanças não verificadas
Pull requests e alterações na branch protegida SHALL executar os gates aplicáveis
de build, lint, testes, migração, análise de dependências, segredos e análise
estática antes de serem consideradas aptas à entrega.

#### Scenario: Módulo alterado
- **WHEN** uma mudança afeta backend ou um dos front-ends
- **THEN** o CI executa build e testes desse módulo e o check agregado falha se qualquer gate falhar

#### Scenario: Apenas documentação alterada
- **WHEN** uma mudança não afeta módulos executáveis
- **THEN** os jobs caros podem ser ignorados, mas o check agregado e as verificações de higiene permanecem conclusivos

### Requirement: Cobertura é publicada e caminhos críticos não ficam sem teste
O CI SHALL publicar métricas de cobertura por componente e SHALL exigir testes
explícitos para todos os cenários críticos listados nesta mudança, sem aceitar
percentual global como substituto desses cenários.

#### Scenario: Novo caminho crítico sem teste
- **WHEN** uma mudança altera autenticação, autorização, migração ou contrato de cadastro sem teste correspondente
- **THEN** a revisão ou o gate de qualidade identifica a lacuna antes da entrega

### Requirement: Dados de teste não contêm dados pessoais reais
Fixtures, seeds e evidências de teste SHALL usar dados sintéticos e SHALL NOT
conter dados de residentes, familiares, trabalhadores ou doadores reais.

#### Scenario: Verificação de fixture
- **WHEN** dados de teste são adicionados ao repositório
- **THEN** a revisão e as verificações automatizadas confirmam sua natureza sintética e ausência de segredos

### Requirement: Migrações são verificadas em instalação e atualização
Cada migração SHALL ser testada tanto em banco vazio quanto sobre a versão
imediatamente anterior suportada, com falha segura e instrução de recuperação.

#### Scenario: Instalação limpa
- **WHEN** a versão é instalada em banco vazio
- **THEN** todas as migrações são aplicadas e a API alcança prontidão

#### Scenario: Atualização de versão
- **WHEN** um banco da versão anterior é atualizado
- **THEN** os dados de catálogo permanecem íntegros e as novas restrições são satisfeitas ou a atualização falha antes de publicar a aplicação
