## ADDED Requirements

### Requirement: Revisão semântica é versionada e independente de provedor
O repositório SHALL manter critérios de revisão semântica versionados e
independentes de um modelo específico. Esses critérios SHALL cobrir aderência ao
OpenSpec, autorização, proteção de dados pessoais, segurança e qualidade dos
achados. Os checks determinísticos obrigatórios SHALL permanecer separados da
revisão semântica e SHALL NOT depender de credencial de assinatura de modelo no
GitHub Actions.

#### Scenario: Revisor autorizado analisa um pull request
- **WHEN** um mecanismo autorizado realiza revisão semântica do pull request
- **THEN** ele encontra no repositório as regras de entrega, OpenSpec,
  autorização, LGPD, segurança e qualidade que devem orientar o parecer

#### Scenario: Pull request executa checks obrigatórios
- **WHEN** um pull request é aberto contra `main` ou `dev`
- **THEN** build, testes, SAST, SCA e detecção de segredos continuam conclusivos
  sem depender da execução de um modelo de linguagem

#### Scenario: Credencial do provedor removido
- **WHEN** não existe mais workflow versionado que use a integração da Anthropic
- **THEN** o repositório não referencia nem mantém o secret de assinatura desse
  provedor no GitHub Actions
