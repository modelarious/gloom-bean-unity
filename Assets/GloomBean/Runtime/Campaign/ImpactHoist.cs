using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    [DefaultExecutionOrder(-110)]
    public sealed class ImpactHoist:MonoBehaviour
    {
        public DescentController frame;public FallingLoad load;public float rise=14,speed=6,lifted;public GameObject releaseCure;public GameObject[] arrivalObjects=System.Array.Empty<GameObject>();Rigidbody2D body;Vector2 origin;
        public bool Arrived=>lifted>=rise-.01f;
        void Start(){body=GetComponent<Rigidbody2D>();origin=body.position-(frame?frame.Offset:Vector2.zero);}
        void FixedUpdate(){if(load&&load.impacted)lifted=Mathf.MoveTowards(lifted,rise,speed*Time.fixedDeltaTime);
            body.MovePosition(origin+(frame?frame.Offset:Vector2.zero)+Vector2.up*lifted);
            if(Arrived){if(releaseCure)releaseCure.SetActive(true);foreach(var obj in arrivalObjects)if(obj)obj.SetActive(true);}}
    }
}
