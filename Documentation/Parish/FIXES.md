# Measured first-world defects

## Echo displacement drift
R06 logged the historical input, expected pre-integration position, and actual replay body position. Velocities were equal while displacement diverged. Reassigning Rigidbody2D.simulated=true on every replay tick re-synchronized an interpolated body. R07 activates only when currently inactive and its first recorded positions match to the displayed precision. The physical echo now traverses the first three stairs and rings both clappers two seconds later, releasing the dual brake without changing its timing predicate.

Replica acquisition now also copies the real initial locomotion state, not only position. The simulation still integrates independently; recorded positions are diagnostics and never drive it.

## Belfry lift and summit
The lift surface was 0.25 metres above its boarding platform, stopping a walking player. Align its top, not its centre. The final summit ledge overlapped the preceding stair and formed an unintended low ceiling; its footprint now leaves the approach open. These are geometry fixes, not teleporting test fixtures.
