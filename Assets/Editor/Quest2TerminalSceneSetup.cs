using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GameDevelopmentProject.Quest;

namespace GameDevelopmentProject.Editor
{
    public static class Quest2TerminalSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/TestScene.unity";
        private const string FontGuid = "8f586378b4e144a9851e7b34d9b748ee";
        private const string MaterialGuid = "71942363c5608a64bb62421349d39c9c";

        [MenuItem("Quest 2/Build Terminal Prototype")]
        public static void BuildQuest2TerminalPrototype()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject gameplaySystems = FindRootObject(scene, "GameplaySystems");
            GameObject player = FindRootObject(scene, "Player");
            GameObject interactionCanvas = FindRootObject(scene, "InteractionCanvas");

            if (gameplaySystems == null || player == null || interactionCanvas == null)
            {
                throw new System.InvalidOperationException(
                    "Quest 2 terminal setup requires GameplaySystems, Player, and InteractionCanvas.");
            }

            Quest2ObjectiveTracker tracker =
                gameplaySystems.GetComponent<Quest2ObjectiveTracker>();
            GameplayEventHub eventHub = gameplaySystems.GetComponent<GameplayEventHub>();
            PlayerInputScript playerInput = player.GetComponent<PlayerInputScript>();
            InteractionController interactionController =
                player.GetComponent<InteractionController>();

            if (tracker == null || eventHub == null || playerInput == null ||
                interactionController == null)
            {
                throw new System.InvalidOperationException(
                    "Quest 2 terminal setup found missing required runtime components.");
            }

            VirtualTerminalCommandParser parser =
                GetOrAddComponent<VirtualTerminalCommandParser>(gameplaySystems);
            SetPrivateReference(parser, "eventHub", eventHub);
            SetPrivateReference(parser, "quest2Tracker", tracker);

            GameObject controllerObject = GetOrMoveToChild(
                scene,
                interactionCanvas,
                "Quest2TerminalUIController");
            VirtualTerminalUI terminalUI =
                GetOrAddComponent<VirtualTerminalUI>(controllerObject);
            controllerObject.SetActive(true);
            ConfigureController(controllerObject);

            GameObject panel = GetOrCreateChild(controllerObject, "Quest2TerminalPanel");
            ConfigurePanel(panel);
            panel.SetActive(false);

            TextMeshProUGUI outputText = GetOrCreateText(panel, "TerminalOutput");
            outputText.text = string.Empty;
            outputText.fontSize = 20;
            outputText.alignment = TextAlignmentOptions.TopLeft;
            SetRectTransform(outputText.rectTransform, new Vector2(0, 30), new Vector2(720, 300));

            TMP_InputField commandInput = GetOrCreateInput(panel, "TerminalCommandInput");
            SetRectTransform(commandInput.GetComponent<RectTransform>(),
                new Vector2(0, -180), new Vector2(720, 48));

            SetPrivateReference(terminalUI, "commandParser", parser);
            SetPrivateReference(terminalUI, "commandInput", commandInput);
            SetPrivateReference(terminalUI, "outputText", outputText);
            SetPrivateReference(terminalUI, "terminalPanel", panel);
            SetPrivateReference(terminalUI, "playerInputScript", playerInput);
            SetPrivateReference(terminalUI, "interactionController", interactionController);

            commandInput.onEndEdit.RemoveAllListeners();
            UnityEventTools.AddPersistentListener(
                commandInput.onEndEdit,
                terminalUI.SubmitCommandFromInput);

            GameObject desktop = GetOrCreateRoot(scene, "ITDesktop_Prototype");
            ConfigureDesktop(desktop);
            ITDesktopTerminalInteraction desktopInteraction =
                GetOrAddComponent<ITDesktopTerminalInteraction>(desktop);
            SetPrivateReference(desktopInteraction, "quest2Tracker", tracker);
            SetPrivateReference(desktopInteraction, "terminalUI", terminalUI);

            EnsureEventSystem(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Quest 2 terminal prototype scene setup completed.");
        }

        private static GameObject FindRootObject(Scene scene, string objectName)
        {
            return scene.GetRootGameObjects().FirstOrDefault(root => root.name == objectName);
        }

        private static GameObject GetOrCreateRoot(Scene scene, string objectName)
        {
            GameObject existing = FindRootObject(scene, objectName);
            if (existing != null)
            {
                return existing;
            }

            GameObject created = new GameObject(objectName, typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(created, scene);
            return created;
        }

        private static GameObject GetOrCreateChild(GameObject parent, string objectName)
        {
            Transform child = parent.transform.Find(objectName);
            if (child != null)
            {
                return child.gameObject;
            }

            GameObject created = new GameObject(objectName);
            created.transform.SetParent(parent.transform, false);
            return created;
        }

        private static GameObject GetOrMoveToChild(
            Scene scene,
            GameObject parent,
            string objectName)
        {
            Transform child = parent.transform.Find(objectName);
            if (child != null)
            {
                return child.gameObject;
            }

            GameObject rootObject = FindRootObject(scene, objectName);
            if (rootObject != null)
            {
                rootObject.transform.SetParent(parent.transform, false);
                return rootObject;
            }

            return GetOrCreateChild(parent, objectName);
        }

        private static void ConfigureController(GameObject controller)
        {
            RectTransform rectTransform = GetOrAddComponent<RectTransform>(controller);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static void ConfigurePanel(GameObject panel)
        {
            RectTransform rectTransform = GetOrAddComponent<RectTransform>(panel);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(800, 500);
            rectTransform.anchoredPosition = Vector2.zero;

            Image image = GetOrAddComponent<Image>(panel);
            image.color = new Color(0.02f, 0.04f, 0.03f, 0.96f);
        }

        private static TextMeshProUGUI GetOrCreateText(GameObject parent, string objectName)
        {
            GameObject textObject = GetOrCreateChild(parent, objectName);
            TextMeshProUGUI text = GetOrAddComponent<TextMeshProUGUI>(textObject);
            TMP_FontAsset font = LoadAsset<TMP_FontAsset>(FontGuid);
            if (font != null)
            {
                text.font = font;
            }

            text.color = new Color(0.45f, 1f, 0.55f, 1f);
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        private static TMP_InputField GetOrCreateInput(GameObject parent, string objectName)
        {
            GameObject inputObject = GetOrCreateChild(parent, objectName);
            Image background = GetOrAddComponent<Image>(inputObject);
            background.color = new Color(0.01f, 0.02f, 0.015f, 1f);
            TMP_InputField input = GetOrAddComponent<TMP_InputField>(inputObject);
            TextMeshProUGUI text = GetOrCreateText(inputObject, "Text");
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            input.textComponent = text;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.placeholder = null;
            input.targetGraphic = background;
            return input;
        }

        private static void ConfigureDesktop(GameObject desktop)
        {
            desktop.transform.position = new Vector3(8.5f, 1f, 7.59f);
            desktop.transform.localScale = new Vector3(1.5f, 1f, 1f);
            BoxCollider collider = GetOrAddComponent<BoxCollider>(desktop);
            collider.size = Vector3.one;
            MeshFilter meshFilter = GetOrAddComponent<MeshFilter>(desktop);
            meshFilter.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            MeshRenderer renderer = GetOrAddComponent<MeshRenderer>(desktop);
            Material material = LoadAsset<Material>(MaterialGuid);
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void SetRectTransform(
            RectTransform rectTransform,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
        }

        private static void EnsureEventSystem(Scene scene)
        {
            if (scene.GetRootGameObjects().Any(root =>
                    root.GetComponent<EventSystem>() != null))
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            SceneManager.MoveGameObjectToScene(eventSystem, scene);
        }

        private static T LoadAsset<T>(string guid) where T : Object
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path)
                ? null
                : AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private static void SetPrivateReference(
            Component component,
            string fieldName,
            Object value)
        {
            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                throw new System.InvalidOperationException(
                    $"Serialized field '{fieldName}' was not found on {component.GetType().Name}.");
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
