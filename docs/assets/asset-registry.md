# Asset Registry

**Status legend:** `Planned` · `In progress` · `Delivered` (FBX in repo, awaiting Unity integration) · `Integrated` (prefab/scene done) · `Changed` (re-export after integration — check handoff).

Related gameplay IDs come from `docs/gameplay-contracts.md`; asset names never replace gameplay IDs.

## Sprint 1 — priority order

| # | Asset | File | Category | Related gameplay | Status |
|---|---|---|---|---|---|
| 1 | Control Panel (cover, fuse bay, card reader, hidden note) | `SM_ControlPanel.fbx` | Interactable | Quest 1 obj. 2–6, `FuseInstalled` | Delivered (look approved 2026-10-17) |
| 2 | ID Card | `SM_IDCard.fbx` | Pickup | `item.id_card` | Delivered |
| 3 | External Fuse | `SM_ExternalFuse.fbx` | Pickup | `item.external_fuse` | Delivered |
| 4 | Hidden Message note | child `HiddenNote` of `SM_ControlPanel` | Interactable | Quest 1 obj. 4 | Delivered (inside Control Panel) |
| 5 | Card Reader (standalone, Control Room door) | `SM_CardReader.fbx` | Interactable | `AccessGranted/Denied` | Planned |
| 6 | Security door (sliding, double) | `SM_SecurityDoor.fbx` | Interactable | Maintenance Wing / Control Room | Planned |
| 7 | Breaker panel | `SM_BreakerPanel.fbx` | Interactable | Quest 2 obj. 2 | Planned |
| 8 | IT computer + card writer hardware | `SM_ITWorkstation.fbx` | Interactable | Quest 2 obj. 5–6 | Planned |
| 9 | IT guide (manual/binder) | `SM_ITGuide.fbx` | Pickup | Quest 2 obj. 4 | Planned |
| 10 | First-person hands (static) | `SM_FPHands.fbx` | Character | Player | Planned |
| 11 | Props: desk, chair, shelf, bin, cart, crates, papers | `SM_Prop_*.fbx` | Props | — | Planned |
| 12 | Rooms: Main Lobby, Janitor, Maintenance Wing, Breaker, IT Center | `SM_Room_*.fbx` | Rooms | Areas | Planned (later) |

## Later sprints

MC full body, Wife (trapped in liquid), NPCs, Control Room interior, doctor's room, antidote, tranquilizer gun, toxic liquid pools/VFX meshes.
