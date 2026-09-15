# Convenções de dados sintéticos acadêmicos

Toda carga acadêmica deve usar apenas os valores abaixo. Valores fora das convenções
são bloqueados pelo gate de fixtures.

- Pessoas: `Pessoa Sintética <n>`; instituições: `Instituição Didática <n>` ou
  `Instituição Didática SeniorCare` para o bootstrap.
- E-mail: `<papel>-<n>@seniorcare.example.test` ou
  `admin@seniorcare.example.test` para o administrador inicial.
- Documento: CPF `000.000.000-<n>` e qualquer identificador institucional começa com `SINT-`.
- Telefone: `+55 11 5555-<n com quatro dígitos>`.
- Endereço: `Rua Exemplo Sintético, <n>, Bairro Didático, São Paulo/SP, 00000-<n>`.
- Dados assistenciais: use estados, datas e descrições genéricas de simulação; nunca nomes,
  diagnósticos, prescrições, evoluções, imagens ou documentos de pessoa real.

Seeds devem ser determinísticos, idempotentes e executados separadamente do startup.

## Carga aprovada atual

A versão atual da carga é o bootstrap mínimo de `seed-academico.sh`: uma
instituição `Instituição Didática SeniorCare` e o administrador
`admin@seniorcare.example.test`. Nenhum residente, prontuário, trabalhador,
familiar, doador, estoque ou evento assistencial é criado até que exista uma
carga de domínio explicitamente aprovada.
