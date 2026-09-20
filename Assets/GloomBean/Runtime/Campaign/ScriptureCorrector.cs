using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // A manuscript remembers physical outbound paths, then erases ink retracing them.
    public sealed class ScriptureCorrector:MonoBehaviour
    {
        public bool imperative;public int ErasedStrokes {get;private set;}
        readonly HashSet<Vector2Int> oldPath=new HashSet<Vector2Int>();
        static Vector2Int Cell(Vector2 p)=>new Vector2Int(Mathf.RoundToInt(p.x/.65f),Mathf.RoundToInt(p.y/.65f));
        public bool Repeated(Vector2 p)=>oldPath.Contains(Cell(p));
        void FixedUpdate()
        {
            var s=StageSession.Current;if(!s||!s.player)return;
            if(!imperative){oldPath.Add(Cell(s.player.Feet));return;}
            foreach(var stroke in s.GetComponentsInChildren<InkStroke>())if(stroke&&stroke.age>.3f&&Repeated(stroke.Midpoint)){stroke.Erase();ErasedStrokes++;}
        }
    }
}
