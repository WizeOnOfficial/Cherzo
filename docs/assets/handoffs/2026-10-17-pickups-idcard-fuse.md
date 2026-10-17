# Asset Handoff — 2026-10-17 — ID Card + External Fuse (pickups)

- **Producer:** Claude (Blender asset pipeline)
- **Status:** Delivered — awaiting Unity integration
- **Depends on:** palette + materials from `2026-10-17-control-panel.md` (steps 1–2 there)
- **Scene/script changes made by this handoff:** none

## Files

| File | Tris | Related item data |
|---|---|---|
| `Assets/Art/Models/Pickups/SM_IDCard.fbx` | 156 | `Assets/ScriptableObject/QuestItems/ID Card.asset` (`item.id_card`) |
| `Assets/Art/Models/Pickups/SM_ExternalFuse.fbx` | 152 | `Assets/ScriptableObject/QuestItems/External Fuse.asset` (`item.external_fuse`) |
| `ArtSource/Pickups/IDCard.blend`, `build_IDCard.py` | | source + rebuild script |
| `ArtSource/Pickups/ExternalFuse.blend`, `build_ExternalFuse.py` | | source + rebuild script |
| `ArtSource/_previews/Pickups_*.png` | | review renders (incl. fuse seated in the panel) |

## Hierarchy

```text
IDCard                      pivot = centre. Photo side faces +Z (Unity) / -Y (Blender). 8.6 × 5.4 cm, 2.5 mm thick (exaggerated)
├─ IDCard_Body              card, orange brand band, photo, name, chip, back magstripe
├─ IDCard_RoleStripe        access-level stripe (red = low access). Separate so it can turn e.g. green
│                           after the IT Center role rewrite (Quest 2 obj. 6): swap material / UV / tint
├─ IDCard_Clip              lanyard clip at the top
└─ IDCard_COL               convex box, 1 cm thick (thin colliders tunnel through floors)

ExternalFuse                pivot = centre. Long axis = Unity Y (same as the panel's fuses). 3.2 × 8.2 cm
├─ ExternalFuse_Body        red body + yellow band (differs from the white standard fuses), brass caps
└─ ExternalFuse_COL         convex box
```

## Fit with the Control Panel (verified)

Snapping `ExternalFuse` root to `ControlPanel/FuseBay/Socket_ExternalFuse` with **local position 0, local rotation 0** seats it in the orange holder's clips (checked by importing both FBX files together). No offset needed.

## Unity steps

1. Import settings for both FBX files: same as Control Panel step 3 — **Bake Axis Conversion ON**, Animation Type None, remap `M_Palette` / `M_Palette_Emit`.
2. Pickup prefab (per item), following the existing pickup setup in `TestScene` (`PhysicsPickup` + `Rigidbody` + collider):
   - Root: add `Rigidbody`, `PhysicsPickup` (`pickupRigidbody` = this Rigidbody, `pickupCollider` = the collider below, `referenceItem` = the matching `InventoryItemData` asset).
   - `*_COL` child: add **Mesh Collider**, tick **Convex** (required with a Rigidbody), remove its Mesh Renderer.
   - Assign the prefab to the item data's `prefab` field if the Inventory owner wants that link.
3. ID Card lying on a desk: rotate the root **X = 90°** (photo side up) — or −90° depending on which side should face up; verify in Editor.
4. Fuse install (Quest 1 obj. 6): parent the fuse to `Socket_ExternalFuse`, local position/rotation zero; disable its Rigidbody physics / collider while installed (or spawn a static copy).

## Notes for the team

- **Discoverability:** the fuse is small (8 cm) because it must fit the panel. In the Janitor Room, place it where light and composition point at it (on a shelf at eye height, not the floor). If it is still hard to spot, an optional "emergency fuse case" prop (red box the fuse sits in) can be produced — ask the asset pipeline.
- **Inventory icons:** not produced; the project's `IconCreationScene` workflow can render them from these prefabs.
- **Import settings not yet applied:** at handoff time `SM_ControlPanel.fbx.meta` still had Bake Axis Conversion OFF and `T_Palette.png.meta` still had bilinear filtering, mipmaps and compression on. Apply Control Panel handoff steps 1–3 before judging the look.

## Validation performed

- Raw FBX re-import: all children rotation 0, scale 1, names intact.
- Fit test: fuse body spans the holder clips; centre exactly at the socket.
- Renders at full and low resolution read clearly.
- Not validated: inside Unity with the pixel/dither renderer; Rigidbody behaviour.

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
Ignore all Unity import steps and earlier updates above. The FBX files are ready as-is (upright, coloured, transforms 0/0/0).
`*_COL` children are no longer in the FBX. Pickup prefab: on the root add **Box Collider** (auto-fits; for the card make it
≥ 1 cm thick to avoid tunnelling) + **Rigidbody** + `PhysicsPickup`. Fuse install: parent to `Socket_ExternalFuse`, local position/rotation 0.

## UPDATE 2026-10-17 (4) — cleanup
Build scripts listed above were deleted after delivery (see style guide §17). Source of truth is the `.blend` in `ArtSource/`.
