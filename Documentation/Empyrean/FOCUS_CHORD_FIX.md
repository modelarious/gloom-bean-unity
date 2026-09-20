# Focus chord was leaking into locomotion

I15 collected the real semicolon while the body stood on Ink, then exposed return fragility. Source inspection found that Down+I changed focus inside BeforeMovement but left Down in the outer input. On an Ink slope the same focus chord therefore began a slope roll and crouched the body; Parallax could also step depth. The leading Echo replica saw the same unintended Down input.

The chord is now consumed centrally after the two-second primary-body delay and before depth/base locomotion. The leading replica receives movement without the focus chord while the original chord remains queued for the primary body. This changes no jump, gravity, tether, Ink lifetime or collision permission. Native tests include a real slope positive control: ordinary Down must still roll. Ordinary Down must also still change depth; only the focus chord is consumed.

Broader chapter regression is required before certifying this shared input change. Failed I15 reports remain preserved separately. No campaign completion is implied by this repair alone.

A chord spans more than one physics tick: consuming its edge alone still left a held Down on the next fixed step. Both the primary and leading-replica filters now suppress that chord direction until it is released. The regression holds Down after the edge, then explicitly releases and presses Down again for the positive control. Scripture uses a deliberately short running jump to draw a shallow, standable apex rather than trying to balance on the steep end of a full jump.
