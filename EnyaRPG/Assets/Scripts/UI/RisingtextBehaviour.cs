using UnityEngine;
using TMPro;

public class RisingTextBehaviour : MonoBehaviour
{
    public float riseSpeed = 1.0f;
    public float lifetime = 2.0f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);
    }
}
