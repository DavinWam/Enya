using UnityEngine;

public class HitEffect : MonoBehaviour
{
    private CameraController cameraController;

    private void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("No CameraController script found on the main camera.");
        }
    }

    public void OnHit()
    {
        // Implement your hit effect logic here.

        // Trigger camera shake
        cameraController.TriggerShake();
    }
}
