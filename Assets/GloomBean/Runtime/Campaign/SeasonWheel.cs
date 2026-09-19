using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class SeasonWheel:MonoBehaviour,IInteractable
    {
        public float width=12,offset,bandAngle;public bool drifting;public float speed=.7f;public Transform[] bands;
        public int Season(float x)=>((Mathf.FloorToInt((x-transform.position.x-offset)/width)%4)+4)%4;
        public int SeasonAt(Vector2 p){float r=bandAngle*Mathf.Deg2Rad;return Season(transform.position.x+Vector2.Dot(p-(Vector2)transform.position,new Vector2(Mathf.Cos(r),Mathf.Sin(r))));}
        public void Interact(ActorMotor a){if(!drifting)offset+=width;}
        void Update(){if(drifting)offset+=Time.deltaTime*speed;foreach(var s in RootSoil.All)if(s&&Vector2.Distance(s.transform.position,transform.position)<75)s.wet=SeasonAt(s.transform.position)==0||SeasonAt(s.transform.position)==2;if(bands!=null)for(int i=0;i<bands.Length;i++){var p=bands[i].position;p.x=transform.position.x+Mathf.Repeat(offset+i*width,width*4)-width*2;bands[i].position=p;}}
    }
}