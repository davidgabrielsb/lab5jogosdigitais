using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// As telas são prefabs editáveis. Este componente só liga os botões e atualiza os dados.
public class GameUI : MonoBehaviour
{
    Canvas canvas;
    Text stats,lives,progress,toast;
    Image progressFill;
    GameObject pause;
    GameSession game;
    void Start()
    {
        Application.runInBackground=true;game=GameSession.Ensure();
        string scene=SceneManager.GetActiveScene().name;
        var prefab=Resources.Load<GameObject>("Prefabs/UI/"+scene);
        if(prefab==null){Debug.LogError("Prefab da interface ausente: "+scene);return;}
        var root=Instantiate(prefab);root.name="Interface";canvas=root.GetComponent<Canvas>();
        if(FindFirstObjectByType<EventSystem>()==null) Instantiate(Resources.Load<GameObject>("Prefabs/Entrada"));
        foreach(var button in root.GetComponentsInChildren<Button>(true))
        {
            button.onClick.RemoveAllListeners();
            if(button.name.StartsWith("COMEÇAR")||button.name.StartsWith("JOGAR NOVAMENTE"))button.onClick.AddListener(()=>game.NewGame());
            else if(button.name=="CONTINUAR")button.onClick.AddListener(()=>game.TogglePause());
            else if(button.name=="VOLTAR AO INÍCIO")button.onClick.AddListener(()=>game.Menu());
            else if(button.name.StartsWith("SOM:"))button.onClick.AddListener(ToggleSound);
        }
        if(scene=="Final")
        {
            TextOf("Eyebrow").text=game.Won?"JORNADA CONCLUÍDA":"A TRILHA PODE RECOMEÇAR";
            TextOf("Resultado").text=game.Won?"O bosque vive!":"O brilho ainda espera";
            TextOf("Descrição").text=game.Won?"Lia recuperou todas as sementes.\nA luz voltou a encontrar seu caminho.":"Suas vidas acabaram, mas toda boa aventura\nmerece uma nova tentativa.";
            TextOf("Números").text=$"{game.Score:0000} PONTOS      •      {game.TotalCollected}/16 SEMENTES";
        }
        else if(scene!="Inicio")
        {
            stats=TextOf("Pontos");lives=TextOf("Vidas");progress=TextOf("Sementes");toast=TextOf("Mensagem");
            foreach(var image in root.GetComponentsInChildren<Image>(true))
            {
                if(image.name=="Progresso")progressFill=image;
                if(image.name=="Pausa")pause=image.gameObject;
            }
            if(pause!=null)pause.SetActive(false);
        }
        UpdateSoundLabel();
    }
    Text TextOf(string name){foreach(var text in canvas.GetComponentsInChildren<Text>(true))if(text.name==name)return text;return null;}
    void ToggleSound(){GameAudio.Muted=!GameAudio.Muted;UpdateSoundLabel();}
    void UpdateSoundLabel(){foreach(var text in canvas.GetComponentsInChildren<Text>(true))if(text.text.StartsWith("SOM:"))text.text=GameAudio.Muted?"SOM: OFF":"SOM: ON";}
    void Update()
    {
        if(game==null||stats==null)return;
        stats.text=$"{game.Score:0000} PTS";lives.text=$"VIDAS  {game.Lives} / 3";progress.text=$"SEMENTES  {game.StageCollected}/{game.StageTarget}";
        progressFill.rectTransform.sizeDelta=new Vector2(180f*game.StageCollected/Mathf.Max(1,game.StageTarget),7);
        toast.text=Time.unscaledTime<game.ToastUntil?game.Toast:"";pause.SetActive(game.Paused);
    }
}
