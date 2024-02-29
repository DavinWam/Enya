using UnityEngine;

public class EnemyRoam : MonoBehaviour
{
    public Vector3 boundsCenter;
    public Vector3 boundsSize;

    public float roamSpeed = 1.0f;
    public float changeDirectionDelay = 2.0f;
    private Vector3 roamDirection;
    private float lastDirectionChangeTime;

    private void Start()
    {
        lastDirectionChangeTime = Time.time;
        ChangeRoamDirection();
    }

    private void Update()
    {
        if (Time.time - lastDirectionChangeTime > changeDirectionDelay)
        {
            ChangeRoamDirection();
            lastDirectionChangeTime = Time.time;
        }

        Vector3 nextPosition = transform.position + roamDirection * roamSpeed * Time.deltaTime;
        if (IsWithinBounds(nextPosition))
        {
            transform.position = nextPosition;
        }
        else
        {
            ChangeRoamDirection(); // Change direction if next position is out of bounds
        }
        if (IsOutsideBounds(transform.position, 2.0f))
        {
            TeleportBackInside();
        }
    }

    private bool IsWithinBounds(Vector3 position)
    {
        // Check if the position is within the roaming bounds
        Vector3 relativePos = position - boundsCenter;
        return Mathf.Abs(relativePos.x) <= boundsSize.x / 2 &&
               Mathf.Abs(relativePos.y) <= boundsSize.y / 2 &&
               Mathf.Abs(relativePos.z) <= boundsSize.z / 2;
    }

    private void ChangeRoamDirection()
    {
        roamDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

        // Ensure the new direction does not take the enemy out of bounds
        if (!IsWithinBounds(transform.position + roamDirection * changeDirectionDelay * roamSpeed))
        {
            roamDirection = -roamDirection; // Reverse direction if it would go out of bounds
        }
    }

    public void SetBounds(Vector3 center, Vector3 size)
    {
        boundsCenter = center;
        boundsSize = size;
    }
        private bool IsOutsideBounds(Vector3 position, float threshold)
    {
        // Check if the position is outside the roaming bounds with a given threshold
        Vector3 relativePos = position - boundsCenter;
        return Mathf.Abs(relativePos.x) > boundsSize.x / 2 + threshold ||
               Mathf.Abs(relativePos.y) > boundsSize.y / 2 + threshold ||
               Mathf.Abs(relativePos.z) > boundsSize.z / 2 + threshold;
    }

    private void TeleportBackInside()
    {
        // Teleport the enemy to a random position within the bounds

        transform.position = boundsCenter;
    }

}
