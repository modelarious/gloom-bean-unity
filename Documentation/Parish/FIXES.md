# Measured first-world defects

## Echo displacement drift
R06 logged the historical input, expected pre-integration position, and actual replay body position. Velocities were equal while displacement diverged. Reassigning Rigidbody2D.simulated=true on every replay tick re-synchronized an interpolated body. R07 activates only when currently inactive and its first recorded positions match to the displayed precision. The physical echo now traverses the first three stairs and rings both clappers two seconds later, releasing the dual brake without changing its timing predicate.

Replica acquisition now also copies the real initial locomotion state, not only position. The simulation still integrates independently; recorded positions are diagnostics and never drive it.

## Belfry lift and summit
The lift surface was 0.25 metres above its boarding platform, stopping a walking player. Align its top, not its centre. The final summit ledge overlapped the preceding stair and formed an unintended low ceiling; its footprint now leaves the approach open. These are geometry fixes, not teleporting test fixtures.

## First-world traversal and optionality
The witnesses exposed real bridge lips, cabinet approach widths, noncontinuous seam flooring, a key above the tiny core reach, and a return approach above rather than below the wardrobe ceiling. The fixes alter appropriate geometry or use a real jump/drop approach; they do not enlarge hidden collection radii or award progress. The Belfry shrine has a far-side release so pausing to read does not trap the player.

## Thread and composition
Marionette uses a tension-only, colliding joint. Its rail graphic follows the actual sag curve. Cutting only the thread re-applies remaining Molt size, mass and tackle restrictions; an absent cure is a no-op. Echo replicas preserve core dimensions and initial movement state, but do not replay recursively spawning possession actions.

## Authoring integrity
The overlay acceptance test found implicit Vector2-to-Vector3 scale producing Z=0. PrimitiveArt now creates nonsingular XYZ scales. Static-layout overlays reject changed source fingerprints and duplicate targets atomically. Scripted machines are excluded rather than exporting transforms that their scripts would immediately overwrite.

The selected historical pass/fail observations in Evidence are immutable snapshots. Consult the latest acceptance receipt for the final code; earlier passes are not automatically reasserted after later changes.
