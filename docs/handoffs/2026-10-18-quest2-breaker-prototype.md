# Handoff — 2026-10-18 — Quest 2 Breaker Prototype

## Status

`Implemented` prototype only. The final Breaker Room asset is still `Planned`.

## Runtime flow

```text
Maintenance Wing Control Panel
  -> AccessResolved(access.control_room, PowerUnavailable)
  -> BreakerPanelPrototypeInteraction becomes available
  -> successful E interaction
  -> InteractionCompleted(interaction.breaker_panel, Success)
  -> Quest2ObjectiveTracker completes quest.2.repair_electrical_short
```

The repair does not consume or modify inventory and does not open the Control
Room. Repeated repair attempts return `AlreadyCompleted`.

## Prototype scene wiring

`TestScene` contains `BreakerPanel_Prototype` near the existing
`MaintenanceWingControlPanel`. Its required `GameplayEventHub` reference is
assigned. `GameplaySystems` contains `Quest2ObjectiveTracker`, also assigned
to the same event hub.

## Final asset integration runbook

Status: `Planned — not yet integrated`.

When `SM_BreakerPanel.fbx` arrives:

1. Inspect the imported hierarchy, colliders, materials, Animator, and any
   existing interaction scripts.
2. Preserve the stable interaction ID
   `interaction.breaker_panel`; do not derive identity from the object name.
3. Replace the prototype visual/collider with the approved asset or create a
   thin adapter on the asset root implementing `IInteractable`.
4. Assign the same `GameplayEventHub` reference in the Inspector.
5. Preserve the event sequence:
   `PowerUnavailable` enables the breaker, and successful repair publishes
   `InteractionCompleted(interaction.breaker_panel, Success)`.
6. Keep `Quest2ObjectiveTracker` unchanged; it must continue consuming the
   typed event rather than the asset or scene name.
7. Save, close, and reopen the scene. Verify no missing script or broken
   reference warnings.
8. Run the smoke test: breaker inactive before the electrical short, available
   after it, one objective completion after repair, and
   `AlreadyCompleted` on repeat.
9. Only after the smoke test succeeds, remove `BreakerPanel_Prototype` and
   record the replacement in the next handoff.

Rollback: disable the imported asset root and re-enable
`BreakerPanel_Prototype`; do not change Quest 2 contracts or objective IDs.
