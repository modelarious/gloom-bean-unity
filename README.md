# Gloom Bean — native Unity project

## Open and play

Open this directory in **Unity 6000.5.9f1** (the version installed on gamer-bro).
Open `Assets/GloomBean/Scenes/Boot.unity` and press Play. The first import creates an editable movement-tuning asset. The boot scene can also be regenerated from **Gloom Bean → Create or refresh boot scene**.

No Asset Store purchases, external models, Nintendo assets, game ROMs, or generated-C# dependencies are needed. The current art is original deterministic graybox art, not the atlas's final production art.

## Two milestones

1. `foundation-v0.1.0` preserves the reusable platformer foundation before atlas implementation.
2. The later campaign checkpoint adds Gloom Bean without replacing that baseline.

The foundation has four mechanical courses followed by a boss. Campaign counts live in the campaign source; they are not hard-coded into the save model. The atlas uses **five worlds, four levels per world, then a boss**. Practice selection does not grant normal progress or Mercy secrets.

## Controls

| Action | Keyboard | Standard gamepad |
|---|---|---|
| Move / aim throw / swim | WASD or arrows | Left stick |
| Variable-height jump | Space or Z | A |
| Run | Shift | LB |
| Tackle / swim dash | J or X | X |
| Ground pound | L, or Down + tackle in air | Down + X |
| Crouch / crawl / initiate slope roll | Down | Stick down |
| Pick up stunned enemy / throw | K or C | Y |
| Interact / pull return switch | E | B |
| Possession action | U | RB |
| Possession secondary | I | Back |
| Pause | Escape | Start |
| Controls card | F1 | Keyboard fallback |

Gamepad button positions assume the common Windows/XInput layout. Physical controller mapping and feel still require a human check.

## Iteration map

- `ActorMotor.cs`: movement state machine, terrain probes, jump buffer/coyote, crouch clearance, tackle tiers, pounding, swimming and rolling.
- `MovementTuning.asset`: created on first import; tune speeds and timings without editing the motor.
- `CarryableEnemy.cs`: patrol, wall/ledge response, stun, recovery, carry and projectile-enemy collision.
- `MotionPlatform.cs` / `Carousel.cs`: moving surfaces and rotation. Contact-point carry is separate from player velocity.
- `StageBuilder.cs`: composable physical-level authoring helpers; creates ordinary inspectable Unity objects.
- `FoundationCampaign.cs`: four authored practice courses and the foundation boss.
- `StageSession.cs` / `SaveStore.cs`: escape phase, prerequisites, optional pickups and atomic JSON save with backup recovery.
- `GameRoot.cs`: boot, menus, practice, pause and stage loading.
- `FoundationVerification.cs`: actual-engine physics/integration checks, enabled with `-gb-verify`.

The scene snapshot menu exports a current runtime scene for inspection. It is not a replacement for the source authoring API; runtime-generated textures and callbacks are reconstructed by the builders.

## Build / verify

Use **Gloom Bean → Build Windows x64**, or:

```text
Unity.exe -batchmode -quit -projectPath <project> -executeMethod GloomBean.Editor.BuildTools.BuildWindows -logFile <build.log>
Builds/Windows/GloomBean.exe -batchmode -gb-verify -gb-reports <absolute-report-directory> -logFile <runtime.log>
```

`Reports/verification.json` distinguishes named observations from a production or human-acceptance claim. Construction tests are not full-course playthrough proofs. A body of C# is not a verified game; consult the shipped evidence and known-limitations record.

## Save / safety

Normal saves use Unity's `persistentDataPath/host-cycle-save.json`; backup recovery is automatic. `-gb-save <path>` selects an isolated save. Verification uses a separate test save. No other project, Unity install, credential or existing save is modified by this project.

## Provenance

The user's exact current request and atlas are preserved under `Documentation/Source` in the delivery package. The code references design principles, not Nintendo implementation, art or audio. This work is a separate platformer project and does not replace DungeonForge's normative source.
