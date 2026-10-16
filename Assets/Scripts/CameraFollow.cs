using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float minX = 0, maxX = 36;
    Vector3 velocity;
    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = new Vector3(Mathf.Clamp(target.position.x + 1.1f, minX, maxX), Mathf.Clamp(target.position.y + 1.1f, .2f, 2f), -10);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, .18f);
    }
}
