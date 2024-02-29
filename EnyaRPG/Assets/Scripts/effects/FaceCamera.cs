using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main; // Assuming the main camera is the one you want the sprite to face
    }

   void Update()
    {
        if (mainCamera != null)
        {
            // Look at the camera along the y-axis only
            Vector3 targetPosition = new Vector3(mainCamera.transform.position.x,
                                                 transform.position.y,
                                                 mainCamera.transform.position.z);
            transform.LookAt(targetPosition);

            // Adjust rotation by 180 degrees on the y-axis
            transform.eulerAngles = new Vector3(45,
                                                transform.eulerAngles.y + 180,
                                                transform.eulerAngles.z);
        }
    }
}
