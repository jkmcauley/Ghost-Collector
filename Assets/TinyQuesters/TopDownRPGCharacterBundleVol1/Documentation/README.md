# Tiny Questers — Top-Down RPG Character Bundle Vol.1

**2D top-down pixel-art character bundle** for RPGs — fully animated heroes,
town & village NPCs, and monsters that all share the same 64×64 base size,
feet-center pivot, and animation workflow, so they drop into the same setup
and line up on the same tile grid.

Pixel art by **Bobddadoo** — https://bobddadoo.itch.io

---

## ✨ Highlights

- **21 fully-animated characters** across Heroes, NPCs, and Monsters —
  **3,059 sprites** in **330 animation clips**.
- **Ready to use out of the box** — the sheets in `Art/` ship pre-sliced (import
  settings and feet-on-ground pivots baked into their `.meta`), with animation
  clips, animator controllers, prefabs and two scenes already wired up.
- **Every source file is included too.** `SourceArt/` carries the art exactly as
  the itch.io release ships it — **9,524 files**: single frames, trimmed
  atlases, power-of-two padded copies of both, and 1× / 5× **GIF previews**.
  Nothing is held back for another edition.
- **Shared 64×64 base canvas + feet-on-ground pivots** → pivots are measured
  from each sheet's actual feet, so every character lines up on the same ground
  line, across every clip and canvas size, and swaps cleanly at runtime.
- **Full 4-direction facing** (down / up / left / right) — **up is a true back
  view**, drawn from scratch rather than reusing the front. Left and right are
  mirrored on most clips (68 of the 81 left/right pairs); the remaining 13 are
  **drawn per side** where mirroring would read wrong — the Orc's walk and
  attack, both Golems' idle and walk, the Mushroom's idle, the Merchant's and
  Old Man's walk, the modular Warrior's walk, the Elf Archer's dodge roll, and
  the directional VFX.
- Animation sets: **idle / walk / attack / hit / death** (4-direction where
  applicable), plus matching VFX for the Mage and bonus FX for monsters. NPCs
  are idle + walk townsfolk; the Guard is the one with an attack.
- **Victory (win) poses** in both facings for the Warrior, Mage, Elf Archer and
  the Guard — and a **spear-thrust attack** for the Guard.
- **Two animation timings** — a *smooth* take and a *classic* take
  (75 ms / 150 ms per frame). In `Art/` this covers the NPCs' hand-drawn side
  idles, which ship as both; `SourceArt/` carries complete smooth **and**
  classic sets of the Elf Archer and the Monster Second Pack.
- **A modular Warrior** whose body is animated *without* gear, with weapon /
  shield / hand pieces shipped separately so you can layer, swap, recolor, or
  remove equipment at runtime.
- Works in **Built-in** and **URP** — every prefab uses Unity's built-in
  `Sprites-Default` material; no custom shaders or materials are shipped.
  Engine-agnostic art (also usable in Godot, GameMaker, Construct, etc. via
  the raw PNG sheets in `SourceArt/`).

> Full specification — asset counts, image dimensions, color depth,
> transparency, and the complete animation list — is in
> **[`TECHNICAL_DETAILS.md`](TECHNICAL_DETAILS.md)**.

---

## 📂 Two folders: `Art/` and `SourceArt/`

| | `Art/` | `SourceArt/` |
|---|---|---|
| What | The sheets the package actually runs on | The complete raw art set |
| Files | **407 PNG** → 3,059 sprites | **9,524 files** (8,634 PNG + 890 GIF) |
| Sliced? | Yes — grid + feet pivots baked into `.meta` | No — each file imports as one plain sprite |
| Used by | All clips, controllers, prefabs and scenes | Nothing — it is reference material |
| Size | 1.0 MB | 21.0 MB |

**`Art/` is all you need to ship a game.** `SourceArt/` is there so you are never
locked into the layout chosen here: re-slice an atlas your own way, grab one
frame as a still, feed the PNGs to another engine, or open a GIF to check the
intended timing at a glance.

> **You can delete `SourceArt/` entirely** and every clip, prefab and scene keeps
> working — nothing references it. Do that if you want the smallest possible
> project.

### What's in `SourceArt/`

| Kind | Files | What it is |
|------|-------|------------|
| `png/single/` | 3,838 | Every animation frame as its own PNG, trimmed to the drawn cell |
| `png/single_pot/` | 3,838 | The same frames padded to power-of-two dimensions |
| `png/atlas/` | 443 | One horizontal strip per clip, trimmed |
| `png/atlas_pot/` | 443 | The same strips padded to power-of-two |
| `gif/1x/`, `gif/5x/` | 886 | Animated previews at 1× and 5× scale, at the source timing |
| `items/`, `extras/`, `bonus/` | 76 | Modular gear, arrows, FX, item icons and shop signs (72 PNG + 4 GIF loops of the FX sets) |

That is **8,634 PNG + 890 GIF = 9,524 files**.

Packs whose art comes in two timings (**Elf Archer**, **Monster Second Pack**)
carry a full `smooth/` and `classic/` set of all of the above — the classic take
uses fewer frames per clip (e.g. the Elf Archer's 32 clips are 382 frames smooth
and 195 classic).

> **About the GIFs:** they are **previews for you to look at**, not playable
> assets. Unity imports a GIF as a still image (its first frame only) and cannot
> play it — use the clips in `Animations/` for playback, and open the GIFs in a
> browser or image viewer when you want to see a motion at a glance.

---

## 📦 What's Inside (21 characters)

### Heroes (4)
| Character | Folder | Clips / Frames | Notes |
|-----------|--------|----------------|-------|
| Elf Archer | `Art/Heroes/ElfArcher` | 32 / 382 | Ranged hero: idle ×6 (incl. two 3/4 diagonal views), walk, 4-way dodge roll, 4-direction bow attack, draw→shoot→recover combo, fast-fire, hit ×4, 4 death variants, and a **win pose in both facings**. Arrow projectiles in `Items/`. |
| Mage | `Art/Heroes/Mage` | 34 / 322 | Staff spellcaster: melee, magic & fireball attacks + 4-direction VFX, a 22-frame turn-around, and a **win pose in both facings**. |
| Warrior | `Art/Heroes/Warrior` | 19 / 220 | Full 4-direction hero (idle, walk, attack ×2 variants, hit, death) + a **win pose in both facings**. |
| Warrior (Modular) | `Art/Heroes/WarriorModular` | 13 / 129 | Body animated **without gear** + 10 separate weapon/shield/hand pieces in `Items/`. |

### NPCs (8)  →  `Art/NPCs/`
| Character | Clips / Frames | Extras beyond idle ×6 + walk ×4 |
|-----------|----------------|----------------------------------|
| Barmaid | 10 / 48 | — |
| Blacksmith | 10 / 52 | — |
| Female Villager | 10 / 49 | — |
| Guard | 14 / 100 | **spear attack ×2 · win pose ×2** |
| Merchant | 14 / 78 | "holding an item" side idle ×2 (+ classic ×2) |
| Old Man | 10 / 69 | — |
| Town Girl | 10 / 48 | — |
| Village Man | 10 / 53 | — |

Each NPC has **idle ×6 + walk ×4**: idle down / up, plus hand-drawn `idle_left`
and `idle_right` in **two timings** — an 8-frame smooth take (the default, used
by the prefabs) and a 4-frame `_classic` take that breathes at the same rate as
the front and back idles. Swap one for the other by name; both are already
animator states.

Bonus in `Art/NPCs/Bonus/`: 6 item **icons** + 7 shop **signs**.

### Monsters — Starter Pack (4)  →  `Art/Monsters/MonsterStarterPack/`
| Character | Clips / Frames |
|-----------|----------------|
| Bat | 16 / 120 |
| Goblin | 15 / 67 |
| Skeleton | 13 / 79 |
| Slime | 17 / 111 |

### Monsters — Second Pack (5 sets)  →  `Art/Monsters/MonsterSecondPack/`
| Character | Clips / Frames |
|-----------|----------------|
| Orc | 16 / 130 |
| Lizard | 22 / 285 |
| Mushroom | 18 / 176 |
| Golem (mossy) | 13 / 228 |
| Golem (clean) | 13 / 226 |

Bonus in `Extras/`: dust FX, lizard teleport FX, and a ginkgo tree.

> **Clip timing:** clips run at **13.33 fps** (75 ms/frame) or **6.67 fps**
> (150 ms/frame), following the source art. The Elf Archer, the Second Pack
> monsters, every win pose, the Guard's attack and the NPCs' smooth side idles
> use 75 ms; everything else — including the 4-frame `_classic` side idles —
> uses 150 ms.

---

## 🚀 Quick Start

1. Import this package into a project (**Unity 2021.3 LTS or newer**; built in
   Unity 6). Nothing else is required — the art, clips, controllers and prefabs
   need no extra packages. (The two demo *scenes* use uGUI for their interface;
   without `com.unity.ugui` installed the package still imports with no errors,
   the demo assembly is simply skipped. Add uGUI from **Window ▸ Package
   Manager** if you want to run those scenes. The 2D Sprite package is only
   needed if you want to re-slice a sheet in the Sprite Editor yourself.)
2. Everything is ready immediately — the sheets in `Art/` are already sliced and
   the **animation clips, animator controllers, prefabs, and scenes are
   included**:
   - Animation clips → `Animations/<Character>/TQ_<Character>_<Action>.anim`
   - Animator controllers + prefabs → `Prefabs/<Category>/TQ_<Character>.*`
   - Showcase scene → `Scenes/TQ_Showcase.unity`
   - One-character demo → `Scenes/TQ_Demo.unity`
3. Drop any `Prefabs/<Category>/TQ_<Character>.prefab` into your scene and it
   plays its idle animation right away — no extra setup.
4. Or open one of the included scenes and press **Play**:
   - **`TQ_Showcase`** — all characters in a grid. Click to reveal them; each
     character automatically cycles through all of its animations. **Scroll to
     zoom, drag (left/middle mouse) to pan, press `Space` to reset the view.**
   - **`TQ_Demo`** — one character at a time: **◀ ▶** change character,
     **▲ ▼** change motion, **Auto** cycles motions automatically, **Space**
     play/pause.

---

## 🗂 Folder Structure

```
Assets/TinyQuesters/TopDownRPGCharacterBundleVol1/
├─ Art/                       ← pre-sliced sheets (what the package runs on)
│  ├─ Heroes/
│  │  ├─ ElfArcher/          (sheets)  + Items/ (arrow projectiles)
│  │  ├─ Warrior/            (sheets)
│  │  ├─ WarriorModular/     (sheets)  + Items/ (weapons/shields/hands)
│  │  └─ Mage/               (sheets)
│  ├─ NPCs/
│  │  ├─ Barmaid/ … VillageMan/   (8 characters)
│  │  └─ Bonus/  Icons/  Signs/
│  ├─ Monsters/
│  │  ├─ MonsterStarterPack/ Bat/ Goblin/ Skeleton/ Slime/
│  │  └─ MonsterSecondPack/  Orc/ Lizard/ Mushroom/ Golem/ GolemClean/  + Extras/
│  └─ UI/                    (demo UI sprites)
├─ SourceArt/                 ← the complete raw art set (optional, deletable)
│  ├─ Heroes/
│  │  ├─ Warrior/            png/{single, single_pot, atlas, atlas_pot} + gif/{1x, 5x}
│  │  ├─ WarriorModular/     …same, + items/
│  │  ├─ Mage/               …same
│  │  └─ ElfArcher/          smooth/ + classic/ (each with the full set), + extras/
│  ├─ NPCs/                  …same, + bonus/
│  └─ Monsters/
│     ├─ MonsterStarterPack/ …same
│     └─ MonsterSecondPack/  smooth/ + classic/, + extras/
├─ Animations/                (.anim clips, per character)
├─ Prefabs/                   (Heroes/ NPCs/ Monsters/ UI/ + .controller files)
├─ Demo/                      (demo scripts + character data assets)
├─ Scenes/                    (TQ_Showcase.unity, TQ_Demo.unity)
├─ Font/                      (Pretendard Regular, demo UI only)
└─ Documentation/             (README, TECHNICAL_DETAILS, CHANGELOG, license)
```

---

## ⚙️ Import Settings

The sheets already ship with these settings baked in. If you add your own art
or re-import by hand, use exactly:

| Setting | `Art/` sheets | `SourceArt/` files |
|---------|---------------|--------------------|
| Texture Type | **Sprite (2D and UI)** | Sprite (2D and UI) |
| Sprite Mode | **Multiple** (333 sheets) · **Single** (74 item/icon/sign/FX/UI files) | Single (all) |
| Pixels Per Unit | **64** — one 64px cell = 1 Unity unit = 1 tile | 64 |
| Filter Mode | **Point (no filter)** | Point (no filter) |
| Compression | **None** | None |
| Generate Mip Maps | **Off** | Off |
| Max Size | **4096** (no sheet exceeds 2048 on either axis) | 4096 |
| Wrap Mode | **Clamp** | Clamp |
| Alpha Is Transparency | **On** | On |

> The 6 demo UI sprites in `Art/UI/` are the one exception — they are interface
> graphics, not characters, so they use their own Pixels Per Unit (16 / 100).

---

## ✂️ Slicing (Grid by Cell Size)

**279 of the 333 sheets in `Art/` are a single horizontal row of frames.** The
other **54** are re-flowed into a multi-row grid so they stay inside the 2048 px
texture limit — the Warrior's and modular Warrior's attacks (2 rows each) and
most of the Monster Second Pack (2–6 rows). All of them are **already sliced**,
row layout included; the rules below are only for reference, or for when you
slice something yourself out of `SourceArt/`.

For the **87.9%** of sprites that use a square cell:

> **cell size = image HEIGHT** &nbsp;·&nbsp; **frame count = WIDTH ÷ HEIGHT**

| Cell size | Sprites | Used by |
|-----------|---------|---------|
| **64 × 64** | 1,705 | Base canvas — idle, walk, hit, death, win poses, most clips |
| **128 × 128** | 827 | Mage magic/fireball attacks & effects, larger monster attacks |
| **128 × 64** | 192 | Wide Golem clips, Lizard teleport, Guard spear attack, teleport FX |
| **80 × 80** | 140 | Elf Archer idle / walk / death / win |
| **73 × 64** | 40 | Warrior attack extents |
| **101 × 256** | 38 | Lizard power-up / final upgrade |
| **124 × 80** | 28 | Elf Archer knock-back deaths |
| **64 × 32** | 16 | Demo UI buttons |
| **94 × 64** | 13 | Skeleton attack extents |
| **48 × 48** | 13 | Dust FX frames |
| **69 × 100** | 12 | Mage turn-around |
| **64 × 137** · **64 × 145** | 24 | Bat effect clips |
| 4 further sizes | 11 | 64×128 (5), 32×32 (4), 569×161 (1), 30×30 (1) — sign, item and demo-UI sprites |

**Manual slice:** open the sheet in *Sprite Editor* → *Slice* → **Grid By Cell
Size** → enter the cell size (e.g. 64×64) → *Slice* → **Apply**.

### Pivots — feet on the ground line

Every character sprite uses **Custom pivot, x = 0.5**, and a **y taken from the
character's feet** rather than the cell's bottom edge. The frames are drawn with
empty padding underneath — typically **16–26 px** on a 64×64 idle cell and
**48–65 px** on a 128×128 attack cell — so a plain Bottom-Center pivot would
leave characters hovering, and, because the padding differs per clip, make them
jump the moment the clip changed.

The pivot y for a sheet is measured from the **bottom-most opaque pixel of its
first frame**, and **every frame of that sheet shares that one value**. The
result: all of a character's clips sit on the same ground line no matter which
canvas size they use.

| Example | Cell | Padding below feet | Pivot |
|---------|------|--------------------|-------|
| `Warrior/idle_down` | 64 × 64 | 26 px | (0.5, 0.40625) |
| `Warrior/attack_down` | 128 × 128 | 50 px | (0.5, 0.390625) |
| `Warrior/win_left` | 64 × 64 | 11 px | (0.5, 0.171875) |
| `ElfArcher/idle_down` | 80 × 80 | 16 px | (0.5, 0.2) |
| `ElfArcher/attack_down` | 64 × 64 | 15 px | (0.5, 0.234375) |

The `Warrior/win_left` row shows why the pivot is measured rather than fixed:
that sheet is drawn 15 px lower in its cell than the idle, so a shared pivot
value would drop the character below the ground line — the measured one keeps
its feet exactly where the idle left them.

The 12 `effect_*` sheets (Mage spell VFX, Bat effect) are the exception: their
first frame is a tiny spark, so measuring it would misplace them. Each instead
**inherits the pivot of its matching character sheet** (`effect_base_magic_left`
takes `attack_magic_left`'s pivot, and so on), keeping VFX registered to the
character exactly as drawn.

Item, icon, sign and FX sprites in `Items/`, `Bonus/` and `Extras/` keep a
centered pivot (0.5, 0.5) — they are parts and overlays, not standing figures.

---

## 🏷 Naming Conventions

- **Animation clip:** `TQ_<Character>_<Action>_<Direction>`
  e.g. `TQ_Warrior_Idle_Down`, `TQ_Mage_Attack_Fireball_Down`, `TQ_Guard_Win_Left`
- **Prefab:** `TQ_<Character>` — e.g. `TQ_Warrior`, `TQ_Mage`, `TQ_Goblin`
- **Animator Controller:** `TQ_<Character>.controller`

Character name = the folder the sheet lives in (`Warrior`, `FemaleVillager`,
`GolemClean`, …). Action/direction come from the sheet's file name
(`idle_down`, `attack_left`, `win_right`, `die`, …).

---

## 📐 Technical Notes

- Base canvas **64×64**, shared **feet-on-ground pivot** (see *Pivots* above);
  some attack/death clips use a larger canvas but their pivot is measured so the
  feet still land on the same ground line. (The Elf Archer is the clearest case:
  its idle, walk, win and two of its deaths sit on **80×80**, its attack, bow,
  hit and dodge-roll clips on **64×64**, and its two knock-back deaths on
  **124×80** — all on the same ground line.)
- All art is **32-bit RGBA PNG** (8 bits per channel) with a full alpha channel.
  The previews in `SourceArt/**/gif/` are the only non-PNG images.
- Recommended sorting: put characters on a **Transparency Sort Mode = Custom
  Axis (0,1,0)** 2D setup, or sort by Y for top-down depth.
- Clip timing follows the source art — 13.33 fps for the Elf Archer, the
  Monster Second Pack, the win poses, the Guard's attack and the NPCs' smooth
  side idles; 6.67 fps for everything else. idle / walk / turn loop;
  attack / hit / death / win play once. Adjust any clip's **Samples** in the
  Animation window to taste.
- **Full technical specification** — counts, dimensions, color depth,
  transparency, per-character animation list, platform and pipeline support —
  see [`TECHNICAL_DETAILS.md`](TECHNICAL_DETAILS.md).

---

## 📄 License & Credit

See `LICENSE.txt`. Distributed under the **Unity Asset Store EULA**.
Credit is appreciated but not required:

> Pixel art by Bobddadoo — https://bobddadoo.itch.io

---

## 💬 Support & Feedback

Questions, a misaligned frame, or a request for the next character? Reach out
via the Asset Store publisher page or https://bobddadoo.itch.io.

Thanks for your support — happy dev! 🙂
