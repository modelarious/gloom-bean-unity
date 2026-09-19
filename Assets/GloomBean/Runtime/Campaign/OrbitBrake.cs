using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign {public sealed class OrbitBrake:MonoBehaviour {public PressurePlate left,right;public RoomOrbit[] rooms;public bool released;void FixedUpdate(){if(left&&right&&left.Pressed&&right.Pressed)released=true;if(released&&rooms!=null)foreach(var r in rooms)if(r)r.running=true;}}}