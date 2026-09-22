using System;
using System.Collections.Generic;
using UnityEngine;
using GloomBean.Foundation;
using GloomBean.Campaign;

namespace GloomBean.Grammar
{
    [Serializable] public sealed class GrammarVec { public float x,y; public Vector2 Value=>new Vector2(x,y); }
    [Serializable] public sealed class GrammarRect { public float x,y,w,h; public Rect Value=>new Rect(x,y,w,h); }
    [Serializable] public sealed class GrammarSurface
    {
        public string role,kind; public float x,y,w; public bool returnOnly,deactivateOnTurn;
    }
    [Serializable] public sealed class GrammarHazard { public float x,y,width; }
    [Serializable] public sealed class GrammarEnemy { public float x,y; public int direction; }
    [Serializable] public sealed class GrammarCoinLine { public float ax,ay,bx,by; public int count; }
    [Serializable] public sealed class GrammarMvpPlan
    {
        public int schemaVersion,seed,visualCourse;
        public string id,title,worldId,possession;
        public GrammarRect bounds;
        public GrammarVec spawn,exit,source,cure,key,nail,mercy;
        public GrammarSurface[] surfaces=Array.Empty<GrammarSurface>();
        public GrammarHazard[] hazards=Array.Empty<GrammarHazard>();
        public GrammarEnemy[] enemies=Array.Empty<GrammarEnemy>();
        public GrammarCoinLine[] coins=Array.Empty<GrammarCoinLine>();
        public string[] productionTrace=Array.Empty<string>();
    }

    /// <summary>
    /// Branch-only MVP: consumes build-time generated plans and realizes them as
    /// ordinary Gloom Bean physics objects. AtlasBuilder.Finish then installs the
    /// current CampaignScenery/V6/Broad/QualityBar presentation stack.
    /// </summary>
    public sealed class GrammarMvpCampaign : ICampaignSource
    {
        static readonly int[] Seeds={137,911,2026,4096};

        static StageDefinition Stage(int seed,bool stress=false)
        {
            return new StageDefinition{
                id="GB-GRAMMAR-"+seed.ToString("0000"),
                title=stress?"Generated Stress Room "+seed:"Generated Laundry "+seed,
                subtitle="Build-time grammar MVP",
                worldId="W1",course=3,atlas=true,boss=stress,
                requiredShards=0,requiresKey=true,timed=false,escapeSeconds=240,
                possessions=new[]{"Marionette","Echo","Molt"},
                gimmick="Generated explore path with a semantic possession source and optional branch.",
                turn="The Nail reveals a generated alternate high return path.",
                mercy="A generated optional branch carries one Mercy.",
                evidence="EXPERIMENTAL_GRAMMAR_MVP"
            };
        }

        public WorldDefinition[] Worlds()
        {
            return new[]{
                new WorldDefinition{
                    id="W1",title="Grammar Lab — Current Art",
                    levels=new[]{Stage(Seeds[0]),Stage(Seeds[1]),Stage(Seeds[2])},
                    boss=Stage(Seeds[3],true)
                }
            };
        }

        public void Build(StageDefinition definition,StageBuilder builder)
        {
            var asset=Resources.Load<TextAsset>("GrammarMvp/"+definition.id);
            if(!asset)throw new InvalidOperationException("Missing grammar plan "+definition.id);
            var plan=JsonUtility.FromJson<GrammarMvpPlan>(asset.text);
            Validate(plan,definition);

            var a=new AtlasBuilder(builder,definition);
            a.Begin(plan.bounds.Value,plan.spawn.Value);
            var reveal=new List<GameObject>();
            var hide=new List<GameObject>();

            foreach(var s in plan.surfaces)
            {
                GameObject g;
                if(s.kind=="floor")g=a.Floor(s.x-s.w*.5f,s.x+s.w*.5f,s.y);
                else if(s.kind=="ledge")g=a.Ledge(s.x,s.y,s.w);
                else throw new InvalidOperationException("Unknown grammar surface "+s.kind);
                g.name="Grammar "+s.role+" / "+g.name;
                if(s.returnOnly){g.SetActive(false);reveal.Add(g);}
                if(s.deactivateOnTurn)hide.Add(g);
            }
            foreach(var h in plan.hazards)builder.Spikes(h.x,h.y,h.width);
            foreach(var e in plan.enemies)builder.Enemy(new Vector2(e.x,e.y),true,false,e.direction);
            foreach(var c in plan.coins)builder.CoinLine(new Vector2(c.ax,c.ay),new Vector2(c.bx,c.by),c.count);

            a.Exit(plan.exit.x,plan.exit.y);
            HostKind kind;
            if(!Enum.TryParse(plan.possession,out kind)||kind==HostKind.None)kind=HostKind.Marionette;
            a.Source(kind,plan.source.x,plan.source.y);
            a.Cure(kind,plan.cure.x,plan.cure.y);
            a.Key(plan.key.x,plan.key.y);
            a.Mercy(plan.mercy.x,plan.mercy.y);
            a.Nail(plan.nail.x,plan.nail.y);
            builder.session.Turned+=()=>{
                foreach(var g in reveal)if(g)g.SetActive(true);
                foreach(var g in hide)if(g)g.SetActive(false);
                RuntimeEvents.Emit("grammar-return-revealed",definition.id+":"+reveal.Count);
            };
            a.Finish();
        }

        static void Validate(GrammarMvpPlan plan,StageDefinition definition)
        {
            if(plan==null||plan.schemaVersion!=1)throw new InvalidOperationException("Grammar plan schema mismatch.");
            if(plan.id!=definition.id)throw new InvalidOperationException("Grammar plan id mismatch.");
            if(plan.bounds==null||plan.spawn==null||plan.exit==null||plan.key==null||plan.nail==null)
                throw new InvalidOperationException("Grammar plan missing required semantic markers.");
            if(plan.surfaces==null||plan.surfaces.Length<8)throw new InvalidOperationException("Grammar plan has too little geometry.");
            bool hasReturn=false,hasSecret=false;
            foreach(var s in plan.surfaces){hasReturn|=s.returnOnly||s.role=="return";hasSecret|=s.role=="secret";}
            if(!hasReturn||!hasSecret)throw new InvalidOperationException("Grammar plan lacks return or optional branch.");
        }
    }
}
