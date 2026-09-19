# Foundation checkpoint

Native Unity 6000.5.9f1, Windows x64, Mono development player. Build completed with zero errors. The actual player executed 43 checks, with 43 passes, zero failures, zero caught runtime errors/exceptions and exit code 0. Read `Reports/foundation-verification.json` for individual observations.

The checks exercise production Rigidbody2D/collider code: jump height, early release, coyote time, buffered landing, crouch clearance, tackles, pounds, enemy pickup/throw collisions, patrol/recovery, swimming, slope acceleration, platform carry, carousel construction, physical scales/gates, return-loop requirements, timeout and save recovery. Four courses and the boss construct in the native player.

These results do **not** prove every authored course can be completed, every boss phase is balanced, or a physical controller feels good. Camera limits, conveyor feel, UI navigation and full-stage completion still need live human acceptance. The Foreman is a simple three-condition mechanical boss, not a finished encounter. The checkpoint marker is exposed to future recovery rules; the delivered failure menu restarts the level.

The first real runtime pass exposed a Unity renderer-component conflict on the slope object; slope meshes were moved to their own child object. Tutorial platform risers were reduced to fit the measured jump envelope. A later pass verified the corrected code.

The foundation commit contains no atlas-specific possession implementation. The later campaign is a separate namespace and is detected as an optional campaign source.
