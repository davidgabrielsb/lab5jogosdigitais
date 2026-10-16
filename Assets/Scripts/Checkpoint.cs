using UnityEngine;
public class Checkpoint : MonoBehaviour
{
    bool active;
    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>(); if (player == null || active) return;
        active = true; player.SetCheckpoint(transform.position + Vector3.up * .7f);
        GameSession.Ensure().Notify("Marco de descanso ativado", 2);
        var sprite = GetComponent<SpriteRenderer>(); if (sprite != null) sprite.color = new Color(.5f,1,.8f);
        GameAudio.Play("seed");
    }
}
