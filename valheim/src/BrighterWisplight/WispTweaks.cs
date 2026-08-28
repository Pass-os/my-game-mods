using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BrighterWisplight
{
    /// <summary>
    /// Deixa a luz das wisps mais forte e, opcionalmente, muda a cor.
    /// So mexe em luz e visual — nada de nevoa.
    ///
    /// O <see cref="Demister"/> e usado apenas como localizador: e o componente
    /// que marca "isto e uma wisp". Nada nele e alterado, entao o mod convive
    /// com qualquer mod de nevoa (o MistBeGone, por exemplo, patcheia
    /// MistEmitter e ParticleMist, e nunca toca em Demister).
    ///
    /// Duas coisas ditam a implementacao:
    ///
    /// 1. Escrever em light.intensity/range NAO gruda. LightFlicker reescreve
    ///    intensity todo frame a partir do campo privado m_baseIntensity, e
    ///    LightLod reescreve range a partir de m_baseRange — capturados no
    ///    Awake deles. Entao alteramos os campos-base tambem. Como a ordem de
    ///    Awake entre GameObjects e indefinida na Unity, escrevemos nos dois
    ///    lugares.
    ///
    /// 2. A LUZ e o ORBE sao objetos diferentes. O Light projeta cor no
    ///    ambiente; a bolinha visivel e Renderer/ParticleSystem com material
    ///    proprio. Mudar um nao muda o outro.
    /// </summary>
    internal static class WispTweaks
    {
        // Propriedades de cor que aparecem nos shaders usados pelo jogo.
        // Testamos quais existem em vez de assumir uma.
        private static readonly string[] ColorProperties =
        {
            "_Color", "_TintColor", "_EmissionColor", "_BaseColor",
        };

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

        private sealed class MaterialState
        {
            internal Material Material;
            internal string Property;
            internal Color BaseColor;
        }

        private sealed class ParticleState
        {
            internal ParticleSystem System;
            internal ParticleSystem.MinMaxGradient BaseStartColor;
        }

        private sealed class WispState
        {
            internal List<LightState> Lights = new List<LightState>();
            internal List<MaterialState> Materials = new List<MaterialState>();
            internal List<ParticleState> Particles = new List<ParticleState>();
        }

        // Guarda os valores de fabrica para que reaplicar a config nao acumule
        // multiplicadores. ConditionalWeakTable nao segura o objeto vivo, entao
        // wisps destruidas somem daqui sozinhas.
        private static readonly ConditionalWeakTable<Demister, WispState> Originals =
            new ConditionalWeakTable<Demister, WispState>();

        internal static void Apply(Demister demister)
        {
            if (demister == null || !MatchesFilter(demister))
            {
                return;
            }

            var state = Originals.GetValue(demister, Capture);

            if (!ModConfig.Active)
            {
                Restore(state);
                return;
            }

            ApplyLights(state);
            ApplyOrbColor(state);

            if (ModConfig.VerboseLogging.Value)
            {
                LogState(demister, state);
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

        private static WispState Capture(Demister demister)
        {
            var state = new WispState();

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

            foreach (var renderer in demister.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                // .materials (nao .sharedMaterials) devolve copias por instancia.
                // Usar sharedMaterials alteraria o asset do jogo para TODAS as
                // wisps e continuaria valendo depois de desligar o mod.
                foreach (var material in renderer.materials)
                {
                    if (material == null)
                    {
                        continue;
                    }

                    foreach (var property in ColorProperties)
                    {
                        if (!material.HasProperty(property))
                        {
                            continue;
                        }

                        state.Materials.Add(new MaterialState
                        {
                            Material = material,
                            Property = property,
                            BaseColor = material.GetColor(property),
                        });
                    }
                }
            }

            foreach (var particles in demister.GetComponentsInChildren<ParticleSystem>(includeInactive: true))
            {
                state.Particles.Add(new ParticleState
                {
                    System = particles,
                    BaseStartColor = particles.main.startColor,
                });
            }

            return state;
        }

        private static void ApplyLights(WispState state)
        {
            var intensityMultiplier = ModConfig.IntensityMultiplier.Value;
            var rangeMultiplier = ModConfig.RangeMultiplier.Value;
            var color = ModConfig.ParsedColor();
            var tintLight = color.HasValue && ModConfig.ColorAffectsLight.Value;

            foreach (var light in state.Lights)
            {
                if (light.Light == null)
                {
                    continue;
                }

                var intensity = light.BaseIntensity * intensityMultiplier;
                var range = light.BaseRange * rangeMultiplier;

                light.Light.intensity = intensity;
                light.Light.range = range;

                // Sem isto, o proximo frame do LightFlicker/LightLod desfaz tudo.
                if (light.Flicker != null)
                {
                    light.Flicker.m_baseIntensity = intensity;
                }

                if (light.Lod != null)
                {
                    light.Lod.m_baseRange = range;
                }

                light.Light.color = tintLight ? color.Value : light.BaseColor;
                light.Light.shadows = ModConfig.CastShadows.Value
                    ? LightShadows.Soft
                    : light.BaseShadows;
            }
        }

        private static void ApplyOrbColor(WispState state)
        {
            var color = ModConfig.ParsedColor();
            var tint = color.HasValue && ModConfig.ColorAffectsOrb.Value;

            foreach (var material in state.Materials)
            {
                if (material.Material == null)
                {
                    continue;
                }

                var target = tint ? color.Value : material.BaseColor;

                // Emissao costuma passar de 1.0 para brilhar (HDR). Preservamos
                // a intensidade original em vez de achatar para a cor crua.
                if (material.Property == "_EmissionColor" && tint)
                {
                    target *= EmissionScale(material.BaseColor);
                }

                material.Material.SetColor(material.Property, target);
            }

            foreach (var particles in state.Particles)
            {
                if (particles.System == null)
                {
                    continue;
                }

                var main = particles.System.main;
                main.startColor = tint
                    ? new ParticleSystem.MinMaxGradient(color.Value)
                    : particles.BaseStartColor;
            }
        }

        /// <summary>
        /// Quao "forte" era a emissao original. Uma cor HDR pode ter canais
        /// acima de 1; sem isto, trocar a cor apagaria o brilho do orbe.
        /// </summary>
        private static float EmissionScale(Color original)
        {
            var peak = Mathf.Max(original.r, Mathf.Max(original.g, original.b));
            return peak > 1f ? peak : 1f;
        }

        private static void Restore(WispState state)
        {
            foreach (var light in state.Lights)
            {
                if (light.Light == null)
                {
                    continue;
                }

                light.Light.intensity = light.BaseIntensity;
                light.Light.range = light.BaseRange;
                light.Light.color = light.BaseColor;
                light.Light.shadows = light.BaseShadows;

                if (light.Flicker != null)
                {
                    light.Flicker.m_baseIntensity = light.BaseIntensity;
                }

                if (light.Lod != null)
                {
                    light.Lod.m_baseRange = light.BaseRange;
                }
            }

            foreach (var material in state.Materials)
            {
                if (material.Material != null)
                {
                    material.Material.SetColor(material.Property, material.BaseColor);
                }
            }

            foreach (var particles in state.Particles)
            {
                if (particles.System == null)
                {
                    continue;
                }

                var main = particles.System.main;
                main.startColor = particles.BaseStartColor;
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

        private static void LogState(Demister demister, WispState state)
        {
            Plugin.Log.LogInfo(
                $"Wisp '{demister.gameObject.name}': {state.Lights.Count} luz(es), " +
                $"{state.Materials.Count} propriedade(s) de cor, " +
                $"{state.Particles.Count} sistema(s) de particula.");

            foreach (var light in state.Lights)
            {
                if (light.Light == null)
                {
                    continue;
                }

                Plugin.Log.LogInfo(
                    $"    luz '{light.Light.name}': " +
                    $"intensidade {light.BaseIntensity:0.##} -> {light.Light.intensity:0.##}, " +
                    $"alcance {light.BaseRange:0.##} -> {light.Light.range:0.##}, " +
                    $"cor {light.BaseColor} -> {light.Light.color}");
            }

            // Estes nomes sao o que voce precisa se a cor do orbe nao pegar:
            // dizem qual shader e qual propriedade o jogo realmente usa.
            foreach (var material in state.Materials)
            {
                if (material.Material == null)
                {
                    continue;
                }

                Plugin.Log.LogInfo(
                    $"    material '{material.Material.name}' " +
                    $"(shader {material.Material.shader?.name}) " +
                    $"{material.Property}: {material.BaseColor} -> " +
                    $"{material.Material.GetColor(material.Property)}");
            }

            foreach (var particles in state.Particles)
            {
                if (particles.System != null)
                {
                    Plugin.Log.LogInfo($"    particulas '{particles.System.name}'");
                }
            }
        }
    }
}
