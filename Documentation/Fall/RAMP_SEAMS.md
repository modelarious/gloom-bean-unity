# Physical seam overlap

R05 measured a stall at x27.52: the static landing extended one metre beyond the second hinge pivot, so the ground probe kept reading the flat floor while the capsule met the sloping support. End that landing at the hinge, as the already working entry seam does. Do not patch the character motor to tunnel through the overlap. Added independent stationary-kneel and completed-hinge regression assertions; no weakening of earlier acceptance.

R06 still stalled at the hinge lip: the rotated slab top began 0.26 m above the landing. Correct the three hinge pivots and fixed target edges by the actual half-thickness/cos(30 degrees) offset, so the collision surface joins the floor flush. Preserve ordinary movement rather than adding a motor step-up cheat. The optional island jump now has an explicit run-up on its real platform.

R07 then reached the upper landing: its left wall began 0.66 m before the ramp ended, leaving a 0.56 m collision riser. Trim the receiving landings to the true ramp endpoints and put the suspended island beyond, not across, its support tip. This preserves visible physical architecture and avoids character-controller exceptions.
