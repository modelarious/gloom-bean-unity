# Building Host Cycle: the difference between a compiled campaign and a completed one

Status: engineering journal for an experimental native Unity implementation. The final measured delivery state is recorded in `Documentation/Release/NATIVE_AUDIT.json` and `Documentation/RELEASE_STATUS.md`; this article does not override either receipt or the original atlas.

## The problem

The brief had two deliberately sequenced outcomes. First, build a reusable platformer foundation with reliable movement, tackle and pound variants, enemy carrying/throwing, swimming, slope rolling, moving platforms, cameras, selection, progression and a return-to-entry loop. Commit that base before adding a game. Then implement the Gloom Bean Host Cycle atlas: twenty authored levels, five worlds and bosses, fifteen temporary possessions and a permanent first corruption.

The important distinction was not “make fifteen abilities.” Each possession had to change how the player reasons. Echo changes temporal control; Marionette changes which point receives locomotion input; Wax partitions a conserved body; Gullet relocates actual terrain; Root records a curved route through substrate; Mirror introduces a second colliding body; Inside-Out and Parallax change spatial representation; Censer changes local simulation rate; Stitch changes structural adjacency; Coffin replaces running with orientation; Lodestone exchanges forces; Shadow follows cast silhouettes; Ink makes a delayed path into temporary terrain. Molt turns an abandoned body into a persistent prop.

The level still needs its own system. A form name beside a colored door does not implement the atlas. Nor does a course constructor that places one source, one collectible and an exit. We needed to demonstrate actual outward routes, world mutations and returns, while keeping each Mercy secret optional for an ordinary clear.

## The base had to stay a base

The immutable `foundation-v0.1.0` checkpoint precedes the atlas. The independent `foundation` branch has continued to receive shared controller fixes and its own native acceptance. The campaign did not become a hidden dependency of the template.

Movement parameters are a ScriptableObject rather than numbers copied into every course. Static layout overrides are ordinary JSON with a source fingerprint. The workbench can rebuild a practice stage, pause it and save supported geometry edits; it refuses to pretend that arbitrary runtime callbacks or generated objects round-trip as a production scene. This makes the project useful for iteration without claiming it is a complete general-purpose level editor.

## What early tests proved—and what they did not

The early native suite exercised real Unity components and built every named course. That was useful, but it was not evidence that twenty levels could be completed. A ground-pound fixture can pass while the real level has an overhead shelf that prevents the required jump. A boss initializer can pass while a support catches the supposedly falling victory object forever.

We added route witnesses that send normal input frames through the production controller. They may inspect position, contacts or mechanism state to decide the next input. They may not teleport the protagonist, grant collectibles, refill health, directly change boss phases or write a completion flag. A witness must collect the key, operate the Nail, negotiate the changed route and physically reach the exit or solve the boss.

The progression test is also physical work, not manufactured save data. Each successful stage/world supplies an unchanged saved file to its successor. The runner records the parent hash, retains the original copy and tests that a new empty save is refused. Practice receives its own negative acceptance: it must not bank clears, Mercy, coins or permanent corruption.

## Failures that were actual game defects

The opening route exposed a headroom trap: return-only shelves were already blocking the first outward jump. Those shelves needed to enter with the Turn. Another opening arrangement accidentally placed an optional secret gate across the ordinary return; moving the secret geometry above the main path restored real optionality.

A world-anchored pendulum crossed its target without any contact event. Tracing positions and collisions showed that this was not insufficient launch force. The relevant DistanceJoint2D configuration suppressed the required world collision. Enabling collision on the actual joint repaired both the chandelier interaction and the suspended Host's relationship with scenery.

Capsule orientation and atomic resizing mattered for liquid and crouched bodies. Repositioning at a boss transition initially risked using a full revive and silently refilling health; the implementation now preserves damage when moving between arena phases. Partial cures needed equally careful treatment: cutting Marionette while another compatible form remained must not require an unrelated full-size restoration footprint.

Stitch and Coffin exposed an input-composition bug. While the player aimed at a seam, the unfocused Coffin also interpreted horizontal input as a request to flip. That destroyed the horizontal brace the puzzle required. Aiming now remains distinct from locomotion, and releasing the aim restores ordinary flip control.

The Weight of Everyone then found a topology error rather than an input error. Its falling mass stopped on spent work stairs. The actual lower-abutment release retracts that support; the boss ends when the real mass reaches the physical bottom, not when a scripted test assigns a victory state.

## Failures that were test or operational defects

An interrupted conversation did not erase the Windows Git history. Several later chapter commits were present while the handoff still described an old ZIP. The failure was stale navigation and incomplete delivery, not demonstrated code loss. Root entry files, a current checkpoint, owned-path commits and complete-history bundles now make recovery a bounded inspection rather than another reconstruction project.

A missing historical tool name did not establish revoked authorization. The known route can read, write, commit and run bounded checks. Unity nevertheless needs the licensed desktop account; running it as a service account is a different identity, not a reason to reinstall the engine. Those facts belong in the project's guide.

GUI evidence had a different boundary. Batchmode physics checks can succeed without painting the native menu/ending backbuffer. We therefore serialize actual windowed final-boss cases. An external keyboard journey separately checks title, controls, selection, live play, pause and practice, with screenshots from the owned window. A camera image of the arena alone does not certify its menu or HUD.

The acceptance runner itself needed tests. A ready ending could appear earlier in a dependency list than the visible case blocking it; that is not a cycle. Likewise, a receipt being replaced was not permanently absent. The reader retries for a bounded interval and returns only complete known states. The writer and reader are tested together while many records are replaced. A failure is never changed to PASS merely because its top-level summary is inconvenient.

## The last strict assertion was not a requirement

The L4 run completed the normal and secret campaign and all three ending boundaries but failed its additional delayed 120-fps Scripture case. The secret had actually been acquired via the connected shadow path. The exposed body, still supported by sloping Ink, had drifted farther than an arbitrary 0.65-metre tolerance.

The original atlas says the Ink bridge must keep the physical body within tether range while Shadow reaches the semicolon dot. It does not require the body to be locked to a world position. The production Shadow form deliberately leaves gravity active. Freezing the body or flattening the slope to satisfy the test would remove an intended cost.

M1 therefore changes the oracle rather than the game: it checks actual grounded solid-Ink support, an enabled simulated dynamic body, and the real tether. It logs support, drift and stroke age. It does not extend Ink lifetime, change gravity, move the secret, grant a form or alter the route. The original failure is retained. The first focused rerun passed the delayed 120-fps practice route, 60-fps practice route and delayed 30-fps secret route, with96,97 and96 assertions respectively. This is a corrected overly strict test—not a claim that gameplay was repaired by deleting an assertion.

The final full common-source run includes that extra case. Its result and all other outcomes remain in the final audit, even if a future run fails.

## Evidence that travels with the build

The release process pins the Assets, Packages and ProjectSettings trees and hashes the actual managed game assembly. The independent audit then checks every child exit, component result, route assertion count and parent-save hash. Diagnostic trace lines are not counted as assertions.

The three earned endings are precise boundary tests. Ordinary completion has twenty levels and five bosses with zero Mercies; the nineteen-Mercy run must remain corrupted; the twenty-Mercy run draws the restored Bean in the ending while retaining the journey's corruption in the saved world. A duplicate secret or an unrelated ID cannot replace a missing canonical Mercy.

The final player ZIP is extracted into a new directory. Its payload and assembly are verified, and that extracted executable defeats the final boss again using unchanged genuinely earned pre-final saves. Source history is independently cloned from the supplied bundle and checked with Git's integrity tools. These are different tests from “the executable exists” or “the ZIP opens.”

The audit infrastructure is also exercised with negative fixtures: missing results, nonzero exits, false summaries, changed parent files, practice leakage, duplicate secrets, incorrect restoration, corrupted-body erasure and missing ending backbuffers. The M1 helper rerun passed 18 continuity tests, 16 audit tests, 10 dependency-graph cases, 34 command-dispatch cases, 7 predecessor-reader cases and 7 concurrent-receipt cases. Those numbers describe test-tool checks, not additional player mechanics.

## Presentation without external dependencies

The current presentation uses original compact pixel characters, fifteen distinct tenant creatures, five boss portraits, world backdrops, procedural animation, typed narrative beats and an offline synthesized modal score. The character preserves the supplied reference's eyes, curl, gloves and shoes across its forms. The pre-corruption menus use an honestly pleasant town palette; subsequent menus retain the damaged identity. No Nintendo sprites, recordings, ROMs or model service are required.

This is a practical implementation approach, not a claim that its art direction has been approved by a human. Visual review checks actual rendered screens for missing assets, contrast and overlapping text. It does not convert an automated route into an uncoached player's experience.

## What surprised us

The most expensive failures were often small contracts crossing subsystem boundaries: a joint suppressing a collision, a partial cure demanding the wrong footprint, an aiming input leaking into locomotion, a static stair catching a dynamic boss, or a scheduler misreading its own dependencies. Adding more content before tracing those boundaries would have produced more named levels without a more complete game.

Another lesson was that strictness is not automatically truth. A hard assertion can enforce a behavior that the design never asked for. The correct response is to compare the observed failure with the original requirement, preserve the evidence, and strengthen the test around the actual invariant rather than making the game artificially comply.

## Remaining uncertainty

A complete witnessed campaign is not exhaustive model checking of every reachable state. The material, shadow, topology and seam systems use authored finite domains rather than unrestricted simulation. The atlas's maps are schematics, not tile-accurate production geometry; incidental micro-stories are not all literal cinematics. Echo deliberately avoids initial self-collision with its original body, while Mirror preserves body-to-body collision.

Physical controller mapping, enjoyment, horror effectiveness, puzzle discovery, pacing and audio mix still need human play. The useful next design evidence is uncoached observation of the opening corruption/Echo lesson and later unrelated rule families—not another claim based on how many files or assertions exist. The delivered source remains easy to revise when that evidence arrives.

Finally, local commits, a verified history bundle, canonical context commits and a public game-repository push are different durability facts. The delivery receipt states which of those actually happened. A repository-administration failure must not be described as lost source-write access, and a context update must not be advertised as a code push.
