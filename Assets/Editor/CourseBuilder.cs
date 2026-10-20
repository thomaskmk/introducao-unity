using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using ObstacleCourse;

public static class CourseBuilder
{
    public const string ScenePath = "Assets/Scenes/Percurso.unity";
    private static Sprite square;
    private static Material spriteMaterial;
    private static Font font;
    private static readonly Color Ink = Hex("101C2B");
    private static readonly Color Muted = Hex("A5B7C7");
    private static readonly Color Mint = Hex("68E6C1");
    private static readonly Color Orange = Hex("FF855F");

    [MenuItem("Percurso/Reconstruir as duas fases (substitui Percurso)")]
    public static void Generate()
    {
        Directory.CreateDirectory("Assets/Art/Prototype");
        var texture = new Texture2D(16,16);
        var colors = new Color[256]; for (int i=0; i<colors.Length; i++) colors[i]=Color.white;
        texture.SetPixels(colors); texture.Apply();
        File.WriteAllBytes("Assets/Art/Prototype/Square.png",texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset("Assets/Art/Prototype/Square.png",ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Art/Prototype/Square.png");
        importer.textureType=TextureImporterType.Sprite; importer.spritePixelsPerUnit=16;
        importer.filterMode=FilterMode.Point; importer.textureCompression=TextureImporterCompression.Uncompressed;
        var textureSettings=new TextureImporterSettings(); importer.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType=SpriteMeshType.FullRect; importer.SetTextureSettings(textureSettings);
        importer.SaveAndReimport();
        square=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Prototype/Square.png");
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        const string materialPath="Assets/Art/Prototype/Unlit.mat";
        spriteMaterial=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(spriteMaterial==null) {
            spriteMaterial=new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            AssetDatabase.CreateAsset(spriteMaterial,materialPath);
        }
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var cameraObject=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(CameraFit));
        cameraObject.tag="MainCamera";
        var camera=cameraObject.GetComponent<Camera>();
        camera.orthographic=true; camera.orthographicSize=9; camera.backgroundColor=Ink;
        camera.clearFlags=CameraClearFlags.SolidColor; cameraObject.transform.position=new Vector3(0,0,-10);

        var manager=new GameObject("GameController").AddComponent<GameController>();
        var p=Box("Player",null,new Vector2(-13,-4.5f),new Vector2(.7f,.7f),Mint,5);
        var body=p.AddComponent<Rigidbody2D>(); body.gravityScale=0; body.freezeRotation=true;
        body.interpolation=RigidbodyInterpolation2D.Interpolate; body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
        var playerCollider=p.AddComponent<BoxCollider2D>();
        var material=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Art/Prototype/NoFriction.physicsMaterial2D");
        if(material==null) {
            material=new PhysicsMaterial2D("Sem atrito"); material.friction=0; material.bounciness=0;
            AssetDatabase.CreateAsset(material,"Assets/Art/Prototype/NoFriction.physicsMaterial2D");
        }
        playerCollider.sharedMaterial=material;
        var player=p.AddComponent<PlayerController2D>(); player.game=manager; player.visual=p.GetComponent<SpriteRenderer>();
        manager.player=player;
        var tagSettings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        tagSettings.FindProperty("layers").GetArrayElementAtIndex(8).stringValue="CourseWalls";
        tagSettings.ApplyModifiedPropertiesWithoutUndo();
        manager.levels=new[] { BuildLevel(manager,true), BuildLevel(manager,false) };
        manager.levels[1].gameObject.SetActive(false);
        BuildHud(manager);
        EditorSceneManager.SaveScene(scene,ScenePath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        PlayerSettings.companyName="thomaskmk";
        PlayerSettings.productName="Percurso Contra o Relógio";
        PlayerSettings.defaultScreenWidth=1280; PlayerSettings.defaultScreenHeight=720;
        PlayerSettings.defaultWebScreenWidth=1280; PlayerSettings.defaultWebScreenHeight=720;
        PlayerSettings.runInBackground=false;
        PlayerSettings.WebGL.template="PROJECT:Percurso";
        PlayerSettings.WebGL.nameFilesAsHashes=true;
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback=true;
        PlayerSettings.WebGL.dataCaching=true;
        AssetDatabase.SaveAssets();
        Debug.Log("PERCURSO: cena criada e referências configuradas.");
    }

    [MenuItem("Percurso/Gerar WebGL")]
    public static void BuildWeb()
    {
        if (!File.Exists(ScenePath)) throw new Exception("Crie a cena antes de compilar.");
        PlayerSettings.WebGL.template="PROJECT:Percurso";
        PlayerSettings.WebGL.nameFilesAsHashes=true;
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback=true;
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=new[]{ScenePath},locationPathName="Builds/WebGL",target=BuildTarget.WebGL,options=BuildOptions.None
        });
        if(report.summary.result!=BuildResult.Succeeded) throw new Exception("WebGL falhou: "+report.summary.result);
        Debug.Log("PERCURSO: WebGL concluído. Bytes: "+report.summary.totalSize);
    }

    private static CourseLevel BuildLevel(GameController game, bool tutorial)
    {
        var root = new GameObject(tutorial ? "Fase 01 — Tutorial" : "Fase 02 — Desafio");
        var level = root.AddComponent<CourseLevel>();
        level.title = tutorial ? "Tutorial" : "Desafio";
        level.tutorial = tutorial;
        level.timeLimit = tutorial ? 90 : 20;
        level.layoutVersion = tutorial ? 2 : 3;
        level.spawn = tutorial ? new Vector2(-13,-4.5f) : new Vector2(-13,0);
        var arena = root.transform;
        Box("Piso",arena,Vector2.zero,new Vector2(29.8f,12.2f),Hex("182A3C"),-10);
        for(int i=-14;i<=14;i++) Box("Grade vertical",arena,new Vector2(i,0),new Vector2(.018f,12),Hex("203449"),-9);
        for(int i=-5;i<=5;i++) Box("Grade horizontal",arena,new Vector2(0,i),new Vector2(29.6f,.018f),Hex("203449"),-9);
        Wall("Limite superior",arena,new Vector2(0,6.15f),new Vector2(30.2f,.3f));
        Wall("Limite inferior",arena,new Vector2(0,-6.15f),new Vector2(30.2f,.3f));
        Wall("Limite esquerdo",arena,new Vector2(-15,0),new Vector2(.3f,12.6f));
        Wall("Limite direito",arena,new Vector2(15,0),new Vector2(.3f,12.6f));
        Box("Área inicial",arena,level.spawn,new Vector2(2.3f,2.2f),Hex("24465A"),-8);
        WorldText("INÍCIO",arena,level.spawn+new Vector2(0,1.45f),.06f,Muted);
        var finish = tutorial ? new Vector2(13,4.6f) : new Vector2(13,0);
        var goal=Box("Chegada",arena,finish,new Vector2(2.5f,2.2f),Hex("287860"),-8);
        goal.AddComponent<BoxCollider2D>().isTrigger=true;
        goal.AddComponent<FinishZone>().game=game;
        WorldText("CHEGADA",arena,finish,.055f,Color.white);

        if (tutorial)
        {
            Wall("Parede 01 — passe por cima",arena,new Vector2(-6,-1.25f),new Vector2(.8f,9.5f));
            Wall("Parede 02 — passe por baixo",arena,new Vector2(3,1.25f),new Vector2(.8f,9.5f));
            WorldText("POR CIMA: MAIS SEGURO  >",arena,new Vector2(-6,5.6f),.037f,Mint);
            WorldText("POR BAIXO: ATALHO  >",arena,new Vector2(-6,3.85f),.035f,Mint);
            WorldText("PASSE POR BAIXO  >",arena,new Vector2(3,-5.6f),.045f,Mint);
            WorldText("01 / DUAS PASSAGENS",arena,new Vector2(-11,2.6f),.045f,Muted);
            WorldText("02 / OBSERVE O CICLO",arena,new Vector2(-1,3.6f),.045f,Muted);
            WorldText("03 / ATRAIA E DESVIE",arena,new Vector2(9,-4.9f),.045f,Muted);
            level.blocks=new[] {
                Hazard("Bloco superior",arena,game,new Vector2(-9,4.65f),new Vector2(-3.8f,4.65f),new Vector2(1,1),1.8f,0),
                Hazard("Bloco inferior",arena,game,new Vector2(.2f,-4.65f),new Vector2(6,-4.65f),new Vector2(1.3f,.65f),2.2f,2)
            };
            level.traps=new[] { Trap(arena,game,new Vector2(-1.45f,.5f),new Vector2(8.1f,1.1f),3,1,2,0) };
            level.slowZones=new[] { Slow(arena,new Vector2(-1.45f,-2.1f),new Vector2(6.5f,1.8f),.5f) };
            level.enemies=new[] { Enemy(arena,game,new Vector2(10.5f,1.5f),new Vector2(12.5f,1.5f),new Rect(7,-1,7,6.3f),3.2f) };
            level.pickups=new[] { Pickup(arena,game,new Vector2(-12,-.9f)) };
        }
        else
        {
            // Duas rotas horizontais se encontram antes do pistão final.
            Box("Rota central",arena,new Vector2(-1,-.3f),new Vector2(19,5.3f),Hex("303342"),-8);
            Wall("Separador das rotas",arena,new Vector2(-.7f,2.4f),new Vector2(16.6f,.5f));
            Wall("Fim do corredor superior",arena,new Vector2(9.4f,4.35f),new Vector2(.5f,3.3f));
            Wall("Base esquerda",arena,new Vector2(-8.3f,-4.5f),new Vector2(13.4f,3));
            Wall("Base direita",arena,new Vector2(8.3f,-4.5f),new Vector2(13.4f,3));
            Wall("Pistão — estrutura superior",arena,new Vector2(12.6f,3.6f),new Vector2(4.8f,4.8f));
            Wall("Pistão — estrutura inferior",arena,new Vector2(12.6f,-2.1f),new Vector2(4.8f,1.8f));
            WorldText("ROTA LONGA / MENOS RISCO  >",arena,new Vector2(-1,5.45f),.045f,Mint);
            WorldText("ATALHO / MAIS RISCO  >",arena,new Vector2(-5.4f,1.7f),.04f,Orange);
            WorldText("GIRO COMPLETO",arena,new Vector2(-4.7f,-2.65f),.035f,Muted);
            WorldText("PÊNDULO",arena,new Vector2(4.3f,1.7f),.04f,Muted);
            WorldText("DESVIO OPCIONAL",arena,new Vector2(0,-5.65f),.032f,Muted);
            WorldText("V",arena,new Vector2(8.35f,3.2f),.065f,Mint);
            level.blocks=new[] {
                Hazard("Pistão vertical",arena,game,new Vector2(10.8f,-2.1f),new Vector2(10.8f,2.1f),new Vector2(.65f,1.4f),3.2f,0)
            };
            level.rotors=new[] {
                Rotor("Barra giratória",arena,game,new Vector2(-4.7f,-.4f),4.3f,0,false),
                Rotor("Pêndulo",arena,game,new Vector2(4.3f,-.4f),4.3f,90,true)
            };
            level.traps=new[] {
                Trap(arena,game,new Vector2(0,-3.5f),new Vector2(3.1f,.9f),1.2f,.7f,2.3f,0)
            };
            level.slowZones=new[] { Slow(arena,new Vector2(-.25f,4.4f),new Vector2(15.5f,3.2f),.35f) };
            level.enemies=new[] {
                Enemy(arena,game,new Vector2(3,-1.6f),new Vector2(6,-1.6f),new Rect(1.9f,-2.5f,5,4.3f),3.6f)
            };
            level.pickups=new[] { Pickup(arena,game,new Vector2(0,-4.8f)) };
        }
        return level;
    }

    private static RotatingHazard Rotor(string name,Transform parent,GameController game,Vector2 center,float length,float angle,bool oscillates)
    {
        // Marcas estáticas indicam o espaço varrido pela barra.
        for(int i=0;i<40;i++)
        {
            float degrees=i*9;
            if(oscillates && !((degrees>=35 && degrees<=145) || (degrees>=215 && degrees<=325))) continue;
            float radians=degrees*Mathf.Deg2Rad;
            Box(name+" trajetória",parent,center+new Vector2(Mathf.Cos(radians),Mathf.Sin(radians))*length/2,
                new Vector2(.07f,.07f),Hex("815340"),-7);
        }
        var go=new GameObject(name,typeof(Rigidbody2D));
        go.transform.SetParent(parent); go.transform.position=center;
        var body=go.GetComponent<Rigidbody2D>(); body.bodyType=RigidbodyType2D.Kinematic;
        body.interpolation=RigidbodyInterpolation2D.Interpolate;
        var bar=Box("Barra",parent,center,new Vector2(length,.32f),oscillates?Hex("FFC166"):Orange,4);
        bar.AddComponent<BoxCollider2D>().isTrigger=true;
        bar.transform.SetParent(go.transform,true);
        var pivot=Box("Eixo",parent,center,new Vector2(.5f,.5f),Hex("FFF0D4"),5);
        pivot.transform.SetParent(go.transform,true);
        go.transform.rotation=Quaternion.Euler(0,0,angle);
        var hazard=go.AddComponent<RotatingHazard>(); hazard.game=game;
        hazard.initialAngle=angle; hazard.oscillates=oscillates;
        return hazard;
    }

    private static CycleTrap Trap(Transform parent,GameController game,Vector2 position,Vector2 size,float safe,float warning,float active,float offset)
    {
        var go=Box("Armadilha cíclica",parent,position,size,Hex("384F5C"),-5);
        go.AddComponent<BoxCollider2D>().isTrigger=true;
        var trap=go.AddComponent<CycleTrap>(); trap.game=game;
        trap.safeSeconds=safe; trap.warningSeconds=warning; trap.activeSeconds=active; trap.phaseOffset=offset;
        trap.label=WorldText("APAGADA",parent,position,.035f,Color.white,-4);
        trap.label.transform.SetParent(go.transform,true);
        return trap;
    }
    private static SlowZone Slow(Transform parent,Vector2 position,Vector2 size,float multiplier)
    {
        var go=Box("Piso lento",parent,position,size,Hex("255C83"),-7);
        go.AddComponent<BoxCollider2D>().isTrigger=true;
        var zone=go.AddComponent<SlowZone>(); zone.speedMultiplier=multiplier;
        WorldText("PISO LENTO",parent,position,.04f,Hex("B5DFFF"),-6);
        return zone;
    }
    private static ExtraLife Pickup(Transform parent,GameController game,Vector2 position)
    {
        var go=Box("Vida extra opcional",parent,position,new Vector2(.8f,.8f),Hex("583B67"),3);
        go.AddComponent<BoxCollider2D>().isTrigger=true;
        var pickup=go.AddComponent<ExtraLife>(); pickup.game=game;
        var crossA=Box("Cruz vertical",parent,position,new Vector2(.2f,.65f),Hex("FFB2DE"),4);
        var crossB=Box("Cruz horizontal",parent,position,new Vector2(.65f,.2f),Hex("FFB2DE"),4);
        crossA.transform.SetParent(go.transform,true); crossB.transform.SetParent(go.transform,true);
        var label=WorldText("+1 VIDA",parent,position+new Vector2(0,.8f),.045f,Hex("FFB2DE"),4);
        label.transform.SetParent(go.transform,true);
        return pickup;
    }
    private static ChaserEnemy Enemy(Transform parent,GameController game,Vector2 a,Vector2 b,Rect territory,float speed)
    {
        var go=Box("Inimigo perseguidor",parent,a,new Vector2(.9f,.9f),Hex("BF54A6"),4);
        var body=go.AddComponent<Rigidbody2D>(); body.bodyType=RigidbodyType2D.Kinematic; body.interpolation=RigidbodyInterpolation2D.Interpolate;
        go.AddComponent<BoxCollider2D>().isTrigger=true;
        var enemy=go.AddComponent<ChaserEnemy>(); enemy.game=game; enemy.pointA=a; enemy.pointB=b;
        enemy.territory=territory; enemy.chaseSpeed=speed; enemy.walls=1<<8;
        enemy.label=WorldText("?",parent,a,.1f,Color.white,6);
        enemy.label.transform.SetParent(go.transform,true);
        return enemy;
    }

    private static GameObject Box(string name,Transform parent,Vector2 position,Vector2 size,Color color,int order)
    {
        var go=new GameObject(name,typeof(SpriteRenderer));
        go.transform.SetParent(parent); go.transform.position=new Vector3(position.x,position.y,-order*.01f); go.transform.localScale=new Vector3(size.x,size.y,1);
        var renderer=go.GetComponent<SpriteRenderer>(); renderer.sprite=square; renderer.color=color; renderer.sortingOrder=order;
        renderer.sharedMaterial=spriteMaterial;
        return go;
    }
    private static void Wall(string name,Transform parent,Vector2 position,Vector2 size)
    {
        var go=Box(name,parent,position,size,Hex("576C80"),1); go.layer=8; go.AddComponent<BoxCollider2D>();
        Box(name+" destaque",parent,position+new Vector2(0,size.y/2-.045f),new Vector2(size.x,.06f),Hex("8DA4B8"),2);
    }
    private static PatrolHazard Hazard(string name,Transform parent,GameController game,Vector2 a,Vector2 b,Vector2 size,float speed,float phase)
    {
        Vector2 center=(a+b)/2;
        Vector2 lineSize=new Vector2(Mathf.Abs(a.x-b.x)+.1f,Mathf.Abs(a.y-b.y)+.1f);
        Box(name+" trilho",parent,center,lineSize,Hex("815340"),-7);
        var go=Box(name,parent,a,size,Orange,4);
        var rb=go.AddComponent<Rigidbody2D>(); rb.bodyType=RigidbodyType2D.Kinematic; rb.interpolation=RigidbodyInterpolation2D.Interpolate;
        go.AddComponent<BoxCollider2D>().isTrigger=true;
        var hazard=go.AddComponent<PatrolHazard>(); hazard.game=game; hazard.pointA=a; hazard.pointB=b; hazard.speed=speed; hazard.phaseOffset=phase;
        return hazard;
    }
    private static TextMesh WorldText(string value,Transform parent,Vector2 position,float size,Color color,int order=-6)
    {
        var go=new GameObject(value,typeof(TextMesh)); go.transform.SetParent(parent); go.transform.position=new Vector3(position.x,position.y,-order*.01f);
        var text=go.GetComponent<TextMesh>(); text.text=value; text.font=font; text.fontSize=64; text.characterSize=size;
        text.anchor=TextAnchor.MiddleCenter; text.alignment=TextAlignment.Center; text.color=color;
        var renderer=go.GetComponent<MeshRenderer>(); renderer.sharedMaterial=font.material; renderer.sortingOrder=order;
        return text;
    }
    private static void BuildHud(GameController game)
    {
        var canvasGo=new GameObject("Interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(CourseHud));
        var canvas=canvasGo.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1600,900);
        scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        var hud=canvasGo.GetComponent<CourseHud>(); game.hud=hud;
        var layout=Rect("Layout centralizado",canvasGo.transform,Vector2.zero,new Vector2(1600,900));
        layout.anchorMin=layout.anchorMax=new Vector2(.5f,.5f);
        var root=layout;
        Panel("Cabeçalho",root,new Vector2(800,835),new Vector2(1520,90),Hex("1D3043"));
        Label("Nome",root,"CONTRA O RELÓGIO",new Vector2(250,850),new Vector2(380,30),25,Color.white,TextAnchor.MiddleLeft,true);
        hud.status=Label("Estado",root,"PERCURSO 01",new Vector2(250,817),new Vector2(380,25),15,Mint,TextAnchor.MiddleLeft);
        Label("Tempo legenda",root,"TEMPO",new Vector2(910,855),new Vector2(100,22),13,Muted,TextAnchor.MiddleCenter);
        hud.timer=Label("Tempo",root,"60s",new Vector2(910,821),new Vector2(130,40),32,Color.white,TextAnchor.MiddleCenter,true);
        Label("Vidas legenda",root,"VIDAS",new Vector2(1080,855),new Vector2(100,22),13,Muted,TextAnchor.MiddleCenter);
        hud.lives=Label("Vidas",root,"1",new Vector2(1080,821),new Vector2(130,40),28,Color.white,TextAnchor.MiddleCenter,true);
        hud.best=Label("Recorde",root,"MELHOR  —",new Vector2(1260,836),new Vector2(200,30),16,Muted,TextAnchor.MiddleCenter);
        var pause=ButtonAt("Pausar",root,"PAUSA  II",new Vector2(1450,835),new Vector2(150,50),Hex("324B61"),Color.white);
        UnityEventTools.AddPersistentListener(pause.onClick,game.TogglePause);
        Panel("Trilho do tempo",root,new Vector2(800,777),new Vector2(1520,4),Hex("2B4054"));
        hud.timerFill=Panel("Barra do tempo",root,new Vector2(800,777),new Vector2(1520,4),Mint);
        hud.timerFill.sprite=square; hud.timerFill.type=Image.Type.Filled; hud.timerFill.fillMethod=Image.FillMethod.Horizontal;
        Label("Controles",root,"WASD / SETAS  mover     •     ESC / P  pausar     •     R  reiniciar",new Vector2(570,65),new Vector2(1020,30),18,Muted,TextAnchor.MiddleLeft);
        Label("Objetivo",root,"CRUZ ROSA = +1 VIDA   /   VERDE = CHEGADA",new Vector2(1270,65),new Vector2(530,30),15,Mint,TextAnchor.MiddleRight);
        Panel("Fundo da dica",root,new Vector2(800,117),new Vector2(1520,46),Hex("1D3043"));
        hud.hint=Label("Dica",root,"",new Vector2(800,117),new Vector2(1480,42),18,Color.white,TextAnchor.MiddleCenter);
        var shade=Panel("Telas da partida",root,new Vector2(800,450),new Vector2(2000,1400),new Color(.025f,.05f,.08f,.91f));
        shade.raycastTarget=true; hud.overlay=shade.gameObject;
        var panel=Panel("Cartão",shade.transform,new Vector2(1000,700),new Vector2(880,690),Hex("1B2D40"));
        // Elementos internos usam coordenadas locais relativas ao canto inferior esquerdo do cartão.
        var card=panel.transform;
        Panel("Acento",card,new Vector2(440,687),new Vector2(880,6),Mint);
        hud.subtitle=Label("Subtítulo",card,"",new Vector2(440,630),new Vector2(780,30),17,Mint,TextAnchor.MiddleCenter,true);
        hud.title=Label("Título",card,"",new Vector2(440,495),new Vector2(800,190),70,Color.white,TextAnchor.MiddleCenter,true);
        hud.description=Label("Descrição",card,"",new Vector2(440,342),new Vector2(780,80),22,Muted,TextAnchor.MiddleCenter);
        hud.result=Label("Resultado",card,"",new Vector2(440,271),new Vector2(810,40),19,Mint,TextAnchor.MiddleCenter,true);
        hud.primary=ButtonAt("Ação principal",card,"COMEÇAR",new Vector2(440,204),new Vector2(410,68),Mint,Ink);
        hud.primaryLabel=hud.primary.GetComponentInChildren<Text>();
        UnityEventTools.AddPersistentListener(hud.primary.onClick,game.PrimaryAction);
        hud.secondary=ButtonAt("Voltar ao menu",card,"VOLTAR AO MENU",new Vector2(440,121),new Vector2(410,52),Hex("30475B"),Color.white);
        UnityEventTools.AddPersistentListener(hud.secondary.onClick,game.ReturnToMenu);
        hud.challenge=ButtonAt("Desafio desbloqueado",card,"JOGAR DESAFIO",new Vector2(440,121),new Vector2(410,52),Hex("30475B"),Color.white);
        UnityEventTools.AddPersistentListener(hud.challenge.onClick,game.StartChallenge);
        Label("Ajuda",card,"WASD ou setas para mover.  Controle: analógico / direcional.\nEnter / A confirma.  Esc / Start pausa.  R reinicia.",new Vector2(440,49),new Vector2(800,60),17,Muted,TextAnchor.MiddleCenter);
        var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        EditorUtility.SetDirty(events);
    }
    private static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)
    {
        var go=new GameObject(name,typeof(RectTransform)); var rect=go.GetComponent<RectTransform>(); rect.SetParent(parent,false);
        rect.anchorMin=rect.anchorMax=Vector2.zero; rect.pivot=new Vector2(.5f,.5f); rect.anchoredPosition=position; rect.sizeDelta=size;
        return rect;
    }
    private static Image Panel(string name,Transform parent,Vector2 position,Vector2 size,Color color)
    {
        var rect=Rect(name,parent,position,size); var image=rect.gameObject.AddComponent<Image>(); image.color=color; image.raycastTarget=false; return image;
    }
    private static Text Label(string name,Transform parent,string value,Vector2 position,Vector2 size,int fontSize,Color color,TextAnchor anchor,bool bold=false)
    {
        var rect=Rect(name,parent,position,size); var text=rect.gameObject.AddComponent<Text>(); text.font=font; text.text=value;
        text.fontSize=fontSize; text.color=color; text.alignment=anchor; text.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
        text.raycastTarget=false; text.horizontalOverflow=HorizontalWrapMode.Wrap; text.verticalOverflow=VerticalWrapMode.Truncate;
        return text;
    }
    private static Button ButtonAt(string name,Transform parent,string value,Vector2 position,Vector2 size,Color fill,Color textColor)
    {
        var image=Panel(name,parent,position,size,fill); image.raycastTarget=true;
        var button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image;
        var colors=button.colors; colors.normalColor=Color.white; colors.highlightedColor=new Color(.85f,.94f,1); colors.selectedColor=new Color(.85f,.94f,1); colors.pressedColor=new Color(.7f,.8f,.9f); button.colors=colors;
        Label("Texto",image.transform,value,size/2,size-new Vector2(16,6),20,textColor,TextAnchor.MiddleCenter,true);
        return button;
    }
    private static Color Hex(string value) { ColorUtility.TryParseHtmlString("#"+value,out var color); return color; }
}
