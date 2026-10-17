# Quest Coding Rules

**Status:** Accepted for Quest 1 and Quest 2 foundation.  
**Scope:** New Quest, shared gameplay contract, interaction bridge, adapter, and quest-state code.

## Dependency rules

- Quest code may depend on shared contracts and read-only query interfaces.
- Quest code must not depend on Inventory private fields, dictionary/list representation, or `InventorySystem.current`.
- Quest code must not call Quest 2 from Quest 1 or Quest 1 from Quest 2.
- Interaction objects publish results/events; they do not call concrete Quest managers.
- `GameplayEventHub` is an explicitly referenced scene component, never a singleton.
- No `GameObject.Find`, `FindObjectOfType`, scattered GameObject names, or hidden static gameplay state.

## Data and identifiers

- Use IDs from `QuestGameplayIds`; do not repeat literal IDs in feature scripts.
- Do not use display names as gameplay identity.
- Do not use inventory indexes as item identity.
- New IDs must be added to the shared contract and documented before use.
- Existing IDs are immutable once serialized into assets or scene data.

## State and events

- A quest owns objective progress; Inventory owns item ownership; interaction objects own local interaction state.
- An event is a fact that already happened, not a command.
- Event payloads must contain stable IDs and enough context for consumers.
- UnityEvent is for local Inspector presentation (Animator, audio, VFX, camera hook), not the sole source of cross-module gameplay state.
- Do not duplicate authoritative state inside an adapter.
- Invalid input must produce an explicit result or error; never silently advance a quest.
- Simulated terminal commands must be parsed and validated inside Unity; never
  execute OS shell commands, call `Process`, access host files, or accept an
  arbitrary command interpreter for gameplay.
- Access-role changes must publish typed gameplay facts and must not mutate the
  permanent `item.id_card` Inventory item.

## Unity component rules

- Use `[SerializeField] private` for scene references.
- Validate required references in `Awake`/`OnValidate` where appropriate.
- Do not add scene wiring in code when an explicit Inspector reference is required.
- Keep editor-only APIs out of runtime Quest assemblies.
- Preserve `.meta` files and serialized references.
- Do not modify `TestScene.unity` during foundation work.

## Implementation status discipline

- Mark unavailable integrations as `Planned` or `Blocked`.
- Do not create a fake Inventory adapter that pretends to be authoritative.
- Every public contract change requires a documentation update and, if incompatible, an ADR.
- Every session must report `Done`, `Not done`, `Adjusted`, and `Blocked`.

## Validation rules

- Compile/type-check the changed scripts.
- Test invalid event inputs and no-subscriber behavior.
- Test event order at the Quest boundary before scene wiring.
- Record Unity Editor manual steps separately from code validation.
