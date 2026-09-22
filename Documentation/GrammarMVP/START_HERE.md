# Grammar MVP start here

Branch: experiment/grammar-art-mvp
Worktree on Punchcard: /home/modelarious/Projects/gloom-bean-grammar-mvp
Parent: visual/qualitybar-q1 @ 0cf69af1dc5e50028eaf876c235ef3aee811296f

This experiment does not replace the authored campaign. Read PLAN.md and GATES.md before changing it.

Core design: Tools/grammar_mvp.py creates deterministic held-out level plans. Unity loads them through GrammarMvpCampaign and creates ordinary StageBuilder/AtlasBuilder objects. AtlasBuilder.Finish installs CampaignScenery, which installs V6WorldDressing, BroadWorldDressing, and QualityBarWorld. Therefore the test is whether current object-driven art survives unseen geometry.

First failing gate is GM02 until generation exists. GM07 remains UNKNOWN until the Windows Unity host is online and the generated stage is inspected natively.
