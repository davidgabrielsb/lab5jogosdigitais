using UnityEngine;
public class Seed : MonoBehaviour
{
    public bool healing;
    Vector3 origin;
    bool collected;
    void Start() { origin = transform.position; }
    void Update() { transform.position = origin + Vector3.up * (Mathf.Sin(Time.time * 2.8f + origin.x) * .09f); }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || other.GetComponent<PlayerController>() == null) return;
        collected = true;
        if (healing) GameSession.Ensure().Heal(); else GameSession.Ensure().Collect();
        SparkBurst.Emit(transform.position, healing ? new Color(1,.4f,.5f) : new Color(1,.83f,.3f)); Destroy(gameObject);
    }
}
