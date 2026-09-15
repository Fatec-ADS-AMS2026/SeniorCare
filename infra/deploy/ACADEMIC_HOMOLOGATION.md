# Homologação acadêmica do SeniorCare

Este ambiente é exclusivamente didático. Use somente dados sintéticos aprovados;
nunca cadastre residentes, trabalhadores, familiares, doadores ou informações de
saúde reais. A identificação pública deve permanecer **“Ambiente didático — dados
fictícios”**.

## Pré-requisitos

A VM deve ter Docker Compose v2, `gh`, acesso autenticado ao GHCR e acesso à
GitHub Release do repositório. Ela já pode hospedar Apache, Monsta, pgAdmin,
cloudflared e Portainer: o projeto acadêmico não os cria, substitui ou remove.

Prepare diretórios privados e persistentes, de propriedade do operador Docker:

```sh
sudo install -d -m 0700 \
  /opt/seniorcare-academico/postgres \
  /opt/seniorcare-academico/data-protection \
  /opt/seniorcare-academico/caddy-data \
  /opt/seniorcare-academico/caddy-config \
  /opt/seniorcare-academico/backups
```

Copie `clients/academico/.env.example` para `clients/academico/.env`, altere as
senhas e mantenha o arquivo fora do Git. `ACADEMIC_HOSTNAME`,
`ACADEMIC_HTTPS_PUBLISH`, `ACADEMIC_NETWORK_SUBNET` e `ACADEMIC_CADDY_IP` devem
ser reservados para esta instância. Não reutilize banco, volume, diretório de
backup, segredo ou key ring de outro ambiente.

Os segredos ficam somente no `.env` privado: `POSTGRES_PASSWORD` e as
credenciais de pull do GHCR (`GHCR_USER` e `GHCR_TOKEN`, se não houver login
prévio). O token precisa apenas de leitura de packages e não deve ser colocado
no arquivo de ambiente, em histórico de shell, logs ou tickets.

O primeiro administrador é o bootstrap sintético definido por
`Bootstrap__AdminEmail` e `Bootstrap__AdminDisplayName` no `.env`. Após o
primeiro deploy, localize a mensagem de ativação no Mailpit, conclua a
ativação e confirme o acesso antes de liberar a turma. Não use uma conta ou
endereço de docente real como bootstrap.

## Hostname e CA interna

No piloto, cada estação deve resolver o hostname configurado para o IP da VM:

```text
<IP-DA-VM> seniorcare.test
```

Após o primeiro deploy, obtenha a raiz da CA do Caddy em
`/opt/seniorcare-academico/caddy-data/caddy/pki/authorities/local/root.crt` e
instale-a no armazenamento de autoridades confiáveis do sistema operacional da
estação. Valide com:

```sh
curl --fail --cacert root.crt https://seniorcare.test:8443/
```

A distribuição por DNS institucional e GPO/MDM substitui o `/etc/hosts` quando
for disponibilizada. Não desative a validação TLS nem use HTTP por IP.

## Publicação, status e rollback

```sh
cd infra/deploy
CLIENT=academico ./deploy.sh 2026.09.0
CLIENT=academico ./deploy.sh status
CLIENT=academico ./deploy.sh rollback
```

O deploy baixa manifesto e migration SQL da GitHub Release para diretório
temporário, valida versão e quatro digests, cria backup prévio quando há banco,
pré-valida a migration e só então atualiza a stack. `rollback` troca a imagem
para o manifesto anterior; não desfaz migration incompatível. Nesse caso, pare
a stack acadêmica, restaure explicitamente o backup acadêmico e só então
retome o deploy. Nunca remova volumes como mecanismo de rollback.

### Backup e restauração

Cada deploy preserva automaticamente até três dumps pré-deploy em
`ACADEMIC_BACKUPS_PATH`. Para um backup sob demanda, com a stack acadêmica
saudável, execute:

```sh
docker exec "$(docker compose -p seniorcare-academico \
  -f docker-compose.yml -f docker-compose.homolog.yml \
  --env-file clients/academico/.env ps -q postgres)" \
  pg_dump -U seniorcare_academico -d db_seniorcare_academico \
  > /opt/seniorcare-academico/backups/manual-$(date -u +%Y%m%dT%H%M%SZ).sql
```

Antes de uma restauração, pare a stack, preserve uma cópia adicional do volume
e restaure somente um dump acadêmico aprovado no banco acadêmico. Registre
versão da imagem, nome e checksum do dump, operador e resultado. Se a migração
impedir rollback simples, mantenha a stack parada, restaure o dump no PostgreSQL
acadêmico e só então execute `CLIENT=academico ./deploy.sh <versão-anterior>`.
Nunca use `down -v`, `docker volume rm` ou reset para recuperar uma falha.

O CI exercita backup/restauração contra PostgreSQL efêmero e o rollback do
`deploy.sh` com dois manifestos imutáveis. Para repetir localmente:

```sh
bash infra/deploy/tests/test-academic-backup-restore.sh
bash infra/deploy/tests/test-deploy-rollback.sh
```

## Sessões e Data Protection

`ACADEMIC_DATA_PROTECTION_PATH` contém as chaves que protegem cookies e tokens.
Preserve-o entre recriações normais da API e inclua-o somente no procedimento de
backup/restauração deste ambiente. Perda ou rotação deliberada do key ring
invalida sessões e tokens existentes; avise a turma e faça novo login. Nunca
copie essas chaves para produção, outro cliente ou outra turma.

## Mailpit

A API acadêmica entrega e-mails somente ao Mailpit na rede Docker. A UI está
presa em `127.0.0.1:8025`; acesse-a remotamente apenas por túnel SSH:

```sh
ssh -L 8025:127.0.0.1:8025 operador@vm-da-faculdade
```

Depois, abra `http://localhost:8025`. As mensagens contêm links e tokens
provisórios; limpe-as entre turmas e após qualquer exercício de recuperação de
conta. Não configure SMTP externo nesse cliente.

## Seed, reset e dados inadequados

A carga acadêmica é operação separada do startup normal. Ela é versionada no
repositório e, hoje, cria somente as capacidades já existentes de instituição e
administrador acadêmicos PROVISIONED, com os valores sintéticos configurados em
`Bootstrap__*`. Execute-a somente depois de publicar um release:

```sh
CLIENT=academico ./seed-academico.sh
```

O comando falha se o cliente, banco, volume, manifesto do release ou
`Bootstrap__RunOnStartup=false` divergirem do contrato acadêmico. É idempotente:
uma segunda execução não recria a instituição nem o administrador.

Antes de qualquer seed ou
reset, confira cliente, projeto Compose, banco e caminho persistente. Um reset
válido exige confirmação digitada exata e só pode atingir o ambiente acadêmico.
Se houver suspeita de dado pessoal real, interrompa o acesso de estudantes,
registre o incidente institucional, remova o dado conforme a governança
aplicável e recarregue somente a carga aprovada.

Entre turmas, confirme que há backup quando a retenção é necessária, execute
o reset com a confirmação explícita e aplique apenas o seed acadêmico aprovado:

```sh
CLIENT=academico ./reset-academico.sh --confirm seniorcare-academico
```

## Capacidade e diagnóstico

A VM inicial possui uma vCPU. Em caso de lentidão, colete `docker stats`, espaço
em disco e logs da stack antes de elevar limites ou concorrência. Reduza a
quantidade de usuários simultâneos ou solicite vCPU adicional; não remova TLS,
healthchecks, backups, persistência ou isolamento de rede para ganhar recursos.
Portainer é apenas observação/resposta emergencial: deploy, status, logs e
rollback devem funcionar pelo `deploy.sh` mesmo com ele indisponível.
