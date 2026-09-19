using System;
using UnityEngine;
using GloomBean.Foundation;
namespace GloomBean.Campaign
{
    public sealed partial class AtlasCampaign:ICampaignSource
    {
        static readonly string[] titles={"Sunday Best","Belfry of Late Voices","The Puppet Laundry","House of Borrowed Skins","Pear Tree Gallows","Kitchen of the Unfinished Feast","The Saint's Irrigation Ditch","The Garden of No Seasons","Tenement of Two Suns","The Street Behind the Wall","The Perspective Tax Office","The Hotel That Is Its Own Courtyard","Rain of Kneeling Men","The Seamstress Bridge","Procession of Closed Lids","The Cathedral in Freefall","Choir of Iron Halos","Noon Without Shadows","Scripture That Writes Back","The White Gate Is Below You"};
        static readonly string[] kinds={"Echo","Echo","Marionette","Molt,Echo,Marionette","Wax","Gullet,Wax","Root,Gullet","Wax,Gullet,Root","Mirror","InsideOut,Mirror","Parallax,InsideOut","Mirror,InsideOut,Parallax","Censer","Stitch,Censer","Coffin,Stitch","Censer,Stitch,Coffin","Lodestone","Shadow,Lodestone","Ink,Shadow","Echo,Marionette,Molt,Wax,Gullet,Root,Mirror,InsideOut,Parallax,Censer,Stitch,Coffin,Lodestone,Shadow,Ink"};
        static readonly string[] rules={"Two-sided parade facades reveal their structural backs after permanent corruption.","Visible sound pulses travel along ropes; opposing brakes demand overlapping arrival windows.","Hanging lines sag, counterweights respond to mass, and fans push pendulums.","Peel room skins into bridges; shed bodies pin returning hides.","Ripening fruit changes mass and bends real branches; wax diverts the drain.","Move actual terrain to route grease into dishes and generate steam.","A bounded pump wets one root seam at a time; the body must follow a curved path.","Moving climate bands change soil moisture and growing platforms.","Two sunlit apartments have mirrored but independently colliding occupants.","Closed fresco stone and its empty complement are separate collision domains.","Depth-stamped architecture changes physical scale while projected positions coincide.","Whole room shells orbit a courtyard while retaining their physical contents.","Telegraphed falling witnesses kneel into temporary platforms, then sink.","Seam endpoints tug real hinges and change the shape of the bridge.","Bell commands stop, lower or rotate moving pallbearer platforms.","A severed cathedral physically descends while its exit rises through it.","Orbiting iron objects reverse polarity; free and fixed masses respond differently.","Moving light and movable screens produce a continuously rebuilt shadow graph.","Punctuation rewraps physical word-platforms; ink supplies temporary alternate routes.","A five-sanctum synthesis course offers multiple systemic routes, not fifteen matching locks."};
        static readonly string[] turns={"The facade route falls away and a backstage return bridge becomes accessible. No countdown.","The cracked bell splits every command into weak and delayed strong pulses.","Surviving rails swing and sag after the supports are cut.","Discarded skins crawl back; solid husks can hold their openings.","Fruit ripens at once and weighted boughs bend into new routes.","Hatch flow reverses, changing which dish receives the grease.","The local pump reverses its wet-seam sequence.","Climate bands stop obeying manual steps and drift continuously.","One sun goes out; the moving shutters reverse their support pattern.","Paint loosens, closing and opening different complement corridors.","The tax stamp reverses its near/mid/far order.","Courtyard rooms begin an orbital exchange and expose rear exits.","Witnesses fall more frequently as the return climbs through their temporary poses.","The bridge's hinges loosen and the return uses the newly folded structure.","The bell is silenced; carriers keep moving and must be braced or bypassed.","Pulling the Nail starts physical descent. Remaining altitude, not a generic timer, is the deadline.","The common polarity clock splits into staggered local rhythms.","The noon light moves overhead and erases long natural shadow routes.","Revisited word-platforms erase behind the player.","The final Nail initiates a controlled descent through the sanctums. It does not cure the protagonist."};
        static readonly string[] secrets={"Hold the smiling mascot's back jaw open with an echo.","Catch a delayed body on furniture so it reaches the upper clapper differently.","Transfer onto the drying cabinet's side rail and reel up from below.","Two abandoned skins make the core small enough for the upper seam.","Partition wax volume to fit the glass bell without losing the reformed body's reach.","Rehome a structural chunk to redirect the hidden dish's output.","Use the briefly wet upper seam, then find a body-sized exit pocket.","Prepare a root route while the moving spring band passes through it.","Desynchronize the reflection against the apartment's one unmatched cabinet.","Close the moon outline to gain a walkable interior that normal space lacks.","Use far-scale clearance and return through the shared projection.","Board a rear-facing orbiting room after the facade opens.","Ride a kneeling witness before its pose sinks away.","Fold two tower edges toward one another rather than extending a rope.","Rotate into the correct horizontal footprint beneath the grille.","Catch the moving exit-side chapel during the descent.","Match the orbital motion between two changing magnetic anchors.","Move the screen so the Host's own shadow connects the final gap.","Use a temporary ink arc to reach the semicolon's shadow route.","Repeat the opening parade's short-short-long jump rhythm."};
        static readonly int[] pages={13,14,15,16,19,20,21,22,25,26,27,28,31,32,33,34,37,38,39,40};
        public WorldDefinition[] Worlds()
        {
            string[] names={"The Parish of Small Mercies","The Orchard of Witnesses","The City Under the Fresco","The Fall","The False Empyrean"};
            string[] bosses={"The Kindly Usher","The Orchard Judge","The Surveyor","The Weight of Everyone","The Host of Hosts"};
            var worlds=new WorldDefinition[5];
            for(int w=0;w<5;w++)
            {
                var levels=new StageDefinition[4];for(int i=0;i<4;i++){int n=w*4+i;levels[i]=new StageDefinition{id="GB-L"+(n+1).ToString("00"),title=titles[n],worldId="W"+(w+1),course=n+1,atlas=true,requiredShards=0,requiresKey=true,timed=n!=0&&n!=15,escapeSeconds=n==19?360:240,possessions=kinds[n].Split(','),atlasPage=pages[n],gimmick=rules[n],turn=turns[n],mercy=secrets[n]};}
                worlds[w]=new WorldDefinition{id="W"+(w+1),title=names[w],levels=levels,boss=new StageDefinition{id="GB-B"+(w+1),title=bosses[w],worldId="W"+(w+1),course=w+1,atlas=true,boss=true,requiresKey=false,timed=false,atlasPage=17+w*6,gimmick="A mechanical multi-phase encounter; arena state and physical interactions expose the objective.",turn="Boss progression is separate from the course return clock.",mercy="No Mercy is required to defeat a boss."}};
            }
            return worlds;
        }
        public void Build(StageDefinition definition,StageBuilder builder)
        {
            var a=new AtlasBuilder(builder,definition);
            if(definition.boss){Boss(a,definition.course);a.Finish();return;}
            switch(definition.course)
            {
                case 1:Sunday(a);break;case 2:Belfry(a);break;case 3:Laundry(a);break;case 4:Skins(a);break;
                case 5:Pears(a);break;case 6:Kitchen(a);break;case 7:Ditch(a);break;case 8:Seasons(a);break;
                case 9:Suns(a);break;case 10:Fresco(a);break;case 11:Tax(a);break;case 12:Hotel(a);break;
                case 13:Rain(a);break;case 14:Seam(a);break;case 15:Procession(a);break;case 16:Cathedral(a);break;
                case 17:Halos(a);break;case 18:Noon(a);break;case 19:Scripture(a);break;case 20:WhiteGate(a);break;
                default:throw new ArgumentException("Unknown atlas stage "+definition.id);
            }
            a.Finish();
        }
    }
}