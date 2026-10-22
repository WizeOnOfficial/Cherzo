# Panduan Integrasi Aset 3D & Alur Quest 1–2 (Map Baru & TestScene)

**Target Selesai:** < 5–30 Menit  
**Fokus:** Menghubungkan seluruh aset 3D baru (model & map dari Aurick) ke dalam alur logika Quest 1 dan Quest 2 tanpa merusak kontrak kode yang ada, menggunakan Player bawaan (capsule/hero standar).

---

## 1. Pemilihan Scene untuk Integrasi

Teman Anda (Aurick) telah menyusun peta ruangan lengkap di:  
📍 **`Assets/Scenes/AurickAssets.unity`**  
Di dalam scene map tersebut sudah terdapat ruangan-ruangan nyata:
- `ControlPanel` (Konsol utama)
- `PintuJanitor` (Ruang sekring)
- `ControlListrik` (Panel sekring daya)
- `ITCenter` (Ruang server dan terminal komputer)
- `PintuConRoom` (Pintu akses ke Control Room)
- `Player` (Hero kapsul standar)

Untuk menjaga **keamanan 100% (Zero Risk)**:
- Kami telah membuatkan menu otomatis di Unity Editor yang **mengkloning map tersebut menjadi scene baru**:
  📍 **`Assets/Scenes/MainGameScene.unity`**
- Scene asli teman Anda (`AurickAssets.unity`) dan scene lama (`TestScene.unity`) **tetap 100% aman dan tidak tersentuh**.

---

## 2. Pemetaan Lengkap Alur Quest 1 & Quest 2 di Map Asli

### A. Alur Quest 1 — Main Lobby Menuju Maintenance Wing

| Langkah | Objektif | Lokasi di Map | Komponen Logika Wajib | Kondisi / Trigger |
|---|---|---|---|---|
| **1.1** | Ambil ID Card | Di depan Player / Meja Lobby (`keycard.fbx`) | `PhysicsPickup`, `BoxCollider`, `Rigidbody`, `InventoryPickupAcquisition` | Player menekan `E`. Kartu masuk ke Inventory (`item.id_card`). |
| **1.2** | Coba Akses Control Panel | Pada objek `ControlPanel` di Map | `ControlPanelPrototypeInteraction`, `BoxCollider` | Tekan `E`. Sistem menolak akses pertama kali (`Denied`). |
| **1.3** | Inspeksi Control Panel | Pada objek `ControlPanel` | `ControlPanelPrototypeInteraction` | Tekan `E` kedua kali. Menerbitkan event `ControlPanelInspected`. |
| **1.4** | Temukan Hidden Message | Dekat `ControlPanel` (`White_Boards.fbx`) | `HiddenMessagePrototypeInteraction`, `BoxCollider` | Menjadi aktif setelah inspeksi panel. Tekan `E` untuk membaca pesan rahasia lokasi sekring. |
| **1.5** | Ambil External Fuse | Di dekat `PintuJanitor` (`fuse.fbx`) | `PhysicsPickup`, `InteractionCompletedPickupGate`, `BoxCollider`, `Rigidbody` | Fuse di Janitor Room terbuka kuncinya setelah pesan ditemukan. Tekan `E` untuk mengambil. |
| **1.6** | Pasang Fuse di Panel | Pada `ControlPanel` | `ControlPanelPrototypeInteraction` | Tekan `E` di panel kontrol. Fuse dikonsumsi (`FuseInstalled`). |
| **1.7** | Scan Ulang ID Card | Pada `ControlPanel` atau Card Reader Lorong | `ControlPanelPrototypeInteraction` / Card Reader | Tekan `E`. Akses dibuka: `MaintenanceWingAccessGranted`. |
| **1.8** | Masuk Maintenance Wing | Lorong Sayap Perawatan | `MaintenanceWingAreaTrigger`, `BoxCollider (isTrigger)` | Player berjalan melewati pintu/lorong. Event `MaintenanceWingEntered` terbit. **Quest 1 Selesai!** |

---

### B. Alur Quest 2 — Maintenance Wing Menuju Control Room

| Langkah | Objektif | Lokasi di Map | Komponen Logika Wajib | Kondisi / Trigger |
|---|---|---|---|---|
| **2.1** | Scan ID Card di Scanner Pintu | Di samping `PintuConRoom` (`handscanner.fbx`) | `MaintenanceWingControlRoomAccessInteraction`, `BoxCollider` | Scan pertama kali mendeteksi konsleting listrik (`PowerUnavailable`). |
| **2.2** | Perbaiki Panel Sekring Listrik | Pada objek `ControlListrik` di Map | `BreakerPanelPrototypeInteraction`, `BoxCollider` | Panel sekring aktif. Tekan `E` untuk memperbaiki listrik (`BreakerRepaired`). |
| **2.3** | Scan Ulang & Penolakan Role | Di samping `PintuConRoom` | `MaintenanceWingControlRoomAccessInteraction` | Daya sudah ada, tapi kartu hanya level Guest (`r--`). Akses ditolak karena role kurang (`quest.2.role_denied`). |
| **2.4** | Temukan IT Guide | Di dalam area `ITCenter` | `PhysicsPickup`, `Quest2ObjectivePickupGate`, `BoxCollider` | Panduan IT terbuka setelah penolakan role. Tekan `E` untuk mengambil panduan IT. |
| **2.5** | Hubungkan Hardware Akses | Di rak server `ITCenter` (`serverrack 1.fbx`) | `HardwareConnectionPrototypeInteraction`, `BoxCollider` | Tekan `E` di rak server untuk menghubungkan modul hardware ke terminal IT. |
| **2.6** | Modifikasi Role via Terminal IT | Di PC `ITCenter` (`komputer.fbx`) | `ITDesktopTerminalInteraction`, `VirtualTerminalUI`, `VirtualTerminalCommandParser` | Tekan `E` pada monitor PC untuk membuka terminal Linux simulasi. Ketik: `chmod u=rwx id_card`. Role berubah menjadi Administrator! |
| **2.7** | Scan Kartu Terakhir Kali | Di samping `PintuConRoom` | `MaintenanceWingControlRoomAccessInteraction` | Role sudah `rwx`. Pintu Control Room terbuka (`ControlRoomAccessGranted`)! |
| **2.8** | Masuk ke Control Room | Masuk melintasi `PintuConRoom` | Ambang Pintu Control Room | Melintasi ambang pintu. **Sprint 1 Complete (Win Screen)!** |

---

## 3. Langkah Menjalankan di Unity Editor (Menu 1-Klik)

Di Unity Editor, menu bar atas telah menyediakan opsi:

1. **Pilihan 1 (Paling Direkomendasikan):**
   - Klik: **`Quest > 1. Buat Scene Baru Dari Map & Pasang Quest (Sangat Direkomendasikan)`**
   - Unity akan otomatis menduplikasi map teman Anda menjadi `Assets/Scenes/MainGameScene.unity` dan langsung memasang seluruh komponen interaksi serta text UI "Press E to...".
2. **Pilihan 2 (Pasang di Scene yang Sedang Anda Buka):**
   - Buka scene manapun yang Anda inginkan di Unity.
   - Klik: **`Quest > 2. Pasang Quest di Scene yang Sedang Dibuka (Active Scene)`**
3. **Pilihan 3 (Pasang di TestScene Lama):**
   - Klik: **`Quest > 3. Pasang Quest di TestScene (Scene Prototype Lama)`**

Setelah itu, tinggal tekan tombol **Play** di Unity Editor untuk mencoba memainkannya!
