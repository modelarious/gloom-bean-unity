# Grammar MVP gates

- [x] GM01 Isolated branch/worktree starts at exact visual/qualitybar-q1 revision 0cf69af1dc5e50028eaf876c235ef3aee811296f.
  CHECK: git merge-base --is-ancestor 0cf69af1dc5e50028eaf876c235ef3aee811296f HEAD && git branch --show-current
  EXPECT: experiment/grammar-art-mvp
- [x] GM02 Build-time grammar is deterministic and validates every generated plan.
  CHECK: python3 Tools/grammar_mvp.py --check
  EXPECT: GRAMMAR_MVP_CHECK_PASS
- [x] GM03 At least three distinct seeded plans contain explore, turn, alternate return, optional branch, and exit semantics.
  CHECK: python3 Tools/grammar_mvp.py --audit
  EXPECT: GRAMMAR_MVP_AUDIT_PASS
- [x] GM04 Unity adapter builds generated geometry through StageBuilder/AtlasBuilder and calls the existing art stack, without mutating authored AtlasCampaign levels.
  CHECK: python3 Tools/check_grammar_mvp_source.py
  EXPECT: GRAMMAR_MVP_SOURCE_PASS
- [x] GM05 Experiment is exposed as its own campaign/menu source; normal Host Cycle practice still selects AtlasCampaign.
  CHECK: python3 Tools/check_grammar_mvp_source.py
  EXPECT: GRAMMAR_MVP_SOURCE_PASS
- [x] GM06 Branch has no edits to existing authored AtlasCampaign level files.
  CHECK: git diff --name-only 0cf69af1dc5e50028eaf876c235ef3aee811296f...HEAD
  EXPECT: reviewed: no Assets/GloomBean/Runtime/Campaign/AtlasCampaign.* level-authoring files
- [ ] GM07 Native Windows Unity import, one generated-stage play, and screenshot/art-stack inspection. **UNKNOWN / first failing gate.**
  MANUAL: UNKNOWN while DESKTOP-S717DNM is offline; do not report PASS from static checks.
