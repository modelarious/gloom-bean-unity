"""Q9D: outdoor construction is vegetation, not a repeated indoor fruit rack.
Uses the existing credited CC0-derived season_tree export for leaf clusters. The
new boughs, hanging growth and palette composition are original adaptations.
No camera coordinates or gameplay data are consumed.
"""
from pathlib import Path
import sys,math,json,hashlib,uuid
ROOT=Path(__file__).resolve().parents[1]
for path in [ROOT/'.visual-tools',ROOT.parent/'GloomBeanUnity/.visual-tools']:sys.path.insert(0,str(path))
from PIL import Image,ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/QualityBar';N=Image.Resampling.NEAREST
INK='#181d29';DARK='#34302c';WOOD='#51453a';MID='#816f53';LIGHT='#b3a074';LEAF='#63765c';MOSS='#99a073'
source=Image.open(OUT/'season_tree.png').convert('RGBA')
def branch(d,points,w):
 d.line(points,fill=INK,width=w+3,joint='curve');d.line(points,fill=WOOD,width=w,joint='curve');d.line([(x-1,y-1) for x,y in points],fill=MID,width=max(1,w//3),joint='curve');d.line([(x-2,y-2) for x,y in points],fill=LIGHT,width=1)
def foliage(image,box,xy,size,autumn=False):
 leaf=source.crop(box).resize(size,N)
 if autumn:
  pixels=[]
  for r,g,b,a in leaf.getdata():
   if g>r*.9 and g>b*1.2 and a:pixels.append((min(210,int(r*1.55+22)),min(145,int(g*.98+4)),min(100,int(b*.8+7)),a))
   else:pixels.append((r,g,b,a))
  leaf.putdata(pixels)
 image.alpha_composite(leaf,xy)
def pear(d,x,y,size,green=True):
 w=size//2;d.line((x,y-5,x+2,y-8),fill=MID,width=2)
 d.polygon([(x,y-4),(x+3,y-2),(x+w,y+4),(x+w-1,y+size//2),(x,y+size//2+2),(x-w+1,y+size//2),(x-w,y+4),(x-3,y-2)],fill=INK)
 d.ellipse((x-w+2,y,x+w-2,y+size//2),fill='#8b9360' if green else '#bd8551');d.ellipse((x-w+2,y+1,x,y+size//2-2),fill='#b4b474' if green else '#d5ae72')
 d.line((x-2,y+3,x-2,y+5),fill=INK);d.line((x+2,y+3,x+2,y+5),fill=INK);d.point((x,y+8),fill=INK)
for kind in [4,12,13]:
 image=Image.new('RGBA',(128,112));d=ImageDraw.Draw(image)
 if kind in (4,13):
  # Open crown framing; negative space is intentional so the walkable edge is clear.
  branch(d,[(19,112),(28,89),(21,64),(26,43),(19,23),(31,3)],10)
  branch(d,[(25,60),(44,49),(59,24),(85,17),(111,27)],6)
  branch(d,[(24,83),(48,86),(67,72),(102,64),(128,74)],5)
  branch(d,[(23,45),(10,36),(-4,17)],5)
  branch(d,[(82,18),(93,6),(118,-1)],4)
  for pts in [[(22,99),(8,109),(-1,110)],[(25,99),(39,106),(48,110)],[(19,81),(7,80),(0,86)]]:branch(d,pts,3)
  foliage(image,(0,0,61,54),(-14,-10),(70,56),kind==13)
  foliage(image,(37,0,104,56),(65,-8),(76,56),kind==13)
  foliage(image,(54,23,104,79),(80,47),(56,48),kind==13)
  d=ImageDraw.Draw(image)
  for x,y,root in [(53,46,(59,24)),(88,40,(88,19)),(108,44,(110,27)),(59,88,(63,75))]:
   d.line((root[0],root[1],x,y-7),fill=MID);pear(d,x,y,13,kind!=13)
  for x,y in [(20,65),(32,94),(12,38)]:
   d.line((x,y,x+4,y-4),fill=LEAF,width=2);d.polygon([(x+2,y-2),(x+7,y-4),(x+4,y+1)],fill=MOSS)
 else:
  # Reed/fern canopy and hanging root hairs, with water sheen at the base rather than cabinetry.
  for x,lean,height in [(10,14,99),(39,-8,75),(89,10,100),(119,-11,73)]:
   branch(d,[(x,112),(x+lean//2,74),(x+lean,112-height)],3)
   for k in range(5):
    y=99-k*13;xx=x+int(lean*(112-y)/max(1,height));direction=-1 if k%2 else 1
    d.polygon([(xx,y),(xx+direction*16,y-12),(xx+direction*10,y-1),(xx,y+3)],fill=INK)
    d.polygon([(xx,y),(xx+direction*14,y-10),(xx+direction*7,y)],fill=LEAF)
    d.line((xx,y,xx+direction*12,y-8),fill=MOSS)
  foliage(image,(1,15,64,69),(-6,-9),(69,48));foliage(image,(41,0,104,61),(72,-10),(65,60))
  d=ImageDraw.Draw(image)
  for x in [18,37,61,86,108]:
   y=16+(x*7)%14;d.line([(x,y),(x+3,y+15),(x-1,y+29)],fill=WOOD,width=2)
   d.line([(x+1,y+22),(x-4,y+30),(x-3,y+35)],fill=MID)
   d.ellipse((x-2,y+33,x+1,y+36),fill='#92b5a5')
  for x,y,w in [(2,106,18),(30,111,21),(66,106,20),(106,110,18)]:
   d.line((x,y,x+w,y),fill='#638990');d.line((x+3,y-1,x+w-5,y-1),fill='#a4c3ad')
 f=OUT/f'construction_room_{kind}.png';image.save(f,optimize=True)
 meta=Path(str(f)+'.meta')
 if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/Q9-outdoor/'+str(kind)).hex+'\n',encoding='utf-8')
rows=[]
for kind in [4,12,13]:
 f=OUT/f'construction_room_{kind}.png';rows.append({'file':f.name,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()})
(ROOT/'Documentation/QualityBarQ1/Q9D_OUTDOOR_ASSETS.json').write_text(json.dumps({'provenance':'Existing season_tree leaf fragments retain ArtSources/Ansimuz CC0 provenance. New branches, pears, roots and composition are an adaptation, not wholly original source leaves.','rejected':'Q9C opaque indoor fruit-rack backdrop on outdoor stages5,7,8','assets':rows},indent=2)+'\n',encoding='utf-8')
print('OUTDOOR_CONSTRUCTION_EXPORT_PASS',len(rows))
