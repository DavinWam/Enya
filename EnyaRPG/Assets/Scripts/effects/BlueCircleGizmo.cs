using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlueCircleGizmo : MonoBehaviour
{
    public int numberToShow = 2; // Example number to show

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        #if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.fontSize = 15; // Set the font size
        style.normal.textColor = Color.blue; // Set the text color

        Handles.Label(transform.position, numberToShow.ToString(), style);
        #endif
    }
}
