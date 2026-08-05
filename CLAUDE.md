## Git / Pull Requests

- Quando um PR se originar de uma issue do GitHub (ou de um change do OpenSpec em
  `openspec/changes/<id>/`), a descrição do PR MUST incluir uma palavra-chave de
  fechamento referenciando essa issue (`Closes #N`, `Fixes #N` ou `Resolves #N`) — assim
  o GitHub fecha a issue automaticamente ao mergear.
- Se o PR resolver mais de uma issue, liste uma palavra-chave por issue (`Closes #16,
  Closes #17`).
- Se o PR não se originar de nenhuma issue específica, não é necessário inventar uma
  referência.
