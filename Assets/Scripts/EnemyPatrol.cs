using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    public float left, right, speed = 1.4f;
    int direction = 1;
    Rigidbody2D rb;
    SpriteRenderer view;
    void Awake() { rb = GetComponent<Rigidbody2D>(); view = GetComponent<SpriteRenderer>(); }
    void FixedUpdate()
    {
        if (GameSession.Instance != null && GameSession.Instance.InputLocked) { rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); return; }
        if (rb.position.x >= right) direction = -1;
        if (rb.position.x <= left) direction = 1;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        view.flipX = direction < 0;
    }
    void OnCollisionEnter2D(Collision2D hit) { Touch(hit); }
    void OnCollisionStay2D(Collision2D hit) { Touch(hit); }
    void Touch(Collision2D hit) { var p = hit.gameObject.GetComponent<PlayerController>(); if (p != null) p.Hurt(); }
}
