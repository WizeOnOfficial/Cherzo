# Inventory Contract

**Status:** Compatibility contract — existing implementation is preserved and actively evolving.

## Important ownership rule

The current Inventory code is not being refactored by Quest work. Its contributor owns its internal implementation and may continue improving it.

This document describes the boundary that Quest and other modules are allowed to consume. It does not require the current implementation to expose every planned operation immediately.

## Responsibility

Inventory menyimpan item dan menyediakan query/mutasi resmi. Inventory tidak memanggil Quest, Cutscene, UI concrete class, atau object scene.

## Existing implementation baseline

Currently observed implementation:

- `InventoryItemData` is a ScriptableObject item definition.
- `InventorySystem` owns a dictionary and list.
- `InventorySystem.Add(InventoryItemData)` and `Remove(InventoryItemData)` are the current mutation methods.
- `InventoryItem` stores item definition and stack count.
- `PhysicsPickup.referenceItem` links a world pickup to an item definition.

These are **current implementation facts**, not a promise that Quest may access their internals. Quest integration must be added through a compatibility adapter.

## Item definition

Item gameplay harus memiliki definisi kanonik:

- stable item ID;
- display name;
- icon;
- prefab/reference visual bila diperlukan;
- stack policy.

Sumber data: ScriptableObject item definition. Jangan gunakan nama GameObject scene sebagai identitas item.

## Target boundary operations

| Operation | Input | Output | Side effect |
|---|---|---|---|
| `Add` | item ID/definition, amount | result | `ItemAdded` jika berhasil |
| `Remove` | item ID, amount | result | `ItemRemoved` jika berhasil |
| `Has` | item ID, amount optional | bool | none |
| `Count` | item ID | integer | none |

Status: `Planned` unless an operation is explicitly marked as implemented in a handoff.

## Compatibility adapter rules

- The adapter may hold a serialized reference to the current Inventory service.
- The adapter translates item-definition references into the stable item contract.
- Quest consumes read-only queries and gameplay-level results, not `InventorySystem.inventory`.
- The adapter must not duplicate authoritative inventory state.
- If the current Inventory has no event, Quest integration must use a documented bridge or an explicit query strategy; do not invent hidden polling.
- Adapter changes require a handoff and a focused validation.

## Rules

- Null/invalid item ditolak dengan error/result yang jelas.
- Stack tidak boleh menjadi negatif.
- Item consumption harus eksplisit.
- Inventory event tidak boleh mengandung reference scene yang rapuh.
- Quest hanya memakai public API/query dan event.

## Acceptance criteria

- [ ] Add, Remove, Has, Count tersedia.
- [ ] Item ID stabil dan terdokumentasi.
- [ ] Tidak ada `new MonoBehaviour`.
- [ ] Tidak ada `InventorySystem.current` tanpa keputusan singleton tertulis.
- [ ] Pickup dapat menerbitkan/memicu penambahan item melalui kontrak resmi.
- [ ] Test untuk item invalid, stack, remove berlebih, dan event.
- [ ] Manual Inspector wiring terdokumentasi.

## Status

| Capability | Status | Next adjustment |
|---|---|---|
| Existing item definitions | Implemented | Add new assets without renaming existing definitions. |
| Existing Add/Remove implementation | Implemented/current | `InventoryItem` is a serializable data class so `InventorySystem.Add/Remove` can construct and query it without Unity `new MonoBehaviour` warnings. |
| Stable read-only query boundary | Implemented foundation | `InventoryCompatibilityAdapter` provides `IInventoryQuery`; Inspector wiring remains. |
| Item-added/removed gameplay events | Planned | Decide bridge strategy with Inventory contributor. |
| Null/stack validation | Not part of current Quest scope | Inventory contributor may address independently. |
