## Purpose

Substitui o login meramente visual por identidade e sessão verificáveis, limitando
o acesso às funções administrativas já implementadas e criando uma fundação
segura para evoluções posteriores.

## ADDED Requirements

### Requirement: Usuário autentica-se com identidade individual
A plataforma SHALL autenticar cada usuário por uma conta individual ativa e
credencial protegida, sem contas compartilhadas ou senhas fixas no código.

#### Scenario: Credenciais válidas
- **WHEN** um usuário ativo apresenta credenciais válidas
- **THEN** a plataforma inicia uma sessão, retorna somente dados mínimos da identidade e direciona o usuário para uma rota permitida

#### Scenario: Credenciais inválidas
- **WHEN** email ou senha são inválidos
- **THEN** a plataforma nega o acesso com mensagem genérica que não confirma a existência da conta

#### Scenario: Conta inativa
- **WHEN** uma conta inativa tenta autenticar-se
- **THEN** a plataforma nega a sessão e registra o evento de segurança

### Requirement: Sessão e tokens possuem proteção e expiração
Sessões SHALL expirar, SHALL poder ser revogadas e SHALL manter credenciais de
longo prazo inacessíveis a scripts do navegador. Tokens e segredos SHALL NOT ser
gravados em logs ou armazenamento persistente inseguro do front-end.

#### Scenario: Sessão expirada
- **WHEN** o usuário apresenta uma sessão expirada e não renovável
- **THEN** a API retorna HTTP 401 e o front-end solicita nova autenticação

#### Scenario: Logout
- **WHEN** o usuário encerra a sessão
- **THEN** a sessão é revogada e novas requisições protegidas são recusadas

#### Scenario: Reutilização de sessão revogada
- **WHEN** uma credencial de sessão revogada é reapresentada
- **THEN** a API retorna HTTP 401 e registra a tentativa sem registrar o token

### Requirement: APIs e rotas são protegidas por papel
A plataforma SHALL distinguir ao menos administrador e operador autorizado. Somente
administradores SHALL criar, alterar ou inativar cadastros; usuários autenticados
com permissão de leitura poderão consultá-los.

#### Scenario: Requisição anônima
- **WHEN** um cliente anônimo acessa endpoint ou rota administrativa
- **THEN** o acesso é negado com HTTP 401 na API ou redirecionamento ao login na interface

#### Scenario: Usuário sem papel necessário
- **WHEN** um operador autenticado tenta modificar um cadastro reservado a administrador
- **THEN** a API retorna HTTP 403 e nenhum dado é alterado

#### Scenario: Administrador autorizado
- **WHEN** um administrador autenticado executa uma ação válida
- **THEN** a operação prossegue e fica atribuída à sua identidade

### Requirement: Tentativas de autenticação são protegidas contra abuso
O serviço SHALL limitar tentativas repetidas, aplicar atraso ou bloqueio temporário
configurável e registrar eventos relevantes sem expor dados secretos.

#### Scenario: Repetição de falhas
- **WHEN** uma origem ou conta excede o limite configurado de falhas em uma janela
- **THEN** novas tentativas são temporariamente limitadas e a resposta não permite enumerar usuários

### Requirement: Credencial administrativa inicial é provisionada com segurança
A primeira conta administrativa SHALL ser criada por um procedimento explícito e
idempotente que receba o segredo fora do repositório e exija sua substituição ou
confirmação conforme a política institucional.

#### Scenario: Primeiro provisionamento
- **WHEN** uma instalação vazia recebe as variáveis de provisionamento válidas
- **THEN** uma única conta administrativa é criada com senha armazenada por derivação criptográfica apropriada

#### Scenario: Reinício posterior
- **WHEN** a instalação já provisionada reinicia com as mesmas variáveis
- **THEN** nenhuma conta duplicada é criada e a credencial existente não é redefinida silenciosamente

### Requirement: Autenticação não implica autorização clínica
A plataforma SHALL deixar explícito que os papéis desta capacidade cobrem apenas
as funções administrativas existentes e SHALL NOT conceder acesso a prontuário ou
dados clínicos futuros por inferência.

#### Scenario: Introdução futura de dado assistencial
- **WHEN** uma capacidade clínica for adicionada
- **THEN** ela exige especificação própria de autorização contextual antes de reutilizar os papéis administrativos

