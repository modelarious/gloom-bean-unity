using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign {public sealed class HostileProjectile:MonoBehaviour {void OnCollisionEnter2D(Collision2D c){var a=c.collider.GetComponent<ActorMotor>();if(a)a.Hit(new HitInfo(null,Vector2.left,1));Destroy(gameObject);}}}