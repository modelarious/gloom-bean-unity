"""QualityBar Q1: offline authored object kits, never a screenshot overlay.
Uses only the credited source fragments inside this repository. No network or runtime model.
"""
from pathlib import Path
import sys,math,hashlib,json,uuid
ROOT=Path(__file__).resolve().parents[1]
# Reuse the sibling's already installed offline art dependency without changing it.
sys.path.insert(0,str(ROOT/'.visual-tools'))
sys.path.insert(0,str(ROOT.parent/'GloomBeanUnity/.visual-tools'))
from PIL import Image,ImageDraw,ImageEnhance,ImageOps
N=Image.Resampling.NEAREST
OUT=ROOT/'Assets/GloomBean/Resources/QualityBar';OUT.mkdir(parents=True,exist_ok=True)
SRC=ROOT/'ArtSources/QualityBarQ1';OGA=ROOT/'ArtSources/Ansimuz'
# Ink, deepest plane, shadow, midtone, facing plane, high edge, light, accent.
P=[['211c35','453649','765667','ac8086','d5ab99','edcf9e','fff0ba','ab7197'],
   ['110f20','292033','493448','755164','aa7c72','dbac81','ffdd9d','be6982'],
   ['161c24','29302c','494936','706344','a58b5b','d1b981','f1dea4','7d9c63'],
   ['0e142a','272c45','43455e','696477','938597','c3abac','f0d5b2','687f9a'],
   ['171325','302237','53364c','835565','b58486','dcb2a1','f4d7b4','a26388'],
   ['211729','43304b','725262','a58085','c5a891','e9cb99','fff0c8','b8945f']]
PAL=[[tuple(bytes.fromhex(x))+(255,) for x in a] for a in P]
def save(im,name):
 im.save(OUT/(name+'.png'),optimize=True);m=OUT/(name+'.png.meta')
 if not m.exists():m.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/qualitybar/'+name).hex+'\n',encoding='utf-8')
def mapped(im,pal,amount=1):
 im=im.convert('RGBA');out=Image.new('RGBA',im.size)
 cols=[]
 for r,g,b,a in im.getdata():
  lum=(r*.24+g*.59+b*.17)/255;idx=min(6,max(0,int(lum*8.2)));target=pal[idx]
  cols.append((int(r*(1-amount)+target[0]*amount),int(g*(1-amount)+target[1]*amount),int(b*(1-amount)+target[2]*amount),a))
 out.putdata(cols);return out
def curve(d,pts,fill,width=1):
 a,b,c,e=pts;xy=[]
 for k in range(41):
  t=k/40;u=1-t;xy.append((round(u**3*a[0]+3*u*u*t*b[0]+3*u*t*t*c[0]+t**3*e[0]),round(u**3*a[1]+3*u*u*t*b[1]+3*u*t*t*c[1]+t**3*e[1])))
 d.line(xy,fill=fill,width=width)
def bolt(d,x,y,c):
 d.rectangle((x-2,y-2,x+2,y+2),fill=c[0]);d.rectangle((x-1,y-1,x+1,y+1),fill=c[4]);d.point((x-1,y-1),fill=c[6]);d.point((x+1,y+1),fill=c[2])
def panel(d,box,c):
 x,y,xx,yy=box;d.rectangle(box,fill=c[0]);d.rectangle((x+1,y+1,xx-1,yy-1),fill=c[3]);d.line((x+2,y+1,xx-2,y+1),fill=c[5]);d.line((x+1,y+2,x+1,yy-2),fill=c[4]);d.line((xx-1,y+2,xx-1,yy-1),fill=c[1]);d.line((x+1,yy-1,xx-1,yy-1),fill=c[1]);d.rectangle((x+4,y+4,xx-4,yy-4),fill=c[1])
 for bx,by in [(x+2,y+2),(xx-2,y+2),(x+2,yy-2),(xx-2,yy-2)]:bolt(d,bx,by,c)
def beam(d,a,b,c,w=5):
 d.line((a,b),fill=c[0],width=w+4);d.line((a,b),fill=c[2],width=w+1);d.line(((a[0]-1,a[1]-1),(b[0]-1,b[1]-1)),fill=c[4],width=1)
 for q in [a,b]:bolt(d,*q,c)
def leaf(d,x,y,c,scale=1):
 d.polygon([(x,y),(x-5*scale,y-3*scale),(x-7*scale,y-9*scale),(x,y-5*scale),(x+5*scale,y-9*scale),(x+6*scale,y-4*scale)],fill=c[0]);d.polygon([(x,y-1),(x-4*scale,y-4*scale),(x-5*scale,y-7*scale),(x,y-4*scale),(x+4*scale,y-7*scale),(x+4*scale,y-4*scale)],fill=c[7]);d.line((x,y-5*scale,x,y),fill=c[5])
# Layered end brackets and carved supports, derived from the original reference object only.
source_bracket=Image.open(SRC/'bracket.png').convert('RGBA');source_car=Image.open(SRC/'carriage.png').convert('RGBA');source_gear=Image.open(SRC/'gear.png').convert('RGBA')
for group,c in enumerate(PAL):
 # Recessed fascia. Width is tiled in-engine while caps retain authored edge shapes.
 im=Image.new('RGBA',(64,32));d=ImageDraw.Draw(im)
 if group==2:
  d.polygon([(0,0),(64,0),(61,9),(51,12),(48,21),(43,18),(36,29),(26,24),(22,13),(8,12),(3,6)],fill=c[0]);d.polygon([(0,1),(64,1),(59,7),(49,9),(45,17),(39,13),(34,23),(29,20),(25,9),(7,8)],fill=c[3]);
  for x in range(1,64,8):curve(d,[(x,2),(x+5,7),(x-2,9),(x+2,15)],c[1],2)
  curve(d,[(0,2),(17,4),(44,0),(63,3)],c[5],2);curve(d,[(7,6),(27,11),(38,4),(56,8)],c[4],1)
  for x,y in [(11,10),(40,13),(54,7)]:leaf(d,x,y,c)
 elif group in (3,4,5):
  d.rectangle((0,0,63,9),fill=c[0]);d.rectangle((0,1,63,3),fill=c[5]);d.rectangle((0,4,63,7),fill=c[3]);d.line((0,8,63,8),fill=c[1])
  for x in range(0,64,16):
   panel(d,(x,9,x+15,28),c)
   if group==3:d.line((x+5,14,x+5,23),fill=c[4]);d.line((x+9,14,x+9,23),fill=c[4])
   else:
    d.polygon([(x+8,12),(x+12,18),(x+8,24),(x+4,18)],fill=c[4]);d.line((x+8,14,x+8,22),fill=c[6]);d.line((x+6,18,x+10,18),fill=c[6])
  d.line((0,30,63,30),fill=c[1])
 else:
  im.alpha_composite(mapped(source_car,c,.75).resize((64,35),N),(0,-3));d=ImageDraw.Draw(im);d.line((0,1,63,1),fill=c[6]);d.line((0,2,63,2),fill=c[4]);
  for x in [7,28,48]:d.line((x,20,x+6,20),fill=c[4]);d.point((x+2,19),fill=c[5])
 save(im,f'fascia_{group}')
 bracket=mapped(source_bracket,c,.8);save(bracket,f'bracket_{group}')
 # Long vertical columns with construction seams: use an actual stone carving fragment.
 stone=Image.open(OGA/'church/tileset.png').convert('RGBA').crop((192,16,224,81));stone=mapped(stone,c,.87)
 col=Image.new('RGBA',(32,96));col.alpha_composite(stone,(0,0));col.alpha_composite(stone,(0,46));d=ImageDraw.Draw(col)
 if group in (0,1):
  col=Image.new('RGBA',(20,64));d=ImageDraw.Draw(col);d.rectangle((3,0,17,63),fill=c[0]);d.rectangle((5,0,14,63),fill=c[2]);d.line((5,0,5,63),fill=c[4]);d.line((13,0,13,63),fill=c[1]);
  for y in [7,41]:panel(d,(1,y,19,y+9),c)
 elif group==2:
  col=Image.new('RGBA',(32,96));d=ImageDraw.Draw(col);curve(d,[(14,-5),(7,19),(26,55),(13,100)],c[0],17);curve(d,[(13,-5),(6,19),(25,55),(12,100)],c[3],13);curve(d,[(9,-5),(4,19),(20,55),(8,100)],c[5],2)
  for yy in [17,45,69]:curve(d,[(14,yy),(6,yy-6),(4,yy-14),(1,yy-18)],c[2],5);leaf(d,7,yy-14,c)
 save(col,f'column_{group}')
 # Hollow structural truss underneath long traversable edges.
 truss=Image.new('RGBA',(96,48));d=ImageDraw.Draw(truss)
 if group==2:
  for x in (12,76):curve(d,[(x,0),(x+4,14),(47,5),(49,44)],c[0],9);curve(d,[(x,0),(x+4,13),(47,5),(49,42)],c[3],5);curve(d,[(x-1,0),(x+1,12),(45,5),(47,38)],c[5],1)
 else:
  beam(d,(3,2),(92,2),c);beam(d,(3,4),(34,36),c);beam(d,(34,36),(89,5),c);beam(d,(34,36),(34,46),c);beam(d,(45,2),(66,18),c,3)
 save(truss,f'truss_{group}')
 # Repeating chain with pixel-metal highlights. Transparent areas remain truly open.
 chain=Image.new('RGBA',(8,32));d=ImageDraw.Draw(chain)
 for y in range(-5,32,10):
  d.ellipse((1,y,6,y+10),outline=c[0],width=3);d.arc((2,y+1,5,y+9),75,265,fill=c[5],width=1);d.line((3,y+2,3,y+6),fill=c[3])
 save(chain,f'chain_{group}')
 # Static/noncolliding rear pipes use consistent value spacing.
 pipe=Image.new('RGBA',(16,48));d=ImageDraw.Draw(pipe);d.rectangle((4,0,12,47),fill=c[0]);d.rectangle((6,0,11,47),fill=c[2]);d.line((6,0,6,47),fill=c[4])
 for y in (2,32):panel(d,(1,y,15,y+8),c)
 save(pipe,f'pipe_{group}')
 save(mapped(source_gear,c,.65),f'gear_{group}')
 # Hard-stepped transparent lantern light, rendered behind interactive art.
 cone=Image.new('RGBA',(64,96));d=ImageDraw.Draw(cone)
 for step in range(5):
  margin=step*5;alpha=13+step*3;d.polygon([(29,2),(35,2),(63-margin,95),(margin,95)],fill=(255,203,123,alpha))
 save(cone,f'light_{group}')
# Independent object silhouettes, at actual GBA-scale clusters.
for kind in ['washer','bell','skin','oven','pump','balcony','ledger','coffin','halo','book','statue']:
 group={'washer':1,'bell':1,'skin':1,'oven':2,'pump':2,'balcony':3,'ledger':3,'coffin':4,'halo':5,'book':5,'statue':5}[kind];c=PAL[group]
 im=Image.new('RGBA',(96,112));d=ImageDraw.Draw(im)
 if kind in ('washer','oven'):
  panel(d,(7,28,86,106),c);d.rectangle((14,37,75,102),fill=c[2]);d.ellipse((19,33,86,102),fill=c[0]);d.ellipse((23,37,82,98),fill=c[4]);d.ellipse((26,41,79,95),fill=c[1]);d.ellipse((29,44,76,92),fill=c[0]);d.arc((24,38,81,97),170,280,fill=c[6],width=2)
  for a in range(0,360,45):x=53+int(math.cos(math.radians(a))*29);y=68+int(math.sin(math.radians(a))*29);bolt(d,x,y,c)
  for y in (48,70,89):panel(d,(2,y,20,y+10),c)
  d.rectangle((27,23,72,29),fill=c[0]);d.line((30,24,70,24),fill=c[5]);d.rectangle((30,16,64,23),fill=c[2]);d.line((30,16,63,16),fill=c[4]);d.rectangle((44,0,53,17),fill=c[1]);d.line((44,0,44,16),fill=c[4])
  for x,y in [(38,74),(52,63),(65,76),(50,82),(38,86)]:
   if kind=='washer':d.ellipse((x-8,y-7,x+7,y+6),fill=c[4]);d.line((x-4,y-3,x+3,y+3),fill=c[2]);d.point((x-3,y-4),fill=c[6])
   else:d.polygon([(x-5,y+8),(x-6,y),(x,y-14),(x+2,y-4),(x+7,y-9),(x+6,y+7)],fill=(219,109,87,255));d.polygon([(x-2,y+6),(x,y-8),(x+3,y+6)],fill=c[6])
  for x in [6,74]:panel(d,(x,102,x+16,111),c)
 elif kind=='bell':
  d.rectangle((44,0,50,18),fill=c[4]);panel(d,(34,11,61,23),c);d.polygon([(39,21),(57,21),(63,36),(65,65),(84,89),(84,96),(11,96),(11,89),(29,64),(32,36)],fill=c[0]);d.polygon([(39,26),(54,25),(57,38),(60,68),(76,88),(19,88),(33,66),(37,37)],fill=c[3]);curve(d,[(40,28),(37,45),(42,67),(28,87)],c[5],4);curve(d,[(54,28),(57,55),(56,64),(67,87)],c[2],5)
  d.ellipse((11,87,84,106),fill=c[0]);d.arc((14,87,82,103),0,180,fill=c[5],width=3);d.ellipse((37,92,57,111),fill=c[1]);d.ellipse((41,94,53,106),fill=c[4]);d.line((22,72,67,72),fill=c[4]);d.line((24,75,65,75),fill=c[1]);d.rectangle((45,49,51,62),fill=c[0]);d.rectangle((39,53,57,57),fill=c[0])
 elif kind=='skin':
  beam(d,(8,8),(86,8),c,4)
  for x in (18,70):d.line((x,7,x,24),fill=c[5],width=2)
  pts=[(18,21),(36,14),(59,21),(73,20),(87,32),(72,41),(79,77),(68,94),(56,100),(48,110),(32,101),(20,109),(15,81),(22,42),(5,31)]
  d.polygon(pts,fill=c[0]);d.polygon([(x+1,y-2) for x,y in pts[1:-1]],fill=c[3]);curve(d,[(35,23),(43,47),(28,78),(45,103)],c[1],2);curve(d,[(57,26),(47,52),(68,78),(60,96)],c[4],3)
  for y in range(29,99,8):d.line((36,y,44,y+2),fill=c[6]);d.line((42,y-2,42,y+3),fill=c[0])
  d.ellipse((39,37,48,50),fill=c[0]);d.ellipse((57,39,66,51),fill=c[0]);curve(d,[(37,61),(44,72),(60,72),(66,61)],c[1],2)
 elif kind=='pump':
  panel(d,(18,87,78,108),c);d.rectangle((33,31,64,91),fill=c[0]);d.rectangle((37,36,61,88),fill=c[3]);d.line((40,38,40,86),fill=c[5],width=2);panel(d,(29,24,68,40),c);d.arc((30,9,92,62),180,285,fill=c[0],width=12);d.arc((31,10,91,61),180,285,fill=c[5],width=3);beam(d,(49,25),(11,7),c,4);d.rectangle((6,4,25,12),fill=c[1]);d.line((8,4,23,4),fill=c[5]);d.ellipse((70,49,93,62),fill=c[0]);d.ellipse((74,52,89,60),fill=c[4]);d.rectangle((77,57,88,67),fill=c[0])
 elif kind=='balcony':
  bg=Image.open(OGA/'church/tileset.png').convert('RGBA');win=bg.crop((283,15,324,80));im.alpha_composite(mapped(win,PAL[3],.5).resize((54,80),N),(23,6));d=ImageDraw.Draw(im);d.rectangle((13,4,21,88),fill=c[0]);d.rectangle((79,4,87,88),fill=c[0]);d.line((15,8,15,85),fill=c[5]);d.line((81,8,81,85),fill=c[4]);panel(d,(5,81,92,105),c)
  for x in range(12,90,9):beam(d,(x,86),(x,100),c,2)
 elif kind=='ledger':
  panel(d,(3,7,92,108),c)
  for row in range(3):
   y=18+row*28;d.rectangle((10,y,85,y+24),fill=c[0]);d.line((10,y+25,85,y+25),fill=c[5],width=2)
   for k in range(10):
    x=12+k*7;h=13+(k*3+row*5)%9;color=[c[3],c[4],c[7]][(k+row)%3];d.rectangle((x,y+23-h,x+4,y+22),fill=color);d.line((x+1,y+23-h,x+1,y+21),fill=c[5]);d.point((x+3,y+19),fill=c[1])
 elif kind=='coffin':
  pts=[(30,4),(65,4),(86,26),(78,93),(61,108),(30,107),(12,87),(7,24)];d.polygon(pts,fill=c[0]);d.polygon([(32,9),(63,9),(79,27),(72,88),(59,101),(32,100),(19,84),(14,26)],fill=c[3]);d.line([(32,10),(16,27),(21,81),(33,97)],fill=c[5],width=2);d.line([(63,11),(75,28),(69,86),(58,97)],fill=c[1],width=3);d.rectangle((42,26,51,77),fill=c[0]);d.rectangle((30,40,64,49),fill=c[0]);d.line((44,29,44,73),fill=c[5]);d.line((32,42,60,42),fill=c[5]);
  for x,y in [(19,32),(76,36),(73,72),(22,70)]:bolt(d,x,y,c)
 elif kind=='halo':
  d.ellipse((8,22,88,100),fill=c[0]);d.ellipse((12,26,84,96),fill=c[4]);d.ellipse((17,31,79,91),fill=c[0]);d.ellipse((20,34,76,88),fill=(0,0,0,0));d.arc((11,25,86,98),175,292,fill=c[6],width=2);d.arc((17,31,79,91),0,90,fill=c[2],width=2)
  for a in range(0,360,45):
   x=48+int(math.cos(math.radians(a))*38);y=61+int(math.sin(math.radians(a))*38);d.polygon([(x-3,y-3),(x+3,y-3),(x+4,y+3),(x,y+7),(x-4,y+3)],fill=c[1]);bolt(d,x,y,c)
  beam(d,(48,28),(48,94),c,3);beam(d,(16,61),(80,61),c,3);d.ellipse((33,46,63,76),fill=c[0]);d.ellipse((37,50,59,72),fill=c[4]);d.ellipse((42,51,54,71),fill=c[0]);panel(d,(40,1,56,18),c);d.rectangle((46,18,50,25),fill=c[5])
 elif kind=='book':
  d.polygon([(3,12),(46,19),(91,10),(91,94),(50,105),(3,96)],fill=c[0]);d.polygon([(7,15),(46,24),(47,100),(7,91)],fill=c[4]);d.polygon([(51,24),(87,15),(87,91),(51,100)],fill=c[5]);d.line((47,24,47,100),fill=c[1],width=3)
  for y in range(31,87,7):
   for x in [12,58]:
    for k in range(4):d.line((x+k*6,y,x+3+k*6,y+1),fill=c[2])
  for x,y in [(15,23),(77,25),(15,83),(77,83)]:d.polygon([(x,y-3),(x+4,y),(x,y+4),(x-4,y)],fill=c[7])
 elif kind=='statue':
  d.ellipse((28,3,68,25),outline=c[5],width=3);d.polygon([(46,14),(59,24),(59,39),(77,91),(75,104),(17,104),(22,88),(34,41),(34,27)],fill=c[0]);d.polygon([(45,19),(54,28),(53,44),(69,96),(26,96),(38,44),(38,29)],fill=c[3]);d.ellipse((37,22,56,42),fill=c[1]);d.line((40,23,37,35),fill=c[5]);d.line((44,49,32,97),fill=c[5],width=2);d.line((53,53,59,97),fill=c[1],width=3);curve(d,[(30,66),(35,57),(41,55),(49,60)],c[5],5);curve(d,[(65,68),(62,57),(55,55),(49,60)],c[4],5);panel(d,(12,99,82,111),c)
 save(im,kind)
# The lamp source has an authored metal shade; flame/light are separate from the solid silhouette.
for group in range(6):save(mapped(Image.open(SRC/'lamp.png'),PAL[group],.55),f'lamp_{group}')
# Foreground decorative bars, never collider-bearing or placed over the active route centre.
for group,c in enumerate(PAL):
 fence=Image.new('RGBA',(96,32));d=ImageDraw.Draw(fence)
 for x in range(7,96,12):
  d.rectangle((x,8,x+3,31),fill=c[0]);d.line((x,12,x,31),fill=c[2]);d.polygon([(x-3,9),(x+2,2),(x+6,9)],fill=c[0])
 d.rectangle((0,16,95,18),fill=c[0]);d.line((0,16,95,16),fill=c[2]);save(fence,f'fence_{group}')
assets=[]
for f in sorted(OUT.glob('*.png')):assets.append({'name':f.stem,'size':list(Image.open(f).size),'sha256':hashlib.sha256(f.read_bytes()).hexdigest()})
(ROOT/'Documentation/QualityBarQ1/ART_MANIFEST.json').write_text(json.dumps({'scope':'Separate reusable environment objects; all are true runtime assets, not complete concept screenshots','assets':assets},indent=2)+'\n',encoding='utf-8')
print('QUALITYBAR_OBJECT_KITS_AUTHORED',len(assets))
