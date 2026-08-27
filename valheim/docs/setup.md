# O setup deste repo vs. os tutoriais

Os tutoriais de modding de Valheim (inclusive os bons, do Azumatt) foram
gravados no estilo antigo de `.csproj` e com passos manuais. Este repo faz a
mesma coisa de forma automatizada. Esta tabela existe para voce conseguir
seguir qualquer video sem se perder.

| O tutorial manda fazer | Aqui | Onde |
| --- | --- | --- |
| Criar *Class Library* .NET Framework 4.6.2 | `.csproj` SDK-style em `net472` (superset compativel, e o perfil real do jogo hoje) | `Directory.Build.props` |
| Editar `.csproj`: `LangVersion 10`, `Nullable`, `AllowUnsafeBlocks` | `LangVersion=latest`, `Nullable=disable` | `Directory.Build.props` |
| Rodar o **AssemblyPublicizer** na mao e guardar `publicized_assemblies/` | `Publicize="true"` nas refs; o publicizer roda a **cada build**, em memoria | `ValheimStarterMod.csproj` |
| Referenciar `BepInEx.dll` e `0Harmony.dll` copiados do jogo | `PackageReference` para `BepInEx.Core` | `ValheimStarterMod.csproj` |
| Bloco `<Choose>` + variavel `$(GamePath)` para portabilidade | Deteccao automatica + `Environment.props` opcional | `Directory.Build.props` |
| Escrever `AssemblyInfo.cs` na mao com titulo/versao | Gerado do `.csproj` (`GenerateAssemblyInfo`) | `Directory.Build.props` |
| Digitar GUID/nome/versao do mod em dois lugares | `MyPluginInfo.PLUGIN_*`, gerado pelo `BepInEx.PluginInfoProps` | `obj/Debug/MyPluginInfo.cs` |
| Copiar o `.dll` do `bin\Debug` para `BepInEx\plugins` | Copia automatica pos-build | target `DeployToBepInEx` |
| Zipar `manifest.json` + `icon.png` + dll na mao | `dotnet build -c Release` gera o zip | target `PackThunderstore` |

## Detalhes que valem explicacao

### Por que `net472` e nao `net462`

O jogo roda Unity 6 com backend Mono, perfil .NET Framework
(`mscorlib 4.6.57`). `net462` funciona, mas `net472` tambem carrega e da acesso
a uma BCL um pouco maior. Se voce um dia ver `TypeLoadException` numa API de
`System.*`, e sinal de que aquela API nao existe no Mono do jogo — ai baixe o
target, nao suba.

### Por que `Nullable` fica desligado

As DLLs do jogo nao tem anotacao de nulabilidade. Com `Nullable=enable` voce
ganha centenas de warnings em codigo que voce nao escreveu. Ligue por projeto
se quiser, nao globalmente.

### `AllowUnsafeBlocks`

Nao esta ligado porque nada aqui precisa. Se voce for mexer com ponteiros,
adicione `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` no `.csproj` do mod.

### `LangVersion=latest` em `net472`

Da acesso a sintaxe moderna de C#. Recursos que dependem de tipos do runtime
novo precisam de um shim — ja incluimos
[`Compat/IsExternalInit.cs`](../src/ValheimStarterMod/Compat/IsExternalInit.cs),
que habilita `record` e propriedades `init`.

### Publicizacao: manual vs. MSBuild

O jeito manual ([CabbageCrow/AssemblyPublicizer](https://github.com/CabbageCrow/AssemblyPublicizer))
consiste em arrastar cada `assembly_*.dll` para o `.exe` da ferramenta,
gerando uma pasta `publicized_assemblies`. Funciona, mas:

- precisa ser refeito **a cada atualizacao do jogo**;
- as DLLs geradas nao devem ir para o git (sao codigo do jogo);
- quem clonar o repo tem que repetir o processo antes de compilar.

O `BepInEx.AssemblyPublicizer.MSBuild` resolve isso: o build gera
`IgnoresAccessChecksTo` para `assembly_valheim`, `assembly_utils` e
`assembly_guiutils` automaticamente. Voce pode conferir o resultado em
`src/ValheimStarterMod/obj/Debug/ValheimStarterMod.IgnoresAccessChecksTo.cs`.

O efeito e o mesmo: `Character.m_nview`, que e `protected`, compila normalmente
(ver `Patches/PlayerPatches.cs`).

## Instalacao do BepInEx

Voce **ja tem** o BepInEx 5.4.23.3 instalado, gerenciado pelo
Thunderstore Mod Manager no perfil `Default`. Nao precisa instalar nada.

Se um dia precisar refazer:

- **Recomendado:** instale via [Thunderstore Mod Manager](https://www.overwolf.com/app/Thunderstore-Thunderstore_Mod_Manager)
  ou [r2modman](https://github.com/ebkr/r2modmanPlus). Eles isolam os mods em
  perfis, entao a instalacao da Steam continua limpa e voce troca de perfil
  (um pra jogar, um pra testar) sem conflito.
- **Manual:** baixe o [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/)
  e extraia o conteudo na raiz do jogo (junto do `valheim.exe`). Esse pack ja
  inclui as *unstripped core libs*, que sao necessarias porque a Unity remove
  partes da BCL no build final e varios mods precisam delas de volta.

Se voce mudar o perfil ou o caminho, ajuste `ModDeployDir` em
`Environment.props` (copie do `Environment.props.example`).
