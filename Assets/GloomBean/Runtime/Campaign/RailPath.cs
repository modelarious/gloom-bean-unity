using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class RailPath : MonoBehaviour
    {
        public Vector2 a,b;public bool loose;public float sag;
        public Vector2 Point(float t)=>Vector2.Lerp(a,b,t)+Vector2.down*(sag*Mathf.Sin(Mathf.PI*t)+(loose?Mathf.Sin(Time.time*1.8f)*.65f:0));
        public float Nearest(Vector2 p){var d=b-a;return d.sqrMagnitude<.01f?0:Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);}
        void Update(){PrimitiveArt.Line("Laundry rail",transform,Point(0),Point(1),.1f,new Color(.85f,.69f,.43f));}
    }
}