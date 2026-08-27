# Publicar o mod

## Gerar o pacote

```powershell
cd valheim
dotnet build -c Release
```

Sai em `valheim/dist/<NomeDoMod>-<versao>.zip`, ja no layout do Thunderstore:

```
NomeDoMod-0.1.0.zip
├── manifest.json
├── icon.png            (256x256, obrigatorio)
├── README.md
├── CHANGELOG.md
└── plugins/
    └── NomeDoMod.dll
```

## Antes de publicar

1. **Suba a versao** no `.csproj` (`<Version>`) **e** no
   `thunderstore/manifest.json` (`version_number`). Os dois precisam bater.
   O Thunderstore recusa reenvio da mesma versao.
2. **Troque o `icon.png`.** O atual e um placeholder gerado. Precisa ser
   exatamente **256x256 PNG**.
3. **Escreva o `thunderstore/README.md`** — e ele que vira a pagina do mod.
4. **Atualize o `CHANGELOG.md`.**
5. **Declare as dependencias** no `manifest.json`:

```json
"dependencies": [
  "denikson-BepInExPack_Valheim-5.4.2202",
  "ValheimModding-Jotunn-2.29.2"
]
```

O BepInEx sempre entra. O Jotunn so se `<UseJotunn>true</UseJotunn>`. Managers
e ServerSync embutidos no fonte **nao** entram — o codigo ja esta no seu `.dll`.

6. **Teste num perfil limpo.** Crie um perfil novo no mod manager com so
   BepInEx + seu mod. E facil o mod parecer funcionar porque outro mod do seu
   perfil esta fazendo metade do trabalho.

## Versionamento

Use semver, e leve a serio o minor em multiplayer:

| Mudou | Suba |
| --- | --- |
| Correcao de bug, sem mudar rede/config | patch (`0.1.0` -> `0.1.1`) |
| Config nova, conteudo novo | minor (`0.1.0` -> `0.2.0`) |
| Mudou RPC / formato de dado sincronizado | major, **e** suba o `MinimumRequiredVersion` do ServerSync |

## Thunderstore

1. Conta em <https://thunderstore.io> (login pelo GitHub ou Discord).
2. Crie ou escolha um **Team** — o pacote pertence ao time, nao a voce.
3. `Upload package` na comunidade **Valheim**, envie o zip.
4. Categorias: marque `Mods` e o que se aplicar (`Server-side`, `Client-side`,
   `Building`, `Utility`...). Isso e o que faz o mod aparecer nas buscas.

Atualizar depois e so subir um zip com `version_number` maior.

## Nexus Mods

Publico diferente, processo manual. Vale se o mod for grande.

1. Conta em <https://www.nexusmods.com/valheim>.
2. `Add a mod`, envie o **mesmo zip**.
3. Escreva a descricao em BBCode e diga explicitamente que precisa de BepInEx,
   com link — o publico do Nexus costuma instalar na mao.

## Quando o Valheim atualizar

1. Compile. Se der erro de compilacao, um metodo do jogo mudou de assinatura —
   redescubra com o descompilador (ver [harmony.md](harmony.md)).
2. Se compilar mas quebrar em runtime com `Harmony patch target not found`, o
   metodo foi renomeado ou removido.
3. Se voce tem AssetBundles, cheque se a versao da Unity do jogo mudou:
   ```powershell
   (Get-Item "C:\Program Files (x86)\Steam\steamapps\common\Valheim\valheim.exe").VersionInfo.ProductVersion
   ```
   Se mudou, reconstrua os bundles na versao nova.
4. Nao e preciso republicizar nada na mao — o publicizer roda a cada build.
