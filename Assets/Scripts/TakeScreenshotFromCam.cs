
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
            cameraLens = GetComponent<Camera>();

        if (cameraLens == null)
        {
            Debug.LogError("No Camera component found!");
            return;
        }

        string directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        // Save the original camera settings.
        RenderTexture previousTarget = cameraLens.targetTexture;
        RenderTexture previousActive = RenderTexture.active;
        Color previousBackground = cameraLens.backgroundColor;
        CameraClearFlags previousClearFlags = cameraLens.clearFlags;

        RenderTexture rt = new RenderTexture(
            256, 256, 24, RenderTextureFormat.ARGB32
        );

        rt.antiAliasing = 1;
        rt.Create();

        Texture2D screenshot = new Texture2D(
            256, 256, TextureFormat.RGBA32, false
        );

        try
        {
            // Force the camera to clear to transparent.
            cameraLens.clearFlags = CameraClearFlags.SolidColor;
            cameraLens.backgroundColor = new Color(0, 0, 0, 0);
            cameraLens.targetTexture = rt;

            // Clear the target to transparent before rendering.
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);

            cameraLens.Render();

            // Read the RGBA pixels from the rendered image.
            RenderTexture.active = rt;

            screenshot.ReadPixels(
                new Rect(0, 0, 256, 256), 0, 0
            );

            screenshot.Apply();

            File.WriteAllBytes(fullPath, screenshot.EncodeToPNG());

            Debug.Log("Transparent PNG saved to: " + fullPath);
        }
        finally
        {
            // Restore the camera and render target.
            cameraLens.targetTexture = previousTarget;
            cameraLens.backgroundColor = previousBackground;
            cameraLens.clearFlags = previousClearFlags;
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
