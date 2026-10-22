using DefaultNamespace;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class BreakerPanelPrototypeInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameplayEventHub eventHub;

        public bool IsAvailable { get; private set; }
        public bool HasRepairedElectricalShort { get; private set; }

        public string InteractMsg => !IsAvailable
            ? string.Empty
            : HasRepairedElectricalShort
                ? "Electrical short repaired"
                : "Press E to Repair Electrical Short";

        public void Interact(InteractionController interactionController)
        {
            if (eventHub == null)
            {
                Debug.LogError(
                    $"{nameof(BreakerPanelPrototypeInteraction)} requires a GameplayEventHub reference.",
                    this);
                return;
            }

            if (!IsAvailable)
            {
                Debug.Log(
                    "Breaker Panel is unavailable until the Control Room scan detects the electrical short.",
                    this);
                eventHub.PublishInteractionCompleted(
                    QuestGameplayIds.BreakerPanel,
                    GameplayInteractionResult.InvalidState,
                    "Electrical short has not been detected.");
                return;
            }

            if (HasRepairedElectricalShort)
            {
                Debug.Log("Electrical short has already been repaired. No new state change.", this);
                eventHub.PublishInteractionCompleted(
                    QuestGameplayIds.BreakerPanel,
                    GameplayInteractionResult.AlreadyCompleted,
                    "Electrical short already repaired.");
                return;
            }

            HasRepairedElectricalShort = true;
            Debug.Log("Electrical short repaired successfully. Power is restored.", this);
            eventHub.PublishInteractionCompleted(
                QuestGameplayIds.BreakerPanel,
                GameplayInteractionResult.Success,
                "Electrical short repaired.");
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
                eventHub.AccessResolved += HandleAccessResolved;
            }
        }

        private void EnsureDependencies()
        {
            if (eventHub == null)
            {
                eventHub = FindFirstObjectByType<GameplayEventHub>();
            }
        }

        private void OnDisable()
        {
            if (eventHub != null)
            {
                eventHub.AccessResolved -= HandleAccessResolved;
            }
        }

        private void HandleAccessResolved(AccessEvent accessEvent)
        {
            if (accessEvent.AccessPointId != QuestGameplayIds.ControlRoom ||
                accessEvent.Result != GameplayInteractionResult.PowerUnavailable)
            {
                return;
            }

            IsAvailable = true;
            Debug.Log("Breaker Panel is now available after the Control Room electrical short.", this);
        }
    }
}
