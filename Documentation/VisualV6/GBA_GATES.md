# Gates: V6 GBA presentation continuation P1

OWNS: Assets/GloomBean/Runtime/Campaign/Gba*, Assets/GloomBean/Runtime/Campaign/V6*, Assets/GloomBean/Runtime/Campaign/CampaignPresentation.cs, Assets/GloomBean/Runtime/Campaign/CampaignScenery.cs, Assets/GloomBean/Runtime/Campaign/NativePresentationChecks.cs, Assets/GloomBean/Runtime/Campaign/VisualReviewV6.cs, Assets/GloomBean/Runtime/Campaign/ColossusMass.cs, Assets/GloomBean/Runtime/Foundation/GameRoot.cs, Assets/GloomBean/Editor/BuildTools.cs, Assets/GloomBean/Resources/VisualV6/**, Tools/*gba*, Tools/*Gba*, Documentation/VisualV6/**, Documentation/Continuity/CURRENT_CHECKPOINT.json, docs/blog/**

Scope: Continue the already-published V6 branch; implement a coherent GBA-style native presentation, compare the same ten scenes, revise measured weaknesses, regress gameplay, preserve and publish every meaningful iteration.

- [ ] P1: Prior complete source and this iteration are remote-verified on the existing public repository, without replacing its history.
  EVIDENCE: main a504e962 and visual/v6 4424d5c independently observed; new commits pending.
- [ ] P2: Native gameplay uses a 240x160 render surface, nearest integer upscale and a bitmap HUD; every upscaled logical pixel is constant inside its screen block.
  CHECK: python Tools/audit_gba_pixels.py Reports/VisualV6/gba_p2
  EXPECT: GBA_PIXEL_AUDIT_PASS
  EVIDENCE: pending; inspect source-sized captures, not a high-resolution mockup.
- [ ] P3: Preserve all ten legacy camera fixtures and the nine attributed WL4 references; add explicitly labelled GBA framing without silently replacing weak examples.
  EVIDENCE: pending raw captures and geometry audit.
- [ ] P4: Grade style, character, clarity and polish across all examples; fix the first pass's concrete defects and recapture a second revision.
  EVIDENCE: pending; analyst judgements, ties are not wins.
- [ ] P5: Presentation-only source audit, zero added visual colliders, native mechanics and affected input-driven routes pass.
  EVIDENCE: pending; no gameplay grants, geometry simplification or threshold weakening.
- [ ] P6: All-example/all-dimension superiority to WL4 is demonstrated honestly.
  EVIDENCE: UNMET; baseline is visibly weaker, and GBA pixel compliance does not imply artistic superiority.
- [ ] P7: Actual playable package, comparison report, journal and current canonical checkpoint are published and independently verified.
  EVIDENCE: pending.

The user's GBA-style requirement is an additional visual constraint, not authorization to change the atlas or original physics. High-resolution generated comparisons are rejected; Nintendo pixels never enter Assets. Human judgement remains distinct from build and script results.
