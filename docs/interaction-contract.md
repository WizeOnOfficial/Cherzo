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

### Gated pickup

- A pickup with a prerequisite must validate that prerequisite before calling
  the inventory adapter.
- A denied pickup must not publish `ItemAdded` or change inventory state.
- Quest 1 External Fuse pickup requires successful Hidden Message discovery.
- A gated pickup must not display a pickup prompt before its prerequisite is
  satisfied; it displays the prerequisite prompt instead.
- The gate reads the prerequisite owner's explicit state through an Inspector
  reference; it does not subscribe to or republish gameplay events.
- Prompt queries must be side-effect free; they must not write gameplay logs or
  mutate state because they can run every frame.

## Inspector checklist

- [ ] Interaction ID assigned.
- [ ] Requirements assigned.
- [ ] Result messages assigned.
- [ ] UnityEvent targets valid.
- [ ] Animator/Timeline references valid.
- [ ] No scene-name lookup.
- [ ] Missing references produce a visible validation error.
