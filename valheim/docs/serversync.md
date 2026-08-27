# ServerSync — config do servidor mandando nos clientes

## Quando voce precisa disto

Se o mod muda **regras de jogo** (dano, custo de receita, drops, stamina) e vai
rodar em servidor dedicado, cada cliente teria a propria config. Um jogador
poderia baixar o custo de tudo para 1 madeira no `.cfg` dele. Alem disso, se
metade do grupo esta numa versao e metade em outra, o resultado e dessync.

O ServerSync resolve os dois problemas:

- envia a config **do servidor** para os clientes ao conectar, sobrescrevendo a
  local enquanto durar a sessao;
- compara versoes do mod no handshake e chuta quem estiver abaixo do minimo.

Se o mod so muda coisas **visuais/locais** (UI, hotkeys, camera, hover stats),
voce **nao precisa** de ServerSync.

## Como integrar

O ServerSync e distribuido como `ServerSync.dll` nas
[releases](https://github.com/blaxxun-boop/ServerSync/releases) (atual: **v1.19**).
O fonte e um unico arquivo, `ConfigSync.cs`.

Voce tem duas opcoes:

### Opcao 1 — arquivo fonte (mais simples)

Copie `ConfigSync.cs` do repositorio para `src/MeuMod/ServerSync/ConfigSync.cs`.
Compila junto, vira parte do seu `.dll`, zero configuracao. E o mesmo modelo dos
[Managers](conteudo.md).

### Opcao 2 — DLL + ILRepack

Baixe `ServerSync.dll`, coloque em `src/MeuMod/libs/`, e use o ILRepack para
fundir a DLL no seu assembly no build. Isso evita conflito quando varios mods
carregam versoes diferentes do ServerSync.

```xml
<ItemGroup>
  <PackageReference Include="ILRepack.Lib.MSBuild.Task" Version="2.0.46" PrivateAssets="all" />
  <Reference Include="ServerSync">
    <HintPath>libs\ServerSync.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

> Sem ILRepack, voce teria que distribuir `ServerSync.dll` solto ao lado do seu
> mod — e se outro mod trouxer uma versao diferente, um dos dois quebra.
> Fundir resolve.

## Uso

```csharp
using ServerSync;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private static readonly ConfigSync ConfigSync = new(MyPluginInfo.PLUGIN_GUID)
    {
        DisplayName = MyPluginInfo.PLUGIN_NAME,
        CurrentVersion = MyPluginInfo.PLUGIN_VERSION,
        MinimumRequiredVersion = MyPluginInfo.PLUGIN_VERSION,
    };

    // Helper: liga toda config nova ao sync de uma vez.
    private ConfigEntry<T> Sync<T>(string section, string key, T value, string description)
    {
        var entry = Config.Bind(section, key, value, description);
        ConfigSync.AddConfigEntry(entry);
        return entry;
    }

    private void Awake()
    {
        // Esta opcao decide se o servidor forca as configs. Deixe-a
        // sincronizada e travada, senao o cliente pode desligar o proprio sync.
        var serverConfigLocked = Sync("Geral", "Lock Configuration", true,
            "Se ligado, a config do servidor sobrescreve a dos clientes.");
        ConfigSync.AddLockingConfigEntry(serverConfigLocked);

        var custoMadeira = Sync("Receitas", "CustoMadeira", 20,
            "Madeira necessaria para a peca.");
    }
}
```

### Pontos de atencao

- **`MinimumRequiredVersion`** so deve subir quando houver mudanca que quebra
  compatibilidade. Se voce apontar sempre para a versao atual (como no exemplo),
  qualquer patch obriga todo mundo a atualizar junto.
- **`AddLockingConfigEntry`** e o que da sentido ao resto. Sem ela, o cliente
  desliga o sync e volta a valer a config local.
- **Ler o valor:** continue usando `.Value` normalmente. Quando conectado a um
  servidor com config travada, `.Value` ja devolve o valor do servidor.
- **Dados fora da config:** para sincronizar coisas que nao sao `ConfigEntry`
  (uma tabela de loot em YAML, por exemplo), use `CustomSyncedValue<T>`.
- Em single player e no proprio servidor, tudo funciona normalmente com os
  valores locais.

## Referencia

- [blaxxun-boop/ServerSync](https://github.com/blaxxun-boop/ServerSync)
- [AzumattDev/ServerSyncModTemplate](https://github.com/AzumattDev/ServerSyncModTemplate) — template com o setup pronto
