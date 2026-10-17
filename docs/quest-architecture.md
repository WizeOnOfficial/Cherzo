# Quest Architecture and Runtime Flow

**Status:** Implemented incrementally in `TestScene`; final production asset
integration remains planned.

This document visualizes the current boundaries. It is the reference for
replacing prototype visuals with production assets without moving Quest state
into asset scripts.

## Module boundary

```text
+-------------------+       +-----------------------------+
| World pickup      |       | Control Panel / Hidden Msg |
| PhysicsPickup     |       | IInteractable prototype    |
+---------+---------+       +--------------+--------------+
          |                                |
          v                                v
+-----------------------+       +---------------------------+
| InventoryPickup       |       | GameplayEventHub          |
| Acquisition           |------>| ItemAdded / Interaction  |
+-----------+-----------+       | Completed / QuestComplete |
            |                  +-------------+-------------+
            v                                |
+-----------------------+                    v
| InventoryCompatibility|<-----------+----------------------+
| Adapter               |             | Quest1ObjectiveTracker|
+-----------+-----------+             | owns objective state  |
            |                         +----------------------+
            v
+-----------------------+
| Existing Inventory    |
| InventorySystem       |
+-----------------------+
```

### Rules

- `Quest1ObjectiveTracker` owns Quest 1 progress.
- `InventorySystem` owns item ownership and storage.
- `InventoryCompatibilityAdapter` is the only Quest-facing Inventory boundary.
- Interaction objects publish facts through `GameplayEventHub`; they do not call
  the Quest tracker directly.
- `GameplayEventHub` is an explicitly assigned scene component, not a singleton.
- Stable IDs come from `QuestGameplayIds`, not GameObject names or prefab names.
- Prototype components may be replaced by asset adapters without changing the
  Quest tracker contract.

## Current Quest 1 event flow

```text
ID Card pickup
  -> InventoryPickupAcquisition
  -> InventoryCompatibilityAdapter.AddItem(item.id_card)
  -> InventorySystem
  -> GameplayEventHub.ItemAdded
  -> Quest1ObjectiveTracker
  -> QuestObjectiveCompleted(quest.1.find_id_card)

Control Panel first attempt
  -> ControlPanelPrototypeInteraction
  -> GameplayEventHub.InteractionCompleted(
       interaction.control_panel, Denied)
  -> Quest1ObjectiveTracker
  -> QuestObjectiveCompleted(quest.1.try_initial_control_panel_access)

Control Panel inspection
  -> ControlPanelPrototypeInteraction
  -> GameplayEventHub.InteractionCompleted(
       interaction.control_panel_inspection, Success)
  -> HiddenMessagePrototypeInteraction unlocks
  -> Quest1ObjectiveTracker
  -> QuestObjectiveCompleted(quest.1.inspect_control_panel)

Hidden Message search
  -> HiddenMessagePrototypeInteraction
  -> GameplayEventHub.InteractionCompleted(
       interaction.hidden_message, Success)
  -> Quest1ObjectiveTracker
  -> QuestObjectiveCompleted(quest.1.find_hidden_message)

External Fuse pickup
  -> InventoryPickupAcquisition
  -> InventoryCompatibilityAdapter.AddItem(item.external_fuse)
  -> InventorySystem
  -> GameplayEventHub.ItemAdded
  -> Quest1ObjectiveTracker
  -> QuestObjectiveCompleted(quest.1.find_external_fuse)

External Fuse installation (prototype validation)
  -> ControlPanelPrototypeInteraction checks Hidden Message + fuse query
  -> GameplayEventHub.InteractionCompleted(
       interaction.control_panel_fuse_installation, Success)
  -> Quest1ObjectiveTracker
  -> InventoryCompatibilityAdapter.RemoveItem(item.external_fuse)
  -> GameplayEventHub.ItemRemoved
  -> QuestObjectiveCompleted(quest.1.install_external_fuse)

Maintenance Wing access card scan on the same Main Room Control Panel
  -> ControlPanelPrototypeInteraction
  -> GameplayEventHub.AccessResolved(
       access.maintenance_wing, Success)
  -> Quest1ObjectiveTracker
  -> QuestObjectiveCompleted(quest.1.scan_id_card_after_fuse)

Maintenance Wing area entry
  -> MaintenanceWingAreaTrigger (explicit player reference + trigger collider)
  -> GameplayEventHub.AreaEntered(area.maintenance_wing)
  -> Quest1ObjectiveTracker
  -> QuestObjectiveCompleted(quest.1.enter_maintenance_wing)
```

## Quest 1 status

| Objective | Runtime status | Owner |
|---|---|---|
| Find ID Card | Implemented prototype | Quest 1 + Inventory boundary |
| Initial Control Panel access | Implemented prototype | Control Panel + Quest 1 |
| Inspect Control Panel | Implemented prototype | Control Panel + Quest 1 |
| Find Hidden Message | Implemented prototype | Hidden Message + Quest 1 |
| Find External Fuse | Implemented prototype | Inventory boundary + Quest 1 |
| Install External Fuse | Implemented prototype | Control Panel / interaction + Inventory adapter consumption |
| Scan ID Card again | Implemented prototype | Control Panel CardReader child + Quest 1 |
| Enter Maintenance Wing | Implemented prototype code; scene wiring pending | Area trigger / Quest 1 |

## Asset integration rule

When a production asset arrives:

1. Inspect its components and existing interaction script.
2. Keep or create a thin adapter implementing the project interaction
   contract.
3. Preserve the stable ID and event payload.
4. Assign references in the Inspector.
5. Run the same Console smoke test.
6. Replace the prototype only after the event sequence is verified.

The production asset may change mesh, Animator, audio, VFX, and presentation.
It must not become the owner of Quest 1 progress.

## Quest 2 simulated IT terminal

The IT desktop is a Quest 2 interaction/presentation boundary. The player
views it through the in-game POV camera and enters a restricted Linux-style
command vocabulary. The command parser is implemented in Unity and validates
the text locally.

Supported prototype commands are `help`, `help chmod`, `ls -l`, `cat id_card`,
`chmod u=rwx id_card`, and `clear`. The parser must not execute shell
commands, call `Process`, access the host filesystem, or accept arbitrary
command text.

The terminal starts with the virtual ID Card permission `r--` and role
`Guest`. A valid `chmod u=rwx id_card` changes the virtual access role to
Control Room Administrator and publishes a typed access-role update event.
This state is separate from Inventory: the permanent `item.id_card` is not
removed, replaced, or rewritten.

The Quest 1 first and second card scans use the same physical reader: the
`CardReader` child inside `SM_ControlPanel`. A separate Maintenance Wing card
reader prototype is not part of this flow.
