#!/usr/bin/env python3
"""Independent read-only audit of actual native receipts and earned save lineage.
Does not run Unity, grant progression, fix failed receipts or certify human experience.
"""
from __future__ import annotations
import argparse,hashlib,json,re,struct,subprocess,sys
from pathlib import Path

class AuditFailure(RuntimeError): pass

def need(value,message):
    if not value: raise AuditFailure(message)
def load(path): return json.loads(Path(path).read_text(encoding='utf-8-sig'))
def sha(path): return hashlib.sha256(Path(path).read_bytes()).hexdigest()
def count(result):
    value=result.get('passed',None)
    return int(value)+int(result['failed']) if value is not None else int(result['checks'])

def audit_case(root:Path,case:dict,spec:dict)->dict:
    name=case['name'];need(re.fullmatch(r'[A-Za-z0-9-]+',name),'Unsafe case path')
    need(case['status']=='PASS',name+': runner is not PASS')
    folder=root/name;suite=spec['suite']
    result_file='verification.json' if suite in ('Foundation','Mechanics') else suite.lower()+'-result.json'
    result=load(folder/result_file);exit_code=(folder/'player.exit').read_text().strip()
    need(result.get('failed') is not None,name+': missing failed count')
    denied=bool(spec.get('expectedDenial'))
    if denied:
        phrase=suite+(' stage' if spec.get('expectedStageDenial') else '')+' was not earned by the supplied real save'
        text=(folder/(suite.lower()+'-observations.txt')).read_text(encoding='utf-8')
        need(exit_code=='1' and result['failed']==1 and phrase in text,name+': wrong negative control')
    else:
        need(exit_code=='0' and case.get('exit')==0,name+': nonzero/missing native exit')
        need(result['failed']==0 and result.get('exceptions',0)==0,name+': native failure')
        need(count(result)>0 and count(result)==case['assertions'],name+': inconsistent assertion count')
        if suite in ('Foundation','Mechanics'):
            tests=result.get('tests',[])
            need(len(tests)==count(result) and all(t.get('pass') is True for t in tests),name+': component evidence disagrees')
        else:
            lines=(folder/(suite.lower()+'-observations.txt')).read_text(encoding='utf-8').splitlines()
            need(not any(x.startswith(('FAIL ','ERROR ')) for x in lines),name+': concealed failure in observations')
            need(sum(x.startswith('PASS ') for x in lines)==count(result),name+': traces counted as assertions')
    parent=spec.get('parent')
    if parent:
        need(re.fullmatch(r'[A-Za-z0-9-]+',parent),'Unsafe parent path')
        receipt=load(folder/'SAVE_PROVENANCE.json')
        expected=sha(root/parent/'test-save.json')
        need(receipt.get('parent')==parent and receipt.get('sha256')==expected,name+': parent provenance mismatch')
        need(sha(folder/'earned-parent-save.json')==expected,name+': parent save changed in transit')
    if spec.get('practice'):
        save=load(folder/'saved-progress.json')
        need(not save.get('cleared') and not save.get('mercies') and not save.get('corrupted') and save.get('totalCoins',0)==0,name+': practice leaked progress')
    return {'name':name,'assertions':count(result),'expected_denial':denied,'result_sha256':sha(folder/result_file)}

def audit_ending(root:Path,name:str,mercy_count:int)->dict:
    folder=root/name;save=load(folder/'test-save.json');ending=load(folder/'ENDING.json')
    expected={f'GB-L{n:02d}' for n in range(1,21)}|{f'GB-B{n}' for n in range(1,6)}
    need(set(save.get('cleared',[]))==expected and len(save['cleared'])==25,name+': full campaign not earned')
    expected_mercy={f'GB-L{n:02d}-MERCY' for n in range(1,mercy_count+1)}
    need(set(save.get('mercies',[]))==expected_mercy and len(save['mercies'])==mercy_count,name+': wrong exact secrets')
    need(save.get('corrupted') is True and ending.get('gameplay_corruption_retained') is True,name+': corruption lost')
    restored=mercy_count==20
    need(ending.get('earned_campaign') is True and ending.get('mercies')==mercy_count,name+': ending not from earned save')
    need(ending.get('restored_ending') is restored and ending.get('normal_figure_drawn') is restored,name+': wrong ending artwork state')
    image=(folder/'ending-screen.png').read_bytes()
    need(image[:8]==b'\x89PNG\r\n\x1a\n' and len(image)>1000,name+': missing ending capture')
    width,height=struct.unpack('>II',image[16:24]);need(width>=640 and height>=400,name+': invalid backbuffer size')
    return {'name':name,'mercies':mercy_count,'cleared':25,'restored':restored,'image_sha256':hashlib.sha256(image).hexdigest(),'save_sha256':sha(folder/'test-save.json')}

def audit(config_path:Path,source_root:Path|None=None)->dict:
    config=load(config_path);root=Path(config['reports']);receipt=load(root/'runner.json')
    need(receipt.get('status')=='PASS' and receipt.get('phase')=='finished','Run incomplete or failed')
    spec={x['name']:x for x in config['cases']};cases={x['name']:x for x in receipt['cases']}
    need(len(spec)==len(config['cases']) and len(cases)==len(receipt['cases']),'Duplicate case names')
    need(set(spec)==set(cases),'Missing or unexpected native cases')
    required={'Foundation','Mechanics','Final-ordinary','Final-restored','Final-nineteen-not-restored','L19-practice-secret','Final-practice','Scripture-no-Ink-control'}
    required|={'Pair-'+x for x in ('magnet-shadow','echo-ink','wax-gullet','stitch-coffin','mirror-parallax')}
    need(required<=set(cases),'Coverage contract omits a required result')
    facts=[audit_case(root,cases[name],spec[name]) for name in spec]
    endings=[audit_ending(root,name,n) for name,n in [('Final-ordinary',0),('Final-nineteen-not-restored',19),('Final-restored',20)]]
    need(endings[0]['image_sha256']!=endings[2]['image_sha256'],'Normal and restored backbuffers are identical')
    builds={}
    for edition in ('foundation','atlas'):
        build=load(root/(edition+'-build.json'));need(build.get('result')=='Succeeded' and build.get('errors')==0,edition+': native build failure');builds[edition]=build
    source=receipt['atlas_source'];need(source['head']==config['atlas_commit'],'Pinned source differs')
    if source_root is not None:
        def git(*args):
            r=subprocess.run(['git','-C',str(source_root),*args],capture_output=True,text=True,encoding='utf-8',timeout=15,check=True);return r.stdout.strip()
        need(not git('status','--porcelain','--','Assets','Packages','ProjectSettings'),'Uncommitted engine inputs')
        for name,tree in [('Assets','assets'),('Packages','packages'),('ProjectSettings','settings')]:need(git('rev-parse','HEAD:'+name)==source[tree],name+': release source differs from tested source')
    assembly=Path(config['atlas'])/'Builds/Windows/GloomBean_Data/Managed/Assembly-CSharp.dll'
    need(sha(assembly)==receipt['managed_assembly_sha256'],'Tested player assembly changed')
    return {'status':'PASS','scope':'Native complete campaign, exact earned-save lineage and ending boundaries; not blind human acceptance or public publication','source':source,'managed_assembly_sha256':receipt['managed_assembly_sha256'],'case_count':len(facts),'expected_negative_cases':sum(x['expected_denial'] for x in facts),'cases':facts,'endings':endings,'builds':builds,'runner_sha256':sha(root/'runner.json'),'config_sha256':sha(config_path),'manual_acceptance':'UNKNOWN'}

def main():
    ap=argparse.ArgumentParser(description=__doc__);ap.add_argument('--config',type=Path,required=True);ap.add_argument('--source',type=Path);ap.add_argument('--output',type=Path);args=ap.parse_args()
    try:
        result=audit(args.config,args.source)
        if args.output:
            args.output.parent.mkdir(parents=True,exist_ok=True)
            with args.output.open('x',encoding='utf-8') as f:json.dump(result,f,indent=2);f.write('\n')
        print(json.dumps(result,indent=2));print('NATIVE_RELEASE_AUDIT_PASS');return 0
    except (OSError,ValueError,KeyError,AuditFailure,subprocess.SubprocessError) as e:
        print(json.dumps({'status':'FAIL','error':str(e)}));return 1
if __name__=='__main__':sys.exit(main())
