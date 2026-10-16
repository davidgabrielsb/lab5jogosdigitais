using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
[InitializeOnLoad]
public static class BosqueLifeRegression
{
    static double next;static string report="";
    static BosqueLifeRegression(){EditorApplication.update+=Tick;}
    [MenuItem("Bosque/Testar vidas da segunda fase")]
    public static void Run(){SessionState.SetBool("LifeRegression",true);SessionState.SetInt("LifeStep",0);report="Fase 2 — regressão de vidas\n";next=EditorApplication.timeSinceStartup+1;if(!EditorApplication.isPlaying){EditorSceneManager.OpenScene("Assets/Scenes/Inicio.unity");EditorApplication.isPlaying=true;}}
    static void Log(string text){report+=text+"\n";File.WriteAllText("Logs/BosqueLifeRegression.txt",report);}
    static void Check(bool ok,string text){Log((ok?"OK: ":"FALHOU: ")+text);if(!ok)throw new Exception(text);}
    static void Fall(PlayerController player){player.Body.position=new Vector2(10,-10);player.transform.position=new Vector3(10,-10,0);for(int i=0;i<6;i++)player.SendMessage("Update");}
    static void Tick()
    {
        if(!SessionState.GetBool("LifeRegression",false)||!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
        EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.timeSinceStartup<next)return;
        try
        {
            var game=GameSession.Instance;var p=UnityEngine.Object.FindFirstObjectByType<PlayerController>();int step=SessionState.GetInt("LifeStep",0);
            switch(step)
            {
                case 0:if(game==null)return;game.NewGame();SceneManager.LoadScene("Fase2");break;
                case 1:
                    Check(game.Lives==3,"Fase 2 começa com 3 vidas");Log("Spawn: "+p.RespawnPoint+"; posição: "+p.transform.position);
                    Fall(p);Check(game.Lives==2,"Uma queda tira somente uma vida, mesmo com Updates antes do próximo passo de física (vidas="+game.Lives+")");break;
                case 2:
                    Check(p!=null&&game.Lives==2&&p.transform.position.y>-5,"Retorno inicial permanece seguro e não consome outras vidas");
                    var marker=UnityEngine.Object.FindFirstObjectByType<Checkpoint>();p.Body.position=marker.transform.position;break;
                case 3:
                    Check(p.RespawnPoint.x>10,"Checkpoint da segunda fase ativado");Fall(p);Check(game.Lives==1,"Queda após checkpoint tira uma vida");break;
                case 4:
                    Check(p!=null&&game.Lives==1&&p.transform.position.y>-5,"Retorno ao checkpoint permanece seguro");
                    game.NewGame();SceneManager.LoadScene("Fase2");break;
                case 5:
                    p.Body.position=UnityEngine.Object.FindFirstObjectByType<EnemyPatrol>().transform.position+Vector3.up*.45f;next=EditorApplication.timeSinceStartup+.3;SessionState.SetInt("LifeStep",6);return;
                case 6:
                    Check(game.Lives==2,"Contato com inimigo na fase 2 tira somente uma vida");p.Hurt();Check(game.Lives==2,"Contatos repetidos respeitam a proteção temporária");break;
                case 7:
                    Check(game.Lives==2&&SceneManager.GetActiveScene().name=="Fase2","Partida continua na fase 2 com duas vidas");Log("BOSQUE_LIVES_OK");Finish();return;
            }
            SessionState.SetInt("LifeStep",step+1);next=EditorApplication.timeSinceStartup+2.2;
        }
        catch(Exception e){Log("Detalhe: "+e.Message);Finish();}
    }
    static void Finish(){SessionState.SetBool("LifeRegression",false);Debug.Log(report);EditorApplication.isPlaying=false;}
}
