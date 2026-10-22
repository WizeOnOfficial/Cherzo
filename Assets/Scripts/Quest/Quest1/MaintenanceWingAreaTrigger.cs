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
            EnsureDependencies();
        }

        private void EnsureDependencies()
        {
            if (eventHub == null)
            {
                eventHub = FindFirstObjectByType<GameplayEventHub>();
            }

            if (playerRoot == null)
            {
                var player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
                if (player != null)
                {
                    playerRoot = player.transform;
                }
            }

            var triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null && !triggerCollider.isTrigger)
            {
                triggerCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (eventHub == null || playerRoot == null)
            {
                EnsureDependencies();
            }

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
