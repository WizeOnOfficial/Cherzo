using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GameDevelopmentProject.Quest;

namespace GameDevelopmentProject.UI
{
    [DisallowMultipleComponent]
    public class GameplayHUDController : MonoBehaviour
    {
        public static GameplayHUDController Instance { get; private set; }

        [Header("Event Hub Reference")]
        [SerializeField] private GameplayEventHub eventHub;

        [Header("Countdown Timer (20 Minutes)")]
        [SerializeField] private float timeRemainingSeconds = 1200f; // 20 minutes
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Objective Banner (Atmospheric / Non-explicit)")]
        [SerializeField] private TextMeshProUGUI objectiveText;

        [Header("Win Screen")]
        [SerializeField] private GameObject winScreenPanel;
        [SerializeField] private TextMeshProUGUI winTimeRemainingText;
        [SerializeField] private Button winRestartButton;
        [SerializeField] private Button winMainMenuButton;

        [Header("Lose Screen (Time Expired)")]
        [SerializeField] private GameObject loseScreenPanel;
        [SerializeField] private Button loseRestartButton;
        [SerializeField] private Button loseMainMenuButton;

        private bool isGameOver;
        private bool isWinState;
        private string currentObjective = "Investigate the facility lobby for security clearance credentials.";
        private bool hasKeycard;
        private bool hasOpenedLobbyDoor;
        private bool hasFuse;
        private bool hasRepairedBreaker;
        private bool hasConnectedServer;
        private bool hasElevatedRole;
        private bool hasOpenedControlDoor;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Time.timeScale = 1f;

            if (eventHub == null)
            {
                eventHub = FindFirstObjectByType<GameplayEventHub>();
            }

            SetupButtons();
        }

        private void Start()
        {
            if (winScreenPanel != null) winScreenPanel.SetActive(false);
            if (loseScreenPanel != null) loseScreenPanel.SetActive(false);

            UpdateTimerDisplay();
            SetObjective("Investigate the facility lobby for security clearance credentials.");
        }

        private void Update()
        {
            if (isGameOver)
            {
                return;
            }

            timeRemainingSeconds -= Time.deltaTime;
            if (timeRemainingSeconds <= 0f)
            {
                timeRemainingSeconds = 0f;
                UpdateTimerDisplay();
                TriggerLoseCondition();
            }
            else
            {
                UpdateTimerDisplay();
            }
        }

        private void UpdateTimerDisplay()
        {
            if (timerText == null) return;

            int minutes = Mathf.FloorToInt(timeRemainingSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeRemainingSeconds % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Optional warning color under 3 minutes
            if (timeRemainingSeconds < 180f)
            {
                timerText.color = Color.red;
            }
        }

        public void SetObjective(string message)
        {
            currentObjective = message;
            if (objectiveText != null)
            {
                objectiveText.text = message;
            }
        }

        private void OnGUI()
        {
            if (timerText == null || !timerText.gameObject.activeInHierarchy)
            {
                int min = Mathf.FloorToInt(timeRemainingSeconds / 60f);
                int sec = Mathf.FloorToInt(timeRemainingSeconds % 60f);
                var timerStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                timerStyle.normal.textColor = timeRemainingSeconds < 180f ? Color.red : Color.cyan;
                GUI.Box(new Rect(Screen.width - 150f, 20f, 130f, 36f), $"TIME: {min:00}:{sec:00}", timerStyle);
            }

            if ((objectiveText == null || !objectiveText.gameObject.activeInHierarchy) && !string.IsNullOrWhiteSpace(currentObjective))
            {
                var objStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };
                objStyle.normal.textColor = Color.white;
                float w = Mathf.Min(650f, Screen.width - 40f);
                GUI.Box(new Rect((Screen.width - w) / 2f, 20f, w, 44f), $"OBJECTIVE: {currentObjective}", objStyle);
            }

            if (isGameOver)
            {
                if (isWinState && (winScreenPanel == null || !winScreenPanel.activeInHierarchy))
                {
                    DrawModal("FACILITY ACCESS RESTORED", "Security clearances verified. Control Room reached!", true);
                }
                else if (!isWinState && (loseScreenPanel == null || !loseScreenPanel.activeInHierarchy))
                {
                    DrawModal("LOCKDOWN INITIATED", "Facility timer expired. Security protocols active.", false);
                }
            }
        }

        private void DrawModal(string title, string subtitle, bool isWin)
        {
            float w = 420f;
            float h = 230f;
            var rect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(rect, GUIContent.none);

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = isWin ? Color.green : Color.red;
            GUI.Label(new Rect(rect.x + 20f, rect.y + 20f, w - 40f, 35f), title, titleStyle);

            var subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            subStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 20f, rect.y + 60f, w - 40f, 55f), subtitle, subStyle);

            if (GUI.Button(new Rect(rect.x + 40f, rect.y + 135f, 150f, 40f), isWin ? "PLAY AGAIN" : "TRY AGAIN"))
            {
                RestartGame();
            }

            if (GUI.Button(new Rect(rect.x + 230f, rect.y + 135f, 150f, 40f), "MAIN MENU"))
            {
                ReturnToMainMenu();
            }
        }

        private void OnEnable()
        {
            if (eventHub != null)
            {
                eventHub.ItemAdded += HandleItemAdded;
                eventHub.AccessResolved += HandleAccessResolved;
                eventHub.InteractionCompleted += HandleInteractionCompleted;
                eventHub.AccessRoleModified += HandleAccessRoleModified;
                eventHub.QuestObjectiveCompleted += HandleQuestObjectiveCompleted;
            }
        }

        private void OnDisable()
        {
            if (eventHub != null)
            {
                eventHub.ItemAdded -= HandleItemAdded;
                eventHub.AccessResolved -= HandleAccessResolved;
                eventHub.InteractionCompleted -= HandleInteractionCompleted;
                eventHub.AccessRoleModified -= HandleAccessRoleModified;
                eventHub.QuestObjectiveCompleted -= HandleQuestObjectiveCompleted;
            }
        }

        private void HandleQuestObjectiveCompleted(QuestObjectiveEvent objectiveEvent)
        {
            if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest1FindIdCard)
            {
                SetObjective("Security clearance acquired. Authorize entry at the lobby scanner.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest1TryInitialControlPanelAccess)
            {
                SetObjective("Scanner error detected. Inspect the terminal panel for diagnostics.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest1InspectControlPanel)
            {
                SetObjective("Terminal locked. Check the surrounding casing for hidden maintenance overrides.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest1FindHiddenMessage)
            {
                SetObjective("Override instructions uncovered. Locate an external power fuse in the utility area.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest1FindExternalFuse)
            {
                SetObjective("External fuse retrieved. Return to the lobby scanner and install the power unit.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest1InstallExternalFuse)
            {
                SetObjective("Auxiliary power established. Scan authorized ID Card to open the corridor blast door.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest1ScanIdCardAfterFuse)
            {
                SetObjective("Access approved. Proceed through the blast doors into the Maintenance Wing.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest1EnterMaintenanceWing)
            {
                SetObjective("Maintenance Wing accessed. Proceed towards the Main Control Room.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest2RepairElectricalShort)
            {
                SetObjective("Facility power restored. Scan authorized ID Card at the Control Room scanner.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest2RoleDenied)
            {
                SetObjective("Security clearance insufficient. Search the IT wing for administrator guidelines.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest2FindItGuide)
            {
                SetObjective("IT guide recovered. Connect the hardware terminal module at the server rack.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest2ConnectHardware)
            {
                SetObjective("Hardware linked. Access server terminal to reconfigure security permissions.");
            }
            else if (objectiveEvent.ObjectiveId == QuestGameplayIds.Quest2ModifyAccessRole)
            {
                SetObjective("Credentials elevated. Scan Administrator ID Card at the Control Room scanner.");
            }
        }

        private void HandleItemAdded(ItemChangedEvent itemEvent)
        {
            if (itemEvent.ItemId == QuestGameplayIds.IdCardItem && !hasKeycard)
            {
                hasKeycard = true;
                if (!hasOpenedLobbyDoor)
                {
                    SetObjective("Authorize security clearance at the corridor gate.");
                }
            }
            else if (itemEvent.ItemId == QuestGameplayIds.ExternalFuseItem && !hasFuse)
            {
                hasFuse = true;
                if (!hasRepairedBreaker)
                {
                    SetObjective("Auxiliary fuse acquired. Restore power at the electrical breaker.");
                }
            }
        }

        private void HandleAccessResolved(AccessEvent accessEvent)
        {
            if (accessEvent.AccessPointId == QuestGameplayIds.MaintenanceWingAccess &&
                accessEvent.Result == GameplayInteractionResult.Success)
            {
                hasOpenedLobbyDoor = true;
                if (!hasRepairedBreaker)
                {
                    SetObjective("Corridor breached. Facility systems unresponsive; inspect electrical panel.");
                }
            }
            else if (accessEvent.AccessPointId == QuestGameplayIds.ControlRoom)
            {
                if (accessEvent.Result == GameplayInteractionResult.PowerUnavailable && !hasRepairedBreaker)
                {
                    SetObjective("Control room offline. Auxiliary electrical power must be restored.");
                }
                else if (accessEvent.Result == GameplayInteractionResult.Denied && !hasElevatedRole)
                {
                    SetObjective("Security clearance insufficient. Elevate credentials via IT server systems.");
                }
                else if (accessEvent.Result == GameplayInteractionResult.Success)
                {
                    hasOpenedControlDoor = true;
                    SetObjective("Sector 2 access authorized. Enter the Main Control Room.");
                }
            }
        }

        private void HandleInteractionCompleted(InteractionEvent interactionEvent)
        {
            if (interactionEvent.InteractionId == QuestGameplayIds.BreakerPanel &&
                interactionEvent.Result == GameplayInteractionResult.Success)
            {
                hasRepairedBreaker = true;
                if (!hasConnectedServer)
                {
                    SetObjective("Main power restored. Establish IT server terminal uplink.");
                }
            }
            else if (interactionEvent.InteractionId == QuestGameplayIds.HardwareConnection &&
                     interactionEvent.Result == GameplayInteractionResult.Success)
            {
                hasConnectedServer = true;
                if (!hasElevatedRole)
                {
                    SetObjective("Hardware linked. Access server terminal to reconfigure security permissions.");
                }
            }
        }

        private void HandleAccessRoleModified(AccessRoleModifiedEvent roleEvent)
        {
            if (roleEvent.AccessSubjectId == QuestGameplayIds.IdCardAccessSubject &&
                roleEvent.PermissionValue == QuestGameplayIds.ControlRoomAdministratorPermission)
            {
                hasElevatedRole = true;
                if (!hasOpenedControlDoor)
                {
                    SetObjective("Credentials elevated. Scan authorized ID Card at the Control Room scanner.");
                }
            }
        }

        public void TriggerWinCondition()
        {
            if (isGameOver) return;
            isGameOver = true;
            isWinState = true;

            int minutes = Mathf.FloorToInt(timeRemainingSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeRemainingSeconds % 60f);

            if (winTimeRemainingText != null)
            {
                winTimeRemainingText.text = $"Time Remaining: {minutes:00}:{seconds:00}";
            }

            if (winScreenPanel != null)
            {
                winScreenPanel.SetActive(true);
            }

            UnlockCursor();
        }

        public void TriggerLoseCondition()
        {
            if (isGameOver) return;
            isGameOver = true;
            isWinState = false;

            if (loseScreenPanel != null)
            {
                loseScreenPanel.SetActive(true);
            }

            UnlockCursor();
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void SetupButtons()
        {
            if (winRestartButton != null)
            {
                winRestartButton.onClick.AddListener(RestartGame);
            }

            if (winMainMenuButton != null)
            {
                winMainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }

            if (loseRestartButton != null)
            {
                loseRestartButton.onClick.AddListener(RestartGame);
            }

            if (loseMainMenuButton != null)
            {
                loseMainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenuScene");
        }
    }
}
