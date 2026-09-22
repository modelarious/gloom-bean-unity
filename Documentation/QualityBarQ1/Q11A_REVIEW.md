# Q11A actual-image and native-check review

All225 native camera frames were captured and all prior camera anchors retained. Fifteen actual route cases passed, including the three newly tested bosses. The mechanics suite failed at the new late-spawn fixture: it placed a live patrol at y=-900, below the existing y=-100 kill-plane. Production code correctly removed it. Q11B moves only that test fixture to x=-900,y=900 with zero gravity; no kill-plane or gameplay threshold changes.

Full contact-sheet inspection exposed two rendering defects that image-count success did not catch. The falling mass uses SpriteRenderer Sliced size4x4 on a unit transform; the new child had remained1x1. Q11B inherits the actual renderer size for non-Simple renderers, preserving the existing4x4 visible footprint. Per-stage native checks now compare source/new sprite world bounds. The original purely decorative sibling named Witnessing eye also overlapped the new authored faces. Q11B suppresses only that collider-free primitive while the actor view is enabled, restoring its original flag on disable/destroy. No stage, boss body, hitbox or solution is moved.

The distant final-Host manifestation now uses the same authored sheet family rather than a separate old portrait. It remains explicitly noncolliding scenery, not a surrogate combat body. Actual boss motion must still be inspected from the native route.

Q11A's original screenshots and failure evidence remain. These corrections require a full225-image recapture and fresh16-case regression. Extreme concept-quality fidelity remains UNMET; this is actor-expression work, not final-art approval.
