# Handoff — <YYYY-MM-DD> — <Module/Goal>

## Session

- **Contributor/AI:** <name>
- **Branch:** `<branch>`
- **Base commit:** `<sha>`
- **Status:** In progress | Ready for review | Blocked
- **Contract status:** Implemented | Planned | Adjusted | Blocked

## Goal

What was this session intended to accomplish?

## Completed

- [ ] <item>

## Changed files

| File | Change | Risk |
|---|---|---|
| `<path>` | <summary> | Low/Medium/High |

## Contracts and dependencies

- Events added/changed: <list>
- Public API added/changed: <list>
- New asset references: <list>
- Modules touched: <list>
- Existing implementation preserved: Yes/No — explain any exception.
- Adapter/compatibility boundary touched: Yes/No

## Scene/Inspector changes

List exact GameObject, component, field, prefab, Animator, Timeline, or Input System changes. Include manual Unity Editor steps still required.

## Validation

- Command/Unity test: <command or procedure>
- Result: Pass | Fail | Not run
- Known warnings: <list>

## Remaining work

- [ ] <item>

## Risks/blockers

- <risk, unresolved conflict, missing reference, or design question>

## Done / not done / adjusted

### Done

- <item>

### Not done

- <item>

### Adjusted

- <item or `None`>

### Blocked

- <item or `None`>

## Next AI prompt

```text
Read project-brain.md, docs/architecture.md, docs/gameplay-contracts.md,
and this handoff. Continue only the remaining work listed above.
Do not change public contracts or scene ownership without documenting it.
```
