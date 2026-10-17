# Gameplay Contracts

**Status:** Accepted direction; shared runtime foundation implemented, feature producers/consumers remain `Planned`.

## Compatibility policy

The current Inventory implementation is preserved. These contracts describe what other modules may rely on, not the internal shape of the current Inventory code.

- Quest must consume an adapter/query boundary.
- Quest must not read `InventorySystem.inventory`, `m_itemDictionary`, or `InventoryItem.stackSize` directly.
- Adding Sprint 2 content is additive.
- Existing IDs, event meanings, and completed sprint outcomes are not silently repurposed.
- Any incompatible change requires an ADR and migration plan.

## ID registry

Gunakan satu ID kanonik. Jangan membuat variasi seperti `fuse`, `ExternalFuse`, dan `external_fuse` untuk entitas yang sama.

| Jenis | ID | Makna | Owner |
|---|---|---|---|
| Item | `TBD_ITEM_ID_CARD` | Kartu identitas | Inventory |
| Item | `TBD_ITEM_EXTERNAL_FUSE` | Fuse eksternal | Inventory |
| Area | `TBD_AREA_ROOM_1` | Room 1 | World |
| Access point | `TBD_ACCESS_CONTROL_ROOM` | Akses control room | Interaction |
| Quest | `TBD_QUEST_1` | Quest 1 | Quest 1 |
| Quest | `TBD_QUEST_2` | Quest 2 | Quest 2 |

Status: `Planned` — replace TBD values only after team validation. Do not create runtime code from unvalidated TBD identifiers.

Runtime Quest 1 objective IDs currently implemented in the prototype are
documented in `QuestGameplayIds`; they preserve the canonical item and
interaction IDs above and do not replace the TBD registry until final team
validation.

The prototype uses `access.maintenance_wing` for the second scan on the same
Main Room Control Panel used for inspection and fuse installation. The
Maintenance Wing Control Panel is reserved for the later Control Room access
flow. This ID is stable within the prototype and must be preserved or migrated
explicitly before production asset integration.

The prototype uses `area.maintenance_wing` for the explicit area trigger after
access is granted. The trigger publishes `AreaEntered`; Quest 1 consumes that
fact and completes `quest.1.enter_maintenance_wing` only after
`quest.1.scan_id_card_after_fuse`.

The first Quest 2 scan at the Maintenance Wing Control Panel only queries
`item.id_card`. It never removes or consumes the card. It publishes
`AccessResolved(access.control_room, PowerUnavailable)` once; repair and later
role/access changes remain planned.

The Quest 2 Breaker Panel prototype becomes available only after that
`PowerUnavailable` result. A successful repair publishes
`InteractionCompleted(interaction.breaker_panel, Success)`. Quest 2 consumes
that fact and completes `quest.2.repair_electrical_short`; the repair does not
modify Inventory or open the Control Room.

Quest 2 role modification is a simulated Linux-style permission puzzle inside
the in-game IT desktop. The supported command is
`chmod u=rwx id_card`; Unity validates this text locally and never executes an
OS command. The initial virtual permission is `r--` (`Guest`) and the target
permission is `rwx` (Control Room Administrator). Successful validation
changes access-role state only and publishes the documented typed role-update
event when that event is implemented. The permanent `item.id_card` remains
unchanged in Inventory.

After a successful Breaker Panel repair, the next Control Room scan publishes
`AccessResolved(access.control_room, Denied)` because the permanent ID Card
still has the virtual `r--` permission. Repeated scans remain denied and do
not repeat the Quest 2 objective.

The IT Guide prototype uses the stable `item.it_guide` item ID and the existing
Inventory pickup adapter. A successful pickup publishes `ItemAdded`; Quest 2
consumes that fact to complete `quest.2.find_it_guide`. The guide does not
change access-role state. Its prototype pickup is gated until the role-denial
objective has been completed, so the item cannot be acquired out of order.

Hardware connection is an interaction-only prototype. It does not add or
remove an Inventory item. After the IT Guide `ItemAdded` fact, the hardware
interaction becomes available and publishes
`InteractionCompleted(interaction.hardware_connection, Success)` once.

The `AccessRoleModified` contract is now implemented for the simulated IT
desktop boundary. Its payload contains:

- `access_subject.id_card`;
- a validated virtual permission value (`r--` or `rwx`);
- a stable role ID (`role.guest` or
  `role.control_room_administrator`).

Only these pairs are valid: `r--` with `role.guest`, or `rwx` with
`role.control_room_administrator`. Mismatched permission/role pairs are
rejected before the event is dispatched.

The event changes only virtual Quest 2 access state. It must never mutate or
replace the permanent `item.id_card` Inventory item.

The prototype terminal parser accepts only the documented command strings.
It requires the Quest 2 hardware-connected state before processing any
command. The exact `chmod u=rwx id_card` command changes the parser's virtual
state once and publishes `AccessRoleModified`; repeated execution does not
publish a duplicate event.

## Event registry

| Event | Payload minimum | Producer | Consumers |
|---|---|---|---|
| `ItemAdded` | item ID, amount | Inventory | Quest, UI |
| `ItemRemoved` | item ID, amount | Inventory | Quest, UI |
| `InteractionSucceeded` | interaction ID | Interaction | Quest, UI |
| `InteractionDenied` | interaction ID, reason | Interaction | UI, Quest |
| `FuseInstalled` | panel ID, fuse ID | Control Panel | Quest, Cutscene |
| `AccessGranted` | access point ID | Access system | Quest, World |
| `ComputerAccessModified` | terminal ID | Computer | Quest |
| `TimeExpired` | timer ID | Timer | Win/Lose |
| `QuestObjectiveCompleted` | quest ID, objective ID | Quest | UI, next flow |
| `AccessRoleModified` | access subject ID, permission value, role ID | IT desktop puzzle | Control Room access |

Status: shared payload types and event hub API are `Implemented`; the
hardware-connection prototype producer and Quest 2 consumer are `Implemented`
in `TestScene`. The terminal role-update producer and final access consumer
remain `Planned`.

`AccessRoleModified` is **Implemented** as a shared Quest 2 event boundary.
Its producer must be the simulated IT desktop puzzle, never an OS shell or
direct Control Room manager call.

The current prototype represents fuse installation through
`InteractionCompleted(interaction.control_panel_fuse_installation, Success)`.
The dedicated `FuseInstalled` payload/event remains the planned final boundary
for production asset integration.

Implemented runtime types:

- `GameplayInteractionResult`;
- `ItemChangedEvent`;
- `InteractionEvent`;
- `AccessEvent`;
- `AreaEnteredEvent`;
- `QuestObjectiveEvent`;
- `AccessRoleModifiedEvent`;
- `IGameplayEventSource`;
- `GameplayEventHub`.

The hub is wired into the current `TestScene` prototype through explicit
Inspector references. Production-scene integration and final asset producers
remain planned.

## Event rules

- Event berarti fakta sudah terjadi, bukan perintah.
- Producer tidak mengetahui daftar consumer.
- Payload harus cukup untuk consumer tanpa mencari object berdasarkan nama.
- Event baru harus ditambahkan ke registry sebelum implementasi.
- Perubahan payload memerlukan ADR atau migration note.

## Result vocabulary

Gunakan result yang eksplisit dan dapat ditampilkan/diterjemahkan:

- `Success`
- `Denied`
- `MissingRequirement`
- `AlreadyCompleted`
- `PowerUnavailable`
- `InvalidState`

## State ownership

| State | Owner | Query publik |
|---|---|---|
| Kepemilikan item | Inventory | `Has(itemId)`, `Count(itemId)` |
| Progress Quest 1 | Quest 1 | read-only objective state |
| Progress Quest 2 | Quest 2 | read-only objective state |
| Panel/fuse state | Control Panel | read-only interaction state |
| Access permission | Access system | `CanAccess(accessPointId)` |
| Timer | Timer/Win-Lose | remaining time, expired |

## Sprint compatibility

Sprint 1 exposes outcomes rather than implementation details:

- `MaintenanceWingAccessGranted`;
- `ControlRoomAccessGranted`;
- `Sprint1Completed`.

Sprint 2 may add items, states, events, objectives, and consumers. It must not change the meaning of Sprint 1 identifiers or read Quest 1 private state.
