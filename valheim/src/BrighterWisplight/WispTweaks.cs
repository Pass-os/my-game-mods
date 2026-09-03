using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BrighterWisplight
{
    /// <summary>
    /// Deixa a luz das wisps mais forte. So mexe em intensidade e alcance —
    /// nada de nevoa, nada de cor.
    ///
    /// O <see cref="Demister"/> e usado apenas como localizador: e o componente
    /// que marca "isto e uma wisp". Nada nele e alterado, entao o mod convive
    /// com qualquer mod de nevoa (o MistBeGone, por exemplo, patcheia
    /// MistEmitter e ParticleMist, e nunca toca em Demister).
    ///
    /// Tres coisas ditam a implementacao:
    ///
    /// 1. O Demister NAO fica no objeto da wisp. Ele fica num filho pelado
    ///    chamado "Particle System Force Field", que so carrega o
    ///    ParticleSystemForceField da nevoa. A luz e IRMA dele. Procurar com
    ///    GetComponentsInChildren a partir do Demister volta sempre vazio.
    ///    Ver <see cref="FindVisualRoot"/>.
    ///
    /// 2. Escrever em light.intensity/range NAO gruda. LightFlicker reescreve
    ///    intensity todo frame a partir do campo privado m_baseIntensity, e
    ///    LightLod reescreve range a partir de m_baseRange — capturados no
    ///    Awake deles. Entao alteramos os campos-base tambem. Como a ordem de
    ///    Awake entre GameObjects e indefinida na Unity, escrevemos nos dois
    ///    lugares. Por isso a captura acontece no Awake/OnEnable, e nao um frame
    ///    depois: adiar leria valores ja modulados.
    ///
    /// 3. Como o item 2 nos prende ao Awake/OnEnable, tudo aqui roda DENTRO do
    ///    carregamento do mundo, uma vez por wisp — e o mundo carrega centenas
    ///    delas de uma vez (cada tocha de wisp tem um Demister). Entao nada de
    ///    caro pode acontecer aqui. Uma versao anterior lia renderer.materials,
    ///    que CLONA cada material, e isso travava o carregamento do mundo.
    ///
    /// Por que nao ha troca de cor: o orbe brilha por emissao HDR, e o bloom
    /// transforma em BRANCO qualquer emissao com os tres canais acima de 1. A
    /// cor original escapa disso porque tem o canal vermelho zerado — a emissao
    /// da wisp e (0, 3.614, 5.340). Qualquer cor "normal", com os tres canais
    /// vivos, precisa de todos eles altos para manter o mesmo brilho, e ai vira
    /// um borrao branco. Recolorir sem estourar o efeito nao da, entao o mod nao
    /// tenta: ele cuida de iluminacao, nao de pintura.
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
            internal LightShadows BaseShadows;
        }

        private sealed class WispState
        {
            internal List<LightState> Lights = new List<LightState>();

            /// <summary>Objeto onde a luz foi de fato encontrada. So para log.</summary>
            internal string RootName;
        }

        // Guarda os valores de fabrica para que reaplicar a config nao acumule
        // multiplicadores. ConditionalWeakTable nao segura o objeto vivo, entao
        // wisps destruidas somem daqui sozinhas.
        //
        // A chave e a RAIZ VISUAL, nao o Demister. Isso importa: quem morre e
        // renasce ao desequipar/equipar e o Demister, enquanto as luzes podem
        // sobreviver. Indexando pelo Demister, o objeto novo recapturava valores
        // QUE NOS JA TINHAMOS ALTERADO e passava a trata-los como "de fabrica",
        // acumulando o multiplicador a cada re-equipe. Indexando pela raiz
        // visual, o estado vive exatamente o tempo dos objetos que descreve.
        private static readonly ConditionalWeakTable<GameObject, WispState> Originals =
            new ConditionalWeakTable<GameObject, WispState>();

        /// <summary>
        /// Quantos niveis subir procurando a luz. O caso conhecido precisa de 1
        /// (o pai). A folga cobre variacoes de prefab sem virar busca infinita.
        /// </summary>
        private const int MaxClimb = 3;

        internal static void Apply(Demister demister)
        {
            if (demister == null)
            {
                return;
            }

            // Isto roda dentro do Awake/OnEnable do jogo. Uma excecao aqui subiria
            // para o carregamento do mundo; melhor logar e deixar a wisp em paz.
            try
            {
                var root = FindVisualRoot(demister);
                if (root == null)
                {
                    // Nao e erro: o jogo pode ter Demisters sem visual nenhum.
                    if (ModConfig.VerboseLogging.Value)
                    {
                        Plugin.Log.LogInfo(
                            $"Nenhuma luz encontrada a partir de '{demister.gameObject.name}' " +
                            $"(subindo ate {MaxClimb} niveis). Ignorando.");
                    }

                    return;
                }

                if (!MatchesFilter(root))
                {
                    return;
                }

                if (ModConfig.SkipCreatures.Value && IsCreature(root))
                {
                    return;
                }

                var state = Originals.GetValue(root.gameObject, _ => Capture(root));

                if (!ModConfig.Active)
                {
                    Restore(state);
                    return;
                }

                ApplyLights(state);

                if (ModConfig.VerboseLogging.Value)
                {
                    LogState(demister, state);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Falha ao aplicar em '{demister.name}': {ex}");
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

        /// <summary>
        /// Acha o objeto que de fato carrega a luz. Sobe ate achar um ancestral
        /// que contenha algum Light. Duas travas: no maximo <see cref="MaxClimb"/>
        /// niveis, e nunca atravessar um Character — sem isso a wisp equipada
        /// subiria ate o Player e pegaria a tocha da mao junto.
        /// </summary>
        private static Transform FindVisualRoot(Demister demister)
        {
            var current = demister.transform;

            for (var level = 0; level <= MaxClimb && current != null; level++)
            {
                if (current.GetComponentsInChildren<Light>(includeInactive: true).Length > 0)
                {
                    return current;
                }

                var parent = current.parent;
                if (parent == null || parent.GetComponent<Character>() != null)
                {
                    break;
                }

                current = parent;
            }

            return null;
        }

        private static WispState Capture(Transform root)
        {
            var state = new WispState { RootName = root.name };

            foreach (var light in root.GetComponentsInChildren<Light>(includeInactive: true))
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
                    BaseShadows = light.shadows,
                });
            }

            return state;
        }

        private static void ApplyLights(WispState state)
        {
            var intensityMultiplier = ModConfig.IntensityMultiplier.Value;
            var rangeMultiplier = ModConfig.RangeMultiplier.Value;

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

                light.Light.shadows = ModConfig.CastShadows.Value
                    ? LightShadows.Soft
                    : light.BaseShadows;
            }
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
        }

        /// <summary>
        /// O filtro testa a RAIZ VISUAL, nao o Demister. O objeto do Demister se
        /// chama sempre "Particle System Force Field", em toda wisp do jogo —
        /// filtrar por ele nunca distinguiu nada. Os nomes uteis estao na raiz:
        /// "Demister(Clone)" (wisp equipada), "_enabled" (tocha de wisp),
        /// "Hugin", "Munin", "Mistwalker".
        /// </summary>
        private static bool MatchesFilter(Transform root)
        {
            var filter = ModConfig.NameFilter.Value;
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }

            return root.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Criaturas tambem carregam Demister — Hugin e Munin sao os casos
        /// visiveis. Deixar a luz deles mais forte nao e o proposito do mod,
        /// entao por padrao ficam de fora.
        /// </summary>
        private static bool IsCreature(Transform root)
        {
            return root.GetComponentInParent<Character>() != null
                || root.GetComponentInParent<Raven>() != null
                || root.GetComponentInParent<BaseAI>() != null;
        }

        private static void LogState(Demister demister, WispState state)
        {
            Plugin.Log.LogInfo(
                $"Wisp '{demister.gameObject.name}' -> raiz visual '{state.RootName}': " +
                $"{state.Lights.Count} luz(es).");

            foreach (var light in state.Lights)
            {
                if (light.Light == null)
                {
                    continue;
                }

                Plugin.Log.LogInfo(
                    $"    luz '{light.Light.name}': " +
                    $"intensidade {light.BaseIntensity:0.##} -> {light.Light.intensity:0.##}, " +
                    $"alcance {light.BaseRange:0.##} -> {light.Light.range:0.##}");
            }
        }
    }
}
