## Context

Ver [proposal.md](proposal.md) para a motivação. O repositório já possui um
Compose pull-based, imagens publicadas no GHCR, manifestos pinados por digest,
`deploy.sh` com backup/pré-validação/healthcheck/rollback, Caddy com CA interna e um
Compose local que inclui Mailpit. Esses elementos ainda estão distribuídos entre
configurações de produção, teste e operação e não formam um ambiente acadêmico
coeso.

A VM inicial usa Ubuntu 20.04 sob Xen, tem 1 vCPU, cerca de 8 GiB de RAM e já
executa Apache na porta 80, cloudflared, Monsta FTP, pgAdmin e um Portainer
independente ligado somente a `127.0.0.1:9443`. Não há domínio público. O Docker
e o Compose existentes são suficientes, mas a baixa capacidade de CPU limita
concorrência e exige imagens prontas.

A autenticação usa cookie `Secure`, `HttpOnly` e `SameSite=Strict`, portanto HTTP
por IP não é um alvo aceitável. A API também confia em `X-Forwarded-For` e
`X-Forwarded-Proto` do proxy imediato; publicar sua porta diretamente ampliaria a
superfície para spoofing. A mudança `introduce-senior-portal` já estabelece mesma
origem e caminhos `/care`, `/stock` e `/api`, mas o workflow de release ainda não
constrói os dois módulos com esses caminhos-base.

## Goals / Non-Goals

**Goals:**

- compor um ambiente acadêmico a partir da base de deploy e de um overlay pequeno;
- usar o mesmo artefato executável que poderá ser promovido a produção;
- estabelecer uma única entrada HTTPS que conviva com a VM atual;
- manter serviços internos fora da LAN e ferramentas administrativas no loopback;
- preservar estado e sessão entre atualizações normais;
- tornar seed, reset, deploy e recuperação verificáveis e seguros;
- manter a implantação independente da disponibilidade do Portainer.

**Non-Goals:**

- administrar o firewall, DNS ou ciclo de vida geral da VM pelo repositório;
- converter o servidor atual em cluster ou PaaS multiusuário;
- permitir que cada estudante publique uma stack concorrente na mesma VM;
- fornecer disponibilidade ou política de backup equivalentes à produção real;
- compartilhar banco, volumes, SMTP ou segredos entre homologação e produção.

## Decisions

### 1. Base pull-based com overlay acadêmico

`infra/deploy/docker-compose.yml` continuará sendo a base de aplicação. Um
`docker-compose.homolog.yml` acrescentará Mailpit e as diferenças estritamente
acadêmicas. O `deploy.sh` aceitará a seleção validada do ambiente/arquivos Compose
sem abrir caminho para nomes arbitrários.

O PostgreSQL continuará dentro do Compose, com volume persistente próprio; não
haverá instalação manual de PostgreSQL no host. Imagens da aplicação sempre virão
do manifesto por digest e o servidor não terá SDKs de build como pré-requisito.

Alternativas consideradas:

- uma segunda cópia integral do Compose: rejeitada pelo risco de divergência;
- Portainer Stack como fonte da verdade: rejeitada por contornar gates e dificultar
  revisão/versionamento;
- Coolify: rejeitado nesta etapa por sobrepor a orquestração existente e consumir
  recursos da VM sem resolver requisitos adicionais.

### 2. Topologia canônica de mesma origem

O release canônico será construído para portal em `/`, assistência em `/care/` e
estoque em `/stock/`. O workflow fornecerá `VITE_BASE_PATH` por item da matriz de
build. Caddy encaminhará `/api/*` sem remover `/api` e usará `handle_path` para
remover os prefixos dos front-ends antes de acessar seus Nginx internos.

Esta decisão torna o mesmo bundle adequado a homologação e produção. Manter
subdomínios em produção exigiria bundles distintos ou base path configurável em
runtime, contrariando a promoção dos mesmos digests.

Alternativas consideradas:

- HTTP por IP: rejeitado por incompatibilidade com o cookie `Secure`;
- subdomínios locais: rejeitados por complexidade desnecessária de DNS/cookie;
- recompilar por ambiente: rejeitado por quebrar build-once/deploy-many.

### 3. TLS interno em porta não conflitante

O hostname de exemplo será `seniorcare.test`, resolvido inicialmente por
`/etc/hosts` e depois, opcionalmente, por DNS interno. Caddy usará sua CA interna e
publicará por padrão `8443:443`, preservando o Apache em 80. O runbook explicará
como obter, distribuir, confiar e validar o certificado raiz. Hostname e porta
permanecerão configuráveis para a rede da faculdade.

`.test` foi escolhido em vez de `.local` para evitar interferência com mDNS. Um
certificado público foi descartado enquanto não houver domínio e exposição
controlada para validação ACME.

### 4. Somente Caddy expõe a aplicação na LAN

As publicações de host da API e dos front-ends serão removidas. PostgreSQL,
Mailpit UI e Portainer ficarão ligados ao loopback quando um acesso administrativo
for necessário; SMTP do Mailpit existirá somente na rede Docker. O acesso remoto a
essas UIs usará túnel SSH, sem ampliar o firewall da LAN.

A confiança em forwarded headers será limitada ao salto/rede de proxy esperada ou,
no mínimo, protegida pela ausência de publicação direta da API. A implementação
deverá testar o IP e o esquema percebidos pela API através do Caddy.

Alternativa considerada: manter portas diretas para conveniência de depuração.
Foi rejeitada; diagnóstico pode usar `docker compose exec`, logs ou túnel temporário.

### 5. Mailpit é dependência exclusiva da homologação

O overlay acadêmico acrescentará Mailpit com tag pinada, SMTP interno e UI em
`127.0.0.1:8025`. A API receberá remetente sintético, TLS SMTP desabilitado e URL
de ativação igual à origem acadêmica. Configurações de produção continuarão
exigindo um provedor SMTP externo e não incluirão esse serviço.

Alternativa considerada: enviar e-mail real a contas dos alunos. Foi rejeitada
por depender da Internet, aumentar risco de envio indevido e dificultar testes
repetíveis de ativação e recuperação.

### 6. Persistência separada por ambiente

PostgreSQL, chaves do ASP.NET Core Data Protection, dados do Caddy e backups usarão
volumes ou caminhos explicitamente qualificados para homologação. A API persistirá
seu key ring com um nome de aplicação estável; esse volume não será compartilhado
com produção. Backups incluirão o banco; as chaves deverão ser preservadas para
continuidade das sessões, mas não serão copiadas para outro ambiente.

Volumes nomeados facilitam o primeiro uso. Caminhos de host continuarão
configuráveis quando a universidade precisar adequar retenção e backup externo.

### 7. Seed idempotente e reset com fail-closed

A carga acadêmica será versionada, determinística e idempotente, com convenções
explícitas para nomes, e-mails, documentos e demais identificadores fictícios. Ela
será aplicada por uma operação separada do startup normal para evitar repopular ou
sobrescrever atividade didática a cada deploy.

O reset validará simultaneamente o identificador do cliente, nome do projeto
Compose, nome do banco e alvo persistente contra valores acadêmicos fixos. A
exclusão exigirá confirmação digitada com o identificador exato. Qualquer ausência
ou divergência falhará antes de executar comandos destrutivos. Testes automatizados
usarão doubles para comprovar que casos negativos não chamam Docker ou SQL.

### 8. Identificação didática é configuração visível

O portal receberá configuração pública de nome/rotulagem do ambiente em runtime e
mostrará de forma persistente “Ambiente didático — dados fictícios”. O rótulo não
substitui os controles de separação, mas reduz o risco de estudantes, docentes e
visitantes confundirem a instância com serviço assistencial real.

### 9. Artefatos de GitHub Release são a interface de entrega

O workflow continuará anexando manifesto e migration SQL ao GitHub Release, mas
não tratará um `git push` direto e ignorado para `main` como entrega confiável. Um
helper obterá os dois assets para a versão solicitada, validando nomes, conteúdo
mínimo, versão e presença dos quatro digests antes de disponibilizá-los ao
`deploy.sh`. Download incompleto será escrito em diretório temporário e não
substituirá artefatos válidos.

Assinatura criptográfica do manifesto poderá ser adicionada futuramente; nesta
mudança, a autenticação do GitHub/GHCR, pinagem por digest e validação estrutural
são o limite definido.

### 10. Portainer permanece fora da stack da aplicação

O Portainer já instalado será preservado. O serviço duplicado deixará de subir com
o profile operacional padrão ou será isolado em profile opt-in próprio. Containers
criados pelo fluxo Compose manterão labels e nomes de projeto suficientes para
serem observados na interface, mas deploy, rollback e reset não dependerão dela.

### 11. Gates combinam validação estática e smoke test

O CI renderizará a configuração Compose com um `.env` exclusivamente sintético e
inspecionará portas, volumes, imagens e healthchecks. O workflow de release testará
que os quatro componentes e a migração pertencem à mesma versão/commit. A carga
acadêmica entrará no scanner de fixtures sintéticas.

Um smoke test executável localmente validará HTTPS, caminhos, prontidão, cookie
seguro e Mailpit. Verificações que exigem a LAN real, confiança instalada em
estações e restauração operacional serão registradas como evidência de
homologação, não simuladas como conclusão de CI.

## Risks / Trade-offs

- [CA interna não confiada em um computador] → documentar instalação por sistema
  operacional e fornecer comando de verificação antes das aulas.
- [Uma única vCPU aumenta tempo de startup e healthcheck] → manter imagens prontas,
  limitar concorrência de deploy/testes e calibrar timeout sem mascarar falha.
- [Caddy em 8443 torna a URL menos amigável] → aceitar a porta enquanto Apache
  ocupa 80/443; migrar somente após decisão coordenada da infraestrutura.
- [Mesma origem exige corte coordenado dos bundles e proxy] → entregar build args,
  Caddy e smoke tests no mesmo release e manter rollback atômico por manifesto.
- [Volume persistente conserva dados inadequados inseridos manualmente] → banner,
  política institucional, acesso restrito e reset periódico com seed aprovado.
- [Mailpit expõe tokens temporários a administradores] → loopback/túnel SSH,
  contas fictícias e reset das mensagens entre turmas.
- [Portainer permite mutação fora do fluxo] → restringir credenciais, documentar
  seu papel emergencial e usar reconciliação pelo deploy versionado.
- [Rollback de imagem não desfaz migração incompatível] → preservar backup
  pré-deploy e exigir procedimento de restauração quando houver mudança destrutiva.

## Migration Plan

1. Criar os overlays, exemplos de configuração e gates sem alterar a VM.
2. Ajustar o build dos módulos para os caminhos canônicos e produzir um release
   candidato completo com quatro imagens e migration SQL.
3. Configurar persistência de Data Protection e validar continuidade da sessão em
   ambiente efêmero.
4. Preparar na VM diretórios/volumes, segredos acadêmicos e login somente leitura
   necessário para obter imagens privadas.
5. Obter os assets do release, renderizar a configuração e executar validações
   prévias sem derrubar os containers existentes da VM.
6. Subir a stack SeniorCare com Caddy em 8443, mantendo Apache, Monsta, pgAdmin,
   cloudflared e Portainer nos endereços atuais.
7. Instalar a CA interna e a resolução de `seniorcare.test` em estações-piloto.
8. Aplicar o seed acadêmico, executar smoke tests, backup/restauração e rollback.
9. Registrar evidências das tarefas reais de homologação do Senior Portal antes
   de ativá-lo como entrada principal.

Rollback: enquanto o novo ambiente não estiver aceito, parar somente o projeto
Compose acadêmico preservando volumes; isso devolve a porta 8443 sem tocar nos
serviços preexistentes. Após aceitação, `deploy.sh rollback` retorna ao manifesto
anterior. Falhas de dados exigem restauração explícita do backup, nunca remoção
automática do volume.

## Open Questions

- Qual mecanismo institucional distribuirá o hostname e a CA para todas as
  estações após o piloto: DNS/GPO/MDM da faculdade ou procedimento manual?
- Qual diretório de backup da VM será incluído no backup institucional externo e
  qual retenção acadêmica será adotada?
- Quantos alunos simultâneos a primeira turma pretende usar, para definir o teste
  de carga e confirmar se a VM precisa receber vCPU adicional?
