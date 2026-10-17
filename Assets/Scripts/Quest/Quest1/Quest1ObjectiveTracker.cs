using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class Quest1ObjectiveTracker : MonoBehaviour
    {
        [SerializeField] private GameplayEventHub eventHub;
        [SerializeField] private InventoryCompatibilityAdapter inventoryQuery;

        public bool HasFoundIdCard { get; private set; }
        public bool HasAttemptedInitialControlPanelAccess { get; private set; }
        public bool HasInspectedControlPanel { get; private set; }
        public bool HasFoundHiddenMessage { get; private set; }
        public bool HasFoundExternalFuse { get; private set; }
        public bool HasInstalledExternalFuse { get; private set; }
        public bool HasScannedIdCardAfterFuse { get; private set; }
        public bool HasEnteredMaintenanceWing { get; private set; }

        private void OnEnable()
        {
            if (eventHub == null)
            {
                Debug.LogError($"{nameof(Quest1ObjectiveTracker)} requires a GameplayEventHub reference.", this);
                return;
            }

            eventHub.ItemAdded += HandleItemAdded;
            eventHub.InteractionCompleted += HandleInteractionCompleted;
            eventHub.InteractionCompleted += HandleControlPanelInspection;
            eventHub.InteractionCompleted += HandleHiddenMessage;
            eventHub.InteractionCompleted += HandleFuseInstallation;
            eventHub.AccessResolved += HandleMaintenanceWingAccess;
            eventHub.AreaEntered += HandleAreaEntered;
            RefreshFromInventory();
        }

        private void OnDisable()
        {
            if (eventHub != null)
            {
                eventHub.ItemAdded -= HandleItemAdded;
                eventHub.InteractionCompleted -= HandleInteractionCompleted;
                eventHub.InteractionCompleted -= HandleControlPanelInspection;
                eventHub.InteractionCompleted -= HandleHiddenMessage;
                eventHub.InteractionCompleted -= HandleFuseInstallation;
                eventHub.AccessResolved -= HandleMaintenanceWingAccess;
                eventHub.AreaEntered -= HandleAreaEntered;
            }
        }

        private void HandleItemAdded(ItemChangedEvent itemChanged)
        {
            if (itemChanged.ItemId == QuestGameplayIds.IdCardItem)
            {
                CompleteFindIdCard();
            }
            else if (itemChanged.ItemId == QuestGameplayIds.ExternalFuseItem)
            {
                CompleteFindExternalFuse();
            }
        }

        private void HandleInteractionCompleted(InteractionEvent interactionEvent)
        {
            if (interactionEvent.InteractionId != QuestGameplayIds.ControlPanel ||
                interactionEvent.Result != GameplayInteractionResult.Denied ||
                !HasFoundIdCard ||
                HasAttemptedInitialControlPanelAccess)
            {
                return;
            }

            HasAttemptedInitialControlPanelAccess = true;
            Debug.Log(
                "Quest 1 objective completed: initial Control Panel access attempt recorded.",
                this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest1,
                QuestGameplayIds.Quest1TryInitialControlPanelAccess);
        }

        private void HandleControlPanelInspection(InteractionEvent interactionEvent)
        {
            if (interactionEvent.InteractionId != QuestGameplayIds.ControlPanelInspection ||
                interactionEvent.Result != GameplayInteractionResult.Success ||
                !HasAttemptedInitialControlPanelAccess ||
                HasInspectedControlPanel)
            {
                return;
            }

            HasInspectedControlPanel = true;
            Debug.Log("Quest 1 objective completed: Control Panel inspected.", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest1,
                QuestGameplayIds.Quest1InspectControlPanel);
        }

        private void HandleHiddenMessage(InteractionEvent interactionEvent)
        {
            if (interactionEvent.InteractionId != QuestGameplayIds.HiddenMessage ||
                interactionEvent.Result != GameplayInteractionResult.Success ||
                !HasInspectedControlPanel ||
                HasFoundHiddenMessage)
            {
                return;
            }

            HasFoundHiddenMessage = true;
            Debug.Log("Quest 1 objective completed: Hidden Message found.", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest1,
                QuestGameplayIds.Quest1FindHiddenMessage);
        }

        private void HandleFuseInstallation(InteractionEvent interactionEvent)
        {
            if (interactionEvent.InteractionId != QuestGameplayIds.ControlPanelFuseInstallation ||
                interactionEvent.Result != GameplayInteractionResult.Success ||
                !HasFoundHiddenMessage ||
                !HasFoundExternalFuse ||
                HasInstalledExternalFuse)
            {
                return;
            }

            HasInstalledExternalFuse = true;
            Debug.Log("Quest 1 objective completed: External Fuse installed.", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest1,
                QuestGameplayIds.Quest1InstallExternalFuse);
        }

        private void HandleMaintenanceWingAccess(AccessEvent accessEvent)
        {
            if (accessEvent.AccessPointId != QuestGameplayIds.MaintenanceWingAccess ||
                accessEvent.Result != GameplayInteractionResult.Success ||
                !HasInstalledExternalFuse ||
                HasScannedIdCardAfterFuse)
            {
                return;
            }

            HasScannedIdCardAfterFuse = true;
            Debug.Log("Quest 1 objective completed: ID Card scanned after fuse installation.", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest1,
                QuestGameplayIds.Quest1ScanIdCardAfterFuse);
        }

        private void HandleAreaEntered(AreaEnteredEvent areaEnteredEvent)
        {
            if (areaEnteredEvent.AreaId != QuestGameplayIds.MaintenanceWing ||
                !HasScannedIdCardAfterFuse ||
                HasEnteredMaintenanceWing)
            {
                return;
            }

            HasEnteredMaintenanceWing = true;
            Debug.Log("Quest 1 objective completed: Maintenance Wing entered.", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest1,
                QuestGameplayIds.Quest1EnterMaintenanceWing);
        }

        private void RefreshFromInventory()
        {
            if (inventoryQuery == null)
            {
                Debug.LogError($"{nameof(Quest1ObjectiveTracker)} requires an inventory query reference.", this);
                return;
            }

            if (inventoryQuery.HasItem(QuestGameplayIds.IdCardItem))
            {
                CompleteFindIdCard();
            }
            else
            {
                Debug.Log($"Quest 1 HasFoundIdCard: {HasFoundIdCard}", this);
            }

            if (inventoryQuery.HasItem(QuestGameplayIds.ExternalFuseItem))
            {
                CompleteFindExternalFuse();
            }
        }

        private void CompleteFindIdCard()
        {
            if (HasFoundIdCard)
            {
                return;
            }

            HasFoundIdCard = true;
            Debug.Log($"Quest 1 HasFoundIdCard: {HasFoundIdCard}", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest1,
                QuestGameplayIds.Quest1FindIdCard);
        }

        private void CompleteFindExternalFuse()
        {
            if (HasFoundExternalFuse)
            {
                return;
            }

            HasFoundExternalFuse = true;
            Debug.Log($"Quest 1 HasFoundExternalFuse: {HasFoundExternalFuse}", this);
            Debug.Log("Quest 1 objective completed: External Fuse found.", this);
            eventHub.PublishQuestObjectiveCompleted(
                QuestGameplayIds.Quest1,
                QuestGameplayIds.Quest1FindExternalFuse);
        }
    }
}
