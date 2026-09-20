# Gloom Bean — full playable campaign delivery

**Native implementation and release acceptance: PASS.** This is the full twenty-level, five-world/five-boss Host Cycle development build, not the older four-world graybox. The independent platformer foundation remains reusable and separately committed. Manual controller, uncoached-player and production-quality judgement remain unverified.

## Measured release evidence

| Check | Result | Exact scope |
|---|---|---|
| Fresh pinned Unity import/Windows build | PASS | Unity 6000.5.9f1; zero errors; atlas 24 obsolete-API warnings, independent base 0 warnings |
| Independent foundation | 72 PASS, 0 failures/exceptions | Production base components without campaign dependencies |
| Combined mechanics/presentation | 309 PASS, 0 failures/exceptions | Actual native components, textures, score/audio data and runtime integrations |
| Full common-source campaign graph | 54/54 scenarios PASS | Ordinary and secret routes,20 levels/5 bosses, all 5 final pair families, alternative sanctums, selected cadence/delay cases, practice, no-Ink and 5 explicit expected-negative access checks |
| Independent native audit | PASS | Child exits/observations, assertion counts, unchanged earned-parent hashes, exact source trees, managed assembly and 0/19/20 Mercy ending boundaries |
| External keyboard UI journey | 14 screens reviewed | Real menu/controls/movement/pause/practice/final-boss rendering; isolated save unchanged |
| Final downloadable ZIP extracted and played |3/3 PASS | Actual B5 victories from genuine pre-final saves: ordinary 51, nineteen 54, restored 54 assertions; no failures, exit 0 |
| Human/controller/art/mix judgement | UNKNOWN | Not established by scripts, screenshots or source presence |

See `Release/NATIVE_AUDIT.json`, `Release/PACKAGED_AUDIT.json`, and untouched reports under `Release/Evidence/`. Earlier failures remain in history. Expected-negative tests deliberately exit 1 with a precise denial; they are not unexpected runtime failures. The ordinary and nineteen-Mercy ending are correctly the same outcome; twenty restores the original Bean only in the ending, while saved gameplay corruption remains true.

## What is delivered

All twenty authored courses and their Turns, fifteen systemic possessions with sources/cures/costs, five mechanical bosses, all twenty optional Mercies, level/world selection, persistent progression, practice isolation, original pixel characters and tenant creatures, world backdrops, procedural animation, typed narrative/animated endings and an offline synthesized score. The foundation retains movement/run/jump, tackles, both pounds, enemy patrol/stun/carry/throw, swim/dash swim, crouch/crawl/slope roll, moving pieces/carousels, camera and the key/return/timer loop.

The final-boss witnesses cover Lodestone+Shadow, Echo+Ink, Wax+Gullet, Stitch+Coffin and Mirror+Parallax. Final-stage alternatives are tested rather than only described. A normal full clear earns20 levels/5 bosses and requires no Mercy; the secret clear earns all 20. The nineteen-Mercy near-miss does not restore the body. No test-generated save is installed as the user's normal save.

## Exact program identity

- Tested engine source: `e8203f2825db1583022ceb37e14ba843ab0ed3a5`.
- Assets tree: `8b7ea8f547def056ac12d1473af231185d99be7c`.
- Packages tree: `3d7201db080528e09addee6e2cbe2cac9ae5461d`.
- ProjectSettings tree: `552a717f21e59d1d7c65bde3cda9507b45601e50`.
- Managed game assembly SHA256: `8ccc3b3ac2d8aba179e74333bd999190b8f8d0ac7a1f871fa268297389795e3e`.
- Final Windows ZIP SHA256: `37ed7162514e4901b58467ec6cfdd49f77597907d15b87936ab1bb330fb637aa`, 62,861,842 bytes, 159 declared payloads.
- Independent base: `1b450786f425c0ca325be8ff9b09cafdaeb0cfb8`; the original foundation-before-atlas tag is unchanged.

The source delivery adds documentation/tooling/evidence after the tested commit. Its engine trees remain byte-identical to the tested program. The archive manifest and complete Git bundle record the exact delivered source HEAD; a source file cannot embed its own future commit hash. Final package integrity/restoration receipts are supplied with the delivery.

## The final correction was an oracle correction

The preceding L4 run passed 53/54 cases. Its extra delayed 120-fps Scripture test required an unjustified 0.65 m world-position lock while Shadow traversed from a body on sloping Ink. The atlas requires actual physical Ink support and tether range, not immobility. M1 checks grounded solid Ink, a simulated vulnerable dynamic body and the real tether separately. It does not change physics, terrain, lifetime, health, grants or progression. Three focused cadence/delay runs passed before the full 54-case clean run. The original failure remains preserved.

The final player ZIP also corrects a documentation filename: the `CampaignScore` class lives in `CampaignPresentation.cs`. The program did not change, but the exact final archive was extracted and all three ending-boundary victories were repeated anyway.

## Engineering and human boundaries

`Release/IMPLEMENTATION_SCOPE.md` maps the atlas to code/witnesses and records finite material/topology/shadow/seam models, original rather than tile-exact map geometry, and incidental micro-stories not recreated as literal cinematics. Echo intentionally ignores its original actor's initial overlap; Mirror copies collide. The game contains witnessed solutions; this is not exhaustive proof against every possible softlock or a guarantee of enjoyment.

Original compact pixel art, procedural animation, captions and synthesized music are the delivered presentation approach. Commercial polish, physical-controller mapping/feel, uncoached comprehension, horror effectiveness, pacing, accessibility and audio mix are human-review items, not falsely certified by green tests.

## Play and iterate

Extract the entire Windows archive and run `GloomBeanWindows/GloomBean.exe`. Use Begin/Continue Host Cycle for earned progression, Practice for direct course/boss access, or Platformer Foundation for the mechanics playground. F1 opens controls. The source opens in Unity 6000.5.9f1 at `Assets/GloomBean/Scenes/Boot.unity`. Use the Campaign workbench, MovementTuning asset and supported static-layout overlays; see `../ITERATION_GUIDE.md` and `../PLAY_GUIDE.md`.

## Preservation and publication

Reviewed units were committed incrementally and old failures retained. Complete main/foundation/release history is in the source bundle, with the unchanged original atlas/character reference in the delivery. Canonical context is separately committed to GitHub. The intended public standalone game repository still had no verified origin/publication path at the last probe; a game-source push is NOT claimed. That administration boundary does not mean source editing, native builds or local Git stopped working.

## Source recovery verification

The complete source/history validation snapshot was independently reassembled and checked in the container: 6,019 payload hashes, fresh bundle clone, `git fsck --full` and matching tested engine trees all passed. The final snapshot only adds documentation/tooling/evidence and is rechecked in the external delivery verification before handoff. The exact source ZIP hash is kept outside the ZIP in that final receipt and canonical context to avoid self-referential hashes.
