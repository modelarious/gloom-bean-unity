from pathlib import Path
import argparse,json,hashlib,zipfile,subprocess
p=Path(__file__).resolve().parents[1]
ap=argparse.ArgumentParser();ap.add_argument('pass_id');args=ap.parse_args();assert args.pass_id in ['P8']
root=p/'Delivery'/('V6-GBA-'+args.pass_id);root.mkdir(exist_ok=True)
out=root/'GloomBean_V6_GBA_Windows_Preview.zip';assert not out.exists(),'Frozen package already exists'
base=p/'Builds/Windows';entries={}
for f in sorted(base.rglob('*')):
 if not f.is_file() or f.suffix.lower() in ['.pdb','.mdb','.log','.ttf','.otf'] or any('donotship' in x.lower() or 'dontship' in x.lower() for x in f.parts):continue
 entries['GloomBeanWindows/'+f.relative_to(base).as_posix()]=f.read_bytes()
assembly=hashlib.sha256((base/'GloomBean_Data/Managed/Assembly-CSharp.dll').read_bytes()).hexdigest()
with zipfile.ZipFile(p/'Delivery/V6-GBA-P7/GloomBean_V6_GBA_Windows_Preview.zip') as z:
 for name in ['START_HERE.txt','PLAY_GUIDE.md','ENVIRONMENT_CREDITS.md']:
  entries[name]=z.read(name).replace(b'(P7)',b'(P8)').replace(b'newer P7 tests',b'newer P7/P8 tests')
head=subprocess.check_output(['git','-C',str(p),'rev-parse','HEAD'],text=True).strip()
entries['BUILD_IDENTITY.json']=json.dumps({'kind':'native Windows GBA-style visual preview','source_head':head,'managed_assembly_sha256':assembly,'full_regression_candidate':'cc40096f1afa50b1d102c0cc7b75308bfd63d819','last_focused_candidate':'d221ea49253f885caf381e853e72bf1fb9c8f758','later_change':'Only displayed ending text and explanation rectangle; no rule, art, physics or save change','strict_visual_superiority':'UNMET','hardware_scope':'Unity Windows player, not GBA hardware port'},indent=2).encode('utf-8')
manifest={'schema':1,'kind':'player','files':{n:{'bytes':len(b),'sha256':hashlib.sha256(b).hexdigest()} for n,b in sorted(entries.items())}};entries['PAYLOAD_SHA256.json']=json.dumps(manifest,indent=2).encode('utf-8')
with zipfile.ZipFile(out,'w',zipfile.ZIP_DEFLATED,6) as z:
 for name,data in sorted(entries.items()):z.writestr(name,data)
extracted=p.parent/('GloomBean V6 GBA Extracted '+args.pass_id);assert not extracted.exists()
with zipfile.ZipFile(out) as z:
 assert z.testzip() is None
 for name,row in manifest['files'].items():assert hashlib.sha256(z.read(name)).hexdigest()==row['sha256']
 z.extractall(extracted)
receipt={'archive':str(out),'bytes':out.stat().st_size,'sha256':hashlib.sha256(out.read_bytes()).hexdigest(),'payload_files':len(manifest['files']),'assembly_sha256':assembly,'extracted':str(extracted),'source_head':head,'status':'HASH_VERIFIED_PENDING_EXACT_ENDINGS'}
(root/'PACKAGE_IDENTITY.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8');print(json.dumps(receipt))
