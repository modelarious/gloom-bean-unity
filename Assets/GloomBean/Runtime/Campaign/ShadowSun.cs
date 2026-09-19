using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class ShadowSun:MonoBehaviour
    {
        public static readonly List<ShadowSun> All=new List<ShadowSun>();public float reach=17;public Vector2 orbit;public float speed=.25f;public bool moving;
        public readonly List<Vector2[]> Polygons=new List<Vector2[]>();Vector2 origin;readonly List<LineRenderer> outlines=new List<LineRenderer>();
        void OnEnable(){All.Add(this);origin=transform.position;}void OnDisable(){All.Remove(this);}
        public static bool Inside(Vector2 p,Vector2[] v){bool yes=false;for(int i=0,j=v.Length-1;i<v.Length;j=i++){if((v[i].y>p.y)!=(v[j].y>p.y)&&p.x<(v[j].x-v[i].x)*(p.y-v[i].y)/(v[j].y-v[i].y)+v[i].x)yes=!yes;}return yes;}
        public static bool Contains(Vector2 p){foreach(var d in ShadowDomain.All)if(d&&d.area.Contains(p))return d.Allows(p);foreach(var sun in All)if(sun)foreach(var poly in sun.Polygons)if(Inside(p,poly))return true;return false;}
        void FixedUpdate(){if(moving)transform.position=origin+new Vector2(Mathf.Sin(Time.time*speed)*orbit.x,Mathf.Cos(Time.time*speed)*orbit.y);Rebuild();}
        public void Rebuild()
        {
            Polygons.Clear();Vector2 light=transform.position;
            foreach(var c in ShadowCaster.All)
            {
                if(!c||!c.shape||!c.shape.enabled||Vector2.Distance(c.transform.position,light)>25)continue;var box=c.shape.bounds;
                Vector2 center=box.center;if(Vector2.Distance(center,light)<.1f)continue;
                Vector2[] corners={new Vector2(box.min.x,box.min.y),new Vector2(box.min.x,box.max.y),new Vector2(box.max.x,box.min.y),new Vector2(box.max.x,box.max.y)};
                float axis=Mathf.Atan2(center.y-light.y,center.x-light.x)*Mathf.Rad2Deg,min=999,max=-999;Vector2 a=center,b=center;
                foreach(var q in corners){float ang=Mathf.DeltaAngle(axis,Mathf.Atan2(q.y-light.y,q.x-light.x)*Mathf.Rad2Deg);if(ang<min){min=ang;a=q;}if(ang>max){max=ang;b=q;}}
                Vector2 farA=a+(a-light).normalized*reach,farB=b+(b-light).normalized*reach;
                Polygons.Add(new[]{a,b,farB,farA});
            }
            for(int i=0;i<Polygons.Count;i++)
            {
                if(i>=outlines.Count)outlines.Add(PrimitiveArt.Line("Shadow projection "+i,transform,Vector2.zero,Vector2.zero,.045f,new Color(.34f,.32f,.53f,.65f),2));
                var l=outlines[i];l.enabled=true;l.positionCount=5;for(int j=0;j<5;j++)l.SetPosition(j,Polygons[i][j%4]);
            }
            for(int i=Polygons.Count;i<outlines.Count;i++)outlines[i].enabled=false;
        }
    }
}