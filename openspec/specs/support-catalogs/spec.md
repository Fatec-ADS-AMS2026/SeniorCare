# support-catalogs Specification

## Purpose
Define contratos previsíveis e seguros para os cadastros auxiliares já presentes
e completa o catálogo de produtos necessário ao front-end de estoque.
## Requirements
### Requirement: Contrato uniforme para cadastros auxiliares
Os cadastros de plano de saúde, cargo, religião, fornecedor, fabricante,
transportadora, grupo de produto, tipo de produto, unidade de medida e produto
SHALL usar a mesma convenção de sucesso, paginação e erro.

#### Scenario: Consulta paginada
- **WHEN** um usuário autorizado consulta um cadastro com página, tamanho e filtro válidos
- **THEN** a API retorna os itens, a paginação solicitada e o total encontrado em um contrato uniforme

#### Scenario: Recurso inexistente
- **WHEN** um usuário autorizado consulta, altera ou remove um identificador inexistente
- **THEN** a API retorna HTTP 404 no envelope de erro padronizado

#### Scenario: Requisição inválida
- **WHEN** os dados enviados violam uma regra de validação
- **THEN** a API retorna HTTP 400 ou 422 com erros por campo e não altera o banco

#### Scenario: Falha interna
- **WHEN** ocorre uma falha inesperada
- **THEN** a API retorna um identificador de correlação sem expor exceção, credencial, consulta ou estrutura interna

### Requirement: Alterações preservam identidade e concorrência
Uma atualização SHALL usar o identificador da rota como identidade canônica,
rejeitar divergências e detectar edição concorrente para não sobrescrever uma
alteração posterior sem aviso.

#### Scenario: Identificador divergente
- **WHEN** o identificador do corpo diverge do identificador da rota
- **THEN** a API rejeita a requisição e nenhum registro é alterado

#### Scenario: Versão desatualizada
- **WHEN** um cliente tenta salvar uma versão anterior à versão persistida
- **THEN** a API retorna conflito e fornece informação suficiente para o cliente recarregar o registro

### Requirement: Exclusão respeita referências e histórico administrativo
Um cadastro referenciado SHALL NOT ser removido fisicamente. A plataforma SHALL
permitir inativação quando o item não puder mais ser usado, preservando registros
que já o referenciam.

#### Scenario: Inativação de item em uso
- **WHEN** um administrador inativa um item já referenciado
- **THEN** o item deixa de aparecer como opção para novos registros e permanece legível nos registros existentes

#### Scenario: Exclusão incompatível
- **WHEN** um cliente solicita exclusão física de item referenciado
- **THEN** a API retorna conflito e não produz exclusão em cascata indevida

### Requirement: Catálogo de produtos é integrado de ponta a ponta
A plataforma SHALL persistir e disponibilizar produtos com descrição, nome
genérico, grupo/tipo, unidade de medida e os atributos de controle já apresentados
na interface de estoque.

#### Scenario: Cadastro válido de produto
- **WHEN** um administrador informa os campos obrigatórios e referências ativas válidas
- **THEN** o produto é persistido e pode ser consultado pelos dois lados da integração

#### Scenario: Referência inválida
- **WHEN** o produto referencia tipo ou unidade inexistente ou inativa
- **THEN** a API rejeita o cadastro com erro no campo correspondente

#### Scenario: Consulta e filtro de produtos
- **WHEN** um usuário autorizado pesquisa por descrição ou nome genérico
- **THEN** a API retorna os produtos correspondentes de forma paginada

#### Scenario: Limite do catálogo
- **WHEN** um produto é cadastrado ou atualizado nesta capacidade
- **THEN** a operação não cria movimento, lote, recebimento, dispensação ou inventário, que permanecem fora desta mudança

### Requirement: Operações administrativas são atribuíveis
Criação, alteração e inativação de cadastros SHALL registrar usuário autenticado,
data/hora, tipo de ação e identificador do recurso, sem armazenar senha ou token.

#### Scenario: Alteração autenticada
- **WHEN** um administrador altera um cadastro
- **THEN** a plataforma preserva uma entrada de auditoria atribuível à sessão responsável

