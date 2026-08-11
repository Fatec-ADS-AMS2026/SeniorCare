## Purpose

Estabelece uma execução reproduzível e configurável para a API e os dois
front-ends, evitando dependências de endereços locais e falhas silenciosas entre
desenvolvimento, teste e produção.

## ADDED Requirements

### Requirement: Todos os componentes produzem artefatos de entrega
Cada componente versionado SHALL concluir seu build de produção a partir de uma
instalação limpa das dependências e sem erros de compilação, tipagem ou resolução
de módulos.

#### Scenario: Build limpo da plataforma
- **WHEN** o pipeline executa os builds da API, do front-end assistencial e do front-end de estoque em um checkout limpo
- **THEN** os três builds terminam com sucesso e produzem os artefatos esperados

#### Scenario: Erro de tipagem bloqueia a entrega
- **WHEN** um front-end contém uma incompatibilidade TypeScript
- **THEN** o build falha e a versão não pode avançar para publicação

### Requirement: Endereço da API é configurável por ambiente
Cada front-end SHALL obter o endereço público da API de configuração fornecida no
build ou na execução, sem depender de `localhost` em artefatos de produção.

#### Scenario: Execução local
- **WHEN** o front-end é iniciado no ambiente de desenvolvimento com a URL local configurada
- **THEN** suas requisições usam a API local informada

#### Scenario: Execução em produção
- **WHEN** o front-end é publicado com a URL de produção configurada
- **THEN** o navegador envia as requisições para essa URL e o bundle não contém a URL local como destino operacional

#### Scenario: Configuração obrigatória ausente
- **WHEN** um build ou startup de produção não recebe a URL obrigatória da API
- **THEN** o processo falha com uma mensagem de configuração clara e sem expor segredos

### Requirement: Configurações e segredos são separados
A plataforma SHALL aceitar opções não secretas por configuração versionável e
segredos somente por mecanismos externos ao repositório, mantendo valores reais
fora de imagens, bundles, logs e arquivos rastreados.

#### Scenario: Credencial fornecida por ambiente
- **WHEN** a API inicia em produção
- **THEN** credenciais de banco, chaves de autenticação e usuário inicial são obtidos de fontes externas ao código e não são registrados em log

#### Scenario: Exemplo de configuração
- **WHEN** um desenvolvedor prepara um novo ambiente
- **THEN** encontra um modelo sem segredos que enumera as variáveis obrigatórias e seus formatos

### Requirement: Diagnóstico distingue vida e prontidão
A API SHALL expor diagnóstico de vida do processo e prontidão das dependências
necessárias, sem divulgar dados pessoais, credenciais ou detalhes internos
sensíveis.

#### Scenario: Processo vivo com banco indisponível
- **WHEN** o processo da API está em execução, mas o banco não responde
- **THEN** a verificação de vida permanece disponível e a verificação de prontidão informa indisponibilidade

#### Scenario: Serviço pronto
- **WHEN** API e banco estão operacionais e a migração exigida está aplicada
- **THEN** a verificação de prontidão retorna sucesso

