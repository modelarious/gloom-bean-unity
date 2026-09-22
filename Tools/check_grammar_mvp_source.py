#!/usr/bin/env python3
from pathlib import Path
import subprocess, sys

ROOT=Path(__file__).resolve().parents[1]
campaign=(ROOT/'Assets/GloomBean/Runtime/Grammar/GrammarMvpCampaign.cs').read_text()
root=(ROOT/'Assets/GloomBean/Runtime/Foundation/GameRoot.cs').read_text()
scenery=(ROOT/'Assets/GloomBean/Runtime/Campaign/CampaignScenery.cs').read_text()
errors=[]

for needle in (
    'Resources.Load<TextAsset>("GrammarMvp/"+definition.id)',
    'var a=new AtlasBuilder(builder,definition);',
    'a.Finish();',
    'grammar-return-revealed',
):
    if needle not in campaign: errors.append('campaign:'+needle)
for needle in ('GrammarMvpCampaign, Assembly-CSharp','GRAMMAR MVP / GENERATED ROOMS','Practice=grammar','GetType().Name=="AtlasCampaign"'):
    if needle not in root: errors.append('gameroot:'+needle)
for needle in ('AddComponent<V6WorldDressing>()','AddComponent<BroadWorldDressing>()','AddComponent<QualityBarWorld>()'):
    if needle not in scenery: errors.append('art-stack:'+needle)

changed=subprocess.check_output(
    ['git','diff','--name-only','0cf69af1dc5e50028eaf876c235ef3aee811296f'],
    cwd=ROOT,text=True
).splitlines()
bad=[p for p in changed if p.startswith('Assets/GloomBean/Runtime/Campaign/AtlasCampaign.')]
if bad: errors.append('authored-level-edits:'+','.join(bad))
plans=list((ROOT/'Assets/GloomBean/Resources/GrammarMvp').glob('GB-GRAMMAR-*.json'))
if len(plans)!=4: errors.append('plan-count:'+str(len(plans)))

if errors:
    print('GRAMMAR_MVP_SOURCE_FAIL')
    for e in errors: print(e)
    sys.exit(1)
print('plans=4 art_stack=V6+Broad+QualityBar authored_atlas_edits=0')
print('GRAMMAR_MVP_SOURCE_PASS')
