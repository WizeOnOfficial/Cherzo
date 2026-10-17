using DefaultNamespace;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class HiddenMessagePrototypeInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameplayEventHub eventHub;

        public bool IsAvailable { get; private set; }
        public bool HasBeenFound { get; private set; }

        public string InteractMsg =>
            IsAvailable ? "Press E to Search Hidden Message" : "Inspect Control Panel First";

        public void Interact(InteractionController interactionController)
        {
            if (eventHub == null)
            {
                Debug.LogError($"{nameof(HiddenMessagePrototypeInteraction)} requires a GameplayEventHub reference.", this);
                return;
            }

            if (!IsAvailable)
            {
                Debug.Log("Hidden Message is unavailable. Inspect the Control Panel first.", this);
                eventHub.PublishInteractionCompleted(
                    QuestGameplayIds.HiddenMessage,
                    GameplayInteractionResult.InvalidState,
                    "Control Panel inspection is required.");
                return;
            }

            if (HasBeenFound)
            {
                Debug.Log("Hidden Message has already been found. No new state change.", this);
                eventHub.PublishInteractionCompleted(
                    QuestGameplayIds.HiddenMessage,
                    GameplayInteractionResult.AlreadyCompleted,
                    "Hidden Message already found.");
                return;
            }

            HasBeenFound = true;
            Debug.Log("Hidden Message found successfully.", this);
            eventHub.PublishInteractionCompleted(
                QuestGameplayIds.HiddenMessage,
                GameplayInteractionResult.Success,
                "Hidden Message found.");
        }

        private void OnEnable()
        {
            if (eventHub == null)
            {
                Debug.LogError($"{nameof(HiddenMessagePrototypeInteraction)} requires a GameplayEventHub reference.", this);
                return;
            }

            eventHub.InteractionCompleted += HandleInteractionCompleted;
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
            if (interactionEvent.InteractionId == QuestGameplayIds.ControlPanelInspection &&
                interactionEvent.Result == GameplayInteractionResult.Success)
            {
                IsAvailable = true;
                Debug.Log("Hidden Message is now available after Control Panel inspection.", this);
            }
        }
    }
}
