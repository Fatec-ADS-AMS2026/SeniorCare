## Why

A base implementada do SeniorCare ainda não constitui uma linha de entrega
confiável: o front-end assistencial não compila, os front-ends usam uma URL de API
fixa, o produto de estoque não possui backend, o login é apenas visual e não há
testes automatizados. Corrigir essas lacunas agora evita que os futuros módulos de
residente e prontuário sejam construídos sobre contratos inconsistentes e uma
plataforma incapaz de proteger dados pessoais sensíveis.

## What Changes

- Restaurar builds reproduzíveis dos três componentes e configurar URLs e opções
  de execução por ambiente, com falha explícita para configuração inválida.
- Padronizar os contratos dos CRUDs auxiliares, incluindo validação, paginação,
  respostas de erro, semântica de atualização e tratamento de exclusão.
- Completar o catálogo de produtos no backend e no banco, alinhando-o à interface
  de estoque já existente.
- Transformar o login visual em uma capacidade institucional de identidade e
  acesso, adaptada do Qualitas: contas individuais com ciclo de vida, política de
  senha, MFA, sessões compartilhadas entre os módulos, papéis técnicos, grupos de
  permissões, responsabilidades organizacionais, exceções individuais e decisões
  de acesso auditáveis.
- Criar APIs e telas administrativas para configurar usuários, papéis, grupos,
  permissões, vínculos organizacionais, políticas de segurança e sessões ativas,
  mantendo a autorização efetiva no backend.
- Consolidar uma baseline de acessibilidade para os componentes já existentes,
  com navegação por teclado, nomes acessíveis, foco visível, contraste e ajuste de
  fonte persistente.
- Introduzir testes automatizados de backend e front-end e torná-los gates do CI,
  junto aos builds, lint e verificações de segurança existentes.
- Documentar migração, compatibilidade e critérios de aceite da plataforma
  estabilizada.
- **BREAKING**: endpoints administrativos deixarão de aceitar acesso anônimo e
  passarão a retornar um envelope de erro uniforme; clientes deverão autenticar-se,
  respeitar as permissões efetivas configuradas e tratar os códigos HTTP definidos
  nas specs.
- Manter fora desta mudança residente, prontuário multidisciplinar, cuidado por
  turno, medicamentos, nutrição, financeiro, doações, dashboards e assinatura
  eletrônica.

## Capabilities

### New Capabilities

- `runtime-configuration`: build reproduzível, configuração da API por ambiente,
  validação de startup e diagnóstico operacional dos componentes atuais.
- `support-catalogs`: contratos consistentes para os cadastros auxiliares e fluxo
  integrado do catálogo de produtos.
- `platform-authentication`: identidade vinculada à instituição, autenticação
  individual, política de senha, MFA, sessões revogáveis, RBAC enriquecido com
  escopo organizacional e exceções, configuração administrativa e auditoria de
  decisões de acesso.
- `accessibility-baseline`: comportamento acessível e preferências visuais nos
  componentes e fluxos já implementados.
- `automated-quality-gates`: testes automatizados e gates obrigatórios de
  qualidade para backend e front-ends.

### Modified Capabilities

Nenhuma. O repositório ainda não possui specs principais publicadas; todas as
capacidades desta mudança inauguram contratos verificáveis para a base existente.

## Impact

- **Domínios afetados:** capacidades de apoio de profissionais (cargos), jornada
  do residente (planos de saúde e religião), estoque e operação (catálogos e
  produtos) e requisitos transversais de segurança e acessibilidade.
- **Atores afetados:** administradores institucionais e de segurança,
  trabalhadores autorizados, profissionais da equipe multidisciplinar,
  desenvolvedores, equipe de operação e, indiretamente, futuros usuários
  assistenciais. Cargo ou profissão não concederá acesso técnico implicitamente,
  e residentes não terão fluxo funcional criado nesta mudança.
- **Código:** API ASP.NET Core, Entity Framework/migrações, ambos os front-ends
  React, configuração Docker/nginx e workflows GitHub Actions.
- **APIs e dados:** novos endpoints e entidades para instituição, usuários,
  credenciais, MFA, papéis, permissões, grupos, vínculos organizacionais, exceções,
  políticas, sessões e auditoria, além do endpoint de produto e da revisão dos
  contratos CRUD; novas migrações; configuração de usuários iniciais sem
  credencial fixa no código ou no repositório.
- **Risco assistencial:** a mudança não autoriza uso com prontuários ou dados reais
  de saúde. Até existir autorização contextual, auditoria e o núcleo longitudinal,
  os ambientes de desenvolvimento, teste e demonstração devem usar dados
  sintéticos.
- **Impacto regulatório:** a autenticação e os controles mínimos reduzem exposição,
  mas não demonstram conformidade integral com a LGPD, S-RES, NGS2/SBIS ou normas
  profissionais. Nenhuma operação sem papel ou assinatura eletrônica será
  declarada.
