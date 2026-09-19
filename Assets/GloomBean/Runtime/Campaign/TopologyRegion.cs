using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // A bounded, authored Boolean complement. '#' is normal solid/inverted air,
    // '.' is normal air/inverted solid, 'A' is a shared transition aperture.
    public sealed class TopologyRegion:MonoBehaviour
    {
        public float cell=1;public bool closed=true;public readonly List<Collider2D> Inverse=new List<Collider2D>();
        public void Build(string[] rows,StageBuilder b,Vector2 bottomLeft)
        {
            for(int y=0;y<rows.Length;y++)for(int x=0;x<rows[y].Length;x++)
            {
                char t=rows[rows.Length-1-y][x];if(t=='A')continue;
                var g=b.Solid(t=='#'?"Fresco stone":"Complement boundary",bottomLeft+new Vector2(x+.5f,y+.5f)*cell,Vector2.one*cell,t=='#'?new Color(.36f,.38f,.48f):new Color(.51f,.85f,.81f,.16f),t=='#'?Layers.Terrain:Layers.Interior);
                if(t!='#')Inverse.Add(g.GetComponent<Collider2D>());
            }
        }
        public void SetClosed(bool value){closed=value;foreach(var c in Inverse)if(c)c.enabled=value;RuntimeEvents.Emit("paint-loop",value?"closed":"open");}
    }
}