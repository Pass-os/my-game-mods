using BepInEx.Configuration;

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
        internal static ConfigEntry<bool> CastShadows;

        internal static ConfigEntry<bool> SkipCreatures;
        internal static ConfigEntry<string> NameFilter;
        internal static ConfigEntry<bool> VerboseLogging;

        internal static void Init(ConfigFile config)
        {
            Enabled = config.Bind(
                "1 - Geral", "Enabled", true,
                "Liga/desliga o mod inteiro. Desligar restaura os valores originais.");

            // ---------------- Luz ----------------
            // Os padroes e os tetos vieram de teste em jogo. A wisp base tem
            // intensity 1.5 e range 10.
            //
            // Os tetos sao apertados de proposito. Este e um mod de conforto:
            // existe para ajudar a enxergar, nao para virar holofote. Teto largo
            // transforma um ajuste de conforto em vantagem de jogo, e efeito
            // forte demais nao ajuda ninguem. Ficam pouco acima do padrao, so
            // dando margem de gosto.
            IntensityMultiplier = config.Bind(
                "2 - Luz", "IntensityMultiplier", 1.127699f,
                new ConfigDescription(
                    "Multiplicador do brilho da wisp. 1 = vanilla. " +
                    "Teto baixo de proposito: passar muito de 1 estoura a imagem.",
                    new AcceptableValueRange<float>(0.1f, 1.5f)));

            RangeMultiplier = config.Bind(
                "2 - Luz", "RangeMultiplier", 4.397653f,
                new ConfigDescription(
                    "Multiplicador do alcance da luz (quao longe ela ilumina). 1 = vanilla. " +
                    "Aumentar aqui e o jeito seguro de enxergar mais, sem estourar o brilho.",
                    new AcceptableValueRange<float>(0.1f, 5f)));

            CastShadows = config.Bind(
                "2 - Luz", "CastShadows", false,
                "Faz a wisp projetar sombras, como uma tocha de verdade. " +
                "Fica bonito, mas custa FPS — por isso vem desligado.");

            // ---------------- Escopo / debug ----------------
            SkipCreatures = config.Bind(
                "3 - Avancado", "SkipCreatures", true,
                "Nao mexe em wisps presas a criaturas. Hugin, Munin e a Mistwalker " +
                "carregam Demister; sem isto, a luz deles muda junto.");

            NameFilter = config.Bind(
                "3 - Avancado", "NameFilter", "",
                "Vazio = afeta a wisp carregada e as tochas de wisp. " +
                "Preencha com parte do nome do objeto para afetar so ele. Nomes reais: " +
                "'Demister' (wisp equipada), '_enabled' (tocha de wisp), 'Mistwalker'. " +
                "Ligue VerboseLogging para descobrir os nomes reais no seu jogo.");

            VerboseLogging = config.Bind(
                "3 - Avancado", "VerboseLogging", false,
                "Loga no console cada objeto afetado, com os valores antes e depois. " +
                "Use para descobrir nomes para o NameFilter.");
        }

        internal static bool Active => Enabled != null && Enabled.Value;
    }
}
