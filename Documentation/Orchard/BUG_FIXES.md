# Orchard physical debugging record

These are observed implementation defects, not excuses to remove atlas mechanics. Raw failed route reports remain under Reports/Orchard-v04.

- Wide Wax used a vertical capsule; its rendered puddle fit the gutter but its physical collider did not. Set capsule direction from requested body dimensions and test actual bounds.
- Gullet terrain placement ignored the stored tile's footprint. Placement now respects tile and Host extents, uses existing sockets when close enough and refuses real solids. Grease is displaced rather than treated as immovable architecture.
- Acquiring Root while Wax+Gullet were active cured BOTH forms and silently lost the stored terrain. Replace only the incompatible locomotion form; preserve its compatible partner and terrain identity.
- An additive trigger force was overwritten by Wax locomotion. Bounded capillary flow is now sampled by the liquid motor itself; an actual conserved wax plug controls its direction.
- Physical kitchen routing is an actual floor tile, grease trajectory, receiving dish and steam column. Its optional freight route transports the same terrain collider, not an inventory flag.
- Duplicate wet soil made the irrigation pump decorative. The pump now owns the actual first channel, and a removable drain stone controls the later channel's moisture. Reversed pumping refills the drained return route.
- Fruit could roll into the entrance gutter permanently. Fallen fruit now breaks on a second impact/tackle or rots after twelve seconds, releasing small physics insects.
- Several graybox platforms intersected headroom, freight return paths, or optional secrets. Corrected collision geometry and measured run-up margins instead of increasing the base jump.
- A boss arena transition used Revive and silently healed accumulated damage. Reposition now changes motion/position without refilling health; its regression waits for real damage immunity to expire rather than writing a private timer.

R09 independently completed all four Orchard critical routes and all four optional Mercy routes using production inputs. Full-chapter, boss-after-no-heal and earlier-chapter regression acceptance is still a separate gate.
