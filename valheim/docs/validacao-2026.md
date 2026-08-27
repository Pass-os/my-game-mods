# Validacao dos tutoriais do Azumatt (2023) contra o estado atual

Os tutoriais do Azumatt sao a melhor referencia pratica de modding de Valheim,
mas foram gravados em **2023**. Desde entao o jogo trocou de motor e o
ecossistema mudou. Esta pagina confere afirmacao por afirmacao.

**Metodo:** cada item foi verificado contra a instalacao local do jogo
(`C:\Program Files (x86)\Steam\steamapps\common\Valheim`), contra a API do
GitHub/NuGet/Thunderstore, ou por build real. Nada aqui foi assumido.

**Data da verificacao:** 27/08/2026.

**Resumo:** o **metodo** continua todo valido. O que envelheceu foram
**numeros de versao** — e um deles (a versao da Unity) e fatal se voce seguir
o video ao pe da letra.

---

## Vermelho — mudou, seguir o video quebra

### 1. Versao da Unity: 2019.4.31f1 -> **6000.0.61f1**

O erro mais caro da lista. Um AssetBundle construido em versao diferente da do
jogo **nao carrega** — voce so descobre depois de extrair assets e modelar.

Verificado por dois caminhos independentes:

```powershell
(Get-Item "C:\Program Files (x86)\Steam\steamapps\common\Valheim\valheim.exe").VersionInfo.ProductVersion
# -> 6000.0.61f1 (74a0adb02c31)
```

E a [documentacao oficial do Jotunn](https://valheim-modding.github.io/Jotunn/tutorials/asset-creation.html)
diz textualmente: *"Valheim uses Unity Version 6000.0.61"*.

> Confira de novo a cada atualizacao do jogo — a Iron Gate sobe a versao da
> Unity de vez em quando.

### 2. `denikson-BepInExPack_Valheim-5.4.2202` -> **5.4.2333**

A string de dependencia no `manifest.json` mudou. Verificado na API do
Thunderstore. Bate com o BepInEx **5.4.23.3** instalado na sua maquina.

### 3. AssetRipper 1.9.9 -> **2.0.0**

O 2.0.0 saiu em 24/08/2026, dias atras. A opcao **"Dll Export Without
Renaming"** continua existindo (agora sob *Script Export Format*), entao a
instrucao em si vale — so o numero da versao mudou.

---

## Amarelo — ainda funciona, mas ha caminho melhor

### 4. `.NET Framework 4.6.2` -> `net472` / `net48`

O 4.6.2 ainda compila. Mas o proprio Azumatt ja migrou: o
[AllManagersModTemplate](https://github.com/AzumattDev/AllManagersModTemplate)
(atualizado em **12/02/2026**) usa `v4.8`. Este repo usa `net472`, testado com
build real. Qualquer um dos tres funciona; 4.6.2 e so o mais conservador.

O que **nao** mudou: o jogo continua em **Mono**, nao IL2CPP. Confirmado pela
pasta `MonoBleedingEdge`, pela secao `[UnityMono]` do `doorstop_config.ini` e
por `mscorlib 4.6.57`. Ou seja: **nunca** use `net8.0`+ aqui.

### 5. `LangVersion 10` -> `latest` / `preview`

Da para usar C# moderno. O template atual do Azumatt usa `preview`; aqui usamos
`latest`. Recursos que dependem de tipos do runtime novo precisam de shim —
ja incluimos [`Compat/IsExternalInit.cs`](../src/ValheimStarterMod/Compat/IsExternalInit.cs)
para `record` e `init`.

### 6. AssemblyPublicizer do CabbageCrow: **abandonado desde 16/05/2021**

Este e o item que mais merece atencao. A ferramenta nao recebe commit ha
**mais de 5 anos** — anterior a todas as mudancas de engine do Valheim.

Nuance importante, para ser justo com o tutorial: o Azumatt **continua usando
publicizacao manual** no template de 2026 (`assembly_valheim_publicized.dll` e
companhia). Entao o *metodo* nao esta morto — a *ferramenta especifica* e que
esta parada.

Este repo usa `BepInEx.AssemblyPublicizer.MSBuild` (mantido, ultima versao
0.4.3), que faz o mesmo em tempo de build. Vantagens praticas: nao precisa
refazer a cada patch do jogo, nao versiona DLLs do jogo no git, e quem clonar o
repo compila direto. Ver [setup.md](setup.md).

### 7. Referencias: falta uma assembly nova

O tutorial de 2023 lista `assembly_valheim`, `assembly_utils` e
`assembly_guiutils`. Esses tres continuam certos, mas hoje falta:

**`SoftReferenceableAssets.dll`** — o sistema de carregamento sob demanda
introduzido no update 0.217.40. Nao existia em 2023. O template atual do
Azumatt ja a referencia (publicizada), e este repo tambem.

### 8. ServerSync via "Copy Always" -> ILRepack

Colocar `ServerSync.dll` como "Copy Always" funciona, mas se outro mod carregar
uma versao diferente do ServerSync, um dos dois quebra. Hoje se recomenda ou
fundir com ILRepack, ou usar o `ConfigSync.cs` como arquivo fonte. Ver
[serversync.md](serversync.md).

O ServerSync em si esta vivo (ultimo push 06/04/2025, release **v1.19**).

---

## Verde — continua exatamente valido

| Afirmacao do tutorial | Situacao | Como foi verificado |
| --- | --- | --- |
| BepInEx e o loader; Harmony faz o patching | Valido | BepInEx 5.4.23.3 rodando na sua maquina |
| Usar **BepInEx 5**, nao o 6 | Valido | 5.4.23.3 instalado; 6.x segue em beta (`6.0.0-be.785`) |
| Publicizar `assembly_valheim`, `assembly_utils`, `assembly_guiutils` | Valido | as tres existem em `valheim_Data\Managed` |
| Referenciar `UnityEngine.dll`, `CoreModule`, `AssetBundleModule` | Valido | todas presentes |
| `BaseUnityPlugin` + `[BepInPlugin(GUID, Name, Version)]` | Valido | build real compila e carrega |
| Config via `Config.Bind(secao, chave, padrao, descricao)` | Valido | bate com a doc atual do BepInEx |
| Registrar item exige `ZNetScene` **e** `ObjectDB` (`Awake` + `CopyOtherDB`) | Valido | os quatro membros existem na build atual |
| Managers como `.cs` em vez de `.dll` | Valido, e ainda recomendado | template de fev/2026 usa exatamente assim |
| AssetBundle como **Embedded Resource** | Valido | continua sendo o padrao dos dois caminhos |
| Nome do bundle e do prefab tem que bater exatamente | Valido | segue sendo a causa n1 de "o item nao aparece" |
| `MaterialReplacer` corrige o item rosa | Valido | `RegisterGameObjectForShaderSwap` / `ForMatSwap` existem no fonte atual |
| Color Space **Linear** | Valido | continua correto para o visual do jogo |
| Shaders em modo **Built-in** | Valido | nao ha DLL de URP/HDRP em `Managed` — o jogo usa mesmo o pipeline built-in |
| Adicionar **Vulkan** nas Graphics APIs | Valido | `UnityPlayer.dll` referencia `vulkan-1.dll`; D3D11/D3D12 tambem suportados |
| Corrigir null reference com `LOD Group` | Valido | comportamento de Unity, independe de versao |
| Estrategia de dois projetos (referencia + trabalho) | Valido | so boa pratica, nao depende de versao |
| Instalacao limpa / duplicar a pasta antes de ripar | Valido | e mais importante hoje, com mod managers mexendo na pasta |
| r2modman / Thunderstore MM para perfis de teste | Valido | r2modman com push em 26/08/2026 |
| Templates do Azumatt | **Vivos** | `AllManagersModTemplate` e `PieceManager` com push em fev/2026 |
| Trocar o campo de autor no template | Valido | aqui isso vem do `.csproj` (`Authors`, `Product`) |

### Sobre o "ZeroHarmony.dll"

Detalhe pequeno: o arquivo chama-se **`0Harmony.dll`** (zero-Harmony). No video
sai falado como "ZeroHarmony", o que confunde na hora de procurar. Neste repo
ele nem aparece — vem pelo pacote `BepInEx.Core`.

---

## Ferramenta que aparece nas buscas e voce deve ignorar

**[heinermann/ValheimExportHelper](https://github.com/heinermann/ValheimExportHelper)** —
plugin do AssetRipper especifico para Valheim. Aparece bem posicionado nas
buscas, mas o ultimo release e de **09/11/2023**, para o AssetRipper da epoca.
Nao acompanhou a migracao para Unity 6. Use o AssetRipper 2.0.0 puro.

---

## Como refazer esta validacao no futuro

```powershell
# Versao da Unity do jogo (a mais importante)
(Get-Item "C:\Program Files (x86)\Steam\steamapps\common\Valheim\valheim.exe").VersionInfo.ProductVersion

# Versao do BepInEx instalado
$p="$env:APPDATA\Thunderstore Mod Manager\DataFolder\Valheim\profiles\Default"
([Diagnostics.FileVersionInfo]::GetVersionInfo("$p\BepInEx\core\BepInEx.dll")).FileVersion

# String de dependencia atual do BepInExPack
(Invoke-RestMethod "https://thunderstore.io/api/experimental/package/denikson/BepInExPack_Valheim/").latest.version_number

# Versao atual do Jotunn
(Invoke-RestMethod "https://api.nuget.org/v3-flatcontainer/jotunnlib/index.json").versions[-1]

# Um repo ainda e mantido?
(Invoke-RestMethod "https://api.github.com/repos/AzumattDev/PieceManager" -Headers @{"User-Agent"="ps"}).pushed_at
```
