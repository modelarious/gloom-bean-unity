# From concept images to a verifiable whole-game visual update

The failure to avoid was straightforward: show a beautiful generated picture, imply that the playable game looks like it, then optimize a few convenient screenshot locations. This continuation instead changed the native Unity project and kept the concept-quality target open when the actual images still fell short.

## Recover the real work, not an old delivery

There were two surviving worktrees. `GloomBeanUnity` contained the later broad-screening work on `visual/v6`. `GloomBean QualityBar Q1` contained an undelivered, object-driven visual layer on `visual/qualitybar-q1`. Both belonged to the already published `modelarious/gloom-bean-unity` repository. The public source and authorized pushes worked. Old canonical text claiming that repository creation was blocked was stale, not evidence of missing access.

The quality worktree had twelve Unity-import metadata changes. They were reviewed, committed and pushed before new work. The later broad-capture camera correction was then merged, preserving both histories. No implementation was rebuilt from an older ZIP and no unrelated untracked work in the other worktree was absorbed.

## Generalization was made inspectable

The existing broad corpus had five camera strata in every one of the twenty levels and five boss stages. Those views were retained. Two additional strata choose actual support surfaces at different quantiles, rejecting positions within 3.1 world units of earlier cameras. The resulting corpus has 175 native backbuffers: seven per stage, including fifty newly sampled locations. The old ten showcase positions are not the basis for this report.

The reference corpus contains 52 additional retail Wario Land 4 images. Their file and decoded-pixel identities are checked against the earlier nine images, with no overlap. All 52 remain available; selecting a representative reference for a stage does not remove the other samples.

The art fixtures deliberately stage a player/form and camera. They are useful for consistent visual scrutiny, but not proof of reachability or human comprehension. Separate production-input route witnesses and a real keyboard journey exercise those different boundaries.

## What was actually changed

The Q5 pass recomposed all 26 scenic profiles: apartment cutaways with furniture, tree and conservatory layers, receding chapels, ruined city silhouettes, manuscript pages, ledger shelves and vaulted interiors. These use original construction and the existing credited CC0 components, plus disclosed individual concept-derived props. A full generated scene was never substituted for the native game.

The world renderer remains object-based. Floor spans, moving trays, materials and level profiles choose their treatment; no renderer reads a review-camera ID or a list of known screenshot positions. The shared rules also handle newly created geometry.

One subtle bug was flattening the lighting: a frame update overwrote the alpha originally authored for every decorative sprite. Restoring per-object authored opacity and using a stepped penumbra stopped the light cones from behaving like opaque triangular panels. Already-dark scenic art also no longer receives an unnecessarily strong second tint. The lighting remains deliberately pixel-based, not a high-resolution blur laid over a small framebuffer.

Other changes moved the visible part of washer and oven housings into the intended rear plane, introduced more dimensional bronze bells and strapped sarcophagi, and replaced flat fabric/book motifs. Cloth now has folds, cavities, stitching and a ragged hem. Open books have page volume, illumination and bindings; closed chained books are a different silhouette. Laundry rooms mix drum, wringer and basket shapes instead of repeating one prop. These variations follow existing structural/bay identity, not screenshot-specific rules.

## A new texture was not automatically an improvement

The Q5 stone replacement looked flatter than the old chamfered clusters when compared in native frames. The older stronger stone was restored for those material groups. The rejected version, its screenshot corpus and its source revision remain in Git. This is the important direction of the review: the image changes the decision; the decision does not rewrite the image's verdict.

The whole-stage contacts still reveal remaining limitations. Apartments are too geometrical, some surfaces repeat conspicuously, and the player/bosses do not yet have the expressive frame-authored animation of the benchmark. Several quiet or awkward frames remain sparse. Those are not omitted from the gallery and are not described as solved by higher coverage counts.

## An integration bug that screenshots missed

The first current regression completed twelve production-input routes, but the mechanics suite stopped with a NullReferenceException. A pre-existing independent test builds scenery as a child of a StageSession. The new quality layer assumed it was on the session root, so its GetComponent lookup returned null.

The correction resolves the owning session through the parent hierarchy and handles fixture/base IDs safely. The original fixture and every gameplay assertion were retained. The rerun passed 466 mechanics/presentation assertions, including 75 new checks for all-stage quality coverage, late-spawned objects well outside any review camera, and the disappearance of decorative children when their source is disabled.

A subsequent clean-source preflight correctly refused to run immediately after Unity expanded four minimal texture importer files. Those generated import settings were preserved as source, and the tests were rebuilt and rerun under a new report ID. A refused preflight is not reported as a gameplay failure or a pass.

## Scope of the proof

The current renderer audit checks all 25 stages, 175 nonempty integer-scaled native images, fifty separated new locations, zero quality-layer colliders, matched eligible/applied object counts and 52 distinct additional reference images. It also confirms that 167 protected runtime source files remain identical to the recovered baseline. Only the declared rendering and visual-test files change.

The current targeted test suite has thirteen scenarios: the mechanics/rendering suite plus opening, Laundry, Kitchen, Wet Root, Seasons, Mirror, Fresco, Procession, Weight, Choir, Noon Shadow and Ink production-input routes. These do not constitute a fresh exhaustive campaign proof or a human playtest. Earlier full campaign proofs remain associated with their own source revisions.

The downloadable player is frozen, hashed, extracted and checked again. The existing proven package-ending verifier uses unmodified genuinely earned pre-final saves. It checks zero, nineteen and twenty Mercies and retained gameplay corruption. Separate action captures and keyboard images come from this exact extracted executable; they are not reconstructions.

## Why the visual gate remains open

There is a real whole-game update here, rather than another picture or a single polished room. But the supplied concept images remain ahead in composed density, distinctive machinery and expressive characterization. A passing renderer audit cannot certify that artistic target.

The remaining work is visible in the full gallery: more individually authored structures and material shapes, less repetition, stronger action poses and better spatial composition in the weakest stages. The next pass should start from those actual images and retain the whole-stage sampling protocol. Another global filter or a new optimistic score is not a substitute.

Current source, actual captures, failed attempts, deterministic art scripts and exact package receipts are published together. The handoff names the active worktree, branch, build and open visual gate so the next session can do art work rather than repeat access recovery.
