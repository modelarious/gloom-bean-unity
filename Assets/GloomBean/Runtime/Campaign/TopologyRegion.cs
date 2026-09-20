using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    // Two complementary physical collision domains, not an ability-labelled gate.
    // '#' = ordinary stone / interior air; '.' = ordinary air / interior boundary.
    // 'A' is an authored aperture shared by both domains.
    public sealed class TopologyRegion:MonoBehaviour
    {
        sealed class Cell {public char material;public Collider2D ordinary,inverse;}
        readonly Dictionary<Vector2Int,Cell> cells=new Dictionary<Vector2Int,Cell>();
        readonly List<Vector2Int> supplement=new List<Vector2Int>();
        readonly List<Vector2Int> supplementFloors=new List<Vector2Int>();
        public float cell=1;public bool closed=true;public readonly List<Collider2D> Inverse=new List<Collider2D>();
        public bool SupplementClosed {get;private set;}public int SupplementCount=>supplement.Count;
        StageBuilder builder;Vector2 origin;bool facadeSolid=true;
        Collider2D Geometry(Vector2Int at,bool ordinary)
        {
            var g=builder.Solid(ordinary?"Fresco stone":"Complement boundary",origin+(new Vector2(at.x,at.y)+Vector2.one*.5f)*cell,Vector2.one*cell,
                ordinary?new Color(.36f,.38f,.48f):new Color(.51f,.85f,.81f,.16f),ordinary?Layers.Terrain:Layers.Interior);
            var c=g.GetComponent<Collider2D>();if(!ordinary)Inverse.Add(c);return c;
        }
        void Refresh(Vector2Int at,Cell c)
        {
            if(c.material=='A')return;
            bool solid=c.material=='#';
            if(solid&&!c.ordinary)c.ordinary=Geometry(at,true);
            if(!solid&&!c.inverse)c.inverse=Geometry(at,false);
            if(c.ordinary)c.ordinary.gameObject.SetActive(solid&&facadeSolid);
            if(c.inverse){c.inverse.gameObject.SetActive(!solid);c.inverse.enabled=!solid&&closed;}
        }
        public void Build(string[] rows,StageBuilder b,Vector2 bottomLeft)
        {
            builder=b;origin=bottomLeft;
            for(int y=0;y<rows.Length;y++)for(int x=0;x<rows[y].Length;x++){
                var at=new Vector2Int(x,y);var c=new Cell{material=rows[rows.Length-1-y][x]};cells.Add(at,c);Refresh(at,c);
            }
        }
        public char MaterialAt(int x,int y)=>cells.TryGetValue(new Vector2Int(x,y),out var c)?c.material:'A';
        public bool Paint(int x,int y,bool stone)
        {
            var at=new Vector2Int(x,y);if(!cells.TryGetValue(at,out var c)||c.material=='A')return false;
            char next=stone?'#':'.';if(c.material==next)return true;c.material=next;Refresh(at,c);return true;
        }
        public void DefineSupplement(IEnumerable<Vector2Int> path){foreach(var p in path)if(cells.TryGetValue(p,out var c)&&c.material=='.'&&!supplement.Contains(p))supplement.Add(p);}
        public void DefineSupplementFloor(Vector2Int point){if(cells.TryGetValue(point,out var c)&&c.material=='#')supplementFloors.Add(point);}
        public void SetSupplement(bool value)
        {
            foreach(var p in supplement)Paint(p.x,p.y,value);foreach(var p in supplementFloors)Paint(p.x,p.y,!value);SupplementClosed=value;Physics2D.SyncTransforms();RuntimeEvents.Emit("paint-loop",value?"moon contour closed":"moon contour erased");
        }
        public void SetFacadeSolid(bool value){facadeSolid=value;foreach(var pair in cells)Refresh(pair.Key,pair.Value);RuntimeEvents.Emit("facade",value?"front":"back");}
        public void SetClosed(bool value){closed=value;foreach(var pair in cells)Refresh(pair.Key,pair.Value);RuntimeEvents.Emit("paint-loop",value?"closed":"open");}
    }
}
