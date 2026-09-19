using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class FoundationCampaign : ICampaignSource
    {
        public WorldDefinition[] Worlds()
        {
            var levels=new StageDefinition[4];string[] titles={"The movement yard","Freight and friction","The drowned workshop","Spokes and shortcuts"};
            for(int i=0;i<4;i++)levels[i]=new StageDefinition{id="BASE-"+(i+1),title=titles[i],worldId="BASE",course=i+1,requiredShards=4,requiresKey=true,timed=true,escapeSeconds=150};
            return new[]{new WorldDefinition{id="BASE",title="Platformer Foundation",color=new Color(.4f,.7f,.72f),levels=levels,boss=new StageDefinition{id="BASE-BOSS",worldId="BASE",title="The Foreman",boss=true,requiresKey=false,timed=false,course=5}}};
        }
        public void Build(StageDefinition d,StageBuilder b)
        {
            b.Bounds(new Rect(-8,-12,142,48));b.Decor(new Rect(-8,-12,142,48),d.title);
            b.Floor(1,0,18);b.Wall(-8,6,15);b.Player(new Vector2(0,1));b.Exit(new Vector2(1,1.2f));
            if(d.boss){Boss(b);return;}
            switch(d.course)
            {
                case 1:Movement(b);break;
                case 2:Freight(b);break;
                case 3:Swim(b);break;
                case 4:Spokes(b);break;
            }
            b.Floor(117,0,28);b.Wall(131,7,16);b.Nail(new Vector2(119,.5f));b.Collect(PickupKind.Key,new Vector2(126,1));
            // Every foundation course has a transformed, shorter return deck, not a duplicate outward corridor.
            var returnDeck=b.Solid("Switch-opened return deck",new Vector2(65,9),new Vector2(110,.45f),new Color(.34f,.58f,.57f));
            var turn=returnDeck.AddComponent<TurnObject>();turn.solidBefore=false;turn.solidAfter=true;
            var lift=b.Slider(new Vector2(117,1.5f),new Vector2(117,9.5f),new Vector2(3,.4f),3);lift.paused=true;
            b.session.Turned+=()=>{lift.paused=false;};
            for(int i=0;i<4;i++)b.Collect(PickupKind.Shard,new Vector2(22+i*25,2),"BASE-"+d.course+"-SHARD-"+i);
            b.Collect(PickupKind.Mercy,new Vector2(75,12),"BASE-MERCY-"+d.course);
            b.Platform(new Vector2(72,10.5f),new Vector2(3,.4f));b.Platform(new Vector2(77,12),new Vector2(3,.4f));
            b.Tip(new Vector2(4,2),"Move A/D or arrows. Space: jump. Hold Shift: run. F1: controls. Find 4 seals and the Keyling; hit the return switch.");
            b.Tip(new Vector2(117,2),"Jump onto the green switch or ground-pound it. The return deck and lift will open. Escape with all four seals and the Keyling.");
            b.Check(new Vector2(62,1));
        }
        void Movement(StageBuilder b)
        {
            b.Floor(27,0,24);b.Floor(58,0,24);b.Floor(86,0,24);b.Floor(103,0,12);
            b.Platform(new Vector2(11,1),new Vector2(3,.5f));b.Platform(new Vector2(16,2.5f),new Vector2(3,.5f));
            b.CoinLine(new Vector2(8,2),new Vector2(19,4),7);b.Enemy(new Vector2(26,1));b.Enemy(new Vector2(33,1));
            b.Break(new Vector2(38,1),new Vector2(1,2),1);b.Tip(new Vector2(25,2),"J tackles. A standing tackle stuns; K picks up a stunned enemy. K again throws it. Aim up/down to change the throw.");
            b.Break(new Vector2(56,1),new Vector2(1,2),2);b.Tip(new Vector2(49,2),"Hold Shift to build running momentum, then J. Jump during a running tackle to keep attacking in the air.");
            b.Solid("Low passage roof",new Vector2(68,1.45f),new Vector2(8,.7f));b.Tip(new Vector2(61,2),"Hold down to crouch/crawl. You cannot stand up inside a low ceiling.");
            b.Platform(new Vector2(84,1.9f),new Vector2(4,.5f));b.Platform(new Vector2(89,3.9f),new Vector2(4,.5f));b.Platform(new Vector2(94,5.9f),new Vector2(5,.5f));
            b.Platform(new Vector2(99,7.9f),new Vector2(4,.5f));
            b.Break(new Vector2(99,.3f),new Vector2(4,.6f),3,true);b.Tip(new Vector2(86,3),"L or Down+J in the air: ground pound. A longer drop upgrades it to an uber pound. The striped magenta floor needs that height.");
            b.Spikes(44,-1,4);b.Slider(new Vector2(40,.5f),new Vector2(46,.5f),new Vector2(2.5f,.4f),2);
        }
        void Freight(StageBuilder b)
        {
            b.Floor(25,0,30);b.Floor(54,0,20);b.Floor(87,0,28);
            b.Ramp(new Vector2(10,4),new Vector2(23,0));b.Platform(new Vector2(9,2),new Vector2(3,.4f));
            b.Tip(new Vector2(12,4),"Crouch on a slope to roll. The slope supplies acceleration; rolling can break stronger blocks and preserve momentum through jumps.");
            b.Break(new Vector2(30,1),new Vector2(1,2),2);b.Enemy(new Vector2(35,1));
            b.Prop(new Vector2(50,1),new Vector2(1.4f,1.4f),2);var plate=b.Plate(new Vector2(55,.16f),1.5f);b.Door(new Vector2(62,1.6f),new Vector2(1,3.2f),plate).latched=true;
            b.Tip(new Vector2(48,2),"Push the freight onto the scale. You weigh one unit; this scale needs more. A tackle supplies an impulse.");
            var belt=b.Solid("Conveyor",new Vector2(83,.2f),new Vector2(14,.4f));belt.AddComponent<Conveyor>().speed=-2.5f;
            b.Slider(new Vector2(68,1),new Vector2(72,3),new Vector2(3,.5f),2.5f);
            b.Slider(new Vector2(100,2),new Vector2(107,2),new Vector2(3,.5f),3);
            b.CoinLine(new Vector2(77,2),new Vector2(92,2),8);b.Enemy(new Vector2(84,1));b.Enemy(new Vector2(90,1));
        }
        void Swim(StageBuilder b)
        {
            b.Floor(24,-5,30);b.Floor(56,0,20);b.Floor(82,-7,28);b.Floor(104,0,13);
            b.Water(new Vector2(26,-1),new Vector2(28,9),new Vector2(-.6f,0));
            b.Wall(41,-3,8);b.Platform(new Vector2(38,-1),new Vector2(4,.4f));b.Platform(new Vector2(44,1),new Vector2(4,.4f));
            b.Tip(new Vector2(16,1),"Swim in eight directions. J: swim dash. At the surface, Space leaps out of the water.");
            b.Water(new Vector2(83,-2),new Vector2(28,11),new Vector2(-2,0));
            b.Solid("Submerged partition",new Vector2(80,-1),new Vector2(2,7));b.Solid("Submerged partition",new Vector2(88,-4),new Vector2(2,6));
            b.CoinLine(new Vector2(72,-5),new Vector2(91,-5),10);b.Spikes(77,-6.5f,5);b.Platform(new Vector2(96,-1),new Vector2(4,.4f));
            b.Enemy(new Vector2(58,1));b.Enemy(new Vector2(108,1));
            b.Tip(new Vector2(75,2),"The current pushes independently of your input. A dash has a cooldown; point it before pressing J.");
        }
        void Spokes(StageBuilder b)
        {
            b.Floor(25,0,26);b.Floor(61,0,20);b.Floor(99,0,16);
            b.Wheel(new Vector2(43,3),4,.55f);b.Wheel(new Vector2(80,4),5,-.45f);
            b.Platform(new Vector2(52,3),new Vector2(4,.5f));b.Platform(new Vector2(70,5),new Vector2(3,.5f));
            b.Tip(new Vector2(32,2),"The four platform arms stay upright. Your contact point follows the moving support; jump when its motion lines up with your route.");
            b.Spikes(44,-2,15);b.Spikes(80,-2,21);b.Enemy(new Vector2(25,1),true);b.Enemy(new Vector2(64,1));
            b.Slider(new Vector2(105,2),new Vector2(111,4),new Vector2(3,.5f),2);
            b.CoinLine(new Vector2(76,10),new Vector2(84,10),5);
        }
        void Boss(StageBuilder b)
        {
            b.Floor(23,0,54);b.Wall(49,8,18);b.session.Camera.bounds=new Rect(-8,-5,58,32);
            b.Platform(new Vector2(12,1.8f),new Vector2(6,.5f));b.Platform(new Vector2(18,3.8f),new Vector2(6,.5f));b.Platform(new Vector2(24,5.8f),new Vector2(6,.5f));
            b.Platform(new Vector2(30,7.8f),new Vector2(6,.5f));
            var boss=b.Solid("The Foreman",new Vector2(36,2.5f),new Vector2(3,5),new Color(.62f,.28f,.35f));var fight=boss.AddComponent<ForemanBoss>();fight.builder=b;
            b.Tip(new Vector2(6,2),"The Foreman opens after an attack. First use a running tackle, then a thrown worker, then an uber ground pound. Watch the telegraph.");
        }
    }

}
