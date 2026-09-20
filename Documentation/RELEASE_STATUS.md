# Gloom Bean — Fall milestone v0.6.0

**Native Unity experimental campaign. First four worlds are input-route verified; the fifth world and final production fidelity are not finished.**

## Evidence

Fresh-clone Unity 6000.5.9f1 Windows builds, with no prior Library cache. All16 acceptance cases passed. Foundation72 and combined mechanics230 assertions passed. World1 ordinary138/secret174, World2 ordinary318/secret374, World3 ordinary284/secret327, World4 ordinary426/secret468 all passed, using unmodified real earned saves from the previous world. Surveyor42 and Weight of Everyone104 standalone assertions passed. City47 and Fall44 practice checks and explicit empty-save denial controls passed.

The ordinary saved game has exactly16 level clears and4 boss clears, no Mercy collectibles, and permanent corruption. The secret saved game has those same clears plus exactly the first16 Mercies. Sixteen is not enough to restore the ending. Practice awarded no earned progress. Parent-save bytes and SHA256 provenance were independently checked.

See `Documentation/Fall/Evidence/Clean-v06-g1/runner.json`, `VERIFIED_SCOPE.json`, individual observations, screenshots and saved test states. Assertions overlap; do not market their sum as independent mechanics. These are automated input-driven playthroughs, not human/controller/readability or enjoyment certification.

## Source and build identity

The exact engine input checkpoint is `ce58671e62086e00d1a2e06d3ee67a8494197905`. This release branch adds documentation/evidence only; Assets, Packages and ProjectSettings must be byte-identical to that checkpoint. The packaged player comes from its clean build, not from the later development worktree. Independent foundation source is `e83d1f5d30f61ed0c0c09235dd8d4c1bee031acc`.

## Included and incomplete

The project contains all20 authored graybox course definitions,5 boss controllers and15 possession simulations, plus the reusable foundation. The first16 courses, their optional Mercies and first4 bosses now have completed native route witnesses. World5 — Choir of Iron Halos, Noon Without Shadows, Scripture That Writes Back, White Gate Is Below You and Host of Hosts — is still incomplete/unverified as a complete chapter. Its old graybox definitions are not finished gameplay. Later magnetic-course development is preserved on main in the complete Git-history bundle; it is not the runtime used for this verified release.

Remaining: robust W5 traversal, its four secrets and final boss/ending witnesses; complete detailed atlas choreography and encounters; production art/animation/music/dialogue/cinematics; uncoached human/controller playtests. Existing original placeholder visuals and action sounds are not final art direction. Nothing here claims a frame-exact WL4 recreation.

## Open and iterate

Extract source and open `Assets/GloomBean/Scenes/Boot.unity` in Unity6000.5.9f1. `Gloom Bean > Campaign workbench` exposes practice stages and tuning. `MovementTuning.asset` controls the base motor. The ordinary Windows player starts at its menu; FOUNDATION, HOST CYCLE and PRACTICE are separate. F1 shows controls; Escape pauses. The player ZIP must be fully extracted with its Data/DLL/Mono directories together.

The original foundation-before-atlas tag remains intact. Source is locally committed and distributed with complete verifiable Git history. The intended public `modelarious/gloom-bean-unity` repository still returned404; no standalone GitHub game-source push is claimed. Canonical context publication is a separate verified GitHub operation. Do not restore this release over the newer active development worktree.
