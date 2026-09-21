using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Render children only. The placement domain is every eligible physical support, not camera samples.
    [DefaultExecutionOrder(299)]
    public sealed class QualityBarConstruction:MonoBehaviour
    {
        public SpriteRenderer Source {get;private set;}
        public bool Applied=>Source&&pieces.Count>0;
        public int PieceCount=>pieces.Count;
        StageSession stage;Vector2 lastSize;int group=-1;readonly List<SpriteRenderer> pieces=new List<SpriteRenderer>();readonly List<Color> colors=new List<Color>();readonly List<SpriteRenderer> hidden=new List<SpriteRenderer>();
        public void Initialize(SpriteRenderer source){Source=source;stage=GetComponentInParent<StageSession>();Refresh();}
        public static int Room(StageDefinition s,int index,bool cute)
        {
            if(cute)return 0;if(s.boss)return new[]{1,5,8,10,9}[int.Parse(s.worldId.Substring(1))-1];
            switch(s.course){case 1:case 2:return 1;case 3:return 2;case 4:return 3;case 5:case 7:case 8:return 4;case 6:return 5;case 9:return index%2==0?6:7;case 10:return 7;case 11:return 8;case 12:return index%2==0?7:8;case 13:case 15:return 10;case 14:return 3;case 16:case 17:case 20:return 9;case 18:return 10;default:return 11;}
        }
        SpriteRenderer Add(string asset,Vector2 position,Vector2 size,int order,Color color,bool flip=false)
        {
            var sr=QualityBarArt.Picture(transform,"constructed "+asset,order);sr.sprite=QualityBarArt.Get(asset);QualityBarArt.Fit(sr,size);sr.transform.localPosition=new Vector3(position.x,position.y,.1f);sr.color=color;sr.flipX=flip;pieces.Add(sr);colors.Add(color);return sr;
        }
        void Refresh()
        {
            if(!Source||!Source.sprite||!stage)return;
            Vector2 size=Source.drawMode==SpriteDrawMode.Simple?(Vector2)Source.sprite.bounds.size:Source.size;int g=QualityBarArt.Group(stage);
            if(size!=lastSize||g!=group){
                foreach(var sr in pieces)if(sr)Destroy(sr.gameObject);foreach(var old in hidden)if(old)old.forceRenderingOff=false;pieces.Clear();colors.Clear();hidden.Clear();lastSize=size;group=g;
                bool horizontal=size.x>=2.5f&&size.y<size.x*.6f;
                bool ordinary=horizontal&&!GetComponent<EdibleChunk>()&&!GetComponent<DepthGeometry>()&&!GetComponent<MagneticBody>()&&!GetComponent<ProcessionCarrier>()&&!Source.name.ToLowerInvariant().Contains("skin")&&!Source.name.ToLowerInvariant().Contains("sheet");
                if(ordinary){
                    if(size.y<1.2f){int braces=Mathf.Clamp(Mathf.CeilToInt(size.x/5),1,24);float width=size.x/braces;
                        for(int i=0;i<braces;i++){float x=(i+.5f)*width-size.x*.5f;Add("construction_brace_"+group,new Vector2(x,-size.y*.5f-.8f),new Vector2(Mathf.Min(width,5),2.2f),-9,new Color(.85f,.78f,.87f,.95f),i%2!=0);}
                        foreach(Transform child in transform)if(child.name=="QualityBar visual / load-bearing rear truss"){var old=child.GetComponent<SpriteRenderer>();if(old){hidden.Add(old);old.forceRenderingOff=true;}}
                    }
                    bool stable=!GetComponent<Rigidbody2D>();bool floor=Source.name=="Floor";
                    if(stable&&((floor&&size.x>=5)||size.x>=7.8f)){
                        int count=Mathf.Clamp(Mathf.CeilToInt(size.x/10),1,40);float span=size.x/count;
                        for(int i=0;i<count;i++){int theme=Room(stage.definition,i+transform.GetSiblingIndex(),g==0);float width=Mathf.Min(10,span+.12f),height=width*112/128f;float x=(i+.5f)*span-size.x*.5f;
                            Add("construction_room_"+theme,new Vector2(x,size.y*.5f+height*.5f),new Vector2(width,height),-27,new Color(.70f,.65f,.75f,1));}
                        foreach(Transform child in transform)if(child.name=="Broad visual / level architecture bay"){var old=child.GetComponent<SpriteRenderer>();if(old){hidden.Add(old);old.forceRenderingOff=true;}}
                    }
                }
            }
            for(int i=0;i<pieces.Count;i++)if(pieces[i]){pieces[i].enabled=Source.enabled;var color=colors[i];color.a*=Source.color.a;pieces[i].color=color;}
        }
        void LateUpdate()=>Refresh();
        void OnDestroy(){foreach(var old in hidden)if(old)old.forceRenderingOff=false;}
    }
    // Every real rail has the same structural treatment, including rails created after entry.
    [DefaultExecutionOrder(303)]
    public sealed class QualityBarRailConstruction:MonoBehaviour
    {
        RailPath rail;SpriteRenderer[] segments,weights,housings;float length=-1;
        public void Initialize(RailPath path){rail=path;Build();}
        void Build(){if(!rail)return;float span=Vector2.Distance(rail.a,rail.b);if(Mathf.Abs(span-length)<.01f)return;length=span;
            if(segments!=null)foreach(var old in segments)if(old)Destroy(old.gameObject);if(weights!=null)foreach(var old in weights)if(old)Destroy(old.gameObject);if(housings!=null)foreach(var old in housings)if(old)Destroy(old.gameObject);
            int count=Mathf.Clamp(Mathf.CeilToInt(span/3.5f),1,36);segments=new SpriteRenderer[count];weights=new SpriteRenderer[count];housings=new SpriteRenderer[2];
            for(int i=0;i<count;i++){segments[i]=QualityBarArt.Picture(transform,"rail girder span "+i,-11);segments[i].sprite=QualityBarArt.Get("construction_girder");weights[i]=QualityBarArt.Picture(transform,"hanging rail garment "+i,-20);weights[i].sprite=QualityBarArt.Get("construction_hanging_"+i%4);QualityBarArt.Fit(weights[i],new Vector2(1.2f,1.8f));weights[i].color=new Color(.60f,.53f,.63f,.85f);}
            for(int i=0;i<2;i++){housings[i]=QualityBarArt.Picture(transform,"actual rail winch "+i,1);housings[i].sprite=QualityBarArt.Get("construction_winch");QualityBarArt.Fit(housings[i],new Vector2(1.35f,1.575f));}
        }
        void LateUpdate(){if(!rail)return;Build();if(segments==null)return;bool active=rail.enabled;
            for(int i=0;i<segments.Length;i++){float t=(i+.5f)/segments.Length;Vector2 a=rail.Point(i/(float)segments.Length),b=rail.Point((i+1)/(float)segments.Length),d=b-a;segments[i].transform.position=(a+b)*.5f+Vector2.up*.45f;segments[i].transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg);QualityBarArt.Fit(segments[i],new Vector2(d.magnitude+.06f,.58f));segments[i].enabled=active;
                weights[i].transform.position=rail.Point(t)+Vector2.down*1.6f;weights[i].transform.rotation=Quaternion.Euler(0,0,Mathf.Sin(Time.time*1.2f+i)*3);weights[i].enabled=active;}
            for(int i=0;i<2;i++){housings[i].transform.position=rail.Point(i)+Vector2.up*.04f;housings[i].transform.rotation=Quaternion.identity;housings[i].enabled=active;}
            foreach(Transform child in transform)if(child.name.StartsWith("Broad visual / rail bearing")||child.name.StartsWith("QualityBar visual / pulley housing")){var sr=child.GetComponent<SpriteRenderer>();if(sr)sr.forceRenderingOff=true;}
        }
    }
}
