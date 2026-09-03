# Brighter Wisplight

A Wisplight afasta a nevoa das Mistlands, mas ilumina quase nada — voce continua
andando no escuro com uma das maos ocupadas. Este mod deixa a luz dela forte o
suficiente para enxergar de verdade.

**So mexe em luz.** Nao altera a nevoa, nao adiciona item, nao muda receita, nao
muda cor.

- Brilho e alcance configuraveis, com teto conservador
- Sombras opcionais
- Client-side: nao precisa estar no servidor nem nos outros jogadores

E um mod de conforto, nao de vantagem. Os limites sao apertados de proposito:
ele ajuda a enxergar onde voce pisa, sem virar holofote.

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
| `IntensityMultiplier` | `1.13` | Brilho. `1` = vanilla. Maximo `1.5` |
| `RangeMultiplier` | `4.4` | Quao longe a luz alcanca. `1` = vanilla. Maximo `5` |
| `CastShadows` | `false` | Projetar sombras. Bonito, mas custa FPS |
| `SkipCreatures` | `true` | Ignora wisps presas a criaturas (Hugin, Munin, Mistwalker) |
| `NameFilter` | vazio | Vazio = afeta a wisp carregada **e** as tochas de wisp. Preencha para restringir |
| `VerboseLogging` | `false` | Loga os objetos afetados e os valores antes/depois |

### Por que o alcance sobe mais que o brilho

A luz da wisp e um point light. Aumentar a **intensidade** estoura a imagem
muito antes de resolver o problema — fica um clarao no meio da tela e o resto
segue escuro. Aumentar o **alcance** e o que de fato deixa voce enxergar onde
pisa. Por isso o padrao mal encosta na intensidade e e generoso no alcance.

Se voce quer sensacao de tocha (luz forte e perto, com queda rapida), va no
caminho contrario: baixe o `RangeMultiplier` para perto de `1.5` e suba o
`IntensityMultiplier`.

### Por que nao da para mudar a cor

Versoes anteriores tentavam. Nao funciona, e vale explicar o porque.

O orbe da wisp brilha por **emissao HDR**, e o bloom transforma em branco
qualquer emissao com os tres canais de cor acima de 1. A cor original escapa
disso por acidente: a emissao da wisp e `(0, 3.614, 5.340)` — o canal vermelho e
**zero**. Ela consegue ser muito brilhante e continuar azul porque um canal fica
apagado.

Qualquer cor normal tem os tres canais vivos. Para manter o mesmo brilho, todos
precisam ficar altos — e ai vira um borrao branco em volta da wisp. Nao ha
ajuste de numero que resolva: e o efeito, nao o valor. Entao o mod nao tenta.

### Quero afetar so a wisp que carrego, nao minhas tochas

Ligue `VerboseLogging`, entre no jogo e olhe o `BepInEx/LogOutput.log`. Ele
lista o nome de cada objeto afetado. Copie o nome para `NameFilter` — a wisp
equipada e `Demister`, as tochas de wisp sao `_enabled`.

## Compatibilidade

O mod usa o componente `Demister` apenas para **localizar** as wisps — nao
altera nada nele. Combina sem conflito com mods de nevoa como o
[MistBeGone](https://thunderstore.io/c/valheim/p/Azumatt/MistBeGone/), que
patcheia `MistEmitter` e `ParticleMist`, classes que este mod nunca toca.

Nao mexe em rede, prefab nem receita.
