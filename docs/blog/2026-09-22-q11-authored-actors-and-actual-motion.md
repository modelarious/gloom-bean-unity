# Gloom Bean Q11: actors that belong to the game, not just the screenshot

The playable campaign already existed. The remaining task was visual: move a native GBA-style platformer toward the much richer horror-art target without treating a few attractive camera positions as proof that the whole game had improved. Earlier concept images were illustrations, not screenshots of the executable. That distinction remains central to this release.

## What Q11 changes

The update preserves the preceding whole-stage construction, semantic materials, water/conveyor/rail animation and player presentation. It adds five authored boss sheets and ten patrol sheets. The patrol variants distinguish the five worlds and armour, while their poses distinguish walking, stunned, carried and thrown states. The walking cycle advances with real travel rather than making stationary enemies run in place. Boss gestures read the existing encounter phase and attack state.

The rendering components do not decide whether an enemy is stunned, apply forces, change hitboxes or award progression. They depict states that the existing game already owns. Consequently, the change reaches each actual matching component—including one instantiated outside the review cameras—and does not require a list of flattering screenshot coordinates. Actorless scenery intentionally remains the preceding artwork; it is not claimed to have changed just because the build number advanced.

## Two real defects, and one bad fixture

Q11A's new boss child looked too small on the falling congregation. The source renderer used a sliced four-by-four size with a unit transform; the child inherited the transform but not that renderer size. The corrected presentation inherits the non-Simple source dimensions, and native checks compare the world bounds. A separate collider-free decorative eye also overlapped the newly authored faces. Its original rendering flag is now suppressed only while the new view is enabled, with restoration on disable or destruction.

The first new lifecycle fixture failed after creating a live enemy below the existing kill-plane. Production code correctly removed it. The repair moved only the off-camera test actor above the kill-plane with zero gravity. No game threshold was relaxed, and the original failure is retained.

These findings illustrate why an image-count success token is insufficient. The capture job produced every requested image before the visual size defect was fixed, and an apparently plausible test setup had itself violated an existing game rule.

## A sample wider than one polished room

The retained current corpus contains nine views of each of twenty levels and five bosses: 225 actual native backbuffers. It preserves earlier named support anchors and the later separated holdouts. The comparison corpus contains 62 WL4 gameplay images plus six separately classified presentation images. All expanded reference pixels were independently checked for duplicates and overlap with the original nine-image set.

These are staged art fixtures, not a promise of exhaustive coverage. Their actor/form setup and cameras are explicit. The review includes all the weaker frames alongside the better ones, and makes the earlier Q10 images available at the same named anchors. Grades are the assistant reviewer's ordinal judgements, not objective measurements or a human panel. None of the 25 stage assessments is a strict all-dimension victory.

## Looking past the first thirty seconds

The exact packaged player was exercised through eight production-input routes covering Laundry, Mirror, Root Ditch and all five bosses. A read-only observer produced 1,700 backbuffers. The resulting clips preserve the captured time spacing; they are silent, sampled video derivatives rather than full-rate recordings or human-controlled play.

The capped early sequence was not enough for the falling-mass encounter or the final Host. A second capture recorded 71 later Weight frames and 109 later FinalHost frames from the actual foreground-owned client window. It neither altered input nor repositioned the camera or actor. The later views show the physical avalanche release and the final encounter's adaptation/victory. They also expose a remaining visual weakness: repeated instructional notices occupy too much of the upper playfield. That failure is visible in the delivered footage rather than edited away.

## Verification with source boundaries intact

The current Q11B suite passes 16 scenarios, including 801 mechanics/presentation assertions and the added actor/lifecycle checks. The frozen ZIP was hashed, extracted and exercised at the 0, 19 and 20 Mercy boundaries. Its three final-boss runs passed 51, 54 and 54 checks from unchanged genuinely earned earlier saves. Only twenty Mercies restores the ending artwork; normal saved corruption is retained.

These results are not a new complete 54-case Q11 campaign certificate. Earlier complete-campaign tests retain their original source identities. Current package verification establishes the particular routes, state boundaries, pixel data and code versions that were actually exercised.

The self-contained HTML review was loaded in a normal isolated Chrome context on the authorized Windows host, with sandboxing enabled. All 663 embedded images loaded, every one of ten embedded videos decoded and advanced, and the page had no horizontal overflow at 1440-pixel desktop and 390-pixel mobile widths. The container browser's navigation policy was left unchanged; no browser policy or user profile was modified.

## Result and next visible work

Q11 is a real, recoverable visual update with more distinctive actors and state-driven poses across the existing game. It does not meet the supplied concept-art quality. Repeated structural modules, sparse traversal portions, large in-action notices and limited individual environment authorship remain visible. More resolution filters or more passing test counts would not solve those problems.

The next art work should address those observed weaknesses while retaining the broad sample and real-action evidence. The player's guide, editable Unity project, exact build, native comparison gallery and motion evidence are deliverables; a claim of universal artistic superiority is not.

Evidence: [review and scores](../../Documentation/QualityBarQ1/Q11Review/), [native package audit](../../Documentation/QualityBarQ1/Q11Review/DELIVERY_AUDIT.json), [visual review](../../Documentation/QualityBarQ1/Q11Review/VISUAL_REVIEW.json), and [current continuation gates](../../Documentation/QualityBarQ1/Q11_GATES.md).
