using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
public static class BosqueBuilder
{
    const string Art="Assets/Resources/Art/";
    static Sprite[] hero, slime, seed, heart;
    static Material unlit;
    static BosqueBuilder() { EditorApplication.delayCall += AutoBuild; }
    static void AutoBuild() { if(File.Exists("Assets/Editor/BuildRequested.txt") && !EditorApplication.isPlayingOrWillChangePlaymode) Build(); }
    [MenuItem("Bosque/Construir jogo")]
    public static void Build()
    {
        try
        {
            Directory.CreateDirectory(Art); Directory.CreateDirectory("Assets/Scenes");
            // A arte é carregada dos arquivos PNG existentes no projeto.
            unlit=AssetDatabase.LoadAssetAtPath<Material>(Art+"PixelUnlit.mat"); if(unlit==null){unlit=new Material(Shader.Find("Sprites/Default")); AssetDatabase.CreateAsset(unlit,Art+"PixelUnlit.mat");}
            hero=Frames("lia",4); slime=Frames("limo",4); seed=Frames("semente",6); heart=Frames("vida",4);
            MakeStage(1); MakeStage(2); MakeScreen("Inicio"); MakeScreen("Final");
            EditorBuildSettings.scenes=new[]{"Inicio","Fase1","Fase2","Final"}.Select(n=>new EditorBuildSettingsScene("Assets/Scenes/"+n+".unity",true)).ToArray();
            PlayerSettings.productName="Lume — As Sementes do Bosque";
            PlayerSettings.defaultScreenWidth=1280; PlayerSettings.defaultScreenHeight=720; PlayerSettings.runInBackground=true;
            File.Delete("Assets/Editor/BuildRequested.txt"); AssetDatabase.Refresh();
            EditorSceneManager.OpenScene("Assets/Scenes/Inicio.unity");
            Debug.Log("BOSQUE_BUILD_OK: 4 cenas, 16 sementes, sprites e interface criados.");
        }
        catch(Exception e) { Debug.LogException(e); File.WriteAllText("BosqueBuildError.txt",e.ToString()); }
    }
    static Sprite[] Frames(string n,int count) => Enumerable.Range(0,count).Select(i=>AssetDatabase.LoadAssetAtPath<Sprite>(Art+n+i+".png")).ToArray();
    static SpriteRenderer SpriteObject(string name,Sprite sprite,Vector3 position,int order,Transform parent=null)
    {
        var go=new GameObject(name); if(parent!=null) go.transform.SetParent(parent,false); go.transform.position=position;
        var r=go.AddComponent<SpriteRenderer>(); r.sprite=sprite; r.sortingOrder=order; r.sharedMaterial=unlit; return r;
    }
    static void Animate(SpriteRenderer r,Sprite[] frames,float fps=8)
    {
        string name=frames[0].name.StartsWith("lia")?"Lia":frames[0].name.StartsWith("limo")?"Limo":frames[0].name.StartsWith("vida")?"Vida":"Semente";
        r.gameObject.AddComponent<Animator>().runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/"+name+".controller");
    }
    static void MakeStage(int number)
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        var ground=GameObject.Find("Tilemap").GetComponent<Tilemap>();
        var decoration=GameObject.Find("TilemapDecoracao").GetComponent<Tilemap>();
        ground.gameObject.layer=8;
        // Escadas e placas são decoração: não podem bloquear fisicamente o jogador.
        foreach(var pos in ground.cellBounds.allPositionsWithin)
        {
            var t=ground.GetTile<Tile>(pos);
            if(t!=null && t.sprite!=null && t.sprite.rect.y>=64)
            { decoration.SetTile(decoration.WorldToCell(ground.CellToWorld(pos)),t); ground.SetTile(pos,null); }
        }
        var grass=ground.GetTile(new Vector3Int(-4,-4,0));
        var soil=ground.GetTile(new Vector3Int(-4,-5,0));
        if(grass==null || soil==null) throw new Exception("Tiles originais de terreno não encontrados");
        var extra=NewMap("Terreno da trilha",ground.transform.parent);
        extra.transform.position=ground.transform.position;
        extra.gameObject.layer=8;
        var coll=extra.gameObject.AddComponent<TilemapCollider2D>();
        var rigid=extra.gameObject.AddComponent<Rigidbody2D>(); rigid.bodyType=RigidbodyType2D.Static;
        var composite=extra.gameObject.AddComponent<CompositeCollider2D>(); composite.geometryType=CompositeCollider2D.GeometryType.Polygons; coll.compositeOperation=Collider2D.CompositeOperation.Merge;
        var sprite=AssetDatabase.LoadAllAssetsAtPath("Assets/Tiles/tilesetOpenGame.png").OfType<Sprite>().First();
        if(number==1)
        {
            for(int x=7;x<=42;x++) if(x<18 || x>19) { for(int y=-6;y<=-4;y++) extra.SetTile(new Vector3Int(x,y,0),y==-4?grass:soil); }
            Shelf(extra,grass,11,14,-2); Shelf(extra,grass,23,26,-2); Shelf(extra,grass,29,32,0);
            LadderAt(new Vector2(.55f,-.9f),4.5f);
            LadderAt(new Vector2(23.55f,-1.2f),3.3f,true);
        }
        else
        {
            ground.ClearAllTiles(); decoration.ClearAllTiles();
            for(int x=-9;x<=45;x++) if((x<8 || x>9) && (x<27 || x>29)) for(int y=-6;y<=-4;y++) extra.SetTile(new Vector3Int(x,y,0),y==-4?grass:soil);
            Shelf(extra,grass,-3,0,-2); Shelf(extra,grass,3,5,0); Shelf(extra,grass,12,15,-2); Shelf(extra,grass,18,21,0); Shelf(extra,grass,25,26,-2); Shelf(extra,grass,32,35,-2); Shelf(extra,grass,38,41,0);
            LadderAt(new Vector2(3.55f,-.8f),4.5f,true);
            LadderAt(new Vector2(18.55f,-.8f),4.5f,true);
            LadderAt(new Vector2(38.55f,-.8f),4.5f,true);
        }
        var player=GameObject.Find("Player"); player.transform.position=new Vector3(-5,-1.8f,0);
        foreach(var old in player.GetComponentsInChildren<SpriteRenderer>()) UnityEngine.Object.DestroyImmediate(old.gameObject==player?old:old.gameObject);
        var view=SpriteObject("Lia — animação",hero[0],player.transform.position,10,player.transform); view.transform.localPosition=Vector3.zero;
        Animate(view,hero,9); view.GetComponent<Animator>().runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Lia-Jogador.controller");view.gameObject.AddComponent<PlayerAnimation>();
        var pc=player.GetComponent<PlayerController>(); pc.speed=5.5f; pc.jumpForce=12; pc.groundMask=1<<8;
        var capsule=player.GetComponent<CapsuleCollider2D>(); capsule.size=new Vector2(.48f,.94f); capsule.offset=new Vector2(0,-.03f);
        var friction=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(Art+"PlayerFriction"+number+".physicsMaterial2D"); if(friction==null){friction=new PhysicsMaterial2D("Sem atrito"){friction=0,bounciness=0}; AssetDatabase.CreateAsset(friction,Art+"PlayerFriction"+number+".physicsMaterial2D");} capsule.sharedMaterial=friction;
        var camera=Camera.main; camera.orthographicSize=5; camera.backgroundColor=number==1?new Color(.38f,.66f,.72f):new Color(.18f,.31f,.46f);
        camera.transform.position=new Vector3(0,.2f,-10); var follow=camera.gameObject.AddComponent<CameraFollow>(); follow.target=player.transform; follow.maxX=number==1?35:39;
        Background(number);
        Vector2[] positions=number==1?new[]{new Vector2(-4.2f,-2.05f),new Vector2(-2.5f,1.95f),new Vector2(-.8f,1.95f),new Vector2(3,-.05f),new Vector2(10,-2.05f),new Vector2(13.5f,-.05f),new Vector2(25.5f,-.05f),new Vector2(31.5f,1.95f)}:
            new[]{new Vector2(-4.2f,-2.05f),new Vector2(-.5f,-.05f),new Vector2(5.5f,1.95f),new Vector2(14.5f,-.05f),new Vector2(20.5f,1.95f),new Vector2(26.5f,-.05f),new Vector2(34.5f,-.05f),new Vector2(40.5f,1.95f)};
        foreach(var pos in positions) Collectible(pos,false);
        Collectible(new Vector2(number==1?22:31,-2.05f),true);
        Enemy(9,-2.4f,8.3f,11.5f,number); Enemy(number==1?16:23,-2.4f,number==1?15.5f:22.5f,number==1?18.6f:26.5f,number);
        Enemy(number==1?28:36,-2.4f,number==1?27.5f:36,number==1?33:40,number);
        if(number==2) Enemy(1,-2.4f,-1,3,number);
        var checkpoint=SpriteObject("Marco de descanso",AssetDatabase.LoadAssetAtPath<Sprite>(Art+"marco.png"),new Vector3(number==1?22.8f:18,-2.28f,0),7);
        var trigger=checkpoint.gameObject.AddComponent<BoxCollider2D>(); trigger.isTrigger=true; trigger.size=new Vector2(.7f,1.3f); checkpoint.gameObject.AddComponent<Checkpoint>();
        // Árvores e flores reutilizam o atlas da aula.
        for(int i=0;i<8;i++)
        {
            float x=8+i*4.6f;
            var tree=SpriteObject("Árvore",Crop("arvore",new Rect(210,62,100,112),64),new Vector3(x,-2.83f+ .86f,0),-1);
            if(number==2) tree.color=new Color(.64f,.78f,.9f);
        }
        var leftWall=new GameObject("Limite inicial"); leftWall.transform.position=new Vector3(-8.2f,0,0); leftWall.layer=8; leftWall.AddComponent<BoxCollider2D>().size=new Vector2(.3f,20);
        var rightWall=new GameObject("Limite final"); rightWall.transform.position=new Vector3(number==1?43:46,0,0); rightWall.layer=8; rightWall.AddComponent<BoxCollider2D>().size=new Vector2(.3f,20);
        var root=new GameObject("Regras e interface"); root.AddComponent<LevelController>().number=number; root.AddComponent<GameUI>();
        ground.RefreshAllTiles(); extra.RefreshAllTiles();
        ground.GetComponent<TilemapCollider2D>().ProcessTilemapChanges(); coll.ProcessTilemapChanges();
        ground.GetComponent<CompositeCollider2D>().GenerateGeometry(); composite.GenerateGeometry();
        Physics2D.SyncTransforms();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Fase"+number+".unity");
    }
    static Tilemap NewMap(string name,Transform parent)
    {
        var go=new GameObject(name,typeof(Tilemap),typeof(TilemapRenderer)); go.transform.SetParent(parent,false);
        go.GetComponent<TilemapRenderer>().sharedMaterial=unlit; return go.GetComponent<Tilemap>();
    }
    static void Shelf(Tilemap m,TileBase t,int start,int end,int y) { for(int x=start;x<=end;x++) m.SetTile(new Vector3Int(x,y,0),t); }
    static Sprite Crop(string name,Rect rect,float ppu)
    {
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+name+".asset");
        if(sprite==null)throw new Exception("Asset de sprite ausente: "+name);
        return sprite;
    }
    static void LadderAt(Vector2 pos,float height,bool draw=false)
    {
        var go=new GameObject("Escada funcional"); go.transform.position=pos; var c=go.AddComponent<BoxCollider2D>(); c.size=new Vector2(.9f,height); c.isTrigger=true; go.AddComponent<Ladder>();
        if(draw)
        {
            var sprite=Crop("escada",new Rect(0,96,32,32),32);
            for(float y=-height*.5f;y<height*.5f;y+=1) SpriteObject("Degraus",sprite,new Vector3(pos.x,pos.y+y+.5f,0),3,go.transform);
        }
    }
    static void Collectible(Vector2 pos,bool heal)
    {
        var r=SpriteObject(heal?"Vida extra":"Semente luminosa",heal?heart[0]:seed[0],pos,6); Animate(r,heal?heart:seed,heal?5:9);
        r.gameObject.AddComponent<CircleCollider2D>().isTrigger=true; r.GetComponent<CircleCollider2D>().radius=.32f; r.gameObject.AddComponent<Seed>().healing=heal;
    }
    static void Enemy(float x,float y,float left,float right,int stage)
    {
        var r=SpriteObject("Limo da sombra",slime[0],new Vector3(x,y,0),8); Animate(r,slime,6);
        r.color=stage==1?Color.white:new Color(.78f,.8f,1);
        var rb=r.gameObject.AddComponent<Rigidbody2D>(); rb.gravityScale=3; rb.freezeRotation=true; rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
        var body=r.gameObject.AddComponent<CapsuleCollider2D>(); body.direction=CapsuleDirection2D.Horizontal; body.size=new Vector2(.8f,.44f);
        var e=r.gameObject.AddComponent<EnemyPatrol>(); e.left=left; e.right=right; e.speed=stage==1?1.15f:1.6f;
    }
    static void Background(int stage)
    {
        var sky=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"ceu.png");
        for(int i=-2;i<8;i++) { var s=SpriteObject("Nuvens distantes",sky,new Vector3(i*16,3.5f,0),-30); s.transform.localScale=new Vector3(1.65f,1.65f,1); s.color=stage==1?Color.white:new Color(.55f,.65f,.85f); s.gameObject.AddComponent<ParallaxLayer>().factor=.88f; }
        for(int layer=0;layer<2;layer++) for(int i=-2;i<8;i++)
        {
            var s=SpriteObject(layer==0?"Colinas distantes":"Colinas próximas",AssetDatabase.LoadAssetAtPath<Sprite>(Art+"colinas"+layer+".png"),new Vector3(i*16,layer==0?-.8f:-2f,0),-20+layer);
            s.color=stage==1?Color.white:new Color(.5f,.65f,.88f); s.gameObject.AddComponent<ParallaxLayer>().factor=layer==0?.65f:.35f;
        }
    }
    static void MakeScreen(string name)
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var cam=new GameObject("Main Camera").AddComponent<Camera>(); cam.tag="MainCamera"; cam.orthographic=true; cam.orthographicSize=5; cam.transform.position=new Vector3(0,1,-10); cam.backgroundColor=new Color(.35f,.6f,.66f); cam.gameObject.AddComponent<AudioListener>();
        Background(1);
        var grid=new GameObject("Cenário",typeof(Grid)); var map=NewMap("Bosque",grid.transform);
        // Atlas original, com tiles de grama e terra já configurados no projeto.
        var original=AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles/tilesetOpenGame_29.asset");
        // O menu usa somente elementos visuais, sem física.
        for(int i=-12;i<=12;i++) { var tree=SpriteObject("Árvore",Crop("arvore",new Rect(210,62,100,112),64),new Vector3(i*2.3f,-2.5f,0),-2); tree.color=new Color(.7f,.8f,.75f); }
        var lia=SpriteObject("Lia",hero[0],new Vector3(3.4f,-2.15f,0),5); lia.transform.localScale=Vector3.one*2; Animate(lia,hero,5);
        new GameObject("Interface").AddComponent<GameUI>(); EditorSceneManager.SaveScene(scene,"Assets/Scenes/"+name+".unity");
    }
}
