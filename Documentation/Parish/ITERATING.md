# Iterating on the native project

## Movement and combat
Open **Gloom Bean > Campaign workbench > Inspect movement-tuning asset**. Speeds, acceleration, jump gravity/cut, coyote time, input buffer, tackle thresholds, swimming and rolling parameters live in the asset rather than in each course. Changing them changes the reachable jump envelope: re-run the native mechanics suite and affected routes afterward.

## Edit static level geometry without losing the changes
1. In the workbench, choose **Edit layout** beside a course. It rebuilds a practice instance and pauses it before ordinary play.
2. Select a listed static box or its matching object in the Scene hierarchy. Use the normal Unity move, rotate or scale tools, or change the BoxCollider2D size.
3. Choose **Save static layout overrides**. This writes `Assets/GloomBean/Resources/LevelLayouts/<stage-id>.json`, an ordinary versionable file.
4. Choose **Reload saved layout**, then resume practice. Later native builds load the same JSON.

Only eligible root-level static box geometry is included. Scripted moving platforms, hinges, enemies, gates, triggers, possession sources, curved colliders, new/deleted hierarchy objects, collider offsets and 3D tilts are not silently serialized as if they were safe. Edit those through their owning course builder or component. The source fingerprint rejects an entire stale overlay before moving any object; rebase it after changing the builder. No overlay is a solvability proof. Camera bounds, route timing and source/cure accessibility still need validation.

The old runtime-scene snapshot remains an inspection aid, not a round-trippable replacement for callback-based course authoring.

## Course source ownership
`Runtime/Foundation/` owns reusable player, combat, environment, save, selection and layout machinery. `Runtime/Campaign/AtlasCampaign.Parish.cs` owns the four first-world courses. `AtlasCampaign.Bosses.cs` owns the boss arrangements. `ParishVerification.cs` is a route witness: it sends real input frames into the same motor a player uses.

## Tests and their limits
The mechanics suite uses controlled fixtures and may place objects directly. The Parish route witness does not warp the player, give invulnerability, directly award pickups, flip gates, pull the Nail through code, or set boss phases. It reads physical state to choose inputs. A passing witness establishes that specific route, not an uncoached player's understanding or every possible state.

F1 opens controls, F2 opens atlas notes, F4 mutes/unmutes original procedural SFX. Verification is always silent and uses an isolated save. Physical gamepad comfort and audio mixing are still human acceptance items.
