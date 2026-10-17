using DefaultNamespace;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class ITDesktopTerminalInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private Quest2ObjectiveTracker quest2Tracker;
        [SerializeField] private VirtualTerminalUI terminalUI;

        public string InteractMsg
        {
            get
            {
                if (quest2Tracker == null || !quest2Tracker.HasConnectedHardware)
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
            if (quest2Tracker == null || !quest2Tracker.HasConnectedHardware)
            {
                Debug.Log("IT terminal is unavailable until hardware is connected.", this);
                return;
            }

            if (terminalUI == null)
            {
                Debug.LogError($"{nameof(ITDesktopTerminalInteraction)} requires a terminal UI reference.", this);
                return;
            }

            terminalUI.Open();
        }

        private void Awake()
        {
            if (quest2Tracker == null)
            {
                Debug.LogError(
                    $"{nameof(ITDesktopTerminalInteraction)} requires a Quest 2 tracker reference.",
                    this);
            }

            if (terminalUI == null)
            {
                Debug.LogError(
                    $"{nameof(ITDesktopTerminalInteraction)} requires a terminal UI reference.",
                    this);
            }
        }
    }
}
