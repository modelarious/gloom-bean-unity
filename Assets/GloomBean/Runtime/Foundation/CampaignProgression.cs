using System;
using System.Linq;
namespace GloomBean.Foundation
{
    /// <summary>The same progression predicates drive the selection UI and acceptance checks.</summary>
    public static class CampaignProgression
    {
        public static bool WorldOpen(WorldDefinition[] worlds,int index,SaveData save,bool practice=false)
        {
            if(worlds==null||index<0||index>=worlds.Length)return false;
            return practice||index==0||(save!=null&&save.cleared.Contains(worlds[index-1].boss.id));
        }
        public static bool LevelOpen(WorldDefinition world,int index,SaveData save,bool practice=false)
        {
            if(world==null||index<0||index>=world.levels.Length)return false;
            return practice||index==0||(save!=null&&save.cleared.Contains(world.levels[index-1].id));
        }
        public static bool BossOpen(WorldDefinition world,SaveData save,bool practice=false)
        {
            return world!=null&&(practice||(save!=null&&world.levels.All(level=>save.cleared.Contains(level.id))));
        }
    }
}
