# Asset Handoff — 2026-10-17 — Palette + Control Panel (style test)

- **Producer:** Claude (Blender asset pipeline)
- **Status:** Delivered — awaiting Unity integration and visual approval
- **Integrator:** team in Unity + VS Code (prefab, scripts, scene wiring)
- **Scene/script changes made by this handoff:** none

## Files

| File | What |
|---|---|
| `Assets/Art/Textures/T_Palette.png` | Shared 32×32 palette texture (all models use it) |
| `Assets/Art/Models/Interactables/SM_ControlPanel.fbx` | Control Panel model, 1,328 tris (visual) |
| `ArtSource/Interactables/ControlPanel.blend` | Blender source |
| `ArtSource/Interactables/build_ControlPanel.py` | Script that rebuilds + re-exports the asset |
| `ArtSource/_tools/chemkit.py` | Shared pipeline helpers + palette definition |
| `ArtSource/_previews/ControlPanel_*.png` | Review renders (closed / open / close-up / pixel-res) |
| `docs/assets/art-style-guide.md`, `asset-registry.md`, `palette_sheet.png` | Pipeline docs |

## Hierarchy (child names are stable — scripts may rely on them)

```text
ControlPanel                     root; pivot = back-centre-bottom (touches wall). Size 0.52 W × 0.18 D × 0.86 H (+0.5 m conduit up)
├─ Housing                       static body (fuse-bay recess cut in)
├─ Housing_Trim                  orange cap, plinth, side stripes, conduit
├─ CardReader                    empty — attach the Card Reader interaction here
│  ├─ CardReader_Body
│  ├─ CardReader_Screen          emissive cyan (idle)
│  ├─ CardReader_LED_Red         emissive — show on Denied
│  ├─ CardReader_LED_Green       emissive — show on Granted
│  └─ CardReader_COL
├─ FuseBay                       empty
│  ├─ FuseBay_Base               back plate, rails, 8 holders, wires
│  ├─ Fuse_01 … Fuse_08          individual fuses (no Fuse_04)
│  └─ Socket_ExternalFuse        empty = exact spot for the installed External Fuse (slot 04, orange holder)
├─ Cover                         pivot ON THE HINGE (left edge seen from front). Rest rotation 0 = closed
│  ├─ Cover_Symbol               high-voltage bolt on the yellow label
│  ├─ Cover_COL                  moves with the cover
│  └─ HiddenNote                 note taped on the INSIDE of the cover (visible only when open)
│     └─ HiddenNote_COL
├─ ControlPanel_COL              housing collider
├─ CamFocus_ControlPanel         empty — Cinemachine LookAt target for the close-up
└─ CamPos_ControlPanel           empty — suggested camera position for the close-up (0.7 m in front)
```

Mapping to `docs/interaction-contract.md`:

| Contract object | Use these children |
|---|---|
| Control Panel — inspect / open cover | `Cover` (+ `Cover_COL`) |
| Control Panel — install fuse | `Socket_ExternalFuse`, holder slot 04 |
| Hidden message (Quest 1 obj. 4) | `HiddenNote` (+ `HiddenNote_COL`) |
| Card Reader | `CardReader` (+ screen, LEDs, `CardReader_COL`) |
| Control-panel focus cutscene | `CamFocus_ControlPanel`, `CamPos_ControlPanel` |

## Unity steps (manual, in order)

### 1. Palette texture — select `Assets/Art/Textures/T_Palette.png`
Inspector: Texture Type **Default**, sRGB **ON**, Wrap Mode **Clamp**, Filter Mode **Point (no filter)**, Generate Mip Maps **OFF**, Compression **None** → Apply.

### 2. Materials — create in `Assets/Art/Materials/`
- `M_Palette`: Shader **Universal Render Pipeline/Lit**, Base Map = `T_Palette` (colour white), Metallic 0, Smoothness 0.1.
- `M_Palette_Emit`: copy of `M_Palette`, tick **Emission**, Emission Map = `T_Palette`, Emission colour white, intensity ~1.5.

### 3. Model import — select `SM_ControlPanel.fbx`
- **Model tab:** Scale Factor 1, Convert Units ON, **Bake Axis Conversion ON** (required), Read/Write OFF, Generate Colliders OFF → Apply.
- **Rig tab:** Animation Type **None**. **Animation tab:** Import Animation OFF.
- **Materials tab:** Material Creation Mode = Import via MaterialDescription, Location = Use External Materials (Legacy) *or* remap: `M_Palette` → `M_Palette`, `M_Palette_Emit` → `M_Palette_Emit` → Apply.
- Check: after Bake Axis Conversion every child shows Rotation (0,0,0) and Scale (1,1,1).

### 4. Prefab (integrator)
1. Drag the FBX into a scene → make a prefab (e.g. `Assets/Prefabs/Interactables/ControlPanel.prefab`).
2. For each `*_COL` child: Add Component **Mesh Collider** (Mesh = that child's mesh, Convex OFF — static), then **remove/disable its Mesh Renderer**.
3. Cover hinge: rotate `Cover` around its **local Y**; ≈ **+100°** = open (verify direction in Editor; if it swings into the wall use −100°).
4. LEDs: disable both LED objects at rest; enable Red on Denied, Green on Granted (or swap material). Screen colour can be tinted via MaterialPropertyBlock if needed.
5. External fuse install: snap the fuse object to `Socket_ExternalFuse` (position + rotation 0).
6. Placement in the lobby: put the root against the wall with its **+Z (blue arrow) facing into the room**, root height ≈ 0.9 m above the floor (card reader then sits at ≈ 1.5 m, eye height).
7. Replace `ControlPanel_Prototype` visuals with the prefab when Quest 1 owner agrees (scene is single-owner — coordinate before editing `TestScene.unity`).

## Recommended (optional) automation for VS Code

An editor-only `AssetPostprocessor` in an `Editor/` folder could apply steps 1–3 automatically for everything under `Assets/Art/`:
- `T_Palette*.png` → Point filter, no mipmaps, no compression, clamp.
- `Assets/Art/Models/**.fbx` → bakeAxisConversion = true, importAnimation = false, animationType = None, remap `M_Palette*` to the materials in `Assets/Art/Materials/`.
- On post-process of the model: children ending in `_COL` get a `MeshCollider` and their renderer removed.

Not done by this handoff (asset producer does not add scripts).

## Validation performed

- Triangle count 1,328 (budget 2,000).
- FBX re-imported raw (no axis conversion): only the root has the −90° X axis rotation; all 26 children rotation 0, scale 1. Hierarchy and names intact. Materials: `M_Palette`, `M_Palette_Emit`.
- Preview renders at full and pixel resolution: shape, cover, fuse bay and note read clearly.
- **Not validated:** actual look inside Unity with the project's URP pixel/dither setup; cover open direction sign.

## Next AI prompt (VS Code)

```text
Read docs/assets/art-style-guide.md and docs/assets/handoffs/2026-10-17-control-panel.md.
Guide me through Unity steps 1–4 for SM_ControlPanel.fbx without editing TestScene.unity.
Then propose how the existing ControlPanelPrototypeInteraction / HiddenMessagePrototypeInteraction
can target the Cover, HiddenNote, CardReader and Socket_ExternalFuse children, respecting
docs/interaction-contract.md.
```

## UPDATE 2026-10-17 — Unity import steps are now automatic

`Assets/Art/Editor/ArtAssetPostprocessor.cs` (editor-only) applies the import settings, creates
`Assets/Art/Materials/M_Palette.mat` + `M_Palette_Emit.mat`, assigns them, and turns every `*_COL`
child into a `MeshCollider` (convex for `/Pickups/`, renderer removed). Steps 1–3 above and the
collider part of the prefab step are no longer manual. If something looks grey, use
**Tools > Art Pipeline > Reimport Art Assets**.

## UPDATE 2026-10-17 (2) — orientation
Unity's "Bake Axis Conversion" laid models flat. The automation now keeps it OFF and bakes the
root rotation into the meshes itself: models stand upright and every transform is rotation 0, scale 1.
Ignore any "Bake Axis Conversion ON" instruction above.

## UPDATE 2026-10-17 (3) — FINAL: no script, no import steps
Ignore all Unity import steps and both earlier updates above. The FBX is ready as-is: upright, coloured, transforms 0/0/0.
`*_COL` children are no longer in the FBX. Add colliders in the prefab with **Add Component → Box Collider** on:
- `ControlPanel` root (housing; adjust to the body, ~0.52 × 0.86 × 0.14 m),
- `Cover` (moves with the hinge — needed to raycast "open cover"),
- `CardReader` / `CardReader_Body` (scan interaction),
- `HiddenNote` (read the note; small box, only reachable when the cover is open).
Cover hinge: rotate `Cover` local Y, ≈ +100° = open (verify sign).

## UPDATE 2026-10-17 (4) — cleanup
Build scripts listed above were deleted after delivery (see style guide §17). Source of truth is the `.blend` in `ArtSource/`.
