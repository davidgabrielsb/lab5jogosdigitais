using UnityEngine;
[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    PlayerController player;Animator animator;
    void Awake(){player=GetComponentInParent<PlayerController>();animator=GetComponent<Animator>();}
    void Update(){if(player==null)return;animator.SetBool("Moving",Mathf.Abs(player.Body.linearVelocity.x)>.1f||player.IsClimbing);animator.SetBool("Airborne",!player.IsGrounded&&!player.IsClimbing);}
}
