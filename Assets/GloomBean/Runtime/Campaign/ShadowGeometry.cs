using System.Collections.Generic;
using UnityEngine;
namespace GloomBean.Campaign
{
    // Convex shadow silhouettes use the actual collider footprint, not its axis-aligned bounds.
    public static class ShadowGeometry
    {
        static Vector2 PhysicalPoint(Collider2D shape,Vector2 local)
        {
            Vector3 world=shape.transform.TransformPoint(local);var rb=shape.attachedRigidbody;
            if(!rb)return world;
            Vector3 relative=Quaternion.Inverse(rb.transform.rotation)*(world-rb.transform.position);
            return rb.position+(Vector2)(Quaternion.Euler(0,0,rb.rotation)*relative);
        }
        public static Vector2[] Outline(Collider2D shape)
        {
            var points=new List<Vector2>();
            if(shape is BoxCollider2D box)
            {var h=box.size*.5f;foreach(var p in new[]{new Vector2(-h.x,-h.y),new Vector2(h.x,-h.y),new Vector2(h.x,h.y),new Vector2(-h.x,h.y)})points.Add(PhysicalPoint(shape,box.offset+p));}
            else if(shape is PolygonCollider2D poly)
            {for(int path=0;path<poly.pathCount;path++)foreach(var p in poly.GetPath(path))points.Add(PhysicalPoint(shape,p+poly.offset));}
            else if(shape is CircleCollider2D circle)
            {for(int i=0;i<24;i++){float a=i*Mathf.PI/12;points.Add(PhysicalPoint(shape,circle.offset+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*circle.radius));}}
            else if(shape is CapsuleCollider2D capsule)
            {bool vertical=capsule.direction==CapsuleDirection2D.Vertical;float r=(vertical?capsule.size.x:capsule.size.y)*.5f,stem=Mathf.Max(0,(vertical?capsule.size.y:capsule.size.x)*.5f-r);
             for(int i=0;i<24;i++){float a=i*Mathf.PI/12;var v=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;v+=vertical?new Vector2(0,Mathf.Sign(v.y)*stem):new Vector2(Mathf.Sign(v.x)*stem,0);points.Add(PhysicalPoint(shape,capsule.offset+v));}}
            else {var b=shape.bounds;points.Add(b.min);points.Add(new Vector2(b.max.x,b.min.y));points.Add(b.max);points.Add(new Vector2(b.min.x,b.max.y));}
            return Hull(points);
        }
        static float Cross(Vector2 a,Vector2 b,Vector2 c)=>(b.x-a.x)*(c.y-a.y)-(b.y-a.y)*(c.x-a.x);
        public static Vector2[] Hull(List<Vector2> points)
        {
            points.Sort((a,b)=>a.x==b.x?a.y.CompareTo(b.y):a.x.CompareTo(b.x));var sorted=new List<Vector2>();foreach(var p in points)if(sorted.Count==0||(p-sorted[sorted.Count-1]).sqrMagnitude>.000001f)sorted.Add(p);
            if(sorted.Count<3)return sorted.ToArray();var hull=new List<Vector2>();
            foreach(var p in sorted){while(hull.Count>=2&&Cross(hull[hull.Count-2],hull[hull.Count-1],p)<=0)hull.RemoveAt(hull.Count-1);hull.Add(p);}int lower=hull.Count;
            for(int i=sorted.Count-2;i>=0;i--){var p=sorted[i];while(hull.Count>lower&&Cross(hull[hull.Count-2],hull[hull.Count-1],p)<=0)hull.RemoveAt(hull.Count-1);hull.Add(p);}hull.RemoveAt(hull.Count-1);return hull.ToArray();
        }
        static Vector2[] HalfPlane(Vector2[] input,int axis,float boundary,bool greater)
        {
            var output=new List<Vector2>();if(input.Length<3)return output.ToArray();
            Vector2 previous=input[input.Length-1];float pd=((axis==0?previous.x:previous.y)-boundary)*(greater?1:-1);
            foreach(var current in input){float cd=((axis==0?current.x:current.y)-boundary)*(greater?1:-1);bool pi=pd>=0,ci=cd>=0;
                if(pi!=ci)output.Add(Vector2.LerpUnclamped(previous,current,pd/(pd-cd)));if(ci)output.Add(current);previous=current;pd=cd;}
            return output.ToArray();
        }
        public static Vector2[] ClipRect(Vector2[] polygon,Rect r)
        {polygon=HalfPlane(polygon,0,r.xMin,true);polygon=HalfPlane(polygon,0,r.xMax,false);polygon=HalfPlane(polygon,1,r.yMin,true);return HalfPlane(polygon,1,r.yMax,false);}
        public static List<Vector2[]> SubtractRect(Vector2[] polygon,Rect r)
        {
            var result=new List<Vector2[]>();int[] axes={0,0,1,1};float[] bounds={r.xMin,r.xMax,r.yMin,r.yMax};bool[] sides={true,false,true,false};
            for(int i=0;i<4&&polygon.Length>=3;i++){var outside=HalfPlane(polygon,axes[i],bounds[i],!sides[i]);if(outside.Length>=3)result.Add(outside);polygon=HalfPlane(polygon,axes[i],bounds[i],sides[i]);}
            return result;
        }
        public static Vector2[] Parallel(Vector2[] outline,Vector2 direction,float reach)
        {var points=new List<Vector2>(outline);foreach(var p in outline)points.Add(p+direction.normalized*reach);return Hull(points);}
        public static Vector2[] Point(Vector2[] outline,Vector2 light,float reach)
        {
            if(outline.Length<3)return outline;Vector2 center=Vector2.zero;foreach(var p in outline)center+=p;center/=outline.Length;
            float axis=Mathf.Atan2(center.y-light.y,center.x-light.x)*Mathf.Rad2Deg,min=999,max=-999;Vector2 a=center,b=center;
            foreach(var q in outline){float angle=Mathf.DeltaAngle(axis,Mathf.Atan2(q.y-light.y,q.x-light.x)*Mathf.Rad2Deg);if(angle<min){min=angle;a=q;}if(angle>max){max=angle;b=q;}}
            return new[]{a,b,b+(b-light).normalized*reach,a+(a-light).normalized*reach};
        }
    }
}
