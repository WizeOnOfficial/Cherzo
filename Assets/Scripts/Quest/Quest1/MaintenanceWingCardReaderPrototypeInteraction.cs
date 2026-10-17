using DefaultNamespace;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class MaintenanceWingCardReaderPrototypeInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameplayEventHub eventHub;
        [SerializeField] private InventoryCompatibilityAdapter inventoryQuery;

        public bool HasEmergencyOverride { get; private set; }
        public bool HasGrantedAccess { get; private set; }

        public string InteractMsg => HasGrantedAccess
            ? "Maintenance Wing Access Granted"
            : "Press E to Scan ID Card";

        public void Interact(InteractionController interactionController)
        {
            if (eventHub == null)
            {
                Debug.LogError(
                    $"{nameof(MaintenanceWingCardReaderPrototypeInteraction)} requires a GameplayEventHub reference.",
                    this);
                return;
            }

            if (inventoryQuery == null)
            {
                Debug.LogError(
                    $"{nameof(MaintenanceWingCardReaderPrototypeInteraction)} requires an inventory query reference.",
                    this);
                return;
            }

            if (HasGrantedAccess)
            {
                Debug.Log("Maintenance Wing access has already been granted. No new state change.", this);
                eventHub.PublishAccessResolved(
                    QuestGameplayIds.MaintenanceWingAccess,
                    GameplayInteractionResult.AlreadyCompleted,
                    "Maintenance Wing access already granted.");
                return;
            }

            if (!inventoryQuery.HasItem(QuestGameplayIds.IdCardItem))
            {
                Debug.Log(
                    $"Maintenance Wing card scan denied. Reason: missing '{QuestGameplayIds.IdCardItem}'.",
                    this);
                eventHub.PublishAccessResolved(
                    QuestGameplayIds.MaintenanceWingAccess,
                    GameplayInteractionResult.MissingRequirement,
                    "ID Card is required.");
                return;
            }

            if (!HasEmergencyOverride)
            {
                Debug.Log(
                    "Maintenance Wing card scan denied. Reason: External Fuse is not installed.",
                    this);
                eventHub.PublishAccessResolved(
                    QuestGameplayIds.MaintenanceWingAccess,
                    GameplayInteractionResult.Denied,
                    "Emergency override is not installed.");
                return;
            }

            HasGrantedAccess = true;
            Debug.Log("Maintenance Wing access granted after second ID Card scan.", this);
            eventHub.PublishAccessResolved(
                QuestGameplayIds.MaintenanceWingAccess,
                GameplayInteractionResult.Success,
                "Maintenance Wing access granted.");
        }

        private void OnEnable()
        {
            if (eventHub != null)
            {
                eventHub.InteractionCompleted += HandleInteractionCompleted;
            }
        }

        private void OnDisable()
        {
            if (eventHub != null)
            {
                eventHub.InteractionCompleted -= HandleInteractionCompleted;
            }
        }

        private void HandleInteractionCompleted(InteractionEvent interactionEvent)
        {
            if (interactionEvent.InteractionId != QuestGameplayIds.ControlPanelFuseInstallation ||
                interactionEvent.Result != GameplayInteractionResult.Success)
            {
                return;
            }

            HasEmergencyOverride = true;
            Debug.Log("Maintenance Wing card reader emergency override is now enabled.", this);
        }

        private void Awake()
        {
            if (eventHub == null)
            {
                Debug.LogError(
                    $"{nameof(MaintenanceWingCardReaderPrototypeInteraction)} requires a GameplayEventHub reference.",
                    this);
            }

            if (inventoryQuery == null)
            {
                Debug.LogError(
                    $"{nameof(MaintenanceWingCardReaderPrototypeInteraction)} requires an inventory query reference.",
                    this);
            }
        }
    }
}
