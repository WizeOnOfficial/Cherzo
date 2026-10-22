using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class ControlRoomWinTrigger : MonoBehaviour
    {
        [SerializeField] private GameplayEventHub eventHub;
        private bool hasTriggered;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            if (eventHub == null)
            {
                eventHub = FindFirstObjectByType<GameplayEventHub>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CheckAndTriggerWin(other);
        }

        private void OnTriggerStay(Collider other)
        {
            CheckAndTriggerWin(other);
        }

        private void CheckAndTriggerWin(Collider other)
        {
            if (hasTriggered)
            {
                return;
            }

            if (!other.CompareTag("Player") && other.GetComponent<PlayerInputScript>() == null && other.GetComponentInParent<PlayerInputScript>() == null)
            {
                return;
            }

            var accessInteraction = FindFirstObjectByType<MaintenanceWingControlRoomAccessInteraction>();
            var quest2Tracker = FindFirstObjectByType<Quest2ObjectiveTracker>();

            bool isAuthorized = (accessInteraction != null && accessInteraction.HasGrantedAccess) ||
                                (quest2Tracker != null && quest2Tracker.HasModifiedAccessRole);

            if (!isAuthorized)
            {
                // Player has not yet authorized access through the Control Room scanner.
                return;
            }

            var slidingDoors = FindObjectsByType<GameDevelopmentProject.Animation.SlidingDoorController>(FindObjectsSortMode.None);
            bool isAnyDoorOpen = false;
            foreach (var door in slidingDoors)
            {
                if (door != null && (door.gameObject.name.Contains("Con") || door.gameObject.name.Contains("Control")) && door.IsOpen)
                {
                    isAnyDoorOpen = true;
                    break;
                }
            }

            if (slidingDoors.Length > 0 && !isAnyDoorOpen)
            {
                return;
            }

            hasTriggered = true;
            Debug.Log("[ControlRoomWinTrigger] Player entered Control Room with authorized credentials! Triggering win sequence.", this);

            if (eventHub != null)
            {
                eventHub.PublishAreaEntered(QuestGameplayIds.ControlRoom);
            }

            var hud = FindFirstObjectByType<GameDevelopmentProject.UI.GameplayHUDController>();
            if (hud != null)
            {
                hud.TriggerWinCondition();
            }
        }
    }
}
