using DefaultNamespace;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class ITDesktopTerminalInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private Quest2ObjectiveTracker quest2Tracker;
        [SerializeField] private VirtualTerminalUI terminalUI;

        private bool IsHardwareConnected()
        {
            if (quest2Tracker != null && quest2Tracker.HasConnectedHardware)
            {
                return true;
            }

            var hw = FindFirstObjectByType<HardwareConnectionPrototypeInteraction>();
            return hw != null && hw.HasConnectedHardware;
        }

        public string InteractMsg
        {
            get
            {
                if (quest2Tracker == null || terminalUI == null)
                {
                    EnsureDependencies();
                }

                if (!IsHardwareConnected())
                {
                    return "Connect the access hardware first";
                }

                return terminalUI != null && terminalUI.IsOpen
                    ? "Terminal is open"
                    : "Press E to Open IT Terminal";
            }
        }

        public void Interact(InteractionController interactionController)
        {
            if (quest2Tracker == null || terminalUI == null)
            {
                EnsureDependencies();
            }

            if (!IsHardwareConnected())
            {
                Debug.Log("IT terminal is unavailable until hardware is connected.", this);
                return;
            }

            if (terminalUI == null)
            {
                terminalUI = FindFirstObjectByType<VirtualTerminalUI>();
            }

            if (terminalUI != null)
            {
                terminalUI.Open();
            }
        }

        private void Awake()
        {
            EnsureDependencies();
        }

        private void EnsureDependencies()
        {
            if (quest2Tracker == null)
            {
                quest2Tracker = FindFirstObjectByType<Quest2ObjectiveTracker>();
            }

            if (terminalUI == null)
            {
                terminalUI = FindFirstObjectByType<VirtualTerminalUI>();
            }
        }
    }
}
