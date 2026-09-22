# Grammar MVP start here

Branch: experiment/grammar-art-mvp
Worktree on Punchcard: /home/modelarious/Projects/gloom-bean-grammar-mvp
Parent: visual/qualitybar-q1 @ 0cf69af1dc5e50028eaf876c235ef3aee811296f

This experiment does not replace the authored campaign. Read PLAN.md and GATES.md before changing it.

Core design: Tools/grammar_mvp.py creates deterministic held-out level plans. Unity loads them through GrammarMvpCampaign and creates ordinary StageBuilder/AtlasBuilder objects. AtlasBuilder.Finish installs CampaignScenery, which installs V6WorldDressing, BroadWorldDressing, and QualityBarWorld. Therefore the test is whether current object-driven art survives unseen geometry.

Implementation checkpoint: d2e74aa29c6031cd41c80ef9b9415f428e7d58c0. GM02-GM06 are statically verified against the committed branch. First failing gate is GM07: Windows Unity import/play and screenshot inspection remain UNKNOWN until DESKTOP-S717DNM is online.
