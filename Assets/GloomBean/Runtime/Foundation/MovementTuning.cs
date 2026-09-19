using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    [CreateAssetMenu(menuName="Gloom Bean/Movement tuning")]
    public sealed class MovementTuning : ScriptableObject
    {
        [Header("All values are original tuning, not measured WL4 constants")]
        public float walkSpeed=5.5f, runSpeed=10f, acceleration=45f, braking=65f, airAcceleration=24f;
        public float gravity=34f, jumpSpeed=12.4f, terminalSpeed=28f, jumpCut=0.46f, coyote=0.11f, jumpBuffer=0.13f;
        public float crawlSpeed=2.3f, rollMax=18f, rollFriction=3f, tackleSpeed=9f, tackleDuration=0.25f, runTackleSpeed=13.5f;
        public float runUp=0.32f, poundWindup=0.16f, poundSpeed=24f, superPoundDistance=5f;
        public float swimSpeed=4.1f, swimAcceleration=19f, swimDashSpeed=10f, swimDashTime=0.28f, swimDashCooldown=0.45f;
        public float hurtInvulnerability=1.2f, throwSpeed=14f, throwLift=5.5f;
        public int maximumHealth=6;
        public static MovementTuning Default() { return CreateInstance<MovementTuning>(); }
    }
}
