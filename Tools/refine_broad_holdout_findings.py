from pathlib import Path
import sys,subprocess,io,math,uuid,re
ROOT=Path(__file__).resolve().parents[1];sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image,ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/BroadVisual';BASE='85a711051387b61a728f991dadc9d951fa809775'
# The False Empyrean must remain luminous; actual dark cast silhouettes need a pale field.
for i in [17,18,20]:
 for prefix in ['scene_','bay_']:
  im=Image.open(io.BytesIO(subprocess.check_output(['git','-C',str(ROOT),'show',BASE+':Assets/GloomBean/Resources/BroadVisual/'+prefix+str(i)+'.png']))).convert('RGBA');dst=[]
  for rr,gg,bb,a in im.getdata():
   lum=(rr*.3+gg*.58+bb*.12)/255
   # High-key recessed stone, with limited dark seams; not a full-screen black overlay.
   t=min(1,lum*1.8);low=(151,145,165) if prefix=='scene_' else (130,123,146);high=(252,238,202)
   c=tuple(round(low[k]*(1-t)+high[k]*t) for k in range(3));dst.append((*c,a))
  im.putdata(dst);im=im.quantize(colors=24).convert('RGBA');im.save(OUT/f'{prefix}{i}.png',optimize=True)
# The Surveyor's true weak point reads as a measuring eye, not a rectangular terrain slab.
for active in [0,1]:
 im=Image.new('RGBA',(32,48));d=ImageDraw.Draw(im);ink='#15182c';gold='#ecd096' if active else '#716780';light='#fff0c3' if active else '#a194aa'
 d.polygon([(16,0),(29,13),(26,37),(16,47),(6,37),(3,13)],fill=ink);d.polygon([(16,3),(26,14),(23,35),(16,42),(9,35),(6,14)],fill='#494660');d.line([(16,4),(24,14),(21,35)],fill=gold,width=2)
 d.ellipse((8,12,24,32),fill=ink);d.ellipse((10,14,22,30),fill=gold);d.ellipse((14,16,18,28),fill=ink)
 if not active:d.line((10,22,22,22),fill=light,width=3)
 for y in [6,36]:d.rectangle((13,y,19,y+2),fill=light)
 f=OUT/f'core_{active}.png';im.save(f);meta=(ROOT/'Assets/GloomBean/Resources/VisualV6/background_7.png.meta').read_text();meta=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/broad/core_'+str(active)).hex,meta);Path(str(f)+'.meta').write_text(meta)
print('BROAD_SHADOW_CONTRAST_AND_CORE_ART')
