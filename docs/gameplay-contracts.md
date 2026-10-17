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

The prototype uses `access.maintenance_wing` for the second scan on the
Control Panel's same `CardReader`. This ID is stable within the prototype and
must be preserved or migrated explicitly before production asset integration.

The prototype uses `area.maintenance_wing` for the explicit area trigger after
access is granted. The trigger publishes `AreaEntered`; Quest 1 consumes that
fact and completes `quest.1.enter_maintenance_wing` only after
`quest.1.scan_id_card_after_fuse`.

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

Status: `Implemented` for payload types and event hub API. Actual gameplay producers and Quest consumers are still `Planned`.

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
