using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace ValheimStarterMod
{
    /// <summary>
    /// Ponto de entrada do mod.
    /// BepInEx instancia esta classe como um MonoBehaviour quando carrega o plugin.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Plugin Instance;

        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            ModConfig.Init(Config);

            // Aplica todos os [HarmonyPatch] deste assembly.
            _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            _harmony.PatchAll(typeof(Plugin).Assembly);

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} carregado.");
        }

        private void OnDestroy()
        {
            // Importante para hot-reload (ScriptEngine) nao deixar patches orfaos.
            _harmony?.UnpatchSelf();
            Log?.LogInfo($"{MyPluginInfo.PLUGIN_NAME} descarregado.");
        }
    }
}
