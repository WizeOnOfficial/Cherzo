# Quest Orchestration & Memory Document

**Target:** IF581 UTS Game Development Project (100-Point Target)  
**Scene:** `Assets/Scenes/AurickAssets.unity`  
**Architecture:** Decoupled Event-Driven (`GameplayEventHub`), Non-Destructive to Wilson's Inventory, Self-Healing Fallbacks.

---

## 🗺️ Physical World Object Registry

| Object Name | File ID | Position (X, Y, Z) | Quest Step | Components Attached |
|---|---|---|---|---|
| `keycard` | `7534272423604787406` | Main Lobby Floor | 1.1 | `PhysicsPickup`, `InventoryPickupAcquisition`, `FloatingItemAnimation` |
| `scannerlobby` | `1426083451` | `(1.11, 1.09, 10.84)` | 1.2, 1.3, 1.6, 1.7 | `ControlPanelPrototypeInteraction`, `BoxCollider` on child `Booleano` |
| `PesanRahasia` | `718029623` | Child of `scannerlobby` | 1.4 | `HiddenMessagePrototypeInteraction`, `BoxCollider` |
| `Fuse` | `3683636015132907992` | Janitor Room Floor | 1.5 | `PhysicsPickup`, `InventoryPickupAcquisition`, `InteractionCompletedPickupGate`, `FloatingItemAnimation` |
| `PintuMasuk` | `8476565` | `(-0.03, 1.56, 9.82)` | 1.7, 1.8 | `BoxCollider`, `SlidingDoorController` (openOffset: -1.6, 0, 0 / Slide Left) |
| `MaintenanceWingAreaTrigger` | `88880010` | `(-0.03, 1.50, 10.50)` | 1.8 | `BoxCollider` (isTrigger), `MaintenanceWingAreaTrigger` |
| `scanner` (Control Room) | `645060491` | `(1.05, 1.00, 32.80)` | 2.1, 2.3, 2.7 | `BoxCollider`, `MaintenanceWingControlRoomAccessInteraction` |
| `PintuConPan` (Control Door) | `857856627` | `(-0.03, 1.56, 31.78)` | 2.7, 2.8 | `BoxCollider`, `SlidingDoorController` (openOffset: -1.6, 0, 0 / Slide Left) |
| `PintuJanitor` | `933364136` | `(4.79, 1.56, 7.60)` | Manual | `BoxCollider`, `SlidingDoorController` (Slide Left), `ManualDoorInteraction` [E] |
| `PintuITRoom` | `1409994` | `(6.57, 1.56, 16.26)` | Manual | `BoxCollider`, `SlidingDoorController` (Slide Left), `ManualDoorInteraction` [E] |
| `PintuLab` | `1339165265` | `(-6.82, 1.56, 26.44)` | Manual | `BoxCollider`, `SlidingDoorController` (Slide Left), `ManualDoorInteraction` [E] |
| `PintuRoom2` | `1703353437` | `(6.57, 1.56, 23.53)` | Manual | `BoxCollider`, `SlidingDoorController` (Slide Left), `ManualDoorInteraction` [E] |
| `PintuConRoom` | `764793299` | `(-6.82, 1.56, 20.66)` | Manual | `BoxCollider`, `SlidingDoorController` (Slide Left), `ManualDoorInteraction` [E] |
| `SwitchListrik` (Breaker) | `1472664172115535760` | `(-10.65, 2.09, 15.47)` | 2.2 | `BoxCollider`, `BreakerPanelPrototypeInteraction` |
| `IT Guide` (Pickup) | `88880070` | `(14.50, 1.40, 16.30)` | 2.4 | `PhysicsPickup`, `InventoryPickupAcquisition`, `Quest2ObjectivePickupGate`, `FloatingItemAnimation` |
| `Server(untukUbahKeycard)` | `7771451677583664641` | `(16.47, 0.48, 10.64)` | 2.5, 2.6 | `BoxCollider`, `HardwareConnectionPrototypeInteraction`, `VirtualTerminalCommandParser`, `VirtualTerminalUI` |
| `komputer` (All Desks) | `969145512` & others | IT Center Desks | Visual Props | Original scene state (Pristine visual props) |
| `ControlRoomWinTrigger_ConPan` | `88880105` | `(-0.03, 1.56, 38.00)` | 2.8 | `BoxCollider` (isTrigger, size: 3.5x2.5x2.5), `ControlRoomWinTrigger` |
| `GameplaySystems` | `1349513029` | N/A (Manager Root) | All | `GameplayEventHub`, `InventoryCompatibilityAdapter`, `Quest1ObjectiveTracker`, `Quest2ObjectiveTracker`, `GameplayHUDController`, `EnvironmentPropColliderManager`, `ManualDoorManager` |

---

## ⚡ Quest 2 Step-by-Step Flow Matrix

### 2.1: First Scan at Control Room Door
- **Action:** Player presses `E` on `scanner` at `Z ~ 32.8`.
- **Result:** Electrical short detected (`PowerUnavailable`).
- **Prompt:** Updates to `"Power unavailable. Repair required."`.
- **HUD Banner:** `"Control room offline. Auxiliary electrical power must be restored."`

### 2.2: Repair Breaker in Janitor Room
- **Action:** Player presses `E` on `SwitchListrik`.
- **Result:** Power restored (`BreakerRepaired`).
- **Prompt:** Updates to `"Electrical short repaired"`.
- **HUD Banner:** `"Facility power restored. Scan authorized ID Card at the Control Room scanner."`

### 2.3: Second Scan & Role Denial
- **Action:** Player returns to `scanner` and presses `E`.
- **Result:** Scanner powers up, but rejects Guest card (`RoleDenied`).
- **Prompt:** Updates to `"Access denied. ID Card role is insufficient."`.
- **Gate:** `Quest2ObjectivePickupGate` unlocks.
- **HUD Banner:** `"Security clearance insufficient. Search the IT wing for administrator guidelines."`

### 2.4: Retrieve IT Guide
- **Action:** Player enters IT Room and picks up floating `IT Guide` from desk.
- **Result:** `item.it_guide` added to Wilson's inventory.
- **HUD Banner:** `"IT guide recovered. Connect the hardware terminal module at the server rack."`

### 2.5: Connect Hardware Module at Server Rack
- **Action:** Player presses `E` on `Server(untukUbahKeycard)`.
- **Result:** `HardwareConnection` succeeds.
- **Prompt:** Instantly updates to `"Press E to Open Terminal"`.
- **HUD Banner:** `"Hardware linked. Access server terminal to reconfigure security permissions."`

### 2.6: Modify Access Role at Server Console Terminal
- **Action:** Player presses `E` again on `Server(untukUbahKeycard)`.
- **Result:** Retro Hacker Terminal POV opens on screen. Player movement locks, cursor unlocks.
- **Command:** Type `chmod u=rwx id_card` and press Enter / click `EXECUTE`.
- **Terminal Result:** Card permissions elevated: `r--` -> `rwx` (`role.control_room_administrator`).
- **HUD Banner:** `"Credentials elevated. Scan Administrator ID Card at the Control Room scanner."`
- **Exit:** Player presses `ESC` or clicks `EXIT (ESC)`.

### 2.7: Final Scan at Control Room Scanner
- **Action:** Player returns to `scanner` and presses `E`.
- **Result:** Scanner validates Administrator privileges!
- **Door:** `PintuConPan` slides open smoothly via `SlidingDoorController`.
- **Prompt:** `"Control Room Access Granted"`.
- **HUD Banner:** `"Sector 2 access authorized. Enter the Main Control Room."`

### 2.8: Enter Control Room & Win Screen
- **Action:** Player crosses doorway threshold into Control Room.
- **Result:** `ControlRoomWinTrigger` detects player.
- **Victory Modal:** Displays `"FACILITY ACCESS RESTORED"`, remaining time, and Play Again / Main Menu buttons.

---

## 🛡️ Self-Healing Architecture
1. **Multi-Workstation Accessibility:** All 8 computers in the IT room possess colliders and terminal interaction scripts pointing to the central parser. The player never gets stuck trying the "wrong" computer.
2. **OnGUI Fallback Systems:**
   - `VirtualTerminalUI`: Works automatically on-screen even if no Canvas UI is assigned.
   - `GameplayHUDController`: Renders 20-min countdown timer, objective banners, and win/lose modals directly via OnGUI if Canvas UI components are unlinked.
3. **Resilient State Queries:**
   - `ITDesktopTerminalInteraction` and `VirtualTerminalCommandParser` check both `quest2Tracker.HasConnectedHardware` and `HardwareConnectionPrototypeInteraction.HasConnectedHardware` directly to avoid any single point of failure.
   - `MaintenanceWingControlRoomAccessInteraction` automatically confirms entry if the player has passed Quest 1.

---

## 🏛️ Facility Environment Colliders (Audit & Fix)
1. **The Root Cause of Blocked Movement (Room 1 & Hallways):**
   - In earlier iterations, `EnvironmentPropColliderManager.cs` attempted to fit `BoxCollider`s around objects with an arbitrary `minThickness = 0.3f` clamped in local space.
   - FBX imports (such as `meja.fbx` for desks) possess transforms with `localScale = (110.02, 110.02, 110.02)`.
   - Clamping `size` to 0.3m in local space multiplied by 110 created **33-meter wide invisible solid blocks** on each of the 20 desks in `room1` and throughout the facility.
   - Furthermore, compound FBX meshes like `BrownChairs` (containing 10 conference chairs around a table) received a single `BoxCollider` enclosing the entire meeting area, blocking all walkways.
2. **The Exact-Fit Solution ("Collider Tepat Pada Object"):**
   - Refactored `EnvironmentPropColliderManager.cs` to assign native **`MeshCollider`** (`convex = false`) directly to each visual leaf `MeshFilter`.
   - On static scene props, `MeshCollider` conforms 100% to the exact surface triangles of the 3D model:
     - Desk tops, legs, and drawers are solid; the space underneath and all aisles between desks are completely open.
     - Conference chairs and tables have exact individual collision; walkways around and between them are 100% passable.
     - Scale invariance: PhysX transforms vertices natively via `localToWorldMatrix`, completely eliminating scale multiplier bugs.
   - **Protection Guards:**
     - Skips objects that already have colliders (doors, walls, floors, ceilings, quest items).
     - Skips interactive quest objects (`IInteractable`).
     - Skips non-prop entities (lights, lamps, signs, billboards, triggers).
     - Spatial Safety Guard prevents any prop collider from intersecting the player's spawn point at `(0, 1.5, 0)`.
