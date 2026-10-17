using System.Collections.Generic;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class MaintenanceWingAreaTrigger : MonoBehaviour
    {
        [SerializeField] private GameplayEventHub eventHub;
        [SerializeField] private Transform playerRoot;

        private readonly HashSet<Collider> playerCollidersInside = new();

        private void Awake()
        {
            if (eventHub == null)
            {
                Debug.LogError($"{nameof(MaintenanceWingAreaTrigger)} requires a GameplayEventHub reference.", this);
            }

            if (playerRoot == null)
            {
                Debug.LogError($"{nameof(MaintenanceWingAreaTrigger)} requires a player root reference.", this);
            }
            else if (playerRoot.GetComponent<Rigidbody>() == null)
            {
                Debug.LogError(
                    $"{nameof(MaintenanceWingAreaTrigger)} requires the configured player root to have a Rigidbody. " +
                    "Use a kinematic Rigidbody for transform-driven movement.",
                    this);
            }

            var triggerCollider = GetComponent<Collider>();
            if (triggerCollider == null || !triggerCollider.isTrigger)
            {
                Debug.LogError(
                    $"{nameof(MaintenanceWingAreaTrigger)} requires a Collider with Is Trigger enabled on the same GameObject.",
                    this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (eventHub == null || playerRoot == null)
            {
                return;
            }

            if (other.transform != playerRoot && !other.transform.IsChildOf(playerRoot))
            {
                return;
            }

            if (playerCollidersInside.Add(other) && playerCollidersInside.Count == 1)
            {
                Debug.Log("Maintenance Wing area entered.", this);
                eventHub.PublishAreaEntered(QuestGameplayIds.MaintenanceWing);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (playerRoot == null ||
                (other.transform != playerRoot && !other.transform.IsChildOf(playerRoot)))
            {
                return;
            }

            playerCollidersInside.Remove(other);
        }
    }
}
