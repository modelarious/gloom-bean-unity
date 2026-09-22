# Native screenshot-to-video derivatives. No frame generation or motion interpolation.
from pathlib import Path
import sys,json,hashlib,subprocess,shutil
ROOT=Path(__file__).resolve().parents[1];sys.path[:0]=[str(ROOT/'.visual-tools'),r'C:\Users\micha\Projects\GloomBeanUnity\.visual-tools']
from PIL import Image,ImageDraw
source=ROOT/'Reports/Delivery-WholeGame-Q11';out=ROOT/'Documentation/QualityBarQ1/Q11Review/Motion';out.mkdir(parents=True,exist_ok=True)
receipt=json.loads((source/'runner.json').read_text(encoding='utf-8-sig'));assert receipt['status']=='PASS_NATIVE_RUNS_CAPTURE_REVIEW_PENDING' and len(receipt['actions'])==8
ffmpeg=shutil.which('ffmpeg');assert ffmpeg
manifest=[]
for action in receipt['actions']:
 name=action['name'];folder=source/name/'MotionAction';shots=json.loads((folder/'native-action.json').read_text())['shots'];assert len(shots)==action['native_frames']
 assert all(s['cameraFollow'] and s['input'] in ['ScriptedInput','WitnessInput'] for s in shots)
 times=[s['time'] for s in shots];assert all(b>a for a,b in zip(times,times[1:]))
 # Complete recorded sequence, not a cherry-picked cut. Display only removes empty letterboxing.
 lines=[];files=[]
 for i,s in enumerate(shots):
  f=folder/s['file'];assert f.exists();files.append({'file':s['file'],'time':s['time'],'sha256':hashlib.sha256(f.read_bytes()).hexdigest()})
  duration=times[i+1]-times[i] if i+1<len(times) else times[-1]-times[-2]
  lines.extend(["file '"+f.as_posix()+"'",f'duration {duration:.6f}'])
 lines.append("file '"+(folder/shots[-1]['file']).as_posix()+"'")
 listing=ROOT/'Reports/Q11-encode'/(name+'.ffconcat');listing.parent.mkdir(parents=True,exist_ok=True);listing.write_text('\n'.join(lines)+'\n',encoding='utf-8')
 im=Image.open(folder/shots[0]['file']);w,h=im.size;scale=min(w//240,h//160);x=(w-240*scale)//2;y=(h-160*scale)//2
 video=out/(name+'.mp4')
 cmd=[ffmpeg,'-hide_banner','-loglevel','error','-y','-f','concat','-safe','0','-i',str(listing),'-vf',f'crop={240*scale}:{160*scale}:{x}:{y},scale=480:320:flags=neighbor','-vsync','vfr','-c:v','libx264','-threads','2','-preset','fast','-crf','17','-pix_fmt','yuv420p','-movflags','+faststart',str(video)]
 run=subprocess.run(cmd,capture_output=True,text=True,timeout=30);assert run.returncode==0,(name,run.stderr)
 # Every nth frame is a chronological storyboard, not a generated motion illustration.
 board=Image.new('RGB',(960,4*182),'#15131e');d=ImageDraw.Draw(board)
 for k in range(16):
  i=round(k*(len(shots)-1)/15);s=shots[i];a=Image.open(folder/s['file']).convert('RGB').crop((x,y,x+240*scale,y+160*scale)).resize((240,160),Image.Resampling.NEAREST)
  xx=(k%4)*240;yy=(k//4)*182;board.paste(a,(xx,yy+22));d.text((xx+4,yy+3),f"{name} {s['time']:.2f}s / {s['form']}",fill='white')
 board.save(out/(name+'-storyboard.png'),optimize=True)
 manifest.append({'name':name,'video_file':video.name,'sha256':hashlib.sha256(video.read_bytes()).hexdigest(),'bytes':video.stat().st_size,'native_frames':len(shots),'source_elapsed_seconds':times[-1]-times[0],'scope':'Complete captured sequence at original recorded time spacing, silent H264 derivative. Native observer samples around0.125s; not a full-rate video or human-controlled run. Original1280x800 PNGs stay in the report and are individually hashed.','frames':files})
 print('VIDEO',name,len(shots),video.stat().st_size,flush=True)
(out/'MANIFEST.json').write_text(json.dumps({'videos':manifest,'frames_are_synthetic':False},indent=2)+'\n',encoding='utf-8');print('Q11_NATIVE_MOTION_ENCODE_PASS',len(manifest),sum(x['native_frames'] for x in manifest))
