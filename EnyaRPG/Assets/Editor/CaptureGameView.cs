using System.IO;
using UnityEditor;
using UnityEngine;

public class CaptureGameView : MonoBehaviour
{
    [MenuItem("Window/Capture Game View Screenshot")]
    public static void CaptureScreenshot()
    {
        // Get the resolution of the Game View
        int width = Screen.width;
        int height = Screen.height;

        // Create a render texture with the same resolution
        RenderTexture renderTexture = new RenderTexture(width, height, 24);

        // Create a temporary camera to render all cameras into the render texture
        Camera[] cameras = Camera.allCameras;
        GameObject cameraContainer = new GameObject("CameraContainer");
        foreach (Camera camera in cameras)
        {
            camera.targetTexture = renderTexture;
        }

        // Render the scene into the render texture
        Camera mainCamera = Camera.main;
        mainCamera.targetTexture = renderTexture;
        mainCamera.Render();

        // Capture the screenshot from the render texture
        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        // Reset the cameras
        foreach (Camera camera in cameras)
        {
            camera.targetTexture = null;
        }
        mainCamera.targetTexture = null;

        // Destroy the temporary camera container
        DestroyImmediate(cameraContainer);

        // Release the render texture
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);

        // Convert the screenshot to bytes
        byte[] bytes = screenshot.EncodeToPNG();

        // Save the screenshot as a PNG file
        string fileName = "GameViewScreenshot_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
        string filePath = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Screenshot captured and saved as " + fileName);
    }
}
