# Iterating on the Gloom Bean Unity project

## Choose the right starting point

Use the independent `foundation` branch/project for a reusable platformer without possession/campaign dependencies. Use `main` for the complete Host Cycle implementation. The immutable `foundation-v0.1.0` tag predates the atlas work. Do not move it when improving the base.

Open the source folder in **Unity 6000.5.9f1** and load `Assets/GloomBean/Scenes/Boot.unity`. Open **Gloom Bean > Campaign workbench**. It offers Foundation/Host Cycle selection, direct practice entry for every course/boss, movement tuning and safe static-layout editing. Practice never banks campaign progress. The native Windows player does not require Unity.

## The source is deliberately inspectable

| Change | Primary owner |
|---|---|
| Acceleration, jump, tackle, swim, roll numbers | `Assets/GloomBean/Resources/MovementTuning.asset` |
| Controller state, floor/head probes, moving support and combat | `Runtime/Foundation/ActorMotor.cs` |
| Enemy patrol, wall/ledge response, stun, carry and projectile collisions | `CarryableEnemy.cs` |
| Platforms, slopes, switches, pickups, doors and four-arm machinery | `StageBuilder.cs`, `MotionPlatform.cs`, `Carousel.cs` |
| Progress, practice, saved corruption and exact Mercy boundaries | `GameRoot.cs`, `StageSession.cs`, `Progress.cs` |
| Form acquisition/cures and compatible composition | `Runtime/Campaign/HostController.cs`, `HostSource.cs`, `HostCure.cs` |
| Temporal/dual bodies, rails, husks and Coffin | `EmbodimentForms.cs` |
| Conserved Wax and terrain relocation | `MaterialForms.cs` and associated terrain/drain components |
| Interior topology and perspective planes | `SpatialForms.cs`, `TopologyRegion.cs`, `DepthGeometry.cs` |
| Local time, structural seams and reciprocal force | `ForceAndTimeForms.cs`, `FoldPanel.cs`, `MagneticBody.cs` |
| Cast silhouettes and delayed temporary terrain | `ShadowForm.cs`, `ShadowSun.cs`, `InkStroke.cs` |
| Courses and boss arrangements | `AtlasCampaign.*.cs` and stage-specific components |
| Character animation, environments, menus and endings | `HostPixelArt.cs`, `HostPixelView.cs`, `CampaignPresentation.cs` and related presentation files |
| Original score and sound | `CampaignScore.cs` and the foundation sound controller |
| Real-input route proofs | `ParishVerification`, `OrchardVerification`, `CityVerification`, `FallVerification`, `EmpyreanVerification.*` |

Paths in the table are below `Assets/GloomBean/` unless shown fully. Prefer a small coherent change and affected-world regression over replacing a working system.

## Edit geometry in the Editor without losing it

Choose **Edit layout** beside a stage in the workbench. The course rebuilds in Practice and pauses. Move, rotate or scale an eligible static box in Scene view, or edit its BoxCollider2D dimensions. **Save static layout overrides** writes an ordinary versionable JSON file at `Assets/GloomBean/Resources/LevelLayouts/<stage-id>.json`. Reload it and play the route before committing.

This overlay is intentionally restricted. It does not silently serialize callbacks, trigger logic, new/deleted objects, hinges, moving machinery, curved colliders or source/cure relationships. Edit those in the builder/component. The whole overlay rejects a mismatched source fingerprint rather than applying old positions to different objects. The project is not pretending that a runtime scene snapshot is a self-contained authored scene.

## Change movement as a reachability change

Walk speed, acceleration and jump hold affect what ledges are reachable. Run-up and pound distances affect which obstacles can be broken. Capsule resizing, ceiling clearance and form conservation affect both outward and return paths. After tuning, run the mechanics suite and the actual affected stage ordinary and secret routes. A fixture jump does not prove there is enough headroom inside a real level.

A possession must change a meaningful rule, introduce a cost and have a diegetic source/cure. Do not reduce a system to checking a form enum at a matching door. Keep optional Mercy routes out of the normal critical path.

## Native testing commands

Use a normal licensed Windows account:

```powershell
.\Tools\Build-Windows.ps1
.\Tools\Verify-Windows.ps1 -Suite Mechanics
.\Tools\Verify-Windows.ps1 -Suite Campaign -Route GB-L19 -WithSecrets -Practice
.\Tools\Verify-Windows.ps1 -Suite Empyrean -Route GB-B5 -FinalPair mirror-parallax
.\Tools\Verify-Windows.ps1 -Suite Campaign -Route GB-L18 -Describe
```

`-Describe` validates and prints dispatch without running the game. Each real run uses a new report directory and isolated save. `-SaveSeed` copies an already earned test save; `-RequireEarned` refuses an unavailable world/stage. `-ExpectedMercies` checks a boundary, never grants items. Final-boss ending verification must be windowed because batchmode does not paint the native menu backbuffer.

For an entire release, copy `Tools/campaign-acceptance-m1.json` to a new uniquely named configuration, change only the intended candidate/clone/report paths, inspect its case graph, and run `Run-Campaign-Acceptance.ps1` under the licensed account. It preserves real saved progression. Never reuse a report directory or erase a failed run. On gamer-bro use the documented scheduled-task route, not a service-account Unity process.

The standalone foundation regression is independent. Changes to shared Foundation files should be integrated into its branch deliberately and tested there as well; do not accidentally pull campaign dependencies into the template.

## Prove the release, not just the source

`audit_release.py` cross-checks every native exit/count, observed assertions, exact parent-save hashes, final25-clear/0–19–20-secret sets, ending artwork state and actual managed assembly identity. `Verify-PackagedEndings.ps1` then executes the extracted downloadable player using unchanged earned pre-final saves. Neither tool turns an error into success or certifies subjective enjoyment.

Keep different worker branches/worktrees and Unity Library/build outputs separate. Commit reviewed owned units before long tests with `checkpoint_work.py`. `package_checkpoint.py` refuses uncommitted tracked files and includes complete Git history. Restoring a Git bundle can create a local-file origin; that is not an authenticated GitHub remote.

## Test interpretation

Component fixtures may place bodies directly. Route witnesses may read state to select inputs but must not teleport the protagonist, refill health, grant forms/items or directly open/clear progression. A successful witness proves that input path, not every possible action sequence. A timing-sensitive test may reveal a controller bug, geometry bug or an invalid oracle; inspect actual contacts and the atlas requirement before changing physics or weakening the claim.

Human acceptance remains separate: physical controller layout, movement feel, readable sources/cures, secret discovery, horror effectiveness, audio mix and pacing. Record actual feedback as feedback rather than inferring it from green scripts.
