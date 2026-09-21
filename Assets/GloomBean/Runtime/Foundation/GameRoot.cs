using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance {get;private set;}
        public SaveStore Save {get;private set;}
        public StageSession Session {get;private set;}
        public ICampaignSource Source {get;private set;}
        public bool Practice {get;private set;}
        public bool IsCorrupted => runCorrupted || Save.Data.corrupted;
        public string PossessionDisplay="";
        public string PossessionHelp="";
        public Action ExtraHud;
        public Action GameplayHud;
        public Action<Rect,string,int,Color> PixelLabel;
        public Action<Rect,Color> PixelPanel;
        public Action<Rect,string,bool,bool> PixelButton;
        public Func<Matrix4x4> PresentationMatrix;

        public Action<string> PresentationBackground;
        public string CurrentScreen=>screen.ToString();
        public bool ShowingRestoredEnding=>screen==ScreenMode.Ending&&!Practice&&Save.RestoredEnding;
        public int SelectedWorldNumber=>SourceCount>1&&sourceIndex==1?worldIndex+1:0;
        public event Action<StageDefinition,StageBuilder> StageBuilt;
        readonly List<ICampaignSource> sources=new List<ICampaignSource>();
        WorldDefinition[] worlds;int sourceIndex,worldIndex,choice;
        enum ScreenMode { Home, Worlds, Levels, Play, Pause, Clear, Fail, Ending }
        ScreenMode screen;
        Transform stageRoot;
        bool controls,loading,runCorrupted,confirmReset;
        bool pendingEnter;
        float menuRepeat;
        GUIStyle title,heading,body,small,button;
        Texture2D white;
        public bool testMode;
        public string reportDirectory;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if(FindFirstObjectByType<GameRoot>())return;
            new GameObject("Gloom Bean — Boot").AddComponent<GameRoot>();
        }
        void Awake()
        {
            if(Instance&&Instance!=this){Destroy(gameObject);return;}Instance=this;
            DontDestroyOnLoad(gameObject);Application.targetFrameRate=120;Time.fixedDeltaTime=1f/60;
            Physics2D.gravity=new Vector2(0,-10);
            for(int i=0;i<32;i++) { Physics2D.IgnoreLayerCollision(Layers.Sensor,i,false); }
            Physics2D.IgnoreLayerCollision(Layers.Actor,Layers.Interior,true);
            Physics2D.IgnoreLayerCollision(Layers.Enemy,Layers.Interior,true);
            // Shadow and inside-out actors use separate collision domains in the campaign.
            sources.Add(new FoundationCampaign());
            var atlasType=Type.GetType("GloomBean.Campaign.AtlasCampaign, Assembly-CSharp");
            if(atlasType!=null&&typeof(ICampaignSource).IsAssignableFrom(atlasType))sources.Add((ICampaignSource)Activator.CreateInstance(atlasType));
            Source=sources[0];worlds=Source.Worlds();screen=ScreenMode.Home;
            string[] args=Environment.GetCommandLineArgs();
            testMode=Array.IndexOf(args,"-gb-visual-v6")>=0||Array.IndexOf(args,"-gb-verify")>=0||Array.IndexOf(args,"-gb-route-verify")>=0||Array.IndexOf(args,"-gb-parish-verify")>=0||Array.IndexOf(args,"-gb-orchard-verify")>=0||Array.IndexOf(args,"-gb-city-verify")>=0||Array.IndexOf(args,"-gb-fall-verify")>=0||Array.IndexOf(args,"-gb-empyrean-verify")>=0;
            reportDirectory=Argument(args,"-gb-reports",Path.Combine(Application.persistentDataPath,"Reports"));
            string savePath=Argument(args,"-gb-save",Path.Combine(Application.persistentDataPath,"host-cycle-save.json"));
            if(testMode)savePath=Path.Combine(reportDirectory,"test-save.json");
            Save=new SaveStore(savePath);
            if(!UnityEngine.Camera.main){var cam=new GameObject("Menu Camera").AddComponent<UnityEngine.Camera>();cam.tag="MainCamera";cam.orthographic=true;cam.backgroundColor=new Color(.045f,.035f,.075f);cam.transform.position=new Vector3(0,0,-10);}
            if(!UnityEngine.Camera.main.GetComponent<AudioListener>())UnityEngine.Camera.main.gameObject.AddComponent<AudioListener>();
            gameObject.AddComponent<GameAudio>().Initialize(testMode);
            var presentation=Type.GetType("GloomBean.Campaign.CampaignPresentation, Assembly-CSharp");
            if(presentation!=null)gameObject.AddComponent(presentation);
            if(testMode)StartCoroutine(BeginVerification());
        }
        static string Argument(string[] args,string flag,string fallback)
        {int i=Array.IndexOf(args,flag);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
        IEnumerator BeginVerification()
        {
            yield return null;
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-visual-v6")>=0){var visual=Type.GetType("GloomBean.Campaign.VisualReviewV6, Assembly-CSharp");visual.GetMethod("Begin").Invoke(gameObject.AddComponent(visual),new object[]{this});yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-empyrean-verify")>=0){var empyrean=Type.GetType("GloomBean.Campaign.EmpyreanVerification, Assembly-CSharp");if(empyrean==null)throw new InvalidOperationException("Empyrean witness is not installed.");empyrean.GetMethod("Begin").Invoke(gameObject.AddComponent(empyrean),new object[]{this});yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-fall-verify")>=0){var fall=Type.GetType("GloomBean.Campaign.FallVerification, Assembly-CSharp");if(fall==null)throw new InvalidOperationException("Fall witness is not installed.");fall.GetMethod("Begin").Invoke(gameObject.AddComponent(fall),new object[]{this});yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-city-verify")>=0)
            {
                var city=Type.GetType("GloomBean.Campaign.CityVerification, Assembly-CSharp");
                if(city==null)throw new InvalidOperationException("City witness is not installed.");
                city.GetMethod("Begin").Invoke(gameObject.AddComponent(city),new object[]{this});yield break;
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-orchard-verify")>=0)
            {
                var orchard=Type.GetType("GloomBean.Campaign.OrchardVerification, Assembly-CSharp");
                if(orchard==null)throw new InvalidOperationException("Orchard witness is not installed.");
                orchard.GetMethod("Begin").Invoke(gameObject.AddComponent(orchard),new object[]{this});yield break;
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-parish-verify")>=0)
            {
                var parish=Type.GetType("GloomBean.Campaign.ParishVerification, Assembly-CSharp");
                if(parish==null)throw new InvalidOperationException("Parish campaign is not installed.");
                parish.GetMethod("Begin").Invoke(gameObject.AddComponent(parish),new object[]{this});yield break;
            }
            var type=Type.GetType("GloomBean.Campaign.RouteVerification, Assembly-CSharp");
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-gb-route-verify")>=0&&type!=null)
                type.GetMethod("Begin").Invoke(gameObject.AddComponent(type),new object[]{this});
            else gameObject.AddComponent<FoundationVerification>().Begin(this);
        }
        public bool OpenPractice(string stageId)
        {
            for(int si=0;si<sources.Count;si++)
            {
                var list=sources[si].Worlds();
                for(int wi=0;wi<list.Length;wi++)
                {
                    foreach(var stage in list[wi].levels)
                        if(stage.id==stageId){SelectSource(si);worldIndex=wi;LoadStage(stage,true);return true;}
                    if(list[wi].boss.id==stageId){SelectSource(si);worldIndex=wi;LoadStage(list[wi].boss,true);return true;}
                }
            }
            return false;
        }
        public void SelectSource(int index){sourceIndex=Mathf.Clamp(index,0,sources.Count-1);Source=sources[sourceIndex];worlds=Source.Worlds();worldIndex=0;}
        public WorldDefinition[] AvailableWorlds=>worlds;
        public int SourceCount=>sources.Count;
        public void LoadStage(StageDefinition d,bool practice=false){if(!loading)StartCoroutine(Load(d,practice));}
        public IEnumerator Load(StageDefinition d,bool practice=false)
        {
            loading=true;Time.timeScale=1;Practice=practice;runCorrupted=false;PossessionDisplay=PossessionHelp="";ExtraHud=null;
            if(stageRoot){stageRoot.gameObject.SetActive(false);Destroy(stageRoot.gameObject);}Session=null;
            yield return null;
            stageRoot=new GameObject("Stage "+d.id+" — "+d.title).transform;
            Session=stageRoot.gameObject.AddComponent<StageSession>();Session.Configure(d,practice?null:Save);
            var cam=UnityEngine.Camera.main;var follow=cam.GetComponent<FollowCamera>();if(!follow)follow=cam.gameObject.AddComponent<FollowCamera>();Session.Camera=follow;
            var builder=new StageBuilder(stageRoot,Session);Source.Build(d,builder);
            StageLayoutSnapshot.Install(d.id,stageRoot);
            follow.target=Session.player.transform;follow.secondary=null;follow.Snap();cam.backgroundColor=builder.background;
            Session.player.GetComponent<ActorView>().corrupted=d.atlas&&IsCorrupted;
            Session.Completed+=()=>{screen=d.boss&&d.worldId=="W5"?ScreenMode.Ending:ScreenMode.Clear;Time.timeScale=0;choice=0;};
            Session.Failed+=()=>{screen=ScreenMode.Fail;Time.timeScale=0;choice=0;};
            StageBuilt?.Invoke(d,builder);
            screen=ScreenMode.Play;controls=false;loading=false;
            yield return null;
            if(d.startsReturning)Session.Turn();
        }
        public void MarkCorrupted()
        {
            runCorrupted=true;if(!Practice)Save.MarkCorruption();
            if(Session&&Session.player){var view=Session.player.GetComponent<ActorView>();if(view)view.corrupted=true;}
        }
        public void MainMenu()
        {
            Time.timeScale=1;if(stageRoot){stageRoot.gameObject.SetActive(false);Destroy(stageRoot.gameObject);}
            Session=null;var follow=UnityEngine.Camera.main.GetComponent<FollowCamera>();if(follow){follow.target=null;follow.secondary=null;}
            screen=ScreenMode.Home;choice=0;
        }
        void Update()
        {
            if(testMode)return;
            if(Input.GetKeyDown(KeyCode.F1))controls=!controls;
            if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.JoystickButton7))
            {
                if(screen==ScreenMode.Play){screen=ScreenMode.Pause;Time.timeScale=0;choice=0;}
                else if(screen==ScreenMode.Pause){screen=ScreenMode.Play;Time.timeScale=1;}
                else if(screen==ScreenMode.Levels){screen=ScreenMode.Worlds;choice=0;}
                else if(screen==ScreenMode.Worlds){screen=ScreenMode.Home;choice=0;}
            }
            if(screen!=ScreenMode.Play)
            {
                menuRepeat-=Time.unscaledDeltaTime;
                float joy=Input.GetJoystickNames().Length>0?Input.GetAxisRaw("JoyY"):0;
                if(Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S)||joy>.5f&&menuRepeat<=0){choice++;menuRepeat=.2f;}
                if(Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.W)||joy<-.5f&&menuRepeat<=0){choice=Mathf.Max(0,choice-1);menuRepeat=.2f;}
                pendingEnter|=Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.JoystickButton0);
            }
        }
        string WorldTitle(WorldDefinition world)
        {
            if(Practice||IsCorrupted||world.id=="BASE")return world.title;
            string[] pleasant={"Little Sunday Town","The Harvest Fair","The Painted Promenade","The Skyline Parade","The Cloud Garden"};
            int number;if(int.TryParse(world.id.Substring(1),out number)&&number>=1&&number<=5)return pleasant[number-1];return world.title;
        }
        string StageTitle(StageDefinition stage)
        {
            if(Practice||IsCorrupted||!stage.atlas)return stage.title;
            string[] pleasant={"Sunday Best","The Bell Tower","Wash Day","The Dress-up House"};return !stage.boss&&stage.course>=1&&stage.course<=4?pleasant[stage.course-1]:stage.boss?"The Parade's Kindly Usher":stage.title;
        }
        public StageDefinition NextStage()
        {
            if(!Session)return null;string id=Session.definition.id;
            for(int w=0;w<worlds.Length;w++){
                for(int i=0;i<worlds[w].levels.Length;i++)if(worlds[w].levels[i].id==id)return i+1<worlds[w].levels.Length?worlds[w].levels[i+1]:worlds[w].boss;
                if(worlds[w].boss.id==id)return w+1<worlds.Length?worlds[w+1].levels[0]:null;
            }
            return null;
        }
        void ContinueJourney()
        {
            var next=NextStage();if(next==null){MainMenu();return;}
            for(int w=0;w<worlds.Length;w++)if(worlds[w].id==next.worldId)worldIndex=w;
            LoadStage(next,Practice);choice=0;
        }
        void Styles()
        {
            if(title!=null)return;
            white=Texture2D.whiteTexture;
            title=new GUIStyle(GUI.skin.label){fontSize=54,fontStyle=FontStyle.Bold,normal={textColor=new Color(1,.86f,.49f)}};
            heading=new GUIStyle(GUI.skin.label){fontSize=22,fontStyle=FontStyle.Bold,normal={textColor=Color.white}};
            body=new GUIStyle(GUI.skin.label){fontSize=16,wordWrap=true,normal={textColor=new Color(.88f,.87f,.92f)}};
            small=new GUIStyle(body){fontSize=12};
            button=new GUIStyle(GUI.skin.button){fontSize=17,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(17,12,8,8)};
        }
        void Text(Rect r,string value,GUIStyle style){if(PixelLabel!=null)PixelLabel(r,value,style.fontSize,style.normal.textColor);else GUI.Label(r,value,style);}
        void Panel(Rect r,Color c){if(PixelPanel!=null){PixelPanel(r,c);return;}var old=GUI.color;GUI.color=c;GUI.DrawTexture(r,white);GUI.color=old;}
        int buttonIndex;
        bool Button(Rect rect,string text,bool enabled=true)
        {
            int i=buttonIndex++;bool selected=choice==i;var old=GUI.backgroundColor;
            GUI.backgroundColor=selected?new Color(.65f,.32f,.65f):new Color(.25f,.23f,.33f);
            GUI.enabled=enabled;
            bool clicked=PixelButton!=null?GUI.Button(rect,GUIContent.none,GUIStyle.none):GUI.Button(rect,(selected?">  ":"    ")+text,button);
            if(PixelButton!=null)PixelButton(rect,text,selected,enabled);
            bool enter=pendingEnter&&selected&&Event.current.type==EventType.Repaint;
            if(enter)pendingEnter=false;GUI.enabled=true;GUI.backgroundColor=old;return enabled&&(clicked||enter);
        }
        void OnGUI()
        {
            Styles();GUI.matrix=PresentationMatrix!=null?PresentationMatrix():Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/960f,Screen.height/600f,1));buttonIndex=0;
            bool friendlyMenu=PresentationBackground!=null&&!IsCorrupted&&!Practice&&(screen==ScreenMode.Home||screen==ScreenMode.Worlds||screen==ScreenMode.Levels);
            title.normal.textColor=friendlyMenu?new Color(.33f,.11f,.37f):new Color(1,.86f,.49f);
            heading.normal.textColor=friendlyMenu?new Color(.24f,.10f,.30f):Color.white;
            body.normal.textColor=small.normal.textColor=friendlyMenu?new Color(.25f,.17f,.29f):new Color(.88f,.87f,.92f);
            if(screen==ScreenMode.Play||screen==ScreenMode.Pause||screen==ScreenMode.Clear||screen==ScreenMode.Fail)
            {
                bool visualHud=Session&&Session.definition.atlas&&GameplayHud!=null;
                if(visualHud)GameplayHud();
                else {
                Panel(new Rect(0,0,960,72),new Color(.03f,.025f,.06f,.92f));
                if(Session)
                {
                    Text(new Rect(20,9,650,28),Session.definition.title,heading);
                    string status="HEALTH "+Session.player.Health+"   COINS "+Session.Coins+"   "+(Session.definition.requiredShards>0?"SEALS "+Session.Shards+"/"+Session.definition.requiredShards+"   ":"")+(Session.definition.requiresKey?(Session.HasKey?"KEYLING YES":"KEYLING —")+"   ":"")+"MERCY "+Session.Mercies.Count;
                    Text(new Rect(20,42,780,24),status,small);
                    Text(new Rect(755,14,185,38),Session.Phase==RunPhase.Returning?(Session.definition.timed?TimeSpan.FromSeconds(Mathf.Max(0,Session.Remaining)).ToString(@"mm\:ss"):"THE TURN"):"EXPLORE",heading);
                    if(!string.IsNullOrEmpty(Session.Message)){Panel(new Rect(115,512,730,64),new Color(.07f,.05f,.1f,.93f));Text(new Rect(131,524,698,48),Session.Message,body);}
                    if(!string.IsNullOrEmpty(PossessionDisplay)){Text(new Rect(20,80,750,25),PossessionDisplay,heading);Text(new Rect(20,110,700,46),PossessionHelp,small);}
                    Text(new Rect(20,577,720,22),(Practice?"PRACTICE — no completion or Mercy save    ":"")+"F1 controls  ·  Esc pause",small);
                }
                }
                ExtraHud?.Invoke();
            }
            if(screen==ScreenMode.Home)
            {
                Panel(new Rect(0,0,960,600),new Color(.055f,.04f,.085f));
                PresentationBackground?.Invoke("Home");
                Text(new Rect(56,53,870,76),IsCorrupted?"GLOOM BEAN":"BEAN'S SUNDAY BEST",title);
                Text(new Rect(61,140,800,35),IsCorrupted?"SAME BEAN. DIFFERENT HOST.":"A LITTLE SUNDAY WALK. A LONG WAY HOME.",heading);
                Text(new Rect(61,182,810,58),IsCorrupted?"TWENTY ROUTES. FIFTEEN UNWANTED GUESTS.\nBORROW A RULE. FIND YOUR WAY BACK.":"THE PARADE IS READY. PUT ON YOUR SHOES.\nTHERE IS NOTHING UNDER THE SCENERY.",body);
                float y=270;
                for(int i=0;i<sources.Count;i++)
                {
                    int index=sources.Count-1-i;
                    if(Button(new Rect(60,y,630,48),index==0?"PLATFORMER FOUNDATION":"BEGIN / CONTINUE HOST CYCLE")){SelectSource(index);screen=ScreenMode.Worlds;choice=0;Practice=false;}
                    y+=60;
                }
                if(Button(new Rect(60,y,630,48),"PRACTICE / direct level selection")){SelectSource(sources.Count-1);Practice=true;screen=ScreenMode.Worlds;choice=0;}
                Text(new Rect(61,520,820,54),"Keyboard: arrows / WASD, Space, Shift, J, K, E.\nGamepad: left stick, A jump, X tackle, Y carry, B interact, LB run. See F1 for possession controls.",small);
            }
            if(screen==ScreenMode.Worlds)
            {
                Panel(new Rect(0,0,960,600),new Color(.055f,.04f,.085f));PresentationBackground?.Invoke("Worlds");Text(new Rect(50,35,860,55),Practice?"CHOOSE A WORLD — PRACTICE":"CHOOSE A WORLD",heading);
                for(int i=0;i<worlds.Length;i++)
                {
                    bool open=CampaignProgression.WorldOpen(worlds,i,Save.Data,Practice);
                    if(Button(new Rect(55,108+i*70,830,55),WorldTitle(worlds[i])+(open?"":"  [clear previous boss]"),open)){worldIndex=i;screen=ScreenMode.Levels;choice=0;}
                }
                if(Button(new Rect(55,500,350,43),"Back")){screen=ScreenMode.Home;choice=0;}
            }
            if(screen==ScreenMode.Levels)
            {
                var world=worlds[worldIndex];Panel(new Rect(0,0,960,600),new Color(.055f,.04f,.085f));PresentationBackground?.Invoke("Levels");Text(new Rect(50,35,860,50),WorldTitle(world),heading);
                bool all=true;
                for(int i=0;i<world.levels.Length;i++)
                {
                    var stage=world.levels[i];bool done=Save.Data.cleared.Contains(stage.id);all&=done;
                    bool unlocked=CampaignProgression.LevelOpen(world,i,Save.Data,Practice);
                    if(Button(new Rect(55,106+i*64,830,58),(i+1)+". "+StageTitle(stage)+(done?"  [cleared]":""),unlocked))LoadStage(stage,Practice);
                }
                if(Button(new Rect(55,380,830,52),"BOSS — "+world.boss.title,CampaignProgression.BossOpen(world,Save.Data,Practice)))LoadStage(world.boss,Practice);
                if(Button(new Rect(55,470,350,44),"World select")){screen=ScreenMode.Worlds;choice=0;}
            }
            if(screen==ScreenMode.Pause||screen==ScreenMode.Clear||screen==ScreenMode.Fail)
            {
                Panel(new Rect(220,162,520,290),new Color(.035f,.025f,.06f,.97f));
                Text(new Rect(247,179,470,40),screen==ScreenMode.Pause?"PAUSED":screen==ScreenMode.Clear?"YOU MADE IT BACK":"THE ROUTE WAS LOST",heading);
                if(screen==ScreenMode.Fail)Text(new Rect(247,220,470,32),Session.FailureReason,body);
                if(screen==ScreenMode.Pause&&Button(new Rect(247,239,466,43),"Resume")){screen=ScreenMode.Play;Time.timeScale=1;}
                if(screen==ScreenMode.Clear&&Button(new Rect(247,239,466,43),"Continue the journey"))ContinueJourney();
                if(Button(new Rect(247,292,466,43),"Restart level"))LoadStage(Session.definition,Practice);
                if(Button(new Rect(247,348,466,43),"Level select")){Time.timeScale=1;if(stageRoot)stageRoot.gameObject.SetActive(false);screen=ScreenMode.Levels;choice=0;}
            }
            if(screen==ScreenMode.Ending)
            {
                Panel(new Rect(0,0,960,600),new Color(.08f,.05f,.12f));
                PresentationBackground?.Invoke("Ending");
                bool restore=ShowingRestoredEnding;
                Text(new Rect(70,64,830,50),restore?"A BODY OF YOUR OWN":"STILL YOURSELF. STILL OPEN.",heading);
                Text(new Rect(70,135,790,85),restore?"Twenty small mercies deny the final tenant. For this ending, the original Bean returns.\nThe journey's scars remain in the saved world.":"The Host of Hosts is gone. The Open Host survives.\nOrdinary completion does not undo the first corruption.",body);
                if(Button(new Rect(250,535,460,42),"Return to title"))MainMenu();
            }
            if(controls)
            {
                // Controls always use their own dark modal panel, including over a friendly title.
                heading.normal.textColor=Color.white;body.normal.textColor=new Color(.88f,.87f,.92f);
                Panel(new Rect(24,18,912,564),new Color(.02f,.015f,.04f,.98f));Text(new Rect(44,34,870,40),"CONTROLS / MOVEMENT VOCABULARY",heading);
                Text(new Rect(44,91,868,470),"MOVE: ARROWS / WASD / LEFT STICK\nJUMP: SPACE / Z / PAD A\nRUN: SHIFT / LB     TACKLE: J / PAD X\nPOUND: L / DOWN+TACKLE IN AIR\nCROUCH / CRAWL / SLOPE ROLL: DOWN\nCARRY / THROW: K / C / PAD Y\nSTUN FIRST. UP / DOWN AIMS THE THROW.\nSWIM DASH: TACKLE IN WATER\nINTERACT / PULL NAIL: E / PAD B\nPOSSESSION: U / RB   SECONDARY: I\nCHANGE FOCUSED FORM: DOWN+I\nPAUSE: ESC / START  CLOSE HELP: F1\nSOUND: F4          MUSIC: F5\n\nFORMS COME FROM CREATURES, NOT MENUS.\nACCEPT THEIR RULE. FIND THEIR CURE.\nF2 SHOWS DEVELOPMENT NOTES",body);
            }
            if(Event.current.type==EventType.Repaint&&buttonIndex>0){choice=Mathf.Clamp(choice,0,buttonIndex-1);pendingEnter=false;}
        }
    }
}
