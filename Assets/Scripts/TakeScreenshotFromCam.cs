
using UnityEditor;
using System.IO;
using UnityEngine;

public class TakeScreenshotFromCam : MonoBehaviour
{
    [SerializeField] private string folderPath = "Icons";
    [SerializeField] private string fileName = "IconName";

    private Camera cameraLens;

    [ContextMenu("Take Screenshot")]
    public void TakeScreenshotFromInspector()
    {
        string fullPath = GetDestinationPath();
        TakeScreenshot(fullPath);
    }

    void TakeScreenshot(string fullPath)
    {
        if (cameraLens == null)
        {
            cameraLens = GetComponent<Camera>();
        }

        if (cameraLens == null)
        {
            Debug.LogError("No Camera component found!");
            return;
        }

        string directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory) &&
            !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Save the original camera settings.
        RenderTexture previousTarget = cameraLens.targetTexture;
        RenderTexture previousActive = RenderTexture.active;
        Color previousBackground = cameraLens.backgroundColor;

        // Create a RenderTexture with an alpha channel.
        RenderTexture rt = new RenderTexture(
            256,
            256,
            24,
            RenderTextureFormat.ARGB32
        );

        rt.Create();

        Texture2D screenshot = new Texture2D(
            256,
            256,
            TextureFormat.RGBA32,
            false
        );

        try
        {
            // Make the camera background transparent.
            cameraLens.backgroundColor = new Color(
                previousBackground.r,
                previousBackground.g,
                previousBackground.b,
                0f
            );

            cameraLens.targetTexture = rt;
            cameraLens.Render();

            RenderTexture.active = rt;

            screenshot.ReadPixels(
                new Rect(0, 0, 256, 256),
                0,
                0
            );

            screenshot.Apply();

            byte[] bytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);

            Debug.Log("Screenshot saved to: " + fullPath);
        }
        finally
        {
            // Restore the original camera settings.
            cameraLens.targetTexture = previousTarget;
            cameraLens.backgroundColor = previousBackground;
            RenderTexture.active = previousActive;

            if (Application.isEditor)
            {
                DestroyImmediate(screenshot);
                DestroyImmediate(rt);
            }
            else
            {
                Destroy(screenshot);
                Destroy(rt);
            }
        }

        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
    }

    public string GetDestinationPath()
    {
        string baseFolder = folderPath;

        if (!Path.IsPathRooted(baseFolder))
        {
            baseFolder = Path.Combine(
                Application.dataPath,
                baseFolder
            );
        }

        return Path.Combine(baseFolder, $"{fileName}.png");
    }
}
