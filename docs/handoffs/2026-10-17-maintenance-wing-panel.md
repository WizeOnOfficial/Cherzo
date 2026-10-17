# Handoff — 2026-10-17 — Maintenance Wing Control Panel

## Session

- Contributor/AI: Copilot
- Branch: local working tree
- Scope: correction of the Quest 1 and Quest 2 Control Panel ownership
- Status: ready for Unity validation

## Goal

Keep the original Main Room Control Panel as the Quest 1 access gate. The
second scan uses the same `item.id_card` on that original panel after the fuse
is installed. Reserve the Maintenance Wing Control Panel for Quest 2 Control
Room access.

## Requirements

- Do not create a duplicate card item or duplicate card-reader logic.
- Reuse the same `item.id_card` from inventory.
- Keep the earlier Control Panel responsible for the complete Quest 1 flow,
  including the second scan that opens Maintenance Wing.
- Do not use the Maintenance Wing Control Panel for Quest 1 access.
- Keep Quest 1 progress driven by typed events, not by GameObject names or direct manager calls.
- Do not use `GameObject.Find` or hidden static state.

## Correct flow

```text
Find ID Card
  -> first Control Panel access denied
  -> inspect first Control Panel
  -> find Hidden Message
  -> find External Fuse
  -> install fuse on first Control Panel
  -> scan the same item.id_card again on the first Control Panel
  -> AccessResolved(access.maintenance_wing, Success)
  -> travel to Maintenance Wing area
  -> Quest objective completed
```

## Implemented changes

- `ControlPanelPrototypeInteraction` owns the complete Quest 1 Main Room flow,
  including the second ID Card scan after fuse installation.
- `MaintenanceWingControlPanel` has no Quest 1 card-reader component and is
  reserved for Quest 2 Control Room access.
- The same `item.id_card` is validated by the original Control Panel on both
  scans.

## Safety rules

- Do not read `InventorySystem.inventory` directly.
- Do not call Quest tracker methods from the interaction script.
- Do not hard-code GameObject names.
- Do not create a second item or second ID Card inventory representation.
- Keep the second scan on the original Control Panel script.
- Do not create a separate Quest 1 reader in the Maintenance Wing.

## Next Unity manual steps

1. Select the original Main Room `ControlPanel_Prototype`.
2. Confirm its `ControlPanelPrototypeInteraction` references are assigned.
3. Install and consume the External Fuse on that panel.
4. Scan the same ID Card again on that same panel.
5. Confirm `AccessResolved(access.maintenance_wing, Success)` logs once.
6. Confirm repeated scans return `AlreadyCompleted` without duplicate success.

## Next validation step

- Verify the Main Room Control Panel owns the entire Quest 1 access flow.
- Keep `MaintenanceWingControlPanel` reserved for Quest 2 Control Room access.
- Do not add a standalone Maintenance Wing card reader for Quest 1.
