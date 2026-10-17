# Quest Work Log

Dokumen ini adalah catatan kerja untuk modul Quest 1/Quest 2. Gunakan dokumen ini
sebagai ringkasan sebelum commit, push, pull, atau handoff ke AI/anggota tim lain.

## Aturan penggunaan

- Satu entri dibuat untuk satu sesi kerja atau satu perubahan terukur.
- Catat file yang berubah, scene/prefab yang disentuh, dan validasi yang dilakukan.
- Jangan menandai `Done` hanya karena kode dibuat; lakukan validasi yang sesuai.
- Catat perubahan kontrak, dependency, dan risiko konflik secara eksplisit.
- Jangan memasukkan secret, credential, atau data pribadi ke dalam log.
- Setelah entri selesai, buat handoff bila perubahan memengaruhi AI/kontributor berikutnya.

## Format runbook integrasi

Setiap perubahan yang memerlukan Unity Editor harus memiliki dua bagian:

1. **Audit record** — apa yang berubah, mengapa, file/asset yang disentuh,
   dependency, risiko konflik, dan hasil validasi.
2. **Integration runbook** — langkah klik yang dapat diulang pada project/scene
   game sebenarnya, bukan hanya pada `TestScene`.

Runbook wajib menjelaskan:

- scene atau prefab target;
- object Hierarchy yang dipilih;
- component dan field Inspector yang diubah;
- asset yang di-drag atau dibuat;
- nilai ID/reference yang diharapkan;
- langkah save dan reopen;
- smoke test dan hasil yang diharapkan;
- apakah perubahan aman diterapkan ke scene produksi;
- langkah rollback manual bila hasil validasi gagal.

Jika perubahan masih berupa prototype di `TestScene`, tulis jelas:
`Prototype only — not yet integrated into production scene`.

## Status legend

- **Done** — selesai dan tervalidasi.
- **Not done** — belum dikerjakan.
- **Adjusted** — ada perubahan sadar terhadap rencana/kontrak.
- **Blocked** — tidak dapat dilanjutkan karena dependency atau keputusan yang belum tersedia.

## Checklist sebelum commit/push

### Scope dan repository

- [ ] Membaca `project-brain.md`, `docs/README.md`, dan kontrak yang relevan.
- [ ] Menjalankan `git status` sebelum mulai dan sebelum commit.
- [ ] Tidak membawa perubahan unrelated ke commit.
- [ ] Tidak mengubah file milik modul/kontributor lain tanpa koordinasi.
- [ ] Tidak menghapus atau mengganti `.meta` dan serialized GUID secara manual.

### Unity Editor

- [ ] Scene yang diubah adalah scene yang memang berada dalam scope.
- [ ] Scene disimpan dengan `Ctrl+S`.
- [ ] Scene ditutup dan dibuka kembali bila ada perubahan Inspector reference.

### 2026-10-15 — Control Panel prototype decision

**Decision:** Use a dedicated placeholder GameObject for the first Control Panel
interaction test. The placeholder is for `TestScene` validation only and must
not be treated as the final environment asset.

**Scope for the next iteration:**

- verify that the placeholder can be targeted by the existing interaction ray;
- verify that pressing `E` produces a visible Console log;
- do not change Inventory, pickup behavior, door state, or production assets;
- do not claim any Quest 1 objective completion yet.

**Status:**

- **Done:** prototype approach selected and bounded.
- **Not done:** placeholder creation, interaction component, and Quest 1 wiring.
- **Adjusted:** Control Panel implementation will be incremental instead of
  integrating an assumed final asset.
- **Blocked:** final Control Panel mesh, placement, animation, and presentation
  remain dependent on the production environment asset.

**Integration runbook — next manual Unity step:**

1. Open `Assets/Scenes/TestScene.unity`.
2. Create an empty GameObject named `ControlPanel_Prototype`.
3. Add a visible primitive child only if needed for targeting; keep it clearly
   marked as a prototype.
4. Do not add Quest or Inventory references yet.
5. Save the scene only after confirming no unrelated Inspector fields changed.
6. Stop and report the Console result before the interaction component is added.
- [ ] Tidak ada reference Inspector penting yang menjadi `Missing`.
- [ ] Tidak ada compile error baru di Console.
- [ ] Play Mode smoke test dilakukan untuk movement, interaction, pickup, dan drop
      bila perubahan dapat memengaruhinya.

### Kontrak dan arsitektur

- [ ] Quest tidak membaca internals `InventorySystem`.
- [ ] Tidak ada `GameObject.Find`, singleton baru, atau input keyboard langsung
      di dalam Quest.
- [ ] ID item/interaksi memakai ID kanonik dari shared contract.
- [ ] Perubahan public API, event payload, atau ownership sudah didokumentasikan.
- [ ] UnityEvent hanya digunakan untuk respons lokal/presentation, bukan satu-satunya
      sumber state lintas modul.

### Validation dan handoff

- [ ] Perintah build/test yang relevan sudah dijalankan atau alasan tidak dijalankan
      dicatat.
- [ ] File yang berubah sudah diperiksa dengan `git diff`.
- [ ] Status `Done`, `Not done`, `Adjusted`, dan `Blocked` diisi.
- [ ] Handoff dibuat jika ada dependency atau pekerjaan lanjutan untuk AI/kontributor lain.

## Current baseline

### Done

- Shared Quest IDs dan typed payloads tersedia.
- `GameplayEventHub` tersedia sebagai explicit scene component.
- `InventoryCompatibilityAdapter` tersedia sebagai read-only query boundary.
- `GameplaySystems` telah dikonfigurasi di Unity Editor berdasarkan Inspector snapshot.
- `Inventory System` pada adapter telah diisi dengan component `InventorySystem`.
- Asset `ID Card` dan `External Fuse` telah dibuat di
  `Assets/ScriptableObject/QuestItems`.
- Pickup scene telah diberi `referenceItem`:
  - `Item (1)` -> `External Fuse`;
  - `Item` -> `ID Card`.
- Pickup dan drop existing tetap berjalan setelah smoke test terakhir.
- Acquisition smoke test berhasil:
  - `E` pada ID Card memasukkan item melalui adapter;
  - object ID Card langsung dinonaktifkan;
  - item tidak masuk `Pickup Holder`;
  - `Q` tidak diperlukan untuk acquisition;
  - perilaku yang sama berhasil untuk External Fuse.

### Not done

- Reopen verification setelah perubahan Inspector berikutnya tetap diperlukan.
- Inventory item event bridge tersedia sebagai foundation melalui adapter dan hub.
- Adapter mutation bridge sudah dikonfigurasi di Inspector.
- Quest 1 runtime belum dibuat.
- Quest 2 runtime belum dibuat.
- Pickup object pada production scene belum dihubungkan ke item definition.

### Adjusted

- Dokumentasi sekarang membedakan konfigurasi Editor yang terlihat pada snapshot
  dengan wiring scene yang sudah tersimpan di Git.

### Blocked

- Quest listener untuk `ItemAdded`/`ItemRemoved` belum dibuat.
- Finalisasi flow item consumption dan retry/reset masih menunggu keputusan gameplay.
- Boundary integrasi Inventory–Quest telah didokumentasikan; implementasi mutation
  dan event bridge tersedia pada adapter.
- `IInventoryCommand` dan `InventoryMutationResult` telah dibuat; adapter kini
  memvalidasi item definition dan wajib memiliki `GameplayEventHub`.
- `InventoryPickupAcquisition` telah dibuat sebagai producer terpisah; pickup yang
  berhasil dimutasi ke Inventory dinonaktifkan dari dunia.
- Keputusan lifecycle disetujui: successful `E` pickup menjadi acquisition moment;
  object world dinonaktifkan setelah mutation berhasil pada prototype.

## Session entries

### 2026-10-15 — Quest foundation and item definitions

**Owner:** Quest contributor  
**Scope:** Shared Quest foundation, adapter wiring, and item-definition preparation.

**Changed files/assets:**

- `Assets/Scripts/Quest/Shared/QuestGameplayContracts.cs`
- `Assets/Scripts/Quest/Shared/GameplayEventHub.cs`
- `Assets/Scripts/Quest/Inventory/InventoryCompatibilityAdapter.cs`
- `Assets/ScriptableObject/QuestItems/ID Card.asset`
- `Assets/ScriptableObject/QuestItems/External Fuse.asset`
- Related documentation under `docs/`

**Unity verification:**

- `GameplaySystems` contains `GameplayEventHub`.
- `GameplaySystems` contains `InventoryCompatibilityAdapter`.
- `InventoryCompatibilityAdapter.Inventory System` references `InventorySystem`.
- `Item (1).PhysicsPickup.Reference Item` references `External Fuse`.
- `Item.PhysicsPickup.Reference Item` references `ID Card`.
- Existing pickup/drop smoke test passed after scene save/reopen:
  - player movement remains functional;
  - `E` pickup works;
  - `Q` drop works;
  - dropped item can be picked up again;
  - no new gameplay compile error was observed.
- `TestScene` was saved; reopen verification remains required after future Inspector edits.

**Integration runbook — current prototype:**

1. Open `Assets/Scenes/TestScene.unity`.
2. Select `GameplaySystems` in Hierarchy.
3. Confirm `GameplayEventHub` and `InventoryCompatibilityAdapter` are present.
4. Confirm `InventoryCompatibilityAdapter.Inventory System` references the existing
   `InventorySystem` component.
5. Select `Item (1)` and assign
   `Assets/ScriptableObject/QuestItems/External Fuse` to
   `PhysicsPickup.Reference Item`.
6. Select `Item` and assign
   `Assets/ScriptableObject/QuestItems/ID Card` to
   `PhysicsPickup.Reference Item`.
7. Press `Ctrl+S`, close the scene, and reopen it.
8. Verify the three Inspector references remain assigned.
9. Enter Play Mode and test movement, `E` pickup, `Q` drop, and pickup again.
10. Do not enable the commented Inventory mutation in `PhysicsPickup.cs`.

**Production-scene integration status:**

- `TestScene`: Implemented prototype wiring.
- Production gameplay scene: Not done.
- Before applying to a production scene, identify the real ID Card and External Fuse
  pickup objects and repeat this runbook on those objects only.
- Do not copy scene YAML manually; use Unity Inspector so serialized GUIDs remain valid.

**Manual rollback:**

If the smoke test fails, stop Play Mode and clear only the affected
`PhysicsPickup.Reference Item` field, then restore the previous scene version using
the team's Git workflow. Do not delete the ScriptableObject assets.

**Contract/dependency impact:**

- No Inventory internals were refactored.
- No public Inventory API was changed.
- No Quest runtime producer/consumer was added.
- Item IDs use `item.id_card` and `item.external_fuse`.

**Status:**

- **Done:** foundation and manual Editor configuration.
- **Not done:** runtime Quest progression and event bridge.
- **Adjusted:** documentation now distinguishes saved `TestScene` prototype wiring
  from future production-scene integration.
- **Blocked:** Inventory event bridge and final item lifecycle policy.

**Next required adjustment:**

1. Buat listener Quest 1 pertama untuk event `ItemAdded` ID Card.
2. Konfigurasi listener pada object Quest terpisah di Unity Inspector.
3. Uji objective tanpa mengubah Inventory atau input player.

### 2026-10-15 — Inventory command boundary

**Changed files:**

- `Assets/Scripts/Quest/Shared/QuestGameplayContracts.cs`
- `Assets/Scripts/Quest/Inventory/InventoryCompatibilityAdapter.cs`
- `docs/inventory-quest-integration-contract.md`

**Implemented:**

- `IInventoryCommand` with explicit add/remove result vocabulary.
- Adapter mapping from stable item ID to configured `InventoryItemData`.
- Adapter mutation calls existing `InventorySystem.Add/Remove` only behind the boundary.
- Successful mutation publishes `ItemAdded`/`ItemRemoved` through the required
  `GameplayEventHub`.
- Invalid item, amount, dependency, and insufficient amount return explicit results.

**Not implemented:**

- No Quest objective consumer.

**Validation:**

- Problems check: no errors in changed C# files.
- Documentation diff check: passed.
- Unity Inspector configuration: pending next manual step.

**Next manual Unity step:**

1. Add `InventoryPickupAcquisition` only to the two prototype item objects.
2. Assign `GameplaySystems/InventoryCompatibilityAdapter` to each producer.
3. Keep `PhysicsPickup.Reference Item` unchanged.
4. Save, reopen, and test one item at a time.

### 2026-10-15 — Pickup acquisition integration

**Implemented:**

- `PhysicsPickup` delegates to `InventoryPickupAcquisition` when configured.
- Successful mutation publishes the item event and disables the world pickup.

**Unity verification:**

- ID Card acquisition passed.
- External Fuse acquisition passed.
- Existing movement remained functional.
- The two configured prototype pickups bypass the physics-held path.

**Not done:**

- Quest 1 event listener/objective state.
- UI objective presentation.
- Production-scene wiring.
- Retry/reset behavior.

**Next bounded step:**

Implement the first Quest 1 listener for the ID Card acquisition event, then
configure it on a dedicated Quest object without modifying Inventory internals.

### 2026-10-15 — Quest 1 ID Card objective listener

**Implemented:**

- Added stable Quest 1 and `Find ID Card` objective IDs.
- Added `Quest1ObjectiveTracker` as an event consumer.
- Tracker subscribes to `ItemAdded`, checks only the canonical ID Card ID, and
  publishes `QuestObjectiveCompleted`.
- Tracker can initialize from the read-only inventory query when enabled.

**Not done:**

- No control panel, hidden message, fuse, door, or Maintenance Wing logic.
- No UI presentation.
- No Quest 2 runtime.

**Next manual Unity step:**

1. Create a dedicated `Quest1` GameObject.
2. Add `Quest1ObjectiveTracker`.
3. Assign `GameplaySystems` to `Event Hub`.
4. Assign `GameplaySystems` to `Inventory Query`.
5. Save/reopen and test ID Card acquisition.

### 2026-10-15 — Control Panel prototype interaction

**Implemented:**

- Added `ControlPanelPrototypeInteraction`, an `IInteractable` component for the
  temporary `ControlPanel_Prototype` object.
- The component only exposes an inspect prompt and writes a Console log when
  `E` is pressed.
- No Inventory, Quest objective, door, fuse, or final environment asset is
  modified by this iteration.

**Integration runbook:**

1. Select `ControlPanelVisual_Prototype` in `Assets/Scenes/TestScene.unity`
   (the object that has the Collider).
2. Add `ControlPanelPrototypeInteraction` to that same object.
3. Confirm the visual child has a Collider and the component is on the same
   GameObject as that Collider. Do not add a second Collider just for the script.
4. Save the scene.
5. Enter Play mode and aim the center reticle at the prototype.
6. Confirm the prompt is `Press E to Inspect Control Panel`.
7. Press `E` and confirm this log:
   `Control Panel prototype interacted. Interaction ID: interaction.control_panel.`
8. Stop Play mode and report the log result before adding Quest state.

**Status:**

- **Done:** prototype interaction component added.
- **Not done:** initial access denial objective, panel inspection state, hidden
  message, fuse installation, and Maintenance Wing progression.
- **Adjusted:** this first interaction is intentionally log-only so it cannot
  falsely advance Quest 1.
- **Blocked:** final panel presentation and scene placement depend on the
  production Control Panel asset.

### 2026-10-15 — Control Panel initial access attempt

**Implemented:**

- The prototype now queries the Inventory adapter for the canonical ID Card ID.
- Without the ID Card, it logs `MissingRequirement` and does not advance state.
- With the ID Card, the first attempt records a denied initial access and logs
  the reason that inspection is required.
- The interaction publishes `InteractionCompleted` with an explicit result.
- Repeated attempts log `AlreadyCompleted` and do not reset or reopen the panel.

**Integration runbook:**

1. Select `ControlPanelVisual_Prototype`, where the Collider and interaction
   component are attached.
2. Assign `GameplaySystems/GameplayEventHub` to `Event Hub`.
3. Assign `GameplaySystems/InventoryCompatibilityAdapter` to `Inventory Query`.
4. Save the scene and enter Play mode.
5. First test without collecting the ID Card. Expected log:
   `Control Panel access denied. Reason: missing 'item.id_card'.`
6. Stop Play mode, collect the ID Card, then interact with the panel. Expected:
   `Control Panel initial access denied. ID Card present, but the panel requires inspection.`
7. Interact a second time. Expected:
   `Control Panel initial access was already attempted. Panel remains locked.`
8. Do not expect the door, hidden message, fuse, or Quest 1 completion to change.

**Status:**

- **Done:** initial Control Panel access validation and explicit result logging.
- **Not done:** panel inspection state, hidden message, fuse installation,
  Maintenance Wing access, and Quest 1 completion.
- **Adjusted:** the placeholder owns only its local initial-attempt state; Quest
  progression remains event-driven and is not advanced by this iteration.
- **Blocked:** final panel behavior still depends on the production asset and
  the agreed inspection interaction.

### 2026-10-15 — Control Panel reference wiring diagnosis

**Finding:**

- The saved `TestScene` contains the `GameplayEventHub` reference on
  `ControlPanelPrototypeInteraction`.
- The saved component does not contain an `Inventory Query` reference.
- This explains why the panel reports the ID Card as missing after the pickup
  succeeds; `Quest1ObjectiveTracker` and the panel were not using the same
  serialized adapter reference.

**Required manual correction:**

1. Select `ControlPanelVisual_Prototype`.
2. On `Control Panel Prototype Interaction`, assign
   `GameplaySystems/InventoryCompatibilityAdapter` to `Inventory Query`.
3. Save with `Ctrl+S`.
4. Stop and restart Play mode so the scene starts with a clean inventory.
5. Collect the ID Card, then interact with the panel.

**Expected result after correction:**

`Control Panel initial access denied. ID Card present, but the panel requires inspection.`

**Status:**

- **Done:** root cause identified from serialized scene data.
- **Not done:** corrected reference and repeated play-mode validation.
- **Adjusted:** no code change is needed for this wiring issue.
- **Blocked:** next Quest 1 objective remains blocked until the corrected
  reference is saved and verified.

  ### 2026-10-15 — Quest 1 Control Panel inspection

  **Implemented:**

  - Added a distinct `interaction.control_panel_inspection` ID and
    `quest.1.inspect_control_panel` objective ID.
  - After the initial Control Panel access has been denied, the next interaction
    records a successful inspection.
  - The inspection publishes `InteractionCompleted` with `Success`.
  - `Quest1ObjectiveTracker` accepts the inspection only after objective 2 is
    complete, then logs and publishes objective 3 completion.
  - Repeated inspection attempts return `AlreadyCompleted` and do not publish
    duplicate quest progress.

  **Integration runbook:**

  1. Clear the Unity Console and restart Play mode.
  2. Collect the ID Card.
  3. Interact with the Control Panel once; verify the initial access denial and
     objective 2 completion logs.
  4. Interact with the Control Panel a second time.
  5. Expected logs:
     `Control Panel inspected successfully. The panel remains locked and reveals no message yet.`
     `Quest 1 objective completed: Control Panel inspected.`
  6. Interact a third time. Expected:
     `Control Panel has already been inspected. No new state change.`
  7. Do not expect a hidden message, fuse objective, door state, or Quest 1
     completion yet.

  **Status:**

  - **Done:** prototype inspection interaction and guarded Quest 1 objective 3.
  - **Not done:** hidden message, External Fuse objective, fuse installation,
    Maintenance Wing access, and Quest 1 completion.
  - **Adjusted:** inspection is a separate interaction ID and event result, not a
    reinterpretation of the initial access denial.
  - **Blocked:** hidden-message interaction requires a separate object/asset and
    remains outside this iteration.

  ### 2026-10-15 — Quest 1 Hidden Message prototype

  **Implemented:**

  - Added `HiddenMessagePrototypeInteraction` as a separate interactable
    prototype component.
  - The component starts unavailable and subscribes to the typed event hub.
  - A successful Control Panel inspection unlocks the Hidden Message.
  - Before unlock, interaction logs `InvalidState` and does not advance Quest 1.
  - The first valid interaction logs success and publishes the Hidden Message
    interaction result.
  - `Quest1ObjectiveTracker` completes objective 4 only for a successful Hidden
    Message interaction after Control Panel inspection.
  - Repeated searches return `AlreadyCompleted` without duplicate objective
    progress.

  **Integration runbook — prototype only:**

  1. Create an empty GameObject in `TestScene` named
     `HiddenMessage_Prototype`.
  2. Add a visible primitive child if needed and ensure its Collider is on the
     same GameObject as `HiddenMessagePrototypeInteraction`.
  3. Add `HiddenMessagePrototypeInteraction` to that Collider GameObject.
  4. Assign `GameplaySystems/GameplayEventHub` to `Event Hub`.
  5. Save the scene.
  6. Restart Play mode, collect ID Card, attempt Control Panel access, and
     inspect the Control Panel.
  7. Before inspection, the prompt must be `Inspect Control Panel First`.
  8. After inspection, expected logs are:
     `Hidden Message is now available after Control Panel inspection.`
     `Hidden Message found successfully.`
     `Quest 1 objective completed: Hidden Message found.`
  9. Search it again. Expected:
     `Hidden Message has already been found. No new state change.`
  10. Do not expect the External Fuse, door, or Maintenance Wing state to change.

  **Status:**

  - **Done:** guarded Hidden Message prototype and objective 4 listener.
  - **Not done:** final message asset/presentation, External Fuse objective,
    installation, access scan, Maintenance Wing, and Quest 1 completion.
  - **Adjusted:** Hidden Message unlocks through the typed event hub rather than
    a direct reference to the Control Panel or Quest tracker.
  - **Blocked:** production placement and presentation depend on the final
    environment asset.

  ### 2026-10-15 — Quest 1 External Fuse objective and architecture map

  **Read baseline:** `project-brain.md`, `docs/README.md`,
  `docs/architecture.md`, `docs/quest-flow.md`, and the current Quest work log
  were reviewed before this change.

  **Implemented:**

  - Added `quest.1.find_external_fuse`.
  - `Quest1ObjectiveTracker` now handles the canonical
    `item.external_fuse` `ItemAdded` event.
  - Added `HasFoundExternalFuse` and success logs.
  - External Fuse pickup now completes only the find-fuse objective; installation
    and door access are not implied.
  - Added `docs/quest-architecture.md` with module boundaries, event diagrams,
    current status, and production asset integration rules.

  **Integration runbook:**

  1. Clear the Unity Console and restart Play mode.
  2. Collect the ID Card and complete the existing Control Panel, inspection, and
     Hidden Message steps.
  3. Approach `QuestPickup_ExternalFuse` and press `E`.
  4. Expected logs:
     `Inventory pickup succeeded. Item: 'item.external_fuse', Added: 1, Current amount: 1.`
     `Quest 1 HasFoundExternalFuse: True`
     `Quest 1 objective completed: External Fuse found.`
  5. Do not expect fuse installation or Maintenance Wing access yet.

  **Status:**

  - **Done:** External Fuse acquisition objective and architecture documentation.
  - **Not done:** fuse installation, ID Card rescan, Maintenance Wing entry, and
    Quest 1 completion.
  - **Adjusted:** architecture documentation now distinguishes event producers,
    adapters, and Quest state ownership.
  - **Blocked:** fuse installation needs an explicit panel interaction and the
    open decision about whether the fuse is consumed.

  ### 2026-10-15 — External Fuse installation prototype

  **Implemented:**

  - Added `interaction.control_panel_fuse_installation` and
    `quest.1.install_external_fuse`.
  - Control Panel now exposes installation only after the Hidden Message is
    found and the External Fuse is available.
  - Missing Hidden Message, missing fuse, repeated installation, and successful
    installation each produce explicit logs/results.
  - Successful prototype installation sets `HasInstalledFuse` and completes the
    Quest 1 objective, but intentionally does not remove the fuse yet.
  - Documented the final behavior decision: the fuse will be consumed after
    prototype validation, only after successful installation.

  **Integration runbook:**

  1. Clear Console and restart Play mode.
  2. Complete ID Card, initial Control Panel denial, inspection, Hidden Message,
     and External Fuse pickup.
  3. Return to the Control Panel. Prompt should be:
     `Press E to Install External Fuse`.
  4. Press `E` once. Expected logs:
     `External Fuse installed successfully. Fuse remains in Inventory during prototype validation.`
     `Quest 1 objective completed: External Fuse installed.`
  5. Press `E` again. Expected:
     `External Fuse has already been installed. No new state change.`
  6. Confirm no fuse removal log is expected in this prototype stage.

  **Negative checks:**

  - Without Hidden Message: `Fuse installation unavailable. Find the Hidden Message first.`
  - Without External Fuse: `Fuse installation denied. Reason: missing 'item.external_fuse'.`

  **Status:**

  - **Done:** guarded installation, successful fuse consumption, and objective 6 event flow.
  - **Not done:** ID Card rescan, Maintenance Wing access,
    and Quest 1 completion.
  - **Adjusted:** fuse consumption is now part of the successful installation
    transaction and is performed only through the Inventory adapter.
  - **Blocked:** the next objective requires an explicit card-reader/access
    interaction.

  ### 2026-10-17 — Quest 1 Maintenance Wing card scan prototype

  **Read baseline:** `project-brain.md`, `docs/README.md`,
  `docs/architecture.md`, `docs/gameplay-contracts.md`, `docs/quest-flow.md`,
  `docs/quest-architecture.md`, and the latest applicable handoff template were
  reviewed before this change.

  **Implemented:**

  - Added `MaintenanceWingCardReaderPrototypeInteraction`.
  - The reader validates the ID Card through `InventoryCompatibilityAdapter`.
  - It listens for successful fuse installation through `GameplayEventHub`.
  - Before the fuse installation event, the scan returns explicit `Denied`.
  - After the event, a valid ID Card scan returns `Success` through
    `AccessResolved`.
  - `Quest1ObjectiveTracker` completes the scan objective only for that exact
    successful access result.
  - Repeated successful scans return `AlreadyCompleted` without duplicate quest
    progress.

  **Integration runbook — prototype only:**

  1. Create an empty GameObject named `MaintenanceWingCardReader_Prototype`.
  2. Add a visible primitive child if needed and ensure its Collider is on the
     same GameObject as `MaintenanceWingCardReaderPrototypeInteraction`.
  3. Add `MaintenanceWingCardReaderPrototypeInteraction` to that Collider object.
  4. Assign `GameplaySystems/GameplayEventHub` to `Event Hub`.
  5. Assign `GameplaySystems/InventoryCompatibilityAdapter` to `Inventory Query`.
  6. Save the scene.
  7. Restart Play mode and test without ID Card. Expected:
     `Maintenance Wing card scan denied. Reason: missing 'item.id_card'.`
  8. With ID Card but before fuse installation, expected:
     `Maintenance Wing card scan denied. Reason: External Fuse is not installed.`
  9. Complete Hidden Message, find/install the fuse, then scan again. Expected:
     `Maintenance Wing card reader emergency override is now enabled.`
     `Maintenance Wing access granted after second ID Card scan.`
     `Quest 1 objective completed: ID Card scanned after fuse installation.`
  10. Scan again. Expected:
      `Maintenance Wing access has already been granted. No new state change.`
  11. Do not expect the player to move or the Maintenance Wing area objective to
      complete yet.

  **Status:**

  - **Done:** prototype card-reader access event and Quest 1 objective 7.
  - **Not done:** Maintenance Wing area trigger, door presentation, and Quest 1
    completion.
  - **Adjusted:** access is represented by `AccessResolved`; the reader does not
    call Quest code directly.
  - **Blocked:** final door/area asset and production access presentation remain
    pending.

  **Console verification — 2026-10-17:**

  - Initial Control Panel access was denied before inspection, as expected.
  - Control Panel inspection and Hidden Message discovery completed once.
  - Fuse installation was correctly denied while the fuse was missing.
  - External Fuse was then consumed successfully; remaining amount was `0`.
  - The second ID Card scan succeeded on the same Control Panel reader and
    completed `quest.1.scan_id_card_after_fuse`.
  - Repeated scans returned `AlreadyCompleted` behavior without duplicate
    objective progress.
  - The `MaintenanceWingCardReader_Prototype` still emitted its old emergency
    override log, so it remains enabled and must be disabled manually before
    the next test.

  ### 2026-10-17 — Maintenance Wing area trigger implementation

  **Implemented:**

  - Added `MaintenanceWingAreaTrigger` with explicit Inspector references to
    `GameplayEventHub` and the player root.
  - The trigger publishes `AreaEntered` with stable ID
    `area.maintenance_wing` once per player entry, allowing a safe re-entry
    after an early entry before access is granted.
  - `Quest1ObjectiveTracker` completes objective 8 only after objective 7 has
    already completed.

  **Unity validation status:**

  - **Code:** Implemented; Unity scene wiring and Play Mode validation pending.
  - **Not done:** Door animation/presentation and final production asset
    integration.
  - **Safety rule:** Do not edit the existing Control Panel interaction or
    Inventory wiring for this iteration.
  - The current prototype Player moves with `Transform.Translate`, so the
    Player root must use a kinematic `Rigidbody` for Unity trigger callbacks.
    This is a physics requirement, not a Quest dependency or a replacement for
    the movement system.

  ### 2026-10-17 — Corrected Quest 1 card-reader flow

  **Finding:**

  - The standalone `MaintenanceWingCardReader_Prototype` was incorrect for the
    agreed flow.
  - The first and second ID Card scans must use the same `CardReader` on the
    Control Panel.
  - The standalone prototype also placed its interaction script on a root object
    while the raycast hit a child Collider, so it was not reliably interactable.

  **Adjusted:**

  - `ControlPanelPrototypeInteraction` now handles the second ID Card scan after
    successful fuse installation.
  - The same component publishes `AccessResolved` for
    `access.maintenance_wing`.
  - The prompt changes to `Press E to Scan ID Card`, then to
    `Maintenance Wing Access Granted`.
  - A separate card-reader prototype is no longer part of the Quest 1 flow.

  **Manual Unity correction:**

  1. Select `MaintenanceWingCardReader_Prototype` in `TestScene`.
  2. Disable the whole GameObject, or remove it after confirming it is only the
     temporary standalone prototype.
  3. Do not add a second card-reader component to the Control Panel yet.
  4. Select the same Control Panel object that currently has
     `ControlPanelPrototypeInteraction` and its Collider.
  5. After fuse installation, aim at that same reader/panel and confirm the
     prompt becomes `Press E to Scan ID Card`.
  6. Press `E` and expect the second-scan success logs.

  **Status:**

  - **Done:** corrected same-reader runtime path and documentation.
  - **Not done:** Maintenance Wing door/area trigger and Quest 1 completion.
  - **Adjusted:** standalone card reader prototype is excluded from the agreed
    flow.
  - **Blocked:** final Control Panel prefab integration still requires attaching
    the interaction adapter to the asset's `CardReader` child or its Collider
    owner.

  **Implementation note:** the prototype uses the existing typed
  `InteractionCompleted` event. The dedicated `FuseInstalled` event remains
  planned for the production interaction boundary.

### 2026-10-15 — Inventory query construction fix

**Finding:**

- The Console warning `You are trying to create a MonoBehaviour using the
  'new' keyword` came from `InventorySystem` constructing `InventoryItem`.
- `InventoryItem` was a `MonoBehaviour` even though it is an in-memory item
  record, which made the successful mutation event unreliable for subsequent
  queries.

**Implemented:**

- Changed `InventoryItem` to a serializable plain data class.
- Preserved the existing `InventorySystem` public methods, list type, item
  fields, and serialized scene references.
- Pickup success logging now includes the current amount returned by the
  compatibility query.

**Validation target:**

- The Unity warning about constructing `MonoBehaviour` with `new` must disappear.
- After ID Card pickup, the log must show `Current amount: 1`.
- Control Panel must report that the ID Card is present and deny only because
  initial access is blocked.

**Status:**

- **Done:** minimal construction fix and query verification log.
- **Not done:** Unity play-mode retest after recompilation.
- **Adjusted:** the existing Inventory implementation was corrected only at the
  invalid object-construction boundary; its public API and ownership remain.
- **Blocked:** Control Panel objective progression remains paused until this
  retest passes.

### 2026-10-15 — Quest 1 initial Control Panel objective listener

**Read baseline:** `project-brain.md`, `docs/README.md`,
`docs/architecture.md`, `docs/quest-flow.md`, and the latest Quest foundation
handoff were reviewed before this change.

**Implemented:**

- Added the canonical objective ID
  `quest.1.try_initial_control_panel_access`.
- `Quest1ObjectiveTracker` now subscribes to `InteractionCompleted`.
- Only a `Denied` result for the Control Panel, after the ID Card objective is
  complete, records the objective.
- The listener logs the completion and publishes `QuestObjectiveCompleted`.
- Subscription is removed on disable; no scene wiring change is required
  because the existing `Quest1` object already references `GameplaySystems`.

**Integration runbook:**

1. Clear the Unity Console.
2. Stop and restart Play mode.
3. Collect the ID Card and verify the existing ID Card success logs.
4. Interact with the Control Panel once.
5. Expected logs:
   `Control Panel initial access denied. ID Card present, but the panel requires inspection.`
   `Quest 1 objective completed: initial Control Panel access attempt recorded.`
6. The second interaction must not repeat the Quest objective completion log.
7. `Gameplay event QuestObjectiveCompleted has no subscribers` may still appear;
   it means the event was published but UI/flow subscribers are not implemented.

**Status:**

- **Done:** prototype listener for Quest 1 objective 2.
- **Not done:** panel inspection, hidden message, fuse, access, and area flow.
- **Adjusted:** objective completion is gated by both canonical interaction ID
  and `Denied` result, preventing missing-item attempts from advancing Quest 1.
- **Blocked:** the next objective requires an explicit inspection interaction
  design and remains out of this iteration.
