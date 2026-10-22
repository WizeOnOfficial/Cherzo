# Master Integration & Exam Rubric Execution Guide (Quest 1 & Quest 2)

**Tanggal:** 2026-10-22  
**Target:** Penyelesaian Penuh Alur Game (UTS Lab IF581: 100/100 Poin)  
**Status Scene:** `Assets/Scenes/AurickAssets.unity` (Peta Utama) & `Assets/Scenes/MainMenuScene.unity` (Wilson/Menu)

---

## 1. Pemetaan Target Nilai UTS Lab (100 Poin)

| Rubrik | Bobot | Target Implementasi & Bukti Kerja |
|---|---|---|
| **Question 1: Gameplay & Win/Lose Flow** | **20%** | • Gameplay utuh Quest 1 & 2 dari Lobby hingga Control Room.<br>• Bebas error runtime / crash.<br>• **Win Screen**: Tampil saat masuk Control Room.<br>• **Lose Screen**: Tampil jika timer 20 menit habis.<br>• **Unbroken Flow**: Tombol "Restart" dan "Main Menu" melooping tanpa henti/bug. |
| **Question 2: Responsive UI (Life, Score, Time)** | **15%** | • Main Menu UI (Dikerjakan Wilson di `MainMenuScene`).<br>• Gameplay HUD: **Health (100 HP)**, **Score (+50 s/d +350)**, **Countdown Timer (20:00)**, dan **Objective Text**.<br>• Responsive: Canvas Scaler `Scale With Screen Size (1920x1080, match 0.5)`. |
| **Question 3: 3D Models & Primitive Structure** | **15%** | • Seluruh lingkungan 3D fasilitas Aurick rapi.<br>• Player capsule tertata rapi dalam hierarki.<br>• Hirarki scene dikelompokkan dalam root folder bersih (`GameplaySystems`, `Interactables`, `Environment`, `UI`). |
| **Question 4: Zero Bug Presentation** | **20%** | • Seluruh input (`Keyboard.current`, `Mouse.current`), raycast, dan event dilindungi null check.<br>• Zero null-reference exceptions di Unity Console. |
| **Question 5: Animated Objects (Min. 2 Objek)** | **15%** | • **Objek 1 (Sliding Doors)**: `PintuMasuk` & `PintuConRoom` bergeser mulus ke samping saat scan kartu disetujui.<br>• **Objek 2 (Floating Items)**: `keycard` & `Fuse` melayang naik-turun dan berputar perlahan di udara saat belum diambil. |
| **Question 6: Kontrol Responsif & Mudah** | **15%** | • Gerakan First-Person halus (WASD + Mouse Look clamped + Sprint Shift + Stamina).<br>• Tombol `E` / Klik Kiri untuk interaksi raycast.<br>• Tombol `Tab` buka/tutup inventory Wilson.<br>• Tombol `Q` drop barang yang dipegang. |

---

## 2. Status Aset Fisik Aurick (`AurickAssets.unity`)

Berdasarkan audit commit terbaru `a5bb56b` (*pembetulan assets*), seluruh objek quest sudah berada di posisi yang benar:

1. **Player**: Kapsul standar di Lobby `{x: 0, y: 1.5, z: 0}`.
2. **Keycard**: `{x: -4.284, y: 1.017, z: 0.304}` (Di atas meja/lantai Main Lobby).
3. **Scanner Lobby**: `{x: 1.105, y: 1.09, z: 10.838}` (Di sebelah kanan `PintuMasuk`).
4. **PintuMasuk**: `{x: -0.03, y: 1.563, z: 9.817}` (Pintu geser penghubung Lobby & Koridor).
5. **Fuse (Sekring)**: `{x: 5.957, y: 1.666, z: 1.203}` (Di dalam ruangan Janitor).
6. **SwitchListrik (Panel Breaker)**: Di ruangan Janitor / kelistrikan.
7. **PintuITRoom**: `{x: 6.568, y: 1.563, z: 16.26}` (Akses ke Ruang Server IT).
8. **Server(untukUbahKeycard)** & **komputer**: Di dalam Ruang IT.
9. **PintuConRoom**: `{x: -6.82, y: 1.56, z: 20.66}` (Pintu geser akses Ruang Kontrol Utama).
10. **handscanner(buatRoomNextLvlAja)**: Di sebelah `PintuConRoom`.

---

## 3. Desain Komponen & Isolasi Modul

Untuk menghindari merge conflict dengan Wilson (yang sedang menangani `MainMenuScene`), seluruh pekerjaan di langkah ini diisolasi ke dalam script baru dan penataan scene `AurickAssets`:

### A. Animasi Objek
1. `Assets/Scripts/Animation/SlidingDoorController.cs`:
   - Menggeser posisi pintu lokal saat `OpenDoor()` dipanggil.
   - Menggunakan interpolasi halus (`Vector3.MoveTowards` / `SmoothDamp`).
   - Mencegah pintu terbuka jika belum diotorisasi (`isLocked = true`).
2. `Assets/Scripts/Animation/FloatingItemAnimation.cs`:
   - Menggerakkan item melayang naik-turun (`Mathf.Sin`) dan rotasi Y (`Vector3.up * speed * Time.deltaTime`).
   - Otomatis mati saat objek diambil ke tangan/inventory.

### B. Responsive HUD & Timer
1. `Assets/Scripts/UI/GameplayHUDController.cs`:
   - **Countdown Timer**: 20 Menit (1200 detik).
   - **Health Bar & Text**: 100 / 100.
   - **Score Display**: Poin terakumulasi dari tiap misi.
   - **Objective Text**: Menjelaskan instruksi langkah saat ini.
   - **Win Modal**: Muncul saat memicu Win Trigger di Ruang Kontrol.
   - **Lose Modal**: Muncul saat waktu 20 menit habis.
   - Membuka mouse cursor (`Cursor.lockState = CursorLockMode.None`, `Cursor.visible = true`) saat Win/Lose aktif.

### C. Alur Logika Pintu & Scanner (Strict Authorization)
- Pintu **TIDAK AKAN PERNAH** terbuka jika scan belum berstatus `Access Granted`:
  - `scannerlobby`: Memeriksa keberadaan `id_card`. Jika ada -> `Access Granted` -> `PintuMasuk.OpenDoor()`. Jika tidak -> `Access Denied`.
  - `handscanner(buatRoomNextLvlAja)`: Memeriksa apakah `id_card` sudah memiliki hak akses Level 2 (`rwx` dari terminal `chmod`). Jika belum -> `Access Denied` (Listrik mati / Role tidak cukup). Hanya setelah `chmod` di terminal selesai, scanner akan `Access Granted` -> `PintuConRoom.OpenDoor()`.

---

## 4. Keamanan Integrasi Model Karakter Masa Depan

Struktur Player saat ini:
- Root: `Player` (`Rigidbody`, `CapsuleCollider`, `PlayerInputScript`, `InteractionController`, `PickUpController`).
- Children:
  - `PlayerCamera` (Cinemachine)
  - `pickupHolder` (Socket penahan item di depan kamera)
  - `Capsule` (Visual visualizer sementara)

Ketika model 3D karakter beranimasi siap:
1. Taruh prefab karakter 3D sebagai child dari `Player`.
2. Disable komponen `MeshRenderer` pada `Capsule`.
3. Pasang parameter float `Speed` pada Animator dari `PlayerInputScript.movementSpeed`.
4. Seluruh interaksi raycast, inventory, trigger pintu, dan timer **tetap berfungsi 100% tanpa ubahan**.
