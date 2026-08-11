# accessibility-baseline Specification

## Purpose
Define uma baseline acessível para login, navegação, tabelas, formulários e modais
já existentes, reduzindo barreiras para trabalhadores com diferentes condições e
níveis de familiaridade digital.
## Requirements
### Requirement: Fluxos existentes são operáveis por teclado
Todos os controles interativos dos fluxos existentes SHALL ser alcançáveis e
acionáveis por teclado, em ordem coerente, com foco visível e sem armadilha de
foco.

#### Scenario: Navegação pelo login
- **WHEN** uma pessoa usa somente teclado no formulário de login
- **THEN** ela percorre campos, visualização de senha e envio em ordem lógica e identifica visualmente o foco

#### Scenario: Modal aberto
- **WHEN** um modal é aberto por teclado
- **THEN** o foco entra no modal, permanece nele enquanto aberto, fecha por mecanismo documentado e retorna ao controle de origem

### Requirement: Controles possuem nome, estado e instrução acessíveis
Campos, botões, links, ícones e mensagens SHALL expor nomes e estados compreensíveis
a tecnologias assistivas; significado não SHALL depender apenas de cor ou forma.

#### Scenario: Botão somente com ícone
- **WHEN** um leitor de tela encontra ação de editar, excluir, aumentar fonte ou alterar contraste
- **THEN** anuncia o propósito e o estado pertinente da ação

#### Scenario: Erro de formulário
- **WHEN** a submissão contém campos inválidos
- **THEN** o resumo e cada campo indicam o erro em texto, associam-no ao controle e movem ou orientam o foco de forma previsível

### Requirement: Preferências visuais são persistentes e limitadas
A plataforma SHALL oferecer contraste elevado e redimensionamento de fonte dentro
de limites seguros, persistindo a escolha no mesmo navegador sem impedir a
reinicialização para o padrão.

#### Scenario: Preferência persistida
- **WHEN** o usuário ajusta contraste ou fonte e recarrega a aplicação
- **THEN** a preferência válida é reaplicada antes ou durante a renderização sem tornar o conteúdo inacessível

#### Scenario: Valor persistido inválido
- **WHEN** o navegador contém preferência fora dos limites aceitos
- **THEN** a aplicação usa o padrão seguro e permite novo ajuste

### Requirement: Estrutura e contraste possuem critérios verificáveis
As páginas existentes SHALL usar títulos, regiões, tabelas e rótulos semânticos e
SHALL atingir, no mínimo, os critérios WCAG 2.2 nível AA aplicáveis a contraste,
teclado, foco e identificação de erros.

#### Scenario: Verificação automatizada
- **WHEN** os fluxos existentes são submetidos à verificação automatizada de acessibilidade
- **THEN** nenhuma violação de impacto crítico ou sério permanece sem exceção documentada e aprovada

#### Scenario: Verificação manual de teclado
- **WHEN** login e um CRUD representativo são percorridos manualmente sem mouse
- **THEN** todas as ações essenciais podem ser concluídas e o resultado é registrado como evidência de aceite

