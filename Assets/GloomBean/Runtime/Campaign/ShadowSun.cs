using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed class ShadowSun:MonoBehaviour
    {
        public static readonly List<ShadowSun> All=new List<ShadowSun>();
        public float reach=17,speed=.25f;public Vector2 orbit;public bool moving,directional,renderFilled;
        public Vector2 direction=new Vector2(1,-1);public bool noon;public float sweep=.85f;
        public readonly List<Vector2[]> Polygons=new List<Vector2[]>();
        public readonly List<ShadowCaster> Casters=new List<ShadowCaster>();
        Vector2 origin;float clock;readonly List<LineRenderer> outlines=new List<LineRenderer>();readonly List<Mesh> meshes=new List<Mesh>();readonly List<GameObject> fills=new List<GameObject>();Material fillMaterial;
        void OnEnable(){if(!All.Contains(this))All.Add(this);origin=transform.position;}void OnDisable(){All.Remove(this);}
        public static bool Inside(Vector2 p,Vector2[] v){bool yes=false;for(int i=0,j=v.Length-1;i<v.Length;j=i++){if((v[i].y>p.y)!=(v[j].y>p.y)&&p.x<(v[j].x-v[i].x)*(p.y-v[i].y)/(v[j].y-v[i].y)+v[i].x)yes=!yes;}return yes;}
        public static bool Contains(Vector2 p){foreach(var d in ShadowDomain.All)if(d&&d.area.Contains(p))return d.Allows(p);foreach(var sun in All)if(sun)foreach(var poly in sun.Polygons)if(Inside(p,poly))return true;return false;}
        public bool CastBy(Vector2 p,Collider2D source){for(int i=0;i<Polygons.Count;i++)if(Casters[i]&&Casters[i].shape==source&&Inside(p,Polygons[i]))return true;return false;}
        public void LockNoon(){noon=true;moving=false;direction=Vector2.down;Rebuild();}
        void FixedUpdate(){clock+=Time.fixedDeltaTime;if(directional){if(noon)direction=Vector2.down;else if(moving)direction=new Vector2(Mathf.Sin(clock*speed)*sweep,-1).normalized;}else if(moving)transform.position=origin+new Vector2(Mathf.Sin(clock*speed)*orbit.x,Mathf.Cos(clock*speed)*orbit.y);Rebuild();}
        public void Rebuild()
        {
            Polygons.Clear();Casters.Clear();Vector2 light=transform.position;
            foreach(var c in ShadowCaster.All)
            {
                if(!c||!c.isActiveAndEnabled||!c.shape||!c.shape.enabled||!c.shape.gameObject.activeInHierarchy||(!directional&&Vector2.Distance(c.shape.bounds.center,light)>25))continue;
                var outline=ShadowGeometry.Outline(c.shape);if(outline.Length<3)continue;
                var polygon=directional?ShadowGeometry.Parallel(outline,direction,reach):ShadowGeometry.Point(outline,light,reach);
                Polygons.Add(polygon);Casters.Add(c);
            }
            for(int i=0;i<Polygons.Count;i++)
            {
                var poly=Polygons[i];
                if(i>=outlines.Count)outlines.Add(PrimitiveArt.Line("Shadow projection "+i,transform,Vector2.zero,Vector2.zero,.035f,new Color(.34f,.32f,.53f,.65f),2));
                var l=outlines[i];l.enabled=true;l.positionCount=poly.Length+1;for(int j=0;j<=poly.Length;j++)l.SetPosition(j,poly[j%poly.Length]);
                if(renderFilled){
                    if(!fillMaterial)fillMaterial=new Material(Shader.Find("Sprites/Default"));
                    while(fills.Count<=i){var g=new GameObject("Cast silhouette");g.transform.SetParent(transform);var mesh=new Mesh();g.AddComponent<MeshFilter>().sharedMesh=mesh;var mr=g.AddComponent<MeshRenderer>();mr.sharedMaterial=fillMaterial;mr.sortingOrder=1;fills.Add(g);meshes.Add(mesh);}
                    fills[i].SetActive(true);var verts=new Vector3[poly.Length];var colors=new Color[poly.Length];var indices=new int[(poly.Length-2)*3];
                    for(int j=0;j<poly.Length;j++){verts[j]=fills[i].transform.InverseTransformPoint(new Vector3(poly[j].x,poly[j].y,0));colors[j]=new Color(.12f,.10f,.22f,.32f);}
                    for(int j=0;j<poly.Length-2;j++){indices[j*3]=0;indices[j*3+1]=j+1;indices[j*3+2]=j+2;}
                    var m=meshes[i];m.Clear();m.vertices=verts;m.colors=colors;m.triangles=indices;m.RecalculateBounds();
                }
            }
            for(int i=Polygons.Count;i<outlines.Count;i++)outlines[i].enabled=false;
            for(int i=renderFilled?Polygons.Count:0;i<fills.Count;i++)fills[i].SetActive(false);
        }
        void OnDestroy(){foreach(var m in meshes)if(m)Destroy(m);if(fillMaterial)Destroy(fillMaterial);}
    }
}
