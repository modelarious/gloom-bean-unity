# Gloom Bean Unity acceptance gates

Scope: two sequenced commits. An import, a source presence check and a gameplay result are different evidence.

- [ ] G01 Foundation: complete editable Unity project boots and builds with installed Unity 6000.5.9f1.
- [ ] G02 Movement: actual physics tests cover acceleration, variable jump, buffered jump, crouch clearance, crawl, roll on slope and moving-platform carry.
- [ ] G03 Combat: grounded/air tackle tiers, pound/strong pound, stun/carry/release/throw with enemy-to-enemy collision and recovery.
- [ ] G04 Traversal: swim, swim dash, water transitions, conveyor/sliding/four-arm carousel and camera bounds.
- [ ] G05 Game loop: key follower, configurable required/optional pickups, switch/return, timer success/failure, world/level/boss menu progression, persistent save.
- [ ] G06 Foundation checkpoint committed and tagged before atlas-specific implementation.
- [ ] G07 Atlas traceability: all 20 level / 5 boss / 15 possession requirements preserved against PDF pages, with implementation/evidence/limits per requirement.
- [ ] G08 Atlas mechanics: each possession has actual distinct simulation, acquisition, constraint, cure and a playable example; no mere renamed enum/door check.
- [ ] G09 Atlas campaign: twenty distinct authored courses, unique stage mechanisms/Turn, Mercy secrets and five distinct multi-phase bosses.
- [ ] G10 Corruption: opening transition, permanent save/menu mutation, ordinary ending remains corrupted; all-20 Mercy restoration occurs only in ending.
- [ ] G11 Regression and shipping: engine compile, runtime integration checks, native build boot, archive/source integrity, clean git history and readable controls/iteration guide.
- [ ] G12 Human feel/readability: uncoached controller playtest. UNKNOWN until performed; not implied by automated checks.

Gates are evaluated with the actual Unity validation harness and independent source/content audits authored during this session. Never mark a feature passed because its name appears in a file. Machine tests cannot certify enjoyment or production-ready art.
