using DefaultNamespace;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class HardwareConnectionPrototypeInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameplayEventHub eventHub;

        public bool IsAvailable { get; private set; }
        public bool HasConnectedHardware { get; private set; }

        public string InteractMsg => !IsAvailable
            ? "Find the IT Guide first"
            : HasConnectedHardware
                ? "Hardware connected to IT desktop"
                : "Press E to Connect Hardware";

        public void Interact(InteractionController interactionController)
        {
            if (eventHub == null)
            {
                Debug.LogError(
                    $"{nameof(HardwareConnectionPrototypeInteraction)} requires a GameplayEventHub reference.",
                    this);
                return;
            }

            if (!IsAvailable)
            {
                Debug.Log("Hardware connection is unavailable. Find the IT Guide first.", this);
                eventHub.PublishInteractionCompleted(
                    QuestGameplayIds.HardwareConnection,
                    GameplayInteractionResult.InvalidState,
                    "IT Guide is required.");
                return;
            }

            if (HasConnectedHardware)
            {
                Debug.Log("Hardware is already connected to the IT desktop. No new state change.", this);
                eventHub.PublishInteractionCompleted(
                    QuestGameplayIds.HardwareConnection,
                    GameplayInteractionResult.AlreadyCompleted,
                    "Hardware is already connected.");
                return;
            }

            HasConnectedHardware = true;
            Debug.Log("Access hardware connected to the IT desktop successfully.", this);
            eventHub.PublishInteractionCompleted(
                QuestGameplayIds.HardwareConnection,
                GameplayInteractionResult.Success,
                "Access hardware connected.");
        }

        private void Awake()
        {
            if (eventHub == null)
            {
                Debug.LogError(
                    $"{nameof(HardwareConnectionPrototypeInteraction)} requires a GameplayEventHub reference.",
                    this);
            }
        }

        private void OnEnable()
        {
            if (eventHub != null)
            {
                eventHub.ItemAdded += HandleItemAdded;
            }
        }

        private void OnDisable()
        {
            if (eventHub != null)
            {
                eventHub.ItemAdded -= HandleItemAdded;
            }
        }

        private void HandleItemAdded(ItemChangedEvent itemChanged)
        {
            if (itemChanged.ItemId != QuestGameplayIds.GuideItem)
            {
                return;
            }

            IsAvailable = true;
            Debug.Log("Hardware connection is now available after the IT Guide was found.", this);
        }
    }
}
