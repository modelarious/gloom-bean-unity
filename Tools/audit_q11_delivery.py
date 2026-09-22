# Independent data audit; artistic acceptance is intentionally separate.
from pathlib import Path
import sys,json,hashlib,zipfile,importlib.util,subprocess
ROOT=Path(__file__).resolve().parents[1];sys.path[:0]=[str(ROOT/'.visual-tools'),r'C:\Users\micha\Projects\GloomBeanUnity\.visual-tools']
spec=importlib.util.spec_from_file_location('pixel',ROOT/'Tools/audit_gba_pixels.py');px=importlib.util.module_from_spec(spec);spec.loader.exec_module(px)
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
identity=load(ROOT/'Delivery/WholeGame-Q11/PACKAGE_IDENTITY.json');report=ROOT/'Reports/Delivery-WholeGame-Q11';run=load(report/'runner.json');native=load(ROOT/'Reports/Orchard-v04/QualityBar-Q11B-regression/runner.json')
assert run['status']=='PASS_NATIVE_RUNS_CAPTURE_REVIEW_PENDING' and len(run['actions'])==8 and native['status']=='PASS'
assert identity['sha256']==run['archive_sha256']==sha(Path(identity['path']))
assert identity['assembly_sha256']==run['assembly_sha256']==native['managed_assembly_sha256']
assert sha(Path(identity['extracted'])/'GloomBeanWindows/GloomBean_Data/Managed/Assembly-CSharp.dll')==identity['assembly_sha256']
with zipfile.ZipFile(identity['path']) as z:
 m=json.loads(z.read('PAYLOAD_SHA256.json'));assert z.testzip() is None
 for path,row in m['files'].items():assert len(z.read(path))==row['bytes'] and hashlib.sha256(z.read(path)).hexdigest()==row['sha256']
results=[];frames=0
for action in run['actions']:
 assert action['exit']==0 and action['failed']==0 and action['checks']>0
 folder=report/action['name'];assert (folder/'player.exit').read_text().strip()=='0'
 route_files=[f for f in folder.glob('*-result.json')];assert len(route_files)==1
 route=load(route_files[0]);assert route['failed']==0 and route['checks']==action['checks']
 raw=load(folder/'MotionAction/native-action.json')['shots'];assert len(raw)==action['native_frames'] and len(raw)>1
 prior=-1;fps=[]
 for shot in raw:
  assert shot['cameraFollow'] and shot['input'] in ['ScriptedInput','WitnessInput'] and shot['time']>prior;prior=shot['time']
  p=folder/'MotionAction'/shot['file'];assert px.measure(p)['pass'];frames+=1
 results.append({'name':action['name'],'checks':route['checks'],'frames':len(raw),'source_seconds':raw[-1]['time']-raw[0]['time'],'pixel_blocks':'PASS','actual_input_follow':'PASS'})
endings=[]
for record in run['endings']:
 name=record['name'];folder=report/'Endings'/name;e=load(folder/'ENDING.json');save=load(folder/'test-save.json');route=load(folder/'empyrean-result.json');expected={'Ordinary':0,'Nineteen':19,'Restored':20}[name]
 assert (folder/'player.exit').read_text().strip()=='0' and route['failed']==0 and route['checks']==record['checks']
 assert len(set(save['cleared']))==25 and len(set(save['mercies']))==expected and save['corrupted']
 assert e['earned_campaign'] and e['restored_ending']==(expected==20) and e['normal_figure_drawn']==(expected==20) and e['gameplay_corruption_retained']
 assert record['seed_sha256']==sha(folder/'earned-parent-save.json') and px.measure(folder/'ending-screen.png')['pass']
 endings.append({'name':name,'mercies':expected,'checks':route['checks'],'native_image_sha256':sha(folder/'ending-screen.png'),'seed_sha256':record['seed_sha256']})
ui=load(report/'KeyboardUI/ui-result.json');assert len(ui['screens'])==14 and ui['isolated_save_has_no_progress'] and ui['status']!='FAIL'
for name in ui['screens']:assert px.measure(report/'KeyboardUI'/(name+'.png'))['pass']
mechanics=load(ROOT/'Reports/Orchard-v04/QualityBar-Q11B-regression/Mechanics-all25-renderers/verification.json');assert mechanics['failed']==mechanics['exceptions']==0 and all(t['pass'] for t in mechanics['tests']) and sum(t['pass'] for t in mechanics['tests'])==mechanics['passed']
assert not subprocess.check_output(['git','-C',str(ROOT),'diff','--name-only',identity['runtime_commit'],'HEAD','--','Assets','Packages','ProjectSettings'],text=True).strip()
result={'status':'PASS_TECHNICAL_NATIVE_PACKAGE','runtime':identity['runtime_commit'],'assembly_sha256':identity['assembly_sha256'],'player_archive':identity,'payloads_verified':len(m['files']),'current_scenarios':len(native['results']),'mechanics_assertions':mechanics['passed'],'motion_frames':frames,'motion':results,'endings':endings,'keyboard_frames':14,'pixel_oracle_controls':px.controls(),'concept_quality':'UNMET','scope':'Data integrity, exact-package replay, actual captured input/camera path and pixel-grid validation. Not an animation-quality or visual-superiority judgement; the prior full-campaign certificate remains tied to its own source.'}
out=ROOT/'Documentation/QualityBarQ1/Q11Review/DELIVERY_AUDIT.json';out.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8');print('Q11_EXACT_PACKAGE_AUDIT_PASS',frames,'motion frames;',len(endings),'endings;',mechanics['passed'],'native assertions')
