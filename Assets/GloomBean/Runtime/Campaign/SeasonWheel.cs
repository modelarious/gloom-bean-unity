using System;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class SeasonWheel:MonoBehaviour,IInteractable
    {
        public float width=12,offset,bandAngle;public bool drifting;public float speed=.7f;public Transform[] bands;
        Color[] colors;
        public int Season(float x)=>((Mathf.FloorToInt((x-transform.position.x-offset)/width)%4)+4)%4;
        public int SeasonAt(Vector2 p){float r=bandAngle*Mathf.Deg2Rad;return Season(transform.position.x+Vector2.Dot(p-(Vector2)transform.position,new Vector2(Mathf.Cos(r),Mathf.Sin(r))));}
        public void Interact(ActorMotor a){if(!drifting)offset+=width;}
        void Start()
        {
            if(bands==null||bands.Length<4)return;colors=new Color[4];for(int i=0;i<4;i++)colors[i]=bands[i].GetComponent<SpriteRenderer>().color;
            var source=bands[0];Array.Resize(ref bands,10);for(int i=4;i<bands.Length;i++)bands[i]=Instantiate(source,source.parent);
        }
        void Update()
        {
            if(drifting)offset+=Time.deltaTime*speed;
            foreach(var soil in RootSoil.All)if(soil&&Vector2.Distance(soil.transform.position,transform.position)<75)soil.wet=SeasonAt(soil.transform.position)==0||SeasonAt(soil.transform.position)==2;
            if(colors==null||bands==null)return;
            float radians=bandAngle*Mathf.Deg2Rad;Vector2 axis=new Vector2(Mathf.Cos(radians),Mathf.Sin(radians));
            float left=StageSession.Current&&StageSession.Current.Camera?StageSession.Current.Camera.bounds.xMin:-20;
            int first=Mathf.FloorToInt((left-transform.position.x-offset)/width)-1;
            for(int i=0;i<bands.Length;i++)if(bands[i]){
                int cell=first+i;Vector2 center=(Vector2)transform.position+axis*(offset+(cell+.5f)*width);center.y+=9;
                bands[i].position=center;bands[i].rotation=Quaternion.Euler(0,0,bandAngle);
                var sprite=bands[i].GetComponent<SpriteRenderer>();sprite.color=colors[((cell%4)+4)%4];
            }
        }
    }
}
