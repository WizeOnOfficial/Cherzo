using DefaultNamespace;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class MaintenanceWingControlRoomAccessInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameplayEventHub eventHub;
        [SerializeField] private InventoryCompatibilityAdapter inventoryQuery;

        [SerializeField] private GameDevelopmentProject.Animation.SlidingDoorController doorToOpen;

        private bool hasEnteredMaintenanceWing;
        private bool hasReportedPowerUnavailable;
        private bool hasPowerBeenRestored;
        private bool hasReportedRoleDenied;
        private bool hasRoleBeenUpgraded;
        private bool hasGrantedAccess;

        public bool HasGrantedAccess => hasGrantedAccess;

        public string InteractMsg
        {
            get
            {
                if (!hasRoleBeenUpgraded)
                {
                    var parser = FindFirstObjectByType<VirtualTerminalCommandParser>();
                    if (parser != null && parser.HasModifiedAccessRole)
                    {
                        hasRoleBeenUpgraded = true;
                    }
                    var q2 = FindFirstObjectByType<Quest2ObjectiveTracker>();
                    if (q2 != null && q2.HasModifiedAccessRole)
                    {
                        hasRoleBeenUpgraded = true;
                    }
                }

                return hasGrantedAccess
                    ? "Control Room Access Granted"
                    : hasRoleBeenUpgraded
                    ? "Press E to Scan Administrator ID Card"
                    : hasReportedRoleDenied
                    ? "Access denied. ID Card role is insufficient."
                    : hasReportedPowerUnavailable && !hasPowerBeenRestored
                    ? "Power unavailable. Repair required."
                    : "Press E to Scan ID Card";
            }
        }

        public void Interact(InteractionController interactionController)
        {
            if (eventHub == null || inventoryQuery == null)
            {
                return;
            }

            if (hasGrantedAccess)
            {
                if (doorToOpen != null && !doorToOpen.IsOpen)
                {
                    doorToOpen.OpenDoor();
                }
                var altDoor = (doorToOpen != null && doorToOpen.gameObject.name == "PintuConPan")
                    ? GameObject.Find("PintuConRoom")?.GetComponent<GameDevelopmentProject.Animation.SlidingDoorController>()
                    : GameObject.Find("PintuConPan")?.GetComponent<GameDevelopmentProject.Animation.SlidingDoorController>();
                if (altDoor != null && !altDoor.IsOpen)
                {
                    altDoor.OpenDoor();
                }
                return;
            }

            if (!hasEnteredMaintenanceWing)
            {
                var q1 = FindFirstObjectByType<Quest1ObjectiveTracker>();
                if (q1 != null && q1.HasScannedIdCardAfterFuse)
                {
                    hasEnteredMaintenanceWing = true;
                }
                else
                {
                    Debug.Log(
                        "Control Room access denied. Reason: Maintenance Wing entry has not been recorded.",
                        this);
                    eventHub.PublishAccessResolved(
                        QuestGameplayIds.ControlRoom,
                        GameplayInteractionResult.Denied,
                        "Maintenance Wing entry is required.");
                    return;
                }
            }

            if (!inventoryQuery.HasItem(QuestGameplayIds.IdCardItem))
            {
                Debug.Log(
                    $"Control Room access denied. Reason: missing '{QuestGameplayIds.IdCardItem}'.",
                    this);
                eventHub.PublishAccessResolved(
                    QuestGameplayIds.ControlRoom,
                    GameplayInteractionResult.MissingRequirement,
                    "ID Card is required.");
                return;
            }

            var pickUpCtrl = interactionController != null
                ? interactionController.GetComponent<PickUpController>() ?? interactionController.GetComponentInParent<PickUpController>()
                : null;
            if (pickUpCtrl == null)
            {
                pickUpCtrl = PickUpController.Instance ?? FindFirstObjectByType<PickUpController>();
            }

            if (pickUpCtrl != null && !pickUpCtrl.IsHoldingItem(QuestGameplayIds.IdCardItem))
            {
                Debug.Log(
                    "Control Room access denied. Reason: ID Card must be held in hand to scan.",
                    this);
                eventHub.PublishAccessResolved(
                    QuestGameplayIds.ControlRoom,
                    GameplayInteractionResult.MissingRequirement,
                    "Hold ID Card in hand to scan.");
                return;
            }

            if (!hasReportedPowerUnavailable)
            {
                hasReportedPowerUnavailable = true;
                Debug.Log(
                    "Control Room access scan detected an electrical short. Power is unavailable.",
                    this);
                eventHub.PublishAccessResolved(
                    QuestGameplayIds.ControlRoom,
                    GameplayInteractionResult.PowerUnavailable,
                    "Electrical short detected.");
                return;
            }

            if (hasReportedPowerUnavailable && !hasPowerBeenRestored)
            {
                Debug.Log(
                    "Control Room access already reported unavailable. Repair is required before another scan.",
                    this);
                eventHub.PublishAccessResolved(
                    QuestGameplayIds.ControlRoom,
                    GameplayInteractionResult.AlreadyCompleted,
                    "Power is unavailable.");
                return;
            }

            if (!hasRoleBeenUpgraded)
            {
                var parser = FindFirstObjectByType<VirtualTerminalCommandParser>();
                if (parser != null && parser.HasModifiedAccessRole)
                {
                    hasRoleBeenUpgraded = true;
                }
                var q2 = FindFirstObjectByType<Quest2ObjectiveTracker>();
                if (q2 != null && q2.HasModifiedAccessRole)
                {
                    hasRoleBeenUpgraded = true;
                }
            }

            if (hasRoleBeenUpgraded)
            {
                hasGrantedAccess = true;
                Debug.Log("Control Room access granted! Administrator privileges confirmed.", this);
                eventHub.PublishAccessResolved(
                    QuestGameplayIds.ControlRoom,
                    GameplayInteractionResult.Success,
                    "Control Room access granted.");

                if (doorToOpen != null)
                {
                    doorToOpen.OpenDoor();
                }
                var altDoor = (doorToOpen != null && doorToOpen.gameObject.name == "PintuConPan")
                    ? GameObject.Find("PintuConRoom")?.GetComponent<GameDevelopmentProject.Animation.SlidingDoorController>()
                    : GameObject.Find("PintuConPan")?.GetComponent<GameDevelopmentProject.Animation.SlidingDoorController>();
                if (altDoor != null && !altDoor.IsOpen)
                {
                    altDoor.OpenDoor();
                }
                return;
            }

            if (hasReportedRoleDenied)
            {
                Debug.Log(
                    "Control Room access is denied. The ID Card role must be modified at the IT desktop.",
                    this);
                eventHub.PublishAccessResolved(
                    QuestGameplayIds.ControlRoom,
                    GameplayInteractionResult.AlreadyCompleted,
                    "ID Card role is insufficient.");
                return;
            }

            hasReportedRoleDenied = true;
            Debug.Log(
                "Control Room access denied. Reason: ID Card role is insufficient after power restoration.",
                this);
            eventHub.PublishAccessResolved(
                QuestGameplayIds.ControlRoom,
                GameplayInteractionResult.Denied,
                "ID Card role must be modified at the IT desktop.");
        }

        private void HandleInteractionCompleted(InteractionEvent interactionEvent)
        {
            if (interactionEvent.InteractionId != QuestGameplayIds.BreakerPanel ||
                interactionEvent.Result != GameplayInteractionResult.Success)
            {
                return;
            }

            hasPowerBeenRestored = true;
            Debug.Log("Control Room panel power state is restored after breaker repair.", this);
        }

        private void HandleAccessRoleModified(AccessRoleModifiedEvent roleEvent)
        {
            if (roleEvent.AccessSubjectId == QuestGameplayIds.IdCardAccessSubject &&
                roleEvent.PermissionValue == QuestGameplayIds.ControlRoomAdministratorPermission)
            {
                hasRoleBeenUpgraded = true;
                Debug.Log("Control Room scanner received administrator credential update.", this);
            }
        }

        private void HandleAreaEntered(AreaEnteredEvent areaEnteredEvent)
        {
            if (areaEnteredEvent.AreaId != QuestGameplayIds.MaintenanceWing)
            {
                return;
            }

            hasEnteredMaintenanceWing = true;
        }

        private void Awake()
        {
            EnsureDependencies();
        }

        private void OnEnable()
        {
            EnsureDependencies();

            if (eventHub != null)
            {
                eventHub.AreaEntered += HandleAreaEntered;
                eventHub.InteractionCompleted += HandleInteractionCompleted;
                eventHub.AccessRoleModified += HandleAccessRoleModified;
            }
        }

        private void EnsureDependencies()
        {
            if (eventHub == null)
            {
                eventHub = FindFirstObjectByType<GameplayEventHub>();
            }

            if (inventoryQuery == null)
            {
                inventoryQuery = FindFirstObjectByType<InventoryCompatibilityAdapter>();
            }

            if (doorToOpen == null)
            {
                var doorObj = GameObject.Find("PintuConPan") ?? GameObject.Find("PintuConRoom");
                if (doorObj != null)
                {
                    doorToOpen = doorObj.GetComponent<GameDevelopmentProject.Animation.SlidingDoorController>();
                }
            }
        }

        private void OnDisable()
        {
            if (eventHub != null)
            {
                eventHub.AreaEntered -= HandleAreaEntered;
                eventHub.InteractionCompleted -= HandleInteractionCompleted;
                eventHub.AccessRoleModified -= HandleAccessRoleModified;
            }
        }
    }
}
