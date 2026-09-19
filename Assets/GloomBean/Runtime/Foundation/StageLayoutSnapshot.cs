using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GloomBean.Foundation
{
    [Serializable] public sealed class LayoutChange
    {
        public int childIndex;
        public string objectName;
        public Vector2 position,size;
        public float angle;
    }
    [Serializable] public sealed class LayoutPatch
    {
        public int schemaVersion=1;
        public string stageId,sourceFingerprint;
        public List<LayoutChange> changes=new List<LayoutChange>();
    }
    /// <summary>
    /// Bounded authoring overlays for static box geometry. Gameplay scripts, dynamic bodies,
    /// machine anchors, sources and gates are deliberately excluded. An incompatible source
    /// rejects the entire patch before any object is mutated.
    /// </summary>
    public sealed class StageLayoutSnapshot:MonoBehaviour
    {
        public sealed class Node
        {
            public int index;public string name;public Transform target;public BoxCollider2D box;
            public Vector2 originalPosition,originalSize,originalOffset;public float originalAngle;
        }
        public string StageId {get;private set;}
        public string Fingerprint {get;private set;}
        public IReadOnlyList<Node> Nodes=>nodes;
        readonly List<Node> nodes=new List<Node>();int capturedChildCount;
        static bool Finite(float x)=>!float.IsNaN(x)&&!float.IsInfinity(x);
        static bool Near(Vector2 a,Vector2 b)=>(a-b).sqrMagnitude<.000001f;
        public static StageLayoutSnapshot Install(string id,Transform root,bool loadResource=true)
        {
            var snapshot=root.gameObject.AddComponent<StageLayoutSnapshot>();snapshot.Capture(id);
            if(loadResource){var asset=Resources.Load<TextAsset>("LevelLayouts/"+id);if(asset){
                try{var patch=JsonUtility.FromJson<LayoutPatch>(asset.text);if(!snapshot.Apply(patch,out string why)){
                    Debug.LogWarning("Layout override rejected for "+id+": "+why);root.GetComponent<StageSession>()?.Notice("Layout override rejected: "+why,8);
                }}catch(Exception e){Debug.LogWarning("Invalid layout override for "+id+": "+e.Message);}
            }}return snapshot;
        }
        public void Capture(string id)
        {
            StageId=id;nodes.Clear();capturedChildCount=transform.childCount;
            for(int i=0;i<transform.childCount;i++){
                var t=transform.GetChild(i);var box=t.GetComponent<BoxCollider2D>();
                if(!box||box.isTrigger||box.attachedRigidbody||!Near(t.localScale,Vector2.one)||Mathf.Abs(t.localScale.z-1)>.001f)continue;
                if(t.GetComponents<MonoBehaviour>().Any(m=>m&&!(m is OneWaySurface)))continue;
                if(t.GetComponent<PolygonCollider2D>())continue;
                nodes.Add(new Node{index=i,name=t.name,target=t,box=box,originalPosition=t.position,originalSize=box.size,originalOffset=box.offset,originalAngle=t.eulerAngles.z});
            }
            var text=new StringBuilder(id);var inv=CultureInfo.InvariantCulture;
            foreach(var n in nodes){text.Append('|').Append(n.index).Append(':').Append(n.name).Append(':').Append(n.target.gameObject.layer);
                foreach(float v in new[]{n.originalPosition.x,n.originalPosition.y,n.originalSize.x,n.originalSize.y,n.originalAngle,n.box.offset.x,n.box.offset.y})text.Append(':').Append(v.ToString("R",inv));}
            using(var hash=SHA256.Create())Fingerprint=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-","").ToLowerInvariant();
        }
        public LayoutPatch Export()
        {
            if(transform.childCount!=capturedChildCount)throw new InvalidOperationException("The live hierarchy changed. Reload with Edit layout before saving; newly added objects are not silently discarded.");
            var patch=new LayoutPatch{stageId=StageId,sourceFingerprint=Fingerprint};
            foreach(var n in nodes){
                if(!n.target||!n.box||n.target.name!=n.name||n.target.parent!=transform||n.target.GetSiblingIndex()!=n.index)throw new InvalidOperationException("An authored object was removed or renamed. Reload the course before editing its layout.");
                if(!Near(n.box.offset,n.originalOffset)||Mathf.Abs(Mathf.DeltaAngle(n.target.eulerAngles.x,0))>.001f||Mathf.Abs(Mathf.DeltaAngle(n.target.eulerAngles.y,0))>.001f)throw new InvalidOperationException("This overlay supports XY position/size and Z rotation, not collider offsets or 3D tilt.");
                Vector3 scale=n.target.localScale;
                if(scale.x<=0||scale.y<=0||Mathf.Abs(scale.z-1)>.001f)throw new InvalidOperationException("Use positive XY scales and leave Z scale at one.");
                Vector2 position=n.target.position,size=Vector2.Scale(n.box.size,scale);float angle=n.target.eulerAngles.z;
                if(!Near(position,n.originalPosition)||!Near(size,n.originalSize)||Mathf.Abs(Mathf.DeltaAngle(angle,n.originalAngle))>.001f)
                    patch.changes.Add(new LayoutChange{childIndex=n.index,objectName=n.name,position=position,size=size,angle=angle});
            }
            if(!Validate(patch,out string error))throw new InvalidOperationException(error);return patch;
        }
        public bool Validate(LayoutPatch patch,out string reason)
        {
            reason="";
            if(patch==null||patch.schemaVersion!=1||patch.stageId!=StageId){reason="Wrong schema or stage.";return false;}
            if(patch.sourceFingerprint!=Fingerprint){reason="Builder geometry changed. Rebase the overlay in the editor; no old changes were applied.";return false;}
            if(patch.changes==null||patch.changes.Count>2048){reason="Invalid change list.";return false;}
            var seen=new HashSet<int>();
            foreach(var c in patch.changes){
                if(c==null||!seen.Add(c.childIndex)){reason="Duplicate or missing object change.";return false;}
                var n=nodes.Find(x=>x.index==c.childIndex);
                if(n==null||!n.target||n.name!=c.objectName){reason="Object identity mismatch.";return false;}
                if(!Finite(c.position.x)||!Finite(c.position.y)||!Finite(c.size.x)||!Finite(c.size.y)||!Finite(c.angle)||
                    Mathf.Abs(c.position.x)>10000||Mathf.Abs(c.position.y)>10000||c.size.x<.05f||c.size.y<.05f||c.size.x>1000||c.size.y>1000){reason="Non-finite or out-of-range geometry.";return false;}
            }
            return true;
        }
        public bool Apply(LayoutPatch patch,out string reason)
        {
            if(!Validate(patch,out reason))return false; // Atomic validation: no partially applied overlays.
            foreach(var c in patch.changes){var n=nodes.Find(x=>x.index==c.childIndex);var z=n.target.position.z;
                n.target.localScale=Vector3.one;n.target.position=new Vector3(c.position.x,c.position.y,z);n.target.rotation=Quaternion.Euler(0,0,c.angle);n.box.size=c.size;
                var sprite=n.target.GetComponent<SpriteRenderer>();if(sprite)sprite.size=c.size;
            }
            Physics2D.SyncTransforms();RuntimeEvents.Emit("layout-applied",StageId+":"+patch.changes.Count);return true;
        }
    }
}
