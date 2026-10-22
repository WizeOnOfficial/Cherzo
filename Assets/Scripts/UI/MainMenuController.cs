using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameDevelopmentProject.UI
{
    [DisallowMultipleComponent]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private string gameplaySceneName = "AurickAssets";

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("Canvas Panels (Optional - Fallback GUI available)")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject creditsPanel;

        // UI State
        private bool isOptionsOpen;
        private bool isCreditsOpen;
        private bool isLoadingScene;
        private float loadingProgress;

        // Settings State (Persisted via PlayerPrefs)
        private float masterVolume = 1.0f;
        private float mouseSensitivity = 1.0f;
        private int qualityLevel = 2;
        private bool isFullscreen = true;

        private void Awake()
        {
            Time.timeScale = 1f;
            UnlockCursor();
            LoadSettings();
            EnsureButtonBindings();
        }

        private void Start()
        {
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
            mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
            qualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
            isFullscreen = Screen.fullScreen;

            AudioListener.volume = masterVolume;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
            PlayerPrefs.SetInt("QualityLevel", qualityLevel);
            PlayerPrefs.Save();

            AudioListener.volume = masterVolume;
            QualitySettings.SetQualityLevel(qualityLevel, true);
        }

        private void EnsureButtonBindings()
        {
            if (playButton == null) playButton = FindButtonByName("PlayButton");
            if (optionsButton == null) optionsButton = FindButtonByName("OptionsButton");
            if (creditsButton == null) creditsButton = FindButtonByName("CreditsButton");
            if (quitButton == null) quitButton = FindButtonByName("QuitButton");

            if (playButton != null)
            {
                playButton.onClick.RemoveListener(OnPlayClicked);
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (optionsButton != null)
            {
                optionsButton.onClick.RemoveListener(OnOptionsClicked);
                optionsButton.onClick.AddListener(OnOptionsClicked);
            }

            if (creditsButton != null)
            {
                creditsButton.onClick.RemoveListener(OnCreditsClicked);
                creditsButton.onClick.AddListener(OnCreditsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitClicked);
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private Button FindButtonByName(string buttonName)
        {
            var tr = transform.Find(buttonName);
            if (tr != null && tr.TryGetComponent<Button>(out var btn))
            {
                return btn;
            }

            var go = GameObject.Find(buttonName);
            return go != null ? go.GetComponent<Button>() : null;
        }

        public void OnPlayClicked()
        {
            if (isLoadingScene) return;
            StartCoroutine(LoadGameplaySceneAsync());
        }

        private IEnumerator LoadGameplaySceneAsync()
        {
            isLoadingScene = true;
            Time.timeScale = 1f;

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(gameplaySceneName);
            while (asyncOp != null && !asyncOp.isDone)
            {
                loadingProgress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                yield return null;
            }
        }

        public void OnOptionsClicked()
        {
            if (optionsPanel != null)
            {
                if (mainPanel != null) mainPanel.SetActive(false);
                optionsPanel.SetActive(true);
            }
            else
            {
                isOptionsOpen = true;
                isCreditsOpen = false;
            }
        }

        public void OnCreditsClicked()
        {
            if (creditsPanel != null)
            {
                if (mainPanel != null) mainPanel.SetActive(false);
                creditsPanel.SetActive(true);
            }
            else
            {
                isCreditsOpen = true;
                isOptionsOpen = false;
            }
        }

        public void CloseSubPanels()
        {
            isOptionsOpen = false;
            isCreditsOpen = false;

            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (mainPanel != null) mainPanel.SetActive(true);

            SaveSettings();
        }

        public void OnQuitClicked()
        {
            Debug.Log("[MainMenuController] Application quit requested.", this);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnGUI()
        {
            // Loading Overlay
            if (isLoadingScene)
            {
                DrawLoadingOverlay();
                return;
            }

            // Fallback Options Window
            if (isOptionsOpen && (optionsPanel == null || !optionsPanel.activeInHierarchy))
            {
                DrawOptionsDialog();
            }

            // Fallback Credits Window
            if (isCreditsOpen && (creditsPanel == null || !creditsPanel.activeInHierarchy))
            {
                DrawCreditsDialog();
            }
        }

        private void DrawLoadingOverlay()
        {
            float w = 380f;
            float h = 100f;
            var rect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(rect, GUIContent.none);

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(rect.x + 10f, rect.y + 15f, w - 20f, 30f), $"LOADING FACILITY... {Mathf.RoundToInt(loadingProgress * 100f)}%", titleStyle);

            // Progress bar background
            GUI.Box(new Rect(rect.x + 20f, rect.y + 55f, w - 40f, 20f), GUIContent.none);
            // Progress bar fill
            if (loadingProgress > 0.01f)
            {
                GUI.Box(new Rect(rect.x + 22f, rect.y + 57f, (w - 44f) * loadingProgress, 16f), GUIContent.none);
            }
        }

        private void DrawOptionsDialog()
        {
            float w = 460f;
            float h = 360f;
            var rect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(rect, GUIContent.none);

            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = Color.green;
            GUI.Label(new Rect(rect.x + 20f, rect.y + 15f, w - 40f, 30f), "SYSTEM SETTINGS", headerStyle);

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            labelStyle.normal.textColor = Color.white;

            // Master Volume
            GUI.Label(new Rect(rect.x + 30f, rect.y + 60f, 150f, 25f), $"Master Volume: {Mathf.RoundToInt(masterVolume * 100f)}%", labelStyle);
            float newVol = GUI.HorizontalSlider(new Rect(rect.x + 200f, rect.y + 65f, 220f, 20f), masterVolume, 0f, 1f);
            if (Mathf.Abs(newVol - masterVolume) > 0.01f)
            {
                masterVolume = newVol;
                AudioListener.volume = masterVolume;
            }

            // Mouse Sensitivity
            GUI.Label(new Rect(rect.x + 30f, rect.y + 105f, 160f, 25f), $"Mouse Sensitivity: {mouseSensitivity:0.0}x", labelStyle);
            mouseSensitivity = GUI.HorizontalSlider(new Rect(rect.x + 200f, rect.y + 110f, 220f, 20f), mouseSensitivity, 0.2f, 3.0f);

            // Graphics Quality
            GUI.Label(new Rect(rect.x + 30f, rect.y + 150f, 150f, 25f), "Graphics Quality:", labelStyle);
            string[] qualityNames = QualitySettings.names;
            if (qualityNames != null && qualityNames.Length > 0)
            {
                int qCount = Mathf.Min(4, qualityNames.Length);
                float btnW = 220f / qCount;
                for (int i = 0; i < qCount; i++)
                {
                    bool isSelected = (qualityLevel == i);
                    if (GUI.Toggle(new Rect(rect.x + 200f + (i * btnW), rect.y + 150f, btnW, 25f), isSelected, qualityNames[i], "Button"))
                    {
                        if (qualityLevel != i)
                        {
                            qualityLevel = i;
                            QualitySettings.SetQualityLevel(qualityLevel, true);
                        }
                    }
                }
            }

            // Fullscreen Toggle
            GUI.Label(new Rect(rect.x + 30f, rect.y + 195f, 150f, 25f), "Fullscreen Mode:", labelStyle);
            bool newFullscreen = GUI.Toggle(new Rect(rect.x + 200f, rect.y + 195f, 120f, 25f), isFullscreen, isFullscreen ? "Enabled" : "Disabled");
            if (newFullscreen != isFullscreen)
            {
                isFullscreen = newFullscreen;
                Screen.fullScreen = isFullscreen;
            }

            // Back / Save Button
            if (GUI.Button(new Rect(rect.x + (w - 180f) / 2f, rect.y + h - 55f, 180f, 35f), "APPLY & CLOSE"))
            {
                CloseSubPanels();
            }
        }

        private void DrawCreditsDialog()
        {
            float w = 480f;
            float h = 340f;
            var rect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(rect, GUIContent.none);

            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(rect.x + 20f, rect.y + 20f, w - 40f, 30f), "FACILITY CONTROL: RECON", headerStyle);

            var contentStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            contentStyle.normal.textColor = Color.white;

            string creditsText =
                "IF581 Game Development - Midterm Project\n\n" +
                "Project Lead & Systems Architecture:\nWilson & Partner\n\n" +
                "Gameplay Engineering & Quest Design:\nDecoupled Event-Driven Architecture\n\n" +
                "Environment & Visuals:\nAurick Assets & Custom Shaders\n\n" +
                "Built with Unity 6 (URP)";

            GUI.Label(new Rect(rect.x + 25f, rect.y + 60f, w - 50f, 200f), creditsText, contentStyle);

            if (GUI.Button(new Rect(rect.x + (w - 150f) / 2f, rect.y + h - 55f, 150f, 35f), "BACK"))
            {
                CloseSubPanels();
            }
        }
    }
}
