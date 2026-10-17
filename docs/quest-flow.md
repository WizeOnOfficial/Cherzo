# Quest Flow

**Status:** Accepted gameplay direction; Quest 1 objectives 1–7 have verified
prototype runtime coverage, while Maintenance Wing entry remains planned.

## Scope boundary

Quest work treats the current Inventory implementation as an external dependency. Quest must not refactor it. The read-only compatibility adapter/query boundary now exists; scene wiring and item event bridging remain planned.

Quest 1 ends when the player enters `Maintenance Wing`. Quest 2 begins when the player first attempts to access `Control Room` from `Maintenance Wing`. Sprint 1 ends immediately after the player enters `Control Room` and the win screen shows `To Be Continued`.

## Global flow

```text
Main Lobby
  -> Maintenance Wing
  -> Control Room access attempt
  -> Electrical short / repair
  -> IT Center access modification
  -> Control Room access
  -> Win or timeout lose
```

## Quest 1 — Main Lobby to Maintenance Wing

| Urutan | Objective | Trigger selesai | Requirement | Event keluar |
|---|---|---|---|---|
| 1 | Find ID card | planned item query/event | none | objective completed |
| 2 | Try initial control panel access | implemented prototype denied result | ID Card | access remains denied |
| 3 | Inspect control panel | implemented prototype success result | initial denial | `ControlPanelInspected` |
| 4 | Find hidden message | implemented prototype success result | panel state | objective completed |
| 5 | Find external fuse in Janitor Room | implemented prototype item event | none | objective completed |
| 6 | Install fuse as emergency override | implemented prototype success result; fuse consumed after success | hidden message + fuse available | `FuseInstalled` |
| 7 | Scan ID card again on the same Control Panel reader | implemented prototype access success | fuse installed | `MaintenanceWingAccessGranted` |
| 8 | Enter Maintenance Wing | implemented prototype area event; Unity wiring pending | access granted + trigger entered | Quest 1 complete |

## Quest 2 — Control room access

| Urutan | Objective | Trigger selesai | Requirement | Event keluar |
|---|---|---|---|---|
| 1 | Scan ID card at Control Room | planned access result | ID Card | electrical short |
| 2 | Repair electrical short in Breaker Room | planned repair event | none/defined repair interaction | objective completed |
| 3 | Scan again and receive role denial | planned denied result | power restored | objective completed |
| 4 | Find IT guide | planned item query/event | none | objective completed |
| 5 | Obtain/connect hardware | planned interaction/event | hardware | objective completed |
| 6 | Modify ID Card role using `rwx` puzzle | planned access update event | guide + hardware | objective completed |
| 7 | Scan card again | planned access event | role updated | `ControlRoomAccessGranted` |
| 8 | Enter Control Room | planned area event | door open | Sprint 1 complete |

## Cutscene hooks

| Trigger | Cutscene response | Completion event |
|---|---|---|
| Control panel focused | switch Cinemachine camera, optionally lock input | `CutsceneCompleted(CONTROL_PANEL)` |
| Electrical short | camera/effect/audio | `CutsceneCompleted(ELECTRICAL_SHORT)` |

## Win/Lose

- Win trigger: `QuestObjectiveCompleted` for the final objective or approved mission-complete event.
- Win presentation: `To be continued`.
- Lose trigger: `TimeExpired`.
- Lose presentation: retry modal.
- Retry must reset only the agreed gameplay state; document the reset policy before implementation.

## Sprint 1 freeze

After `ControlRoomAccessGranted`, the player crosses the Control Room threshold and sees `To Be Continued`. There is no Control Room interior gameplay in Sprint 1.

Sprint 2 extends the game additively. It may add new item definitions and events, but must consume the public outcome `Sprint1Completed` rather than modifying Quest 1 internals.

## Open questions

- [x] Nama area pertama: `Maintenance Wing`.
- [x] Player menemukan ID Card di Main Lobby sebelum scan pertama.
- [x] Quest 1 dan Quest 2 berjalan sequentially.
- [x] Sprint 1 selesai setelah player melewati pintu Control Room.
- [x] Fuse dikonsumsi saat dipasang setelah pemasangan berhasil.
- [ ] State apa yang di-reset saat retry?
- [ ] Final stable IDs and payload types validated by the team.
- [ ] Compatibility adapter/query boundary agreed with Inventory owner.
- [ ] Exact `rwx` puzzle UI and required permission values.
