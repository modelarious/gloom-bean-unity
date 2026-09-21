# Quality-bar continuation: actual whole-game art

Scope: Continue the real Unity rendering from recovered Q4. The two generated concept scenes are targets, not native-game evidence. The original user requires a whole-game overhaul, not isolated improved rooms.

OWNS: Assets/GloomBean/Runtime/Campaign/QualityBar*, Assets/GloomBean/Runtime/Campaign/CampaignScenery.cs, Assets/GloomBean/Resources/QualityBar/**, Assets/GloomBean/Resources/BroadVisual/scene_*.png, Tools/*qualitybar*, Tools/*QualityBar*, Documentation/QualityBarQ1/**, Documentation/Continuity/CURRENT_CHECKPOINT.json, START_HERE.md, docs/blog/**

- [x] R1: Native rendering improvements apply by object/material and level profile in all25 stages, including later-spawned eligible objects. No review-coordinate predicates.
  EVIDENCE: PASS:466 native assertions include75 all25stage quality/late-spawn/disabled-source checks;167 protected source files unchanged. See AUDIT_Q8.json and Review/NATIVE_REGRESSION.json.
- [x] R2: Capture two additional genuinely unseen positions per stage and compare all125 existing positions before/after. Keep the52new WL4 references and don't select away weak shots.
  EVIDENCE: PASS:175 current images,50 separated F/G new positions,52 additional reference pixel hashes with zero overlap; all25 per-stage reviews and actual40action frames retained. Staging explicitly labelled.
- [x] R3: After visual review, correct the largest observed defects and build/capture again; run current native mechanics and input-driven route checks.
  EVIDENCE: PASS:Q5/Q6/Q7/Q8 actual iteration and current13-case suite; negative texture/lifecycle/import observations retained.
- [ ] R4: Native game meets the supplied concept quality and extreme whole-game visual-fidelity bar, retaining GBA-style pixels/readability.
  EVIDENCE: UNMET. Neither coverage nor any successful software test proves this manual judgement.
- [ ] R5: Publish the improved playable build, actual screenshots, source, retained negatives and precise next gate. Verify Git and derived Hindsight record.
  EVIDENCE: Native build and review are committed/pushed; final source archive and canonical/Hindsight closure receipt are written after snapshot. Those later acknowledgements resolve R5, not the still-open R4 quality gate.

Two previously interrupted workstreams have now been recovered. Active rendering work is visual/qualitybar-q1 at C:\Users\micha\Projects\GloomBean QualityBar Q1. BroadVisual source main worktree is separately retained and must not be overwritten. Same public origin; authorized SSH push works. No paid worker/agent is started. No gameplay/atlas requirement is weakened by an art iteration.
