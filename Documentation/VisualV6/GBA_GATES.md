# Gates: V6 GBA presentation continuation P1

OWNS: Assets/GloomBean/Runtime/Campaign/Gba*, Assets/GloomBean/Runtime/Campaign/V6*, Assets/GloomBean/Runtime/Campaign/CampaignPresentation.cs, Assets/GloomBean/Runtime/Campaign/CampaignScenery.cs, Assets/GloomBean/Runtime/Campaign/NativePresentationChecks.cs, Assets/GloomBean/Runtime/Campaign/VisualReviewV6.cs, Assets/GloomBean/Runtime/Campaign/ColossusMass.cs, Assets/GloomBean/Runtime/Foundation/GameRoot.cs, Assets/GloomBean/Editor/BuildTools.cs, Assets/GloomBean/Resources/VisualV6/**, Tools/*gba*, Tools/*Gba*, Documentation/VisualV6/**, Documentation/Continuity/CURRENT_CHECKPOINT.json, docs/blog/**

Scope: Continue the already-published V6 branch; implement a coherent GBA-style native presentation, compare the same ten scenes, revise measured weaknesses, regress gameplay, preserve and publish every meaningful iteration.

- [x] P1: Prior complete source and this iteration are remote-verified on the existing public repository, without replacing its history.
  EVIDENCE: Public main and visual/v6 independently observed; every meaningful code/art iteration committed and pushed with matching remote SHA. Final exact SHA in delivery receipt.
- [x] P2: Native gameplay uses a 240x160 render surface, nearest integer upscale and a bitmap HUD; every upscaled logical pixel is constant inside its screen block.
  CHECK: python Tools/audit_gba_pixels.py Reports/VisualV6/gba_p8
  EXPECT: GBA_PIXEL_AUDIT_PASS
  EVIDENCE: PASS: P7/P8 twenty-image audits,14 actual-keyboard images, five positive/negative controls; see each retained gba-pixel-audit.json.
- [x] P3: Preserve all ten legacy camera fixtures and the nine attributed WL4 references; add explicitly labelled GBA framing without silently replacing weak examples.
  EVIDENCE: PASS: all ten original lenses and nine attributed WL4 reference images retained; explicitly labelled GBA viewpoints added. Original ten collider inventories unchanged.
- [x] P4: Grade style, character, clarity and polish across all examples; fix the first pass's concrete defects and recapture a second revision.
  EVIDENCE: PASS for execution of iteration/review: P1-P8 revisions and retained negative images. Current scorecard covers all10pairs; ties are not wins.
- [x] P5: Presentation-only source audit, zero added visual colliders, native mechanics and affected input-driven routes pass.
  EVIDENCE: PASS: protected154 runtime files/tuning/settings, P4 full54-case native regression, P7 six current focused cases, P8 exact-package three ending proofs. No production grants/geometry simplification.
- [ ] P6: All-example/all-dimension superiority to WL4 is demonstrated honestly.
  EVIDENCE: UNMET; baseline is visibly weaker, and GBA pixel compliance does not imply artistic superiority.
- [ ] P7: Actual playable package, comparison report, journal and current canonical checkpoint are published and independently verified.
  EVIDENCE: Preview/PDF/journal/source commits exist and were verified. Final source archive and canonical exact-delivery receipt are written after this snapshot; that later receipt resolves this final durability gate, not the strict-art gate.

The user's GBA-style requirement is an additional visual constraint, not authorization to change the atlas or original physics. High-resolution generated comparisons are rejected; Nintendo pixels never enter Assets. Human judgement remains distinct from build and script results.
