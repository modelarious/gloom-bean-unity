using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign { public sealed class MassWindow:MonoBehaviour {public PressurePlate plate;public Gate gate;public float minimum=.6f,maximum=.85f;public bool Valid=>plate&&plate.Mass>=minimum&&plate.Mass<=maximum;void FixedUpdate(){if(gate)gate.SetOpen(Valid);}} }