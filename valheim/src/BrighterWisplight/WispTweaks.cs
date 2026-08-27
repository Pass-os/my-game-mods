using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BrighterWisplight
{
    /// <summary>
    /// Ajusta as wisps (qualquer objeto com <see cref="Demister"/>).
    ///
    /// Dois detalhes do jogo ditam como isto funciona:
    ///
    /// 1. O raio que dissipa a nevoa NAO e um campo do Demister. O jogo faz
    ///    Vector3.Distance(demister.position, p) &lt; demister.m_forceField.endRange
    ///    em ParticleMist.InsideDemister. Ou seja: o raio e o endRange do
    ///    ParticleSystemForceField do objeto.
    ///
    /// 2. Escrever em light.intensity / light.range NAO adianta sozinho.
    ///    LightFlicker reescreve intensity todo frame a partir do campo privado
    ///    m_baseIntensity, e LightLod reescreve range a partir de m_baseRange.
    ///    Os dois capturam esses valores no proprio Awake. Entao alteramos os
    ///    campos-base tambem — e por isso o mod publiciza as assemblies.
    ///
    /// A ordem de Awake entre componentes de GameObjects diferentes e indefinida
    /// na Unity, entao escrevemos nos dois lugares: se o Flicker/Lod ainda nao
    /// acordou, ele vai capturar o valor ja multiplicado; se ja acordou,
    /// sobrescrevemos a base dele.
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

        private sealed class OriginalState
        {
            internal bool HasForceField;
            internal float ForceFieldEndRange;
            internal List<LightState> Lights = new List<LightState>();
        }

        // Guarda os valores de fabrica para que reaplicar a config nao acumule
        // multiplicadores. ConditionalWeakTable nao segura o objeto vivo, entao
        // wisps destruidas somem daqui sozinhas.
        private static readonly ConditionalWeakTable<Demister, OriginalState> Originals =
            new ConditionalWeakTable<Demister, OriginalState>();

        internal static void Apply(Demister demister)
        {
            if (demister == null || !MatchesFilter(demister))
            {
                return;
            }

            var original = Originals.GetValue(demister, Capture);

            if (!ModConfig.Active)
            {
                Restore(demister, original);
                return;
            }

            ApplyMist(demister, original);
            ApplyLights(demister, original);

            if (ModConfig.VerboseLogging.Value)
            {
                LogState(demister, original);
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

            // Copia a lista: Apply nao mexe nela, mas o jogo pode mexer.
            foreach (var demister in demisters.ToArray())
            {
                Apply(demister);
            }
        }

        private static OriginalState Capture(Demister demister)
        {
            var state = new OriginalState();

            var field = demister.m_forceField;
            if (field != null)
            {
                state.HasForceField = true;
                state.ForceFieldEndRange = field.endRange;
            }

            foreach (var light in demister.GetComponentsInChildren<Light>(includeInactive: true))
            {
                var flicker = light.GetComponent<LightFlicker>();
                var lod = light.GetComponent<LightLod>();

                state.Lights.Add(new LightState
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

            return state;
        }

        private static void ApplyMist(Demister demister, OriginalState original)
        {
            if (!original.HasForceField)
            {
                return;
            }

            var field = demister.m_forceField;
            if (field == null)
            {
                return;
            }

            var absolute = ModConfig.DemistRadiusAbsolute.Value;
            field.endRange = absolute > 0f
                ? absolute
                : original.ForceFieldEndRange * ModConfig.DemistRadiusMultiplier.Value;
        }

        private static void ApplyLights(Demister demister, OriginalState original)
        {
            var intensityMultiplier = ModConfig.IntensityMultiplier.Value;
            var rangeMultiplier = ModConfig.RangeMultiplier.Value;
            var color = ModConfig.ParsedColor();

            foreach (var state in original.Lights)
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

        private static void Restore(Demister demister, OriginalState original)
        {
            if (original.HasForceField && demister.m_forceField != null)
            {
                demister.m_forceField.endRange = original.ForceFieldEndRange;
            }

            foreach (var state in original.Lights)
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

        private static void LogState(Demister demister, OriginalState original)
        {
            var radius = original.HasForceField && demister.m_forceField != null
                ? $"{original.ForceFieldEndRange:0.##} -> {demister.m_forceField.endRange:0.##}"
                : "sem ParticleSystemForceField";

            Plugin.Log.LogInfo(
                $"Demister '{demister.gameObject.name}': raio da nevoa {radius}, " +
                $"{original.Lights.Count} luz(es).");

            foreach (var state in original.Lights)
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
