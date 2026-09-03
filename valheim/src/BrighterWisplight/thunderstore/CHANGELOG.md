# Changelog

## 1.2.0

Primeira versao que funciona de verdade. As anteriores nao chegavam a alterar a
luz — ver "corrigido" abaixo.

### Removido

- **Troca de cor** (`OverrideColor`, `LightColor`, `ColorAffectsLight`,
  `ColorAffectsOrb`). O orbe brilha por emissao HDR e o bloom transforma em
  branco qualquer emissao com os tres canais acima de 1. A cor original so
  escapa disso porque tem o canal vermelho zerado. Recolorir sem estourar o
  efeito nao e possivel, entao o recurso saiu em vez de ficar quebrado.

### Corrigido

- **O mod nao alterava nada.** O componente `Demister` nao fica no objeto da
  wisp: fica num filho chamado "Particle System Force Field", que so carrega o
  force field da nevoa. A busca por luzes partia dali e para baixo, entao voltava
  sempre vazia. Agora sobe ate achar o objeto que realmente tem a luz.
- **Travamento ao carregar o mundo.** A captura lia `renderer.materials`, que
  CLONA cada material do renderer. Isso rodava no `Awake` de cada wisp, e o mundo
  carrega centenas de uma vez. Sem troca de cor, o mod nao toca mais em material
  nenhum.
- **Multiplicador acumulando ao re-equipar.** O estado original era indexado pelo
  `Demister`, que morre e renasce ao desequipar/equipar, enquanto as luzes
  sobrevivem. O objeto novo recapturava valores ja alterados e os tratava como
  "de fabrica". Agora o estado e indexado pela raiz visual.
- **`NameFilter` nunca filtrou nada.** Ele testava o nome do objeto do
  `Demister`, que e "Particle System Force Field" em toda wisp do jogo. Agora
  testa o nome da raiz visual.

### Mudado

- Padroes: `IntensityMultiplier` `1.127699`, `RangeMultiplier` `4.397653`.
- Tetos apertados: intensidade maximo `1.5`, alcance maximo `5` (eram `20`).
  Mod de conforto nao precisa de teto que vira vantagem de jogo.
- Novo `SkipCreatures` (ligado): Hugin, Munin e a Mistwalker carregam `Demister`
  e ficam de fora por padrao.
- Excecao em uma wisp nao sobe mais para o carregamento do mundo.

## 1.1.0

- Cor da wisp aplicavel separadamente a luz projetada e ao orbe visivel.

## 1.0.0

- Brilho e alcance da luz da Wisplight configuraveis.
- Cor da luz e sombras opcionais.
- Config recarrega com o jogo aberto.
