using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class ScriptureLayout:MonoBehaviour,IInteractable
    {
        public Transform[] words;public float spacing=3.6f,rowHeight=2.1f;public int wrap=5;public bool erasing;public Vector2 origin;readonly HashSet<int> stoodOn=new HashSet<int>();int previous=-1;readonly HashSet<int> erased=new HashSet<int>();
        public void Interact(ActorMotor a){wrap=wrap==5?3:5;RuntimeEvents.Emit("line-wrap",wrap.ToString());}
        void FixedUpdate()
        {
            if(words==null)return;for(int i=0;i<words.Length;i++)if(words[i]){var target=origin+new Vector2(i%wrap*spacing,i/wrap*rowHeight);words[i].position=Vector2.MoveTowards(words[i].position,target,Time.fixedDeltaTime*6);}
            var player=StageSession.Current?StageSession.Current.player:null;if(!player||!player.GroundCollider)return;
            int current=-1;for(int i=0;i<words.Length;i++)if(words[i]&&player.GroundCollider.transform==words[i])current=i;
            if(current!=previous){if(previous>=0&&erasing&&stoodOn.Contains(previous)&&!erased.Contains(previous)){var c=words[previous].GetComponent<Collider2D>();if(c)c.enabled=false;erased.Add(previous);var s=words[previous].GetComponent<SpriteRenderer>();if(s){var col=s.color;col.a=.18f;s.color=col;}}if(current>=0)stoodOn.Add(current);previous=current;}
        }
    }
}