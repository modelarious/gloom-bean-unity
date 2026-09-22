# Current art meets a level grammar: the smallest Gloom Bean experiment

The architectural risk was discovered before the art direction was finished: Gloom Bean is trying to combine Wario-Land-like room-specific visual craft with a grammar capable of producing layouts that were never individually authored. Waiting for all twenty levels to become bespoke art and only then reverse-engineering a grammar could make the art inseparable from those exact layouts. Starting from abstract procedural rectangles, however, would test almost nothing about the actual game.

This experiment therefore branches the current visual source rather than replacing it. The worktree `experiment/grammar-art-mvp` starts exactly at `visual/qualitybar-q1` revision `0cf69af1dc5e50028eaf876c235ef3aee811296f`. The normal Host Cycle remains untouched.

## The useful seam already existed

The important finding was that the current presentation stack is substantially object-driven. `StageBuilder` creates ordinary collision-bearing Unity objects. `AtlasBuilder.Finish()` installs `CampaignScenery`, which in turn installs the V6, Broad and QualityBar dressing layers. Those systems inspect stage identity and object roles such as floors, supports, rails and moving bodies; they are not simply flattened screenshots tied to the authored camera positions.

That means the first test does not need to generate new art. It can generate new *physical arrangements* of known object vocabulary and ask the current art system to dress them. If that fails visually, the failure tells us something concrete about what the visual grammar is missing.

## The intentionally tiny grammar

`Tools/grammar_mvp.py` implements one deterministic build-time grammar:

`ENTRY -> FORWARD BEATS -> KEY -> NAIL/TURN -> ALTERNATE RETURN -> EXIT`

A possession source appears early in the forward route. An optional branch contains one Mercy. The Turn reveals a separate high return route over the previously traversed space. Hazards, patrol enemies and coin lines are derived from the generated supports rather than placed at a fixed screenshot coordinate.

Four fixed seeds — 137, 911, 2026 and 4096 — currently produce four distinct held-out plans. The build-time validator checks required semantic roles, marker ordering, world bounds, forward gaps, vertical changes and production trace. Regeneration must reproduce the committed JSON byte-for-byte.

## Unity remains the realization layer
The Unity adapter `GrammarMvpCampaign` reads those JSON plans and realizes them using the existing `StageBuilder` and `AtlasBuilder` APIs. It does not fork or edit any authored `AtlasCampaign.*` level file. Generated return supports are initially inactive and appear on the real StageSession Turn event. The current art stack is then installed through the same `AtlasBuilder.Finish()` path used by authored stages.

The experiment appears as a separate **GRAMMAR MVP / GENERATED ROOMS** source in the menu. Selecting it automatically enables Practice mode so the experiment cannot persist the campaign's corruption/progression state into the real save. The existing Practice button explicitly continues to select `AtlasCampaign`.

## What has been measured

The committed generator passes deterministic regeneration and validation for all four plans. The audit reports four distinct geometry fingerprints. A source integration check confirms the V6, Broad and QualityBar dressing chain, the separate menu source, Practice sandboxing, four Resources plans and zero authored AtlasCampaign-level edits. Python files compile, and `git diff --check` is clean.

Those are static/build-time facts only. Punchcard has no Unity editor or C# compiler, and the licensed Windows Unity host was offline during this pass. Therefore native import, C# compilation, actual traversal, Turn behavior and the most important question — whether current art looks coherent on these unseen layouts — remain **UNKNOWN**, not PASS.

## Why this MVP is useful even before the native run

The experiment has already falsified one pessimistic assumption: the current art pipeline is not necessarily trapped inside the twenty authored layouts. There is an existing interface where semantic geometry can feed the same object-driven art rules.

It has not established the stronger claim that the rules generalize *well*. The first native screenshots may be ugly, repetitive or unreadable. That would be a valuable result: it would identify which visual decisions currently depend on authored composition and therefore need to become explicit visual-grammar rules.

The next evidence-producing step is narrowly defined: import this branch in Windows Unity, open all four generated rooms, exercise at least one full explore-to-Turn-to-return route, and inspect native 240x160 captures. Do not patch the first bad screenshot by coordinates. Any recurring failure should become a reusable grammar or art-composition rule.

This is an **EXPERIMENTAL MVP**, not a decision to replace the authored Gloom Bean campaign and not evidence that arbitrary Wario-Land-level handcrafted generation is solved.
