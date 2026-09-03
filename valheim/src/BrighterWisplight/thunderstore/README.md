# Brighter Wisplight

The Wisplight pushes back the Mistlands fog, but it barely lights anything — you
still walk in the dark with one hand occupied. This mod makes its light strong
enough to actually see by.

**Light only.** It does not touch the fog, add items, change recipes, or change
colors.

- Configurable brightness and range, with a conservative ceiling
- Optional shadows
- Client-side: not required on the server or on other players

This is a comfort mod, not an advantage. The limits are deliberately tight: it
helps you see where you step, without turning into a floodlight.

## Before / after

Same spot, same night, same Wisplight. Only the light changed.

| Vanilla | With Brighter Wisplight |
| :---: | :---: |
| ![Wisplight in vanilla Valheim: the Mistlands floor is almost black](https://raw.githubusercontent.com/Pass-os/my-game-mods/main/valheim/src/BrighterWisplight/thunderstore/images/vanilla.png) | ![Wisplight with the mod: the same ground, plants and rocks are now readable](https://raw.githubusercontent.com/Pass-os/my-game-mods/main/valheim/src/BrighterWisplight/thunderstore/images/modded.png) |

Note that the **fog is untouched**. The mist-clearing bubble around you is exactly
the vanilla size — the mod never modifies it. What changes is how far you can see
*through* the mist, because the light now reaches further.

## Installation

Install through Thunderstore Mod Manager / r2modman, or copy
`BrighterWisplight.dll` into `BepInEx/plugins`.

## Configuration

`BepInEx/config/com.pass-os.brighterwisplight.cfg`, generated on first launch.
You can edit it while the game is running (F1 with ConfigurationManager) — changes
apply immediately, no restart needed.

| Option | Default | What it does |
| --- | --- | --- |
| `Enabled` | `true` | Master switch. Turning it off restores the original values |
| `IntensityMultiplier` | `1.13` | Brightness. `1` = vanilla. Max `1.5` |
| `RangeMultiplier` | `4.4` | How far the light reaches. `1` = vanilla. Max `5` |
| `CastShadows` | `false` | Cast shadows. Looks good, costs FPS |
| `SkipCreatures` | `true` | Skips wisps attached to creatures (Hugin, Munin, Mistwalker) |
| `NameFilter` | empty | Empty = affects the carried wisp **and** wisp torches. Fill in to narrow it down |
| `VerboseLogging` | `false` | Logs affected objects with before/after values |

### Why range goes up more than brightness

The wisp's light is a point light. Raising the **intensity** blows out the image
long before it solves anything — you get a glare in the middle of the screen and
everything else stays dark. Raising the **range** is what actually lets you see
where you are stepping. That is why the defaults barely touch intensity and are
generous with range.

If you want a torch feel instead (strong light, close in, with a fast falloff),
go the other way: drop `RangeMultiplier` to around `1.5` and raise
`IntensityMultiplier`.

### Why you cannot change the color

Earlier versions tried. It does not work, and the reason is worth explaining.

The wisp orb glows through **HDR emission**, and bloom turns any emission with
all three color channels above 1 into white. The original color escapes this by
accident: the wisp's emission is `(0, 3.614, 5.340)` — the red channel is
**zero**. It gets to be very bright and stay blue precisely because one channel
is dark.

Any normal color has all three channels alive. To keep the same brightness they
all have to be high — and then you get a white blob around the wisp. No amount of
tuning fixes it: the problem is the effect, not the value. So the mod does not
try.

### I only want my carried wisp affected, not my torches

Turn on `VerboseLogging`, load into the game, and check `BepInEx/LogOutput.log`.
It lists the name of every affected object. Copy the name into `NameFilter` — the
equipped wisp is `Demister`, wisp torches are `_enabled`.

## Compatibility

The mod uses the `Demister` component only to **locate** wisps — it never
modifies it. It coexists cleanly with fog mods such as
[MistBeGone](https://thunderstore.io/c/valheim/p/Azumatt/MistBeGone/), which
patches `MistEmitter` and `ParticleMist`, classes this mod never touches.

It does not touch networking, prefabs, or recipes.
