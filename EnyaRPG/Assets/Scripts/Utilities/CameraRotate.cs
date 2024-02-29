
using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    private float x;
    private float y;
    public float verticalSensitivity = 1f;
    public float horizontalSensitivity = 1f;
    private Vector3 rotate;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        y = Input.GetAxis("Mouse X");
        x = Input.GetAxis("Mouse Y");
        rotate = new Vector3(x * horizontalSensitivity, (-1)* y * verticalSensitivity, 0);
        transform.eulerAngles = transform.eulerAngles - rotate;
    }
}