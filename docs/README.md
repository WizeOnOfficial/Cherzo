# Project Coordination Documents

Dokumen di folder ini adalah sumber konteks bersama untuk manusia dan AI. Chat bukan sumber kebenaran utama.

## Status legend

- **Implemented** — terlihat di kode/aset dan dapat diverifikasi.
- **Planned** — sudah disepakati sebagai target, tetapi belum dibuat.
- **Blocked** — belum dapat dikerjakan karena dependency/keputusan belum tersedia.
- **Assumption** — asumsi sementara yang harus divalidasi manusia.
- **Adjusted** — aturan/kontrak diubah secara sadar dan harus dicatat dalam handoff atau ADR.

AI wajib mempertahankan label status ini. Jangan menggambarkan kontrak `Planned` seolah-olah sudah tersedia di runtime.

## Dokumen inti

| Dokumen | Fungsi |
|---|---|
| [`../project-brain.md`](../project-brain.md) | Audit global dan baseline kondisi proyek. |
| [`architecture.md`](architecture.md) | Batas modul dan dependency yang diizinkan. |
| [`gameplay-contracts.md`](gameplay-contracts.md) | Event, state, ID, dan API lintas modul. |
| [`quest-flow.md`](quest-flow.md) | Urutan gameplay dan acceptance flow. |
| [`quest-architecture.md`](quest-architecture.md) | Diagram arsitektur dan alur event antar-modul Quest. |
| [`inventory-contract.md`](inventory-contract.md) | Kontrak Inventory System. |
| [`inventory-quest-integration-contract.md`](inventory-quest-integration-contract.md) | Boundary ketat integrasi Inventory dengan Quest. |
| [`interaction-contract.md`](interaction-contract.md) | Kontrak object interaction. |
| [`quest-coding-rules.md`](quest-coding-rules.md) | Aturan coding wajib untuk Quest dan shared gameplay code. |
| [`quest-work-log.md`](quest-work-log.md) | Log kerja, checklist validasi, dan ringkasan sebelum commit/push. |
| [`git-workflow.md`](git-workflow.md) | Aturan branch, pull, push, merge, dan ownership. |
| [`decisions/`](decisions/) | Architecture Decision Records (ADR). |
| [`handoffs/`](handoffs/) | Handoff antar-AI/anggota tim. |

## Cara memakai

1. Baca dokumen inti sebelum coding.
2. Jika keputusan desain berubah, perbarui kontrak atau buat ADR.
3. Setelah sesi kerja, isi handoff dan commit bersama perubahan yang relevan.
4. Jangan menganggap isi percakapan AI sebagai state proyek yang permanen.

Dokumen bertanda `DRAFT` harus disepakati sebelum dijadikan kontrak implementasi.

## Current coordination status

| Area | Status | Owner/next action |
|---|---|---|
| Existing Inventory implementation | Implemented/actively evolving | Inventory contributor |
| Inventory compatibility boundary | Implemented foundation | Read-only adapter exists; serialized scene wiring must be saved/verified; event bridge remains planned |
| Quest 1 flow | Implemented prototype through objective 7 | Implement area trigger and Maintenance Wing entry |
| Quest 2 flow | Planned | Quest 2 contributor |
| Shared gameplay IDs/events | Implemented foundation | Producers/consumers still planned |
| Main scene integration | Planned | One scene integrator only; current repository scene must be verified after Editor changes are saved |
| Sprint 1 freeze rules | Planned | Freeze after Control Room entry is verified |

Latest validation note:

- GameplayEventHub scene setup: reported working in the Editor play test; serialized scene wiring still requires repository verification.
- Existing movement and pickup: still functional.
- Console cleanup: blocked by a pre-existing missing script warning and Unity AI Assistant `NoSubscription` messages.

## AI session closeout

Every AI session must report:

1. what is `Done`;
2. what is `Not done`;
3. what was `Adjusted`;
4. what is `Blocked`;
5. files changed;
6. contract/dependency changes;
7. validation performed;
8. next adjustment required.
