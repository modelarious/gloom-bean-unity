"""Independent Q9 coverage/source audit; it cannot certify beauty or animation feel."""
from pathlib import Path
from collections import Counter
import sys,hashlib,json,math,subprocess,importlib.util,argparse
ROOT=Path(__file__).resolve().parents[1];sys.path[:0]=[str(ROOT/'.visual-tools'),str(ROOT.parent/'GloomBeanUnity/.visual-tools')]
from PIL import Image
spec=importlib.util.spec_from_file_location('px',ROOT/'Tools/audit_gba_pixels.py');px=importlib.util.module_from_spec(spec);spec.loader.exec_module(px)
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def tree(ref):
 data={}
 for line in subprocess.check_output(['git','-C',str(ROOT),'ls-tree','-r',ref,'--','Assets/GloomBean/Runtime'],text=True).splitlines():
  fields,path=line.split('\t',1)
  if path.endswith('.cs'):data[path]=fields.split()[2]
 return data
ap=argparse.ArgumentParser();ap.add_argument('run');args=ap.parse_args();folder=ROOT/'Reports/BroadVisual'/args.run
meta=json.loads((folder/'broad-result.json').read_text());runner=json.loads((folder/'runner.json').read_text(encoding='utf-8-sig'))
assert meta['status']=='PASS' and not meta['errors'] and runner['status']=='PASS' and runner['exit']==0
expected={f'GB-L{i:02}' for i in range(1,21)}|{f'GB-B{i}' for i in range(1,6)};counts=Counter(s['stage'] for s in meta['shots']);assert set(counts)==expected and set(counts.values())=={9}
images=[];new=[];seen={};coverage=[]
for shot in meta['shots']:
 image=px.measure(folder/(shot['id']+'.png'));assert image['pass'],shot['id'];images.append(image)
 assert shot['eligible']>0 and shot['eligible']==shot['applied'] and shot['visualColliders']==0
 assert shot['qualityEligible']>0 and shot['qualityEligible']==shot['qualityApplied'] and shot['qualityColliders']==0
 previous=seen.setdefault(shot['stage'],[])
 if shot['stratum']>=7:
  distance=min(math.hypot(shot['cx']-x,shot['cy']-y) for x,y in previous);assert distance>=3.1-1e-4,(shot['id'],distance)
  if shot['stratum']==8 and not shot['stage'].startswith('GB-B'):assert shot['returned']
  new.append({'id':shot['id'],'minimum_prior_camera_distance':distance,'returned':shot['returned']})
 previous.append((shot['cx'],shot['cy']));coverage.append({k:shot[k] for k in ['id','stage','eligible','applied','qualityEligible','qualityApplied','qualityColliders']})
assert len(new)==50
# Retain all earlier anchors; moving supports can change actual coordinates and are recorded honestly.
old=json.loads((ROOT/'Reports/BroadVisual/Q8/broad-result.json').read_text())['shots'];now={s['id']:s for s in meta['shots']};retained=[]
assert len(old)==175
for shot in old:
 n=now[shot['id']];assert n['anchor']==shot['anchor'] and n['form']==shot['form'] and n['stratum']==shot['stratum']
 retained.append({'id':shot['id'],'same_anchor':True,'camera_delta':[round(n[k]-shot[k],4) for k in ['cx','cy','size']]})
# Check all previous reference images and exclude their decoded pixels from the new corpus.
previous=set()
for path in (ROOT/'Documentation/VisualV6/References').glob('*.png'):previous.add(hashlib.sha256(Image.open(path).convert('RGB').tobytes()).hexdigest())
base=ROOT/'Documentation/BroadVisual/References';reference=json.loads((base/'SOURCES.json').read_text())['files'];assert len(reference)==52
for row in reference:
 path=base/row['file'];assert sha(path)==row['sha256'];previous.add(hashlib.sha256(Image.open(path).convert('RGB').tobytes()).hexdigest())
extra=ROOT/'Documentation/QualityBarQ1/ReferencesQ9';rows=json.loads((extra/'SOURCES.json').read_text())['images'];added=set();categories=Counter()
for row in rows:
 path=extra/row['file'];assert sha(path)==row['sha256'];pixels=hashlib.sha256(Image.open(path).convert('RGB').tobytes()).hexdigest();assert pixels not in previous and pixels not in added;added.add(pixels);categories[row['category']]+=1
assert categories=={'gameplay-comparison':10,'presentation-only-not-environment-comparison':6}
before=tree('a4386033c564f736cc00885ab4a152124a454d8a');after=tree(runner['source'])
allowed={'HostPixelArt.cs','HostPixelView.cs','QualityBarWorld.cs','GbaWorldSign.cs','GbaActionCapture.cs','NativePresentationChecks.cs','BroadVisualCapture.cs','BroadVisualChecks.cs'}
changed=[f for f in before if before[f]!=after.get(f)];assert all(Path(f).name in allowed for f in changed),changed
protected=[f for f in before if Path(f).name not in allowed];assert all(before[f]==after.get(f) for f in protected)
newfiles=[f for f in after if f not in before];assert {Path(f).name for f in newfiles}<={'QualityBarConstruction.cs','QualityBarMotion.cs','QualityBarMotionChecks.cs'}
result={'status':'PASS','source':runner['source'],'assembly':runner['assembly'].lower(),'stage_count':25,'images':images,'per_stage_counts':dict(counts),'new_HI_views':new,'retained_Q8_views':retained,'native_pixel_oracle_controls':px.controls(),'application':coverage,'previous_WL4_gameplay_references':52,'new_WL4_gameplay':10,'new_WL4_presentation_only':6,'decoded_reference_overlap':0,'protected_runtime_files':len(protected),'changed_existing_runtime':changed,'new_runtime':newfiles,'strict_concept_quality':'UNMET','scope':'225 real pixel-grid captures,25-stage base-art application,50 new support positions and untouched protected gameplay sources. These are not continuous coverage of every possible frame, reachable staged poses, complete collider-bound equality, or artistic superiority. New object-driven construction/lifecycle and motion-selection tests are separate native evidence.'}
out=ROOT/'Documentation/QualityBarQ1'/('AUDIT_'+args.run+'.json');out.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print('WHOLE_GAME_Q9_AUDIT_PASS',len(images),'images;',len(new),'new positions;',len(protected),'unchanged gameplay sources;',categories)
