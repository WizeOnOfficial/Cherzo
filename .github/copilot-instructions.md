# Project Guidelines

## Read Before Editing

This is a Unity 6 project imported from Git. Before changing code or assets:

1. Read `project-brain.md`.
2. Read `docs/architecture.md`.
3. Read the contract document relevant to the task.
4. Read the latest applicable handoff under `docs/handoffs/`.
5. Inspect `git status` and do not overwrite unrelated local changes.
6. Check the status and open decisions in `docs/README.md`.

## Architecture

- Treat `docs/architecture.md` and `docs/gameplay-contracts.md` as the shared design baseline.
- Treat the existing Inventory implementation as current, actively evolving code owned by its contributor.
- Do not refactor, rename, relocate, or "fix" existing Inventory code unless the Inventory owner explicitly requests it.
- Quest code must depend on an abstract inventory/query contract or compatibility adapter, never on Inventory internals.
- Keep Inventory, Quest 1, Quest 2, Interaction, Cutscene, UI, and Win/Lose as separate modules.
- Prefer typed contracts, explicit references, ScriptableObject definitions, and events over direct manager-to-manager calls.
- UnityEvent is for Inspector-configured presentation or local prefab responses; it is not the sole source of cross-module gameplay state.
- Do not use `GameObject.Find`, scattered string names, hidden static state, or new singletons for gameplay coupling.
- Do not change a public API, event payload, asset schema, or scene ownership rule without updating the relevant contract or adding an ADR.
- When a contract is not implemented yet, document it as `Planned` or `Blocked`; do not silently implement a substitute.

## Unity and Asset Rules

- Preserve `.meta` files and serialized GUID references.
- Treat `.unity`, prefab, Animator, Timeline, and UI asset changes as high-risk.
- Do not edit `TestScene.unity` concurrently with another contributor. Prefer prefab and ScriptableObject changes.
- When a scene or Inspector change is necessary, document the exact manual Unity Editor steps and the affected object/field.
- Keep editor-only code out of runtime assemblies.

## Scope and Git

- Work only on the requested module and explicitly approved files.
- Do not perform broad refactors or formatting unrelated files.
- Use focused commits and feature branches. Never force-push or rewrite another contributor's branch.
- Before handoff, report changed files, new dependencies, contract changes, validation performed, and remaining risks.
- At the end of every task, report each relevant item as `Done`, `Not done`, `Adjusted`, or `Blocked`, including the next required adjustment.

## Build and Validation

- Use the project's existing Unity version and packages.
- Run the smallest relevant validation available; do not claim the Unity Editor was validated if it was not opened.
- For serialized Unity changes, inspect the diff carefully and verify references are still valid.

## Documentation

- Update the contract documentation whenever behavior or dependency boundaries change.
- Documentation must distinguish `Implemented`, `Planned`, `Blocked`, and `Assumption`.
- For cross-AI continuity, create a handoff from `docs/handoffs/TEMPLATE.md`.
- For a lasting architecture decision, copy `docs/decisions/ADR-000-template.md` and assign the next ADR number.
