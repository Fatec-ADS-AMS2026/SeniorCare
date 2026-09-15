## 1. Contratos de release e caminhos-base

- [x] 1.1 Adicionar `VITE_BASE_PATH` à matriz do workflow de release, usando `/care/` para assistência, `/stock/` para estoque e `/` para API/portal, e verificar os bundles produzidos.
- [x] 1.2 Validar no workflow que API, portal, assistência, estoque e migration SQL pertencem à mesma versão e commit antes de montar o manifesto por digest.
- [x] 1.3 Remover a dependência do `git push` ignorado para `main` como mecanismo de entrega e manter manifesto e migration SQL como assets obrigatórios do GitHub Release.
- [x] 1.4 Implementar helper de obtenção atômica dos assets de uma versão, validando versão, quatro digests e migration SQL antes de atualizar o diretório local de releases.
- [x] 1.5 Adicionar testes do helper para release completo, asset ausente, versão divergente, digest malformado e download interrompido.

## 2. Composição do ambiente acadêmico

- [x] 2.1 Criar `infra/deploy/docker-compose.homolog.yml` como overlay da stack pull-based, com Mailpit pinado e diferenças exclusivamente acadêmicas.
- [x] 2.2 Criar `infra/deploy/clients/academico/.env.example` sem segredos, enumerando hostname, porta HTTPS, banco, volumes, SMTP sintético, URL de ativação, limites de memória e identificação didática.
- [x] 2.3 Parametrizar `deploy.sh` para selecionar de forma fechada a composição acadêmica, usar nome de projeto estável e preservar backup, pré-validação, healthcheck, histórico e rollback.
- [x] 2.4 Remover a publicação direta de API e front-ends na LAN e limitar PostgreSQL e Mailpit UI ao loopback, mantendo comunicação pela rede privada da aplicação.
- [x] 2.5 Configurar volumes e caminhos com identificação inequívoca de homologação para PostgreSQL, Data Protection, Caddy e backups.
- [x] 2.6 Adicionar healthchecks para todos os serviços necessários e calibrar limites/timeout para a VM de 1 vCPU sem aceitar falso estado saudável.

## 3. Borda HTTPS e mesma origem

- [x] 3.1 Adaptar a configuração do Caddy para hostname interno configurável com exemplo `seniorcare.test`, CA interna e porta host `8443`, sem conflito com Apache.
- [x] 3.2 Verificar o roteamento do portal em `/`, API em `/api`, assistência em `/care` e estoque em `/stock`, incluindo assets, refresh de SPA e cabeçalhos de segurança.
- [x] 3.3 Garantir que URLs de ativação e demais links públicos usem a origem acadêmica HTTPS completa, incluindo a porta configurada.
- [x] 3.4 Restringir a confiança da API em forwarded headers ao proxy/rede esperados e testar esquema HTTPS e IP de origem percebidos através da borda.
- [x] 3.5 Criar verificação automatizada que falhe se portas diretas da API, banco, front-ends, Mailpit ou Portainer forem publicadas na LAN pela configuração acadêmica.

## 4. Persistência de autenticação

- [x] 4.1 Configurar ASP.NET Core Data Protection para usar key ring persistente e nome de aplicação estável fornecidos por configuração.
- [x] 4.2 Montar o volume de chaves somente na API e separá-lo por ambiente, sem registrar material criptográfico ou compartilhá-lo com produção.
- [x] 4.3 Adicionar teste de integração que autentique, recrie a instância da API usando o mesmo key ring e confirme que uma sessão ainda válida permanece verificável.
- [x] 4.4 Documentar o efeito operacional de perda/rotação das chaves e o tratamento esperado das sessões sem incluir as chaves no backup de outro ambiente.

## 5. Mailpit e fluxos de conta

- [x] 5.1 Configurar a API acadêmica para SMTP interno sem TLS, remetente sintético e ausência de credenciais de provedor externo.
- [x] 5.2 Publicar a interface do Mailpit somente em `127.0.0.1` e manter sua porta SMTP somente na rede Docker.
- [x] 5.3 Adicionar smoke test que crie um fluxo de ativação ou recuperação fictício, consulte o Mailpit e confirme destinatário sintético e URL HTTPS acadêmica.
- [x] 5.4 Documentar acesso à UI do Mailpit por túnel SSH e limpeza das mensagens/tokens entre turmas.

## 6. Dados sintéticos e reset protegido

- [x] 6.1 Definir convenções versionadas para nomes, e-mails, documentos, telefones, endereços e dados assistenciais inequivocamente fictícios.
- [x] 6.2 Implementar carga acadêmica determinística e idempotente, aplicada separadamente do startup normal e cobrindo somente capacidades existentes.
- [x] 6.3 Ampliar `check-synthetic-fixtures.sh` para escanear a carga acadêmica e bloquear valores fora das convenções aprovadas.
- [x] 6.4 Exibir no portal, por configuração pública de runtime, a identificação persistente “Ambiente didático — dados fictícios”.
- [x] 6.5 Implementar reset acadêmico fail-closed que valide cliente, projeto Compose, banco e alvo persistente e exija confirmação digitada exata antes de excluir.
- [x] 6.6 Adicionar testes positivos do reset em alvo efêmero e testes negativos que comprovem ausência de chamada destrutiva para qualquer identificador divergente.

## 7. Portainer e operação independente

- [x] 7.1 Remover o Portainer do profile operacional padrão ou isolá-lo em profile opt-in para evitar conflito com a instância já instalada na VM.
- [x] 7.2 Garantir que a stack use projeto/labels estáveis e possa ser observada no Portainer sem ser criada ou atualizada por ele.
- [x] 7.3 Verificar deploy, status, logs e rollback com o Portainer indisponível e registrar a evidência.
- [x] 7.4 Revisar o overlay operacional para subir Caddy e backup sem duplicar ferramentas existentes nem permitir que `--remove-orphans` afete serviços da VM.

## 8. Gates e testes de implantação

- [x] 8.1 Adicionar ao CI a renderização de `docker compose config` para base mais overlay acadêmico usando valores exclusivamente sintéticos.
- [x] 8.2 Criar validador da configuração renderizada para imagens por digest, serviços obrigatórios, healthchecks, redes privadas, volumes separados e bindings de loopback/LAN permitidos.
- [x] 8.3 Implementar smoke test reproduzível para HTTPS, health/readiness, `/`, `/api`, `/care`, `/stock`, assets e cookie `Secure`/`HttpOnly`/`SameSite`.
- [x] 8.4 Exercitar instalação em banco vazio e atualização sobre a versão anterior suportada, verificando migração, preservação de dados sintéticos e falha segura.
- [x] 8.5 Exercitar backup, restauração e rollback em ambiente efêmero e comprovar que uma falha não remove automaticamente o volume persistente.
- [x] 8.6 Integrar os novos gates ao check agregado das branches protegidas e publicar diagnóstico acionável quando falharem.

## 9. Runbook e homologação na VM

- [x] 9.1 Criar runbook acadêmico com pré-requisitos, coexistência com Apache/Monsta/pgAdmin/cloudflared/Portainer, segredos externos, GHCR, deploy, status e diagnóstico de recursos.
- [x] 9.2 Documentar resolução inicial por `/etc/hosts`, instalação e validação da CA interna em estações-piloto e futura migração para DNS institucional.
- [x] 9.3 Documentar seed, banner, proibição de dados reais, primeiro administrador, Mailpit, reset entre turmas e resposta a inclusão acidental de dado inadequado.
- [x] 9.4 Documentar e testar procedimentos de backup sob demanda, retenção, restauração, rollback de imagem e recuperação quando migração impedir rollback simples.
- [ ] 9.5 Implantar um release candidato na VM atual em `8443`, confirmando que nenhuma porta ou container preexistente foi substituído.
- [ ] 9.6 Validar a partir de uma estação-piloto da LAN o certificado, a origem única, a ausência das portas internas e os fluxos de portal, módulos e Mailpit.
- [ ] 9.7 Medir CPU, memória, disco e latência com a concorrência representativa informada pela primeira turma e registrar limite ou necessidade de vCPU adicional.
- [ ] 9.8 Executar rollback e restauração reais, registrar evidências e vincular os resultados às tarefas 9.1, 9.4 e 9.7 da mudança `introduce-senior-portal`.
