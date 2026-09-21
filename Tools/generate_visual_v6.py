"""Original V6 art source. Deterministic, offline, no game simulation dependencies.
Pillow 12.0.0 is an authoring-only dependency. Coordinates are deliberately pixel snapped.
Nintendo images are never read by this generator.
"""
from pathlib import Path
import sys,math,random,json,hashlib,uuid
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image,ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/VisualV6'
OUT.mkdir(parents=True,exist_ok=True)
MANIFEST=[]
def rgb(s):return tuple(bytes.fromhex(s.strip('#')))
def mix(a,b,t):return tuple(round(a[i]+(b[i]-a[i])*t) for i in range(3))
def box(d,xy,c):d.rectangle(tuple(round(v) for v in xy),fill=c)
def line(d,p,c,w=1):d.line([(round(x),round(y)) for x,y in p],fill=c,width=max(1,round(w)),joint='curve')
def poly(d,p,c):d.polygon([(round(x),round(y)) for x,y in p],fill=c)
def ell(d,xy,c,outline=None,width=1):d.ellipse(tuple(round(v) for v in xy),fill=c,outline=outline,width=width)
def bez(d,p,c,w=1):
 pts=[]
 for i in range(41):
  t=i/40;pts.append(tuple((1-t)**3*p[0][j]+3*(1-t)**2*t*p[1][j]+3*(1-t)*t*t*p[2][j]+t**3*p[3][j] for j in (0,1)))
 line(d,pts,c,w)
def save(im,name):
 path=OUT/(name+'.png');im.save(path,optimize=True)
 meta='fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/v6/'+name).hex+'\n'
 Path(str(path)+'.meta').write_text(meta,encoding='utf-8')
 MANIFEST.append({'file':path.name,'size':list(im.size),'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
# Four value planes per world. Hero gold, hot pink and white are reserved for the foreground.
PALETTES=[
 ('dce8c5','a5c9bf','718f87','476a6d','52634a','f3d995'),
 ('141c30','273044','454456','695966','261e32','ba875d'),
 ('10292c','204343','375854','597568','1c3033','ba9c59'),
 ('122737','254554','3a5c68','5b7d83','182c3b','b7af82'),
 ('27172c','442a3d','614054','936268','241c30','c49169'),
 ('dddcca','a8bdc0','728f9e','d6c4a0','425b73','b09155')]

def arch(d,x,y,w,h,fill,edge=None,width=1):
 pts=[(x,y+h),(x,y+w*.46),(x+w*.12,y+w*.23),(x+w*.5,y),(x+w*.88,y+w*.23),(x+w,y+w*.46),(x+w,y+h)]
 poly(d,pts,fill)
 if edge:line(d,pts+[pts[0]],edge,width)

def rose(d,x,y,r,wall,ink,rim):
 ell(d,(x-r,y-r,x+r,y+r),rim);ell(d,(x-r+3,y-r+3,x+r-3,y+r-3),ink)
 for a in range(0,360,45):
  t=math.radians(a);ex=x+math.cos(t)*r*.71;ey=y+math.sin(t)*r*.71
  ell(d,(ex-r*.20,ey-r*.20,ex+r*.20,ey+r*.20),wall,outline=rim)
  line(d,[(x,y),(x+math.cos(t)*(r-2),y+math.sin(t)*(r-2))],rim)
 ell(d,(x-r*.16,y-r*.16,x+r*.16,y+r*.16),rim)

def cathedral(d,x,base,w,h,theme,layer=1):
 sky,far,stone,edge,ink,gold=map(rgb,PALETTES[theme]);rng=random.Random(int(x*13+w*7+h*5+theme))
 if layer==0:stone=mix(far,sky,.2);edge=mix(stone,edge,.19);ink=mix(stone,ink,.28);gold=edge
 elif layer==1:stone=mix(far,stone,.72);edge=mix(stone,edge,.28);ink=mix(ink,stone,.3);gold=mix(gold,stone,.7)
 y=base-h
 box(d,(x,y+w*.35,x+w,base),stone);poly(d,[(x-w*.05,y+w*.4),(x+w*.5,y),(x+w*1.05,y+w*.4)],edge)
 line(d,[(x,y+w*.4),(x+w*.5,y+w*.03),(x+w,y+w*.4)],ink,2)
 # Curved buttresses and layered cornices, not identical tiled houses.
 for side in (0,1):
  xx=x+side*w;box(d,(xx-3,y+w*.26,xx+3,base),edge)
  poly(d,[(xx-5,y+w*.3),(xx,y+w*.14),(xx+5,y+w*.3)],gold)
  for yy in range(round(y+w*.6),round(base),max(10,round(w*.65))):
   k=-1 if side==0 else 1
   bez(d,[(xx,yy),(xx+k*w*.25,yy+5),(xx+k*w*.25,yy+w*.28),(xx+k*w*.3,yy+w*.4)],edge,3)
 for yy in range(round(y+w*.53),round(base-9),max(16,round(w*.58))):
  line(d,[(x,yy+w*.39),(x+w,yy+w*.39)],edge,2)
  for j in range(3):
   ww=w*.18;xx=x+w*(.11+j*.29);arch(d,xx,yy,ww,w*.32,ink,edge,1)
   if layer and rng.random()<.22:box(d,(xx+ww*.25,yy+w*.11,xx+ww*.45,yy+w*.25),gold)
   if layer>1:line(d,[(xx+ww/2,yy+2),(xx+ww/2,yy+w*.30)],edge)
 if w>48:
  rose(d,x+w*.5,y+w*.31,w*.14,stone,ink,gold)
 if layer>1:
  for k in range(int(w*h/450)):
   xx=x+rng.randrange(max(1,int(w)));yy=y+w*.55+rng.random()*max(1,h-w*.55)
   line(d,[(xx,yy),(xx+2+rng.random()*5,yy)],mix(edge,stone,.55))

def tree(d,x,y,h,c,high,seed):
 rng=random.Random(seed);sign=-1 if seed%2 else 1
 bez(d,[(x,y),(x-h*.25*sign,y-h*.35),(x+h*.10*sign,y-h*.72),(x-h*.13*sign,y-h)],c,max(4,h*.055))
 for k in range(5):
  at=y-h*(.25+k*.14);dx=(-1 if k%2 else 1)*h*(.25+rng.random()*.15)
  bez(d,[(x,at),(x+dx*.35,at-h*.10),(x+dx*.85,at-h*.08),(x+dx,at-h*.31)],c,max(2,h*.024))
  line(d,[(x+dx*.42,at-h*.07),(x+dx*.58,at-h*.22)],high,1)
 for k in range(4):bez(d,[(x,y-h*.07),(x-h*.12+k*h*.08,y-h*.02),(x-h*.3+k*h*.2,y-h*.01),(x-h*.3+k*h*.2,y+h*.02)],c,max(2,h*.027))

def pear(d,x,y,s,c,edge,ink):
 poly(d,[(x-s*.1,y-s*.55),(x+s*.15,y-s*.55),(x+s*.24,y-s*.24),(x+s*.49,y+s*.19),(x+s*.43,y+s*.49),(x+s*.2,y+s*.58),(x-s*.3,y+s*.52),(x-s*.46,y+s*.22),(x-s*.31,y-s*.14)],ink)
 ell(d,(x-s*.38,y-s*.07,x+s*.4,y+s*.5),c);ell(d,(x-s*.15,y-s*.49,x+s*.20,y+s*.1),c)
 bez(d,[(x-s*.24,y+s*.28),(x-s*.43,y),(x-s*.07,y-s*.02),(x-s*.05,y-s*.37)],edge,max(1,s*.06))
 line(d,[(x+s*.04,y-s*.47),(x+s*.15,y-s*.72),(x+s*.32,y-s*.75)],ink,max(1,s*.045))
 ell(d,(x-s*.09,y+s*.13,x+s*.02,y+s*.24),ink);ell(d,(x+s*.13,y+s*.11,x+s*.22,y+s*.22),ink)
 bez(d,[(x,y+s*.34),(x+s*.08,y+s*.40),(x+s*.19,y+s*.36),(x+s*.23,y+s*.29)],ink,max(1,s*.024))

def statue(d,x,y,s,base,edge,ink,kneel=False):
 # Wrapped pilgrim; head and hands are deliberately legible inside a large robe silhouette.
 if not kneel:
  poly(d,[(x-s*.12,y+s*.23),(x-s*.27,y+s*.93),(x+s*.23,y+s*.89),(x+s*.16,y+s*.22)],base)
  for k in range(5):bez(d,[(x-s*.12+k*s*.06,y+s*.23),(x-s*.05+k*s*.035,y+s*.5),(x-s*.1+k*s*.045,y+s*.7),(x-s*.24+k*s*.10,y+s*.9)],edge,max(1,s*.008))
  ell(d,(x-s*.17,y-s*.13,x+s*.17,y+s*.25),ink);ell(d,(x-s*.14,y-s*.1,x+s*.14,y+s*.23),edge)
  ell(d,(x-s*.10,y+s*.045,x-s*.025,y+s*.115),ink);ell(d,(x+s*.035,y+s*.045,x+s*.11,y+s*.115),ink)
  line(d,[(x,y+s*.09),(x-s*.02,y+s*.19),(x+s*.055,y+s*.19)],base,max(1,s*.018))
  bez(d,[(x-s*.14,y+s*.28),(x-s*.39,y+s*.52),(x-s*.06,y+s*.52),(x+s*.04,y+s*.39)],base,max(2,s*.08))
  bez(d,[(x+s*.14,y+s*.28),(x+s*.36,y+s*.5),(x+s*.09,y+s*.49),(x+s*.04,y+s*.39)],base,max(2,s*.08))
  ell(d,(x-s*.04,y+s*.32,x+s*.10,y+s*.46),edge)
 else:
  poly(d,[(x-s*.42,y+s*.03),(x+s*.32,y+s*.03),(x+s*.42,y+s*.14),(x+s*.25,y+s*.30),(x-s*.35,y+s*.31)],ink)
  poly(d,[(x-s*.37,y+s*.06),(x+s*.28,y+s*.06),(x+s*.32,y+s*.14),(x+s*.12,y+s*.25),(x-s*.32,y+s*.25)],base)
  for k in range(7):line(d,[(x-s*.30+k*s*.075,y+s*.075),(x-s*.25+k*s*.065,y+s*.24)],edge,max(1,s*.014))
  ell(d,(x+s*.23,y+s*.10,x+s*.48,y+s*.36),edge);ell(d,(x+s*.35,y+s*.18,x+s*.42,y+s*.29),ink)
  bez(d,[(x+s*.05,y+s*.14),(x-s*.06,y+s*.40),(x+s*.31,y+s*.36),(x+s*.40,y+s*.31)],base,max(2,s*.07))
  line(d,[(x-s*.4,y+s*.02),(x+s*.28,y+s*.02)],edge,max(1,s*.025))

def background(theme):
 W,H=768,480;sky,far,stone,edge,ink,gold=map(rgb,PALETTES[theme]);rng=random.Random(664+theme)
 im=Image.new('RGB',(W,H));d=ImageDraw.Draw(im)
 for yy in range(H):
  c=mix(sky,far,(yy/H)*(.7 if theme==0 else .58));line(d,[(0,yy),(W,yy)],c)
 # Small, deliberately clustered light/dust marks. No unbounded texture noise.
 for i in range(260):
  x=rng.randrange(W);y=rng.randrange(H);c=mix(sky,gold,.20 if theme else .12)
  if rng.random()<.3:line(d,[(x,y),(x+2,y)],c)
 if theme==0:
  ell(d,(571,30,651,110),rgb('f2d89a'));ell(d,(580,35,641,100),rgb('f8e9bb'))
  for i,(x,y) in enumerate([(36,67),(300,91),(660,130)]):
   for k in range(5):ell(d,(x+k*17,y+6-(k%3)*5,x+k*17+45,y+17),mix(sky,(255,255,234),.4))
  for k in range(3):
   pts=[(0,480)]+[(x,325+k*37+math.sin(x*.012+k)*25) for x in range(-20,810,10)]+[(768,480)]
   poly(d,pts,mix(far,stone,.20+k*.12))
  for x,w,h in [(15,61,163),(98,82,216),(221,60,140),(295,111,249),(474,66,180),(575,86,202),(698,71,258)]:cathedral(d,x,440,w,h,0,2)
  for k in range(2):
   pts=[(x,110+k*29+math.sin(x*.005+k)*26) for x in range(-20,800,5)];line(d,pts,mix(ink,sky,.15),2)
   for x in range(15,768,35):
    y=110+k*29+math.sin(x*.005+k)*26;poly(d,[(x,y),(x+21,y+3),(x+9,y+23)],rgb(['ce8b7a','e9c879','749c94'][(x//35+k)%3]))
 elif theme==1:
  ell(d,(585,25,663,103),mix(stone,gold,.22));ell(d,(603,19,671,93),sky)
  for x,w,h in [(-22,58,230),(52,53,310),(142,70,256),(246,50,344),(315,80,280),(427,52,355),(497,85,295),(603,60,271),(686,70,332)]:cathedral(d,x,480,w,h,1,0)
  cathedral(d,-48,496,157,375,1,2);cathedral(d,548,508,145,421,1,2);cathedral(d,240,523,156,342,1,1)
  # Belfry mask and hanging laundry connect fiction with locomotion.
  rose(d,319,243,33,stone,ink,mix(gold,stone,.52))
  for y in (137,173):bez(d,[(-10,y),(240,y+90),(530,y-15),(790,y+36)],mix(stone,edge,.28),2)
  for x,y in [(72,162),(460,189),(653,179)]:
   poly(d,[(x,y),(x+29,y+4),(x+37,y+69),(x+15,y+60),(x-2,y+75)],mix(stone,ink,.15));line(d,[(x+5,y+4),(x+9,y+56)],edge)
  for x,y in [(95,355),(577,301),(350,426)]:
   ell(d,(x-12,y-13,x+12,y+14),mix(gold,sky,.85));box(d,(x-3,y-5,x+3,y+6),mix(gold,stone,.2));line(d,[(x-5,y-7),(x+5,y-7),(x+5,y+9),(x-5,y+9),(x-5,y-7)],ink,2)
 elif theme==2:
  ell(d,(255,10,434,177),mix(far,gold,.12));cathedral(d,301,490,119,296,2,0)
  for i,x in enumerate([35,170,284,431,577,720]):tree(d,x,493,395-i%3*38,far,mix(stone,far,.3),20+i)
  for x,seed in [(-5,11),(683,18)]:tree(d,x,485,445,ink,stone,seed)
  # Pears are witnesses, not arbitrary blobs: stems, cheeks, eye slits, and tethering.
  for x,y,s in [(90,200,74),(188,305,41),(504,134,91),(633,338,56),(352,359,47)]:
   bez(d,[(x,y-s*.7),(x+9,y-100),(x-11,92),(x+34,14)],mix(edge,stone,.5),2)
   pear(d,x,y,s,mix(stone,gold,.22),mix(edge,gold,.15),mix(ink,stone,.1))
  for i in range(35):
   x=rng.randrange(W);y=rng.randrange(70,460);s=rng.randrange(3,9)
   ell(d,(x-s,y-s*.5,x+s,y+s*.5),mix(stone,gold,.07));line(d,[(x-s,y),(x+s,y)],mix(stone,ink,.25))
 elif theme==3:
  for x,y,r in [(147,56,38),(619,103,25)]:ell(d,(x-r,y-r,x+r,y+r),mix(stone,edge,.5));ell(d,(x-r+5,y-r+5,x+r-5,y+r-5),mix(far,sky,.5))
  for x,w,h in [(-30,73,340),(69,67,460),(179,94,373),(286,63,480),(389,94,402),(513,69,452),(616,80,358),(712,73,474)]:cathedral(d,x,490,w,h,3,0)
  for x,w,h in [(-32,130,412),(449,155,434),(663,111,375)]:cathedral(d,x,503,w,h,3,2)
  # Fresco profile: a carved inhabitant of the architecture.
  statue(d,530,175,194,mix(stone,far,.2),mix(edge,stone,.5),mix(ink,stone,.12))
  for x in (105,275,610):
   bez(d,[(x,167),(x+40,212),(x+91,211),(x+129,196)],mix(stone,edge,.2),2)
   for j in range(0,125,13):line(d,[(x+j,198),(x+j,218)],mix(stone,far,.3))
  for i in range(4):line(d,[(-20,139+i*73),(788,69+i*79)],mix(ink,stone,.28),1)
 elif theme==4:
  for k in range(4):
   pts=[(0,H)]+[(x,324+k*33+math.sin(x*.009+k)*26) for x in range(-10,790,10)]+[(W,H)]
   poly(d,pts,mix(far,gold,.04+k*.06))
  cathedral(d,267,464,131,302,4,0);cathedral(d,593,513,94,274,4,0)
  for x,y,s in [(71,174,218),(490,63,291),(721,289,185)]:statue(d,x,y,s,mix(stone,far,.25),mix(edge,stone,.72),mix(ink,stone,.24))
  for x in (25,662):
   for k in range(23):
    xx=x+k*3.9;yy=k*24-15;ell(d,(xx,yy,xx+15,yy+32),None,outline=mix(edge,ink,.58),width=3);line(d,[(xx+3,yy+2),(xx+3,yy+11)],mix(edge,stone,.4),1)
  for k in range(23):
   x=rng.randrange(W);y=rng.randrange(H);poly(d,[(x,y),(x+3,y-2),(x+5,y+6),(x+2,y+9)],mix(stone,edge,.3))
 else:
  # False heaven: bright air, cold recesses, gold structural lines. Avoid beige fog over everything.
  for r in range(122,22,-12):ell(d,(374-r,83-r,374+r,83+r),None,outline=mix(sky,gold,.05+(122-r)/250),width=2)
  ell(d,(349,58,399,108),mix(gold,sky,.25));ell(d,(359,69,389,97),mix(ink,sky,.55))
  for x,w,h in [(-63,159,453),(130,100,381),(478,106,401),(661,141,487)]:
   cathedral(d,x,515,w,h,5,2)
   for dx in (8,w-11):
    box(d,(x+dx,86,x+dx+5,480),mix(sky,(255,249,224),.50));line(d,[(x+dx+6,87),(x+dx+6,480)],mix(gold,sky,.22),2)
  for x in (-36,267,559):
   arch(d,x,149,246,353,mix(sky,far,.14),mix(gold,sky,.4),2)
   for k in range(3):arch(d,x+6+k*5,155+k*6,234-k*10,350-k*6,mix(sky,far,.12),mix(gold,sky,.72),1)
  for x in (299,420):statue(d,x,282,147,mix(sky,far,.35),mix(sky,(255,247,218),.3),mix(ink,sky,.68))
 # Art pixels remain visible; no bloom, smooth blur or copied game imagery.
 save(im,'background_'+str(theme))

def material(theme):
 sky,far,stone,edge,ink,gold=map(rgb,PALETTES[theme]);rng=random.Random(410+theme)
 # Foreground saturation/value is intentionally separate from distant painting.
 sets=[('617347','9faf69','dcd39b','303c3e'),('633b59','926573','d4b19a','252336'),('4b5542','78804f','c3b77d','1e3435'),('365164','65828d','bec7b4','182b3b'),('6c4354','a16c78','ddba9a','302133'),('929fa2','cbd0bb','f4e7b9','3a5265')]
 base,mid,light,shadow=map(rgb,sets[theme]);im=Image.new('RGB',(64,64),base);d=ImageDraw.Draw(im)
 for row in range(4):
  yy=row*16
  for col in range(-1,3):
   xx=col*32+(row%2)*16;c=mix(base,mid,.10+rng.random()*.17)
   box(d,(xx+1,yy+1,xx+31,yy+15),c);line(d,[(xx+1,yy+2),(xx+30,yy+2)],mix(mid,base,.45));line(d,[(xx+1,yy+1),(xx+1,yy+13)],mix(mid,base,.62));line(d,[(xx,yy+15),(xx+32,yy+15)],shadow)
   if (row+col)%3==0:line(d,[(xx+18,yy+4),(xx+16,yy+8),(xx+20,yy+12)],mix(shadow,base,.47))
   for i in range(3):
    x=xx+rng.randrange(3,29);y=yy+rng.randrange(3,14);line(d,[(x,y),(x+2,y)],mix(mid,base,.5))
 if theme==2:
  for x in range(-4,68,9):bez(d,[(x,0),(x+8,17),(x-8,45),(x+3,64)],shadow,2);bez(d,[(x+2,0),(x+10,17),(x-6,45),(x+5,64)],mid,1)
 if theme==5:
  for x in (16,48):
   for y in (16,48):
    poly(d,[(x,y-6),(x+7,y),(x,y+6),(x-7,y)],mix(base,light,.13));line(d,[(x,y-5),(x+6,y),(x,y+5)],mix(gold,base,.65))
 save(im,'fill_'+str(theme))
 # A one-unit cornice, designed for the actual 0.4-unit collision thickness.
 im=Image.new('RGBA',(32,16),(0,0,0,0));d=ImageDraw.Draw(im)
 box(d,(0,0,31,12),shadow);box(d,(0,1,31,4),light);box(d,(0,5,31,9),mid);box(d,(0,10,31,12),base)
 line(d,[(0,0),(31,0)],light);line(d,[(0,4),(31,4)],shadow)
 for x in (4,16,28):
  box(d,(x,6,x+4,10),base);line(d,[(x,6),(x+4,6)],light);box(d,(x+1,7,x+2,8),gold)
 if theme==0:
  for x in range(0,32,3):line(d,[(x,2),(x+1,0)],light)
 if theme==2:
  for x in (2,13,26):poly(d,[(x,10),(x+4,12),(x+1,15)],mid)
 if theme==4:
  for x in (4,20):line(d,[(x,2),(x+5,8),(x+10,2)],gold)
 save(im,'lip_'+str(theme))
 # Modular corbels read as support, not an extra walking surface.
 im=Image.new('RGBA',(64,64),(0,0,0,0));d=ImageDraw.Draw(im)
 poly(d,[(0,0),(63,0),(63,10),(53,10),(48,20),(40,23),(35,39),(29,39),(24,23),(16,20),(11,10),(0,10)],shadow)
 poly(d,[(3,2),(60,2),(53,9),(47,15),(38,18),(33,30),(29,22),(25,17),(17,15),(12,7)],base)
 line(d,[(6,4),(56,4),(48,11),(38,14),(33,23)],mid,2);ell(d,(26,6,39,17),shadow);ell(d,(29,8,36,14),gold)
 save(im,'corbel_'+str(theme))

def icon(name):
 im=Image.new('RGBA',(64,64),(0,0,0,0));d=ImageDraw.Draw(im);ink=rgb('16192b');shadow=rgb('4f3854');gold=rgb('e7b962');cream=rgb('fff0bc');pink=rgb('d64775');teal=rgb('71c8bd')
 if name=='nail':
  poly(d,[(28,12),(38,12),(39,51),(32,61),(25,51)],ink);box(d,(29,20,35,50),gold);line(d,[(29,49),(32,56),(35,49)],cream,2)
  ell(d,(12,4,51,32),ink);ell(d,(16,7,48,28),shadow);ell(d,(20,10,44,24),gold);ell(d,(28,11,37,23),ink);line(d,[(18,28),(14,35),(9,35)],gold,2);line(d,[(47,28),(51,35),(56,35)],gold,2)
 elif name=='portal':
  arch(d,9,2,47,60,ink,ink,2);arch(d,14,6,37,56,shadow,gold,2);arch(d,20,13,25,49,ink,teal,2)
  for y in range(28,59,8):line(d,[(15,y),(20,y+3)],gold);line(d,[(45,y+3),(50,y)],cream)
  ell(d,(24,4,39,18),ink);ell(d,(28,8,35,15),pink);box(d,(7,58,57,62),ink);box(d,(12,58,53,59),teal)
 elif name=='hinge':
  ell(d,(9,9,54,54),ink);ell(d,(13,13,50,50),shadow);ell(d,(17,17,46,46),gold);ell(d,(22,22,41,41),ink);ell(d,(28,28,36,36),cream)
  for a in range(0,360,90):x=32+int(math.cos(math.radians(a))*17);y=32+int(math.sin(math.radians(a))*17);ell(d,(x-2,y-2,x+2,y+2),cream)
 elif name=='coin':
  ell(d,(10,7,53,57),ink);ell(d,(14,9,49,53),gold);ell(d,(18,13,45,50),rgb('a96a43'),cream,2);poly(d,[(32,17),(38,30),(32,44),(25,31)],cream)
 elif name=='key':
  ell(d,(18,5,47,34),ink);ell(d,(22,9,43,30),gold);ell(d,(29,14,36,25),ink);box(d,(27,30,36,58),ink);box(d,(30,28,34,53),gold);box(d,(31,44,46,48),ink);box(d,(32,44,44,46),gold);box(d,(31,53,42,57),ink);box(d,(32,53,41,55),gold)
 elif name=='mercy':
  for a in range(0,360,60):x=32+math.cos(math.radians(a))*17;y=32+math.sin(math.radians(a))*17;ell(d,(x-7,y-10,x+7,y+10),ink);ell(d,(x-4,y-7,x+4,y+7),cream)
  ell(d,(20,20,43,43),ink);ell(d,(24,24,39,39),pink);ell(d,(29,27,34,36),cream)
 elif name=='heart':
  poly(d,[(10,24),(13,13),(23,9),(32,17),(41,9),(51,13),(55,25),(48,38),(32,55),(16,38)],ink)
  poly(d,[(15,23),(17,17),(24,14),(32,22),(41,14),(48,18),(50,25),(44,36),(32,49),(21,37)],pink);line(d,[(20,20),(24,18),(28,23)],cream,3)
 save(im,name)

def figures():
 im=Image.new('RGBA',(96,128));d=ImageDraw.Draw(im);statue(d,48,20,103,rgb('866c83'),rgb('d1bca8'),rgb('1b2033'));save(im,'penitent_falling')
 im=Image.new('RGBA',(128,64));d=ImageDraw.Draw(im);statue(d,55,6,121,rgb('886a7f'),rgb('d9bfa8'),rgb('1b2033'),True);save(im,'penitent_kneeling')
 for i in range(3):
  im=Image.new('RGBA',(64,96));d=ImageDraw.Draw(im);pear(d,31,48,55,rgb(['829947','d8a456','71594b'][i]),rgb(['becb74','f1d386','9d8770'][i]),rgb('182d32'));save(im,'pear_'+str(i))

def bosses():
 ink=rgb('101726');dark=rgb('382c46');mid=rgb('784767');pink=rgb('d4547c');bone=rgb('e0c7a1');white=rgb('fff0c9');gold=rgb('d1a257');blue=rgb('6497a2')
 def eye(d,x,y,r=8):ell(d,(x-r-2,y-r-3,x+r+2,y+r+3),ink);ell(d,(x-r,y-r-1,x+r,y+r+1),gold);ell(d,(x-r+2,y-r,x+r-1,y+r-1),white);ell(d,(x-2,y-r+2,x+2,y+r-2),ink)
 for w in range(1,6):
  for phase in range(3):
   im=Image.new('RGBA',(128,128));d=ImageDraw.Draw(im)
   if w==1:
    poly(d,[(42,39),(17,109),(7,113),(30,123),(63,112),(96,123),(123,113),(110,103),(88,37)],ink)
    poly(d,[(45,43),(24,107),(63,97),(101,111),(87,43)],mid)
    for x in (32,42,80,91):line(d,[(x,62),(x-10 if x<64 else x+10,107)],pink,2)
    ell(d,(33,24,96,83),ink);ell(d,(39,29,90,78),bone);eye(d,53,48,7);eye(d,78,48,6)
    d.arc((45,47,87,73),0,180,fill=ink,width=4)
    for x in range(49,83,6):line(d,[(x,64),(x,69)],white)
    box(d,(25,25,102,33),ink);poly(d,[(41,26),(45,2),(87,5),(90,26)],ink);box(d,(45,16,86,25),dark);line(d,[(45,22),(87,22)],gold,3)
    for x in (12,111):ell(d,(x-8,68,x+7,84),ink);ell(d,(x-5,70,x+4,80),bone)
    for k in range(phase+1):ell(d,(50+k*9,84,62+k*9,99),ink);eye(d,56+k*9,90,3)
   elif w==2:
    pear(d,65,58,80,rgb('95884b'),rgb('dfc589'),ink);eye(d,55,60,7);eye(d,78,58,8)
    box(d,(36,20,92,27),ink);box(d,(42,16,87,22),gold)
    line(d,[(20,53),(8,68),(26,77)],ink,5);line(d,[(101,52),(117,72),(102,80)],ink,5)
    for x in (15,110):line(d,[(x,45),(x,95)],bone);line(d,[(x-9,91),(x+10,91)],gold,2);d.arc((x-9,83,x+10,101),0,180,fill=gold,width=2)
    for x in (43,79):poly(d,[(x,94),(x-11,119),(x+11,121),(x+7,98)],ink);line(d,[(x,105),(x-3,115)],gold,3)
   elif w==3:
    poly(d,[(45,30),(30,108),(12,124),(44,121),(59,67),(71,69),(89,121),(116,121),(101,109),(82,29)],ink)
    poly(d,[(49,35),(41,110),(59,82),(78,111),(78,36)],blue)
    for x in (22,105):line(d,[(x,7),(x,92)],ink,8);line(d,[(x,8),(x,90)],gold,3)
    ell(d,(38,11,91,63),ink);poly(d,[(45,20),(77,17),(88,33),(74,59),(62,53),(43,41)],bone);eye(d,57,32,6);eye(d,77,30,5)
    poly(d,[(62,31),(63,48),(72,43)],gold);line(d,[(49,20),(78,17)],white,2)
    line(d,[(25,57),(44,66),(32,75)],ink,6);line(d,[(105,57),(83,68),(99,79)],ink,6)
    rose(d,66,82,12,blue,ink,gold)
   elif w==4:
    # Crowd avalanche. Unequal heads and clinging hands, no generic sphere portrait.
    for x,y,r in [(45,73,40),(75,60,35),(57,35,28),(89,87,28),(28,91,24)]:ell(d,(x-r,y-r,x+r,y+r),ink);ell(d,(x-r+4,y-r+4,x+r-4,y+r-4),mid)
    for k,(x,y,s) in enumerate([(37,47,25),(65,27,27),(84,49,24),(24,79,22),(55,76,27),(91,88,25),(63,111,19)]):
     ell(d,(x-s*.4,y-s*.5,x+s*.4,y+s*.45),ink);ell(d,(x-s*.32,y-s*.42,x+s*.32,y+s*.38),bone)
     line(d,[(x-s*.18,y-s*.08),(x-s*.18,y+s*.10)],ink,3);line(d,[(x+s*.17,y-s*.08),(x+s*.17,y+s*.10)],ink,3)
     line(d,[(x-2,y+s*.22),(x+3,y+s*.22)],ink,2)
    for x,y in [(12,59),(104,38),(33,111),(113,101)]:
     bez(d,[(x,y),(x-12,y-20),(x+18,y-12),(x+3,y+8)],bone,5)
     for k in range(3):line(d,[(x+k*3,y),(x+1+k*3,y+10)],white,1)
   else:
    for k in range(9):
     a=k*math.pi*2/9;r=45;ex=64+math.cos(a)*r;ey=59+math.sin(a)*r
     bez(d,[(64,60),(ex+math.sin(a)*26,ey+math.cos(a)*13),(ex+math.sin(a)*29,ey-30),(ex*1.1-5,ey*.9+8)],ink,7)
     bez(d,[(64,60),(ex+math.sin(a)*26,ey+math.cos(a)*13),(ex+math.sin(a)*29,ey-30),(ex*1.1-5,ey*.9+8)],bone,2)
    ell(d,(26,23,105,108),ink);ell(d,(32,29,99,103),dark);ell(d,(38,35,93,97),mid)
    eye(d,53,53,14);eye(d,84,42,9)
    poly(d,[(48,82),(56,67),(88,69),(91,87),(79,100),(52,95)],ink)
    for x in range(57,88,7):poly(d,[(x,70),(x+5,71),(x+2,80)],bone)
    bez(d,[(73,94),(113,108),(59,119),(78,124)],pink,7);line(d,[(70,100),(93,109),(88,112)],gold,2)
    for k in range(7):
     a=k*math.pi/6;ex=64+math.cos(a)*32;ey=26-math.sin(a)*19;ell(d,(ex-4,ey-4,ex+4,ey+4),gold);ell(d,(ex-1,ey-2,ex+1,ey+2),ink)
   # Minute phase-specific cracking is visible without changing the boss's footprint.
   for k in range(phase):line(d,[(39+k*39,31),(46+k*24,44),(41+k*30,57)],pink,2)
   save(im,f'boss_{w}_{phase}')

for world in range(6):background(world);material(world)
for name in ('nail','portal','hinge','coin','key','mercy','heart'):icon(name)
figures();bosses()
(OUT/'MANIFEST.json').write_text(json.dumps({'version':1,'method':'Original deterministic pixel drawing; no imported Nintendo or generated-comparison assets','assets':MANIFEST},indent=2)+'\n',encoding='utf-8')
print('VISUAL_ASSETS_GENERATED',len(MANIFEST))
