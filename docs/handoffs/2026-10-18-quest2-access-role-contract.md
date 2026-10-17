# Handoff — 2026-10-18 — Quest 2 Access-Role Contract

## Status

`Implemented` shared contract, isolated parser prototype, and UI controller
scripts; scene wiring and final access consumer remain `Planned`.

## Contract

`GameplayEventHub.PublishAccessRoleModified` publishes
`AccessRoleModifiedEvent` with:

- `AccessSubjectId`: `access_subject.id_card`;
- `PermissionValue`: `r--` or `rwx`;
- `RoleId`: `role.guest` or `role.control_room_administrator`.

The hub rejects empty subject/role IDs, unsupported permission values, and
mismatched permission/role pairs.
The event represents a virtual Quest 2 access-state fact only. It does not
change Inventory and does not execute an OS command.

## Validation

- `dotnet build .\Assembly-CSharp.csproj --no-restore`: passed with 0 warnings
  and 0 errors.
- Static audit confirmed no `Process`, shell, filesystem, or arbitrary command
  execution path in the parser.
- Unity Play Mode: pending; the parser has not yet been wired to a terminal
  UI or scene object.

## Parser prototype

`VirtualTerminalCommandParser` requires the Quest 2 hardware-connected state,
accepts only the documented whitelist, and publishes the event only for the
exact valid `chmod u=rwx id_card` command. Repeating the command is
idempotent and does not publish a duplicate event.

## UI controller prototype

`VirtualTerminalUI` presents parser output and forwards exact text from a
`TMP_InputField`. It disables the explicitly assigned
`PlayerInputScript` and `InteractionController` while open, then restores
both previous enabled states on Escape. The UI controller must remain on an
active GameObject under the scene's UI Canvas and separate from the panel
GameObject that it hides. Opening clears the stale interaction prompt while
controls are locked.
`ITDesktopTerminalInteraction` opens it only after hardware connection.

The repeatable scene setup is available through the Unity menu
`Quest 2/Build Terminal Prototype`, implemented in
`Assets/Editor/Quest2TerminalSceneSetup.cs`. The current project was locked by
an already-running Unity Editor during this session, so scene serialization
was not completed automatically. After running the menu action, save/reopen
the scene and perform Play Mode validation; do not implement the final scan in
that step.
