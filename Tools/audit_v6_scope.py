"""Fail-closed scope audit for this visual branch; does not claim to parse C# semantics."""
from pathlib import Path
import subprocess,json,hashlib,argparse
ROOT=Path(__file__).resolve().parents[1]
BASE='4424d5c46256bc74cbfd402cb61f2d6c3183603f'
ALLOWED={
'Assets/GloomBean/Runtime/Foundation/GameRoot.cs',
'Assets/GloomBean/Runtime/Campaign/CampaignPresentation.cs',
'Assets/GloomBean/Runtime/Campaign/CampaignScenery.cs',
'Assets/GloomBean/Runtime/Campaign/ColossusMass.cs',
'Assets/GloomBean/Runtime/Campaign/NativePresentationChecks.cs',
'Assets/GloomBean/Runtime/Campaign/V6VisualArt.cs',
'Assets/GloomBean/Runtime/Campaign/V6VisualHud.cs',
'Assets/GloomBean/Runtime/Campaign/VisualReviewV6.cs',
'Assets/GloomBean/Runtime/Campaign/GbaDisplay.cs',
'Assets/GloomBean/Runtime/Campaign/GbaPixels.cs',
'Assets/GloomBean/Runtime/Campaign/GbaWorldSign.cs',
'Assets/GloomBean/Runtime/Campaign/GbaMechanismView.cs',
'Assets/GloomBean/Runtime/Campaign/GbaActionCapture.cs'}
def git(*args):return subprocess.check_output(['git','-C',str(ROOT),*args],text=True,encoding='utf-8').strip()
def tree(ref):
 result={}
 for line in git('ls-tree','-r',ref,'--','Assets/GloomBean/Runtime').splitlines():
  info,path=line.split('\t',1)
  if path.endswith('.cs'):result[path]=info.split()[-1]
 return result
ap=argparse.ArgumentParser();ap.add_argument('--output',type=Path,required=True);args=ap.parse_args()
before,after=tree(BASE),tree('HEAD');changed=sorted(p for p in set(before)|set(after) if before.get(p)!=after.get(p))
unexpected=[p for p in changed if p not in ALLOWED]
if unexpected:raise SystemExit('Unexpected runtime changes: '+repr(unexpected))
protected=[p for p in before if p not in ALLOWED]
assert all(before[p]==after.get(p) for p in protected)
assets={}
for path in ['Assets/GloomBean/Resources/MovementTuning.asset','Assets/GloomBean/Scenes/Boot.unity','Packages','ProjectSettings']:
 same=git('rev-parse',BASE+':'+path)==git('rev-parse','HEAD:'+path);assert same,path;assets[path]='UNCHANGED'
foundation=git('rev-parse','foundation-v0.1.0^{}');assert foundation=='84c52bb17fea2ff46dd45c34fed07ad005b05330'
result={'status':'PASS','baseline':BASE,'head':git('rev-parse','HEAD'),'protected_runtime_files_unchanged':len(protected),'unchanged_paths':protected,'reviewed_presentation_and_test_paths':changed,'protected_assets':assets,'original_foundation_tag':foundation,'scope':'Exact protected-source identity plus enumerated changed rendering/test files. GameRoot draw callbacks, GbaDisplay camera framing and ColossusMass HUD suppression were separately reviewed; this script is not a semantic proof of those changes. Actual native regressions are separate.'}
args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print('V6_PROTECTED_SOURCE_AUDIT_PASS',len(protected),'unchanged runtime files;',len(changed),'explicit rendering/test changes')
