# Interaction Contract

**Status:** DRAFT — berlaku untuk Control Panel, Computer, Card Reader, pickup, dan object interaktif lain.

## Interaction lifecycle

```text
Player targets object
  -> object exposes prompt
  -> player submits interaction
  -> object validates requirements/state
  -> object returns explicit result
  -> successful state change emits gameplay event
  -> UnityEvent may drive local presentation
```

## Interaction definition

| Field | Meaning |
|---|---|
| Interaction ID | Stable identifier, not GameObject name |
| Prompt | Player-facing text |
| Requirements | Item/state/access requirements |
| Result | Success/Denied/MissingRequirement/etc. |
| State transition | Exact state changed on success |
| Gameplay event | Fact emitted after success |
| UnityEvent | Local Animator/audio/VFX/camera hook |

## Object contracts

### Control Panel

- Can inspect/open cover.
- Inspection is a distinct successful interaction result before later panel
  objectives may advance.
- Can install external fuse when requirement is satisfied.
- Emits `FuseInstalled` after successful installation.
- Must not call Quest 1/Quest 2 directly.

### Computer

- Can connect hardware.
- Can modify access role after requirements are satisfied.
- Emits `ComputerAccessModified`.

### Card Reader

- Checks access through an access/query contract.
- Emits either `AccessDenied` or `AccessGranted`.
- Must not decide quest completion itself.

## Inspector checklist

- [ ] Interaction ID assigned.
- [ ] Requirements assigned.
- [ ] Result messages assigned.
- [ ] UnityEvent targets valid.
- [ ] Animator/Timeline references valid.
- [ ] No scene-name lookup.
- [ ] Missing references produce a visible validation error.
