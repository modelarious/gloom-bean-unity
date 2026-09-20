using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    /// <summary>IMGUI presentation and an original offline score. No UI scene system or gameplay side effects.</summary>
    public sealed class CampaignPresentation:MonoBehaviour
    {
        public string LastPaintedScreen {get;private set;}="";public bool LastFigureWasNormal {get;private set;}
        GameRoot game;CampaignScore score;GUIStyle caption,kicker;string lastScreen="";float entered;
        void Start(){game=GetComponent<GameRoot>();game.PresentationBackground=Draw;score=gameObject.AddComponent<CampaignScore>();score.Initialize(game);}
        void Update(){if(!game)return;if(lastScreen!=game.CurrentScreen){lastScreen=game.CurrentScreen;entered=Time.unscaledTime;}}
        static void Texture(Rect r,Sprite sprite,Color tint){var before=GUI.color;GUI.color=tint;GUI.DrawTexture(r,sprite.texture,ScaleMode.ScaleToFit,true);GUI.color=before;}
        static void Fill(Rect r,Color tint){var before=GUI.color;GUI.color=tint;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=before;}
        void Draw(string screen)
        {
            if(caption==null){caption=new GUIStyle(GUI.skin.label){fontSize=17,wordWrap=true,alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(.95f,.88f,.70f)}};kicker=new GUIStyle(caption){fontSize=11,alignment=TextAnchor.MiddleLeft};}
            LastPaintedScreen=screen;bool ending=screen=="Ending",restored=ending&&game.ShowingRestoredEnding;
            int world=!game.IsCorrupted&&!game.Practice?0:screen=="Home"?1:ending?0:game.SelectedWorldNumber;
            // The opening deception must be visibly welcoming, not a dark menu with a cute label.
            bool friendly=!ending&&!game.IsCorrupted&&!game.Practice;
            var art=SceneryArt.Get(world);Texture(new Rect(0,0,960,600),art,friendly?Color.white:ending?new Color(.60f,.52f,.58f):new Color(.34f,.30f,.39f));
            Fill(new Rect(0,0,960,600),friendly?new Color(1,.98f,.87f,.10f):new Color(.025f,.02f,.04f,ending?.24f:.58f));
            caption.normal.textColor=friendly?new Color(.24f,.10f,.30f):new Color(.95f,.88f,.70f);
            kicker.normal.textColor=friendly?new Color(.24f,.10f,.30f):new Color(.95f,.88f,.70f);
            for(int i=0;i<3;i++)Fill(new Rect(22+i*5,22+i*5,916-i*10,1),new Color(.73f,.56f,.39f,.35f));
            if(screen=="Home"){
                var sprite=HostPixelArt.Host(HostKind.None,game.IsCorrupted,(int)(Time.unscaledTime*3)%4);
                Texture(new Rect(714,253,196,215),sprite,Color.white);
                GUI.Label(new Rect(704,475,216,45),game.IsCorrupted?"SAME BEAN.\nDIFFERENT HOST.":"YOUR SUNDAY BEST.",caption);
                GUI.Label(new Rect(61,557,790,25),"Original Host Cycle campaign  /  Editable native Unity project",kicker);
            }else if(ending){
                float age=Time.unscaledTime-entered;bool showNormal=restored&&age>=5;LastFigureWasNormal=showNormal;
                float wobble=showNormal?0:Mathf.Sin(age*1.4f)*5;
                Texture(new Rect(374+wobble,245,216,218),HostPixelArt.Host(HostKind.None,!showNormal,(int)(age*3)%4),Color.white);
                if(restored){for(int i=0;i<20;i++){float angle=i*Mathf.PI*2/20+age*.18f;Vector2 p=new Vector2(480+Mathf.Cos(angle)*(age<5?143:165),344+Mathf.Sin(angle)*99);Fill(new Rect(p.x-3,p.y-3,6,6),new Color(1,.87f,.52f,Mathf.Clamp01(age/2)));}}
                else for(int i=0;i<5;i++){float x=421+i*27;Fill(new Rect(x,433,3,15+Mathf.Sin(age+i)*5),new Color(.83f,.09f,.36f));}
                string line=age<2?"The last cable gives way.":age<5?"For once, the silence belongs to you.":restored?"Twenty small mercies. One body that is finally yours.":"You are still open. You are still yourself.";
                GUI.Label(new Rect(145,477,670,43),line,caption);
            }
        }
        void OnDestroy(){if(game)game.PresentationBackground=null;}
    }
    public sealed class CampaignScore:MonoBehaviour
    {
        public const int Rate=24000;GameRoot game;GameAudio fx;AudioSource[] voices;int current,key=-1;float blend=1;bool muted;
        readonly Dictionary<int,AudioClip> clips=new Dictionary<int,AudioClip>();
        public int PlayingTheme {get;private set;}=-1;
        public static float[] Compose(int world,bool turned)
        {
            // Eight original modal phrases; no sampled recordings or network dependency.
            int[][] motifs={new[]{0,4,7,9,7,4,2,7},new[]{0,3,7,8,7,2,1,3},new[]{0,1,5,3,7,6,1,0},new[]{0,2,6,4,10,8,6,2},new[]{0,0,6,1,0,-2,3,1},new[]{0,4,7,11,6,7,4,1},new[]{0,4,7,12,9,7,4,2},new[]{0,3,7,8,6,3,1,0}};
            world=Mathf.Clamp(world,0,7);int steps=world==2?10:world==3?14:world==4?8:12;
            float eighth=world==4?.28f:world==0?.23f:world==5?.35f:.31f;if(turned)eighth*=.88f;
            int count=(int)(steps*8*eighth*Rate);var data=new float[count];int root=new[]{62,50,43,48,38,55,62,50}[world];
            void Note(float at,float duration,float midi,float level,int voice)
            {
                int first=(int)(at*Rate),length=(int)(duration*Rate);double hz=440*Math.Pow(2,(midi-69)/12.0),step=2*Math.PI*hz/Rate;
                for(int i=0;i<length&&first+i<count;i++){float t=(float)i/Rate,u=(float)i/Math.Max(1,length);double wave;
                    if(voice==0)wave=Math.Sin(i*step)+.31*Math.Sin(i*step*2.004)+.12*Math.Sin(i*step*3.01);
                    else if(voice==1)wave=Math.Sin(i*step)*.7+Math.Sin(i*step*2.756)*Math.Exp(-t*9)*.24;
                    else wave=Math.Sin(i*step)+.22*Math.Sin(i*step*3)+.08*Math.Sin(i*step*5);
                    float attack=Mathf.Min(1,t/(voice==2?.16f:.012f)),release=Mathf.Min(1,(1-u)*12),env=voice==2?1-u*.35f:(float)Math.Exp(-t*(voice==0?3.4:5));
                    data[first+i]+=(float)wave*attack*release*env*level;
                }
            }
            for(int bar=0;bar<8;bar++){
                int harmony=new[]{0,-5,-3,-7,0,-5,world==0||world==6?5:1,-7}[bar];float start=bar*steps*eighth;
                Note(start,steps*eighth,root-12+harmony,.10f,2);Note(start,steps*eighth*.96f,root+harmony+7,.045f,2);
                for(int k=0;k<steps;k++){
                    float at=start+k*eighth;int index=(k+bar*2)%8;
                    if(k%2==0||turned&&k%3==1)Note(at,eighth*(k%4==0?2.6f:1.6f),root+12+motifs[world][index],.105f,world==0||world==5||world==6?1:0);
                    if(k%3==0)Note(at,eighth*.55f,root+harmony+(k%2==0?0:7),.055f,0);
                    if(world==4||turned){int first=(int)(at*Rate),n=(int)(.065f*Rate);uint noise=(uint)(7321+bar*79+k*431);
                        for(int j=0;j<n&&first+j<count;j++){noise^=noise<<13;noise^=noise>>17;noise^=noise<<5;float u=(float)j/n;data[first+j]+=(noise%65536/32768f-1)*.025f*(1-u)*(1-u);}}
                }
            }
            // Peak limiting preserves headroom for separate movement and interaction cues.
            for(int i=0;i<count;i++)data[i]=Mathf.Clamp(data[i],-.65f,.65f)*Mathf.Min(1,Mathf.Min(i/240f,(count-1-i)/240f));
            return data;
        }
        public void Initialize(GameRoot root){game=root;fx=GetComponent<GameAudio>();muted=PlayerPrefs.GetInt("GloomBean.MusicMuted",0)==1;voices=new AudioSource[2];for(int i=0;i<2;i++){voices[i]=gameObject.AddComponent<AudioSource>();voices[i].loop=true;voices[i].playOnAwake=false;voices[i].spatialBlend=0;voices[i].volume=0;}}
        void Update()
        {
            if(!game||voices==null||game.testMode)return;
            if(Input.GetKeyDown(KeyCode.F5)){muted=!muted;PlayerPrefs.SetInt("GloomBean.MusicMuted",muted?1:0);}
            int theme=game.Session&&game.Session.definition.atlas?int.Parse(game.Session.definition.worldId.Substring(1)):game.IsCorrupted?1:0;
            if(game.Session&&game.Session.definition.course==1&&!game.IsCorrupted)theme=0;
            bool ending=game.CurrentScreen=="Ending";if(ending)theme=game.ShowingRestoredEnding?6:7;
            bool turn=!ending&&game.Session&&(game.Session.Phase==RunPhase.Returning||game.Session.definition.boss);
            int next=theme*2+(turn?1:0);if(key!=next){key=next;PlayingTheme=theme;int old=current;current=1-current;blend=0;if(!clips.TryGetValue(key,out var clip)){var pcm=Compose(theme,turn);clip=AudioClip.Create("Host Cycle original score "+key,pcm.Length,1,Rate,false);clip.SetData(pcm,0);clips[key]=clip;}voices[current].clip=clip;voices[current].Play();}
            blend=Mathf.Min(1,blend+Time.unscaledDeltaTime*.75f);float level=muted||fx&&fx.Muted?0:game.CurrentScreen=="Pause"?.05f:.22f;
            voices[current].volume=level*blend;voices[1-current].volume=level*(1-blend);if(blend>=1)voices[1-current].Stop();
        }
        void OnDestroy(){foreach(var c in clips.Values)if(c)Destroy(c);}
    }
}
