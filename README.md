# Gloom Bean - native Unity platformer

## Start here
The current increment hardens the first chapter and reusable editing tools. See `Documentation/Parish/ITERATING.md`, `PLAY_GUIDE.md`, and the latest acceptance receipt before interpreting broader campaign completeness.

**Unity project:** Open this folder with Unity **6000.5.9f1**, open `Assets/GloomBean/Scenes/Boot.unity`, and press Play. **Gloom Bean > Campaign workbench** lets you jump directly to any foundation course, atlas level or boss in practice mode. Its tuning button selects the movement configuration in the Inspector.

**Windows player:** Run `GloomBean.exe` in the separately delivered Windows folder. Keep the executable, DLLs, MonoBleedingEdge and GloomBean_Data together. Select FOUNDATION for the reusable mechanics playground or HOST CYCLE for the atlas campaign. Normal progression is separate from practice, which neither grants clears nor banks Mercy secrets.

## Two products, separate history
`foundation-v0.1.0` is the original base commit, made before atlas implementation. The `foundation` branch contains an updated base-only project with no possession/campaign dependency. `main` adds the Host Cycle. A Git bundle travels with the delivery so both histories remain recoverable even while the GitHub creation request is blocked.

The foundation has four authored mechanics courses plus a foreman boss. The atlas has five worlds with four levels plus one boss in each. Campaign sizes are data in ICampaignSource, not fixed assumptions in the save system.

## Implemented base
Walking/running acceleration and braking; variable jump, coyote time and buffering; crouching, crawl clearance and slope rolling; standing/running/air tackle tiers; ordinary and height-powered ground pounds; patrol enemies with wall/optional ledge responses, stun/recovery, carry, aimed throws and thrown-enemy collisions; swimming and swim dash; moving surfaces, conveyors, sliders and four-arm orbital platforms; bounded camera; Keyling follower, pickups, World Nail/return timer, exit validation, world/level/boss selection and atomic save with backup recovery.

Movement numbers are original tunable defaults, not measured frame-perfect Wario Land 4 values. No Nintendo assets, ROMs, paid assets or runtime AI services are needed.

## Controls
| Action | Keyboard | Common Windows gamepad |
|---|---|---|
| Move, throw aim, swim | WASD / arrows | Left stick |
| Variable jump | Space / Z | A |
| Run | Shift | LB |
| Tackle / swim dash | J / X | X |
| Ground pound | L or airborne Down + J | Airborne Down + X |
| Crouch, crawl, begin slope roll | Down | Stick down |
| Pick up / throw | K / C | Y |
| Interact / pull Nail | E | B |
| Possession primary | U (hold for Root) | RB |
| Possession secondary | I | Back |
| Focus other possession in a pair | Down + I | Down + Back |
| Pause | Escape | Start |
| Controls / designer notes | F1 / F2 | Keyboard fallback |
| Mute original action cues | F4 | Keyboard fallback |

Physical-controller mapping and feel still require a human check. Not every generic controller shares XInput's button numbering.

## Edit and extend
- `Runtime/Foundation/ActorMotor.cs` and `Resources/MovementTuning.asset`: player movement and its numbers.
- `Runtime/Foundation/CarryableEnemy.cs`: patrol, combat, stun, carry, throw and recovery.
- `Runtime/Foundation/StageBuilder.cs`, `FoundationCampaign.cs`: reusable environment pieces and foundation courses.
- `Runtime/Foundation/StageSession.cs`, `Progress.cs`: game/escape state and save data.
- `Runtime/Campaign/HostController.cs` and the form files: possession lifecycle, control ownership, cures and interactions.
- `Runtime/Campaign/AtlasCampaign.*.cs`: the 20 courses and five bosses; `ATLAS_COVERAGE.csv` maps them to the source PDF.
- `Editor/CampaignWorkbench.cs`: direct practice navigation; no editor-only dependency in the player.

Geometry and components are ordinary inspectable Unity objects during Play. For static boxes, use **Campaign workbench > Edit layout**, modify position/rotation/scale in Scene view, then **Save static layout overrides**. These ordinary JSON files are reloaded by later play sessions and native builds; source fingerprints reject stale overlays atomically. Scripted machinery, source/cure placement and gameplay rules remain builder-authored. The scene-snapshot command is an inspection aid, not a production substitute for reconstructing runtime callbacks/textures. This release does not pretend to contain 20 hand-authored tilemap scenes.

## Build and verify
Run `Tools/Build-Windows.ps1` as your normal licensed Windows account. It discovers the pinned Hub editor or accepts `-UnityPath`. Then run `Tools/Verify-Windows.ps1 -Suite Mechanics`; use `-Suite OpeningRoute` for the original opening-level regression, `-Suite Parish -Route W1` for the first chapter and `-Suite Parish -Route W1 -WithSecrets` for its four Mercy routes. `-Suite Parish -Route GB-L01 -WithSecrets -Practice` verifies that a real practice clear never writes progress. Each process has a maximum nine-minute timeout, a unique report directory and an isolated verification save. No new credentials or licence changes are performed.

The final receipts identify what passed. Mechanical tests, scene construction and screenshots are **not** a complete 20-level/5-boss playthrough or a human-fun certificate. Read `Documentation/ATLAS_IMPLEMENTATION.md` and `Documentation/RELEASE_STATUS.md` before interpreting the prototype as finished.

## Save and recovery
Normal saves live at Unity's `persistentDataPath/host-cycle-save.json`. `-gb-save <absolute path>` selects a separate save. Verification always forces its own test save. Permanent first corruption is retained. Restoration requires every exact `GB-L01-MERCY` through `GB-L20-MERCY` identifier; duplicate or unrelated identifiers cannot satisfy that ending.

The original user request is in `Documentation/Source/CURRENT_REQUEST.txt`; the PDF is bundled alongside the project with a SHA-256 reference. New code/tuning is an experimental implementation, not a silent amendment to the design atlas or DungeonForge requirements.
