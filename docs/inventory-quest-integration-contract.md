# Inventory–Quest Integration Contract

**Status:** Contract baseline — query/mutation boundary implemented in the
compatibility adapter; pickup producer remains `Planned`.

**Scope:** Integrasi item antara Inventory, world pickup, Quest 1, Quest 2, UI,
dan sistem gameplay lain tanpa mengekspos internal Inventory.

## Tujuan

Quest harus dapat memeriksa kebutuhan item dan merespons perubahan item tanpa
mengetahui bagaimana Inventory menyimpan item, membuat stack, atau mengelola
object runtime.

Kontrak ini memungkinkan pemilik Inventory mengubah implementasi internal tanpa
memaksa Quest 1 dan Quest 2 ikut berubah.

## Boundary resmi

```text
World Pickup
    -> successful E pickup acquisition
    -> Inventory integration producer
    -> IInventoryCommand / Inventory adapter
    -> existing InventorySystem

Quest
    -> IInventoryQuery
    -> InventoryCompatibilityAdapter

Inventory change
    -> ItemChangedEvent
    -> GameplayEventHub
    -> Quest/UI/other read-only consumers
```

Quest tidak boleh melewati adapter untuk membaca atau mengubah Inventory.

## API yang boleh digunakan Quest

### Read-only query

Interface saat ini:

```csharp
public interface IInventoryQuery
{
    bool HasItem(string itemId, int requiredAmount = 1);
    int CountItem(string itemId);
}
```

Aturan:

- `itemId` harus stable ID, bukan nama GameObject;
- `requiredAmount` harus lebih besar dari nol;
- query tidak boleh mengubah state;
- query tidak boleh mengembalikan reference ke object scene;
- jika dependency tidak dikonfigurasi, adapter harus menghasilkan error yang terlihat,
  bukan success palsu.

Implementasi saat ini berada pada
[`InventoryCompatibilityAdapter.cs`](../Assets/Scripts/Quest/Inventory/InventoryCompatibilityAdapter.cs).

### Mutasi inventory

Quest tidak boleh memanggil `InventorySystem.Add` atau `InventorySystem.Remove`
secara langsung.

Target boundary:

```text
Add(itemId, amount)    -> InventoryMutationResult
Remove(itemId, amount) -> InventoryMutationResult
```

Status: `Implemented foundation` in `InventoryCompatibilityAdapter`; the
producer that calls it is still `Planned`.

Mutation result minimal harus membedakan:

- `Success`;
- `MissingItem`;
- `InsufficientAmount`;
- `InvalidItem`;
- `InvalidAmount`;
- `NotInitialized`;
- `Rejected`.

Tidak boleh ada operasi remove yang diam-diam berhasil ketika jumlah item tidak
mencukupi.

## Event item

Event adalah fakta bahwa perubahan telah berhasil terjadi, bukan perintah.

### `ItemAdded`

```text
ItemId: stable item ID
Amount: positive integer
```

Producer: Inventory integration boundary.  
Consumer: Quest, UI, telemetry jika diperlukan.

### `ItemRemoved`

```text
ItemId: stable item ID
Amount: positive integer
```

Producer: Inventory integration boundary.  
Consumer: Quest, UI, telemetry jika diperlukan.

Aturan event:

- publish hanya setelah mutation berhasil;
- tidak mengandung `GameObject`, `MonoBehaviour`, `InventoryItemData`,
  atau reference scene;
- tidak boleh dipakai sebagai perintah `Add`/`Remove`;
- producer tidak mengetahui daftar consumer;
- payload tidak boleh berubah tanpa update contract dan handoff;
- event dikirim satu kali untuk satu perubahan logis;
- amount selalu lebih besar dari nol.

Target publisher saat ini adalah
[`GameplayEventHub.cs`](../Assets/Scripts/Quest/Shared/GameplayEventHub.cs).

Status event bridge: `Implemented foundation` when `GameplayEventHub` is assigned
to the adapter. The event hub is a required dependency for mutation; producer
wiring remains `Planned`.

## Ownership dan lifecycle

| Concern | Owner | Quest boleh mengakses |
|---|---|---|
| Item definition dan stable ID | Inventory/data owner | ID melalui contract |
| Inventory storage dan stack | Inventory | Tidak |
| Add/remove mutation | Inventory boundary | Hasil mutation resmi |
| Pickup physics | Interaction/Pickup | Tidak langsung |
| Item objective progress | Quest | Query/event |
| Presentation pickup effect | Prefab/UnityEvent | Local response only |
| Inventory UI | UI | Read-only contract |

World pickup tidak otomatis dianggap masuk Inventory hanya karena memiliki
`referenceItem`. Integrasi pickup harus mendefinisikan dengan jelas:

1. kapan item dianggap diperoleh;
2. apakah item langsung masuk Inventory atau tetap dipegang secara physics;
3. apakah object world dihancurkan, dinonaktifkan, atau dapat dijatuhkan;
4. apakah item dapat diambil ulang;
5. bagaimana retry/reset mengembalikan state.

Approved lifecycle decision for Sprint 1:

- item dianggap diperoleh ketika `E` berhasil memanggil proses pickup;
- acquisition producer meneruskan stable item ID ke mutation boundary;
- event `ItemAdded` hanya diterbitkan setelah Inventory mutation berhasil;
- detail apakah object physics dihancurkan, dinonaktifkan, atau tetap dapat dijatuhkan
  masih `Planned` dan tidak boleh ditebak oleh Quest.

Status lifecycle: `Adjusted` — acquisition timing and disable-on-success behavior
are implemented for the prototype; retry/reset behavior remains `Planned`.

## Aturan Quest

Quest 1 dan Quest 2:

- hanya menerima `IInventoryQuery` dan event typed;
- tidak menyimpan salinan jumlah item sebagai authoritative state;
- tidak memanggil concrete `InventorySystem`;
- tidak memakai `GameObject.Find`;
- tidak membaca `PhysicsPickup.referenceItem` untuk menentukan progress;
- tidak mengandalkan nama object scene;
- tidak membaca field private atau collection internal Inventory;
- tidak mengaktifkan baris legacy `InventorySystem.current.Add(...)`;
- menyelesaikan objective berdasarkan stable ID dan event/result resmi.

Contoh dependency yang dilarang:

```text
Quest1Controller -> InventorySystem.inventory
Quest1Controller -> InventoryItem.stackSize
Quest1Controller -> PhysicsPickup.referenceItem
Quest1Controller -> InventorySystem.current
```

## Adapter compatibility policy

Jika teman mengubah `InventorySystem`, hanya compatibility layer yang boleh
disesuaikan terlebih dahulu.

Urutan perubahan:

1. Inventory owner mengubah implementasi internal.
2. Adapter menyesuaikan mapping/query/mutation.
3. Contract test dijalankan.
4. Gameplay event bridge diverifikasi.
5. Quest tidak diubah kecuali contract memang berubah.
6. Jika contract berubah, buat ADR atau migration note sebelum implementasi.

Adapter tidak boleh:

- menduplikasi authoritative inventory state;
- melakukan polling tersembunyi tanpa dokumentasi;
- mengubah object Inventory tanpa mutation boundary;
- menyembunyikan error konfigurasi;
- mengubah stable item ID secara diam-diam.

## Stable item IDs

ID saat ini:

| Item | Stable ID | Definition |
|---|---|---|
| ID Card | `item.id_card` | `Assets/ScriptableObject/QuestItems/ID Card.asset` |
| External Fuse | `item.external_fuse` | `Assets/ScriptableObject/QuestItems/External Fuse.asset` |

ID tidak boleh berasal dari:

- nama GameObject;
- display name;
- nama prefab;
- posisi scene;
- instance ID Unity.

## Acceptance criteria

### Boundary

- [x] `IInventoryQuery` tersedia.
- [x] Read-only adapter tersedia.
- [x] Stable ID ID Card dan External Fuse tersedia.
- [x] Quest tidak membaca Inventory internals.
- [x] Mutation boundary resmi tersedia pada adapter.
- [x] Item event bridge tersedia pada adapter + `GameplayEventHub`.
- [ ] Contract tests untuk valid/invalid item dan amount tersedia.

### Pickup integration

- [x] `PhysicsPickup.referenceItem` dapat menunjuk item definition.
- [x] Existing pickup/drop tetap bekerja.
- [x] Acquisition moment didefinisikan: successful `E` pickup.
- [x] Pickup-to-inventory producer disetujui dan diimplementasikan for the prototype.
- [ ] Duplicate pickup/reset behavior didefinisikan.

### Unity integration

- [x] `GameplaySystems` memiliki event hub dan adapter pada prototype `TestScene`.
- [ ] Production scene target ditentukan.
- [ ] Inspector runbook tersedia di
  [`quest-work-log.md`](quest-work-log.md).
- [ ] Scene production wiring diverifikasi melalui save/reopen dan smoke test.

## Status dan next step

**Implemented:**

- Query boundary dan compatibility adapter foundation.
- Typed item event payload dan event publisher foundation.
- Stable item definitions untuk Sprint 1 prototype.
- `InventoryPickupAcquisition` producer and disable-on-success behavior for the
  prototype pickup path.
- Quest 1 may consume `ItemAdded` through a dedicated objective tracker.

**Planned:**

- Production-scene pickup wiring.
- Retry/reset behavior for acquired item objects.
- Contract tests.
- Quest 1 runtime consumer.

**Blocked:**

- Perilaku object world setelah acquisition masih belum diputuskan.
- Keputusan konsumsi External Fuse saat dipasang.
- Keputusan retry/reset inventory state.

**Next bounded implementation step:**

Implementasi berikutnya harus membuat contract test atau validation harness untuk
`IInventoryQuery` dan item IDs terlebih dahulu. Jangan mengaktifkan legacy mutation
di `PhysicsPickup` dan jangan membuat Quest controller sebelum lifecycle pickup
disetujui.
