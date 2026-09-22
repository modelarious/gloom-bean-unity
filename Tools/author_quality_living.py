"""Q10 original pixel animation banks. Offline, deterministic; no screenshot input."""
from pathlib import Path
import sys,math,json,hashlib,uuid
ROOT=Path(__file__).resolve().parents[1]
sys.path[:0]=[str(ROOT/'.visual-tools'),str(ROOT.parent/'GloomBeanUnity/.visual-tools')]
from PIL import Image,ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/QualityBar'
manifest=[]
def save(im,n):
 f=OUT/(n+'.png');im.save(f,optimize=True);meta=Path(str(f)+'.meta')
 if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/Q10/'+n).hex+'\n',encoding='utf-8')
 manifest.append({'file':f.name,'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'pixels':list(im.size)})
for k in range(8):
 a=k*math.pi/24
 im=Image.new('RGBA',(48,48));d=ImageDraw.Draw(im)
 for n in range(12):
  t=n*math.pi/6+a
  pts=[(24+math.cos(t+x)*r,24+math.sin(t+x)*r) for x,r in [(0,18),(.05,23),(.30,23),(.35,18)]]
  d.polygon(pts,fill='#161422');d.line(pts[:3],fill='#9e7d6a',width=1)
 d.ellipse((5,5,43,43),fill='#151625');d.ellipse((7,7,41,41),fill='#695066');d.arc((7,7,41,41),185,315,fill='#e7bd80',width=2);d.ellipse((12,12,36,36),fill='#1d2031')
 for n in range(6):
  t=n*math.pi/3+a;end=(24+math.cos(t)*13,24+math.sin(t)*13);d.line((24,24,*end),fill='#a47b5d',width=3);d.line((23,23,end[0]-1,end[1]-1),fill='#dcad6c',width=1)
 d.ellipse((18,18,30,30),fill='#141524');d.ellipse((20,20,28,28),fill='#b8916a');d.line((21,21,26,21),fill='#f6d69e');d.rectangle((23,23,25,25),fill='#353044')
 save(im,f'living_gear_{k}')
 for family,base,shade,hi in [(0,(35,64,82),(51,85,100),(120,174,171)),(1,(32,61,63),(51,86,78),(145,179,135)),(2,(53,60,87),(79,86,117),(170,183,211))]:
  im=Image.new('RGBA',(64,32),(*base,130));d=ImageDraw.Draw(im)
  for row in range(4):
   y=row*8+(k+row)%3;offset=(k*3+row*19)%64
   for start,length in [(offset,18),(offset+34,10)]:
    for dx in range(length):
     x=(start+dx)%64;dy=1 if dx<3 or dx>length-4 else 0;d.point((x,y+dy),fill=(*shade,185))
     if 4<dx<length-4:d.point((x,y+2),fill=(*hi,96))
  save(im,f'living_water_{family}_{k}')
  im=Image.new('RGBA',(64,8));d=ImageDraw.Draw(im)
  for x in range(64):
   y=2+round(math.sin((x+k*3)*math.pi/16));d.line((x,y,x,7),fill=(*base,185));d.point((x,y),fill=(*hi,240));d.point((x,y+1),fill=(*shade,230))
  for x in range(4,64,19):d.line((x,4,x+5,4),fill=(*hi,145))
  save(im,f'living_surface_{family}_{k}')
 # Condensed metallic tread, frame displacement follows signed belt travel.
 im=Image.new('RGBA',(32,8));d=ImageDraw.Draw(im);d.rectangle((0,0,31,7),fill='#232233');d.line((0,0,31,0),fill='#e4c497');d.line((0,7,31,7),fill='#0d1524')
 for x in range(-16,48,8):
  xx=x+k;d.line((xx,1,xx+4,3,xx,5),fill='#b19a89',width=2);d.line((xx+6,1,xx+6,6),fill='#504656')
 save(im,f'living_tread_{k}')
 im=Image.new('RGBA',(32,12));d=ImageDraw.Draw(im)
 width=10+k*2;left=16-width//2;right=16+width//2
 d.arc((left,2,right,9),8,172,fill=(186,224,212,max(90,230-k*18)),width=1);d.arc((left+3,1,right-3,8),195,340,fill=(102,168,176,185),width=1)
 for x,y in [(left+3,3),(right-2,4),(15,2)]:d.point((x,y),fill=(219,238,218,210))
 save(im,f'living_wake_{k}')
# A different-looking fixed casing. Only the inner flywheel animates.
im=Image.new('RGBA',(64,64));d=ImageDraw.Draw(im)
d.polygon([(8,8),(56,8),(61,15),(61,51),(53,59),(11,59),(3,51),(3,16)],fill='#151421')
d.rectangle((7,14,57,52),fill='#5c485d');d.rectangle((10,16,54,48),fill='#252334');d.line((8,15,8,50,55,50),fill='#9f7f72',width=2)
for x,y in [(9,18),(55,18),(9,48),(55,48)]:d.rectangle((x-2,y-2,x+2,y+2),fill='#121322');d.rectangle((x-1,y-1,x+1,y+1),fill='#c59d68');d.point((x-1,y-1),fill='#ffe3a0')
d.rectangle((23,0,41,9),fill='#302438');d.line((24,1,39,1),fill='#c5a070');d.rectangle((23,54,41,63),fill='#302438');d.line((24,61,39,61),fill='#c5a070')
save(im,'living_casing')
(ROOT/'Documentation/QualityBarQ1/Q10_ASSETS.json').write_text(json.dumps({'scope':'Original48x48 articulated bronze wheels; signed belt treads and48water/surface frames,8wakes. Pixel art only, no physics/rules or screen-derived decoration.','assets':manifest},indent=2)+'\n',encoding='utf-8')
print('Q10_LIVING_ASSETS_EXPORTED',len(manifest))
