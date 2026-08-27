# Brighter Wisplight

A Wisplight afasta a nevoa das Mistlands, mas ilumina quase nada — voce continua
andando no escuro com uma das maos ocupadas. Este mod deixa a luz dela forte o
suficiente para servir de tocha, e voce escolhe o quanto.

**So mexe em luz.** Nao altera a nevoa, nao adiciona item, nao muda receita.

- Brilho e alcance configuraveis
- Cor da luz opcional (padrao: mantem o ciano original)
- Sombras opcionais
- Client-side: nao precisa estar no servidor nem nos outros jogadores

## Instalacao

Instale pelo Thunderstore Mod Manager / r2modman, ou copie
`BrighterWisplight.dll` para `BepInEx/plugins`.

## Configuracao

`BepInEx/config/com.pass-os.brighterwisplight.cfg`, gerado no primeiro boot.
Da para editar com o jogo aberto (F1 no ConfigurationManager) — as mudancas
valem na hora, sem reiniciar.

| Opcao | Padrao | O que faz |
| --- | --- | --- |
| `Enabled` | `true` | Liga/desliga. Desligar restaura os valores originais |
| `IntensityMultiplier` | `3.0` | Brilho. `1` = vanilla |
| `RangeMultiplier` | `3.0` | Quao longe a luz alcanca. `1` = vanilla |
| `OverrideColor` | `false` | Trocar a cor da luz |
| `LightColor` | `#FFD9A0` | Cor, se `OverrideColor` estiver ligado |
| `CastShadows` | `false` | Projetar sombras. Bonito, mas custa FPS |
| `NameFilter` | vazio | Vazio = afeta a wisp carregada **e** as tochas de wisp. Preencha para restringir |
| `VerboseLogging` | `false` | Loga os objetos afetados e os valores antes/depois |

### Quero afetar so a wisp que carrego, nao minhas tochas

Ligue `VerboseLogging`, entre no jogo e olhe o `BepInEx/LogOutput.log`. Ele
lista o nome de cada objeto afetado. Copie o nome da wisp carregada para
`NameFilter`.

## Compatibilidade

O mod usa o componente `Demister` apenas para **localizar** as wisps — nao
altera nada nele. Combina sem conflito com mods de nevoa como o
[MistBeGone](https://thunderstore.io/c/valheim/p/Azumatt/MistBeGone/), que
patcheia `MistEmitter` e `ParticleMist`, classes que este mod nunca toca.

Nao mexe em rede, prefab nem receita.
