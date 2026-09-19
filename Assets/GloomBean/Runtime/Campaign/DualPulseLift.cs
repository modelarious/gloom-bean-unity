using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign { public sealed class DualPulseLift:MonoBehaviour {public PulseReceiver a,b;public MotionPlatform lift;public bool latched;void FixedUpdate(){if(a&&b&&a.Until>Time.time&&b.Until>Time.time&&Mathf.Abs(a.Until-b.Until)<1.1f)latched=true;if(lift)lift.paused=!latched;}} }