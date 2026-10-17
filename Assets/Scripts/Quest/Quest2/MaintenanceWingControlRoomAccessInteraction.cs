using DefaultNamespace;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class MaintenanceWingControlRoomAccessInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameplayEventHub eventHub;
        [SerializeField] private InventoryCompatibilityAdapter inventoryQuery;

        private bool hasEnteredMaintenanceWing;
        private bool hasReportedPowerUnavailable;
        private bool hasPowerBeenRestored;
        private bool hasReportedRoleDenied;

        public string InteractMsg => hasReportedRoleDenied
            ? "Access denied. ID Card role is insufficient."
            : hasReportedPowerUnavailable && !hasPowerBeenRestored
            ? "Power unavailable. Repair required."
            : "Press E to Scan ID Card";

        public void Interact(InteractionController interactionController)
        {
            if (eventHub == null || inventoryQuery == null)
            {
                return;
            }

            if (!hasEnteredMaintenanceWing)
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
            if (eventHub == null)
            {
                Debug.LogError(
                    $"{nameof(MaintenanceWingControlRoomAccessInteraction)} requires a GameplayEventHub reference.",
                    this);
            }

            if (inventoryQuery == null)
            {
                Debug.LogError(
                    $"{nameof(MaintenanceWingControlRoomAccessInteraction)} requires an inventory query reference.",
                    this);
            }
        }

        private void OnEnable()
        {
            if (eventHub != null)
            {
                eventHub.AreaEntered += HandleAreaEntered;
                eventHub.InteractionCompleted += HandleInteractionCompleted;
            }
        }

        private void OnDisable()
        {
            if (eventHub != null)
            {
                eventHub.AreaEntered -= HandleAreaEntered;
                eventHub.InteractionCompleted -= HandleInteractionCompleted;
            }
        }
    }
}
