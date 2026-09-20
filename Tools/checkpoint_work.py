#!/usr/bin/env python3
"""Commit explicit owned paths, preserve full Git history, report actual durability."""
import argparse,datetime,hashlib,json,os,subprocess,sys,uuid
from pathlib import Path

def run(root,*args,timeout=25,checked=True):
 p=subprocess.run(['git','-C',str(root),*args],capture_output=True,text=True,encoding='utf-8',errors='replace',timeout=timeout,env={**os.environ,'GIT_TERMINAL_PROMPT':'0'})
 if checked and p.returncode:raise RuntimeError(p.stderr.strip() or 'Git command failed')
 return p
def checkpoint(root,expected_head,message,paths,publish=False):
 root=Path(root).resolve();current=run(root,'rev-parse','HEAD').stdout.strip()
 if current!=expected_head:raise RuntimeError('HEAD_CHANGED: refresh state; no files staged')
 if run(root,'diff','--cached','--name-only').stdout.strip():raise RuntimeError('EXISTING_STAGED_CHANGES: review ownership first')
 owned=[];forbidden={'.git','library','temp','obj','builds','logs','usersettings','.continuity'}
 for entry in paths:
  rel=Path(entry)
  if rel.is_absolute() or '..' in rel.parts or any(x.lower() in forbidden for x in rel.parts):raise ValueError('UNSAFE_OWNERSHIP_PATH: '+entry)
  if rel.name in {'.env','.git-credentials'} or rel.suffix.lower() in {'.key','.pem','.pfx'}:raise ValueError('SECRET_PATH_REFUSED: '+entry)
  (root/rel).resolve().relative_to(root)
  if entry in {'.',''}:raise ValueError('Explicit owned paths required')
  owned.append(rel.as_posix())
 if not owned:raise ValueError('No owned paths supplied')
 base=root/'.continuity';base.mkdir(exist_ok=True);lock=base/'checkpoint.lock';nonce=uuid.uuid4().hex
 with lock.open('x',encoding='utf-8') as stream:stream.write(nonce)
 stamp=datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ')+'-'+nonce[:8];dest=base/('checkpoint-'+stamp+'.json');receipt={'status':'STARTED','base':current,'owned_paths':owned}
 try:
  if run(root,'rev-parse','HEAD').stdout.strip()!=current:raise RuntimeError('HEAD_CHANGED_AFTER_LOCK')
  run(root,'add','--',*owned);changed=run(root,'diff','--cached','--name-only').stdout.strip()
  if changed:run(root,'commit','-m',message)
  head=run(root,'rev-parse','HEAD').stdout.strip();receipt.update(status='LOCAL_COMMITTED',commit=head,committed_paths=changed.splitlines())
  bundle=base/('history-'+stamp+'.bundle');run(root,'bundle','create',str(bundle),'--all',timeout=40);run(root,'bundle','verify',str(bundle));receipt['bundle']={'path':str(bundle),'sha256':hashlib.sha256(bundle.read_bytes()).hexdigest(),'bytes':bundle.stat().st_size};receipt['publication']='NOT_ATTEMPTED'
  if publish:
   remote=run(root,'remote','get-url','origin',checked=False)
   if remote.returncode:receipt['publication']='BLOCKED_NO_ORIGIN'
   else:
    branch=run(root,'branch','--show-current').stdout.strip()
    if not branch:raise RuntimeError('Cannot publish detached HEAD automatically')
    run(root,'push','origin','HEAD:refs/heads/'+branch,timeout=40);refs=run(root,'ls-remote','origin','refs/heads/'+branch).stdout.split()
    receipt['publication']='VERIFIED' if refs and refs[0]==head else 'REF_MISMATCH'
  receipt['receipt']=str(dest);return receipt
 except Exception as e:receipt.update(error=str(e),status='CHECKPOINT_INCOMPLETE');raise
 finally:
  dest.write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8')
  if lock.exists() and lock.read_text(encoding='utf-8')==nonce:lock.unlink()
def main():
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('--project',type=Path,default=Path(__file__).resolve().parent.parent);p.add_argument('--expected-head',required=True);p.add_argument('--message',required=True);p.add_argument('--paths',nargs='+',required=True);p.add_argument('--publish',action='store_true');a=p.parse_args()
 try:
  r=checkpoint(a.project,a.expected_head,a.message,a.paths,a.publish);print(json.dumps(r,indent=2));return 2 if a.publish and r['publication']!='VERIFIED' else 0
 except (OSError,RuntimeError,ValueError,subprocess.TimeoutExpired) as e:print(json.dumps({'status':'CHECKPOINT_FAILED','error':str(e)}));return 1
if __name__=='__main__':sys.exit(main())
