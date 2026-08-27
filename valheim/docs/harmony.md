# Patches com HarmonyX

O HarmonyX reescreve metodos do jogo em runtime. Voce nao altera o `.dll` do
jogo — voce registra "ganchos" que rodam antes, depois ou no lugar do codigo
original.

## Os tres tipos de patch

| Tipo | Quando roda | Use para |
| --- | --- | --- |
| `[HarmonyPrefix]` | Antes do metodo original | Alterar argumentos, ou **cancelar** o original (`return false`) |
| `[HarmonyPostfix]` | Depois do original | Ler/ajustar o resultado. **Prefira este** — quase nunca quebra outros mods |
| `[HarmonyTranspiler]` | Reescreve o IL | Ultimo recurso: mudar uma constante no meio de um metodo grande |

**Regra de ouro:** use `Postfix` sempre que der. `Prefix` com `return false`
cancela o metodo para *todos* os mods e e a causa numero 1 de incompatibilidade.

## Anatomia de um patch

```csharp
[HarmonyPatch(typeof(Player))]
internal static class PlayerPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.Awake))]
    private static void Awake_Postfix(Player __instance)
    {
        __instance.m_staminaRegen *= 2f;
    }
}
```

### Parametros magicos

O nome do parametro e o que importa, nao a posicao:

| Nome | O que e |
| --- | --- |
| `__instance` | O objeto em que o metodo foi chamado (o `this`) |
| `__result` | O valor de retorno. Em `Postfix`, use `ref` para alterar |
| `__state` | Carrega dados do `Prefix` para o `Postfix` do mesmo patch |
| `___m_campo` | **Tres** underscores: acessa um campo privado da instancia |
| nome do argumento | Recebe o argumento original; use `ref` para alterar |

```csharp
// Le e altera o retorno
[HarmonyPostfix]
[HarmonyPatch(nameof(Player.GetMaxStamina))]
private static void GetMaxStamina_Postfix(ref float __result)
{
    __result *= 1.5f;
}
```

### Metodos sobrecarregados

Se o metodo tem overloads, informe os tipos:

```csharp
[HarmonyPatch(typeof(Character), nameof(Character.Damage), typeof(HitData))]
```

## Como descobrir o que patchear

Voce precisa saber o nome exato do metodo e a assinatura na build atual.

### Opcao 1 — Descompilador (recomendado)

Abra `valheim_Data\Managed\assembly_valheim.dll` no
[dnSpy](https://github.com/dnSpyEx/dnSpy) ou
[ILSpy](https://github.com/icsharpcode/ILSpy). Voce le o codigo em C# quase
como o original. E assim que se descobre o que existe.

### Opcao 2 — IntelliSense

Como o projeto referencia as DLLs do jogo **publicizadas**, o autocomplete da
IDE ja mostra tudo, inclusive membros `private` e `protected`.

### Opcao 3 — Dump por linha de comando

Util para checar rapido se um metodo ainda existe apos um patch do jogo:

```powershell
# Lista membros de um tipo, filtrando por regex
Add-Type -AssemblyName System.Reflection.Metadata
$path = "C:\Program Files (x86)\Steam\steamapps\common\Valheim\valheim_Data\Managed\assembly_valheim.dll"
$fs = [IO.File]::OpenRead($path)
$pe = [Reflection.PortableExecutable.PEReader]::new($fs)
$md = [Reflection.Metadata.PEReaderExtensions]::GetMetadataReader($pe)
foreach ($th in $md.TypeDefinitions) {
    $td = $md.GetTypeDefinition($th)
    if ($md.GetString($td.Name) -ne "Player") { continue }
    foreach ($mh in $td.GetMethods()) {
        $m = $md.GetMethodDefinition($mh)
        Write-Output ("{0} params={1} [{2}]" -f $md.GetString($m.Name), $m.GetParameters().Count, $m.Attributes)
    }
}
$pe.Dispose(); $fs.Dispose()
```

## Classes importantes do jogo

| Classe | Responsabilidade |
| --- | --- |
| `Player` | Jogador. `Player.m_localPlayer` e o seu |
| `Character` | Base de `Player` e `Humanoid` (vida, stamina, dano) |
| `Humanoid` | Personagens que usam equipamento |
| `ItemDrop.ItemData` | Uma instancia de item (o `m_shared` tem os stats) |
| `Inventory` | Inventario |
| `ObjectDB` | Banco de todos os prefabs de item/receita |
| `ZNetScene` | Todos os prefabs de mundo |
| `ZNetView` / `ZDO` | Rede e persistencia. `m_nview.IsOwner()` = voce manda neste objeto |
| `ZRoutedRpc` | RPCs entre cliente e servidor |
| `Game`, `ZoneSystem` | Ciclo de vida do mundo |
| `WearNTear` | Durabilidade/estabilidade de construcao |
| `MessageHud` | Mensagens na tela |
| `Localization` | Traducoes |

## Onde enganchar a inicializacao

`Awake()` do plugin roda **antes do jogo carregar qualquer coisa**. `ObjectDB`,
`ZNetScene` e `Player` ainda nao existem la. Use um patch para agir na hora certa:

| Precisa de | Patcheie |
| --- | --- |
| Itens e receitas | `ObjectDB.Awake` e `ObjectDB.CopyOtherDB` (roda de novo ao entrar no mundo) |
| Prefabs do mundo | `ZNetScene.Awake` |
| Jogador local pronto | `Player.OnSpawned` |
| Mundo carregado | `Game.Start` |

## Multiplayer

O servidor manda no estado. Antes de alterar algo persistente, cheque a posse:

```csharp
if (__instance.m_nview == null || !__instance.m_nview.IsOwner()) return;
```

E lembre: se o mod muda regras de jogo, cliente e servidor precisam concordar.
Para isso existe o [ServerSync](serversync.md).

## Debug

1. Ative o console do BepInEx: em `BepInEx\config\BepInEx.cfg`, secao
   `[Logging.Console]`, `Enabled = true`.
2. Log: `Plugin.Log.LogInfo(...)` / `LogWarning` / `LogError`.
3. Log completo em `BepInEx\LogOutput.log`.
4. **Hot reload:** instale o plugin `ScriptEngine`. Ele recarrega o `.dll` com
   o jogo aberto (tecla F6), sem reiniciar. Por isso o `Plugin.OnDestroy()`
   chama `_harmony.UnpatchSelf()` — sem isso, os patches antigos ficariam
   duplicados a cada reload.
5. Erro `MissingMethodException` / `Harmony patch target not found` depois de
   um patch do jogo = o metodo mudou de nome ou assinatura. Redescubra com o
   descompilador.
