using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameDevelopmentProject.Quest
{
    public sealed class VirtualTerminalUI : MonoBehaviour
    {
        [SerializeField] private VirtualTerminalCommandParser commandParser;
        [SerializeField] private TMP_InputField commandInput;
        [SerializeField] private TextMeshProUGUI outputText;
        [SerializeField] private GameObject terminalPanel;
        [SerializeField] private PlayerInputScript playerInputScript;
        [SerializeField] private InteractionController interactionController;

        private bool isOpen;
        private bool playerInputWasEnabled;
        private bool interactionControllerWasEnabled;
        private string interactionPromptBeforeOpen;

        public bool IsOpen => isOpen;

        public void Open()
        {
            if (!HasRequiredReferences())
            {
                return;
            }

            if (isOpen)
            {
                commandInput.ActivateInputField();
                commandInput.Select();
                return;
            }

            isOpen = true;
            playerInputWasEnabled = playerInputScript.enabled;
            interactionControllerWasEnabled = interactionController.enabled;
            interactionPromptBeforeOpen = interactionController.InteractionText;
            terminalPanel.SetActive(true);
            playerInputScript.enabled = false;
            interactionController.enabled = false;
            interactionController.ClearInteractionText();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            outputText.text = "Virtual IT terminal ready.\nType 'help' for commands.";
            commandInput.text = string.Empty;
            commandInput.ActivateInputField();
            commandInput.Select();
        }

        public void Close()
        {
            if (!isOpen)
            {
                return;
            }

            isOpen = false;
            terminalPanel.SetActive(false);
            playerInputScript.enabled = playerInputWasEnabled;
            interactionController.enabled = interactionControllerWasEnabled;
            if (interactionControllerWasEnabled)
            {
                interactionController.SetInteractionText(interactionPromptBeforeOpen);
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SubmitCommand()
        {
            if (!isOpen || commandParser == null || commandInput == null || outputText == null)
            {
                return;
            }

            string command = commandInput.text;
            bool commandSucceeded = commandParser.TryExecuteCommand(command, out string output);

            if (command == "clear" && commandSucceeded)
            {
                outputText.text = string.Empty;
            }
            else
            {
                string resultPrefix = commandSucceeded ? "$ " : "! ";
                outputText.text += $"\n{resultPrefix}{command}\n{output}";
            }

            commandInput.text = string.Empty;
            commandInput.ActivateInputField();
            commandInput.Select();
        }

        public void SubmitCommandFromInput(string _)
        {
            SubmitCommand();
        }

        private void Update()
        {
            if (isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
            }
        }

        private bool HasRequiredReferences()
        {
            if (commandParser == null ||
                commandInput == null ||
                outputText == null ||
                terminalPanel == null ||
                terminalPanel == gameObject ||
                playerInputScript == null ||
                interactionController == null)
            {
                Debug.LogError(
                    $"{nameof(VirtualTerminalUI)} requires parser, UI, player control references, " +
                    "and a terminal panel different from its controller GameObject.",
                    this);
                return false;
            }

            return true;
        }

        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                return;
            }

            terminalPanel.SetActive(false);
        }
    }
}
