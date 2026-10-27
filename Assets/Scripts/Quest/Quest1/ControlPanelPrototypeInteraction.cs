using DefaultNamespace;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class ControlPanelPrototypeInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameplayEventHub eventHub;
        [SerializeField] private InventoryCompatibilityAdapter inventoryQuery;
        [SerializeField] private GameDevelopmentProject.Animation.SlidingDoorController doorToOpen;

        public bool HasAttemptedInitialAccess { get; private set; }
        public bool HasInspectedPanel { get; private set; }
        public bool HasFoundHiddenMessage { get; private set; }
        public bool HasInstalledFuse { get; private set; }
        public bool HasGrantedMaintenanceWingAccess { get; private set; }

        public string InteractMsg
        {
            get
            {
                if (!HasAttemptedInitialAccess)
                {
                    return "Press E to Scan ID Card";
                }

                if (!HasInspectedPanel)
                {
                    return "Press E to Inspect Scanner";
                }

                if (!HasFoundHiddenMessage)
                {
                    return "Inspect Back of Scanner First";
                }

                if (!HasInstalledFuse)
                {
                    return "Press E to Install External Fuse";
                }

                return HasGrantedMaintenanceWingAccess
                    ? "Maintenance Wing Access Granted"
                    : "Press E to Scan ID Card";
            }
        }

        public void Interact(InteractionController interactionController)
        {
            EnsureDependencies();

            if (eventHub == null)
            {
                Debug.LogError($"{nameof(ControlPanelPrototypeInteraction)} requires a GameplayEventHub reference.", this);
                return;
            }

            if (inventoryQuery == null)
            {
                Debug.LogError($"{nameof(ControlPanelPrototypeInteraction)} requires an inventory query reference.", this);
                return;
            }

            var pickUpCtrl = interactionController != null
                ? interactionController.GetComponent<PickUpController>() ?? interactionController.GetComponentInParent<PickUpController>()
                : null;
            if (pickUpCtrl == null)
            {
                pickUpCtrl = PickUpController.Instance ?? FindFirstObjectByType<PickUpController>();
            }

            if (!inventoryQuery.HasItem(QuestGameplayIds.IdCardItem))
            {
                Debug.Log(
                    $"Control Panel access denied. Reason: missing '{QuestGameplayIds.IdCardItem}'.",
                    this);
                eventHub.PublishInteractionCompleted(
                    QuestGameplayIds.ControlPanel,
                    GameplayInteractionResult.MissingRequirement,
                    "ID Card is required.");
                return;
            }

            if (HasAttemptedInitialAccess)
            {
                if (HasInspectedPanel)
                {
                    if (!HasFoundHiddenMessage)
                    {
                        Debug.Log(
                            "Fuse installation unavailable. Find the Hidden Message first.",
                            this);
                        eventHub.PublishInteractionCompleted(
                            QuestGameplayIds.ControlPanelFuseInstallation,
                            GameplayInteractionResult.InvalidState,
                            "Hidden Message is required.");
                        return;
                    }

                    if (HasInstalledFuse)
                    {
                        if (HasGrantedMaintenanceWingAccess)
                        {
                            if (doorToOpen != null && !doorToOpen.IsOpen)
                            {
                                doorToOpen.OpenDoor();
                            }

                            Debug.Log(
                                "Maintenance Wing access has already been granted. No new state change.",
                                this);
                            eventHub.PublishAccessResolved(
                                QuestGameplayIds.MaintenanceWingAccess,
                                GameplayInteractionResult.AlreadyCompleted,
                                "Maintenance Wing access already granted.");
                            return;
                        }

                        if (pickUpCtrl != null && !pickUpCtrl.IsHoldingItem(QuestGameplayIds.IdCardItem))
                        {
                            Debug.Log(
                                "Maintenance Wing card scan denied. Reason: ID Card must be held in hand.",
                                this);
                            eventHub.PublishAccessResolved(
                                QuestGameplayIds.MaintenanceWingAccess,
                                GameplayInteractionResult.MissingRequirement,
                                "Hold ID Card in hand to scan.");
                            return;
                        }

                        HasGrantedMaintenanceWingAccess = true;
                        Debug.Log(
                            "Maintenance Wing access granted after second ID Card scan on the Main Room Control Panel.",
                            this);
                        eventHub.PublishAccessResolved(
                            QuestGameplayIds.MaintenanceWingAccess,
                            GameplayInteractionResult.Success,
                            "Maintenance Wing access granted.");

                        if (doorToOpen != null)
                        {
                            doorToOpen.OpenDoor();
                        }
                        return;
                    }

                    if (!inventoryQuery.HasItem(QuestGameplayIds.ExternalFuseItem))
                    {
                        Debug.Log(
                            $"Fuse installation denied. Reason: missing '{QuestGameplayIds.ExternalFuseItem}'.",
                            this);
                        eventHub.PublishInteractionCompleted(
                            QuestGameplayIds.ControlPanelFuseInstallation,
                            GameplayInteractionResult.MissingRequirement,
                            "External Fuse is required.");
                        return;
                    }

                    if (pickUpCtrl != null && !pickUpCtrl.IsHoldingItem(QuestGameplayIds.ExternalFuseItem))
                    {
                        Debug.Log(
                            "External Fuse installation denied. Reason: External Fuse must be held in hand.",
                            this);
                        eventHub.PublishInteractionCompleted(
                            QuestGameplayIds.ControlPanelFuseInstallation,
                            GameplayInteractionResult.MissingRequirement,
                            "Hold External Fuse in hand to install.");
                        return;
                    }

                    var removalResult = inventoryQuery.RemoveItem(QuestGameplayIds.ExternalFuseItem);
                    if (removalResult != InventoryMutationResult.Success)
                    {
                        Debug.LogError(
                            $"External Fuse installation could not consume the item. Inventory result: {removalResult}.",
                            this);
                        eventHub.PublishInteractionCompleted(
                            QuestGameplayIds.ControlPanelFuseInstallation,
                            GameplayInteractionResult.InvalidState,
                            "External Fuse could not be consumed.");
                        return;
                    }

                    if (pickUpCtrl != null && pickUpCtrl.IsHoldingItem(QuestGameplayIds.ExternalFuseItem))
                    {
                        pickUpCtrl.Unequip();
                    }

                    HasInstalledFuse = true;
                    var remainingFuseAmount =
                        inventoryQuery.CountItem(QuestGameplayIds.ExternalFuseItem);
                    Debug.Log(
                        $"External Fuse installed successfully and consumed. Remaining amount: {remainingFuseAmount}.",
                        this);
                    eventHub.PublishInteractionCompleted(
                        QuestGameplayIds.ControlPanelFuseInstallation,
                        GameplayInteractionResult.Success,
                        "External Fuse installed.");
                    return;
                }

                HasInspectedPanel = true;
                Debug.Log(
                    "Control Panel inspected successfully. The panel remains locked and reveals no message yet.",
                    this);
                eventHub.PublishInteractionCompleted(
                    QuestGameplayIds.ControlPanelInspection,
                    GameplayInteractionResult.Success,
                    "Control Panel inspection completed.");
                return;
            }

            if (pickUpCtrl != null && !pickUpCtrl.IsHoldingItem(QuestGameplayIds.IdCardItem))
            {
                Debug.Log(
                    "Control Panel initial access denied. Reason: ID Card must be held in hand to scan.",
                    this);
                eventHub.PublishInteractionCompleted(
                    QuestGameplayIds.ControlPanel,
                    GameplayInteractionResult.MissingRequirement,
                    "Hold ID Card in hand to scan.");
                return;
            }

            HasAttemptedInitialAccess = true;
            Debug.Log(
                "Control Panel initial access denied. ID Card present, but the panel requires inspection.",
                this);
            eventHub.PublishInteractionCompleted(
                QuestGameplayIds.ControlPanel,
                GameplayInteractionResult.Denied,
                "Initial access is blocked.");
        }

        private void OnEnable()
        {
            EnsureDependencies();

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
            if (interactionEvent.InteractionId == QuestGameplayIds.HiddenMessage &&
                interactionEvent.Result == GameplayInteractionResult.Success)
            {
                HasFoundHiddenMessage = true;
                Debug.Log("Control Panel fuse installation is now available.", this);
            }
        }

        private void Awake()
        {
            EnsureDependencies();
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
                var doorObj = GameObject.Find("PintuMasuk");
                if (doorObj != null)
                {
                    doorToOpen = doorObj.GetComponent<GameDevelopmentProject.Animation.SlidingDoorController>();
                }
            }
        }
    }
}
