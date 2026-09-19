using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class EnemyView : MonoBehaviour
    {
        CarryableEnemy enemy;Transform eyes;
        void Start()
        {
            enemy=GetComponent<CarryableEnemy>();
            var a=PrimitiveArt.Shape("Eyes",transform,Vector2.zero,new Vector2(.55f,.3f),new Color(1,.86f,.45f),PrimitiveArt.Icon.Eye,8);a.transform.localPosition=new Vector3(0,.18f,0);eyes=a.transform;
        }
        void LateUpdate(){if(!enemy||!eyes)return;eyes.localRotation=Quaternion.Euler(0,0,enemy.state==EnemyState.Stunned?Mathf.Sin(Time.time*9)*20:0);}
    }
}
