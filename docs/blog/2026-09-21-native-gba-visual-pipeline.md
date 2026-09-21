# Making Gloom Bean read like a GBA game — and refusing to fake the comparison

The problem was not that Gloom Bean lacked a playable campaign. Its twenty levels, five worlds, five bosses and fifteen possession systems already existed. The problem was that it still looked like a desktop prototype with pixel-art components: thin floating platforms, inconsistent pixel sizes, a text-heavy HUD, weak material definition, and scenery enlarged until much of the composition disappeared.

The requested benchmark was unusually demanding: compare real screenshots against Wario Land 4, improve what loses, rebuild, capture again, commit, and continue until every selected example wins in style, character, clarity and polish. A later clarification made the visual constraint explicit: this game also needs to look like a GBA game. These are different acceptance conditions. A native pixel grid is testable; artistic superiority is a comparative judgement that cannot be established by a green build.

## Recovering work instead of rebuilding the project

The public `modelarious/gloom-bean-unity` repository was already present when this continuation began. The prior session had published the complete history and the first V6 work. The active Windows checkout matched `visual/v6` at `4424d5c`. The old canonical checkpoint still said repository creation was blocked. Direct remote refs, not that stale sentence, established the current publication state.

The existing source, foundation branch, reference artwork and native fixtures were retained. There was no reason to create another repository, replace the controller or restart the first world. Each meaningful change in this continuation was committed and pushed before its long native run. The earlier foundation-before-atlas tag remained unchanged.

## Establishing a sample that could not quietly improve itself

Ten stage/reference pairs had been selected before these changes: the cute parade, puppet machinery, orchard, mirror tenement, falling witnesses, cathedral, magnetic choir, manuscript, falling-mass boss and final-Host opening. Those original camera centres and lenses were kept. A second, explicitly prefixed GBA view was added for each scene so that tighter production framing could be judged without silently discarding the older view. The image aspect changed to 3:2; the legacy lens therefore does not imply a pixel-identical crop.

The fixtures run inside the native Unity player and capture its actual backbuffer. They deliberately stage an actor and a form, so they are not evidence that a player can reach that state. Input-driven route tests and later native action captures provide that separate evidence. No generated comparison poster, repainted screenshot, or screenshot-based claim that a missing system exists was accepted.

## First change: one real pixel grid

The campaign now renders into a 240×160 render texture with point filtering and no multisample antialiasing. The backbuffer displays only integer multiples of those pixels, centred inside black letterboxing. At a 1280×800 client this produces a 1200×800 image with exactly5×5 blocks. At an observed1274×783 client it uses a960×640 image with4×4 blocks. The latter matters: fixed screenshots at a convenient window size do not prove the actual desktop client follows the same rule.

The UI could not remain a high-resolution overlay. An original5×7 bitmap alphabet replaced smoothed desktop glyphs; icons and menu hit areas were adapted to the same logical grid. The original selection, enabled-state and interaction code still decides what a button does. The bitmap layer draws that decision. The independent foundation branch is not made dependent on campaign presentation code.

This is a Unity Windows implementation with a GBA-style presentation. It is not a GBA ROM and does not claim all of the original hardware's palette, memory or scanline restrictions. Nintendo's published240×160 screen specification is the format reference, not a claim of hardware emulation.

## Second change: make surfaces and state readable

A coarse contact edge, a midtone material and a dark recess are more useful than adding noise everywhere. Separate small foreground banks distinguish timber, masonry and ivory/gold surfaces. The visible walking edge remains aligned with the existing collider; decoration does not invent new collision.

The first art iteration also exposed an economy problem in the HUD. Simplification had hidden useful mechanical state. Wax still has conserved volume, Ink still has a finite length and lifetime, and Lodestone still has polarity. Those states returned as compact numbers or words, not paragraphs. The final pass moved the form card away from the lower-left landing space. That change came from actual images in which the card obscured the feet or nearby ledge.

The source and cure vocabulary also needed to be more than coloured arches. Original bitmap objects now depict felt, shears, a brush, cold water, bitter herbs, salt, velvet, frames, wind, a grave mouth, ceramic and an eclipse. Plate and lever sprites read the real pressed/toggled state. They do not set that state themselves.

## A green capture job concealed a real regression

The second pass built and produced all its images, but the world signs disappeared. The mistake was treating the successful image count as sufficient evidence. An unreadable texture copy did not preserve the owned lettering in the way expected. The correction was to create the sign's own bitmap pixels directly rather than borrow a destructible UI-cache texture or clone an unreadable texture.

The repaired sign was then observed in the real manuscript image. A native test stresses UI-cache turnover and confirms that the independent world lettering still contains opaque pixels. A GPU readback also checks that those pixels are visible to rendering. The old failed images were preserved; they are not overwritten by the repaired ones.

A separate keyboard journey exposed menu problems that staged gameplay could not: clipped F1 text, a colliding footer, awkward disabled labels and unpainted outer borders when the stage was inactive. These were corrected and the keyboard journey repeated. Compilation would not have found them.

## Composition mattered more than another texture

The panorama problem was geometric, not simply artistic. The existing image was being enlarged so that a huge window, paragraph fragment or empty region occupied the whole screen. Fitting the complete panorama vertically revealed the orchard canopy and chapel, the tilted city, the cathedral rose windows and the illuminated manuscript columns. The parallax remains; the scene is no longer dominated by a cropped fragment.

A new Cathedral composition separates that level from the open Fall skyline. The False Empyrean uses ivory masonry, violet recesses and warmer window light. Selected environment fragments by Luis Zuno / Ansimuz are credited as CC0 components, with archive hashes and author-page provenance retained. Their composed adaptations are not falsely described as entirely original source artwork. No third-party music, code, character graphics or Nintendo assets were imported into the runtime.

## Character revision without inflating the hitbox

The fourth boss's previous artwork was connected, but it looked like repeated face icons. Renderer inventories confirmed this was an art-quality problem rather than a missing-component problem. The replacement uses asymmetric hoods, grasping limbs, layered faces and directed cloth folds on the same physical4×4 world footprint. The boss still wins or loses through the real falling mass and structural solution, not a visual flag.

The final entity also received a more asymmetric, readable face. Its large arena-opening apparition is explicitly distant, noncolliding background art. It was too faint and clipped in an earlier pass, so its visual placement and tint were corrected. That does not move the combat body or make the opening image a substitute for a boss-action screenshot. The report preserves that distinction.

## What was measured and what was not

The pixel auditor reconstructs each actual viewport from its logical240×160 samples and checks every pixel. It has negative controls: a single altered subpixel, bilinear scaling and an empty frame must fail. A valid odd-size client must pass. The original ten collider name/type/layer/trigger inventories are compared separately; that census is not claimed as a full proof of identical collider bounds.

A protected-source audit checks the unchanged movement, possession, stage and save implementation outside an explicit list of rendering/test changes. The shared menu draw hooks, camera framing and HUD suppression are separately reviewed. The native campaign and affected-route tests remain necessary because an allowed-file list is not a semantic proof.

The full54-scenario GBA P4 run passed with genuinely earned save lineage through all twenty levels and five bosses. Zero and nineteen Mercies retained the ordinary ending; twenty produced the restored ending. That run belongs to its pinned P4 candidate. The subsequent P5–P7 art/render changes have their own captures, focused native checks and frozen-package tests; the earlier54-case certificate is not relabelled as a later engine.

<!-- CURRENT_VERIFICATION_RESULTS -->
Current exact source identities and final packaged-run results are recorded in the adjacent verification JSON and receipts. Do not infer a later PASS from an earlier candidate.
<!-- END_CURRENT_VERIFICATION_RESULTS -->

## The comparison result is not a victory lap

The reviewer grades four still-image dimensions on a0–5 scale. These are ordinal visual judgements, not instrument readings or human-panel consensus. The current images are more coherent and more recognisably handheld than the pre-GBA V6 frames, but none of the ten pairs is an honest sweep of all four dimensions against the selected Wario Land4 reference. The strict superiority gate remains open.

The strongest remaining deficit is authored foreground form: actual laundry machinery, weight-bearing orchard branches, believable apartment interiors, crowd-impact poses and distinctive structural apparatus. The next iteration should invest in those objects and real action sequences, not merely add another global filter, brighten everything, or increase texture resolution. Animation feel, uncoached comprehension, camera comfort and horror effectiveness remain human judgements.

That is the useful finding of this work: technical consistency removes distractions and makes the remaining artistic weaknesses easier to see. It does not automatically solve them.

## Evidence and source links

- [Native visual review and open gates](../../Documentation/VisualV6/GBA_GATES.md)
- [Pinned full-campaign audit](../../Documentation/VisualV6/GBA_P4_FULL_AUDIT.json)
- [Protected-source scope](../../Documentation/VisualV6/GBA_SCOPE_AUDIT.json)
- [Environment source credits](../../ArtSources/Ansimuz/CREDITS.md)
- [Church fragment credits](../../ArtSources/Ansimuz/church/CREDITS.md)
- [Nintendo screen specification](https://www.nintendo.com/en-gb/Hardware/Nintendo-History/Game-Boy-Advance-SP/Game-Boy-Advance-SP-627141.html)

All claimed images are produced by the native game. The source repository, raw negatives, hashes and separate test scopes make the next iteration recoverable without reconstructing this session from memory.
