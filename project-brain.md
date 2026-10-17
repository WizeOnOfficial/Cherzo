# Project Brain — Audit Total dan Pemetaan Arsitektur

**Proyek:** `GameDevelopmentProject`  
**Root yang diaudit:** `Assets/`  
**Metode:** audit statis terhadap skrip C#, scene YAML, prefab, ScriptableObject, input actions, material, shader, URP settings, TextMesh Pro resources, dan metadata Unity.  
**Status dokumen:** cetak biru arsitektur berdasarkan isi proyek yang terdeteksi saat audit.

> Catatan pembacaan: laporan ini membedakan fakta yang terserialized/terlihat di kode dari inferensi perilaku runtime. Audit ini tidak menjalankan game dan tidak mengubah aset.

## 1. IKHTISAR SISTEM GLOBAL (OVERVIEW)

### 1.1 Identitas proyek

Proyek ini adalah prototipe game first-person berbasis Unity yang berfokus pada:

- pergerakan pemain dan kontrol kamera;
- interaksi objek melalui raycast dari tengah kamera;
- mengambil, membawa, memakai, dan menjatuhkan objek pickup berbasis physics;
- fondasi inventory berbasis `InventoryItemData` dan `InventoryItem`;
- eksperimen visual bergaya pixelated/retro melalui Universal Render Pipeline (URP), shader graph, dan custom renderer pass;
- pembuatan ikon item dari kamera melalui scene khusus.

Bukti utama gameplay berada di [`TestScene.unity`](Assets/Scenes/TestScene.unity). Scene tersebut memiliki `Main Camera`, `Player`, dua objek item, `PlayerInput`, `InteractionController`, `PickUpController`, `InventorySystem`, UI inventory, kamera Cinemachine, dan `Pickup Holder`.

### 1.2 Alur gameplay aktual

Alur interaksi yang benar-benar tersambung saat ini adalah:

1. `InteractionController.Update()` mengambil `Camera.main`.
2. Raycast ditembakkan dari pusat viewport dengan jarak interaksi scene sekitar 5.
3. Collider yang terkena dicoba dikonversi menjadi `IInteractable`.
4. Pesan `InteractMsg` ditampilkan pada `TextMeshProUGUI`.
5. Saat `E` ditekan, `IInteractable.Interact()` dipanggil.
6. `PhysicsPickup.Interact()` mengambil `PickUpController` pada object controller dan memanggil `Grab()`.
7. `PickUpController` menyimpan satu `IPickupable`, memindahkannya ke `Pickup Holder`, lalu `PhysicsPickup` mengatur rigidbody menjadi kinematic dan collider menjadi nonaktif.
8. `Q` menjatuhkan item; klik kiri memanggil `Use()`.

Inventory belum menjadi bagian aktif dari alur tersebut. Pemanggilan `InventorySystem.current.Add(referenceItem)` di [`PhysicsPickup.cs`](Assets/Scripts/PhysicsPickup.cs) masih berupa komentar, kedua pickup scene tidak memiliki `referenceItem`, dan tidak ditemukan binding UI yang membaca `InventorySystem.inventory`.

### 1.3 Batasan sistem saat ini

Proyek tampak berada pada tahap prototipe/eksperimen, bukan arsitektur game final:

- kontrak interaksi dan pickup sudah dipisahkan memakai interface;
- sistem inventory baru berupa fondasi data dan dictionary/list;
- objek pickup scene dikonfigurasi manual, sementara prefab `Cube` belum memiliki komponen pickup physics;
- ada komponen scene `PlayerCameraScript` yang serialized tetapi file/class-nya tidak ditemukan;
- terdapat aset tutorial bawaan URP/TMP di samping aset gameplay;
- fitur pixelation ditulis dengan API Render Graph Unity 6, tetapi tidak tampak terdaftar eksplisit pada `PC_Renderer.asset` yang diaudit.

### 1.4 Inventaris kuantitatif

Di bawah `Assets/` terdeteksi setidaknya:

- 15 file `.cs`;
- 2 scene `.unity`;
- 13 ScriptableObject/config `.asset`;
- 4 material `.mat`;
- 15 shader `.shader`;
- 5 shader graph `.shadergraph`;
- 3 gambar `.png`;
- 1 prefab `.prefab`;
- 1 input action asset `.inputactions`;
- 1 HLSL, 4 include `.cginc`, 1 font `.ttf`, 1 JSON, 4 TXT, 1 layout `.wlt`;
- 93 metadata `.meta`.

## 2. PEMETAAN FOLDER & ARSITEKTUR KODE

### 2.1 Pemetaan folder utama

| Folder | Isi dan tanggung jawab |
|---|---|
| [`Assets/Scripts/`](Assets/Scripts/) | Gameplay runtime: interaksi, pickup, inventory, player movement, head bob, screenshot utility. |
| [`Assets/Scenes/`](Assets/Scenes/) | [`TestScene.unity`](Assets/Scenes/TestScene.unity) untuk gameplay dan [`IconCreationScene.unity`](Assets/Scenes/IconCreationScene.unity) untuk menghasilkan ikon. |
| [`Assets/Prefabs/`](Assets/Prefabs/) | [`Cube.prefab`](Assets/Prefabs/Cube.prefab), model cube dasar dengan mesh, renderer, dan collider. |
| [`Assets/ScriptableObject/`](Assets/ScriptableObject/) | [`cubeItem.asset`](Assets/ScriptableObject/cubeItem.asset), data item `InventoryItemData`. |
| [`Assets/Materials/`](Assets/Materials/) | Material cube dan material visual terkait. |
| [`Assets/Shader/`](Assets/Shader/) | `DitherEffect` shader graph serta `PixelatedEffect` custom URP feature/pass/shader. |
| [`Assets/Settings/`](Assets/Settings/) | URP pipeline assets, renderer assets, global settings, dan volume profile PC/mobile. |
| [`Assets/TextMesh Pro/`](Assets/TextMesh%20Pro/) | Font, material, sprite asset, shader, include, dan konfigurasi TMP. |
| [`Assets/Icons/`](Assets/Icons/) | [`CubeIcon.png`](Assets/Icons/CubeIcon.png), output ikon dari kamera. |
| [`Assets/TutorialInfo/`](Assets/TutorialInfo/) | `Readme` ScriptableObject, custom inspector editor, layout, dan ikon tutorial bawaan. |
| `Assets/InputSystem_Actions.inputactions` | Action map `Player` dan action input/UI. |

### 2.2 Bentuk arsitektur

Arsitektur saat ini adalah kombinasi kecil dari:

- **interface-based interaction:** `IInteractable` dan `IPickupable` mendefinisikan kontrak;
- **controller-oriented MonoBehaviour:** `InteractionController`, `PickUpController`, dan `PlayerInputScript` mengatur alur;
- **data-object inventory:** `InventoryItemData` sebagai ScriptableObject dan `InventoryItem` sebagai model item;
- **manual scene wiring:** referensi utama diisi melalui Inspector scene;
- **custom rendering pipeline:** `PixelizeFeature` membuat dan mengantrikan `PixelizePass`;
- **bukan singleton yang selesai:** `InventorySystem` digunakan sebagai instance MonoBehaviour, tetapi `PhysicsPickup` mengasumsikan static `current` yang tidak ada.

Tidak ditemukan implementasi MVC formal, event bus, dependency injection, atau event-driven inventory. Komunikasi utama bersifat langsung dan imperative melalui referensi component, `GetComponent`, input polling, dan interface call. Dengan demikian, struktur saat ini lebih tepat disebut prototipe component-based dengan beberapa kontrak interface dan sebagian coupling langsung, bukan spaghetti murni tetapi juga belum menjadi arsitektur modular.

### 2.3 Peta komunikasi sistem

```text
PlayerInput / Keyboard / Mouse
        |
        +--> PlayerInputScript --> CinemachineCamera
        |
        +--> InteractionController --> Camera.main + Physics.Raycast
                                      |
                                      +--> IInteractable
                                            |
                                            +--> PhysicsPickup
                                                  |
                                                  +--> PickUpController
                                                        |
                                                        +--> Pickup Holder
                                                        +--> Rigidbody/Collider

InventoryItemData (ScriptableObject)
        |
        +--> InventoryItem
        +--> InventorySystem
        +--> PhysicsPickup.referenceItem (saat ini kosong di scene)
```

Detail dependensi:

1. `InteractionController` bergantung pada `Camera.main`, `Physics.Raycast`, `IInteractable`, dan TMP text.
2. `PhysicsPickup` bergantung pada `PickUpController` yang ditemukan pada GameObject interaksi.
3. `PickUpController` bergantung pada kontrak `IPickupable`, tetapi implementasi aktual yang terdeteksi adalah `PhysicsPickup`.
4. `InventorySystem` tidak dipanggil oleh alur pickup aktif.
5. `InventorySystem` juga tidak memiliki publisher/subscriber event atau presenter UI.
6. `PlayerInputScript` mengandalkan Unity Input System dan mencari `CinemachineCamera` pada child.
7. `PlayerHeadBobScript` memakai callback Input System terpisah dan tidak memiliki bukti wiring scene yang aktif.
8. `PixelizeFeature` dan `PixelizePass` berkomunikasi melalui nested `CustomPassSettings`; shader diambil melalui nama `PixelizeRender`.

### 2.4 Scene dan konfigurasi runtime

#### `TestScene.unity`

- `Main Camera` sekitar baris 568, digunakan oleh `Camera.main`.
- Referensi serialized `PlayerCameraScript` sekitar baris 701–703, tetapi script tidak ada.
- `Item (1)` sekitar baris 1080 dan `Item` sekitar baris 1731.
- `PhysicsPickup` pertama sekitar baris 1124; rigidbody/collider terisi, `referenceItem` kosong sekitar baris 1128.
- `Player` sekitar baris 1239.
- `PlayerInputScript` sekitar baris 1354.
- Unity `PlayerInput` dan input action asset sekitar baris 1370–1373.
- `InteractionController` sekitar baris 1402.
- `PickUpController` sekitar baris 1416.
- `InventorySystem` aktif sekitar baris 2292–2294.
- `InventoryCanvas` sekitar baris 2675; tidak ditemukan kode presenter inventory.
- `PlayerCamera` sekitar baris 2775.
- `Pickup Holder` sebagai child player sekitar baris 2961.

#### `IconCreationScene.unity`

- memiliki kamera utama dan Cinemachine Brain;
- memiliki `TakeScreenshotFromCam` sekitar baris 269–271;
- digunakan untuk membuat `Assets/Icons/CubeIcon.png`.

### 2.5 Prefab dan ScriptableObject

[`Cube.prefab`](Assets/Prefabs/Cube.prefab) berisi `Transform`, `MeshFilter`, `MeshRenderer`, dan `BoxCollider`. Prefab tersebut tidak memiliki `Rigidbody` maupun `PhysicsPickup`.

[`cubeItem.asset`](Assets/ScriptableObject/cubeItem.asset) adalah `InventoryItemData` dengan:

- `id: cubeItem`;
- `displayName: Cube`;
- `icon` kosong;
- `prefab` mengarah ke [`Cube.prefab`](Assets/Prefabs/Cube.prefab).

Artinya, data item sudah mengenal prefab, tetapi prefab belum merepresentasikan pickup physics lengkap dan ikon item belum dihubungkan pada ScriptableObject.

## 3. DAFTAR CLASS & SKRIP KUNCI (BREAKDOWN)

Semua path di bagian ini relatif terhadap root proyek.

### 3.1 Kontrak interaksi dan pickup

#### [`IInteractable`](Assets/Scripts/IInteractable.cs)

- Interface pada namespace `DefaultNamespace`.
- Kontrak: `string InteractMsg { get; }` dan `Interact(InteractionController)`.
- Dependency internal: `InteractionController`.
- Peran: abstraksi minimum agar `InteractionController` tidak harus mengenal setiap jenis objek.

#### [`IPickupable`](Assets/Scripts/IPickupable.cs)

- Interface turunan `IInteractable`.
- Menambah `Grab`, `Drop`, `SetPositionInParent(Transform)`, dan `Use`.
- Dependency internal: `IInteractable`, `PickUpController`, `InteractionController`.
- Dependency Unity: `Transform`.
- Implementasi yang terdeteksi: `PhysicsPickup`.

#### [`InteractionController`](Assets/Scripts/InteractionController.cs)

- `MonoBehaviour` pada namespace global.
- Field penting: `interactionText`, `interactionDistance`.
- `Update()` melakukan raycast dari tengah kamera, membaca `IInteractable`, mengubah TMP text, dan membaca `Keyboard.current.eKey`.
- Dependency: `IInteractable`, `Camera.main`, `Physics.Raycast`, `Keyboard.current`, TextMesh Pro.
- Scene dependency: `TestScene` menghubungkan text UI dan jarak interaksi sekitar 5.
- Import `Unity.Cinemachine` terdeteksi tetapi tidak digunakan di class.

#### [`PickUpController`](Assets/Scripts/PickUpController.cs)

- `MonoBehaviour` yang menyimpan satu `IPickupable`.
- Field penting: `pickupHolder`, `pickup`; property `HasPickup`.
- `GrabPickup()` menyimpan pickup dan memindahkannya ke holder.
- `Update()` membaca `Q` untuk drop dan klik kiri untuk use.
- Dependency: `IPickupable`, `PhysicsPickup` secara runtime melalui interface, `Transform`, `Keyboard.current`, `Mouse.current`.
- Scene dependency: `Player/Pickup Holder` di `TestScene`.
- Import `Unity.VisualScripting` tidak digunakan eksplisit.

#### [`PhysicsPickup`](Assets/Scripts/PhysicsPickup.cs)

- `MonoBehaviour, IPickupable`.
- Field penting: `Rigidbody pickupRigidbody`, `Collider pickupCollider`, `Vector3 pickupPositionOffset`, `InventoryItemData referenceItem`.
- `Interact()` mencari `PickUpController`; `Grab()` memulai pengambilan; `Drop()` melepas item; `SetPositionInParent()` mengatur parent/posisi; `Use()` mencatat penggunaan; `SetPhysicsValues()` mengubah kinematic dan enabled collider.
- Dependency: `IPickupable`, `IInteractable`, `PickUpController`, `InventoryItemData`, `Rigidbody`, `Collider`, `Transform`, `Quaternion`.
- Scene dependency: dua instance pada `TestScene`; rigidbody/collider terisi tetapi `referenceItem` kosong.
- Baris inventory yang relevan masih komentar: `InventorySystem.current.Add(referenceItem)`.

### 3.2 Inventory

#### [`InventoryItemData`](Assets/Scripts/InventoryItemData.cs)

- `ScriptableObject` dengan `[CreateAssetMenu(menuName = "Inventory Item Data")]`.
- Field: `id`, `displayName`, `Sprite icon`, `GameObject prefab`.
- Dipakai oleh `InventoryItem`, `InventorySystem`, dan `PhysicsPickup`.
- Asset konkret: [`cubeItem.asset`](Assets/ScriptableObject/cubeItem.asset).

#### [`InventoryItem`](Assets/Scripts/InventoryItem.cs)

- Dideklarasikan `[Serializable]` tetapi mewarisi `MonoBehaviour`.
- Menyimpan `InventoryItemData data` dan `int stackSize`.
- Constructor menerima `InventoryItemData`; `AddToStack()` menambah stack; `RemoveFromStack()` mengurangi stack.
- Dependency: `InventoryItemData`; dibuat oleh `InventorySystem`.
- Ketidaksesuaian penting: `InventorySystem` membuatnya dengan `new InventoryItem(referenceData)`, padahal `MonoBehaviour` adalah component Unity.
- `RemoveFromStack()` tidak terlihat memiliki guard untuk mencegah stack negatif.

#### [`InventorySystem`](Assets/Scripts/InventorySystem.cs)

- `MonoBehaviour`.
- Menyimpan dictionary `InventoryItemData -> InventoryItem` dan list inventory.
- `Awake()` menginisialisasi collection.
- `Add(InventoryItemData)` mencari item yang sudah ada atau membuat `new InventoryItem(referenceData)`.
- `Remove(InventoryItemData)` mengurangi stack/menghapus item sesuai implementasi.
- Dependency: `InventoryItemData`, `InventoryItem`, `Dictionary`, `List`.
- Tidak memiliki deklarasi static `current`.
- Tidak memiliki event perubahan inventory, presenter UI, atau hubungan aktif dengan pickup.
- Dipasang pada `TestScene`, tetapi tidak terbukti menerima input pickup saat audit.

### 3.3 Player dan input

#### [`PlayerInputScript`](Assets/Scripts/PlayerInputScript.cs)

- `MonoBehaviour` untuk movement, look, dan sprint.
- Mencari `CinemachineCamera` melalui `GetComponentInChildren`.
- `OnMove(InputValue)` menerima input movement dari `PlayerInput`.
- Membaca keyboard/mouse untuk gerakan, look, dan sprint.
- Dependency: Unity Input System, `CinemachineCamera`, `Keyboard.current`, `Mouse.current`.
- Scene dependency: `Player` dan child `PlayerCamera`.
- Field `sprintSpeed`/`sprintCooldown` terdeteksi tetapi tidak menjadi sumber sprint efektif; kode mengubah `movementSpeed` secara kumulatif dengan faktor 1.5 lalu membaginya saat dilepas.

#### [`PlayerHeadBobScript`](Assets/Scripts/PlayerHeadBobScript.cs)

- `MonoBehaviour` untuk efek head bob berbasis input gerak.
- Mengelola parameter amount, frequency, smoothness, timer, dan input.
- Memiliki callback `OnMove(InputAction.CallbackContext)`.
- Dependency: `Vector2/Vector3`, `Time`, `Mathf`, Input System.
- Tidak ditemukan referensi component yang jelas pada scene yang diaudit.
- `Debug.Log` dipanggil setiap frame pada sekitar baris 25.
- Bentuk callback berbeda dari `PlayerInputScript.OnMove(InputValue)` dan memerlukan wiring notification yang sesuai.

#### [`TakeScreenshotFromCam`](Assets/Scripts/TakeScreenshotFromCam.cs)

- `MonoBehaviour` utility dengan `ContextMenu("Take Screenshot")`.
- Merender kamera ke `RenderTexture`, membaca `Texture2D`, menulis PNG, lalu melakukan refresh AssetDatabase pada editor.
- Dependency: `Camera`, `RenderTexture`, `Texture2D`, `System.IO`, `Application`, `UnityEditor`.
- Scene dependency: `IconCreationScene`.
- Hasil terdeteksi: [`CubeIcon.png`](Assets/Icons/CubeIcon.png).
- Berada di folder runtime `Assets/Scripts` walaupun mengimpor `UnityEditor`.

### 3.4 Rendering

#### [`PixelizeFeature`](Assets/Shader/PixelatedEffect/PixelizeFeature.cs)

- `ScriptableRendererFeature` URP.
- Membuat `PixelizePass`, memasukkannya ke renderer queue kecuali Scene View, dan dispose pass.
- Nested `CustomPassSettings` menjadi dependency langsung `PixelizePass`.
- Mengandalkan shader bernama `PixelizeRender`.
- Tidak tampak terdaftar aktif secara eksplisit pada `PC_Renderer.asset` yang diperiksa.

#### [`PixelizePass`](Assets/Shader/PixelatedEffect/PixelizePass.cs)

- `ScriptableRenderPass` dengan implementasi Render Graph Unity 6.
- Mengambil active color texture, menghitung ukuran target, membuat tekstur resolusi rendah, melakukan downsample, lalu upsample.
- Dependency: `PixelizeFeature.CustomPassSettings`, URP Render Graph, `UniversalCameraData`, `UniversalResourceData`, `TextureHandle`, `Blitter`, `CoreUtils`.
- Dependency aset: [`PixelizeRender.shader`](Assets/Shader/PixelatedEffect/PixelizeRender.shader).
- Kode lama berbasis temporary RT masih ada sebagai komentar pada bagian awal file.

### 3.5 Tutorial/editor

#### [`Readme`](Assets/TutorialInfo/Scripts/Readme.cs)

- `ScriptableObject` data model dokumentasi tutorial.
- Field: icon, title, sections, loaded layout; nested `Section`.
- Dependency aset: [`Readme.asset`](Assets/Readme.asset), `TutorialInfo/Layout.wlt`, ikon tutorial.

#### [`ReadmeEditor`](Assets/TutorialInfo/Scripts/Editor/ReadmeEditor.cs)

- `Editor` custom inspector dengan `[InitializeOnLoad]`.
- Mengelola pemilihan readme, pemuatan layout, tampilan section/link, `SessionState`, `AssetDatabase`, dan operasi editor.
- Berada di folder `Editor`, sehingga bersifat editor-only.
- Memiliki tombol yang secara desain dapat menghapus aset tutorial jika dijalankan pengguna; tidak dijalankan selama audit.

## 4. DIAGNOSIS ERROR & BLOKADE SAAT INI

### 4.1 Diagnosis CS0117

Lokasi:

- pemanggil: [`PhysicsPickup.cs`](Assets/Scripts/PhysicsPickup.cs), sekitar baris 37;
- target: [`InventorySystem.cs`](Assets/Scripts/InventorySystem.cs), definisi class sekitar baris 4–42.

Referensi bermasalah:

```csharp
InventorySystem.current.Add(referenceItem);
```

`InventorySystem` tidak mendeklarasikan member static `current`. Tidak ada:

```csharp
public static InventorySystem current;
```

Karena `current` bukan member class, pemanggilan static tersebut menghasilkan **CS0117** apabila baris diaktifkan. Pada keadaan file yang diaudit, baris itu masih dikomentari, sehingga merupakan sumber error potensial atau sisa dari versi file sebelumnya, bukan statement aktif yang pasti dikompilasi pada snapshot ini.

Relasi masalahnya bukan hanya nama member:

1. `InventorySystem` saat ini adalah instance `MonoBehaviour`, bukan singleton.
2. `PhysicsPickup` tidak memiliki referensi instance `InventorySystem`.
3. `referenceItem` kosong pada kedua instance pickup di scene.
4. Integrasi `Add` belum aktif.
5. Jika `Add(null)` sampai dipanggil, dictionary inventory berpotensi menerima key null atau gagal pada alur item baru.

### 4.2 Potensi bug/error lain

#### A. Pembuatan `MonoBehaviour` dengan `new` — risiko runtime tinggi

`InventorySystem.cs` membuat `new InventoryItem(referenceData)`, sedangkan `InventoryItem` mewarisi `MonoBehaviour`. Component Unity tidak dibuat sebagai object C# biasa melalui `new`; ini merupakan ketidaksesuaian lifecycle yang dapat menyebabkan error runtime Unity dan membuat model inventory tidak berfungsi sebagaimana diharapkan.

#### B. `PlayerCameraScript` hilang dari Assets

`TestScene.unity` menyimpan serialized component `Assembly-CSharp::PlayerCameraScript` sekitar baris 701–703 dengan GUID `5f7fcf314d0517b49bd51f1fdce76ab5`, tetapi tidak ada file/class tersebut di `Assets`. Dampaknya dapat berupa Missing Script dan hilangnya perilaku kamera lama.

#### C. `TakeScreenshotFromCam` mengimpor `UnityEditor` di folder runtime

[`TakeScreenshotFromCam.cs`](Assets/Scripts/TakeScreenshotFromCam.cs) berada di `Assets/Scripts`, tetapi mengimpor `UnityEditor` pada baris awal. Walaupun pemakaian `AssetDatabase` dibungkus `#if UNITY_EDITOR`, import tersebut tidak dibungkus. Ini berpotensi mengganggu build player karena `UnityEditor` tidak tersedia pada assembly runtime.

#### D. `referenceItem` kosong pada dua pickup

`TestScene.unity` memiliki `referenceItem: {fileID: 0}` sekitar baris 1128 dan 1845. Inventory tidak dapat mengetahui data item dari pickup tersebut. Ini menjadi blokade langsung bila integrasi inventory diaktifkan.

#### E. Prefab `Cube` bukan pickup physics lengkap

`Cube.prefab` tidak memiliki `Rigidbody` dan `PhysicsPickup`. Jika prefab itu dipakai sebagai item pickup baru tanpa konfigurasi scene tambahan, field physics pickup dapat null atau perilaku pickup tidak tersedia.

#### F. Referensi perangkat input tidak null-safe

`InteractionController`, `PickUpController`, dan `PlayerInputScript` mengakses `Keyboard.current`/`Mouse.current` secara langsung. Pada platform atau kondisi tanpa device yang sesuai, referensi dapat null.

#### G. Potensi wiring callback Input System tidak konsisten

`PlayerInputScript` memakai `OnMove(InputValue)`, sedangkan `PlayerHeadBobScript` memakai `OnMove(InputAction.CallbackContext)`. Scene memiliki `m_ActionEvents: []` sekitar bagian `PlayerInput`. Jika notification behavior dan action wiring tidak cocok, callback dapat tidak terpanggil.

#### H. Sprint berpotensi mengalami drift nilai

Kode sprint mengubah `movementSpeed` dengan perkalian dan pembagian, bukan menggunakan nilai dasar/sprint yang terpisah secara stabil. Pengulangan transisi dapat menghasilkan nilai yang menyimpang dari konfigurasi yang diharapkan. Field `sprintSpeed` juga terdeteksi tidak menjadi sumber nilai aktif.

#### I. Logging setiap frame

`PlayerHeadBobScript` menulis `Debug.Log("Horizontal Value: " + horizontalInput)` setiap frame. Ini dapat membanjiri Console dan menambah biaya runtime secara signifikan.

#### J. Risiko null pada `PhysicsPickup`

`PhysicsPickup.Interact()` tidak menunjukkan validasi eksplisit bila `GetComponent<PickUpController>()` gagal. `SetPhysicsValues()` juga mengandalkan field rigidbody/collider serialized. Instance scene saat ini terisi, tetapi prefab dasar tidak.

#### K. Risiko stack inventory negatif

`InventoryItem.RemoveFromStack()` dapat menurunkan `stackSize` tanpa guard yang terdeteksi. Nilai inventory dapat menjadi tidak valid bila dipanggil lebih banyak daripada jumlah item.

### 4.3 Rendering dan aset visual

- [`PC_RPAsset.asset`](Assets/Settings/PC_RPAsset.asset) memiliki render scale sekitar `0.2`.
- [`Mobile_RPAsset.asset`](Assets/Settings/Mobile_RPAsset.asset) memiliki render scale sekitar `0.8`.
- [`PC_Renderer.asset`](Assets/Settings/PC_Renderer.asset) memperlihatkan Full Screen Pass dan SSAO aktif.
- [`DefaultVolumeProfile.asset`](Assets/Settings/DefaultVolumeProfile.asset) memuat Vignette, Lens Distortion, Channel Mixer, Lift Gamma Gain, Split Toning, Outline, dan komponen volume URP/Core lain.
- [`PixelizeRender.shader`](Assets/Shader/PixelatedEffect/PixelizeRender.shader) menyediakan uniform `_BlockCount`, `_BlockSize`, dan `_HalfBlockSize`.
- `DitherEffect` berisi shader graph retro dan material terkait.
- TMP menyediakan resource font, material, sprite, shader, dan include bawaan; dependency gameplay yang terlihat langsung adalah text interaksi pada `InteractionController`.

## 5. ATURAN KERJA & MEMORI PERMANEN

Aturan proyek yang harus dipertahankan untuk sesi-sesi berikutnya:

1. Proyek ini menggunakan **Unity 6**.
2. Proyek ini **di-import dari Git**; perubahan harus memperhatikan serialized scene/prefab, GUID `.meta`, dan konsistensi source control.
3. Setiap solusi coding ke depan wajib disertai **panduan manual Unity Editor** yang dapat dipraktikkan, termasuk langkah klik yang relevan di:
   - Inspector;
   - Animator;
   - Scene/Hierarchy;
   - prefab atau ScriptableObject;
   - Player/Input System;
   - URP Renderer Feature bila menyentuh rendering.
4. Referensi scene/prefab dan field serialized harus dianggap sebagai bagian dari arsitektur, bukan hanya detail konfigurasi.
5. Perubahan terhadap sistem inventory harus dianalisis bersama `PhysicsPickup`, `InventoryItemData`, `InventoryItem`, scene pickup, dan UI inventory.
6. Perubahan terhadap input harus memeriksa action asset, `PlayerInput`, notification behavior, callback signature, dan binding Inspector secara bersamaan.
7. Aset editor-only harus dibedakan dari script runtime agar build player tetap valid.

Dokumen ini menjadi baseline pemahaman arsitektur sebelum diskusi atau perubahan lanjutan dilakukan.
