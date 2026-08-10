# Relatório de avaliação: requisitos versus implementação

- **Projeto:** SeniorCare
- **Data da avaliação:** 6 de agosto de 2026
- **Baseline de requisitos:** `docs/escopo-do-projeto.md`, versão 1.0
- **Baseline do código:** commit `d3b68cd`
- **Natureza da avaliação:** inspeção estática, rastreabilidade por evidências e
  validações locais disponíveis

## 1. Resultado executivo

O SeniorCare possui uma base técnica e operacional relevante, mas o produto ainda
está em estágio **pré-MVP assistencial**. A implementação predominante é formada
por CRUDs de cadastros auxiliares, dois front-ends React, uma API ASP.NET Core com
PostgreSQL e uma estrutura madura de CI/CD e operação em contêineres.

O núcleo definido no escopo — residente longitudinal, assistência cotidiana,
prontuário multidisciplinar, alimentação, financeiro, doações, conformidade e
dashboards — ainda não está implementado. O próprio escopo reconhece essa
distância em sua seção 12; esta avaliação confirma a afirmação com evidências do
código atual.

No nível dos 11 domínios funcionais avaliados:

| Classificação | Quantidade | Proporção |
|---|---:|---:|
| Implementado | 0 | 0% |
| Parcialmente implementado | 3 | 27% |
| Não implementado | 8 | 73% |

Esses percentuais são uma contagem por domínio, sem ponderação por tamanho ou
risco. Eles não devem ser interpretados como percentual de linhas de código ou
de esforço concluído.

Os três domínios parciais são profissionais, estoque/operação e
governança/conformidade. Mesmo neles, somente capacidades auxiliares ou de
infraestrutura estão presentes; nenhum fluxo de negócio do domínio está completo
de ponta a ponta.

### Conclusão de prontidão

- **Pronto como núcleo de gestão de ILPI:** não.
- **Pronto para registrar prontuário ou dados clínicos reais:** não.
- **Pronto para operação sem papel ou assinatura eletrônica:** não.
- **Pronto para dashboards estatísticos:** não; faltam dados operacionais de
  origem, definições de indicadores e camada analítica.
- **Pronto como base evolutiva:** parcialmente; a infraestrutura é promissora,
  mas a aplicação assistencial não compila no estado avaliado e não há testes
  automatizados.

## 2. Método e critérios

A avaliação cruzou:

1. os domínios, fluxos e requisitos transversais do escopo canônico;
2. o grafo de conhecimento gerado pelo Graphify;
3. entidades, migrações, controllers, serviços e repositórios do backend;
4. rotas, páginas, formulários e serviços dos dois front-ends;
5. configuração de CI/CD, segurança, deploy, health check e backup;
6. lint e build executáveis no ambiente local;
7. presença de testes automatizados e artefatos OpenSpec.

Classificações utilizadas:

- **Implementado:** o fluxo essencial do requisito existe de ponta a ponta, com
  persistência, API, interface quando aplicável e evidência mínima de validação.
- **Parcial:** existe uma capacidade executável relevante, mas o fluxo essencial,
  a integração, os controles ou as validações ainda estão incompletos.
- **Não implementado:** não foi encontrada capacidade executável correspondente;
  documentação, enums, telas estáticas e catálogos auxiliares isolados não foram
  contados como implementação do domínio principal.
- **Não verificado:** há código, mas não foi possível executar a validação
  necessária no ambiente disponível.

O escopo é evolutivo. Portanto, “não implementado” representa uma lacuna frente
ao estado-alvo, não necessariamente um atraso: a prioridade deverá ser decidida
por mudanças OpenSpec e pelo MVP da ILPI-piloto.

## 3. Inventário comprovado da implementação atual

### 3.1 Backend

A API possui nove entidades persistidas e nove controllers:

- plano de saúde;
- cargo;
- religião;
- fornecedor;
- fabricante;
- transportadora;
- grupo de produto;
- tipo de produto;
- unidade de medida.

Evidências principais:

- `SeniorCareManager-Backend/SeniorCareManager.WebAPI/Data/AppDbContext.cs:12`
  declara somente esses nove `DbSet`;
- `SeniorCareManager-Backend/SeniorCareManager.WebAPI/Data/Migrations/20260805233320_InitialCreate.cs:14`
  cria as tabelas correspondentes;
- `SeniorCareManager-Backend/SeniorCareManager.WebAPI/Startup.cs:116` registra os
  nove pares de serviços e repositórios;
- os controllers expõem CRUDs REST em `api/v1`.

Não foram encontradas entidades para residente, usuário, instituição, quarto,
leito, profissional, escala, cuidado, ocorrência, prontuário, evolução,
prescrição, medicamento, dieta, refeição, conta financeira, doação, campanha,
lote, movimento de estoque, auditoria, documento assinado ou indicador.

### 3.2 Front-end assistencial

O front-end assistencial oferece:

- landing page, login visual e estrutura administrativa;
- CRUDs de religião, plano de saúde e cargo;
- componentes reutilizáveis de formulário, tabela, modal e navegação;
- alternância de alto contraste e ajuste de tamanho da fonte.

Não há páginas ou rotas de residente, cuidado, prontuário, medicamentos,
nutrição, família, escalas, finanças, doações, conformidade ou dashboards.

### 3.3 Front-end de estoque

O front-end de estoque oferece interfaces para:

- transportadoras, fabricantes, fornecedores;
- grupos e tipos de produto;
- unidades de medida;
- cadastro visual de produtos.

O cadastro de produto existe apenas no front-end. Ele chama o endpoint
`Product`, mas o backend não possui `ProductController`, entidade `Product` ou
`DbSet<Product>`. Consequentemente, esse fluxo não está integrado.

### 3.4 Infraestrutura e entrega

Estão presentes:

- CI com lint e build condicional por módulo;
- análise SAST com CodeQL;
- análise de dependências e verificação de segredos;
- imagens Docker e release pelo GHCR;
- deploy pull-based com imagens fixadas por digest;
- PostgreSQL com volume persistente;
- health check da API e do banco;
- backup diário com retenção configurável;
- configuração de CORS por ambiente.

Essa é uma base forte de engenharia, mas infraestrutura de entrega não substitui
os controles de segurança, auditoria e integridade exigidos dentro da aplicação.

## 4. Matriz dos domínios funcionais

| ID | Domínio | Situação | Evidência e lacuna principal |
|---|---|---|---|
| 6.1 | Jornada e cadastro longitudinal do residente | **Não implementado** | Não há modelo, tabela, API, rota ou tela de residente, responsável, admissão, contrato, dependência, quarto ou leito. Plano de saúde e religião são apenas catálogos auxiliares. |
| 6.2 | Assistência cotidiana e atividades de vida diária | **Não implementado** | Não há agenda de cuidados, tarefas por turno, execução, recusa, passagem de plantão, ocorrência ou escalonamento. |
| 6.3 | Saúde e cuidado multidisciplinar | **Não implementado** | Não há prontuário, avaliações, riscos, plano individual, medicamentos, sinais vitais, consultas, exames, evoluções ou autoria profissional. O plano de saúde cadastrado não implementa cuidado clínico. |
| 6.4 | Alimentação e nutrição | **Não implementado** | Não há avaliação nutricional, dietas, cardápios, refeições, aceitação, hidratação ou integração com consumo de estoque. |
| 6.5 | Assistência social, família, convivência e autonomia | **Não implementado** | Não há estudo social, rede de apoio, visitas, atividades ou participação do residente. O catálogo de religião é somente um pré-requisito de cadastro. |
| 6.6 | Profissionais, escalas e educação permanente | **Parcial** | Há CRUD de cargos, mas não há trabalhador, vínculo, conselho profissional, escala, turno, treinamento, dimensionamento ou alertas de cobertura. |
| 6.7 | Gestão financeira | **Não implementado** | Não há contas a pagar/receber, fluxo de caixa, orçamento, centro de custo, contrato financeiro ou prestação de contas. |
| 6.8 | Doações, captação e voluntariado | **Não implementado** | Não há doador, campanha, doação financeira/material, restrição de finalidade, recibo, destinação ou voluntário. |
| 6.9 | Compras, estoque, patrimônio e operação da casa | **Parcial** | Há catálogos de fornecedor, fabricante, transportadora, grupos, tipos e unidades. Produto está apenas no front-end. Não há compra, cotação, lote, validade, movimento, inventário, dispensação, patrimônio, manutenção ou operação da casa. |
| 6.10 | Governança, qualidade, segurança e conformidade | **Parcial** | CI/CD, health checks, backup e verificações de segurança estão presentes. Não há licenças, procedimentos, inspeções, não conformidades, indicadores obrigatórios, notificações, auditoria de aplicação ou documentos assinados. |
| 6.11 | Dashboards estatísticos e inteligência institucional | **Não implementado** | Não há tela, API, modelo analítico, dicionário de indicadores, agregações, exportações ou rastreabilidade estatística. |

## 5. Matriz dos fluxos integrados prioritários

| Fluxo | Situação | Avaliação |
|---|---|---|
| Admissão e início do cuidado | **Não implementado** | Nenhuma etapa do fluxo possui modelo operacional; há apenas catálogos que poderão apoiá-lo. |
| Ciclo contínuo do cuidado | **Não implementado** | Não existem avaliação, objetivos, intervenções, execução ou revisão do plano. |
| Intercorrência de saúde | **Não implementado** | Não existem ocorrência, acionamento, remoção, comunicação, notificação ou investigação. |
| Alimentação integrada | **Não implementado** | Não existem dados nutricionais, cardápio, produção, consumo ou reavaliação. |
| Doação financeira | **Não implementado** | Não existem doador, recebimento, finalidade, conciliação ou prestação de contas. |
| Doação de materiais | **Não implementado** | O catálogo de estoque não possui oferta, triagem, aceite, lote, destinação ou prestação de contas. |

Nenhum fluxo prioritário pode ser demonstrado de ponta a ponta no estado atual.

## 6. Matriz dos requisitos transversais

| ID | Requisito | Situação | Evidência e lacuna principal |
|---|---|---|---|
| 8.1 | Segurança e privacidade | **Parcial, insuficiente para dados reais** | Há CORS configurável, HSTS em produção, scanners no CI e backup. Porém não existe autenticação no backend, `UseAuthorization` está comentado (`Startup.cs:181`), não há `[Authorize]`, RBAC, MFA, usuário, auditoria, consentimento, retenção ou segregação de dados. |
| 8.2 | Usabilidade e acessibilidade | **Parcial** | Há design responsivo, alto contraste e redimensionamento persistido. A página específica de acessibilidade é apenas um placeholder e não há teste WCAG, auditoria de teclado/leitor de tela ou tratamento de retomada de tarefas. |
| 8.3 | Rastreabilidade e integridade | **Não implementado** | Modelos não registram autor, datas, estado, versão ou motivo de correção; CRUDs permitem atualização e exclusão física. |
| 8.4 | Interoperabilidade | **Parcial** | A API REST, Swagger em desenvolvimento e PostgreSQL constituem pontos técnicos de integração. Não há integração contábil, bancária, clínica, documental, de pagamento ou padrão nacional de saúde. |
| 8.5 | Configuração institucional | **Não implementado** | Existem variáveis técnicas de ambiente, mas não modelos configuráveis de instituição, unidade, capacidade, papéis, instrumentos, alertas, aprovações ou regras locais. |
| 8.6 | Assinaturas eletrônicas e redução de papel | **Não implementado, previsto para evolução** | Não há documento eletrônico estável, hash, certificado, assinatura, validação, carimbo de tempo ou preservação. As etapas prévias de identidade, autoria, auditoria e versionamento também ainda não existem. |
| 8.7 | Governança de dados na parceria universitária | **Não implementado na aplicação** | Há separação técnica entre composições de desenvolvimento e produção, mas não há perfis acadêmicos, dados sintéticos formalizados, aprovação de acesso, anonimização, trilha de pesquisa ou controle de finalidade. |

## 7. Avaliação específica das capacidades críticas

### 7.1 Prontuário multidisciplinar

**Situação: não iniciado no código. Risco: crítico.**

Não foi encontrada estrutura para:

- residente e identidade longitudinal;
- profissionais e conselhos;
- autoria e autorização por profissão;
- registro imutável concluído;
- adendo e correção versionada;
- plano individual interdisciplinar;
- evolução, avaliação, prescrição ou anexos;
- auditoria de leitura e alteração;
- contingência e exportação assistencial.

O prontuário não deve ser implementado como CRUD genérico sobre o padrão atual.
Antes das telas clínicas, é necessário especificar identidade, estados do
registro, imutabilidade, adendos, permissões contextuais, auditoria e cadeia de
evidências.

### 7.2 Assinatura eletrônica

**Situação: apenas documentada como evolução futura. Risco atual: controlado,
desde que o sistema não seja usado como prontuário sem papel.**

A ausência de integração com o certificado ICP-Brasil/CFM é coerente com a fase
de expansão definida no escopo. O problema é que os pré-requisitos da etapa 1 —
identidade, papéis, autoria, auditoria, versionamento e exportação confiável —
também não existem.

A próxima decisão técnica não deve ser escolher um provedor de assinatura. Deve
ser projetar o registro eletrônico confiável e identificar, por profissão e tipo
documental, quais atos exigirão assinatura simples, avançada ou qualificada.

### 7.3 Dashboards estatísticos

**Situação: não iniciado no código. Risco: alto se antecipado.**

Os dashboards dependem de registros operacionais confiáveis que ainda não
existem. Implementar gráficos agora produziria telas sem proveniência, histórico
ou denominadores confiáveis. A ordem recomendada é:

1. definir o dicionário mínimo de indicadores do piloto;
2. implementar os eventos e registros operacionais de origem;
3. criar verificações de completude e consistência;
4. entregar um painel operacional básico;
5. somente então separar uma camada analítica histórica.

## 8. Validações técnicas executadas

| Verificação | Resultado | Observação |
|---|---|---|
| Consulta Graphify de requisitos versus código | **Executada** | O grafo orientou a navegação, mas as classificações foram confirmadas diretamente nos arquivos. |
| Lint do front-end assistencial | **Passou** | O comando avançou do lint para a compilação. |
| Build do front-end assistencial | **Falhou** | Erros TypeScript em `src/features/api/hooks/useApiHandler.ts:14-17`: uso de `error` em vez de `errors` e atribuição possivelmente `undefined`. |
| Lint do front-end de estoque | **Passou** | Sem erro reportado. |
| Build do front-end de estoque | **Passou** | Vite gerou o bundle de produção; houve apenas aviso de base Browserslist desatualizada. |
| Build do backend | **Não verificado localmente** | O executável `dotnet` não está instalado no ambiente da avaliação. O CI possui passo de build, mas isso não substitui a execução desta rodada. |
| Testes automatizados | **Ausentes** | Não há projeto de testes no solution, arquivos de teste nos fontes nem scripts de teste nos `package.json`. O próprio CI registra a ausência em `.github/workflows/ci.yml:56`. |
| Mudanças OpenSpec | **Ausentes** | `openspec list --json` retornou lista vazia. |

## 9. Riscos e lacunas priorizados

### Críticos — bloqueiam uso assistencial real

1. **Ausência de autenticação, autorização e auditoria.** A presença de uma
   definição Bearer no Swagger e de interceptor de cookie no front-end não cria
   segurança: o backend não valida token e todos os controllers estão públicos.
2. **Ausência do núcleo residente/prontuário.** Não é possível representar a
   pessoa cuidada, preservar sua história ou atribuir atos profissionais.
3. **Ausência de integridade clínica.** O padrão CRUD atual permite alteração e
   exclusão, sem versão, autoria ou adendo; ele não é adequado para registros
   concluídos de saúde.

### Altos — bloqueiam um MVP confiável

4. **Front-end assistencial não compila.** O principal módulo de cuidado não
   produz bundle no estado avaliado.
5. **Nenhum teste automatizado.** Não há proteção contra regressão em API,
   persistência, permissões ou interfaces.
6. **URL de API fixa em localhost.** Ambos os front-ends usam
   `https://localhost:7053/api/v1/`; em uma implantação, o navegador do usuário
   tentará acessar sua própria máquina, salvo transformação externa não
   encontrada no código.
7. **Produto sem backend.** A interface de produto chama um endpoint inexistente,
   impedindo completar até mesmo o catálogo básico de estoque.
8. **Ausência de especificações incrementais.** O OpenSpec está configurado, mas
   não há mudança ativa para transformar o escopo amplo em requisitos e testes
   implementáveis.

### Médios — dívida de qualidade e operação

9. **Tratamento de erro inconsistente.** Controllers convertem diferentes falhas
   em HTTP 500 e retornam mensagens genéricas; `PATCH` executa atualização
   completa.
10. **Backup sem evidência de restauração testada.** O script diário existe, mas
    esta avaliação não encontrou ensaio automatizado de recuperação.
11. **Acessibilidade sem critérios verificáveis.** Recursos visuais existem, mas
    faltam metas WCAG e testes automatizados/manuais documentados.
12. **Swagger apenas em desenvolvimento.** É útil para desenvolvimento, mas não
    existe contrato de API versionado ou teste de compatibilidade.

## 10. Sequência recomendada de evolução

### Prioridade 0 — estabilizar a linha de base

- corrigir o build do front-end assistencial;
- tornar a URL da API configurável por ambiente;
- alinhar o cadastro de produto entre banco, backend e front-end ou removê-lo da
  navegação até estar integrado;
- criar projetos de testes e incluir `dotnet test` e testes de front-end no CI;
- validar build e migração do backend em ambiente reproduzível;
- criar uma mudança OpenSpec para a fundação do produto.

### Prioridade 1 — fundação segura

- instituição, unidade, quarto e leito;
- identidade, autenticação, sessão e papéis;
- profissional, vínculo, função e conselho;
- residente, responsáveis e admissão básica;
- trilha de auditoria append-only para dados sensíveis;
- estados, autoria e versionamento dos registros;
- política de dados sintéticos e acesso universitário;
- testes de isolamento, autorização e auditoria.

### Prioridade 2 — operação cotidiana

- plano individual básico;
- agenda por turno e registro de execução, recusa e não realização;
- passagem de plantão, ocorrência e comunicação crítica;
- escalas e dimensionamento mínimo;
- painel operacional de pendências, ocupação, riscos e cobertura;
- dicionário versionado dos primeiros indicadores.

### Prioridade 3 — prontuário multidisciplinar

- especificação própria do modelo clínico e profissional;
- avaliações, evoluções, riscos e objetivos interdisciplinares;
- imutabilidade após conclusão e correção por adendo;
- medicamentos, sinais vitais, vacinação, consultas e exames;
- exportação e contingência;
- testes de autorização por profissão e preservação histórica.

### Prioridade 4 — sustentabilidade institucional

- nutrição integrada, compras, lote, validade e consumo;
- financeiro e centros de custo;
- doações financeiras e materiais com finalidade e destinação;
- conformidade, inspeções, planos de ação e indicadores obrigatórios;
- dashboards táticos e estratégicos derivados dos registros de origem.

### Prioridade 5 — assinatura e redução de papel

- inventário de documentos e signatários;
- exportação estável e validação de assinatura externa;
- integração ICP-Brasil sem custódia de chave privada;
- preservação de longo prazo, carimbo de tempo e verificação independente;
- avaliação formal de S-RES, NGS2/SBIS e normas dos conselhos profissionais.

## 11. Critérios mínimos para declarar o primeiro MVP

O primeiro MVP não deve ser declarado pronto apenas por quantidade de telas. No
mínimo, deve demonstrar:

1. build reproduzível dos três componentes e migração do banco;
2. autenticação real, autorização por papel e auditoria de acesso/alteração;
3. cadastro longitudinal de residente sem perda de histórico;
4. profissional com identidade e conselho quando aplicável;
5. plano de cuidado básico e tarefas por turno;
6. distinção entre planejado, executado, recusado e não realizado;
7. ocorrência com escalonamento e rastreabilidade;
8. correção por nova versão ou adendo, sem sobrescrita silenciosa;
9. painel operacional com indicadores definidos e rastreáveis;
10. testes automatizados dos fluxos críticos e restauração de backup ensaiada;
11. uso apenas de dados sintéticos até a homologação dos controles de segurança,
    privacidade e governança.

## 12. Parecer final

O código atual é uma boa semente técnica e contém cadastros de apoio que podem ser
reaproveitados, especialmente no estoque. A infraestrutura de CI/CD, análise de
segurança, deploy e backup está mais madura do que o domínio funcional.

Entretanto, o SeniorCare ainda não deve ser apresentado como software completo de
gestão de ILPI nem receber dados reais de saúde. O caminho mais seguro é preservar
os componentes úteis, corrigir a linha de base e iniciar uma fundação explícita
para identidade, residente, profissionais, autorização, auditoria e integridade.
Prontuário, dashboards e assinatura eletrônica devem evoluir nessa ordem de
dependência, por mudanças OpenSpec verificáveis.

## 13. Limitações da avaliação

- O backend foi inspecionado estaticamente, mas não compilado localmente por
  ausência do SDK `dotnet`.
- Não foi realizada avaliação dinâmica de segurança, teste de invasão, auditoria
  WCAG formal ou validação jurídica/regulatória.
- Os documentos históricos em DOCX/PDF foram tratados como evidência de processo
  acadêmico, não como prova de software executável.
- A classificação considera o commit informado e deverá ser atualizada após cada
  incremento relevante do produto.

## 14. Atualização pós stabilize-existing-platform (§1-§12)

- **Data desta atualização:** 10 de agosto de 2026.
- **Baseline do código:** conclusão da mudança OpenSpec `stabilize-existing-platform`
  (seções §1 a §12), que corrigiu a linha de base técnica e de infraestrutura
  identificada na avaliação original (seções 12-13 acima) sem tocar no escopo
  assistencial ainda não implementado (residente, prontuário, dashboards,
  assinatura — continuam ausentes, ver seção 3-10 acima).

O parecer da seção 12 apontava a infraestrutura como "mais madura do que o
domínio funcional" e listava, como critério de MVP ainda não atendido:
autenticação real, autorização por papel, auditoria, testes automatizados dos
fluxos críticos e uso só de dado sintético até homologação de segurança. Esta
mudança fecha diretamente essas lacunas transversais:

1. **Backend agora compila e roda testes de verdade** — a limitação da seção
   13 ("não compilado localmente por ausência do SDK dotnet") não se aplica
   mais: 28 testes unitários + 118 de integração (Testcontainers PostgreSQL
   real, incluindo migração desde banco vazio e sobre dado pré-existente da
   versão anterior) rodam em CI a cada PR, com cobertura publicada.
2. **Autenticação e autorização real** — login por sessão (cookie
   `HttpOnly`+`Secure`), MFA obrigatório para conta administrativa,
   recuperação de conta, e controle de acesso completo (papéis, grupos de
   permissão, exceções, políticas, escopo institucional/organizacional) nos
   dois front-ends, com API como autoridade final de decisão.
3. **Auditoria** — autenticações, MFA, ativações, mudanças de estado de
   conta, sessões e decisões de acesso protegido são registradas de forma
   imutável.
4. **9 catálogos administrativos + Produto** com paginação, filtro,
   concorrência otimista e contrato OpenAPI publicado e validado contra os
   dois front-ends.
5. **Baseline de acessibilidade WCAG 2.2 AA** — a limitação da seção 13
   ("não foi realizada... auditoria WCAG formal") tem agora verificação
   automatizada real (jest-axe cobrindo login/modal/tabela/formulário nos
   dois apps) somada a contraste recalculado à mão e navegação por teclado
   verificada mecanicamente (login + CRUD de referência por app) — não é uma
   auditoria WCAG completa de todo o produto (esse continua sendo um item
   maior, fora do escopo desta mudança), mas os componentes compartilhados e
   as jornadas críticas hoje atendem o nível AA nos critérios aplicáveis.
6. **CI/CD fechado** — build+teste+cobertura dos três componentes, migração
   testada em banco vazio e sobre versão anterior, pré-validação de dado
   antes de aplicar migração em produção, verificação de fixture sintética
   (nenhum dado pessoal real em teste/seed), e bootstrap de instituição/admin
   agora efetivamente conectado ao deploy (as variáveis `Bootstrap__*`
   existiam desde antes mas nunca chegavam ao container — corrigido e
   verificado com um smoke test real da stack completa nesta seção).

O parecer da seção 12 continua válido: o SeniorCare segue não pronto para
receber dado real de saúde nem ser apresentado como software completo de
gestão de ILPI — o núcleo assistencial (residente, cuidado, prontuário,
financeiro, doações, dashboards) não foi tocado por esta mudança. O que mudou
é que a FUNDAÇÃO que a seção 12 pedia como pré-requisito ("identidade,
residente, profissionais, autorização, auditoria e integridade") agora existe
de fato para identidade/autorização/auditoria — o próximo incremento pode
construir sobre uma base testada, auditável e com CI real, em vez de sobre um
código que não compilava em CI.
