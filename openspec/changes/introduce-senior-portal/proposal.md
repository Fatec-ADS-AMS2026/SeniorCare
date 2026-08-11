## Why

O SeniorCare possui front-ends separados para assistência e estoque, cada um com
entrada, login, navegação e identidade visual próprios. À medida que prontuário,
nutrição, financeiro, doações, dashboards e administração forem adicionados, essa
fragmentação produzirá múltiplos pontos de acesso, descoberta inconsistente de
funcionalidades e risco de divergência na proteção da sessão.

O Senior Portal cria uma entrada institucional única para trabalhadores e gestores
da ILPI, reutilizando a identidade, a sessão e as permissões efetivas definidas na
mudança `stabilize-existing-platform`, sem retirar dos módulos a responsabilidade
por suas regras de negócio nem confundir esse acesso interno com o futuro portal
do residente e da família.

## What Changes

- Criar o Senior Portal como aplicação web institucional de entrada, autenticação,
  seleção de contexto, descoberta e navegação entre módulos do SeniorCare.
- Exibir somente módulos disponíveis e autorizados para a identidade e instituição
  atuais, com estados explícitos de disponível, indisponível, manutenção e acesso
  negado.
- Disponibilizar navegação global, perfil, segurança da conta, preferências de
  acessibilidade, encerramento de sessão e retorno consistente ao portal.
- Reutilizar uma única sessão protegida entre portal, assistência e estoque, sem
  novo login durante a transição autorizada entre aplicações.
- Introduzir um catálogo configurável de módulos com identificador estável, nome,
  descrição, ícone, destino, estado operacional, ordem e permissão exigida.
- Publicar os módulos sob a mesma origem, inicialmente em `/care`, `/stock` e
  `/admin`, preparando destinos futuros para financeiro, doações, dashboards e
  outros domínios sem declarar esses módulos como implementados.
- Adaptar os módulos existentes para oferecer retorno ao portal e consumir o mesmo
  contexto institucional e de sessão.
- Manter a API como autoridade final de autorização; ocultar cards ou menus no
  portal é somente comportamento de interface.
- Diferenciar formalmente o Senior Portal, destinado a usuários internos, do
  futuro portal externo do residente e da família.
- **BREAKING**: após a transição coordenada, os logins e landing pages duplicados
  dos módulos deixarão de ser pontos de entrada primários; URLs antigas deverão
  redirecionar com segurança ao portal, preservando destinos autorizados quando
  possível.

### Objetivos

- oferecer experiência única de acesso aos módulos da instituição;
- reduzir duplicação de login, navegação e preferências transversais;
- permitir crescimento modular sem transformar o portal em uma aplicação de
  negócio monolítica;
- manter a pessoa idosa e seu plano individual no centro dos módulos assistenciais,
  sem carregar dados clínicos desnecessários no portal.

### Não objetivos

- implementar prontuário, nutrição, financeiro, doações ou dashboards nesta
  mudança;
- unificar todo o código dos front-ends ou introduzir microfrontends;
- criar o portal do residente, familiar, responsável legal ou curador;
- exibir resumos clínicos, listas de residentes ou dados financeiros sensíveis na
  página inicial;
- substituir autorização do backend, gestão de identidade ou auditoria;
- suportar múltiplas instituições operacionais além do contexto preparado pelo IAM.

## Capabilities

### New Capabilities

- `senior-portal`: entrada institucional unificada, catálogo e descoberta de
  módulos, navegação autorizada, estados operacionais, sessão compartilhada,
  experiência global e integração incremental dos front-ends existentes.

### Modified Capabilities

Nenhuma. A capacidade depende dos contratos de `platform-authentication` ainda em
planejamento na mudança `stabilize-existing-platform`, mas não altera suas regras.

## Impact

- **Domínios afetados:** identidade e acesso, navegação institucional,
  acessibilidade e integração transversal dos módulos assistencial, estoque e
  administração. Domínios futuros aparecem apenas no catálogo quando existirem.
- **Atores afetados:** administradores institucionais e de segurança, gestores,
  trabalhadores autorizados e profissionais da equipe multidisciplinar. Residentes,
  familiares e representantes não recebem acesso por esta capacidade.
- **Código:** novo front-end React/TypeScript/Vite para o portal; adaptações
  delimitadas nos front-ends assistencial e de estoque; API ASP.NET Core para
  catálogo/contexto; nginx, Docker Compose e CI para roteamento de mesma origem.
- **APIs e dados:** endpoint autenticado de módulos efetivos e configuração do
  catálogo; nenhum dado clínico ou financeiro de negócio será copiado ao portal.
- **Dependências:** conclusão das partes de instituição, identidade, sessão
  compartilhada, permissões efetivas e configuração segura previstas em
  `stabilize-existing-platform`.
- **Risco assistencial:** indisponibilidade ou erro de navegação não pode impedir
  acesso contingencial aos módulos críticos quando a sessão ainda for válida; o
  portal não será tratado como fonte de informação clínica.
- **Impacto regulatório:** a mudança aplica minimização, privacidade por padrão,
  controle de acesso e auditoria compatíveis com a LGPD, mas não demonstra
  conformidade integral nem autoriza compartilhamento de prontuário.
- **Risco operacional:** falha do portal pode dificultar a descoberta dos módulos;
  links diretos protegidos e diagnóstico operacional devem permanecer disponíveis
  para contingência controlada.
