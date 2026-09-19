# Gloom Bean Unity - release status

**Foundation: usable native iteration base. Host Cycle: experimental authored graybox, not a finished or fully validated campaign.**

## What was actually executed

| Check | Result | Scope |
|---|---|---|
| Independent base project: clean Unity import and Windows x64 build | PASS | Unity 6000.5.9f1, no pre-existing Library cache |
| Independent base native suite | 44 PASS, 0 FAIL, 0 exceptions | Production movement, combat, environment and game-loop component tests |
| Atlas fresh Git clone: import and Windows x64 build | PASS | Path containing spaces, portable build script, 0 build errors, 28 obsolete-API warnings |
| Atlas native suite | 165 PASS, 0 FAIL, 0 exceptions | Foundation regression, all 15 possession systems, 20 outward/return construction checks and five boss initializers |
| Sunday Best actual entrance-to-exit route | 31 PASS assertions, 0 FAIL | Gameplay inputs only; Echo, Keyling, Nail, timed return, optional-secret bypass and isolated real save |
| Full remaining 19 level completions / five boss victories / all 20 Mercy solutions | UNKNOWN | Not established by component or initializer tests |
| Physical gamepad feel, blind comprehension, pacing and enjoyment | UNKNOWN | Requires actual uncoached human play |

The raw opening-route receipt's `checks:87` includes diagnostic trace lines. `OpeningRoute/ASSERTION_COUNT.json` correctly distinguishes 31 assertions from those traces. Do not market the trace count as 87 tests.

## Version and history

- Original foundation-before-atlas checkpoint: `84c52bb17fea2ff46dd45c34fed07ad005b05330`, immutable tag `foundation-v0.1.0`.
- Independent hardened base: branch `foundation`, tag `foundation-v0.1.1`, commit `ab2d9ec420fd263ffec5a47d1754e34c8ee11fde`.
- Engine-tested atlas program: commit `513edc626c5ee97f65af5ec4855c3fae7a19264e`.
- The final atlas release tag adds documentation/evidence/packaging only. Every tracked Assets, Packages and ProjectSettings file byte-matches the fresh engine-tested checkout; see TESTED_SOURCE_IDENTITY.json. Some working-copy CRLF variants were normalized to the clean checkout before the comparison.

## What this includes

Two separate Unity project editions, complete C# source, ordinary Boot scene, editable MovementTuning asset, direct-practice campaign workbench, authored course builders, portable build/verification scripts, structured native evidence and recoverable Git history. The Windows player offers both FOUNDATION and HOST CYCLE from the menu. The separate foundation source does not require any atlas scripts.

The atlas edition has all 20 named courses, the five world selections and bosses, all 15 distinct possession simulations, unique Mercy pickups, permanent first corruption and the exact all-20-secret restored-ending condition. These are implemented graybox systems, not final Nintendo-Power-style layouts rendered as finished pixel art.

## Deliberate limits and remaining work

Final sprites/animation, art-direction execution, music, cinematic endings and authored dialogue are not implemented. The atlas's exact map geometry, advanced return-path choreography, all-secret solutions and boss balance need further implementation and verification. Several mechanics are intentionally bounded simulation models, documented in ATLAS_IMPLEMENTATION.md, rather than general-purpose fluid, topology or physics solvers. The program is not a frame-perfect reimplementation of Nintendo's controller.

Twenty scene-construction checks are not twenty completed levels. Five boss initializers are not five defeated bosses. A graybox populated with the required entities is not evidence that every puzzle has a good or even complete route. This release is an iteration checkpoint, with these remaining gates preserved rather than silently certified.

## Publication and safety

All source is committed locally and delivered with a verified Git bundle. Private GitHub provisioning request `modelarious/project-factory#37` returned HTTP 401 Bad credentials; the intended repository `modelarious/gloom-bean-unity` has NOT been certified created or pushed. No tokens, credentials or licence changes were requested or embedded.

Normal saves and test saves are separate. Practice does not grant progress. Native builds/tests run under the already licensed desktop account through bounded local processes. Nintendo assets, ROMs, raw license-bearing logs, Library caches and external AI dependencies are not included.

## Next engineering gate

Extend the existing input-only route harness to Belfry of Late Voices, then every remaining course and boss. Repair actual collision, timing, source/cure and return-path failures before claiming campaign completeness. Keep final art and human feel acceptance separate from those machine checks.
