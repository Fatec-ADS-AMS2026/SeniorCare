# Escopo do projeto SeniorCare

- **Status:** contexto conceitual canônico do produto
- **Versão:** 1.1
- **Data:** 6 de agosto de 2026
- **Contexto regulatório de referência:** Brasil

## 1. Resumo executivo

O SeniorCare é uma plataforma de gestão integrada para casas de cuidados e
Instituições de Longa Permanência para Pessoas Idosas (ILPIs). Seu propósito é
apoiar uma instituição residencial na prestação de cuidado digno, seguro,
individualizado e centrado na pessoa idosa, ao mesmo tempo que organiza sua
operação administrativa, financeira e regulatória.

O produto deve integrar, em uma única visão longitudinal:

- assistência cotidiana ao residente;
- acompanhamento de saúde por equipe multidisciplinar;
- alimentação e acompanhamento nutricional;
- convivência, autonomia, vínculos familiares e participação comunitária;
- gestão de profissionais, escalas e qualificação;
- gestão financeira, contratos e contribuições;
- compras, estoque, patrimônio e serviços operacionais;
- gestão de doações, campanhas e prestação de contas;
- qualidade, segurança, documentos e conformidade institucional;
- dashboards estatísticos para acompanhamento assistencial, operacional,
  financeiro e regulatório.

O núcleo do produto não é o cadastro administrativo isolado. O núcleo é a
jornada da pessoa idosa:

```text
Residente
   -> avaliações biopsicossociais e de saúde
   -> plano individual de cuidado
   -> agenda e execução cotidiana do cuidado
   -> evoluções e ocorrências
   -> revisão multidisciplinar
   -> resultados de saúde, autonomia, segurança e qualidade de vida
```

Cadastros, estoque, finanças, alimentação, escalas e doações existem para
sustentar essa jornada e garantir a continuidade do cuidado.

### 1.1 Natureza social e universitária

O SeniorCare é uma iniciativa de atuação da universidade junto à sociedade. Seu
público prioritário inicial são ILPIs beneficentes, filantrópicas ou sem fins
lucrativos, frequentemente submetidas a restrições de orçamento, equipe e
infraestrutura tecnológica.

O projeto deve ser tratado como tecnologia social construída com as
instituições, e não apenas entregue a elas. Isso implica:

- escuta e participação de residentes, trabalhadores e gestores;
- desenvolvimento orientado por necessidades reais da instituição-piloto;
- implantação gradual, acompanhada de formação e suporte;
- baixo custo total de adoção e manutenção;
- preferência por padrões abertos e redução de dependência de fornecedores;
- acessibilidade e adequação a diferentes níveis de maturidade digital;
- produção de conhecimento e formação prática para estudantes;
- avaliação do impacto social e assistencial produzido;
- continuidade do serviço além dos ciclos acadêmicos e trocas de turma.

A atuação universitária não concede acesso automático a dados reais. Ensino,
extensão, operação do sistema e pesquisa são finalidades distintas e devem ter
bases, autorizações, ambientes e controles próprios.

## 2. Posicionamento do produto

### 2.1 Definição

> O SeniorCare é uma plataforma de gestão integrada para casas de cuidados e
> instituições de longa permanência para pessoas idosas. O sistema organiza a
> jornada do residente, coordena o cuidado cotidiano e multidisciplinar,
> acompanha saúde e nutrição, preserva vínculos familiares e sociais e dá
> suporte à gestão financeira, operacional, regulatória e de doações da
> instituição.

### 2.2 Natureza do serviço apoiado

No contexto brasileiro, uma instituição residencial coletiva destinada a
pessoas com 60 anos ou mais, com ou sem suporte familiar, tende a se enquadrar
como ILPI. Ela não deve ser conceituada apenas como hospedagem e tampouco como
hospital. Trata-se de uma residência coletiva que articula moradia, assistência
social, cuidado cotidiano, promoção e coordenação da saúde, alimentação,
convivência, autonomia e proteção de direitos.

O SeniorCare deve preservar a característica de lar. A digitalização não pode
transformar a rotina em um processo impessoal ou excessivamente hospitalar.
Registros e controles devem aumentar a segurança e liberar tempo da equipe para
o cuidado humano.

### 2.3 Proposta de valor

Para a pessoa idosa, o SeniorCare deve favorecer cuidado personalizado,
autonomia possível, respeito às preferências, segurança e continuidade.

Para profissionais e cuidadores, deve oferecer uma visão compartilhada do plano
de cuidado, prioridades do turno, registros simples e comunicação segura.

Para familiares e responsáveis, deve oferecer transparência compatível com a
autonomia, a privacidade e as autorizações do residente.

Para gestores, deve integrar qualidade assistencial, capacidade operacional,
custos, receitas, estoque, pessoas, doações e conformidade.

Para doadores e órgãos de controle, deve permitir rastreabilidade e prestação de
contas dos recursos recebidos e aplicados.

Para a universidade, deve permitir extensão responsável, formação
interdisciplinar e produção de conhecimento sem subordinar o cuidado ou a
privacidade dos residentes aos objetivos acadêmicos.

## 3. Princípios orientadores

1. **Pessoa idosa no centro:** necessidades, preferências, história, metas e
   direitos orientam o plano de cuidado.
2. **Autonomia e dignidade:** dependência funcional não elimina poder decisório,
   privacidade ou identidade.
3. **Cuidado integral:** dimensões física, cognitiva, emocional, social,
   funcional, nutricional, cultural e espiritual são interdependentes.
4. **Plano único e execução coordenada:** as profissões colaboram em objetivos
   comuns sem apagar suas responsabilidades técnicas específicas.
5. **Característica de lar:** rotinas institucionais devem respeitar escolhas e
   evitar padronização desnecessária da vida do residente.
6. **Prevenção e segurança:** riscos devem ser identificados, acompanhados e
   reduzidos antes que se tornem danos.
7. **Vínculos e comunidade:** família, pessoas de referência e participação
   comunitária fazem parte do cuidado.
8. **Rastreabilidade proporcional:** ações críticas precisam de autoria, data,
   contexto e histórico, sem burocracia sem finalidade.
9. **Privacidade por padrão:** dados pessoais e de saúde são acessados somente
   por quem necessita deles para uma finalidade legítima.
10. **Transparência e sustentabilidade:** recursos financeiros, materiais e
    doações devem ser administrados com responsabilidade e prestação de contas.

## 4. Modelo conceitual do cuidado

```text
                         PESSOA IDOSA
                              |
             +----------------+----------------+
             |                |                |
       História e         Necessidades     Preferências,
        vínculos           de cuidado      autonomia e metas
             |                |                |
             +----------------+----------------+
                              v
                 Plano Individual de Cuidado
                              |
       +-----------+----------+----------+-------------+
       v           v          v          v             v
 Assistência     Saúde    Alimentação  Convivência   Família e
 cotidiana   multidisciplinar             e lazer    comunidade
       +-----------+----------+----------+-------------+
                              v
             Evolução, segurança e qualidade de vida
```

O SeniorCare deve trabalhar com três níveis complementares de planejamento:

1. **Plano institucional:** plano de trabalho e Plano de Atenção Integral à
   Saúde da instituição.
2. **Plano individual:** necessidades, riscos, objetivos, intervenções,
   responsáveis e critérios de revisão de cada residente.
3. **Plano operacional diário:** agenda de cuidados, refeições, medicamentos,
   atendimentos e atividades derivada do plano individual.

O modelo se inspira na abordagem ICOPE da Organização Mundial da Saúde:
triagem de perdas de capacidade, avaliação aprofundada, elaboração de plano
personalizado, encaminhamento e monitoramento, participação comunitária e apoio
a cuidadores.

## 5. Atores do sistema

O controle de acesso e as experiências de uso devem considerar, no mínimo:

- pessoa idosa residente;
- familiar, responsável legal ou curador;
- cuidador de pessoas idosas;
- enfermagem;
- medicina e geriatria;
- nutrição;
- fisioterapia;
- psicologia;
- terapia ocupacional;
- fonoaudiologia;
- serviço social;
- farmácia, odontologia e outros profissionais de saúde;
- responsável técnico da instituição;
- coordenação assistencial;
- administração e direção;
- financeiro e contabilidade;
- compras, almoxarifado e patrimônio;
- cozinha e serviço de alimentação;
- lavanderia, limpeza e manutenção;
- captação de recursos, doações e voluntariado;
- auditoria, fiscalização e órgãos de controle;
- administrador técnico da plataforma.

Uma mesma pessoa pode acumular papéis, mas as permissões e responsabilidades
devem permanecer explícitas.

## 6. Domínios funcionais

### 6.1 Jornada e cadastro longitudinal do residente

Abrange o período entre o primeiro contato e o encerramento da permanência:

- interesse, triagem, visita e lista de espera;
- critérios de elegibilidade e capacidade de atendimento;
- avaliação social, funcional, cognitiva, nutricional e clínica de admissão;
- classificação do grau de dependência;
- identificação civil e dados de contato;
- familiares, pessoas de referência, responsável legal e curador;
- contrato, serviços incluídos, preços, contribuições e autorizações;
- convênio, plano de saúde e referências públicas ou privadas;
- pertences, valores e documentos entregues à instituição;
- história de vida, preferências, hábitos, cultura, religião e diretivas;
- quarto, leito, mudanças de acomodação e histórico de ocupação;
- ausências, consultas externas, internações e retornos;
- transferência, desligamento, óbito e guarda documental.

O registro do residente é longitudinal e não deve perder a história quando o
residente muda de quarto, de grau de dependência ou de responsável.

### 6.2 Assistência cotidiana e atividades de vida diária

Organiza o trabalho dos cuidadores e da equipe em cada turno:

- higiene, banho, cuidados bucais e vestuário;
- alimentação e hidratação assistidas;
- mobilidade, marcha, transferências e posicionamento;
- sono, repouso e eliminações;
- mudança de decúbito e prevenção de lesões;
- uso de órteses, próteses e equipamentos de apoio;
- rotina individual, preferências e recusas;
- tarefas programadas, prioridade e confirmação da execução;
- passagem de plantão e pendências críticas;
- observações, alterações de comportamento e sinais de alerta;
- quedas, lesões, fugas, conflitos e outras ocorrências;
- comunicação e escalonamento para enfermagem ou responsável técnico.

O sistema deve distinguir claramente:

- cuidado planejado;
- cuidado executado;
- cuidado não realizado e seu motivo;
- recusa consciente do residente;
- observação ou evolução;
- intercorrência e incidente de segurança.

### 6.3 Saúde e cuidado multidisciplinar

Deve existir um prontuário longitudinal compartilhado com espaços profissionais
e responsabilidades bem definidos:

- avaliações periódicas por profissão;
- problemas, diagnósticos, condições crônicas e histórico clínico;
- alergias e reações adversas;
- riscos de queda, lesão por pressão, desnutrição, desidratação, suicídio e
  outros riscos definidos pela instituição;
- capacidade funcional, cognição, humor, comunicação, visão e audição;
- objetivos interdisciplinares e intervenções por profissão;
- prescrição, conciliação, dispensação e administração de medicamentos;
- checagem de horários, omissões, recusas e efeitos adversos;
- sinais vitais e parâmetros individualizados;
- vacinação e comprovantes;
- consultas, exames, procedimentos e laudos;
- encaminhamentos, remoções, internações e retornos hospitalares;
- evolução profissional e reuniões multidisciplinares;
- revisão periódica do plano individual;
- resumo assistencial para transições de cuidado;
- comunicação autorizada com familiares e responsáveis.

Registros de profissões regulamentadas devem preservar autoria, categoria
profissional, conselho, data, hora, correções e histórico.

#### 6.3.1 Prontuário multidisciplinar como capacidade crítica

O prontuário multidisciplinar é uma das capacidades de maior risco e valor do
SeniorCare. Ele deve ser projetado antes da implementação das telas clínicas e
não como uma coleção posterior de campos independentes.

O prontuário deve combinar continuidade e separação de responsabilidades:

- visão longitudinal compartilhada do residente;
- plano individual com objetivos comuns à equipe;
- registros próprios de cada profissão;
- anotações da equipe de enfermagem e checagem de cuidados;
- autoria inequívoca e identificação do conselho profissional, quando houver;
- controle de quem pode visualizar, produzir, corrigir ou assinar cada conteúdo;
- anexos classificados como natos digitais ou digitalizados;
- correções por adendo ou nova versão, sem sobrescrita silenciosa;
- preservação do conteúdo originalmente assinado;
- vínculo entre avaliação, decisão, intervenção e resultado;
- resumo para transições de cuidado sem eliminar o histórico completo;
- acesso do residente aos próprios dados nos limites legais aplicáveis;
- plano de contingência para indisponibilidade do sistema.

Uma evolução clínica, prescrição ou avaliação concluída deve se tornar um
registro imutável. Uma correção posterior deve indicar o registro corrigido,
autor, data, motivo e novo conteúdo. A trilha técnica de auditoria complementa,
mas não substitui, a assinatura profissional exigida para cada tipo de ato.

O prontuário não deve pressupor que todos os seus conteúdos são documentos
médicos. Ele reúne registros de medicina, enfermagem, nutrição, fisioterapia,
psicologia, serviço social e outras áreas, cada qual sujeita às competências e
normas do respectivo conselho profissional.

### 6.4 Alimentação e nutrição

Integra o cuidado nutricional ao planejamento e à operação da cozinha:

- avaliação nutricional e diagnóstico;
- peso, medidas e evolução antropométrica;
- necessidades energéticas e de hidratação;
- alergias, intolerâncias e restrições;
- dietas, consistências, suplementos e vias de alimentação;
- preferências e aspectos culturais;
- cardápios, ciclos de cardápio e fichas técnicas;
- planejamento de pelo menos seis refeições diárias;
- mapa de dietas e produção por refeição;
- distribuição, aceitação alimentar e ingestão hídrica;
- sobras, desperdícios e não conformidades;
- fornecedores, compras, lote, validade e armazenamento;
- boas práticas, limpeza, vetores e resíduos;
- integração entre cardápio, demanda prevista, compras e estoque.

### 6.5 Assistência social, família, convivência e autonomia

Abrange a vida social e comunitária do residente:

- estudo social e plano de acompanhamento;
- composição familiar e rede de apoio;
- vínculos, visitas, contatos e participação familiar;
- comunicação de mudanças relevantes conforme autorizações;
- situações de abandono, negligência ou violação de direitos;
- acesso a documentos, benefícios e serviços públicos;
- atividades culturais, educativas, físicas, recreativas e espirituais;
- calendário individual e coletivo de atividades;
- participação na comunidade e relações intergeracionais;
- preferências, escolhas e participação do residente nas decisões da casa;
- satisfação, qualidade de vida e sugestões;
- apoio no luto, adaptação e transições.

O portal familiar, se adotado, não deve pressupor acesso irrestrito ao prontuário.
Seu conteúdo depende da autonomia do residente, da representação legal, das
bases legais aplicáveis e da finalidade do compartilhamento.

### 6.6 Profissionais, escalas e educação permanente

- cadastro de trabalhadores, vínculos e funções;
- habilitação, conselho profissional e vencimentos documentais;
- competências, treinamentos e educação permanente em gerontologia;
- escalas, turnos, ausências, substituições e cobertura;
- dimensionamento por ocupação e grau de dependência dos residentes;
- alertas de cobertura insuficiente;
- alocação por unidade, setor e residente;
- passagem de plantão e ciência de procedimentos;
- saúde e segurança do trabalhador, quando incluídas na fase do produto.

Como referência federal mínima da RDC 502/2021, o dimensionamento de cuidadores
considera:

- grau I: um cuidador para cada 20 residentes ou fração, por oito horas diárias;
- grau II: um cuidador para cada 10 residentes ou fração, por turno;
- grau III: um cuidador para cada seis residentes ou fração, por turno.

A regulamentação também estabelece responsável técnico de nível superior, com
carga mínima de 20 horas semanais, e parâmetros para lazer, alimentação,
limpeza e lavanderia. Regras estaduais, municipais e profissionais podem impor
exigências adicionais.

### 6.7 Gestão financeira

Deve suportar modelos privados, públicos, filantrópicos e sem fins lucrativos,
conforme configuração:

- contratos, planos de serviço e reajustes;
- mensalidades, coparticipações e contribuições;
- descontos, bolsas, gratuidades e inadimplência;
- contas a receber e cobrança;
- contas a pagar e aprovações;
- fluxo de caixa e conciliação bancária;
- plano de contas e centros de custo;
- orçamento e acompanhamento do realizado;
- custos por residente, serviço, setor ou unidade;
- compras, fornecedores e compromissos;
- recursos públicos, convênios, subvenções e projetos;
- prestação de contas e relatórios gerenciais;
- integração contábil, bancária e fiscal quando definida.

O SeniorCare não deve substituir automaticamente um sistema contábil ou fiscal.
Pode funcionar como sistema operacional e financeiro integrado a soluções
especializadas.

### 6.8 Doações, captação e voluntariado

Doações devem ser tratadas como recursos rastreáveis, não apenas como uma receita
genérica:

- cadastro, histórico e preferências de relacionamento com doadores;
- campanhas, metas e finalidades;
- doações financeiras, recorrentes e prometidas;
- doações em espécie: alimentos, medicamentos, roupas, higiene e equipamentos;
- triagem, aceite, recusa e destinação de itens;
- quantidade, valor atribuído, lote, validade e estado de conservação;
- entrada integrada no estoque ou patrimônio;
- recursos livres ou vinculados a finalidade específica;
- recibos, agradecimentos e comprovantes;
- aplicação do recurso e vínculo com despesas ou entregas;
- prestação de contas ao doador, aos financiadores e à sociedade;
- termos, disponibilidade, competências e atividades de voluntários;
- mensuração e registro do trabalho voluntário quando aplicável.

Doações de medicamentos ou alimentos não devem entrar automaticamente em uso:
precisam respeitar critérios sanitários, prescrição, lote, validade, condições de
armazenamento e política institucional.

### 6.9 Compras, estoque, patrimônio e operação da casa

Expande o SeniorStock Manager para atender a operação real da instituição:

- catálogo, grupos, tipos, unidades, fabricantes e fornecedores;
- solicitação, cotação, aprovação e pedido de compra;
- recebimento, inspeção e divergências;
- lote, validade, localização e condições de armazenamento;
- estoque de alimentos, medicamentos, higiene, limpeza, roupas e materiais;
- dispensação e consumo por setor ou residente quando pertinente;
- inventário, perdas, vencimentos, descarte e rastreabilidade;
- níveis mínimo e máximo, previsão e reposição;
- equipamentos de apoio, bens patrimoniais e manutenção;
- quartos, leitos e ambientes;
- lavanderia, enxoval e identificação de roupas pessoais;
- limpeza, rotinas, produtos e inspeções;
- manutenção preventiva e corretiva da infraestrutura;
- fornecedores e contratos de serviços terceirizados.

### 6.10 Governança, qualidade, segurança e conformidade

- cadastro e vencimento de alvarás, registros, contratos e licenças;
- inscrição do programa nos conselhos competentes;
- plano de trabalho institucional;
- Plano de Atenção Integral à Saúde;
- procedimentos operacionais, aprovação e controle de versões;
- inspeções, não conformidades, responsáveis e planos de ação;
- indicadores obrigatórios e indicadores internos;
- notificações sanitárias, epidemiológicas e de proteção de direitos;
- eventos sentinela, incidentes e investigação de causa;
- auditoria de acesso e alteração dos registros;
- assinatura e validação de documentos;
- relatórios para fiscalização e prestação de contas;
- gestão de riscos e continuidade operacional.

Indicadores federais mínimos a serem consolidados incluem:

- mortalidade;
- incidência de doença diarreica aguda;
- incidência de escabiose;
- incidência de desidratação;
- prevalência de lesão por pressão;
- prevalência de desnutrição.

Queda com lesão e tentativa de suicídio são eventos sentinela previstos na RDC
502/2021. A instituição também deve tratar notificações compulsórias de doenças,
suspeitas de violência e outras obrigações aplicáveis.

### 6.11 Dashboards estatísticos e inteligência institucional

O SeniorCare deve prever um módulo de dashboards estatísticos capaz de
transformar os registros dos demais domínios em informação compreensível e
acionável. O módulo não será uma fonte paralela de dados: seus indicadores devem
ser derivados dos registros operacionais e manter rastreabilidade até a origem.

Os dashboards devem atender a três horizontes:

1. **Operacional:** situação do dia, pendências, riscos e capacidade da equipe.
2. **Tático:** tendências por período, setor, turno, perfil de dependência ou
   programa, apoiando coordenações e planos de melhoria.
3. **Estratégico e regulatório:** sustentabilidade, qualidade institucional,
   indicadores obrigatórios e prestação de contas.

#### Painéis previstos

**Residentes e assistência cotidiana**

- ocupação, vagas, admissões, desligamentos e permanência média;
- distribuição por idade, sexo e grau de dependência;
- planos individuais vigentes, vencidos ou próximos da revisão;
- cuidados programados, realizados, atrasados, recusados ou não realizados;
- ocorrências por tipo, local, turno e recorrência;
- hospitalizações, remoções e retornos;
- participação em atividades e vínculos familiares, quando mensuráveis de forma
  ética e significativa.

**Saúde e qualidade assistencial**

- quedas e quedas com lesão;
- lesões por pressão;
- desnutrição, perda de peso e risco nutricional;
- desidratação;
- vacinação e acompanhamentos pendentes;
- uso e omissões de medicamentos;
- eventos adversos, intercorrências e tempo de resposta;
- consultas, exames e encaminhamentos pendentes;
- evolução dos objetivos do plano individual;
- indicadores sanitários exigidos pela regulamentação aplicável.

**Alimentação e nutrição**

- aceitação das refeições e ingestão hídrica;
- residentes por dieta, consistência ou restrição;
- evolução antropométrica e alertas nutricionais;
- produção, sobras e desperdício;
- custo estimado das refeições;
- insumos críticos ou próximos do vencimento.

**Pessoas e escalas**

- cobertura planejada e realizada por turno;
- relação entre cuidadores e residentes por grau de dependência;
- ausências, substituições, horas extras e sobrecarga;
- habilitações e registros profissionais próximos do vencimento;
- treinamentos obrigatórios e educação permanente;
- distribuição da carga de trabalho, sem uso punitivo ou descontextualizado.

**Financeiro e sustentabilidade**

- receitas, despesas, saldo e fluxo de caixa;
- contas a pagar e receber e inadimplência;
- orçamento previsto e realizado;
- custo por residente, unidade, setor ou serviço;
- composição das fontes de recursos;
- dependência de mensalidades, convênios, recursos públicos e doações;
- projeções de curto prazo com premissas explícitas.

**Doações e impacto social**

- doações financeiras e materiais por período, campanha e finalidade;
- doadores ativos, recorrência e retenção;
- recursos livres, vinculados, aplicados e ainda disponíveis;
- itens recebidos, destinados, consumidos, recusados ou descartados;
- valor e alcance dos projetos apoiados;
- prestação de contas para doadores, universidade e sociedade.

**Estoque e operação**

- estoque atual, cobertura estimada e itens abaixo do mínimo;
- consumo por categoria, setor ou período;
- lotes próximos do vencimento, perdas e descartes;
- compras emergenciais e tempo de reposição;
- manutenção preventiva pendente e indisponibilidade de equipamentos;
- uso e giro de bens doados.

**Conformidade e governança**

- licenças, alvarás, contratos e documentos próximos do vencimento;
- inspeções, não conformidades e planos de ação;
- procedimentos institucionais pendentes de revisão;
- notificações e indicadores obrigatórios pendentes de consolidação ou envio;
- situação do Plano de Atenção Integral à Saúde e do plano de trabalho;
- incidentes de segurança e privacidade em acompanhamento.

#### Requisitos de confiança estatística

Cada indicador deve possuir um dicionário versionado contendo:

- nome e finalidade;
- definição do numerador e denominador;
- população incluída e excluída;
- unidade, período de referência e frequência de atualização;
- fonte dos dados e responsável pela qualidade;
- fórmula e regras de arredondamento;
- dimensões permitidas para filtro e comparação;
- data da última atualização;
- limitações de interpretação e versão da definição.

O módulo deve ainda:

- diferenciar contagem, proporção, taxa, prevalência, incidência, média e mediana;
- permitir acompanhar tendências sem apresentar correlação como causalidade;
- exibir o denominador e o período junto ao resultado;
- sinalizar dados incompletos, atrasados ou inconsistentes;
- oferecer filtros coerentes entre os painéis;
- permitir detalhamento autorizado até os registros de origem;
- registrar exportações e acessos a informações sensíveis;
- exportar visões autorizadas em formatos legíveis e estruturados;
- preservar a definição usada em relatórios históricos;
- permitir metas e faixas de referência com origem e responsável explícitos.

#### Privacidade, ética e prevenção de uso indevido

- Perfis assistenciais, administrativos, acadêmicos e públicos devem ter visões
  diferentes.
- Painéis agregados não devem permitir reidentificação por combinações de
  filtros ou grupos muito pequenos.
- Dados destinados à gestão da ILPI não podem ser reutilizados automaticamente
  para pesquisa acadêmica.
- Rankings individuais de trabalhadores ou residentes não devem ser adotados
  sem finalidade legítima, contexto e avaliação de riscos.
- Indicadores não devem substituir avaliação clínica, escuta do residente ou
  análise qualitativa.
- Painéis públicos e de prestação de contas devem usar dados agregados,
  minimizados e aprovados pela governança institucional.

#### Direção arquitetural

O módulo analítico deve evoluir para uma camada de leitura separada da operação
transacional. Atualizações podem ocorrer por eventos, processamento incremental
ou cargas programadas, com reconciliação e monitoramento de falhas.

Essa camada deve preservar segregação entre instituições, histórico temporal,
proveniência e qualidade dos dados. Consultas estatísticas pesadas não devem
comprometer o registro de cuidados, medicamentos ou ocorrências. O desenho
detalhado da camada analítica deverá ser definido em uma mudança OpenSpec
própria antes da implementação.

## 7. Fluxos integrados prioritários

### 7.1 Admissão e início do cuidado

```text
Interesse/encaminhamento
  -> triagem e capacidade da casa
  -> avaliações de admissão
  -> contrato e documentos
  -> classificação de dependência e riscos
  -> plano individual inicial
  -> quarto/leito e acolhimento
  -> agenda de cuidados e revisão programada
```

### 7.2 Ciclo contínuo do cuidado

```text
Avaliar
  -> definir objetivos e intervenções
  -> programar cuidados
  -> executar e registrar
  -> observar resultados e ocorrências
  -> discutir em equipe
  -> revisar o plano com o residente
```

### 7.3 Intercorrência de saúde

```text
Sinal de alerta ou ocorrência
  -> avaliação e ação imediata
  -> acionamento do responsável técnico
  -> encaminhamento/remoção quando necessário
  -> comunicação autorizada à família
  -> registro, notificação e investigação
  -> atualização do plano de cuidado
```

### 7.4 Alimentação integrada

```text
Avaliação nutricional + preferências
  -> prescrição e mapa de dietas
  -> cardápio e previsão de consumo
  -> compras e estoque
  -> produção e distribuição
  -> registro de aceitação/hidratação
  -> reavaliação nutricional
```

### 7.5 Doação financeira

```text
Doador/campanha
  -> recebimento e recibo
  -> classificação livre ou vinculada
  -> aplicação em despesa/projeto
  -> conciliação e contabilização
  -> prestação de contas
```

### 7.6 Doação de materiais

```text
Oferta
  -> triagem sanitária e operacional
  -> aceite e avaliação
  -> recibo
  -> entrada em estoque/patrimônio
  -> destinação e consumo
  -> prestação de contas
```

## 8. Requisitos transversais

### 8.1 Segurança e privacidade

- autenticação individual e, para perfis críticos, fator adicional;
- autorização por papel, função, unidade e contexto assistencial;
- princípio do menor privilégio;
- separação entre dados assistenciais, sociais e administrativos;
- trilha de auditoria para leitura e alteração de dados sensíveis;
- histórico de correções sem apagamento silencioso;
- criptografia em trânsito e em repouso conforme risco;
- gestão de sessão e dispositivos;
- consentimentos e bases legais associados às finalidades;
- política de retenção, descarte e exportação de dados;
- resposta e comunicação de incidentes de segurança;
- cópias de segurança, restauração testada e continuidade do serviço.

Dados de saúde, religião, biometria e outras informações íntimas são dados
pessoais sensíveis. A instituição deve definir seu papel como agente de
tratamento, suas bases legais e as condições de compartilhamento. Consentimento
não deve ser usado como justificativa genérica quando outra base legal é a
adequada.

### 8.2 Usabilidade e acessibilidade

- interface compatível com diferentes níveis de familiaridade digital;
- operação rápida em celular ou tablet durante o cuidado;
- alto contraste, redimensionamento e navegação por teclado;
- mensagens claras e prevenção de erros;
- identificação visual de prioridades sem depender apenas de cor;
- formulários progressivos e reutilização segura de informações;
- suporte a interrupções e retomada de tarefas;
- acessibilidade alinhada às diretrizes vigentes aplicáveis.

### 8.3 Rastreabilidade e integridade

- data e hora confiáveis;
- identificação do autor e do papel exercido;
- estado do registro: rascunho, concluído, corrigido ou cancelado;
- motivo de correções e cancelamentos;
- versionamento de planos, prescrições, dietas e procedimentos;
- vínculo entre planejamento, execução e resultado;
- exportação legível para transições de cuidado e fiscalização.

### 8.4 Interoperabilidade

Devem ser previstos pontos de integração, sem assumir que todos estarão na
primeira versão:

- contabilidade e bancos;
- folha e gestão de pessoas;
- laboratórios e prestadores de saúde;
- prontuários e redes públicas ou privadas;
- emissão de documentos e assinatura eletrônica;
- plataformas de pagamento e doação;
- dispositivos clínicos e de chamada;
- padrões nacionais de interoperabilidade em saúde quando aplicáveis.

### 8.5 Configuração institucional

O produto deve evitar regras rígidas onde a regulamentação permite variações.
Devem ser configuráveis:

- unidades e capacidade;
- modelo jurídico e financeiro;
- serviços oferecidos;
- perfis e responsabilidades;
- instrumentos de avaliação;
- periodicidades e alertas;
- fluxos de aprovação;
- regras locais adicionais;
- documentos e modelos institucionais.

### 8.6 Assinaturas eletrônicas e redução de papel

A redução de papel é uma direção estratégica futura. O objetivo não é apenas
substituir uma folha por um PDF, mas manter documentos eletrônicos autênticos,
íntegros, confidenciais, verificáveis e preservados pelo período necessário.

O SeniorCare deve diferenciar:

1. **Autenticação:** comprova quem entrou no sistema.
2. **Autoria do registro:** associa uma ação ao usuário autenticado e à trilha de
   auditoria.
3. **Assinatura eletrônica:** associa o signatário ao conteúdo e permite detectar
   alterações conforme o nível adotado.
4. **Assinatura eletrônica qualificada:** usa certificado digital ICP-Brasil.
5. **Digitalização:** converte documento físico existente em documento digital
   mediante processo controlado; não é equivalente a produzir um documento nato
   digital.

#### Certificado Digital do CFM

O Conselho Federal de Medicina oferece a médicos elegíveis um certificado
digital em nuvem, de pessoa física, no padrão ICP-Brasil. Segundo as informações
vigentes do CFM, a solicitação depende de inscrição regular e adimplente,
Cédula de Identidade Médica, biometria compatível e disponibilidade do serviço
no CRM. O certificado é operado pelo médico por aplicativo e pode ser usado em
prontuário eletrônico, prescrições, relatórios, exames e outros documentos.

O certificado pertence e permanece sob controle do médico. O SeniorCare:

- não deve armazenar chave privada, senha, código temporário ou segredo do
  certificado;
- deve aceitar certificados ICP-Brasil válidos, sem limitar o médico ao
  certificado emitido pelo CFM;
- deve validar identidade, cadeia, validade e situação do certificado;
- deve preservar o documento exato que foi assinado e as evidências necessárias
  para sua verificação futura;
- deve permitir verificação por ferramenta independente, como o verificador do
  Instituto Nacional de Tecnologia da Informação;
- não deve confundir a plataforma gratuita de Prescrição Eletrônica do CFM com
  uma integração automática de prontuário;
- deve avaliar tecnicamente os meios de assinatura local, em nuvem ou por
  provedor compatível antes de escolher uma integração.

A disponibilidade e a gratuidade do certificado são condições externas ao
SeniorCare e podem mudar. Elas reduzem uma barreira de adoção para ILPIs
beneficentes, mas não podem ser a única estratégia de assinatura do produto.

#### Demais signatários

- Profissionais de enfermagem e das demais áreas devem assinar conforme a
  legislação e as normas do respectivo conselho.
- Residentes, familiares e representantes podem usar assinatura avançada
  Gov.br, assinatura qualificada ou outro mecanismo legalmente aceito, conforme
  o risco, o tipo de documento e a concordância das partes.
- Aceites operacionais simples, ciência de comunicação, consentimentos,
  contratos, prescrições e evoluções profissionais não devem receber
  automaticamente o mesmo fluxo de assinatura.

#### Estratégia de implantação

```text
Etapa 1 - Registro eletrônico confiável
  identidade, papéis, auditoria, versionamento e exportação

Etapa 2 - Assinatura externa controlada
  PDF estável -> assinatura ICP-Brasil fora do SeniorCare -> validação e guarda

Etapa 3 - Assinatura integrada
  assinatura avançada/qualificada dentro do fluxo, sem custodiar chaves privadas

Etapa 4 - Operação sem papel
  requisitos de S-RES/NGS2, preservação, contingência, comissões e validação formal
```

A eliminação do papel não deve ocorrer na primeira etapa. A Lei nº 13.787/2018
exige integridade, autenticidade e confidencialidade na digitalização, prevê
análise por comissão permanente antes da destruição dos originais e estabelece
prazo mínimo de 20 anos a partir do último registro antes da possível eliminação
dos prontuários, ressalvadas regras diferenciadas.

Antes de declarar o SeniorCare como sistema sem papel, o projeto deverá realizar
uma especificação própria e avaliar, no mínimo:

- enquadramento do sistema como S-RES;
- requisitos de segurança NGS2 e certificação SBIS aplicável;
- normas atualizadas do CFM, Cofen e demais conselhos;
- assinatura de longo prazo, carimbo do tempo e validação após expiração ou
  revogação do certificado;
- política de guarda, migração de formatos e restauração;
- comissão de revisão de prontuários e avaliação documental;
- contingência, recuperação de desastre e acesso durante indisponibilidade;
- convivência e transição entre acervo físico, digitalizado e nato digital.

### 8.7 Governança de dados na parceria universitária

Por lidar com residentes vulneráveis e dados pessoais sensíveis, a parceria de
extensão deve ter governança explícita:

- a ILPI e a universidade devem definir contratualmente seus papéis como
  controladora, operadora ou eventual controladoria conjunta;
- produção, homologação, ensino e pesquisa devem usar ambientes separados;
- estudantes não devem ter acesso a dados reais por padrão;
- desenvolvimento e aulas devem usar dados sintéticos ou anonimizados sempre que
  possível;
- acessos excepcionais a produção precisam de necessidade, autorização,
  supervisão, prazo, auditoria e compromisso de confidencialidade;
- dados coletados para prestar o serviço não podem ser reutilizados
  automaticamente em pesquisa;
- projetos de pesquisa devem observar a LGPD, a governança institucional e a
  análise ética aplicável pelo sistema CEP/Conep;
- resultados acadêmicos não devem permitir reidentificação de residentes,
  familiares, trabalhadores ou doadores;
- o acordo deve definir propriedade intelectual, licenciamento, suporte,
  resposta a incidentes, continuidade e saída da parceria.

## 9. Requisitos regulatórios de referência

O escopo foi fundamentado nas seguintes referências, que devem ser validadas com
o responsável técnico, assessoria jurídica e contabilidade da instituição:

### 9.1 Estatuto da Pessoa Idosa

A Lei nº 10.741/2003 estabelece, entre outros pontos:

- direitos à vida, saúde, alimentação, dignidade e convivência;
- preservação de vínculos familiares e identidade;
- atendimento personalizado;
- contrato escrito de prestação de serviços;
- cuidados de saúde conforme a necessidade;
- estudo social e pessoal;
- atividades educacionais, culturais, esportivas e de lazer;
- assistência religiosa conforme a vontade do residente;
- arquivo individual de atendimento, responsáveis, parentes, pertences e
  contribuições;
- publicidade da prestação de contas dos recursos recebidos;
- fiscalização por conselhos, Ministério Público, Vigilância Sanitária e demais
  órgãos competentes.

### 9.2 RDC Anvisa nº 502/2021

Estabelece padrões mínimos para o funcionamento de ILPIs, incluindo:

- alvará sanitário e constituição regular;
- responsável técnico;
- contrato formal e documentação acessível à fiscalização;
- dimensionamento de recursos humanos por grau de dependência;
- educação permanente em gerontologia;
- requisitos de infraestrutura e acessibilidade;
- plano de trabalho com participação dos residentes;
- registro atualizado de cada residente;
- Plano de Atenção Integral à Saúde a cada dois anos e avaliação anual;
- vacinação, medicamentos, procedimentos de cuidado e remoção;
- pelo menos seis refeições diárias e boas práticas de alimentação;
- rotinas de lavanderia, limpeza e resíduos;
- eventos sentinela, notificações e indicadores anuais.

### 9.3 Assistência social e ICOPE

A Tipificação Nacional de Serviços Socioassistenciais reforça proteção integral,
característica domiciliar, convivência familiar e comunitária e desenvolvimento
da autonomia. A abordagem ICOPE da OMS complementa o contexto com cuidado
integrado e centrado na pessoa, avaliação da capacidade intrínseca, plano
personalizado, monitoramento, comunidade e apoio a cuidadores.

### 9.4 Contabilidade de entidades sem fins lucrativos

Quando a instituição não possuir finalidade lucrativa, o tratamento de doações,
subvenções, trabalho voluntário, demonstrações contábeis e renúncia fiscal deve
observar as normas contábeis aplicáveis, incluindo a ITG 2002 e orientações do
Conselho Federal de Contabilidade.

### 9.5 Regras complementares

A regulamentação federal é uma base mínima. Estados, municípios, conselhos
profissionais, contratos, convênios e o modelo jurídico da instituição podem
criar exigências adicionais. Este documento define contexto de produto e não
substitui avaliação jurídica, sanitária, clínica ou contábil.

### 9.6 Prontuário e documentos eletrônicos

Além da LGPD, devem orientar as especificações futuras:

- Lei nº 13.787/2018, sobre digitalização, guarda, armazenamento e manuseio de
  prontuários;
- Lei nº 14.063/2020 e a regulamentação das assinaturas eletrônicas;
- Resolução CFM nº 2.299/2021, sobre documentos médicos eletrônicos, e normas
  posteriores aplicáveis;
- requisitos vigentes de Sistemas de Registro Eletrônico em Saúde e do nível de
  garantia de segurança NGS2;
- Resoluções Cofen nº 736/2024 e nº 754/2024 para registros e prontuário
  eletrônico no âmbito da enfermagem;
- normas específicas dos demais conselhos profissionais representados no
  prontuário.

## 10. Limites do produto

O SeniorCare:

- apoia a gestão e a documentação do cuidado, mas não decide condutas clínicas;
- não substitui julgamento profissional nem o responsável técnico;
- não substitui automaticamente sistemas contábeis, fiscais ou de folha;
- não é um serviço de emergência ou de telemedicina por definição;
- não deve diagnosticar ou prescrever sem profissional e enquadramento legal;
- não garante conformidade apenas pela existência de uma tela ou relatório;
- não autoriza compartilhamento irrestrito de dados com familiares;
- não transforma toda ILPI em estabelecimento hospitalar;
- não cobre inicialmente todas as integrações externas possíveis.

Funcionalidades de suporte à decisão ou inteligência artificial deverão ter
escopo, evidências, supervisão humana, riscos e responsabilidades definidos em
especificações próprias.

## 11. Escopo sugerido por evolução

A divisão abaixo é uma orientação inicial, sujeita às decisões em aberto.

### Fundação

- identidade, autenticação, papéis e auditoria;
- estrutura institucional, unidades, quartos e leitos;
- residentes, responsáveis, contratos e documentos;
- grau de dependência, riscos e avaliações iniciais;
- plano individual básico;
- profissionais e escalas;
- agenda e registro dos cuidados cotidianos;
- ocorrências, passagem de plantão e comunicação crítica;
- dicionário inicial de indicadores e painel operacional básico de pendências,
  ocupação, riscos e cobertura do turno.

Nesta etapa, o prontuário deve nascer com identidade, versionamento e auditoria
compatíveis com sua evolução futura, mesmo que a assinatura qualificada e a
operação sem papel ainda não estejam disponíveis.

### Cuidado integrado

- prontuário multidisciplinar;
- medicamentos, vacinação, consultas e exames;
- nutrição, dietas, cardápios, aceitação e hidratação;
- atividades, família, assistência social e qualidade de vida;
- revisão multidisciplinar e transições de cuidado;
- indicadores assistenciais e notificações;
- dashboards de saúde, cuidado, nutrição e qualidade assistencial.

### Gestão sustentável

- financeiro, contratos, contribuições e centros de custo;
- compras, estoque por lote e validade e patrimônio;
- doações financeiras e materiais;
- campanhas, voluntariado e prestação de contas;
- documentos regulatórios, inspeções e planos de ação;
- dashboards financeiros, de doações, estoque, pessoas e conformidade.

### Expansão

- portal do residente e da família (distinto do Senior Portal interno já
  implementado — ver seção 12.3);
- múltiplas instituições e unidades;
- integrações contábeis, bancárias e de saúde;
- análises preditivas e apoio à decisão com governança;
- aplicativos móveis e operação resiliente a indisponibilidade de rede;
- painéis comparativos de qualidade e sustentabilidade;
- camada analítica histórica, análises avançadas e comparações autorizadas;
- assinatura eletrônica integrada e evolução controlada para redução de papel.

## 12. Relação com o software existente

O repositório atual contém uma API compartilhada, um front-end assistencial e um
front-end de estoque. Seus cadastros de planos de saúde, cargos, religiões,
fornecedores, fabricantes, transportadoras, grupos, tipos e unidades de medida
são capacidades de apoio válidas.

Entretanto, o estado atual ainda não representa o novo núcleo do produto. A
evolução deve deslocar a arquitetura funcional de cadastros isolados para a
jornada integrada:

```text
Estado atual predominante           Estado-alvo
-------------------------           -----------
Cadastros administrativos    ->     Residente longitudinal
CRUDs independentes          ->     Fluxos de cuidado integrados
Interfaces por entidade      ->     Trabalho por turno e por objetivo
Estoque genérico             ->     Suprimentos ligados ao cuidado
Login visual/incompleto      ->     Identidade, papéis e auditoria
```

Cada evolução relevante deverá ser formalizada em uma mudança OpenSpec própria,
com requisitos, cenários, desenho, migração e tarefas verificáveis. Este
documento fornece o contexto; ele não autoriza implementar todos os módulos de
uma só vez.

### 12.1 Avaliação formal do estado atual

A implementação foi avaliada em 6 de agosto de 2026 contra os domínios, fluxos e
requisitos transversais deste escopo. O relatório completo, incluindo método,
evidências, riscos e sequência recomendada de evolução, está em
[`relatorio-avaliacao-requisitos-implementacao.md`](relatorio-avaliacao-requisitos-implementacao.md).

Na granularidade dos 11 domínios funcionais da seção 6, a baseline avaliada
apresenta:

| Situação | Domínios | Resultado |
|---|---|---:|
| Implementado | nenhum | 0 de 11 |
| Parcialmente implementado | profissionais; estoque e operação; governança e conformidade | 3 de 11 |
| Não implementado | residente; assistência cotidiana; saúde multidisciplinar; alimentação; assistência social; financeiro; doações; dashboards | 8 de 11 |

A classificação é feita por domínio, sem ponderação por tamanho, risco ou esforço.
Um domínio somente é considerado implementado quando seu fluxo essencial existe
de ponta a ponta, com persistência, API, interface quando aplicável e evidência
mínima de validação. Catálogos auxiliares, enums, protótipos e documentação não
equivalem à implementação do fluxo principal.

As capacidades executáveis encontradas concentram-se em:

- CRUDs de planos de saúde, cargos e religiões no front-end assistencial;
- catálogos de fornecedores, fabricantes, transportadoras, grupos, tipos e
  unidades de medida no estoque;
- API ASP.NET Core com nove entidades persistidas em PostgreSQL;
- componentes React reutilizáveis e recursos iniciais de contraste e tamanho de
  fonte;
- CI/CD, verificações de segurança, contêineres, health checks e backup.

Não foram encontradas implementações do residente longitudinal, assistência por
turno, prontuário multidisciplinar, medicamentos, nutrição, gestão financeira,
doações, conformidade operacional, indicadores ou dashboards. O cadastro visual
de produto do front-end de estoque não possui entidade ou endpoint correspondente
no backend.

### 12.2 Restrições de prontidão decorrentes da avaliação

Enquanto as lacunas críticas permanecerem, o sistema:

- não deve ser tratado como núcleo completo de gestão de ILPI;
- não deve receber prontuários ou dados reais de saúde em operação assistencial;
- não deve ser declarado apto à eliminação de papel ou assinatura eletrônica;
- não deve publicar dashboards como indicadores institucionais confiáveis;
- deve usar dados sintéticos para desenvolvimento, ensino e demonstração.

A baseline técnica também registrou que:

- o front-end de estoque passa em lint e build de produção;
- o front-end assistencial passa no lint, mas falha na compilação TypeScript;
- não existem testes automatizados nos três componentes;
- o backend não pôde ser compilado no ambiente da avaliação por ausência do SDK
  `dotnet`, permanecendo tecnicamente não verificado nessa rodada;
- não há mudança OpenSpec ativa para a fundação do novo núcleo do produto.

Antes do primeiro MVP, a prioridade é estabilizar os builds e estabelecer
instituição, identidade, autenticação, papéis, profissionais, residente,
autorização, auditoria, versionamento e testes. Prontuário, dashboards e
assinatura eletrônica devem evoluir sobre essa fundação, nessa ordem de
dependência.

### 12.3 Senior Portal interno vs. portal futuro de residentes e famílias

A partir da mudança OpenSpec `introduce-senior-portal`, o repositório passou a
ter uma terceira aplicação front-end (`SeniorPortal-Frontend/`) chamada
**Senior Portal**. É importante não confundir esse produto com o "portal do
residente e da família" já previsto na seção 11 ("Expansão") — são dois
produtos com público, propósito e modelo de acesso distintos:

| | Senior Portal (interno, implementado) | Portal do residente/família (futuro, não implementado) |
|---|---|---|
| Público | equipe interna já autenticada (assistência, estoque, administração) | residentes, familiares, responsáveis legais |
| Propósito | ponto de entrada único e catálogo de módulos internos (substitui logins/landing pages separados de cada front-end) | acompanhamento e comunicação autorizada com a instituição |
| Sessão/autenticação | reusa a mesma sessão institucional (cookie `HttpOnly`) já usada pelos módulos assistencial e de estoque — nenhum mecanismo novo | modelo de acesso próprio, ainda a ser desenhado (provavelmente fora da sessão de staff) |
| Dados expostos | nenhum dado clínico ou financeiro — só nome/descrição de módulos, estado operacional e navegação (spec.md "Senior Portal", `docs/architecture/senior-portal-contracts.md`) | subconjunto do prontuário/relacionamento autorizado pelo residente ou responsável, nos limites legais aplicáveis (ver seção 6.5) |
| Módulos hoje cobertos | assistência (`/care`) e estoque (`/stock`); outros módulos futuros (financeiro, doações, dashboards — seção 11) entram no mesmo catálogo à medida que forem implementados, sem aparecer antes disso (spec.md "catálogo operacional") | nenhum — depende de especificação própria |
| Documentação | `openspec/specs/senior-portal/spec.md`, `docs/architecture/senior-portal-contracts.md` | nenhuma ainda — depende de mudança OpenSpec própria, como todo domínio novo (seção 12) |

Módulos futuros do catálogo do Senior Portal (financeiro, doações, dashboards
etc.) seguem a mesma regra da seção 12: cada um precisa da sua própria mudança
OpenSpec antes da implementação. Até lá, eles não aparecem no catálogo
operacional — o catálogo só lista módulos com `InstitutionModule` habilitado
para a instituição (nunca um módulo "planejado" ou inexistente, spec.md
"Descoberta de módulos usa permissões efetivas").

## 13. Decisões em aberto

Duas premissas já estão estabelecidas:

- o público prioritário inicial são ILPIs beneficentes e sem fins lucrativos;
- o SeniorCare é uma iniciativa de extensão e atuação da universidade junto à
  sociedade.

As seguintes respostas ainda são necessárias antes de definir o primeiro
produto mínimo:

1. Qual é a natureza jurídica e o perfil operacional da ILPI-piloto?
2. O produto atenderá inicialmente uma única casa ou nascerá preparado para
   múltiplas instituições e unidades?
3. Quais graus de dependência são aceitos pela instituição-piloto?
4. A equipe de saúde é própria, terceirizada ou predominantemente externa?
5. Quem exerce a responsabilidade técnica e quais registros precisam de
   assinatura em cada profissão?
6. Qual é o modelo de receita: mensalidades, contribuições, convênios, recursos
   públicos, doações ou combinação?
7. Qual parte do financeiro ficará no SeniorCare e qual será integrada à
   contabilidade?
8. Familiares terão portal? Quais informações poderão consultar e com qual base?
9. A operação precisa funcionar em celular, tablet ou sem conexão contínua?
10. Quais instrumentos de avaliação já são utilizados pela casa?
11. Quais regras estaduais e municipais se aplicam à primeira implantação?
12. Quais integrações existentes são indispensáveis no início?
13. Qual é o modelo de governança, financiamento e suporte do software após cada
    ciclo acadêmico?
14. O SeniorCare será software aberto? Como serão tratados propriedade
    intelectual, contribuições externas e sustentabilidade?
15. Qual estratégia de assinatura será usada primeiro: exportação e assinatura
    externa ou integração dentro do produto?
16. Quais indicadores são prioritários para a ILPI-piloto e quais já possuem
    dados confiáveis para cálculo?
17. Quais dashboards serão exclusivamente internos, compartilhados com a
    universidade ou publicados na prestação de contas?

## 14. Critérios de sucesso do produto

Os resultados do SeniorCare devem ser avaliados por benefícios, não apenas por
quantidade de telas:

- maior proporção de cuidados planejados executados no prazo;
- redução de omissões de medicamentos e cuidados críticos;
- identificação e resposta mais rápidas a riscos e intercorrências;
- planos individuais atualizados e usados pela equipe;
- melhor continuidade entre turnos, profissões e serviços externos;
- maior participação do residente e preservação de suas preferências;
- comunicação familiar mais clara e segura;
- redução de perdas, vencimentos e compras emergenciais;
- visibilidade de custos e sustentabilidade financeira;
- rastreabilidade das doações e qualidade da prestação de contas;
- redução do esforço para inspeções e consolidação de indicadores;
- acesso rápido a indicadores confiáveis, com redução de planilhas paralelas;
- uso documentado dos dashboards em decisões e planos de melhoria;
- satisfação de residentes, familiares e trabalhadores.

Metas quantitativas deverão ser definidas a partir de uma linha de base da
instituição-piloto.

## 15. Glossário inicial

- **ILPI:** Instituição de Longa Permanência para Pessoas Idosas, de caráter
  residencial.
- **Residente:** pessoa idosa que vive na instituição; termo preferencial no
  produto quando adequado ao contexto.
- **Pessoa idosa:** pessoa com idade igual ou superior a 60 anos no marco legal
  brasileiro citado.
- **AVD:** atividade de vida diária, como alimentação, higiene e mobilidade.
- **Grau de dependência:** classificação da necessidade de auxílio para AVDs e
  condição cognitiva conforme a regulamentação aplicável.
- **RT:** responsável técnico da instituição.
- **Plano institucional:** planejamento geral da instituição e de sua atenção à
  saúde.
- **Plano individual de cuidado:** objetivos e intervenções personalizados,
  construídos com participação do residente e da equipe.
- **Prontuário longitudinal:** conjunto organizado de informações produzidas ao
  longo da permanência e das transições de cuidado.
- **Evento sentinela:** ocorrência relevante que exige resposta e notificação
  conforme regras aplicáveis.
- **Doação vinculada:** recurso que só pode ser aplicado na finalidade acordada.
- **Indicador:** medida definida por fórmula, população, período, fonte e
  finalidade, usada para acompanhar uma condição ou resultado.
- **Dashboard:** conjunto de indicadores e visualizações organizado para um
  público e uma decisão específicos.
- **Senior Portal:** aplicação interna que serve como ponto de entrada único e
  catálogo de módulos para a equipe já autenticada — distinto do portal futuro
  de residentes e famílias (seção 12.3).

## 16. Referências

- Agência Nacional de Vigilância Sanitária. [Instituições de Longa Permanência
  para Idosos](https://www.gov.br/anvisa/pt-br/assuntos/servicosdesaude/saloes-tatuagens-creches/instituicoes-de-longa-permanencia-para-idosos).
- Agência Nacional de Vigilância Sanitária. [Roteiro Objetivo de Inspeção para
  ILPI](https://pesquisa.anvisa.gov.br/index.php/978156?lang=pt-BR).
- Brasil. [Lei nº 10.741/2003 — Estatuto da Pessoa
  Idosa](https://www.planalto.gov.br/ccivil_03/leis/2003/l10.741compilado.htm).
- Ministério da Mulher, da Família e dos Direitos Humanos. [Manual de
  Fiscalização das
  ILPIs](https://www.gov.br/mdh/pt-br/centrais-de-conteudo/pessoa-idosa/manual-de-fiscalizacao-das-ilpis.pdf),
  contendo a RDC Anvisa nº 502/2021.
- Conselho Nacional de Assistência Social. [Resolução nº 109/2009 — Tipificação
  Nacional de Serviços
  Socioassistenciais](https://www.mds.gov.br/webarquivos/public/resolucao_CNAS_N109_%202009.pdf).
- Organização Mundial da Saúde. [Integrated Care for Older People — ICOPE,
  segunda
  edição](https://www.who.int/publications/i/item/integrated-care-for-older-people-%28-icope%29-guidance-for-person-centred-assessment-and-pathways-in-primary-care).
- Conselho Federal de Contabilidade. [Entidades sem Finalidade de
  Lucros](https://cfc.org.br/tecnica/perguntas-frequentes/entidades-sem-finalidade-de-lucros/).
- Autoridade Nacional de Proteção de Dados. [Perguntas frequentes sobre a
  LGPD](https://www.gov.br/anpd/pt-br/acesso-a-informacao/perguntas-frequentes/perguntas-frequentes).
- Ministério da Saúde. [LGPD e dados de
  saúde](https://www.gov.br/saude/pt-br/acesso-a-informacao/lgpd).
- Conselho Federal de Medicina. [Certificado Digital do
  CFM](https://certificadodigital.cfm.org.br/perguntas-frequentes/).
- Conselho Federal de Medicina. [Assinatura digital na plataforma de Prescrição
  Eletrônica](https://prescricaoeletronica.cfm.org.br/faq_medicos/assinatura-digital/).
- Brasil. [Lei nº 13.787/2018 — digitalização e guarda de prontuário de
  paciente](https://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/lei/l13787.htm).
- Brasil. [Lei nº 14.063/2020 — assinaturas eletrônicas em interações com entes
  públicos e em questões de saúde](https://www.planalto.gov.br/ccivil_03/_ato2019-2022/2020/lei/l14063.htm).
- Conselho Federal de Medicina. [Resolução CFM nº 2.299/2021 — documentos
  médicos eletrônicos](https://sistemas.cfm.org.br/normas/visualizar/resolucoes/BR/2021/2299).
- Instituto Nacional de Tecnologia da Informação. [Conceitos de assinatura e
  certificação digital](https://validar.iti.gov.br/conceitos.html).
- Conselho Federal de Enfermagem. [Resolução Cofen nº
  736/2024](https://www.cofen.gov.br/resolucao-cofen-no-736-de-17-de-janeiro-de-2024/).
- Conselho Federal de Enfermagem. [Resolução Cofen nº 754/2024 — prontuário
  eletrônico e plataformas digitais](https://www.cofen.gov.br/wp-content/uploads/2024/05/Resolucao-Cofen-no-754-2024-Normatiza-o-uso-do-prontuario-eletronico-e-plataformas-digitais-no-ambito-da-Enfermagem.pdf).
- Autoridade Nacional de Proteção de Dados. [Tratamento de dados pessoais para
  fins acadêmicos e pesquisas](https://www.gov.br/anpd/pt-br/centrais-de-conteudo/materiais-educativos-e-publicacoes/guia-orientativo-tratamento-de-dados-pessoais-para-fins-academicos-e-para-a-realizacao-de-estudos-e-pesquisas).
