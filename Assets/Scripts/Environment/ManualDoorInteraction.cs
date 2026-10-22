using DefaultNamespace;
using GameDevelopmentProject.Animation;
using UnityEngine;

namespace GameDevelopmentProject.Environment
{
    /// <summary>
    /// Enables manual interaction (Press [E] to Open / Close) for non-quest doors.
    /// Implements IInteractable to seamlessly integrate with InteractionController.
    /// Self-heals by ensuring SlidingDoorController and BoxCollider are present.
    /// </summary>
    [DisallowMultipleComponent]
    public class ManualDoorInteraction : MonoBehaviour, IInteractable
    {
        [Header("Door Configuration")]
        [SerializeField] private SlidingDoorController doorController;
        [SerializeField] private string doorDisplayName = "Door";
        [SerializeField] private Vector3 slideOffset = new Vector3(-1.6f, 0f, 0f);

        [Header("Lock Settings (Optional)")]
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string lockedMessage = "Door is locked.";

        public string InteractMsg
        {
            get
            {
                if (isLocked)
                {
                    return lockedMessage;
                }

                if (doorController != null && doorController.IsOpen)
                {
                    return $"[E] Tutup {doorDisplayName}";
                }

                return $"[E] Buka {doorDisplayName}";
            }
        }

        private void Awake()
        {
            EnsureComponents();
        }

        public void Initialize(string displayName, Vector3 offset)
        {
            doorDisplayName = displayName;
            slideOffset = offset;
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            // Ensure solid BoxCollider is present on the door
            var col = GetComponent<Collider>();
            if (col == null)
            {
                var box = gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(0.015513732f, 0.028852351f, 0.020000003f);
                box.center = new Vector3(0f, 0.0025441863f, 0f);
                box.isTrigger = false;
            }

            // Ensure SlidingDoorController is mounted and configured to slide left
            if (doorController == null)
            {
                doorController = GetComponent<SlidingDoorController>();
            }

            if (doorController == null)
            {
                doorController = gameObject.AddComponent<SlidingDoorController>();
                doorController.SetOpenOffset(slideOffset);
            }
            else
            {
                doorController.SetOpenOffset(slideOffset);
            }
        }

        public void Interact(InteractionController interactionController)
        {
            if (isLocked)
            {
                Debug.Log($"[ManualDoorInteraction] {doorDisplayName} is locked.", this);
                return;
            }

            EnsureComponents();

            if (doorController != null)
            {
                doorController.ToggleDoor();
            }
        }

        public void SetLocked(bool locked, string customLockedMessage = null)
        {
            isLocked = locked;
            if (!string.IsNullOrEmpty(customLockedMessage))
            {
                lockedMessage = customLockedMessage;
            }
        }
    }
}
