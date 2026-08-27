# Brighter Wisplight

A Wisplight ja afasta a nevoa das Mistlands, mas ilumina quase nada — voce
continua andando no escuro com uma das duas maos ocupadas. Este mod transforma
ela numa tocha de verdade, e deixa voce escolher o quanto.

- Brilho e alcance da luz configuraveis
- Raio de dissipacao da nevoa configuravel
- Cor da luz opcional (padrao: mantem o ciano original)
- Sombras opcionais
- Client-side: nao precisa estar no servidor, nao precisa que os outros tenham

## Instalacao

Instale pelo Thunderstore Mod Manager / r2modman, ou copie
`BrighterWisplight.dll` para `BepInEx/plugins`.

## Configuracao

`BepInEx/config/com.pass-os.brighterwisplight.cfg`, gerado no primeiro boot.
Da para editar com o jogo aberto (F1 no ConfigurationManager) — as mudancas
valem na hora, sem reiniciar.

| Opcao | Padrao | O que faz |
| --- | --- | --- |
| `Enabled` | `true` | Liga/desliga tudo. Desligar restaura os valores originais |
| `IntensityMultiplier` | `3.0` | Brilho. `1` = vanilla |
| `RangeMultiplier` | `3.0` | Quao longe a luz alcanca. `1` = vanilla |
| `OverrideColor` | `false` | Trocar a cor da luz |
| `LightColor` | `#FFD9A0` | Cor, se `OverrideColor` estiver ligado |
| `CastShadows` | `false` | Projetar sombras. Bonito, mas custa FPS |
| `DemistRadiusMultiplier` | `2.0` | Raio que afasta a nevoa. `1` = vanilla |
| `DemistRadiusAbsolute` | `0` | Raio fixo em metros. `0` = usar o multiplicador |
| `NameFilter` | vazio | Vazio = afeta a wisp carregada **e** as tochas de wisp. Preencha para restringir |
| `VerboseLogging` | `false` | Loga os objetos afetados e os valores antes/depois |

### Quero afetar so a wisp que eu carrego, nao minhas tochas

Ligue `VerboseLogging`, entre no jogo e olhe o `BepInEx/LogOutput.log`. Ele
lista o nome de cada objeto afetado. Copie o nome da wisp carregada para
`NameFilter`.

## Compatibilidade

Mexe apenas em componentes de objetos ja existentes, em `Demister.Awake`.
Nao adiciona prefab, nao altera receita e nao toca em rede — entao convive bem
com mods de conteudo e nao gera dessync em multiplayer.
