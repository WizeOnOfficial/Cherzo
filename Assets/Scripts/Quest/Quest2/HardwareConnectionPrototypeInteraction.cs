using DefaultNamespace;
using UnityEngine;

namespace GameDevelopmentProject.Quest
{
    public sealed class HardwareConnectionPrototypeInteraction : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameplayEventHub eventHub;
        [SerializeField] private Quest2ObjectiveTracker quest2Tracker;
        [SerializeField] private VirtualTerminalUI terminalUI;
        [SerializeField] private VirtualTerminalCommandParser commandParser;

        public bool IsAvailable { get; private set; }
        public bool HasConnectedHardware { get; private set; }

        public string InteractMsg
        {
            get
            {
                EnsureDependencies();

                if (!IsAvailable)
                {
                    if ((quest2Tracker != null && quest2Tracker.HasFoundItGuide) || HasGuideInInventory())
                    {
                        IsAvailable = true;
                    }
                    else
                    {
                        return "Connect hardware requires IT Guide";
                    }
                }

                if (!HasConnectedHardware)
                {
                    return "Press E to Connect Hardware";
                }

                if (terminalUI != null && terminalUI.IsOpen)
                {
                    return "Terminal is open";
                }

                return "Press E to Open Terminal";
            }
        }

        public void Interact(InteractionController interactionController)
        {
            EnsureDependencies();

            if (!IsAvailable)
            {
                if ((quest2Tracker != null && quest2Tracker.HasFoundItGuide) || HasGuideInInventory())
                {
                    IsAvailable = true;
                }
                else
                {
                    Debug.Log("Hardware connection is unavailable. Find the IT Guide first.", this);
                    if (eventHub != null)
                    {
                        eventHub.PublishInteractionCompleted(
                            QuestGameplayIds.HardwareConnection,
                            GameplayInteractionResult.InvalidState,
                            "IT Guide is required.");
                    }
                    return;
                }
            }

            if (!HasConnectedHardware)
            {
                HasConnectedHardware = true;
                Debug.Log("Access hardware connected to the server console successfully.", this);
                if (eventHub != null)
                {
                    eventHub.PublishInteractionCompleted(
                        QuestGameplayIds.HardwareConnection,
                        GameplayInteractionResult.Success,
                        "Access hardware connected.");
                }
                return;
            }

            // Hardware already connected: open the terminal to modify keycard permissions
            if (terminalUI == null)
            {
                terminalUI = GetComponent<VirtualTerminalUI>() ?? FindFirstObjectByType<VirtualTerminalUI>();
            }

            if (terminalUI != null)
            {
                terminalUI.Open();
            }
            else
            {
                Debug.LogWarning("VirtualTerminalUI component not found on Server or in scene.", this);
            }
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
                eventHub.ItemAdded += HandleItemAdded;
            }

            if ((quest2Tracker != null && quest2Tracker.HasFoundItGuide) || HasGuideInInventory())
            {
                IsAvailable = true;
            }
        }

        private void EnsureDependencies()
        {
            if (eventHub == null)
            {
                eventHub = GetComponent<GameplayEventHub>() ?? FindFirstObjectByType<GameplayEventHub>();
            }

            if (quest2Tracker == null)
            {
                quest2Tracker = GetComponent<Quest2ObjectiveTracker>() ?? FindFirstObjectByType<Quest2ObjectiveTracker>();
            }

            if (terminalUI == null)
            {
                terminalUI = GetComponent<VirtualTerminalUI>() ?? FindFirstObjectByType<VirtualTerminalUI>();
            }

            if (commandParser == null)
            {
                commandParser = GetComponent<VirtualTerminalCommandParser>() ?? FindFirstObjectByType<VirtualTerminalCommandParser>();
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

        private bool HasGuideInInventory()
        {
            var adapter = FindFirstObjectByType<InventoryCompatibilityAdapter>();
            return adapter != null && adapter.HasItem(QuestGameplayIds.GuideItem);
        }
    }
}
