# Criar itens com o ItemManager

Guia do fluxo completo: duplicar um item do jogo na Unity, retexturizar,
exportar como AssetBundle e registrar no jogo. O exemplo usado e o do tutorial
do Azumatt — um martelo alternativo ("Bronze Hammer") com mais durabilidade.

---

## Antes de comecar: voce precisa mesmo de um item novo?

Vale separar dois objetivos que parecem iguais mas dao trabalhos muito diferentes:

| Objetivo | O que e preciso |
| --- | --- |
| **"O martelo do jogo devia durar mais"** | Um patch Harmony de ~5 linhas. Sem Unity, sem AssetBundle, sem ItemManager, sem dependencia |
| **"Quero um martelo NOVO, com visual proprio, craftavel"** | Unity + AssetBundle + ItemManager — este guia inteiro |

Se o seu caso e o primeiro, pare aqui e faca isto:

```csharp
[HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
private static class BuffHammerDurability
{
    private static void Postfix(ObjectDB __instance)
    {
        var hammer = __instance.GetItemPrefab("Hammer");
        var shared = hammer?.GetComponent<ItemDrop>()?.m_itemData?.m_shared;
        if (shared == null) return;

        shared.m_maxDurability = 500f;
        shared.m_durabilityPerLevel = 100f;
    }
}
```

Campos relevantes de `ItemDrop.ItemData.SharedData` (todos `public`, conferidos
na build atual):

| Campo | O que faz |
| --- | --- |
| `m_maxDurability` | Durabilidade base |
| `m_durabilityPerLevel` | Quanto ganha por nivel de upgrade |
| `m_durabilityDrain` | Quanto gasta por uso |
| `m_useDurability` | `false` = durabilidade infinita |
| `m_canBeReparied` | Se pode ser reparado (sim, o typo e do jogo) |
| `m_destroyBroken` | Se some ao quebrar |

> Patcheie **`ObjectDB.Awake` e `ObjectDB.CopyOtherDB`**. O `CopyOtherDB` roda
> de novo ao entrar num mundo — so o `Awake` faz o efeito sumir ao carregar
> save. E se for servidor dedicado, isso muda regra de jogo: use
> [ServerSync](serversync.md).

O resto deste documento e para o segundo caso: **conteudo novo de verdade**.

---

## 1. Preparar o projeto

O ItemManager e um arquivo `.cs`, nao um `.dll`. Baixe de
[AzumattDev/ItemManager](https://github.com/AzumattDev/ItemManager) e copie
`ItemManager.cs` para `src/MeuMod/Managers/`.

> **Por que `.cs` e nao `.dll`:** alem de nao virar dependencia para o jogador,
> voce pode tornar uma classe interna em `public` quando precisar acessar algo
> especifico. Com DLL isso nao da.

Se for usar `MaterialReplacer` (secao 5), copie tambem `MaterialReplacer.cs`
do [PieceManager](https://github.com/AzumattDev/PieceManager).

Ajuste os metadados no `.csproj` — eles alimentam o `MyPluginInfo` e as
propriedades do `.dll` no Windows, o que ajuda o jogador a conferir a versao:

```xml
<AssemblyName>MoreHammers</AssemblyName>
<Product>More Hammers</Product>
<Version>1.0.0</Version>
<BepInExPluginGuid>com.pass-os.morehammers</BepInExPluginGuid>
```

## 2. Duplicar o prefab na Unity

Pre-requisito: projeto de referencia extraido com AssetRipper e um projeto de
trabalho limpo, os dois na Unity **6000.0.61f1**. Ver
[assets-unity.md](assets-unity.md).

1. No projeto de **referencia**, ache o prefab `Hammer`.
2. Exporte como `.unitypackage` e importe no projeto de **trabalho**.
3. **Duplique** o prefab (`Ctrl+D`) e renomeie para algo unico —
   `BronzeHammer`. O nome nao pode colidir com nada do jogo.
4. Duplique tambem o material e as texturas. Se voce editar os originais,
   voce altera o martelo vanilla junto.

## 3. Retexturizar

1. Exporte a textura duplicada, edite no Photoshop/GIMP/Krita.
2. Reimporte. Se for normal map, marque **Texture Type: Normal map** no
   inspector — senao a iluminacao fica errada.
3. Aponte o material duplicado para as texturas novas.

## 4. Exportar o AssetBundle

1. Selecione o prefab e, no rodape do Inspector, atribua um **AssetBundle name**
   (ex.: `morehammers`).
2. Construa o bundle (`BuildPipeline.BuildAssetBundles` ou o menu do seu script
   de build).
3. Copie o arquivo gerado para `src/MeuMod/Assets/morehammers`.
4. Declare como recurso embutido no `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Assets\morehammers" />
</ItemGroup>
```

> Isso equivale ao "Build Action = Embedded Resource" do Rider/Visual Studio.
> Neste repo o `.csproj` e editado direto.

## 5. Registrar no codigo

```csharp
using ItemManager;

private void Awake()
{
    ModConfig.Init(Config);

    // ("nome do bundle", "nome do prefab")
    // Os dois precisam bater EXATAMENTE com os nomes na Unity.
    Item bronzeHammer = new("morehammers", "BronzeHammer");

    bronzeHammer.Name.English("Bronze Hammer");
    bronzeHammer.Description.English("Um martelo reforcado. Aguenta mais obra.");

    bronzeHammer.Crafting.Add(CraftingTable.Workbench, 2);
    bronzeHammer.RequiredItems.Add("Bronze", 8);
    bronzeHammer.RequiredItems.Add("Wood", 10);
    bronzeHammer.RequiredUpgradeItems.Add("Bronze", 4);

    bronzeHammer.MaximumRequiredStationLevel = 4;
    bronzeHammer.RepairStation.Add(CraftingTable.Workbench, 1);

    _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
    _harmony.PatchAll(typeof(Plugin).Assembly);
}
```

O ItemManager cuida sozinho do registro no `ZNetScene` e no `ObjectDB`,
incluindo o rerregistro no `CopyOtherDB`. Voce **nao** precisa escrever esses
patches na mao.

Outros recursos uteis:

```csharp
bronzeHammer.CraftAmount = 1;
bronzeHammer.DropsFrom.Add("Greydwarf", 0.05f, 1, 1);   // chance, min, max
bronzeHammer.Snapshot();                                 // gera o icone do prefab

// Efeitos visuais/sonoros extras do mesmo bundle
ItemManager.PrefabManager.RegisterPrefab("morehammers", "hammerVisual");
```

Ajustar os stats do item novo (durabilidade, dano) e igual ao patch do inicio
deste doc, so que mirando o seu prefab.

## 6. Corrigir o item rosa

Se o item aparece **rosa/magenta** no jogo, o material esta apontando para um
shader que existia na Unity mas nao existe no jogo. Isso e esperado — os
shaders do Valheim nao vem no bundle.

A correcao roda em runtime:

```csharp
MaterialReplacer.RegisterGameObjectForShaderSwap(prefab, MaterialReplacer.ShaderType.PieceShader);
// ou, para reaproveitar materiais inteiros do jogo:
MaterialReplacer.RegisterGameObjectForMatSwap(prefab);
```

`RegisterGameObjectForShaderSwap` troca so o shader, mantendo as suas texturas —
e o que voce quer para um item retexturizado. `RegisterGameObjectForMatSwap`
substitui o material inteiro pelo do jogo, o que descarta a sua textura.

> Se voce usar Jotunn em vez do ItemManager, o equivalente e passar
> `fixReference: true` ao registrar o `CustomItem`.

## 7. Testar

```powershell
cd valheim
dotnet build -c Debug
```

O `.dll` ja vai para o perfil do Thunderstore MM. No jogo:

1. `F5` > `devcommands`
2. `spawn BronzeHammer` — se aparecer, o registro no `ZNetScene` funcionou
3. Confira o item craftavel na bancada — valida o `ObjectDB` e a receita
4. Cheque `BepInEx\LogOutput.log` se algo nao aparecer

### Erros comuns

| Sintoma | Causa provavel |
| --- | --- |
| Item nao existe (`spawn` falha) | Nome do bundle ou do prefab diferente do que esta na Unity |
| Item rosa | Shader — ver secao 6 |
| Item aparece mas nao e craftavel | Faltou `Crafting.Add(...)` ou o nome da estacao esta errado |
| Icone em branco | Falta `Snapshot()` ou o prefab nao tem sprite atribuido |
| Funciona sozinho, quebra em servidor | Config de regra sem [ServerSync](serversync.md) |
| Sumiu depois de carregar o save | Voce patcheou so `ObjectDB.Awake`, faltou `CopyOtherDB` (o ItemManager ja faz isso — so vale se registrou na mao) |

## Referencias

- [AzumattDev/ItemManager](https://github.com/AzumattDev/ItemManager)
- [AzumattDev/ItemManagerModTemplate](https://github.com/AzumattDev/ItemManagerModTemplate)
- [AzumattDev/PieceManager](https://github.com/AzumattDev/PieceManager) (traz o `MaterialReplacer`)
