# A complete campaign needs more than a green controller test

*Gloom Bean engineering journal, 20 September 2026. Original native Unity implementation; no Nintendo code or assets.*

The starting request sounded like a movement checklist: walk, run, jump, tackle, pound, swim, carry an enemy, throw it at another enemy, ride a moving platform. But the intended product was not a controller demonstration. It was a reusable foundation followed by an entire transformation-platformer campaign: twenty authored stages, fifteen possessions and five environmental bosses.

The useful distinction was between a feature existing and a player being able to use it in a complete route. A controller fixture can jump correctly while the first actual level traps its head under a return-only shelf. A hinged platform can rotate correctly while it leaves an impossible gap. An optional secret can exist while the path home accidentally makes it mandatory. Each of those conditions needs a different check.

## Borrowing a grammar, not a set of powers

The base movement stays separate from temporary possession. The campaign changes laws of play instead of adding better jumps. Echo repeats past inputs through independently colliding geometry. Root pulls an exposed body along a curve that the player actually steered. Gullet removes a terrain chunk, leaving the original hole, and relocates that same piece of support. Stitch folds architecture; Coffin changes locomotion into a sequence of corner pivots and can become a structural brace.

That separation also matters in Git. The original foundation checkpoint was made before the campaign. Its independent branch still builds without the possession implementation. Tuning the next game does not require unpicking twenty levels of this one.

## A witness must play the same game

The course witnesses submit movement, jump, aim, interaction and possession inputs to production controllers. They inspect outcomes but do not hand out keys, grant secrets, restore health, teleport around geometry or call the level-clear function. Component fixtures are allowed controlled initial conditions; complete-route witnesses are a different category and keep their normal entrances and exits.

Save lineage is part of the test. The next stage receives the exact bytes written by the preceding completed stage. The runner keeps a copy and a SHA-256 record. An empty-save control must be refused at an unearned progression boundary. A practice run must not earn anything. The normal campaign, nineteen-secret campaign and twenty-secret campaign must reach their own ending through actual victories.

## Three failures worth keeping

The first surprising late failure was a scheduler race, not a Unity bug. Visible ending tests are serialized so they can actually paint their interface. An earlier pending job could be skipped because a later visible job was running; the later job then finished during the same scan. The old end-of-pass test saw no running jobs and declared a dependency cycle. In fact, the skipped job was now ready. The repair validates the dependency graph before execution and separately asks whether pending work can advance. Its tests include a real cycle, a failed parent, an out-of-order graph and the just-unblocked GUI case.

The second was the optional Ink route. Ink is deliberately not solid immediately. Waiting for real hardening corrected one witness error, but a later practice run landed on the far left of the refuge and tried an ordinary jump from the wrong side. The level and controller did not need a secret boost. The witness needed to walk to the right side of the platform before taking off. We kept the failed traces and repeated ordinary, secret and practice routes at different rendering cadences and with a start delay.

The third was visual. The title had the cute Bean and pleasant names, but a native keyboard review showed that the menu was still nearly black. That contradicted the atlas's sincerely pleasant opening. The final presentation pass makes the pre-corruption menus light and welcoming while retaining the darker treatment after infection. It changes paint, not progression. The fixed build is run through the full acceptance sequence again rather than inheriting an older binary's certificate.

## Why images and sound have their own evidence

A camera-only render does not prove that a menu or ending drew: the interface is painted separately. The final witnesses therefore wait for the real ending callback and capture the actual windowed backbuffer. The graphics suite reads textures back from the GPU; the audio suite examines the original synthesized PCM for finite values, headroom and distinct phrases. Those observations establish that the assets are present and functioning. They do not establish that a human finds the score moving or the artwork frightening.

External keyboard testing covers the title, world/level menus, a live jump, pause, return to selection and practice entry. It uses an isolated save. The review script no longer toggles the user's persistent sound preference merely to make the test quieter.

## What the release record must say

The final record reports exact tested source and assembly identities, each child exit/result, unchanged parent saves, all three ending boundaries and the recovered archive's integrity. The independent auditor is deliberately capable of rejecting a runner that says PASS while hiding a failed child, a missing result, a changed seed, a duplicate secret or an absent ending image.

This is a witnessed complete playable implementation, not an exhaustive proof of every possible action sequence. The geometry is an authored realization of schematic pitches, not a literal tile-for-tile map conversion. Simulations have documented bounds; temporal echoes, for example, do not collide with their original body even though both interact with the world. Original pixel art and the offline score provide an implementation of the visual direction, not a claim of final commercial art quality.

The remaining human questions are concrete: can a new player infer a cure, read a moving shadow network, understand the delayed body, and enjoy the coffin rather than merely tolerate it? Those require observation of someone who did not write the solution. They should not be disguised as another hundred automated assertions.

## Reproducibility

Read `README.md`, `PLAY_GUIDE.md`, `Documentation/Release/IMPLEMENTATION_SCOPE.md` and the final `Documentation/RELEASE_STATUS.md`. `Tools/Run-Campaign-Acceptance.ps1` executes the earned graph; `Tools/audit_release.py` independently reads its evidence. Failed intermediate reports remain in `Documentation/Completion/Evidence/`. The source download includes complete Git history and the original atlas; public GitHub repository provisioning is tracked separately from local source preservation.
