"""Q5 authored whole-stage depth, local materials and material-specific construction.
Uses only credited Ansimuz components and previously extracted individual concept props.
No capture IDs, player coordinates, screenshot colours or runtime rules are inputs.
"""
from pathlib import Path
import sys,math,random,json,hashlib,io,subprocess
ROOT=Path(__file__).resolve().parents[1];sys.path[:0]=[str(ROOT/'.visual-tools'),str(ROOT.parent/'GloomBeanUnity/.visual-tools')]
from PIL import Image,ImageDraw,ImageEnhance,ImageOps
O=ROOT/'Assets/GloomBean/Resources/QualityBar';B=ROOT/'Assets/GloomBean/Resources/BroadVisual';A=ROOT/'ArtSources/Ansimuz';N=Image.Resampling.NEAREST
BASE='77a46bb7160b25c39713bf7b23dda2fe069e55d7'
def original(path):return Image.open(io.BytesIO(subprocess.check_output(['git','-C',str(ROOT),'show',BASE+':'+path]))).convert('RGBA')
def asset(n):return Image.open(O/(n+'.png')).convert('RGBA')
def tone(im,lo,hi):
 im=im.convert('RGBA');out=[]
 for r,g,b,a in im.getdata():
  v=(r*.25+g*.62+b*.13)/255;v=min(1,max(0,(v-.03)/.76));out.append(tuple(round(lo[k]+(hi[k]-lo[k])*v) for k in range(3))+(a,))
 im.putdata(out);return im
pal=[['231c34','594765','9c7e88','dfbea0'],['20172f','504054','967276','d4ab89'],['142b30','415755','849879','c5c29b'],['161f36','425570','87909e','d4bbad'],['281e36','654353','a87c7e','e0ba9a'],['2c253d','716477','b9a199','f2d5a7']]
def colors(w):return [tuple(bytes.fromhex(c)) for c in pal[w]]
def fit(im,w,h):return im.resize((w,h),N)
def paste(bg,im,x,y,w=None,h=None):
 if w is not None:im=fit(im,w,h)
 bg.alpha_composite(im,(int(x),int(y)))
def beam(d,box,c,vertical=False):
 x,y,xx,yy=box;d.rectangle(box,fill=c[0]);d.rectangle((x+1,y+1,xx-1,yy-1),fill=c[1]);d.line((x+2,y+1,xx-2,y+1),fill=c[2]);d.line((x+1,y+2,x+1,yy-2),fill=c[2]);
 for a in range(y+5,yy-2,17) if vertical else range(x+5,xx-2,17):
  px,py=(x+3,a) if vertical else (a,y+3);d.rectangle((px,py,px+1,py+1),fill=c[3])
def masonry(size,c,seed):
 im=Image.new('RGBA',size,c[0]);d=ImageDraw.Draw(im);rng=random.Random(seed)
 for y in range(0,size[1],12):
  for x in range(-12 if (y//12)%2 else 0,size[0],24):
   v=rng.randrange(3);col=tuple(max(0,min(255,int(n*(.95+v*.04)))) for n in c[1]);d.rectangle((x,y,x+22,y+10),fill=col);d.line((x+2,y+1,x+20,y+1),fill=c[2]);d.line((x+1,y+1,x+1,y+8),fill=c[2]);d.line((x+2,y+9,x+20,y+9),fill=c[0]);d.point((x+4,y+3),fill=c[3])
 return im
church=Image.open(A/'church/backgrounds.png').convert('RGBA');col=Image.open(A/'church/column.png').convert('RGBA');tree=Image.open(A/'bridge/Props/tree.png').convert('RGBA');house=Image.open(A/'town/PNG/environment/props-sliced/house-b.png').convert('RGBA');books=original('Assets/GloomBean/Resources/BroadVisual/scene_11.png')
# Entire structural compositions, three physically different planes, calm value groups.
for i in range(26):
 w=0 if i==0 else ((i-1)//4+1 if i<=20 else i-20);c=colors(w);rng=random.Random(901+i);bg=Image.new('RGBA',(320,200),c[0]);d=ImageDraw.Draw(bg)
 for y in range(200):
  t=y/199;d.line((0,y,319,y),fill=tuple(round(c[0][k]*(1-t)+c[1][k]*t) for k in range(3)))
 if i==0:
  bg=original('Assets/GloomBean/Resources/BroadVisual/scene_0.png');bg=ImageEnhance.Color(bg).enhance(1.10)
 elif i in [5,7,8,13,22,24]:
  if i in [13,24]:
   # Ruined skyline at three depths, real window spacing and deliberately broken silhouette.
   for layer,scale in [(0,.42),(1,.67)]:
    for x,y in [(-26,85),(75,47),(177,98),(252,59)]:
     h=tone(house,c[0],c[2] if layer else c[1]);h=h.resize((int(130*scale),int(240*scale)),N).rotate((-14 if x%2 else 13),resample=N,expand=True);paste(bg,h,x+layer*8,y+layer*35)
   d=ImageDraw.Draw(bg)
   for x in [32,150,286]:d.line((x,0,x-17,199),fill=c[2],width=1)
   for x,y in [(66,28),(187,11),(237,111)]:d.polygon([(x,y),(x+4,y+3),(x+3,y+13),(x+6,y+20),(x+1,y+15),(x-5,y+21),(x-3,y+10)],fill=c[1])
  else:
   # Real tree limbs and chapel/conservatory planes, rather than duplicate wheel props.
   back=tone(house,c[0],c[1]);paste(bg,back,125,59,117,148)
   for x,y,wid,hei in [(-42,-26,136,231),(237,-12,136,240),(80,-100,116,180)]:paste(bg,tone(tree,c[0],c[2]),x,y,wid,hei)
   d=ImageDraw.Draw(bg)
   for x,y in [(33,48),(92,25),(249,68),(276,29),(45,122)]:
    d.line((x,y-19,x,y),fill=c[2]);d.ellipse((x-4,y,x+4,y+11),fill=c[2],outline=c[0]);d.line((x-2,y+2,x,y+1),fill=c[3])
   if i in [7,8]:
    for x in [-20,78,176,274]:
     d.line((x,5,x+44,199),fill=c[1],width=4);d.line((x+1,5,x+45,199),fill=c[2]);
    for y in [40,113,185]:d.line((0,y,319,y),fill=c[1],width=4);d.line((0,y,319,y),fill=c[2])
   if i==7:
    for y in range(150,200,8):d.line((0,y,320,y-2),fill=c[2])
 elif i==19:
  bg=original('Assets/GloomBean/Resources/BroadVisual/scene_19.png');bg=ImageEnhance.Contrast(bg).enhance(1.12)
  d=ImageDraw.Draw(bg)
  for x in [6,306]:
   beam(d,(x,0,x+7,199),c,True)
   for y in range(10,196,20):d.ellipse((x+1,y,x+5,y+9),outline=c[3])
 elif i in [9,12,23]:
  # A cutaway apartment wall: furnished rooms separated by unambiguously distant masonry.
  bg=masonry((320,200),[c[0],c[1],tuple(round(x*.82) for x in c[2]),c[2]],i);d=ImageDraw.Draw(bg)
  for row,y in enumerate([7,109]):
   for n,x in enumerate([9,117,225]):
    d.rectangle((x,y,x+87,y+76),fill=c[0]);d.rectangle((x+2,y+2,x+85,y+70),fill=tuple(round(a*.65) for a in c[1]));d.rectangle((x+5,y+6,x+31,y+38),fill=c[1]);d.arc((x+5,y+1,x+31,y+21),180,360,fill=c[2],width=2)
    d.line((x+17,y+7,x+17,y+36),fill=c[2]);d.line((x+7,y+23,x+29,y+23),fill=c[2])
    # curtains, bed/table/chair silhouettes, picture frames; not interactive fake platforms.
    d.polygon([(x+2,y+2),(x+10,y+4),(x+6,y+43),(x+1,y+40)],fill=c[2]);d.line((x+4,y+8,x+3,y+38),fill=c[1])
    d.rectangle((x+43,y+11,x+61,y+30),outline=c[2],width=2);d.ellipse((x+48,y+15,x+57,y+24),fill=c[1]);
    if (row+n)%2:
     d.rectangle((x+34,y+54,x+82,y+58),fill=c[2]);d.line((x+37,y+59,x+37,y+70),fill=c[1],width=3);d.line((x+77,y+59,x+77,y+70),fill=c[1],width=3);d.rectangle((x+57,y+40,x+63,y+52),fill=c[1])
    else:
     d.rectangle((x+36,y+52,x+82,y+68),fill=c[1]);d.line((x+36,y+52,x+82,y+52),fill=c[2]);d.rectangle((x+69,y+49,x+79,y+52),fill=c[2]);d.line((x+80,y+39,x+80,y+70),fill=c[2],width=2)
    beam(d,(x-3,y+75,x+93,y+84),c)
 elif i in [11]:
  bg=books.resize((320,200),N);bg=ImageEnhance.Brightness(bg).enhance(1.22);bg=ImageEnhance.Color(bg).enhance(.85)
 elif i in [1,2,3,4,6,10,14,15,21]:
  # Broad wall bays with a shared construction grammar but level-specific equipment.
  bg=masonry((320,200),[c[0],tuple(round(x*.73) for x in c[1]),tuple(round(x*.72) for x in c[2]),c[2]],i)
  d=ImageDraw.Draw(bg)
  for x in [7,115,223]:
   d.rectangle((x+7,18,x+91,171),fill=c[0]);d.arc((x+7,-10,x+91,74),180,360,fill=c[1],width=7);d.line((x+7,30,x+7,171),fill=c[1],width=5);d.line((x+91,30,x+91,171),fill=c[1],width=5)
  motif={1:'skin',2:'bell',3:'washer',4:'skin',6:'oven',10:'balcony',14:'spindle',15:'coffin',21:'bell'}[i]
  for n,(x,y) in enumerate([(19,76),(139,30),(250,88)]):
   obj=tone(asset(motif),c[0],c[2]);sz=(58,69) if motif not in ['bell','coffin','spindle'] else (35,67);paste(bg,obj,x,y,*sz)
  d=ImageDraw.Draw(bg)
  for x in [0,103,211,315]:beam(d,(x,0,x+6,199),c,True)
  for y in [14,167,191]:beam(d,(0,y,319,y+5),c)
  if i==3:
   # Overhead laundry rail, hooks and hanging rags all the way across the panoramic cell.
   for x in range(28,308,35):d.line((x,20,x,42+(x%3)*5),fill=c[2]);d.arc((x-3,38+(x%3)*5,x+4,46+(x%3)*5),0,240,fill=c[2]);d.polygon([(x-8,57),(x+9,57),(x+7,87),(x+2,84),(x-2,90),(x-8,84)],fill=c[1]);d.line((x-5,60,x-4,82),fill=c[2])
   for y in [144,149]:d.line((0,y,319,y),fill=c[1],width=2)
 else:
  # Cathedral architecture is made of deep stone bays, not a flat gigantic line-drawn rose.
  bg=masonry((320,200),[c[0],tuple(round(x*.64) for x in c[1]),tuple(round(x*.72) for x in c[2]),c[2]],i)
  arc=church.crop((0,0,160,192));arc=tone(arc,c[0],c[2])
  for x in [-24,183]:paste(bg,arc,x,0,160,192)
  for x in [120,304]:paste(bg,tone(col,c[0],c[3]),x,-2,32,204)
  d=ImageDraw.Draw(bg)
  for y in [3,190]:beam(d,(0,y,319,y+6),c)
  if i in [17,18,20]:
   # Daylight belongs to a coherent recessed plane, leaving interactive metal darker.
   for x in [51,257]:
    d.ellipse((x-18,18,x+18,27),outline=c[2],width=2);d.arc((x-18,18,x+18,27),180,360,fill=c[3])
  if i==16:
   glass=tone(asset('glass'),c[0],c[3]);paste(bg,glass,12,12,73,91);paste(bg,glass,213,52,82,100)
  if i==25:
   for x in [58,109,229,281]:d.line((x,0,x-7,199),fill=c[1],width=2);d.line((x+1,0,x-6,199),fill=c[2])
 # Limit palette without flattening planes; source derived components remain credited.
 bg=bg.convert('RGB').quantize(colors=40,dither=Image.Dither.NONE).convert('RGBA');bg.save(B/f'scene_{i}.png',optimize=True)
# Material faces: local highlights and asymmetric wear replace continuous identical bevel bars.
for w in range(6):
 c=colors(w)
 if w in [0,2]:
  im=Image.new('RGBA',(64,32),c[0]);d=ImageDraw.Draw(im)
  for y in range(0,32,8):
   d.rectangle((0,y,63,y+6),fill=c[1]);d.line((0,y,63,y),fill=c[2]);d.line((1,y+1,62,y+1),fill=c[3] if w==0 else c[2]);
   for x in [3,21,44]:d.line([(x,y+3),(x+7,y+2),(x+13,y+4)],fill=c[0]);d.line((x+1,y+5,x+9,y+5),fill=c[2])
  d.ellipse((26,17,39,23),outline=c[0]);d.arc((28,18,37,22),180,360,fill=c[2])
 else:
  im=masonry((64,32),c,900+w);d=ImageDraw.Draw(im)
  if w==1:
   im=Image.new('RGBA',(64,32),c[0]);d=ImageDraw.Draw(im)
   for x in [0,32]:
    d.rectangle((x+1,1,x+30,30),fill=c[1]);d.line((x+2,1,x+29,1),fill=c[3]);d.line((x+2,2,x+29,2),fill=c[2]);d.line((x+1,3,x+1,29),fill=c[2]);d.rectangle((x+6,7,x+26,26),fill=c[0]);
    for y in range(8,26,4):d.line((x+7,y,x+25,y),fill=c[2]);d.line((x+8,y+1,x+24,y+1),fill=c[1])
    for bx,by in [(3,4),(28,4),(3,27),(28,27)]:d.point((x+bx,by),fill=c[3])
 im.save(O/f'solid_{w}.png',optimize=True)
 # No all-white trim slab: a readable narrow gold/wood contact ridge over a dark bevel.
 edge=Image.new('RGBA',(64,8),c[0]);d=ImageDraw.Draw(edge);d.rectangle((0,1,63,2),fill=c[3]);d.line((0,3,63,3),fill=c[2]);d.rectangle((0,4,63,6),fill=c[1])
 for x in range(0,64,16):d.line((x,1,x,6),fill=c[0]);d.point((x+2,4),fill=c[3])
 edge.save(O/f'edge_{w}.png',optimize=True)
 # Stepped lights have a transparent outer penumbra, not one hard triangular opacity wall.
 light=Image.new('RGBA',(64,96));d=ImageDraw.Draw(light)
 for k in range(6):
  half=30-k*4;y0=k*2;d.polygon([(32,y0),(32-half,95),(32+half,95)],fill=(255,215+min(k,4)*4,159+min(k,4)*4,7+k*4))
 light.save(O/f'light_{w}.png',optimize=True)
# Dark sewn fabric with directional folds and ragged hem, reusable below cloth apparatus.
im=Image.new('RGBA',(64,64));d=ImageDraw.Draw(im);c=colors(1)
pts=[(3,1),(60,1),(57,45),(52,50),(46,48),(44,62),(38,51),(31,56),(27,49),(20,54),(15,48),(6,50)]
d.polygon(pts,fill=c[0]);d.polygon([(5,3),(58,3),(55,45),(50,47),(44,45),(42,55),(38,47),(29,51),(24,45),(18,48),(9,46)],fill=c[1])
for x in [10,21,35,49]:d.line([(x,3),(x-2,26),(x+1,43)],fill=c[2],width=2);d.line([(x+3,5),(x+2,28),(x+4,42)],fill=c[0],width=2)
for y in range(5,46,5):d.line((31,y,36,y+2),fill=c[3]);d.line((33,y,34,y+3),fill=c[0])
im.save(O/'drapery.png',optimize=True)
import uuid
for f in [O/'drapery.png']:
 Path(str(f)+'.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/qualitybar/'+f.stem).hex+'\n')
files=[*B.glob('scene_*.png'),*O.glob('solid_[0-5].png'),*O.glob('edge_*.png'),*O.glob('light_*.png'),O/'drapery.png']
(ROOT/'Documentation/QualityBarQ1/Q5_ASSETS.json').write_text(json.dumps({'scope':'26 wholly framed level compositions, material surfaces and restrained pixel-stepped local light; original/credited individual components, never a concept screenshot as a backplate','source_base':BASE,'assets':[{'file':str(f.relative_to(ROOT)).replace('\\','/'),'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'size':list(Image.open(f).size)} for f in files]},indent=2)+'\n')
print('QUALITY_Q5_AUTHORED',len(files))
