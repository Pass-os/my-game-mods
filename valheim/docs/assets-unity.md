# Assets proprios: AssetRipper + projeto Unity

**So precisa disto se o mod for adicionar conteudo visual novo** (modelos,
pecas de construcao, itens com mesh proprio, prefabs). Para tweaks de
comportamento (dano, stamina, timers, UI existente), pule este guia.

O fluxo tem duas metades:

1. **Extrair** os assets do jogo com o AssetRipper -> vira um projeto Unity.
2. **Criar** o seu asset num projeto Unity limpo, exportar como **AssetBundle**,
   e carregar esse bundle pelo mod.

---

## Atencao: versoes

Muitos tutoriais em video estao desatualizados. Confira antes de comecar:

| Item | Tutorial antigo | **Atual (verificado nesta maquina / ago-2026)** |
| --- | --- | --- |
| Versao da Unity | 2019.4.31f1 | **6000.0.61f1** — o jogo migrou para Unity 6 |
| API Compatibility Level | .NET 4.0 | **.NET Framework** (equivalente na Unity 6) |
| AssetRipper | 1.9.9 | **2.0.0** |
| Sistema de assets | tudo em memoria | **SoftReferenceableAssets** (AssetBundles sob demanda, desde 0.217.40) |

> A versao da Unity **precisa bater com a do jogo**. Um AssetBundle construido
> numa versao diferente da que o jogo roda simplesmente nao carrega.
>
> Como conferir a versao exata a qualquer momento:
> ```powershell
> (Get-Item "C:\Program Files (x86)\Steam\steamapps\common\Valheim\valheim.exe").VersionInfo.ProductVersion
> ```

---

## 1. Preparar uma copia limpa do jogo

O AssetRipper precisa dos arquivos **originais**, sem mods.

1. Se voce usa Thunderstore MM / r2modman, os mods ficam no perfil, nao na
   pasta do jogo — mas o `winhttp.dll` e o `doorstop_config.ini` ficam. Por
   seguranca, use "Verificar integridade dos arquivos" na Steam, ou:
2. Copie a pasta inteira do jogo para outro lugar (ex.: `D:\Valheim-Clean`) e
   trabalhe sobre a copia. Nunca aponte o ripper para a instalacao que voce joga.

## 2. Extrair com o AssetRipper

1. Baixe o [AssetRipper 2.0.0](https://github.com/AssetRipper/AssetRipper/releases)
   (`AssetRipper_win_x64.zip`).
2. Abra e carregue a pasta **limpa** do jogo.
3. Nas configuracoes de exportacao, escolha **"DLL export without renaming"**.
   Isso preserva os nomes das classes de script — sem essa opcao, os prefabs
   exportados perdem a ligacao com os componentes e viram lixo.
4. Exporte. Demora bastante (dezenas de minutos, depende do disco).
5. O resultado e um projeto Unity abrivel.

## 3. Abrir na Unity

Instale a Unity **6000.0.61f1** pelo Unity Hub e abra o projeto exportado.

Em `Edit > Project Settings`:

| Setting | Valor | Por que |
| --- | --- | --- |
| Player > Color Space | **Linear** | Sem isso as cores ficam lavadas/erradas |
| Player > Auto Graphics API | **desmarcado**, adicione **Vulkan** | Bate com o que o jogo usa |
| Player > API Compatibility Level | **.NET Framework** | Bate com o perfil Mono do jogo |
| Graphics > Shaders | modo **Built-in** | O Valheim usa o pipeline built-in; sem isso arvores e materiais renderizam errado |

## 4. Estrategia de dois projetos

Nao trabalhe dentro do projeto extraido — ele e enorme e lento.

```
Projeto de REFERENCIA        Projeto de TRABALHO
(saida do AssetRipper)       (Unity novo, vazio)
        │                            │
        │  exporta .unitypackage     │
        └──────────────────────────> │  importa so o que precisa
                                     │  (ex.: uma porta de madeira)
                                     │
                                     └─> Build AssetBundle
```

- **Referencia:** so para procurar e exportar. Ache o prefab, clique com o
  botao direito > `Export Package`.
- **Trabalho:** projeto limpo com as mesmas Project Settings acima. Importe o
  `.unitypackage`, modifique, e construa o bundle daqui.

Isso mantem o projeto de trabalho leve e o build do bundle rapido.

## 5. Erros comuns

**`NullReferenceException` em prefabs importados.** Geralmente e um `LOD Group`
ou um "material fixer" que nao veio na exportacao. Adicione o componente
`LOD Group` manualmente no objeto raiz e configure os niveis. Alem de corrigir
o erro, isso e obrigatorio para performance — sem LOD, seu asset renderiza em
full detail a qualquer distancia.

**Shaders rosa.** O material aponta para um shader que nao existe no projeto.
No jogo, a solucao correta e trocar o shader pelo do proprio Valheim em runtime
(o Jotunn faz isso automaticamente ao registrar o prefab).

**O bundle nao carrega no jogo.** Quase sempre e versao da Unity diferente da
do jogo. Reconstrua na versao certa.

## 6. Carregar o bundle no mod

Embuta o `.unity3d` / bundle como **EmbeddedResource** no `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Assets\meuassetbundle" />
</ItemGroup>
```

Com Jotunn (`<UseJotunn>true</UseJotunn>`), o carregamento e o registro ficam
assim:

```csharp
var bundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("meuassetbundle");
var prefab = bundle.LoadAsset<GameObject>("MinhaPeca");

Jotunn.Managers.PieceManager.Instance.AddPiece(new Jotunn.Entities.CustomPiece(
    prefab,
    fixReference: true,          // troca os shaders/refs pelos do jogo
    new Jotunn.Configs.PieceConfig
    {
        PieceTable = "Hammer",
        Requirements = new[]
        {
            new Jotunn.Configs.RequirementConfig { Item = "Wood", Amount = 5, Recover = true }
        }
    }));

bundle.Unload(false);
```

Sem o Jotunn voce teria que registrar manualmente no `ZNetScene`/`ObjectDB`,
corrigir shaders e cuidar da rede na mao — e por isso que para conteudo novo
a recomendacao e ligar o Jotunn.

## Referencias

- [Valheim Unity Project Guide (Wiki oficial de modding)](https://github.com/Valheim-Modding/Wiki/wiki/Valheim-Unity-Project-Guide)
- [Jotunn — Developing Assets with Unity](https://valheim-modding.github.io/Jotunn/tutorials/asset-creation.html)
- [Modding FAQ do update de AssetBundles 0.217.40](https://www.valheimgame.com/support/modding-faq-for-the-asset-bundle-update-0-217-40/)
- [AssetRipper Releases](https://github.com/AssetRipper/AssetRipper/releases)
