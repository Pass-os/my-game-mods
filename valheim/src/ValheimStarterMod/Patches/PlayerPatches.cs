using HarmonyLib;

namespace ValheimStarterMod.Patches
{
    /// <summary>
    /// Exemplos de patch verificados contra a build atual do jogo
    /// (Valheim / Unity 6000.0.61f1).
    /// </summary>
    [HarmonyPatch(typeof(Player))]
    internal static class PlayerPatches
    {
        /// <summary>
        /// Postfix em Player.Awake (protected virtual, 0 params).
        /// Ajusta m_staminaRegen, que e um campo public float do Player.
        ///
        /// Awake roda em TODO Player instanciado (inclusive clones de outros
        /// jogadores em multiplayer). Como m_staminaRegen so afeta a regen
        /// local simulada, isso e seguro; mas se o seu patch tiver efeito
        /// visivel pra outros, filtre por Player.m_localPlayer.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.Awake))]
        private static void Awake_Postfix(Player __instance)
        {
            if (!ModConfig.Active)
            {
                return;
            }

            var multiplier = ModConfig.StaminaRegenMultiplier.Value;
            if (multiplier == 1f)
            {
                return;
            }

            __instance.m_staminaRegen *= multiplier;

            if (ModConfig.VerboseLogging.Value)
            {
                Plugin.Log.LogInfo(
                    $"staminaRegen ajustado para {__instance.m_staminaRegen} (x{multiplier}).");
            }
        }

        /// <summary>
        /// Postfix em Player.OnSpawned(bool).
        /// Demonstra o acesso a m_nview, que e PROTECTED em Character --
        /// so compila porque o build publiciza assembly_valheim.dll.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player.OnSpawned))]
        private static void OnSpawned_Postfix(Player __instance)
        {
            if (!ModConfig.Active || !ModConfig.VerboseLogging.Value)
            {
                return;
            }

            if (__instance != Player.m_localPlayer)
            {
                return;
            }

            var isOwner = __instance.m_nview != null && __instance.m_nview.IsOwner();
            Plugin.Log.LogInfo($"Player local spawnou. IsOwner={isOwner}");
        }
    }
}
