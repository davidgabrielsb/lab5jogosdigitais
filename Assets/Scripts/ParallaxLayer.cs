using UnityEngine;
public class ParallaxLayer : MonoBehaviour
{
    public float factor = .5f;
    Vector3 origin, cameraOrigin;
    Transform cam;
    void Start() { origin = transform.position; if (Camera.main != null) { cam = Camera.main.transform; cameraOrigin = cam.position; } }
    void LateUpdate() { if (cam != null) transform.position = origin + new Vector3((cam.position.x-cameraOrigin.x)*factor, (cam.position.y-cameraOrigin.y)*factor*.25f, 0); }
}
