#!/usr/bin/env python3
"""Bounded project orientation. --probe-write touches only a unique temporary file."""
import argparse,hashlib,json,os,subprocess,sys,uuid
from pathlib import Path

def git(root,*args,timeout=12):
 r=subprocess.run(['git','-C',str(root),*args],capture_output=True,text=True,encoding='utf-8',errors='replace',timeout=timeout,env={**os.environ,'GIT_TERMINAL_PROMPT':'0'})
 return r.returncode,r.stdout.strip(),r.stderr.strip()
def fingerprint(root):
 rc,out,err=git(root,'ls-files','-z','--','Assets','Packages','ProjectSettings')
 if rc:raise RuntimeError(err)
 h=hashlib.sha256();count=0
 for rel in sorted(filter(None,out.split('\0'))):
  f=root/rel
  if not f.is_file():raise RuntimeError('Tracked source missing: '+rel)
  b=f.read_bytes()
  if f.suffix.lower() in {'.cs','.meta','.json','.asset','.unity','.txt','.shader','.asmdef'}:b=b.replace(b'\r\n',b'\n')
  h.update(rel.encode()+b'\0'+hashlib.sha256(b).digest());count+=1
 return {'files':count,'normalized_sha256':h.hexdigest()}
def evidence(root,item):
 f=(root/item['path']).resolve();f.relative_to(root);r={'name':item['name'],'path':item['path']}
 if not f.is_file():return {**r,'status':'MISSING'}
 raw=f.read_bytes();r['sha256']=hashlib.sha256(raw).hexdigest()
 try:
  d=json.loads(raw.decode('utf-8-sig'))
  if 'cases' in d or 'results' in d:r.update(status=d.get('status','UNKNOWN'),cases=d.get('cases',d.get('results')))
  else:
   e=f.parent/item.get('exit_file','player.exit');code=e.read_text(encoding='utf-8-sig').strip() if e.exists() else 'MISSING'
   r.update({k:d.get(k) for k in ['passed','checks','failed','exceptions']});r['exit']=code
   r['status']='PASS' if d.get('failed')==0 and d.get('exceptions',0)==0 and code=='0' else 'FAIL_OR_INCOMPLETE'
 except (ValueError,OSError) as e:r.update(status='UNREADABLE',error=str(e))
 return r
def inspect(root,probe_write=False):
 root=Path(root).resolve();rc,head,error=git(root,'rev-parse','HEAD')
 if rc:raise RuntimeError('Not an accessible Git project: '+error)
 f=root/'Documentation/Continuity/CURRENT_CHECKPOINT.json';s=json.loads(f.read_text(encoding='utf-8-sig')) if f.exists() else {}
 dirty=git(root,'status','--short')[1].splitlines();tested=s.get('tested_commit')
 r={'project':str(root),'head':head,'branch':git(root,'branch','--show-current')[1],'changes':dirty[:60],'changes_truncated':len(dirty)>60,'recent_commits':git(root,'log','-8','--format=%h %s')[1].splitlines(),'remote_names':git(root,'remote')[1].splitlines(),'checkpoint_found':f.exists(),'next_gate':s.get('next_gate','UNKNOWN: read START_HERE.md'),'source_fingerprint':fingerprint(root),'tested_source':tested,'publication':s.get('publication',{'status':'UNKNOWN'}),'active_job':s.get('active_job'),'evidence':[evidence(root,i) for i in s.get('evidence_refs',[])],'scope':'Inspection only. No gameplay tests, builds or remote publication performed.'}
 if tested:
  rc,_,_=git(root,'diff','--quiet',tested,'--','Assets','Packages','ProjectSettings');u=git(root,'ls-files','--others','--exclude-standard','--','Assets','Packages','ProjectSettings')[1]
  r['source_vs_tested']='MATCH' if rc==0 and not u else ('DIFF' if rc in (0,1) else 'UNKNOWN')
 else:r['source_vs_tested']='UNKNOWN'
 if probe_write:
  folder=root/'.continuity';folder.mkdir(exist_ok=True);f=folder/('probe-'+uuid.uuid4().hex+'.tmp');value=os.urandom(32)
  try:
   with f.open('xb') as stream:stream.write(value)
   if f.read_bytes()!=value:raise RuntimeError('Write probe read-back mismatch')
   r['write_probe']='PASS'
  finally:
   if f.exists():f.unlink()
 return r
def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--project',type=Path,default=Path(__file__).resolve().parent.parent);p.add_argument('--probe-write',action='store_true');a=p.parse_args()
 try:print(json.dumps(inspect(a.project,a.probe_write),indent=2));print('CHECKPOINT_INSPECTION_COMPLETE (not gameplay acceptance)');return 0
 except (OSError,ValueError,RuntimeError,subprocess.TimeoutExpired) as e:print(json.dumps({'status':'INSPECTION_FAILED','error':str(e)}));return 1
if __name__=='__main__':sys.exit(main())
