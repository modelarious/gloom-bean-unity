using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GloomBean.Foundation
{
    [Serializable]
    public sealed class SaveData
    {
        public int version=1;
        public bool corrupted;
        public List<string> cleared=new List<string>(),mercies=new List<string>();
        public int totalCoins;
    }
    public sealed class SaveStore
    {
        public SaveData Data {get;private set;}
        readonly string path;
        public string LastError {get;private set;}
        public SaveStore(string file){path=file;Load();}
        public void Load()
        {
            LastError=null;Data=new SaveData();
            foreach(string candidate in new[]{path,path+".bak"})
            {
                try{if(!File.Exists(candidate))continue;var d=JsonUtility.FromJson<SaveData>(File.ReadAllText(candidate));if(d==null||d.version!=1||d.cleared==null||d.mercies==null)throw new InvalidDataException("Unsupported save");Data=d;return;}
                catch(Exception e){LastError=e.Message;}
            }
        }
        public bool Write()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string tmp=path+".tmp";File.WriteAllText(tmp,JsonUtility.ToJson(Data,true));
                if(File.Exists(path)){File.Copy(path,path+".bak",true);File.Replace(tmp,path,null);}else File.Move(tmp,path);
                LastError=null;return true;
            }
            catch(Exception e){LastError=e.Message;Debug.LogError("Save failed: "+e.Message);return false;}
        }
        public void MarkCorruption(){Data.corrupted=true;Write();}
        public void CommitRun(StageDefinition stage,int coins,IEnumerable<string> mercies)
        {
            if(!Data.cleared.Contains(stage.id))Data.cleared.Add(stage.id);
            foreach(var m in mercies)if(!Data.mercies.Contains(m))Data.mercies.Add(m);
            Data.totalCoins+=coins;Write();
        }
        public bool RestoredEnding=>Data.mercies.FindAll(m=>m.StartsWith("GB-L",StringComparison.Ordinal)).Count>=20;
    }


}
