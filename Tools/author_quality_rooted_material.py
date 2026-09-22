"""Original Q10 rooted earth and kiln masonry, chosen by level/material/shape, never camera."""
from pathlib import Path
import sys,json,hashlib,math,uuid
ROOT=Path(__file__).resolve().parents[1];sys.path[:0]=[str(ROOT/'.visual-tools'),str(ROOT.parent/'GloomBeanUnity/.visual-tools')]
from PIL import Image,ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/QualityBar';manifest=[]
palette=['#171e27','#29312e','#454b39','#666047','#8a7753','#b59a6b','#d4ba83','#567956']
def write(im,n):
 f=OUT/(n+'.png');im.save(f,optimize=True);meta=Path(str(f)+'.meta')
 if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/Q10/'+n).hex+'\n',encoding='utf-8')
 manifest.append({'name':n,'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'size':list(im.size)})
for vertical in [False,True]:
 im=Image.new('RGBA',(64,64),palette[1]);d=ImageDraw.Draw(im)
 # Offset angular soil masses with hand-selected light planes, not a noise overlay.
 for row in range(4):
  for col in range(4):
   x=col*16+(8 if row%2 else 0)-8;y=row*16
   pts=[(x,y+5),(x+5,y+1),(x+13,y+3),(x+17,y+9),(x+12,y+15),(x+3,y+14),(x-1,y+9)]
   for shift in [-64,0,64]:
    d.polygon([(xx+shift,yy) for xx,yy in pts],fill=palette[2]);d.line([(x+shift+1,y+5),(x+shift+5,y+3),(x+shift+12,y+5)],fill=palette[3],width=1);d.line([(x+shift+13,y+10),(x+shift+10,y+13),(x+shift+4,y+13)],fill=palette[0],width=1)
 # Root segments meet tile boundaries; directional grain makes tall soil shafts legible.
 roots=[(10,6,0),(42,4,1)]
 for center,width,phase in roots:
  path=[]
  for y in range(-6,71):
   x=center+round(5*math.sin((y+phase*19)*math.pi/32))
   path.append((x,y) if vertical else (y,x))
  for shift in [-64,0,64]:
   pts=[(x+shift,y) if vertical else (x,y+shift) for x,y in path]
   d.line(pts,fill=palette[0],width=width+5);d.line(pts,fill=palette[4],width=width+2);d.line([(x-1,y-1) for x,y in pts],fill=palette[5],width=max(1,width//2));d.line([(x+2,y+1) for x,y in pts],fill=palette[3],width=2)
  # A split, carved hollow and a smaller crossing root; single-pixel glints stay sparse.
  x,y=(center,31) if vertical else (31,center)
  d.ellipse((x-4,y-6,x+4,y+6),fill=palette[0]);d.arc((x-4,y-6,x+4,y+6),170,350,fill=palette[5],width=1);d.ellipse((x-1,y-3,x+2,y+3),fill=palette[2])
 for x,y in [(4,28),(24,50),(49,13),(61,47)]:d.line((x,y,x+2,y-1,x+4,y),fill=palette[7]);d.point((x+2,y-2),fill='#8d9962')
 write(im,'rooted_vertical' if vertical else 'rooted_earth')
im=Image.new('RGBA',(32,32),'#201b2b');d=ImageDraw.Draw(im)
for y in range(0,32,8):
 for x in range(-8 if y%16 else 0,32,16):
  d.rectangle((x,y,x+14,y+6),fill='#5c4447');d.line((x+1,y+1,x+12,y+1),fill='#997367');d.line((x+2,y+2,x+10,y+2),fill='#77535a');d.line((x+1,y+6,x+14,y+6),fill='#342635');d.point((x+2,y+1),fill='#cba17a')
write(im,'kiln_masonry')
(ROOT/'Documentation/QualityBarQ1/Q10_MATERIAL_ASSETS.json').write_text(json.dumps({'scope':'Rooted ground/vertical growth and furnace stone. Actual food/cloth/semantic-tint material identities remain separate.','assets':manifest},indent=2)+'\n',encoding='utf-8')
print('Q10_MATERIAL_EXPORT_PASS',len(manifest))
