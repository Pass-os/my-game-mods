using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BrighterWisplight
{
    /// <summary>
    /// Deixa a luz das wisps mais forte. So luz — o mod nao mexe na nevoa.
    ///
    /// O <see cref="Demister"/> e usado apenas como localizador: e o componente
    /// que marca "isto e uma wisp". Nada nele e alterado, entao o mod convive
    /// com qualquer mod de nevoa (o MistBeGone, por exemplo, patcheia
    /// MistEmitter e ParticleMist, e nunca toca em Demister).
    ///
    /// O detalhe que dita a implementacao: escrever em light.intensity e
    /// light.range NAO gruda. LightFlicker reescreve intensity todo frame a
    /// partir do campo privado m_baseIntensity, e LightLod reescreve range a
    /// partir de m_baseRange — os dois capturados no Awake deles. Entao o mod
    /// altera os campos-base tambem, e por isso publiciza as assemblies.
    ///
    /// Como a ordem de Awake entre GameObjects e indefinida na Unity, o mod
    /// escreve nos dois lugares: se o Flicker/Lod ainda nao acordou, ele vai
    /// capturar o valor ja multiplicado; se ja acordou, sobrescrevemos a base.
    /// </summary>
    internal static class WispTweaks
    {
        private sealed class LightState
        {
            internal Light Light;
            internal LightFlicker Flicker;
            internal LightLod Lod;
            internal float BaseIntensity;
            internal float BaseRange;
            internal Color BaseColor;
            internal LightShadows BaseShadows;
        }

        // Guarda os valores de fabrica para que reaplicar a config nao acumule
        // multiplicadores. ConditionalWeakTable nao segura o objeto vivo, entao
        // wisps destruidas somem daqui sozinhas.
        private static readonly ConditionalWeakTable<Demister, List<LightState>> Originals =
            new ConditionalWeakTable<Demister, List<LightState>>();

        internal static void Apply(Demister demister)
        {
            if (demister == null || !MatchesFilter(demister))
            {
                return;
            }

            var lights = Originals.GetValue(demister, Capture);

            if (!ModConfig.Active)
            {
                Restore(lights);
                return;
            }

            ApplyLights(lights);

            if (ModConfig.VerboseLogging.Value)
            {
                LogState(demister, lights);
            }
        }

        /// <summary>Reaplica em todas as wisps vivas. Usado quando a config muda.</summary>
        internal static void ApplyToAll()
        {
            var demisters = Demister.GetDemisters();
            if (demisters == null)
            {
                return;
            }

            foreach (var demister in demisters.ToArray())
            {
                Apply(demister);
            }
        }

        private static List<LightState> Capture(Demister demister)
        {
            var lights = new List<LightState>();

            foreach (var light in demister.GetComponentsInChildren<Light>(includeInactive: true))
            {
                var flicker = light.GetComponent<LightFlicker>();
                var lod = light.GetComponent<LightLod>();

                lights.Add(new LightState
                {
                    Light = light,
                    Flicker = flicker,
                    Lod = lod,
                    // Se o Flicker/Lod ja acordou, a base verdadeira esta neles
                    // (light.intensity/range ja pode estar modulado neste frame).
                    // Se ainda nao, o valor do prefab esta no proprio Light.
                    BaseIntensity = flicker != null && flicker.m_light != null
                        ? flicker.m_baseIntensity
                        : light.intensity,
                    BaseRange = lod != null && lod.m_light != null
                        ? lod.m_baseRange
                        : light.range,
                    BaseColor = light.color,
                    BaseShadows = light.shadows,
                });
            }

            return lights;
        }

        private static void ApplyLights(List<LightState> lights)
        {
            var intensityMultiplier = ModConfig.IntensityMultiplier.Value;
            var rangeMultiplier = ModConfig.RangeMultiplier.Value;
            var color = ModConfig.ParsedColor();

            foreach (var state in lights)
            {
                if (state.Light == null)
                {
                    continue;
                }

                var intensity = state.BaseIntensity * intensityMultiplier;
                var range = state.BaseRange * rangeMultiplier;

                state.Light.intensity = intensity;
                state.Light.range = range;

                // Sem isto, o proximo frame do LightFlicker/LightLod desfaz tudo.
                if (state.Flicker != null)
                {
                    state.Flicker.m_baseIntensity = intensity;
                }

                if (state.Lod != null)
                {
                    state.Lod.m_baseRange = range;
                }

                state.Light.color = color ?? state.BaseColor;
                state.Light.shadows = ModConfig.CastShadows.Value
                    ? LightShadows.Soft
                    : state.BaseShadows;
            }
        }

        private static void Restore(List<LightState> lights)
        {
            foreach (var state in lights)
            {
                if (state.Light == null)
                {
                    continue;
                }

                state.Light.intensity = state.BaseIntensity;
                state.Light.range = state.BaseRange;
                state.Light.color = state.BaseColor;
                state.Light.shadows = state.BaseShadows;

                if (state.Flicker != null)
                {
                    state.Flicker.m_baseIntensity = state.BaseIntensity;
                }

                if (state.Lod != null)
                {
                    state.Lod.m_baseRange = state.BaseRange;
                }
            }
        }

        private static bool MatchesFilter(Demister demister)
        {
            var filter = ModConfig.NameFilter.Value;
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }

            return demister.gameObject.name.IndexOf(
                filter, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void LogState(Demister demister, List<LightState> lights)
        {
            Plugin.Log.LogInfo(
                $"Wisp '{demister.gameObject.name}': {lights.Count} luz(es).");

            foreach (var state in lights)
            {
                if (state.Light == null)
                {
                    continue;
                }

                Plugin.Log.LogInfo(
                    $"    luz '{state.Light.name}': " +
                    $"intensidade {state.BaseIntensity:0.##} -> {state.Light.intensity:0.##}, " +
                    $"alcance {state.BaseRange:0.##} -> {state.Light.range:0.##}, " +
                    $"flicker={(state.Flicker != null ? "sim" : "nao")}, " +
                    $"lod={(state.Lod != null ? "sim" : "nao")}");
            }
        }
    }
}
