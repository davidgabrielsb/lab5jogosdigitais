using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }
    public int Lives { get; private set; } = 3;
    public int Score { get; private set; }
    public int TotalCollected { get; private set; }
    public int StageCollected { get; private set; }
    public int StageTarget { get; private set; } = 8;
    public bool Won { get; private set; }
    public bool Paused { get; private set; }
    public bool Transitioning { get; private set; }
    public bool InputLocked => Transitioning || Paused;
    public string Toast { get; private set; }
    public float ToastUntil { get; private set; }
    public static GameSession Ensure()
    {
        if (Instance == null) new GameObject("GameSession").AddComponent<GameSession>();
        return Instance;
    }
    void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; DontDestroyOnLoad(gameObject); gameObject.AddComponent<GameAudio>(); }
    void OnDestroy() { if (Instance == this) Instance = null; Time.timeScale = 1; }
    public void NewGame()
    {
        Time.timeScale = 1; Lives = 3; Score = TotalCollected = StageCollected = 0;
        Won = false; Paused = Transitioning = false; SceneManager.LoadScene("Fase1");
    }
    public void BeginStage(int count) { StageTarget = count; StageCollected = 0; Transitioning = Paused = false; Time.timeScale = 1; Notify(SceneManager.GetActiveScene().name == "Fase2" ? "Última trilha • recolha as 8 sementes restantes" : "Recolha 8 sementes • use W ou ↑ nas escadas", 4); }
    public void Collect()
    {
        if (Transitioning) return;
        StageCollected++; TotalCollected++; Score += 100; GameAudio.Play("seed");
        if (StageCollected >= StageTarget) StartCoroutine(CompleteStage());
    }
    IEnumerator CompleteStage()
    {
        Transitioning = true; GameAudio.Play("win");
        bool final = SceneManager.GetActiveScene().name == "Fase2";
        Notify(final ? "O bosque voltou a brilhar!" : "Trilha concluída! Rumo ao Vale Azul…", 3);
        yield return new WaitForSeconds(1.4f);
        if (final) { Won = true; SceneManager.LoadScene("Final"); }
        else SceneManager.LoadScene("Fase2");
    }
    public void LoseLife() { if (Transitioning) return; Lives = Mathf.Max(0, Lives - 1); Notify("Cuidado! Você perdeu uma vida.", 2); if (Lives == 0) { Won = false; Transitioning = true; SceneManager.LoadScene("Final"); } }
    public void Heal() { Lives = Mathf.Min(3, Lives + 1); Notify("Fôlego renovado • +1 vida", 2); GameAudio.Play("seed"); }
    public void Notify(string message, float seconds = 2) { Toast = message; ToastUntil = Time.unscaledTime + seconds; }
    public void TogglePause() { if (Transitioning) return; Paused = !Paused; Time.timeScale = Paused ? 0 : 1; }
    public void Menu() { Time.timeScale = 1; Paused = Transitioning = false; SceneManager.LoadScene("Inicio"); }
}
