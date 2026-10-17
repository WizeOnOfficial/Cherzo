# Quest Flow

**Status:** Accepted gameplay direction; Quest 1 objectives 1–8 have prototype
runtime coverage and require final Unity play-mode validation.

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
| 5 | Find external fuse in Janitor Room | implemented prototype item event after Hidden Message success | Hidden Message found | objective completed |
| 6 | Install fuse as emergency override | implemented prototype success result; fuse consumed after success | hidden message + fuse available | `FuseInstalled` |
| 7 | Scan the same ID card again at the Main Room Control Panel | implemented prototype access success | fuse installed | `MaintenanceWingAccessGranted` |
| 8 | Enter Maintenance Wing | implemented prototype area event; Unity wiring pending | access granted + trigger entered | Quest 1 complete |

## Quest 2 — Control room access

| Urutan | Objective | Trigger selesai | Requirement | Event keluar |
|---|---|---|---|---|
| 1 | Scan ID card at the Maintenance Wing Control Panel to access Control Room | implemented prototype `PowerUnavailable` result | Maintenance Wing entered + ID Card; ID Card remains in inventory | electrical short |
| 2 | Repair electrical short in Breaker Room | implemented prototype `InteractionCompleted(interaction.breaker_panel, Success)` | `PowerUnavailable` detected | objective completed |
| 3 | Scan again and receive role denial | implemented prototype `AccessResolved(access.control_room, Denied)` | power restored; same ID Card still present | objective completed |
| 4 | Find IT guide | implemented prototype item event | none; `item.it_guide` pickup | objective completed |
| 5 | Obtain/connect hardware | implemented prototype `InteractionCompleted(interaction.hardware_connection, Success)` | IT Guide found; hardware is interaction-only, not an Inventory item | objective completed |
| 6 | Modify ID Card role using a simulated Linux-style `chmod` puzzle | implemented prototype parser and `AccessRoleModified`; terminal UI planned | guide + hardware connected to IT desktop; initial permission `r--` | objective completed |
| 7 | Scan card again | planned access event | simulated permission updated to `rwx` | `ControlRoomAccessGranted` |
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

## Quest 2 IT desktop terminal contract

The IT Center role-modification step uses a simulated terminal shown through
the player's in-game desktop POV. It is a Unity puzzle, not an operating
system shell.

Initial virtual card state:

```text
permission: r--
role: Guest
```

Required command:

```text
chmod u=rwx id_card
```

The terminal may expose only the documented virtual commands:

```text
help
help chmod
ls -l
cat id_card
chmod u=rwx id_card
clear
```

`help chmod` must explain `r`, `w`, `x`, the `u` owner selector, and the
required `rwx` target. Invalid or unsupported command text must not mutate
the access state and should show a local terminal error/hint.

The prototype must never invoke `Process`, PowerShell, Command Prompt, a
Linux shell, filesystem APIs, or arbitrary command execution. Successful
validation changes only the Quest 2 access-role state and publishes a typed
access-role update event. It does not create, remove, or mutate the
`item.id_card`; the same card remains permanently in Inventory.
