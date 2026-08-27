# my-game-mods

Monorepo de mods, **uma pasta por jogo**. Cada pasta e autocontida: tem a
propria solution, as proprias dependencias e o proprio guia.

| Jogo | Pasta | Stack | Status |
| --- | --- | --- | --- |
| Valheim | [`valheim/`](valheim/) | BepInEx 5 + HarmonyX, C# `net472` | configurado |

## Como adicionar um jogo novo

1. Crie `nome-do-jogo/` na raiz.
2. Coloque ali `Directory.Build.props`, `NuGet.config` e a solution do jogo.
3. Documente a stack no `README.md` da pasta.
4. Adicione a linha na tabela acima.

O `.gitignore` e o `.editorconfig` da raiz valem para todos os jogos.
