# P3 native review and P4 target

Native captures at ca00f825 show restored WE / WERE / HERE signs in Scripture and real object labels. The fixed texture path now owns freshly authored bitmap pixels rather than copying an unreadable texture. The rendering is still an actual240x160 framebuffer with integer-scale GUI.

P3 exposes the larger compositional defect: the scenic assets were framed as giant cropped fragments. At the tighter camera, a single window or blank manuscript paragraph occupied the entire background. P4 instead fits the entire existing authored panorama vertically, preserving aspect and parallax/repetition, so the cathedral vault, illuminated manuscript and receding buildings can be read as compositions. This changes only background rendering scale; camera witnesses, player size, colliders and source interactions stay unchanged.

P4 also restores the combined foundation's required-seal HUD and adds native GPU glyph/cache lifecycle checks. Strict subjective superiority remains unmet pending the actual pair review.
