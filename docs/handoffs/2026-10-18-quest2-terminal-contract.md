# Handoff — 2026-10-18 — Quest 2 Simulated IT Terminal Contract

## Status

`Planned` contract; implementation has not started.

## Authoritative flow

```text
Control Room scan 1
  -> PowerUnavailable / electrical short
  -> Breaker Room repair
  -> Control Room scan 2
  -> role denied because card permission is r--
  -> IT Center
  -> find IT guide
  -> connect hardware to IT desktop
  -> open simulated terminal through in-game POV
  -> use help to learn the restricted command vocabulary
  -> chmod u=rwx id_card
  -> AccessRoleModified
  -> Control Room scan 3
  -> access granted
  -> enter Control Room
```

## Virtual permission model

| Virtual permission | Virtual role | Meaning |
|---|---|---|
| `r--` | `Guest` | Initial ID Card state |
| `rw-` | `Operator` | Intermediate puzzle state, optional |
| `rwx` | `Control Room Administrator` | Required state for final access |

The ID Card remains the same permanent `item.id_card` in Inventory. Only the
Quest 2 access-role state changes.

## Restricted commands

```text
help
help chmod
ls -l
cat id_card
chmod u=rwx id_card
clear
```

`chmod u=rwx id_card` is validated as game text by Unity. It must never be
passed to an OS shell, `Process`, PowerShell, Command Prompt, Linux runtime,
or host filesystem API.

## Implementation boundary

- IT desktop owns terminal presentation and command parsing.
- Quest 2 owns objective progress.
- The parser publishes a typed `AccessRoleModified` fact after successful
  validation.
- Control Room access consumes the typed fact and does not call the desktop
  directly.
- The guide and hardware steps remain separate prerequisites.

## Next bounded implementation

Implement only the shared typed role contract and the simulated terminal
parser after the guide/hardware prerequisite states are explicitly represented.
Do not implement final door access, OS command execution, or Inventory mutation.
