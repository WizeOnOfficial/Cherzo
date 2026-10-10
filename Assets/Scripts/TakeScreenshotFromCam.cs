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

        string directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        RenderTexture rt = new RenderTexture(256, 256, 24);
        cameraLens.targetTexture = rt;
        Texture2D screenshot = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        cameraLens.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
        cameraLens.targetTexture = null;
        RenderTexture.active = null;

        if(Application.isEditor)
        {
            DestroyImmediate(rt);
        }
        else
        {
            Destroy(rt);
        }

        byte[] bytes = screenshot.EncodeToPNG();
        System.IO.File.WriteAllBytes(fullPath, bytes);
        #if UNITY_EDITOR
            AssetDatabase.Refresh();
        #endif
    }

    public string GetDestinationPath()
    {
        string baseFolder = folderPath;

        if (!Path.IsPathRooted(baseFolder))
        {
            baseFolder = Path.Combine(Application.dataPath, baseFolder);
        }

        string finalName = fileName;

        return Path.Combine(baseFolder, $"{finalName}.png");
    }
}
