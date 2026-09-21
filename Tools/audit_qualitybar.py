"""Exact all-stage screenshot, novel-position, reference and protected-source audit.
This script cannot decide artistic quality. Strict visual quality is a separate open manual gate.
"""
from pathlib import Path
from collections import Counter
import sys,hashlib,json,math,subprocess,importlib.util,argparse
ROOT=Path(__file__).resolve().parents[1];sys.path[:0]=[str(ROOT/'.visual-tools'),str(ROOT.parent/'GloomBeanUnity/.visual-tools')]
from PIL import Image
spec=importlib.util.spec_from_file_location('px',ROOT/'Tools/audit_gba_pixels.py');px=importlib.util.module_from_spec(spec);spec.loader.exec_module(px)
def sha(f):return hashlib.sha256(f.read_bytes()).hexdigest()
def tree(ref):
 out={}
 for line in subprocess.check_output(['git','-C',str(ROOT),'ls-tree','-r',ref,'--','Assets/GloomBean/Runtime'],text=True).splitlines():
  desc,path=line.split('\t');
  if path.endswith('.cs'):out[path]=desc.split()[2]
 return out
ap=argparse.ArgumentParser();ap.add_argument('run');args=ap.parse_args();folder=ROOT/'Reports/BroadVisual'/args.run
meta=json.loads((folder/'broad-result.json').read_text());runner=json.loads((folder/'runner.json').read_text(encoding='utf-8-sig'))
assert meta['status']=='PASS' and not meta['errors'] and runner['exit']==0 and runner['status']=='PASS'
stages={f'GB-L{i:02}' for i in range(1,21)}|{f'GB-B{i}' for i in range(1,6)};counts=Counter(s['stage'] for s in meta['shots']);assert set(counts)==stages and set(counts.values())=={7}
images=[];novel=[];seen={};coverage=[]
for sh in meta['shots']:
 image=px.measure(folder/(sh['id']+'.png'));assert image['pass'],sh['id'];images.append(image)
 assert sh['eligible']>0 and sh['eligible']==sh['applied'] and sh['visualColliders']==0
 assert sh['qualityEligible']>0 and sh['qualityEligible']==sh['qualityApplied'] and sh['qualityColliders']==0,sh['id']
 previous=seen.setdefault(sh['stage'],[])
 if sh['stratum']>=5:
  dist=min(math.hypot(sh['cx']-x,sh['cy']-y) for x,y in previous);assert dist>=3.1-1e-4,(sh['id'],dist);novel.append({'id':sh['id'],'minimum_prior_camera_distance':dist})
 previous.append((sh['cx'],sh['cy']));coverage.append({k:sh[k] for k in ['id','stage','eligible','applied','qualityEligible','qualityApplied','qualityColliders']})
assert len(novel)==50
refs=ROOT/'Documentation/BroadVisual/References';manifest=json.loads((refs/'SOURCES.json').read_text());old={hashlib.sha256(Image.open(f).convert('RGB').tobytes()).hexdigest() for f in (ROOT/'Documentation/VisualV6/References').glob('*.png')}
new=[]
for row in manifest['files']:
 f=refs/row['file'];assert sha(f)==row['sha256'];h=hashlib.sha256(Image.open(f).convert('RGB').tobytes()).hexdigest();assert h not in old;new.append(h)
assert len(new)==52 and len(set(new))==52
before=tree('77a46bb7160b25c39713bf7b23dda2fe069e55d7');after=tree('HEAD');allowed={'QualityBarWorld.cs','CampaignScenery.cs','BroadVisualCapture.cs','BroadVisualChecks.cs'}
changed=[f for f in before if before[f]!=after.get(f)];assert all(Path(f).name in allowed for f in changed),changed
protected=[f for f in before if Path(f).name not in allowed];assert all(before[f]==after.get(f) for f in protected)
oldshots=json.loads((ROOT/'Reports/BroadVisual/Q4/broad-result.json').read_text())['shots'];now={s['id']:s for s in meta['shots']};comparisons=[]
for s in oldshots:
 n=now[s['id']];delta={k:round(n[k]-s[k],4) for k in ['cx','cy','size'] if abs(n[k]-s[k])>.01}
 comparisons.append({'id':s['id'],'same_anchor':n['anchor']==s['anchor'],'camera_deltas':delta,'note':'Do not call moving support or corrected-E sampling changes pixel-identical.'})
result={'status':'PASS','source':runner['source'],'assembly':runner['assembly'].lower(),'stage_count':len(stages),'levels':20,'bosses':5,'images':images,'per_stage_counts':dict(counts),'native_pixel_oracle_controls':px.controls(),'new_FG_views':novel,'application':coverage,'new_WL4_frames':52,'old_WL4_overlap':0,'protected_runtime_files':len(protected),'changed_existing_runtime':changed,'prior_camera_comparisons':comparisons,'strict_concept_visual_quality':'UNMET','scope':'Full175-view native pixel/renderer coverage,50 unseen support-derived positions,52 different reference pixel hashes and protected source identity. Not continuous whole-game visibility, reachable poses, equal collision bounds or artistic superiority.'}
out=ROOT/'Documentation/QualityBarQ1'/('AUDIT_'+args.run+'.json');out.write_text(json.dumps(result,indent=2)+'\n');print('QUALITY_NATIVE_AUDIT_PASS',len(stages),'stages',len(images),'views',len(novel),'new positions',len(protected),'unchanged runtime sources')
