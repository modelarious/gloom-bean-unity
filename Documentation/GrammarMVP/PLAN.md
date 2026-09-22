# Grammar + current-art MVP

Status: EXPERIMENTAL branch-only work. Parent source is visual/qualitybar-q1 at 0cf69af1dc5e50028eaf876c235ef3aee811296f.

Goal: prove one small end-to-end build-time pipeline: deterministic semantic/spatial grammar -> validated plan -> native Gloom Bean objects -> existing CampaignScenery/V6/Broad/QualityBar art dressing.

Scope:
1. A deterministic Python grammar emits several unseen room plans from seeds.
2. The grammar encodes an explore path, possession source, key, turn/Nail, alternate return path, optional Mercy, and exit.
3. A thin Unity campaign adapter loads those plans and builds them through StageBuilder/AtlasBuilder.
4. Existing art systems must dress generated physical objects; no generated screenshot or one-off scene artwork counts.
5. The normal authored Host Cycle remains untouched and selectable.
6. Windows Unity import/play validation is required before calling the native MVP accepted; the Windows host is currently offline.

Non-goals: runtime infinite generation, full Wario Land 4 grammar, generated new artwork, replacing authored campaign levels, or claiming final visual quality.
