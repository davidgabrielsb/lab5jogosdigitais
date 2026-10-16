using UnityEngine;
public class LevelController : MonoBehaviour
{
    public int number = 1;
    void Start() { GameSession.Ensure().BeginStage(FindObjectsByType<Seed>(FindObjectsSortMode.None).Length - System.Array.FindAll(FindObjectsByType<Seed>(FindObjectsSortMode.None), s => s.healing).Length); }
    void Update() { if (Input.GetKeyDown(KeyCode.Escape)) GameSession.Ensure().TogglePause(); }
}
