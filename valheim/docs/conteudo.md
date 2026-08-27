# Adicionar conteudo novo (itens, pecas, criaturas)

Registrar um item ou uma peca de construcao "na mao" e chato: voce tem que
enganchar em `ObjectDB.Awake` **e** `ObjectDB.CopyOtherDB`, registrar no
`ZNetScene`, corrigir os shaders do prefab, cuidar da localizacao e garantir que
o servidor conheca o prefab. Por isso ninguem faz isso na mao — existem duas
familias de bibliotecas que resolvem tudo.

## Jotunn vs. Managers — qual usar

| | **Jotunn** | **Managers** (Blaxxun / Azumatt) |
| --- | --- | --- |
| O que e | Biblioteca completa, um `.dll` unico | Arquivos `.cs` avulsos que voce adiciona ao projeto |
| Como o jogador instala | Precisa **baixar o Jotunn** tambem (dependencia no Thunderstore) | Nada. O codigo esta dentro do seu `.dll` |
| Cobertura | Itens, pecas, receitas, criaturas, skills, localizacao, GUI, comandos de console, KitBash, inputs | Um manager por dominio; voce pega so o que usa |
| Atualizacao | `dotnet build` pega a versao nova do NuGet | Voce recopia o `.cs` quando sair correcao |
| Melhor para | Mods grandes, varios tipos de conteudo | Mods focados ("so adiciona 3 pecas"), zero dependencia |

**Recomendacao:** se o mod adiciona conteudo variado, use Jotunn. Se e um mod
enxuto e voce nao quer obrigar o jogador a instalar dependencia, use o Manager
especifico.

---

## Caminho A — Jotunn

Ligue no `.csproj`:

```xml
<UseJotunn>true</UseJotunn>
```

Isso adiciona `JotunnLib 2.29.2` via NuGet. Declare a dependencia no
`thunderstore/manifest.json`:

```json
"dependencies": [
  "denikson-BepInExPack_Valheim-5.4.2333",
  "ValheimModding-Jotunn-2.29.2"
]
```

E marque no plugin para o BepInEx carregar o Jotunn primeiro:

```csharp
[BepInDependency(Jotunn.Main.ModGuid)]
public class Plugin : BaseUnityPlugin { }
```

Registrar uma peca:

```csharp
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

private void AddPieces()
{
    var bundle = AssetUtils.LoadAssetBundleFromResources("meuassetbundle");
    var prefab = bundle.LoadAsset<GameObject>("MinhaPorta");

    PieceManager.Instance.AddPiece(new CustomPiece(prefab, fixReference: true, new PieceConfig
    {
        Name        = "Minha Porta",
        PieceTable  = "Hammer",       // Hammer, Hoe, Cultivator
        Category    = "Building",
        CraftingStation = "piece_workbench",
        Requirements = new[]
        {
            new RequirementConfig { Item = "FineWood", Amount = 20, Recover = true }
        }
    }));

    bundle.Unload(false);
}
```

`fixReference: true` e o que troca os shaders do bundle pelos shaders reais do
jogo — sem isso o modelo fica rosa.

Documentacao: <https://valheim-modding.github.io/Jotunn/>

---

## Caminho B — Managers

Cada manager e um repositorio com um ou dois arquivos `.cs`. Voce **copia os
arquivos para o seu projeto** (ex.: `src/MeuMod/Managers/PieceManager.cs`) e
compila junto — nada e baixado pelo jogador.

| Manager | Repo | Para que |
| --- | --- | --- |
| PieceManager | [AzumattDev/PieceManager](https://github.com/AzumattDev/PieceManager) (fork mantido) | Pecas de construcao |
| ItemManager | [AzumattDev/ItemManager](https://github.com/AzumattDev/ItemManager) | Itens, armas, receitas |
| CreatureManager | [blaxxun-boop/CreatureManager](https://github.com/blaxxun-boop/CreatureManager) | Criaturas |
| SkillManager | [blaxxun-boop/SkillManager](https://github.com/blaxxun-boop/SkillManager) | Skills customizadas |
| LocalizationManager | [blaxxun-boop/LocalizationManager](https://github.com/blaxxun-boop/LocalizationManager) | Traducoes por arquivo |
| ServerSync | [blaxxun-boop/ServerSync](https://github.com/blaxxun-boop/ServerSync) | Config servidor->cliente (ver [serversync.md](serversync.md)) |

O `PieceManager` traz junto `MaterialReplacer.cs` (corrige os shaders, mesmo
papel do `fixReference` do Jotunn) e `SnapPointMaker.cs` (pontos de encaixe).

Exemplo de uso:

```csharp
BuildPiece piece = new BuildPiece("meuassetbundle", "MinhaPorta");
piece.Name.English("Minha Porta");
piece.Description.English("Uma porta caprichada.");
piece.RequiredItems.Add("FineWood", 20, true);
piece.Category.Add(BuildPieceCategory.Building);
piece.Crafting.Set(CraftingTable.Workbench);
```

Templates prontos do Azumatt (bons pontos de partida para copiar codigo):

- [AllManagersModTemplate](https://github.com/AzumattDev/AllManagersModTemplate) — todos juntos
- [PieceManagerModTemplate](https://github.com/AzumattDev/PieceManagerModTemplate)
- [ItemManagerModTemplate](https://github.com/AzumattDev/ItemManagerModTemplate)

---

## Embutir o AssetBundle (vale para os dois caminhos)

Os dois esperam o bundle **embutido no `.dll`**, nao solto no disco.

1. Construa o bundle na Unity (ver [assets-unity.md](assets-unity.md)) — atencao
   a versao da Unity, que precisa bater com a do jogo (**6000.0.61f1**).
2. Copie o arquivo do bundle para `src/MeuMod/Assets/`.
3. Declare como recurso embutido no `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Assets\meuassetbundle" />
</ItemGroup>
```

> No Visual Studio / Rider isso equivale a clicar no arquivo > Propriedades >
> **Build Action = Embedded Resource**. Neste repo o `.csproj` e editado
> direto, entao basta a linha acima.

4. O **nome do bundle no codigo** e o **nome do prefab** precisam bater
   exatamente com os nomes na Unity. E o erro mais comum: o mod compila, carrega
   sem erro, e a peca simplesmente nao aparece no jogo.

## Testar no jogo

1. `dotnet build -c Debug` — o `.dll` ja vai para o perfil do Thunderstore MM.
2. Abra o Valheim pelo mod manager.
3. Entre num mundo, aperte `F5` e digite `devcommands`, depois `debugmode`.
4. Abra o martelo — a peca deve estar la. Se nao estiver, cheque o
   `BepInEx\LogOutput.log`.
