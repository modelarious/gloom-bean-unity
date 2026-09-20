using UnityEngine;
namespace GloomBean.Campaign
{
    // A severed span drifts as one physical object; Censer changes its clock, not the player's.
    [DefaultExecutionOrder(-110)]
    public sealed class StructuralDrift:MonoBehaviour
    {
        public bool released;public Vector2 direction=Vector2.right;public float amplitude=1.2f,period=12,phase;
        public float TimeScale {get;private set;}=1;Vector2 origin;Rigidbody2D body;float clock;
        void Start(){origin=transform.position;body=GetComponent<Rigidbody2D>();}
        void FixedUpdate(){if(!released)return;TimeScale=LocalTime.Scale(body?body.position:(Vector2)transform.position);clock+=Time.fixedDeltaTime*TimeScale;
            Vector2 next=origin+direction*(Mathf.Sin(clock*2*Mathf.PI/period+phase)-Mathf.Sin(phase))*amplitude;
            if(body)body.MovePosition(next);else transform.position=next;}
    }
}
