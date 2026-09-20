#!/usr/bin/env python3
"""Package committed source and complete Git history; never stage or publish anything."""
import argparse,hashlib,io,json,re,subprocess,sys,zipfile
from pathlib import Path

def git(root,*args,raw=False):
 r=subprocess.run(['git','-C',str(root),*args],capture_output=True,timeout=40)
 if r.returncode:raise RuntimeError(r.stderr.decode('utf-8',errors='replace')[-1500:])
 return r.stdout if raw else r.stdout.decode('utf-8').strip()
def package(root,output,label):
 root=Path(root).resolve();output=Path(output).resolve()
 if not re.fullmatch(r'[A-Za-z0-9_-]{1,70}',label):raise ValueError('Label must be a simple unique filename component')
 if git(root,'status','--porcelain','--untracked-files=no'):raise RuntimeError('Commit reviewed tracked changes before packaging')
 head=git(root,'rev-parse','HEAD');branch=git(root,'branch','--show-current')
 if not branch:raise RuntimeError('Package a named branch, not a detached checkout')
 output.mkdir(parents=True,exist_ok=True);folder=output/label;folder.mkdir() # Never overwrite a checkpoint.
 bundle=folder/'history.bundle';git(root,'bundle','create',str(bundle),'--all');git(root,'bundle','verify',str(bundle))
 archive=git(root,'archive','--format=zip','HEAD',raw=True)
 manifest={'schema_version':1,'source_head':head,'source_branch':branch,'refs':git(root,'show-ref').splitlines(),'scope':'Committed source and complete history. Test results and publication status are separate; packaging does not certify the game.','files':[]}
 path=folder/'GloomBean_Execution_Checkpoint.zip'
 with zipfile.ZipFile(path,'w',zipfile.ZIP_DEFLATED) as target:
  def put(name,data):
   target.writestr(name,data);manifest['files'].append({'path':name,'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()})
  with zipfile.ZipFile(io.BytesIO(archive)) as source:
   for item in source.infolist():
    if not item.is_dir():put('GloomBeanUnity/'+item.filename,source.read(item.filename))
  put('GitHistory/history.bundle',bundle.read_bytes())
  for source,name in [('Documentation/Continuity/AGENT_CHECKPOINT.md','GloomBean_Agent_Checkpoint.md'),('Documentation/Continuity/CURRENT_CHECKPOINT.json','CURRENT_CHECKPOINT.json')]:
   data=git(root,'show','HEAD:'+source,raw=True);put(name,data)
  instructions='# Restore without overwriting work\n\nStart with GloomBean_Agent_Checkpoint.md and CURRENT_CHECKPOINT.json. GloomBeanUnity/ is the committed source snapshot; GitHistory/history.bundle preserves the full commit history and branch/tag refs.\n\nRestore into a NEW directory:\n\n```text\ngit bundle list-heads GitHistory/history.bundle\ngit clone -b '+branch+' GitHistory/history.bundle RestoredGloomBean\ngit -C RestoredGloomBean fsck --full\n```\n\nDo not clone over a current divergent project. Run Tools/agent_status.py --probe-write from the restored project, rehydrate canonical context, then inspect the next gate. Native builds use the already licensed desktop-user runner on gamer-bro; the cloud sandbox is not Unity. The archive contains no Library/build caches; binaries and human acceptance are separate. Source snapshot and history are not a claim of public GitHub publication.\n'
  put('RESTORE.md',instructions.encode());target.writestr('MANIFEST.json',json.dumps(manifest,indent=2)+'\n')
 with zipfile.ZipFile(path) as z:
  if z.testzip() is not None:raise RuntimeError('Archive CRC failed')
  for item in manifest['files']:
   if hashlib.sha256(z.read(item['path'])).hexdigest()!=item['sha256']:raise RuntimeError('Payload hash mismatch: '+item['path'])
 receipt={'path':str(path),'bytes':path.stat().st_size,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'source_head':head,'source_branch':branch,'payload_files':len(manifest['files']),'bundle_sha256':hashlib.sha256(bundle.read_bytes()).hexdigest(),'publication':'NOT_PERFORMED'}
 (folder/'RECEIPT.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8');return receipt
def main():
 a=argparse.ArgumentParser(description=__doc__);a.add_argument('--project',type=Path,default=Path(__file__).resolve().parent.parent);a.add_argument('--output',type=Path);a.add_argument('--label',required=True);v=a.parse_args()
 try:print(json.dumps(package(v.project,v.output or v.project/'Delivery',v.label),indent=2));return 0
 except (OSError,ValueError,RuntimeError,subprocess.TimeoutExpired) as e:print(json.dumps({'status':'PACKAGE_FAILED','error':str(e)}));return 1
if __name__=='__main__':sys.exit(main())
