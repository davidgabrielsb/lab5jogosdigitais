using UnityEngine;

// Mantém a movimentação horizontal e o salto do protótipo, com física em FixedUpdate.
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    public float speed = 5.5f;
    public float jumpForce = 12f;
    public LayerMask groundMask = 1 << 8;
    Rigidbody2D rb;
    CapsuleCollider2D body;
    SpriteRenderer visual;
    float move, vertical, jumpBuffer, coyote, immuneUntil;
    int ladders;
    bool climbing;
    Vector3 spawn;
    public bool IsGrounded { get; private set; }
    public bool IsClimbing => climbing;
    public bool IsInvulnerable => Time.time < immuneUntil;
    #if UNITY_EDITOR
    // Controle exclusivo do teste de integração; não é incluído no jogo compilado.
    public bool EditorControl;
    public Vector2 EditorInput;
    public bool EditorJump;
#endif
    public Rigidbody2D Body => rb;
    public Vector3 RespawnPoint => spawn;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); body = GetComponent<CapsuleCollider2D>();
        visual = GetComponentInChildren<SpriteRenderer>(); spawn = transform.position;
        rb.freezeRotation = true; rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }
    void Update()
    {
        if (GameSession.Instance != null && GameSession.Instance.InputLocked) { move = 0; return; }
        move = Input.GetAxisRaw("Horizontal"); vertical = Input.GetAxisRaw("Vertical");
        bool jump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z);
#if UNITY_EDITOR
        if (EditorControl) { move = EditorInput.x; vertical = EditorInput.y; jump = EditorJump; EditorJump = false; }
#endif
        if (jump) jumpBuffer = .13f;
        jumpBuffer -= Time.deltaTime;
        if (ladders > 0 && Mathf.Abs(vertical) > .1f) climbing = true;
        if (ladders == 0) climbing = false;
        if (visual != null) { if (move != 0) visual.flipX = move < 0; visual.color = IsInvulnerable && Mathf.Sin(Time.time * 35) > 0 ? new Color(1,1,1,.3f) : Color.white; }
        if (transform.position.y < -9) Hurt(true);
    }
    void FixedUpdate()
    {
        if (GameSession.Instance != null && GameSession.Instance.InputLocked) { rb.linearVelocity = Vector2.zero; return; }
        Bounds b = body.bounds;
        IsGrounded = Physics2D.BoxCast(b.center, new Vector2(b.size.x * .75f, b.size.y * .9f), 0, Vector2.down, .12f, groundMask).collider != null;
        coyote = IsGrounded ? .12f : coyote - Time.fixedDeltaTime;
        rb.gravityScale = climbing ? 0 : 3;
        rb.linearVelocity = new Vector2(move * speed, climbing ? vertical * 4 : rb.linearVelocity.y);
        if (jumpBuffer > 0 && (coyote > 0 || climbing))
        {
            rb.linearVelocity = new Vector2(move * speed, jumpForce); climbing = false; rb.gravityScale = 3;
            jumpBuffer = 0; coyote = 0; GameAudio.Play("jump");
        }
    }
    public void SetCheckpoint(Vector3 point) { spawn = point; }
    public void Hurt(bool falling = false)
    {
        var game = GameSession.Instance;
        if (game == null || game.InputLocked) return;
        if (IsInvulnerable)
        {
            if (falling) Respawn();
            return;
        }
        immuneUntil = Time.time + 1.8f;
        game.LoseLife(); GameAudio.Play("hurt");
        if (game.Lives > 0) Respawn();
    }
    void Respawn()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.gravityScale = 3;
        rb.position = spawn;
        // Update pode executar novamente antes do próximo FixedUpdate.
        // Atualiza também o Transform para não detectar outra queda no ponto antigo.
        transform.position = spawn;
        climbing = false; ladders = 0;
        coyote = jumpBuffer = 0;
        IsGrounded = false;
    }
    void OnTriggerEnter2D(Collider2D other) { if (other.GetComponent<Ladder>() != null) ladders++; }
    void OnTriggerExit2D(Collider2D other) { if (other.GetComponent<Ladder>() != null) ladders = Mathf.Max(0, ladders - 1); }
}
