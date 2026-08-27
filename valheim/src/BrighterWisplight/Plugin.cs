using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace BrighterWisplight
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;

            ModConfig.Init(Config);

            // Editar o .cfg (ou usar o ConfigurationManager com F1) reaplica
            // nas wisps que ja estao no mundo, sem precisar reiniciar.
            Config.SettingChanged += (_, _) => WispTweaks.ApplyToAll();

            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            _harmony.PatchAll(typeof(Plugin).Assembly);

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} carregado.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
