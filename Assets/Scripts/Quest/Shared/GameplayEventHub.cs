using System;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class GameplayEventHub : MonoBehaviour, IGameplayEventSource
    {
        public event Action<ItemChangedEvent> ItemAdded;
        public event Action<ItemChangedEvent> ItemRemoved;
        public event Action<InteractionEvent> InteractionCompleted;
        public event Action<AccessEvent> AccessResolved;
        public event Action<AreaEnteredEvent> AreaEntered;
        public event Action<QuestObjectiveEvent> QuestObjectiveCompleted;

        public void PublishItemAdded(string itemId, int amount)
        {
            PublishItemChanged(ItemAdded, itemId, amount, nameof(ItemAdded));
        }

        public void PublishItemRemoved(string itemId, int amount)
        {
            PublishItemChanged(ItemRemoved, itemId, amount, nameof(ItemRemoved));
        }

        public void PublishInteractionCompleted(
            string interactionId,
            GameplayInteractionResult result,
            string reason = null)
        {
            Publish(
                InteractionCompleted,
                new InteractionEvent(interactionId, result, reason),
                nameof(InteractionCompleted));
        }

        public void PublishAccessResolved(
            string accessPointId,
            GameplayInteractionResult result,
            string reason = null)
        {
            Publish(
                AccessResolved,
                new AccessEvent(accessPointId, result, reason),
                nameof(AccessResolved));
        }

        public void PublishAreaEntered(string areaId)
        {
            Publish(AreaEntered, new AreaEnteredEvent(areaId), nameof(AreaEntered));
        }

        public void PublishQuestObjectiveCompleted(string questId, string objectiveId)
        {
            Publish(
                QuestObjectiveCompleted,
                new QuestObjectiveEvent(questId, objectiveId),
                nameof(QuestObjectiveCompleted));
        }

        private static void PublishItemChanged(
            Action<ItemChangedEvent> subscribers,
            string itemId,
            int amount,
            string eventName)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                Debug.LogError($"Cannot publish {eventName}: itemId is empty.");
                return;
            }

            if (amount <= 0)
            {
                Debug.LogError($"Cannot publish {eventName}: amount must be greater than zero.");
                return;
            }

            Publish(subscribers, new ItemChangedEvent(itemId, amount), eventName);
        }

        private static void Publish<T>(Action<T> subscribers, T payload, string eventName)
        {
            if (subscribers == null)
            {
                Debug.LogWarning($"Gameplay event {eventName} has no subscribers.");
                return;
            }

            subscribers.Invoke(payload);
        }
    }
}
