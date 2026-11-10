# Changelog — Tiny Questers: Top-Down RPG Character Bundle Vol.1

All notable changes to this package are documented here.

## [1.2.0] — 2026-08-30
Brings the package in line with the latest release of the art set: every sheet
was re-checked frame-by-frame against the source, the motions that were only in
the source set are now here, and the complete raw art set ships alongside the
ready-to-use sheets.

### Added
- **`SourceArt/` — the complete raw art set, 9,524 files (21.0 MB).** Everything
  the standalone release of this art ships with is now in the package: every
  animation frame as its own PNG (3,838), the same frames padded to
  power-of-two (3,838), trimmed per-clip atlases (443), power-of-two atlases
  (443), 1× and 5× animated **GIF previews** (890), and the loose item / FX /
  icon / sign art (76). Packs drawn at two speeds (Elf Archer, Monster Second
  Pack) carry a full `smooth/` and `classic/` set of each.
  Nothing in the package references `SourceArt/` — it is there so you can
  re-slice an atlas your own way, pull a single frame as a still, or take the
  PNGs to another engine. **Delete the folder and everything keeps working.**
  (GIFs are previews for viewing outside Unity; Unity imports a GIF as a still
  first frame and cannot play it.)
- **Victory (win) poses**, in both facings, for the **Warrior** (10 frames
  each), **Mage** (8), **Elf Archer** (16) and the **Guard** (8) — 8 new clips.
- **Guard spear-thrust attack**, left and right (15 frames each, 128×64 cells) —
  the first NPC with an attack. `TQ_Guard` now carries idle, walk, attack and a
  win pose.
- **Classic-timing side idles for all 8 NPCs** — a 4-frame, 150 ms variant of
  each hand-drawn `idle_left` / `idle_right` (the Merchant also gets classic
  versions of its two "holding an item" idles), 18 new clips. Use the classic
  pair when you want the NPC's side pose to breathe at the same rate as its
  front and back idles; the existing 8-frame smooth pair is untouched and stays
  the default in the prefabs.
- **Lizard `teleport` is now the full 34-frame take** (was 30 — the last four
  frames of the effect were missing).

All new clips are registered as animator states on their character's controller
and listed in the demo character assets, so they show up in `TQ_Showcase` and
`TQ_Demo` without any setup.

### Fixed
- **The NPC side idles were playing at half speed.** `idle_left` /
  `idle_right` (and the Merchant's two `_grap` idles) are 8-frame clips drawn to
  run at ~75 ms/frame, but shipped at 150 ms/frame — the rate used by the
  NPCs' 4-frame front/back idles — so the hand-drawn side poses drifted along at
  half the intended speed. All 18 clips now play at **13.33 fps**, matching the
  source art. Front/back idles and every walk clip are unchanged at 6.67 fps.
- **Five clips were playing nothing, or missing frames.** Found by importing the
  package into a clean project and walking every clip's sprite curve:
  - `TQ_Skeleton_Walk_Right` and `TQ_Slime_Walk_Up` had **no sprite curve at
    all** — the states existed and the art was fine, but playing them showed a
    frozen frame. Rebuilt from their sheets (4 and 5 frames).
  - `TQ_WarriorModular_Hit`, `TQ_WarriorModular_Idle_Down` and
    `TQ_WarriorModular_Idle_Up` carried **more keys than their sheet has
    frames** (5 vs 3, 9 vs 5, 9 vs 5); the surplus keys referenced sprite ids
    that no longer existed, so the character vanished partway through the clip.
    Rebuilt at the correct 3 / 5 / 5 frames. This is the same breakage that
    1.1.1 fixed for the six modular attack and death clips — these three were
    missed then.

  All five now match the source art frame-for-frame. Clip counts are unchanged;
  frame totals move accordingly (Skeleton 75 → 79, Slime 106 → 111,
  Warrior Modular 139 → 129).
- **Importing into a project without uGUI raised 51 compile errors.** The demo
  scripts use `UnityEngine.UI` / `UnityEngine.EventSystems`, and a project that
  does not have `com.unity.ugui` installed (a plain 3D or empty project — 2D
  templates ship it) could not compile them, so the console filled with errors
  even though not one of them touches the art. The demo assembly now declares
  `defineConstraints: TQ_HAS_UGUI`, derived by a version define from the
  presence of the uGUI package: without uGUI the assembly is skipped and the
  package imports clean; with it, everything behaves as before. `ToggleSpriteUI`
  moved from `Scripts/` into `Demo/` so it sits inside that assembly too, and
  the now-empty `Scripts/` folder is gone.
- **The side idles never appeared in `TQ_Demo`.** They existed as clips and
  animator states (so `TQ_Showcase` did show them), but no NPC demo asset listed
  them, so the demo scene's ▲ ▼ motion list skipped straight from the up idle to
  the walks. Every NPC demo asset is now rebuilt from its animator controller,
  so the demo lists exactly what the controller holds, ordered
  idle → walk → attack → win.

### Notes
- `Art/` now holds **407 PNG sheets** (was 379) carrying **3,059 sprites**
  (was 2,869), in **330 animation clips** (was 302).
- **Corrected sprite count.** Earlier documentation listed 2,892 sprites for
  `Art/`. That figure double-counted 23 leftover sub-rects inside the `.meta`
  files of Single-mode sprites (loose items, icons, signs, FX), which Unity
  imports as one sprite each. Counting the way Unity actually imports them, the
  1.1.1 content was **2,869** sprites and this release is **3,059**. Every
  count in the README and in TECHNICAL_DETAILS was re-derived from the shipped
  files for this release.
- Pivots for the new sheets follow the same rule as 1.1.1 — measured from the
  bottom-most opaque pixel of the sheet's first frame — so the new motions land
  on the same ground line as the rest of the character.

## [1.1.1] — 2026-07-30
### Added
- **Elf Archer now appears in the `TQ_Showcase` scene**, in the Heroes row right
  after Warrior (the third slot on that row was empty, so no other cell moved).
  The cell carries the character, its name label and its motion label, and is
  registered with `ShowcaseController` like every other character.
- Elf Archer also moved to third place in `TQ_Demo`, so both scenes now run
  Warrior → Mage → Elf Archer.
- Demo UI toggle buttons support a **held-down sprite**: `ToggleSpriteUI` now
  shows `toggledOnPressedSprite` / `toggledOffPressedSprite` while the pointer
  is down, falling back to the normal sprite when one is left empty.

### Fixed
- The demo's **Auto button did nothing**: its `ToggleSpriteUI` pointed at the
  Play button's Button and Image instead of its own, so clicking Auto had no
  effect while clicking Play toggled two components at once. Both references now
  point at the Auto object's own components.
- `ToggleSpriteUI` is namespaced (`TinyQuesters.Demo`) and null-guarded — a
  missing target Image logs an error and disables the component instead of
  throwing, a missing Button warns, and the click listener is removed on destroy.
- **Character pivots now put the feet on the ground line.** Every character
  sprite (301 sheets / 2,779 sprites across Heroes, Monsters and NPCs) moved
  from Bottom-Center (0.5, 0) to a **Custom pivot (0.5, y)**, where y is
  measured from the bottom-most opaque pixel of the sheet's first frame and
  shared by every frame of that sheet.
  The art carries transparent padding below the feet (≈16 px on a 64×64 idle
  cell, ≈48 px on a 128×128 attack cell), so the old pivot left characters
  hovering above the ground and — because the padding differs per clip — made
  them jump when switching clips. For example the Warrior rose 24 px
  (0.375 units) the moment an attack started, and the Elf Archer shifted 16 px
  between `idle_down` and `idle_left`.
- The 12 `effect_*` VFX sheets (Mage spells, Bat effect) **inherit their
  matching character sheet's pivot** instead of measuring their own first frame,
  which is a single spark — this keeps the VFX registered to the character.
  Items, icons, signs and FX sprites keep their centered (0.5, 0.5) pivot.
- Restored the slice data and sprite pivots of the four Elf Archer `rolling_*`
  sheets, which had been cleared, and repaired the sprite references of the
  30 Elf Archer animation clips, the Elf Archer prefab, and
  `TQ_Mage_Attack_Fireball_Left`.
- **Rebuilt six long-broken Warrior (Modular) clips** — `Attack_Down/Left/Right/Up`,
  `Die`, `Die_No_Shadow`. Their sheets had been re-sliced without updating the
  clips, so every sprite reference pointed at an ID that no longer existed and
  the clips played nothing. Rebuilt from the matching Warrior clips (same art,
  same sheet layout, same sprite names) by remapping each frame by name.
- **Re-fitted all 20 character prefab colliders.** The `BoxCollider2D` on each
  prefab was authored for the old bottom-of-cell pivot (offset y 0.5, size
  1×1 = the whole cell), which left it floating above the character once the
  pivot moved to the feet. Each box is now derived from the actual alpha bounds
  of the sprite the character shows on spawn (its animator's default state), so
  it fits the body — e.g. Slime 0.28×0.22, Golem 0.61×0.59, Bat 0.86×0.30
  (wingspan). This affects click-picking in the showcase scene.

## [1.1.0] — 2026-07-29
### Added
- **New hero — Elf Archer**: 30 animation clips / 350 frames. Idle ×6
  (including two hand-drawn 3/4 diagonal views), walk ×4, 4-way dodge roll,
  4-direction bow attack, a draw → shoot → recover bow combo, a fast-fire
  attack, hit ×4, and four death variants (fall, mirrored fall, and two
  knock-back deaths). Includes 4 arrow projectile sprites in
  `Art/Heroes/ElfArcher/Items/`.
- Elf Archer animator controller, prefab, and demo entry — the character
  appears in `TQ_Demo` automatically.
- **`TECHNICAL_DETAILS.md`** — full specification: asset counts, image
  dimensions, color depth, transparency, per-character animation list,
  import settings, pipeline and platform support.

### Changed
- **All 21 character prefabs (and the demo scene) now use Unity's built-in
  `Sprites-Default` material** instead of the URP-only `Sprite-Lit-Default`.
  The package now renders correctly in **both Built-in and URP** with no
  setup. The demo scenes use no 2D lights, so the visual result is unchanged.
- README corrected and expanded: character count (20 → 21), accurate cell-size
  table (the package has no 256×256 cells; ~87% of sprites use a square cell),
  updated folder tree, and correct `Bonus/` / `Extras/` folder names.

## [1.0.0] — 2026-07-08
### Added
- Initial Asset Store release.
- Heroes: Warrior, Warrior (Modular + separate weapon/shield/hand items), Mage (+ VFX).
- NPCs: Barmaid, Blacksmith, Female Villager, Guard, Merchant, Old Man,
  Town Girl, Village Man (+ bonus item icons & shop signs).
- Monsters — Starter Pack: Bat, Goblin, Skeleton, Slime.
- Monsters — Second Pack: Orc, Lizard, Mushroom, Golem (mossy & clean),
  plus bonus dust / teleport FX and a ginkgo tree.
- Power-of-two padded sprite sheets (64 / 128 / 256 square cells), pre-sliced
  with feet-center pivots baked into their `.meta`.
- Included animation clips, animator controllers, prefabs, a showcase scene, and
  a one-character demo scene — ready to use out of the box.
- Documentation: README, license, naming conventions, cell-size reference.
