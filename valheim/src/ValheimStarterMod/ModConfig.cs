using BepInEx.Configuration;

namespace ValheimStarterMod
{
    /// <summary>
    /// Todas as opcoes do mod em um lugar so.
    /// O arquivo gerado fica em BepInEx\config\com.pass-os.valheimstartermod.cfg
    /// e pode ser editado com o jogo aberto (SettingChanged dispara na hora).
    /// </summary>
    internal static class ModConfig
    {
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<float> StaminaRegenMultiplier;
        internal static ConfigEntry<bool> VerboseLogging;

        internal static void Init(ConfigFile config)
        {
            Enabled = config.Bind(
                "Geral",
                "Enabled",
                true,
                "Liga/desliga todos os efeitos do mod sem precisar remover o .dll.");

            StaminaRegenMultiplier = config.Bind(
                "Jogador",
                "StaminaRegenMultiplier",
                1.0f,
                new ConfigDescription(
                    "Multiplicador da regeneracao de stamina. 1.0 = vanilla.",
                    new AcceptableValueRange<float>(0.1f, 10f)));

            VerboseLogging = config.Bind(
                "Debug",
                "VerboseLogging",
                false,
                "Loga detalhes extras no console do BepInEx. Util so em desenvolvimento.");
        }

        /// <summary>Atalho usado pelos patches: o mod deve agir agora?</summary>
        internal static bool Active => Enabled != null && Enabled.Value;
    }
}
