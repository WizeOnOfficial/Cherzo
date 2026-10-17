# Git and Collaboration Workflow

## Branches

```text
main                         stable
develop                      integration branch
feature/inventory-foundation one module/goal
feature/quest-1              one module/goal
feature/quest-2              one module/goal
feature/interactions         one module/goal
feature/cutscene             one module/goal
```

Do not share one feature branch concurrently. Do not push unfinished work directly to `main` or `develop`.

## Ownership

| Asset type | Rule |
|---|---|
| Runtime script | Module owner changes it |
| ScriptableObject definition | Definition owner changes it |
| Prefab | Prefab owner changes it |
| `TestScene.unity` | One scene integrator at a time |
| `.meta` | Always commit with its asset |
| Contract docs | Any API/behavior owner updates them |

## Start of work

```text
git status
git switch <your-feature-branch>
git pull --rebase origin <your-feature-branch>
```

If local changes are present, stop and decide whether to commit, stash, or discard them. Never overwrite another contributor's work silently.

## Commit rules

Use focused commits:

```text
Add inventory item query contract
Add Quest 1 objective definitions
Wire control panel fuse event
Document card reader interaction result
```

Avoid broad messages such as `fix everything`.

## Pull and integration

1. Finish or commit local work.
2. Pull/rebase the target branch.
3. Resolve conflicts deliberately.
4. Inspect Unity YAML and `.meta` diffs.
5. Run validation.
6. Open a PR with changed files, dependencies, scene/asset changes, and test steps.

## PR checklist

- [ ] Scope is one module/goal.
- [ ] No unrelated file churn.
- [ ] Contracts/docs updated.
- [ ] Scene/prefab changes listed.
- [ ] `.meta` files preserved.
- [ ] Validation result recorded.
- [ ] Manual Unity Editor steps recorded.
- [ ] Handoff created if work is incomplete.
