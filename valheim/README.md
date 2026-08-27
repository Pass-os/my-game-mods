# Mods de Valheim

Ambiente de desenvolvimento configurado e **validado com build real** contra a
instalacao local do jogo.

## Como o modding de Valheim funciona

Valheim e um jogo Unity com backend **Mono** (nao IL2CPP), o que significa que o
codigo do jogo fica em DLLs .NET legiveis. Isso torna o modding relativamente
simples e e por isso que a stack e sempre a mesma:

```
valheim.exe
  └─ winhttp.dll            <- Unity Doorstop: sequestra o boot da Unity
       └─ BepInEx.Preloader <- carrega o BepInEx antes do jogo iniciar
            └─ BepInEx      <- descobre e instancia os plugins
                 └─ seu .dll em BepInEx/plugins
                      └─ HarmonyX  <- reescreve metodos do jogo em runtime
```

Voce **nunca edita** `assembly_valheim.dll`. Voce escreve um plugin que o
HarmonyX injeta nos metodos do jogo (`Prefix`, `Postfix`, `Transpiler`).

### Ambiente detectado nesta maquina

| Item | Valor |
| --- | --- |
| Instalacao | `C:\Program Files (x86)\Steam\steamapps\common\Valheim` |
| Motor | Unity **6000.0.61f1** (Unity 6), backend Mono |
| Perfil .NET do jogo | .NET Framework 4.x (`mscorlib 4.6.57`) -> mods usam **`net472`** |
| BepInEx | **5.4.23.3** (via Thunderstore Mod Manager, perfil `Default`) |
| Doorstop | 4.4.0 |
| SDK usado no build | .NET SDK 10.0.302 (compila `net472` sem problema) |

## Estrutura

```
valheim/
├── Valheim.sln
├── Directory.Build.props        # TFM, deteccao do jogo, pasta de deploy
├── NuGet.config                 # nuget.org + feed oficial do BepInEx
├── Environment.props.example    # copie p/ Environment.props se os paths mudarem
├── docs/                        # guias detalhados
├── dist/                        # (gitignored) zips prontos p/ Thunderstore
└── src/
    └── ValheimStarterMod/
        ├── ValheimStarterMod.csproj
        ├── Plugin.cs            # entrypoint (BaseUnityPlugin)
        ├── ModConfig.cs         # opcoes do mod (BepInEx ConfigFile)
        ├── Patches/
        │   └── PlayerPatches.cs # exemplos de patch HarmonyX
        └── thunderstore/        # manifest.json, icon.png, README, CHANGELOG
```

## Comandos

```powershell
cd valheim

# Compila e copia o .dll direto pro perfil do Thunderstore MM
dotnet build -c Debug

# Compila e gera valheim/dist/<Mod>-<versao>.zip pronto pro Thunderstore
dotnet build -c Release
```

Nao ha passo manual de publicizacao nem de copia de arquivo. O build resolve os
caminhos sozinho.

## O que ja esta resolvido no build

**Publicizacao automatica.** O jogo tem muita coisa `private`/`protected`
(ex.: `Character.m_nview`). O pacote `BepInEx.AssemblyPublicizer.MSBuild` marca
as refs com `Publicize="true"` e gera `IgnoresAccessChecksTo` em tempo de
compilacao — voce acessa membros privados normalmente, sem gerar nem versionar
DLLs modificadas.

> A alternativa manual e o [CabbageCrow/AssemblyPublicizer](https://github.com/CabbageCrow/AssemblyPublicizer),
> que gera um `assembly_valheim_publicized.dll` em disco. Funciona, mas exige
> rodar a ferramenta de novo a cada patch do jogo e nao versiona bem.
> O metodo MSBuild faz a mesma coisa em memoria, a cada build.

**Deteccao de caminhos.** `Directory.Build.props` acha o Valheim e o perfil do
mod manager sozinho. Se sua maquina for diferente, copie
`Environment.props.example` para `Environment.props` (ignorado pelo git).

**Deploy pos-build.** Em `Debug`, o `.dll` + `.pdb` vao para
`BepInEx\plugins\<NomeDoMod>\` do perfil ativo.

**Empacotamento.** Em `Release`, gera o zip no layout que o Thunderstore exige
(`manifest.json`, `icon.png` 256x256, `README.md`, `CHANGELOG.md`, `plugins/`).

## Guias

| Doc | Assunto |
| --- | --- |
| [`docs/validacao-2026.md`](docs/validacao-2026.md) | **Leia primeiro.** O que dos tutoriais de 2023 ainda vale e o que quebra |
| [`docs/setup.md`](docs/setup.md) | O que este build automatiza vs. o passo a passo dos tutoriais |
| [`docs/harmony.md`](docs/harmony.md) | Como escrever patches, achar metodos do jogo, debugar |
| [`docs/conteudo.md`](docs/conteudo.md) | Adicionar itens/pecas: Jotunn vs. Managers |
| [`docs/itens.md`](docs/itens.md) | Fluxo completo de um item novo com o ItemManager |
| [`docs/assets-unity.md`](docs/assets-unity.md) | AssetRipper + projeto Unity para criar assets proprios |
| [`docs/serversync.md`](docs/serversync.md) | Sincronizar config entre servidor e clientes |
| [`docs/publicar.md`](docs/publicar.md) | Publicar no Thunderstore / Nexus |

> Seguindo algum tutorial em video? Os do Azumatt sao os melhores, mas sao de
> **2023** e desde entao o jogo migrou para Unity 6. Leia
> [`docs/validacao-2026.md`](docs/validacao-2026.md) antes — ele confere cada
> afirmacao dos videos contra o estado atual.

## Bibliotecas que voce pode querer adicionar

| Lib | Para que | Como ligar |
| --- | --- | --- |
| [Jotunn](https://valheim-modding.github.io/Jotunn/) `2.29.2` | Adicionar **conteudo**: itens, prefabs, receitas, pecas de construcao, localizacao, GUI, comandos de console | `<UseJotunn>true</UseJotunn>` no `.csproj` |
| [ServerSync](https://github.com/blaxxun-boop/ServerSync) `v1.19` | Forcar a config do servidor nos clientes + checagem de versao | ver [`docs/serversync.md`](docs/serversync.md) |
| BepInEx ConfigurationManager | Editar a config in-game com F1 | ja instalado no seu perfil (`Azumatt-Official_BepInEx_ConfigurationManager`) |

**Regra pratica:** so tweaks de comportamento (dano, stamina, timers, UI
existente) = BepInEx puro, sem dependencia. Conteudo novo = Jotunn.

## Referencias

- [BepInEx — tutorial de plugin](https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/index.html)
- [Jotunn — docs](https://valheim-modding.github.io/Jotunn/)
- [Valheim-Modding Wiki](https://github.com/Valheim-Modding/Wiki/wiki)
- [JotunnModStub (template oficial)](https://github.com/Valheim-Modding/JotunnModStub)
