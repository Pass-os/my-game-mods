using HarmonyLib;

namespace BrighterWisplight.Patches
{
    /// <summary>
    /// Engancha na criacao de cada wisp. Awake e OnEnable sao privados no jogo —
    /// por isso o patch usa o nome em string em vez de nameof.
    /// </summary>
    [HarmonyPatch(typeof(Demister))]
    internal static class DemisterPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        private static void Awake_Postfix(Demister __instance)
        {
            WispTweaks.Apply(__instance);
        }

        /// <summary>
        /// Rede de seguranca: se o jogo reaproveitar um objeto em vez de criar
        /// um novo, Awake nao roda de novo, mas OnEnable sim. Reaplicar e
        /// barato e nao acumula, porque os valores originais ficam guardados.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        private static void OnEnable_Postfix(Demister __instance)
        {
            WispTweaks.Apply(__instance);
        }
    }
}
