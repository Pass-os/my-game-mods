# Changelog

## 1.2.1

Documentation only — no code changes.

- Added before/after screenshots to the readme, side by side.
- Spelled out that the fog is untouched: the mist-clearing bubble is vanilla
  sized, only the light reach changed.
- `website_url` now points at the mod's folder instead of the monorepo root.

## 1.2.0

The first version that actually works. Earlier ones never changed the light at
all — see "Fixed" below.

### Removed

- **Color override** (`OverrideColor`, `LightColor`, `ColorAffectsLight`,
  `ColorAffectsOrb`). The orb glows through HDR emission, and bloom turns any
  emission with all three channels above 1 into white. The original color only
  escapes this because its red channel is zero. Recoloring without blowing out
  the effect is not possible, so the feature was removed rather than left broken.

### Fixed

- **The mod did nothing.** The `Demister` component does not sit on the wisp
  object: it sits on a child named "Particle System Force Field", which only
  carries the fog force field. The search for lights started there and went
  downward, so it always came back empty. It now walks up until it finds the
  object that actually holds the light.
- **Freeze on world load.** Capture read `renderer.materials`, which CLONES every
  material on the renderer. That ran inside each wisp's `Awake`, and a world
  loads hundreds of them at once. With color gone, the mod no longer touches
  materials at all.
- **Multiplier compounding on re-equip.** The original state was keyed by the
  `Demister`, which dies and respawns when you unequip and re-equip, while the
  lights survive. The new object re-captured already-modified values and treated
  them as factory defaults. State is now keyed by the visual root.
- **`NameFilter` never filtered anything.** It tested the name of the
  `Demister`'s object, which is "Particle System Force Field" on every wisp in
  the game. It now tests the visual root's name.

### Changed

- Defaults: `IntensityMultiplier` `1.127699`, `RangeMultiplier` `4.397653`.
- Tighter ceilings: intensity max `1.5`, range max `5` (both were `20`). A
  comfort mod does not need a ceiling that turns into a gameplay advantage.
- New `SkipCreatures` (on by default): Hugin, Munin and the Mistwalker carry a
  `Demister` and are left alone.
- An exception on one wisp no longer propagates into world loading.

## 1.1.0

- Wisp color applied separately to the projected light and the visible orb.

## 1.0.0

- Configurable brightness and range for the Wisplight.
- Optional light color and shadows.
- Config reloads while the game is running.
