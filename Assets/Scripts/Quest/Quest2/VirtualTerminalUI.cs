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

        // OnGUI fallback state
        private string guiTerminalOutput = "Virtual IT terminal ready.\nType 'help' for commands.\nType 'chmod u=rwx id_card' to update permissions.";
        private string guiCommandInput = string.Empty;
        private Vector2 guiScrollPos = Vector2.zero;

        public bool IsOpen => isOpen;

        public void Open()
        {
            EnsureDependencies();

            if (isOpen)
            {
                if (commandInput != null)
                {
                    commandInput.ActivateInputField();
                    commandInput.Select();
                }
                return;
            }

            isOpen = true;

            if (playerInputScript != null)
            {
                playerInputWasEnabled = playerInputScript.enabled;
                playerInputScript.enabled = false;
            }

            if (interactionController != null)
            {
                interactionControllerWasEnabled = interactionController.enabled;
                interactionPromptBeforeOpen = interactionController.InteractionText;
                interactionController.enabled = false;
                interactionController.ClearInteractionText();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (terminalPanel != null && terminalPanel != gameObject)
            {
                terminalPanel.SetActive(true);
            }

            if (outputText != null)
            {
                outputText.text = "Virtual IT terminal ready.\nType 'help' for commands.";
            }

            if (commandInput != null)
            {
                commandInput.text = string.Empty;
                commandInput.ActivateInputField();
                commandInput.Select();
            }
        }

        public void Close()
        {
            if (!isOpen)
            {
                return;
            }

            isOpen = false;

            if (terminalPanel != null && terminalPanel != gameObject)
            {
                terminalPanel.SetActive(false);
            }

            if (playerInputScript != null)
            {
                playerInputScript.enabled = playerInputWasEnabled;
            }

            if (interactionController != null)
            {
                interactionController.enabled = interactionControllerWasEnabled;
                if (interactionControllerWasEnabled)
                {
                    interactionController.SuppressInteraction(0.25f);
                    interactionController.SetInteractionText(interactionPromptBeforeOpen);
                }
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SubmitCommand()
        {
            if (!isOpen || commandParser == null)
            {
                return;
            }

            string command = commandInput != null ? commandInput.text : guiCommandInput;
            ExecuteCommand(command);

            if (commandInput != null)
            {
                commandInput.text = string.Empty;
                commandInput.ActivateInputField();
                commandInput.Select();
            }
        }

        public void SubmitCommandFromInput(string _)
        {
            SubmitCommand();
        }

        private void ExecuteCommand(string command)
        {
            if (commandParser == null)
            {
                EnsureDependencies();
            }

            if (commandParser == null)
            {
                return;
            }

            bool commandSucceeded = commandParser.TryExecuteCommand(command, out string output);

            if (command == "clear" && commandSucceeded)
            {
                if (outputText != null) outputText.text = string.Empty;
                guiTerminalOutput = string.Empty;
            }
            else
            {
                string resultPrefix = commandSucceeded ? "$ " : "! ";
                string newEntry = $"\n{resultPrefix}{command}\n{output}";
                if (outputText != null) outputText.text += newEntry;
                guiTerminalOutput += newEntry;
            }

            guiScrollPos.y = 99999f; // auto-scroll to bottom
        }

        private void Update()
        {
            if (!isOpen) return;

            bool escapePressed = false;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                escapePressed = true;
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                escapePressed = true;
            }

            if (escapePressed)
            {
                Close();
            }
        }

        private void OnGUI()
        {
            if (!isOpen) return;

            // Render fallback OnGUI terminal if TMP canvas panel is not configured or not active
            if (terminalPanel == null || !terminalPanel.activeInHierarchy || commandInput == null || outputText == null)
            {
                float w = Mathf.Min(740f, Screen.width - 40f);
                float h = Mathf.Min(500f, Screen.height - 40f);
                Rect winRect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

                GUI.Box(winRect, GUIContent.none);

                var headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
                headerStyle.normal.textColor = Color.green;
                GUI.Label(new Rect(winRect.x + 15f, winRect.y + 10f, w - 140f, 25f), "ACCESS CONTROL TERMINAL v2.4 (IT WORKSTATION)", headerStyle);

                if (GUI.Button(new Rect(winRect.x + w - 110f, winRect.y + 10f, 95f, 26f), "EXIT (ESC)"))
                {
                    Close();
                    return;
                }

                float logHeight = h - 90f;
                var logStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft,
                    wordWrap = true
                };
                logStyle.normal.textColor = new Color(0.2f, 1f, 0.4f);

                int lineCount = guiTerminalOutput.Split('\n').Length;
                float contentHeight = Mathf.Max(logHeight - 10f, lineCount * 22f + 20f);
                guiScrollPos = GUI.BeginScrollView(
                    new Rect(winRect.x + 15f, winRect.y + 40f, w - 30f, logHeight),
                    guiScrollPos,
                    new Rect(0, 0, w - 50f, contentHeight));
                GUI.Label(new Rect(5, 5, w - 60f, contentHeight), guiTerminalOutput, logStyle);
                GUI.EndScrollView();

                var promptStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                promptStyle.normal.textColor = Color.green;
                GUI.Label(new Rect(winRect.x + 15f, winRect.y + h - 38f, 25f, 26f), ">", promptStyle);

                GUI.SetNextControlName("TerminalInput");
                guiCommandInput = GUI.TextField(new Rect(winRect.x + 40f, winRect.y + h - 38f, w - 160f, 26f), guiCommandInput);

                bool shouldSubmit = GUI.Button(new Rect(winRect.x + w - 110f, winRect.y + h - 38f, 95f, 26f), "EXECUTE");

                if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && GUI.GetNameOfFocusedControl() == "TerminalInput")
                {
                    shouldSubmit = true;
                }

                if (shouldSubmit && !string.IsNullOrWhiteSpace(guiCommandInput))
                {
                    ExecuteCommand(guiCommandInput.Trim());
                    guiCommandInput = string.Empty;
                }

                GUI.FocusControl("TerminalInput");
            }
        }

        private void EnsureDependencies()
        {
            if (commandParser == null)
            {
                commandParser = GetComponent<VirtualTerminalCommandParser>() ?? FindFirstObjectByType<VirtualTerminalCommandParser>();
            }

            if (playerInputScript == null)
            {
                playerInputScript = FindFirstObjectByType<PlayerInputScript>();
            }

            if (interactionController == null)
            {
                interactionController = FindFirstObjectByType<InteractionController>();
            }
        }

        private void Awake()
        {
            EnsureDependencies();

            if (terminalPanel != null && terminalPanel != gameObject)
            {
                terminalPanel.SetActive(false);
            }
        }
    }
}
