# Host Cycle - implementation and fidelity boundary

## What is here
The native Unity project contains a reusable platformer foundation and a separate authored graybox campaign: five worlds, 20 named courses, five different three-act boss controllers, all 15 possession simulations, 20 unique Mercy pickups, persistent first corruption, world/level selection and the escape/return loop. This is an **EXPERIMENTAL implementation**, not a finished commercial game or certified complete playthrough.

The implementation uses original code and original procedural placeholder art. It does not extract Nintendo physics, sprites, sound, levels or code. Runtime geometry is authored in the five AtlasCampaign.* world files through AtlasBuilder/StageBuilder. It is not procedurally generating arbitrary new campaign layouts, and it does not reproduce the atlas diagrams at pixel scale.

## Evidence boundaries
The native verification suite executes the production physics and possession classes, constructs all 20 courses in outward/return states, renders them, and initializes every boss. That establishes substantially more than compilation, but **does not establish completion of all 20 courses or victory over all five bosses**. The separate opening-route suite uses only production inputs from the actual entrance: jumping, source contact, an Echo-operated door, Keyling collection, the real Nail interaction, return navigation and persisted ordinary completion. It uses its own save file. Read the final receipts for actual outcomes.

Full unassisted campaign traversal, all-Mercy traversal, boss balance, controller feel, difficulty, pacing and blind readability remain UNKNOWN. All five boss constructors and their multi-act conditions are implemented; unless a report specifically says otherwise, an initializer test is not a victory test.

## Mechanical fidelity
All 15 forms have distinct state and behavior, rather than names attached to a matching door. Leading Echo deliberately means immediate echo input with a delayed main body: there is no future-input prediction. Echo ignores its initially overlapping original while Mirror twins remain mutually collidable. Root follows the saved curved path. Gullet rehomes the same terrain object. Wax divides conserved volume among body and plugs. Coffin performs swept corner pivots and physical bracing. Lodestone exchanges forces with free masses. Shadow traverses cast silhouettes while a finite tether constrains its physical body. Ink uses delayed, finite-lived terrain from movement history.

The local implementations are bounded approximations: complement geometry is authored explicitly; Parallax uses registered discrete planes; shadow sampling and rootable seams have finite resolution; Wax is a kinematic puddle/volume model, not a general fluid solver. These boundaries are deliberate implementation choices, not permission to call unsupported geometry or arbitrary combinations solved.

## Features not yet equivalent to the atlas's final vision
- Final pixel-art characters, possession animations, illustrated environments, authored music, cinematic presentation and narrative dialogue are not implemented.
- Atlas map geometry, some advanced return transformations, secret solutions and combination routes need per-room design review. The stage identity, sources and Turn events are present, but labels in the inspector are not gameplay acceptance.
- Boss attacks currently use a shared readable lane-commitment primitive alongside five different environmental solutions. Their bespoke final art, animation and full balance are unfinished.
- The ordinary/restored ending distinction exists in save/menu behavior; a final cinematic ending is not implemented.
- Pair composition is bounded and prevents incompatible simultaneous locomotion owners. It does not claim every possible pair is supported or tested.

## Recovery and priority
The original foundation-v0.1.0 tag is immutable and precedes the atlas. The separate foundation branch contains base-only hardening. Main contains the campaign. Do not rebuild this from chat, replace the atlas requirements, or mistake the old checkpoint report for the current release receipt. The next production gate is to extend input-only route tests and correct the remaining 19 courses and five bosses against the original atlas.
