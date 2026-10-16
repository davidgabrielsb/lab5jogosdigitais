using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// Teste de integração em Play Mode: usa os colliders, triggers e transições reais.
[InitializeOnLoad]
public static class BosquePlaytest
{
    static int step, seedsBefore;
    static double next, deadline;
    static Vector3 cameraBefore, parallaxBefore;
    static string report="";
    static BosquePlaytest() { EditorApplication.update+=Poll; }
    [MenuItem("Bosque/Testar fluxo completo")]
    public static void Run()
    {
        SessionState.SetBool("BosqueTesting",true); EditorApplication.isPaused=false; step=0; report="LUME — Teste de integração\n"; Debug.Log("BOSQUE_TEST_STARTED"); deadline=EditorApplication.timeSinceStartup+100;
        if(!EditorApplication.isPlaying) { EditorSceneManager.OpenScene("Assets/Scenes/Inicio.unity"); EditorApplication.isPlaying=true; }
        next=EditorApplication.timeSinceStartup+2;
    }
    static void Pass(string text) { report+="OK: "+text+"\n"; File.WriteAllText("Logs/BosqueValidation.txt", report); }
    static void Check(bool condition,string message) { if(!condition) throw new Exception(message); Pass(message); }
    static void Poll()
    {
        if(File.Exists("Assets/Editor/TestRequested.txt") && !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling) { File.Delete("Assets/Editor/TestRequested.txt"); Run(); return; }
        if(!SessionState.GetBool("BosqueTesting",false)||!EditorApplication.isPlaying||EditorApplication.isCompiling) return;
        EditorApplication.isPaused=false; EditorApplication.QueuePlayerLoopUpdate();
        if(EditorApplication.timeSinceStartup<next) return;
        File.WriteAllText("Logs/BosqueTestState.txt", "step="+step+" play="+EditorApplication.isPlaying+" time="+EditorApplication.timeSinceStartup+" scene="+SceneManager.GetActiveScene().name+" ui="+UnityEngine.Object.FindObjectsByType<GameUI>(FindObjectsSortMode.None).Length+" scale="+Time.timeScale);
        if(deadline==0) { deadline=EditorApplication.timeSinceStartup+100; next=EditorApplication.timeSinceStartup+2; }
        try
        {
            if(EditorApplication.timeSinceStartup>deadline) throw new Exception("Tempo limite do teste");
            var game=GameSession.Instance; var player=UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            string scene=SceneManager.GetActiveScene().name;
            switch(step)
            {
                case 0:
                    if(game==null)return;
                    Check(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Length>=2,"Tela inicial com botão e instruções"); ClickButton("COMEÇAR A JORNADA"); step++; Wait(.7); break;
                case 1:
                    if(player==null)return;
                    Check(scene=="Fase1" && game.StageTarget==8,"Fase 1 com 8 sementes");
                    Check(UnityEngine.Object.FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None).Length==3,"Inimigos da fase 1");
                    Check(UnityEngine.Object.FindObjectsByType<Ladder>(FindObjectsSortMode.None).Length>=2,"Escadas com triggers");
                    Check(player.IsGrounded,"Jogador apoiado no Tilemap");
                    cameraBefore=Camera.main.transform.position; parallaxBefore=UnityEngine.Object.FindObjectsByType<ParallaxLayer>(FindObjectsSortMode.None)[0].transform.position;
                    player.Body.position=new Vector2(12,-1.5f); step++; Wait(.6); break;
                case 2:
                    Check(Camera.main.transform.position.x>cameraBefore.x+2,"Câmera acompanha deslocamento");
                    Check(UnityEngine.Object.FindObjectsByType<ParallaxLayer>(FindObjectsSortMode.None)[0].transform.position!=parallaxBefore,"Fundo move com parallax");
                    game.TogglePause(); Check(Time.timeScale==0 && game.Paused,"Pausa congela o jogo");game.TogglePause();
                    player.Body.position=UnityEngine.Object.FindFirstObjectByType<Checkpoint>().transform.position; step++;Wait(.3);break;
                case 3:
                    Check(player.RespawnPoint.x>10,"Checkpoint registra retorno");
                    Pass("Vidas antes do contato: "+game.Lives); var enemy=UnityEngine.Object.FindFirstObjectByType<EnemyPatrol>(); Pass("Posição inimigo: "+enemy.transform.position);player.Body.position=enemy.transform.position+Vector3.up*.45f; step++;Wait(.2);break;
                case 4:
                    Check(game.Lives==2,"Contato com inimigo remove uma vida (vidas="+game.Lives+")");
                    Check(player.IsInvulnerable,"Invulnerabilidade temporária após dano");
                    Check(Mathf.Abs(player.transform.position.x-player.RespawnPoint.x)<1,"Dano retorna ao checkpoint");
                    var heal=UnityEngine.Object.FindObjectsByType<Seed>(FindObjectsSortMode.None).First(s=>s.healing);player.Body.position=heal.transform.position;step++;Wait(.3);break;
                case 5:
                    Check(game.Lives==3,"Vida extra recupera vida");step++;CollectNext(player);break;
                case 6:
                    if(scene=="Fase2"){Check(game.TotalCollected==8&&game.Score==800,"Coleta e pontuação preservadas entre cenas");step++;Wait(.5);}
                    else CollectNext(player);break;
                case 7:
                    Check(scene=="Fase2"&&game.StageTarget==8,"Fase 2 com 8 sementes");
                    Check(UnityEngine.Object.FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None).Length==4,"Dificuldade ampliada na fase 2");step++;CollectNext(player);break;
                case 8:
                    if(scene=="Final"){Check(game.Won&&game.TotalCollected==16&&game.Score==1600,"Vitória somente após todas as 16 sementes");step++;Wait(.5);}
                    else CollectNext(player);break;
                case 9:
                    Check(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Any(t=>t.text=="O bosque vive!"),"Tela final apresenta vitória");
                    var retry=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name.Contains("JOGAR NOVAMENTE"));ClickButton("JOGAR NOVAMENTE");step++;Wait(.5);break;
                case 10:
                    Check(scene=="Fase1"&&game.Lives==3&&game.Score==0&&game.TotalCollected==0,"Botão reinicia vidas, pontos e itens");
                    player.EditorControl=true; player.EditorJump=true; step=20;Wait(.25);break;
                case 20:
                    Check(player.Body.linearVelocity.y>1 && player.transform.position.y>-1.3f,"Salto usa física e sai do chão");
                    player.Body.position=new Vector2(.55f,-1.8f); player.Body.linearVelocity=Vector2.zero; player.EditorInput=Vector2.up;step=21;Wait(1.2);break;
                case 21:
                    Check(player.IsClimbing && player.transform.position.y>1.6f,"Escada permite alcançar plataforma alta com entrada vertical");
                    player.EditorInput=Vector2.left;step=22;Wait(.55);break;
                case 22:
                    Check(game.StageCollected>=1 && player.transform.position.y>1.4f,"Jogador sai da escada e coleta sementes da plataforma alta");
                    player.EditorInput=Vector2.zero;player.EditorControl=false;
                    player.Body.position=UnityEngine.Object.FindFirstObjectByType<EnemyPatrol>().transform.position+Vector3.up*.45f;step=11;Wait(2.1);break;
                case 11:
                    Check(game.Lives==2,"Primeiro dano na nova partida");player.Body.position=UnityEngine.Object.FindFirstObjectByType<EnemyPatrol>().transform.position+Vector3.up*.45f;step++;Wait(2.1);break;
                case 12:
                    Check(game.Lives==1,"Segundo dano na nova partida");player.Body.position=UnityEngine.Object.FindFirstObjectByType<EnemyPatrol>().transform.position+Vector3.up*.45f;step++;Wait(.5);break;
                case 13:
                    Check(scene=="Final"&&!game.Won&&game.Lives==0,"Derrota quando inimigos zeram as vidas");
                    Check(UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Any(t=>t.text=="O brilho ainda espera"),"Tela final apresenta derrota");
                    Finish(true);break;
            }
        }
        catch(Exception e) { report+="FALHOU: "+e.Message+"\n";Finish(false); }
    }
    static void ClickButton(string name)
    {
        Canvas.ForceUpdateCanvases();
        var button=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name.Contains(name));
        var rect=button.GetComponent<RectTransform>(); var corners=new Vector3[4];rect.GetWorldCorners(corners);
        var point=(corners[0]+corners[2])*.5f;
        var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,point),button=PointerEventData.InputButton.Left};
        var results=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,results);
        Check(results.Count>0&&results[0].gameObject.GetComponent<Button>()==button,"Botão recebe cliques: "+name);
        ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
    }
    static void CollectNext(PlayerController player)
    {
        var seed=UnityEngine.Object.FindObjectsByType<Seed>(FindObjectsSortMode.None).Where(s=>!s.healing).OrderBy(s=>s.transform.position.x).FirstOrDefault();
        if(seed!=null && player!=null) { player.Body.position=seed.transform.position; player.Body.linearVelocity=Vector2.zero; }
        Wait(seed==null?1.7:.17);
    }
    static void Wait(double seconds) { next=EditorApplication.timeSinceStartup+seconds; }
    static void Finish(bool success)
    {
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/BosqueValidation.txt",report);SessionState.SetBool("BosqueTesting",false);
        Debug.Log(success?"BOSQUE_TEST_OK\n"+report:"BOSQUE_TEST_FAILED\n"+report);EditorApplication.isPlaying=false;
    }
}
