# Technical Details — Tiny Questers: Top-Down RPG Character Bundle Vol.1

This document lists the exact technical specification of every asset in this
package. It mirrors the **Technical details** section of the Asset Store
listing. Every number below was counted from the shipped files.

Last verified: package version **1.2.0**.

---

## 1. Asset counts

| Item | Count |
|------|-------|
| **Unique characters** | **21** (4 Heroes · 8 NPCs · 9 Monsters) |
| **Ready-to-use sheets** (`Art/`) | **407 PNG** → **3,059 sliced sprites** |
| **Raw source art** (`SourceArt/`) | **9,524 files** — 8,634 PNG + 890 GIF |
| **Animation clips (`.anim`)** | **330** (329 character + 1 demo UI) |
| Animator controllers (`.controller`) | 26 (21 character + 5 demo UI) |
| Prefabs (`.prefab`) | 23 (21 character + 2 demo UI) |
| ScriptableObject assets (`.asset`) | 21 (20 demo character data + 1 demo index) |
| Demo C# scripts (`.cs`) | 8 (demo/showcase only — not required at runtime) |
| Scenes (`.unity`) | 2 (showcase + demo) |
| Font | 1 (Pretendard Regular, `.otf`, SIL OFL 1.1 — demo UI only) |
| Documentation | 4 (README, TECHNICAL_DETAILS, CHANGELOG, LICENSE) |
| **Total files** | **10,348** (25.4 MB before `.meta`) |

**No 3D models, meshes, materials, shaders, audio, or video files are
included. LODs: No (2D sprite package).**

### `Art/` vs `SourceArt/`

| | `Art/` | `SourceArt/` |
|---|---|---|
| Purpose | What every clip, prefab and scene runs on | Raw art, exactly as the itch.io release ships it |
| Files | 407 PNG (1.0 MB) | 9,524 files (21.0 MB) |
| Sprite Mode | Multiple (333) · Single (74) | Single (all) |
| Sliced | Yes — grid + feet pivots baked into `.meta` | No |
| Referenced by the package | Yes | **No — safe to delete** |

`SourceArt/` breakdown:

| Kind | Files | Description |
|------|-------|-------------|
| `png/single/` | 3,838 | Every animation frame as its own PNG, trimmed |
| `png/single_pot/` | 3,838 | The same frames padded to power-of-two dimensions |
| `png/atlas/` | 443 | One horizontal strip per clip, trimmed |
| `png/atlas_pot/` | 443 | The same strips padded to power-of-two |
| `gif/1x/`, `gif/5x/` | 886 | Animated previews at 1× and 5×, at the source timing |
| `items/`, `extras/`, `bonus/` | 76 | Modular gear, arrows, FX, icons, shop signs (72 PNG + 4 GIF loops of the FX sets) |
| **Total** | **9,524** | 8,634 PNG + 890 GIF |

Packs whose art has two timings (Elf Archer, Monster Second Pack) carry a full
`smooth/` and `classic/` set of all of the above.

> **GIFs are previews, not playable assets.** Unity imports a GIF as a still
> image (its first frame only) and cannot play it. Use the clips in
> `Animations/` for playback; open the GIFs outside Unity to preview a motion.

---

## 2. Image format specification

All art is hand-drawn pixel art exported as PNG. The only non-PNG images are the
890 GIF previews in `SourceArt/**/gif/`.

| Property | Value |
|----------|-------|
| **File format** | PNG (`.png`) — plus GIF (`.gif`) for previews only |
| **Color depth** | **8 bits per channel — 32-bit RGBA** (100% of PNG files) |
| **Color type** | Truecolor with alpha (PNG color type 6) |
| **Transparency** | **Yes — full 8-bit alpha channel.** Every sprite sits on a fully transparent background. Art itself is hard-edged pixel art (no partial-alpha anti-aliasing on character silhouettes); soft alpha is used only in a few FX sprites. |
| **Interlacing** | None |
| **Embedded color profile** | None (straight sRGB) |

---

## 3. Image dimensions

Each PNG in `Art/` holds all frames of one animation clip, already sliced in its
`.meta`. **279 of the 333 animation sheets are a single horizontal strip**; the
other **54** are re-flowed into a multi-row grid (up to 1024 px wide) so they
stay within the texture size limit — the Warrior's and modular Warrior's attacks
at 2 rows, and most of the Monster Second Pack at 2–6 rows. Both layouts ship
pre-sliced and need no manual work.

**Sheet (file) dimensions in `Art/`**
- Widths: **30 – 2048 px** (30, 32, 48, 64, 128, 256, 384, 448, 512, 569, 576, 640, 896, 1024, 1120, 1280, 1344, 1536, 1736, 1984, 2048)
- Heights: **30 – 1024 px** (30, 32, 48, 64, 80, 128, 161, 256, 320, 384, 1024)
- No sheet exceeds **2048 px** on either axis.

**Frame (cell) dimensions** — 3,059 sprites in `Art/`, across 17 cell sizes

| Cell size | Sprites | Used by |
|-----------|---------|---------|
| **64 × 64** | 1,705 | Base canvas — idle / walk / hit / death / win for most characters |
| **128 × 128** | 827 | Large attacks, Mage magic & fireball VFX, big monster clips |
| **128 × 64** | 192 | Wide Golem clips, Lizard teleport, Guard spear attack, teleport FX |
| **80 × 80** | 140 | Elf Archer idle / walk / death / win |
| **73 × 64** | 40 | Warrior attack extents |
| **101 × 256** | 38 | Lizard power-up / final upgrade |
| **124 × 80** | 28 | Elf Archer knock-back deaths |
| **64 × 32** | 16 | Demo UI buttons |
| **94 × 64** | 13 | Skeleton attack extents |
| **48 × 48** | 13 | Dust FX frames |
| **69 × 100** | 12 | Mage turn-around |
| **64 × 137** | 12 | Bat effect clip |
| **64 × 145** | 12 | Bat effect clip |
| **64 × 128** | 5 | Ginkgo tree / tall FX |
| **32 × 32** | 4 | Small item sprites |
| **569 × 161** | 1 | Demo title graphic |
| **30 × 30** | 1 | Demo UI dot |

**87.9% of all sprites use a square cell.** For those sheets the rule
`cell size = image height`, `frame count = width ÷ height` holds, so *Grid By
Cell Size* slicing works directly.

---

## 4. Import settings (already baked into every `.meta`)

| Setting | `Art/` | `SourceArt/` |
|---------|--------|--------------|
| Texture Type | Sprite (2D and UI) | Sprite (2D and UI) |
| Sprite Mode | **Multiple** (333 animation sheets) · **Single** (74 item / icon / sign / FX / UI files) | Single (all 9,524) |
| **Pixels Per Unit** | **64** — one 64 px cell = 1 Unity unit = 1 tile | 64 |
| Filter Mode | Point (no filter) | Point (no filter) |
| Compression | None | None |
| Generate Mip Maps | Off | Off |
| Alpha Is Transparency | On | On |
| Wrap Mode | Clamp | Clamp |
| Max Size | 4096 (sources are ≤ 2048) | 4096 |

> Exception: the 6 demo UI sprites in `Art/UI/` are interface graphics rather
> than characters, so they use their own Pixels Per Unit (16 / 100).

**Pivot.** Character sprites use **Custom, x = 0.5**. The y is measured from the
**bottom-most opaque pixel of each sheet's first frame** (the character's feet)
and applied to every frame of that sheet, so all clips share one ground line
regardless of canvas size. Frames carry transparent padding below the feet
(typically 16–26 px on a 64×64 idle cell, 48–65 px on a 128×128 attack cell),
which a plain Bottom-Center pivot would turn into a visible hover, and — because
the padding differs per clip — a jump the moment the clip changes. The
12 `effect_*` VFX sheets inherit their matching character sheet's pivot instead
of self-measuring. Items, icons, signs and FX sprites use a centered pivot
(0.5, 0.5).

---

## 5. Animation list

Clips are `AnimationClip` assets driving `SpriteRenderer.sprite` (sprite-swap
frame animation). **No skeletal rigs, no Mecanim humanoid avatars, no bones,
no blend trees.** Each character has one Animator Controller holding every one
of its clips as a separate state.

Naming: `TQ_<Character>_<Action>_<Direction>`
(e.g. `TQ_Warrior_Idle_Down`, `TQ_Mage_Attack_Fireball_Left`, `TQ_Guard_Win_Left`).

Playback rate follows the source art: **13.33 fps** (75 ms/frame) for the Elf
Archer, the Second Pack monsters, every win pose, the Guard's spear attack and
the NPCs' 8-frame smooth side idles; **6.67 fps** (150 ms/frame) for everything
else, including the 4-frame `_classic` side idles. Both loop-time and clip
length are pre-set; idle / walk / turn loop, attack / hit / death / win play
once.

### Heroes (4) — 98 clips / 1,053 frames

| Character | Clips | Frames | Animations included |
|-----------|-------|--------|---------------------|
| **Elf Archer** | 32 | 382 | idle ×6 (down, up, left, right, down-left, down-right 3/4 views) · walk ×4 · rolling dodge ×4 · attack ×4 · attack_fast · bow loading / shot / finish · hit ×4 · die · die_reflect · die_flying_left / _right · **win ×2** |
| **Mage** | 34 | 322 | idle ×2 · walk ×4 · attack_stick ×4 · attack_magic ×4 · attack_fireball ×4 (+2 no-light variants) · base_magic VFX ×4 · fireball VFX ×4 · hit · die (+ no-shadow) · turn360 · **win ×2** |
| **Warrior** | 19 | 220 | idle ×2 · walk ×4 · attack ×4 (+4 "stay" hold variants) · hit · die (+ no-shadow) · **win ×2** |
| **Warrior (Modular)** | 13 | 129 | idle ×2 · walk ×4 · attack ×4 · hit · die (+ no-shadow) — body animated **without gear**; 10 separate weapon/shield/hand sprites in `Items/` |

### NPCs (8) — 88 clips / 497 frames

| Character | Clips | Frames | Extras beyond idle ×6 + walk ×4 |
|-----------|-------|--------|----------------------------------|
| Barmaid | 10 | 48 | — |
| Blacksmith | 10 | 52 | — |
| Female Villager | 10 | 49 | — |
| Guard | 14 | 100 | spear attack ×2 · win ×2 |
| Merchant | 14 | 78 | "holding an item" side idle ×2 (+ classic ×2) |
| Old Man | 10 | 69 | — |
| Town Girl | 10 | 48 | — |
| Village Man | 10 | 53 | — |

Each NPC has **idle ×6 + walk ×4** — idle down / up, plus hand-drawn `idle_left`
and `idle_right` in two timings each (an 8-frame smooth take at 13.33 fps and a
4-frame `_classic` take at 6.67 fps). The prefabs default to the smooth pair;
both are animator states, so swapping is a one-line change.

### Monsters (9) — 143 clips / 1,422 frames

| Character | Clips | Frames | Animations included |
|-----------|-------|--------|---------------------|
| Bat | 16 | 120 | idle ×2 · walk ×4 · attack ×4 · effect ×4 · hit · die |
| Goblin | 15 | 67 | idle ×2 · walk ×4 · attack ×4 · hit ×4 · die |
| Skeleton | 13 | 79 | idle ×2 · walk ×4 · attack ×4 · hit ×2 · die |
| Slime | 17 | 111 | idle ×2 · walk ×5 · attack ×5 · hit ×3 · die ×2 |
| Orc | 16 | 130 | idle ×2 · walk ×4 · attack ×4 · hit ×4 · die ×2 |
| Lizard | 22 | 285 | idle ×7 (incl. eye-blink, power-up) · walk ×4 · attack ×4 · hit ×4 · die ×2 · teleport (34 frames) |
| Mushroom | 18 | 176 | idle ×4 · walk ×4 · attack ×4 · hit ×4 · die ×2 |
| Golem (mossy) | 13 | 228 | idle ×4 · walk ×4 · attack ×4 · die |
| Golem (clean) | 13 | 226 | idle ×4 · walk ×4 · attack ×4 · die |

Demo UI adds 1 further clip (2 frames), for **330 clips / 2,974 keyframes** in
total.

### Extra, non-character art in `Art/` — 78 PNG / 90 sprites

| Set | PNG | Sprites |
|-----|-----|---------|
| Warrior modular items (swords, shields, hands) | 10 | 10 |
| Monster FX & props (dust, lizard teleport FX, ginkgo tree) | 45 | 45 |
| NPC bonus item icons | 6 | 6 |
| NPC bonus shop signs | 7 | 7 |
| Elf Archer arrow projectiles (4 facings) | 4 | 4 |
| Demo UI sprites | 6 | 18 |

---

## 6. Compatibility

| Item | Value |
|------|-------|
| **Built with** | Unity **6000.3.7f1** (Unity 6) |
| **Minimum version** | Unity **2021.3 LTS** or newer |
| **Render pipelines** | **Built-in and URP** — all prefabs use Unity's built-in `Sprites-Default` material, which renders correctly in both. No custom shaders or materials are shipped. |
| **Package dependency** | **None for the art.** Sprites, clips, animator controllers and prefabs need nothing beyond a stock Unity project. The two demo scenes use `com.unity.ugui` for their interface. |
| **Without uGUI installed** | The package still imports cleanly and **produces no compile errors**. The demo assembly declares `defineConstraints: TQ_HAS_UGUI`, which a version define derives from the presence of `com.unity.ugui`, so it simply does not compile when uGUI is absent. Every sprite, clip, controller and character prefab works exactly the same; only the two demo scenes need uGUI to run. Add it from **Window ▸ Package Manager** if you want them. |
| **External dependencies** | **None.** No third-party plugins, no network access, no native libraries. |
| **Supported platforms** | **All Unity build targets** — the package contains only PNG/GIF textures, animation clips, prefabs and plain C# demo scripts. Nothing is platform-specific. |
| **Scripting** | 8 demo-only C# scripts, all inside the `TinyQuesters.Demo` assembly definition. They drive the showcase/demo scenes and are **not** needed to use the art; the folder can be deleted without affecting any sprite, clip, controller or prefab. |
| **Scripting backend** | Mono and IL2CPP (no reflection, no `unsafe` code) |
| **API compatibility** | .NET Standard 2.1 |

---

## 7. Known limitations

- Art is fixed-resolution pixel art; upscaling beyond integer multiples will
  soften the look. Use **Point (no filter)** filtering and integer camera
  scaling.
- **Left/right facings are mirrored on most clips** — 68 of the 81 left/right
  pairs are an exact horizontal mirror. The other 13 are drawn per side (the
  Orc's walk and attack, both Golems' idle and walk, the Mushroom's idle, the
  Merchant's and Old Man's walk, the modular Warrior's walk, the Elf Archer's
  dodge roll, and the directional VFX). Up is a genuine back view throughout,
  drawn separately from the front.
- Diagonal (8-direction) movement is not covered except for the Elf Archer's two
  3/4 idle poses — the intended use is 4-direction top-down movement.
- The Warrior (Modular) equipment sprites are static single sprites intended to
  be parented and offset per frame by your own code; no pre-built layered
  prefab is included.
- The GIFs in `SourceArt/` are previews only — Unity imports the first frame as
  a still image and cannot play them.
- `SourceArt/` is 9,524 files. It is reference material that nothing in the
  package references, so deleting the folder is safe and shortens import time
  considerably if you do not need the raw art.
- Demo UI text uses the bundled Pretendard font; if you delete the font, the
  demo scene labels fall back to Unity's default font.
