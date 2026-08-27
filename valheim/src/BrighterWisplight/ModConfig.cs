using BepInEx.Configuration;
using UnityEngine;

namespace BrighterWisplight
{
    /// <summary>
    /// Opcoes do mod. Gera BepInEx\config\com.pass-os.brighterwisplight.cfg.
    /// Editavel com o jogo aberto (F1 no ConfigurationManager) — qualquer
    /// mudanca reaplica na hora nas wisps que ja existem.
    /// </summary>
    internal static class ModConfig
    {
        internal static ConfigEntry<bool> Enabled;

        internal static ConfigEntry<float> IntensityMultiplier;
        internal static ConfigEntry<float> RangeMultiplier;
        internal static ConfigEntry<bool> OverrideColor;
        internal static ConfigEntry<string> LightColor;
        internal static ConfigEntry<bool> CastShadows;

        internal static ConfigEntry<float> DemistRadiusMultiplier;
        internal static ConfigEntry<float> DemistRadiusAbsolute;

        internal static ConfigEntry<string> NameFilter;
        internal static ConfigEntry<bool> VerboseLogging;

        internal static void Init(ConfigFile config)
        {
            Enabled = config.Bind(
                "1 - Geral", "Enabled", true,
                "Liga/desliga o mod inteiro. Desligar restaura os valores originais.");

            // ---------------- Luz ----------------
            IntensityMultiplier = config.Bind(
                "2 - Luz", "IntensityMultiplier", 3.0f,
                new ConfigDescription(
                    "Multiplicador do brilho da wisp. 1 = vanilla. 3 ja da pra andar sem tocha.",
                    new AcceptableValueRange<float>(0.1f, 20f)));

            RangeMultiplier = config.Bind(
                "2 - Luz", "RangeMultiplier", 3.0f,
                new ConfigDescription(
                    "Multiplicador do alcance da luz (quao longe ela ilumina). 1 = vanilla.",
                    new AcceptableValueRange<float>(0.1f, 20f)));

            OverrideColor = config.Bind(
                "2 - Luz", "OverrideColor", false,
                "Se ligado, troca a cor da wisp pela definida em LightColor. " +
                "Desligado mantem o ciano original.");

            LightColor = config.Bind(
                "2 - Luz", "LightColor", "#FFD9A0",
                "Cor da luz em hexadecimal, quando OverrideColor esta ligado. " +
                "O padrao e um tom quente de tocha.");

            CastShadows = config.Bind(
                "2 - Luz", "CastShadows", false,
                "Faz a wisp projetar sombras, como uma tocha de verdade. " +
                "Fica bonito, mas custa FPS — por isso vem desligado.");

            // ---------------- Nevoa ----------------
            DemistRadiusMultiplier = config.Bind(
                "3 - Nevoa", "DemistRadiusMultiplier", 2.0f,
                new ConfigDescription(
                    "Multiplicador do raio que dissipa a nevoa das Mistlands. 1 = vanilla.",
                    new AcceptableValueRange<float>(0.1f, 20f)));

            DemistRadiusAbsolute = config.Bind(
                "3 - Nevoa", "DemistRadiusAbsolute", 0f,
                new ConfigDescription(
                    "Raio fixo em metros, ignorando o multiplicador. 0 = usar o multiplicador.",
                    new AcceptableValueRange<float>(0f, 200f)));

            // ---------------- Escopo / debug ----------------
            NameFilter = config.Bind(
                "4 - Avancado", "NameFilter", "",
                "Vazio = afeta tudo que dissipa nevoa (a wisp carregada e as tochas de wisp). " +
                "Preencha com parte do nome do objeto (ex.: demister_ball) para afetar so ele. " +
                "Ligue VerboseLogging para descobrir os nomes reais no seu jogo.");

            VerboseLogging = config.Bind(
                "4 - Avancado", "VerboseLogging", false,
                "Loga no console cada objeto afetado, com os valores antes e depois. " +
                "Use para descobrir nomes para o NameFilter.");
        }

        internal static bool Active => Enabled != null && Enabled.Value;

        /// <summary>Cor configurada, ou null se o texto for invalido.</summary>
        internal static Color? ParsedColor()
        {
            if (!OverrideColor.Value)
            {
                return null;
            }

            if (ColorUtility.TryParseHtmlString(LightColor.Value, out var color))
            {
                return color;
            }

            Plugin.Log.LogWarning(
                $"LightColor '{LightColor.Value}' nao e uma cor valida. Use formato #RRGGBB. Mantendo a cor original.");
            return null;
        }
    }
}
