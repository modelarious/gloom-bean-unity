using UnityEngine;
namespace GloomBean.Foundation
{
    /// <summary>A thin landing surface, solid from above but not a low ceiling from below.</summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class OneWaySurface:MonoBehaviour
    {
        void Awake(){var c=GetComponent<BoxCollider2D>();c.usedByEffector=true;var e=GetComponent<PlatformEffector2D>();if(!e)e=gameObject.AddComponent<PlatformEffector2D>();e.useOneWay=true;e.useOneWayGrouping=true;e.surfaceArc=160;e.useSideFriction=false;e.useSideBounce=false;}
    }
}
