# Architecture Baseline

**Status:** Accepted baseline for current coordination; implementation remains incremental.  
**Scope:** Inventory, Quest 1, Quest 2, Interaction, Cutscene, UI, Win/Lose.

## Tujuan

Menjaga setiap modul dapat dikerjakan oleh orang atau AI berbeda tanpa dependency tersembunyi dan tanpa hard-coded scene coupling.

## Compatibility-first decision

The existing Inventory implementation is preserved as current code and remains owned by its contributor. It may continue evolving without being treated as a stable internal API.

Quest modules must not depend directly on:

- `InventorySystem` private fields;
- `InventoryItem` internals;
- the inventory dictionary/list representation;
- `InventorySystem.current`;
- pickup GameObjects or scene names.

Quest integration will use a compatibility boundary:

```text
Quest
  -> abstract inventory/query contract
      -> compatibility adapter
          -> existing Inventory implementation
```

The adapter is the only layer allowed to translate the current Inventory API into the Quest contract. If the Inventory contributor changes internals later, the adapter is adjusted first; Quest contracts remain stable.

This decision intentionally does **not** require changing `InventoryItem`, `InventorySystem`, or `PhysicsPickup` at this stage.

## Modul dan kepemilikan

| Modul | Tanggung jawab | Tidak boleh bertanggung jawab atas |
|---|---|---|
| Inventory | Menyimpan item, query kepemilikan, perubahan inventory | Quest, cutscene, win/lose |
| Quest 1 | Objective dan progress Quest 1 | Detail implementasi inventory/interaksi |
| Quest 2 | Objective dan progress Quest 2 | Memanggil Quest 1 secara langsung |
| Interaction | Menerima input/interaksi dan menghasilkan result | Menentukan seluruh aturan quest |
| Cutscene | Timeline, Cinemachine, lock/unlock kontrol | Menyimpan progress quest |
| UI | Presentasi state/event | Menjadi sumber kebenaran gameplay |
| Win/Lose | Menang, timeout, retry flow | Mengelola inventory |
| Shared Contracts | ID, event, result, state interfaces | Logika fitur spesifik |
| Compatibility Adapter | Menerjemahkan implementation Inventory saat ini ke query/event contract | Memiliki inventory state sendiri atau mengubah Inventory internals |

## Dependency rules

Allowed:

```text
Interaction -> Shared Contracts
Inventory -> Shared Contracts
Quest -> Shared Contracts + query interface
Cutscene -> Shared Contracts
UI -> Shared Contracts + read-only queries
```

Disallowed without ADR:

```text
Quest 1 -> Quest 2 concrete class
Inventory -> Quest concrete class
Interaction object -> Cutscene concrete manager
UI -> private fields of gameplay modules
Any module -> GameObject.Find("...")
```

## Event boundary

Modul menerbitkan fakta yang terjadi. Modul lain merespons event tersebut. Modul tidak memanggil implementasi concrete modul lain hanya untuk memberi tahu bahwa sesuatu terjadi.

## UnityEvent policy

`UnityEvent` boleh dipakai untuk:

- Animator;
- AudioSource;
- VFX;
- local UI;
- camera/cutscene hook pada prefab.

Untuk state lintas modul, gunakan contract/event typed yang terdokumentasi. Setiap UnityEvent penting harus memiliki owner, target, dan cara validasi Inspector.

## Scene and prefab policy

- Prefab menyimpan perilaku reusable.
- ScriptableObject menyimpan definisi/data.
- Scene menyusun prefab dan referensi Inspector.
- `TestScene.unity` memiliki satu scene integrator; jangan diedit paralel tanpa koordinasi.

## Open decisions

- [ ] Bentuk final event bus/dispatcher.
- [ ] Bentuk final `ItemDefinition` dan item ID.
- [ ] Bentuk persistence quest.
- [ ] Owner tunggal untuk scene integration.
- [ ] Strategy untuk save/load dan retry.
- [ ] Bentuk `IInventoryQuery`/adapter final dan owner-nya.
- [ ] Cara adapter menerima perubahan item dari Inventory yang belum memiliki gameplay events.

## Status of current foundation

| Boundary | Status | Notes |
|---|---|---|
| `InventoryItemData` as item definition | Implemented | Existing ScriptableObject pattern is retained. |
| Existing `InventorySystem.Add/Remove` | Implemented/current | Do not refactor from Quest work. |
| Inventory public query contract | Planned | Quest must not read current internals directly. |
| Inventory compatibility adapter | Implemented foundation | Read-only adapter exists; scene wiring and event bridge remain planned. |
| Shared gameplay event payloads/interfaces | Implemented | Runtime contract exists; no scene producer/listener wiring yet. |
| `GameplayEventHub` | Implemented foundation | Explicit `MonoBehaviour`; Editor wiring must be saved and verified in `TestScene.unity`. |
| Quest 1 runtime logic | Incremental prototype | Objectives 1–5 have prototype coverage; panel installation, access, and area flow remain planned. |
