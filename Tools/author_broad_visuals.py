"""Whole-campaign art kit. Authored palette/material/profile combinations, not screenshot patches.
Only original shapes and already-credited CC0 architectural components are used. Nintendo files are never read.
"""
from pathlib import Path
import sys,math,random,hashlib,json,uuid
ROOT=Path(__file__).resolve().parents[1];sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image,ImageDraw,ImageEnhance
OUT=ROOT/'Assets/GloomBean/Resources/BroadVisual';OUT.mkdir(exist_ok=True)
# ink, deep, recess, mid, lit, highlight, accent. Each level has a designed material bank.
PROFILES=[
('Sunday parade','wood','251d35 46344b 825564 bc827a eac395 fff0b9 87a872'),
('Under the parade','wood','16162a 34243e 64384e 995766 cd8b81 f5c494 d3a14c'),
('Belfry','stone','141a2a 283343 475363 797d82 b2a6a1 f0d1a0 5e9394'),
('Puppet laundry','machine','16182b 34324c 68506a 9c718a cea49c f9d0a3 c39147'),
('Borrowed skin','skin','25152e 4a263e 75435b b16a75 d89a95 ffc4a9 9a8c5b'),
('Pear gallows','root','101f28 243d39 52654c 899361 b9be7d e5dba0 c28d4c'),
('Saint kitchen','ceramic','192030 343c52 56657b 879ba5 c5d1c0 f6edd0 c89962'),
('Irrigation','root','101b28 223640 3c6661 689281 a3b59b e6d9b1 759abb'),
('Winter greenhouse','glass','161e30 293d53 4b6977 739396 adc4b1 eff0ca bfa26b'),
('Two suns tenement','stone','17182f 34334f 5b5277 8e77a0 bdabb5 f1dbc0 e2a262'),
('Fresco','plaster','262032 4c3a52 80566b ad8390 d4afa6 f7dfbd 93a1a2'),
('Tax office','wood','141d29 293742 4e5b58 85857a b9b79d e9dfb4 b9a05e'),
('Lodging carousel','cloth','211c30 423148 704b68 a6738a cca5a8 f2d7bd 689a9e'),
('Rain of witnesses','ruin','1c192f 3b2b49 63445f 96697e c29596 edc5af b89c76'),
('Hem bridge','cloth','191e30 353b55 5b627d 8a8ba2 bec0b6 e9e4ca c47e7d'),
('Coffin parade','wood','1e1728 402d3e 6e4850 a7756c d6a48e f6d3a4 bca158'),
('Freefall cathedral','stone','19182d 353049 635872 978199 c6aeb7 efd6bd bb9260'),
('Iron choir','machine','202135 454158 736b7b a098a0 d0c1ac fbe5b2 b99b57'),
('Noon shadows','plaster','211e36 423d58 746b86 a69ca9 d3c7b4 fbe9be dba867'),
('Scripture','paper','251e37 49374e 796176 a59196 d4c0a3 f4e1b3 a05873'),
('White gate','ceramic','211e32 454058 766881 a99ca8 d4c9be fff0c7 ca9f64'),
('Kindly Usher','cloth','21182e 432c48 714767 a57488 cda1a6 f7d5ae dca459'),
('Orchard Judge','root','182326 33433a 5b6a4b 8a9566 bcc18a eadeaa d2a353'),
('Surveyor','plaster','141c31 2e3752 53627a 8396a3 b8c5b8 eae7c6 d69782'),
('Weight of Everyone','ruin','21192f 41314e 6e4b69 9a718b c7a5a8 f3d2b3 b18f58'),
('Host of Hosts','machine','211d33 423950 715775 a08197 c7b0b5 f6d7b4 dab366')]
def cols(i):return ['#'+x for x in PROFILES[i][2].split()]
def save(im,name):
 f=OUT/(name+'.png');im.save(f,optimize=True)
 meta=Path(str(f)+'.meta')
 if not meta.exists():
  # Complete fixed import prevents shape=2 cube imports and guarantees pixel/readback contracts.
  template=(ROOT/'Assets/GloomBean/Resources/VisualV6/background_7.png.meta').read_text(encoding='utf-8')
  import re
  template=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/broad/'+name).hex,template)
  meta.write_text(template,encoding='utf-8')
def block(d,xy,c):d.rectangle(tuple(round(x) for x in xy),fill=c)
def line(d,xy,c,w=1):d.line([(round(x),round(y)) for x,y in xy],fill=c,width=w)
def oval(d,xy,c,outline=None,w=1):d.ellipse(tuple(round(x) for x in xy),fill=c,outline=outline,width=w)
def bez(d,points,c,width=1):
 a,b,e,f=points;ps=[]
 for k in range(33):t=k/32;u=1-t;ps.append((u*u*u*a[0]+3*u*u*t*b[0]+3*u*t*t*e[0]+t*t*t*f[0],u*u*u*a[1]+3*u*u*t*b[1]+3*u*t*t*e[1]+t*t*t*f[1]))
 line(d,ps,c,width)
def arch(d,x,y,w,h,c,edge=None):
 r=w/2;block(d,(x,y+r,x+w,y+h),c);oval(d,(x,y,x+w,y+w),c)
 if edge:d.arc((x,y,x+w,y+w),180,360,fill=edge,width=2);line(d,[(x,y+r),(x,y+h)],edge,2);line(d,[(x+w,y+r),(x+w,y+h)],edge,2)
def gear(d,x,y,r,c):
 for k in range(12):
  a=k*math.tau/12;xx=x+math.cos(a)*r;yy=y+math.sin(a)*r;block(d,(xx-2,yy-2,xx+2,yy+2),c[0])
 oval(d,(x-r+2,y-r+2,x+r-2,y+r-2),c[2],c[0],2);oval(d,(x-r+5,y-r+5,x+r-5,y+r-5),c[1],c[4],1)
 for k in range(6):a=k*math.tau/6;line(d,[(x,y),(x+math.cos(a)*(r-5),y+math.sin(a)*(r-5))],c[3],2)
 oval(d,(x-3,y-3,x+3,y+3),c[0]);oval(d,(x-1,y-1,x+1,y+1),c[5])
def stone(im,c,rng,plaster=False):
 d=ImageDraw.Draw(im)
 for y in range(0,im.height,12):
  x=-rng.randint(0,15)
  while x<im.width:
   w=rng.choice([14,18,22,27]);h=11
   block(d,(x,y,x+w-2,y+h-1),c[2]);line(d,[(x+1,y+1),(x+w-4,y+1)],c[4]);line(d,[(x+1,y+2),(x+w-4,y+2)],c[3]);line(d,[(x+w-2,y+2),(x+w-2,y+h)],c[0]);line(d,[(x+2,y+h-2),(x+w-4,y+h-2)],c[1])
   if rng.random()<.4:line(d,[(x+w//2,y+1),(x+w//2-2,y+5),(x+w//2+1,y+7)],c[1])
   if plaster: block(d,(x+3,y+3,x+w-4,y+7),c[3])
   x+=w
 return im
for i,(name,kind,palette) in enumerate(PROFILES):
 c=cols(i);rng=random.Random(1700+i);im=Image.new('RGBA',(64,32),c[1]);d=ImageDraw.Draw(im)
 if kind in ['stone','ruin','plaster']:stone(im,c,rng,kind=='plaster')
 elif kind in ['wood','root']:
  for y in range(0,32,16):
   block(d,(0,y,63,y+14),c[2]);line(d,[(0,y),(63,y)],c[4],2);line(d,[(0,y+15),(63,y+15)],c[0],2)
   for k in range(7):
    x=rng.randrange(-10,55);yy=y+rng.randrange(3,13);bez(d,[(x,yy),(x+6,yy-2),(x+10,yy+3),(x+25,yy)],c[3] if k%2 else c[1])
   oval(d,(15+y,y+3,26+y,y+11),c[1]);oval(d,(17+y,y+4,24+y,y+9),c[2]);line(d,[(20+y,y+5),(22+y,y+8)],c[0])
  if kind=='root':
   for x in [7,26,51]:bez(d,[(x,0),(x+10,8),(x-8,20),(x+3,31)],c[0],4);bez(d,[(x,0),(x+9,8),(x-7,20),(x+2,31)],c[3],2)
  else:
   for x in (0,59):block(d,(x,0,x+4,31),c[0]);block(d,(x+1,1,x+2,30),c[6]);oval(d,(x,6,x+3,9),c[5]);oval(d,(x,23,x+3,26),c[1])
 elif kind=='machine':
  block(d,(1,1,62,30),c[3]);block(d,(3,3,60,28),c[1]);block(d,(5,5,58,26),c[2]);line(d,[(5,5),(58,5),(58,25)],c[4]);line(d,[(6,24),(56,24)],c[0],2)
  for x in [2,59]:
   for y in [2,27]:oval(d,(x,y,x+3,y+3),c[0]);d.point((x+1,y+1),fill=c[5])
  for x in range(10,55,6):line(d,[(x,11),(x,20)],c[0],2);line(d,[(x+2,11),(x+2,19)],c[3])
  block(d,(27,6,36,8),c[6]);block(d,(29,6,34,6),c[5])
 elif kind in ['ceramic','glass']:
  for y in range(0,32,16):
   for x in range(0,64,16):
    block(d,(x+1,y+1,x+14,y+14),c[3]);line(d,[(x+2,y+2),(x+13,y+2),(x+13,y+12)],c[4]);line(d,[(x+2,y+13),(x+13,y+13)],c[2]);d.polygon([(x+8,y+5),(x+11,y+8),(x+8,y+11),(x+5,y+8)],fill=c[2]);d.point((x+8,y+7),fill=c[5])
    if kind=='glass':line(d,[(x+3,y+10),(x+9,y+4)],c[5]);line(d,[(x+7,y+12),(x+12,y+7)],c[4])
 elif kind=='cloth':
  for x in range(64):
   t=int((math.sin(x/5)+1)*1.4);line(d,[(x,0),(x,31)],c[2+t])
  for y in range(3,32,9):
   for x in range(3,64,8):line(d,[(x,y),(x+3,y+2)],c[6]);line(d,[(x+3,y+2),(x+5,y)],c[0])
 elif kind=='skin':
  block(d,(1,1,62,30),c[3]);
  for x in range(2,64,16):bez(d,[(x,0),(x-4,9),(x+7,21),(x+2,31)],c[1],2)
  for y in range(3,32,7):
   for x in range(3,64,16):line(d,[(x-2,y),(x+4,y+2)],c[5]);line(d,[(x-1,y+2),(x+4,y+3)],c[2])
 else:
  block(d,(1,1,62,30),c[4]);block(d,(3,3,60,28),c[3]);line(d,[(3,3),(60,3)],c[5]);
  for y in (7,13,20):
   for x in range(6,58,7):line(d,[(x,y+2),(x+2,y),(x+4,y+3)],c[1]);d.point((x+5,y+3),fill=c[2])
 save(im,f'face_{i}')
 # Wide architectural cornice, flat top is the exact collision contact. Irregular underside is recessed.
 im=Image.new('RGBA',(64,16));d=ImageDraw.Draw(im)
 block(d,(0,0,63,1),c[0]);block(d,(0,2,63,3),c[5]);block(d,(0,4,63,6),c[4]);block(d,(0,7,63,10),c[2]);line(d,[(0,11),(63,11)],c[0],2)
 for x in range(0,64,16):line(d,[(x,4),(x,10)],c[1]);line(d,[(x+1,4),(x+1,6)],c[3]);block(d,(x+5,9,x+11,13),c[1]);block(d,(x+6,9,x+10,11),c[3])
 if kind=='root':
  for x in range(4,64,11):line(d,[(x,10),(x+2,15)],c[1]);d.polygon([(x,3),(x-4,6),(x+3,6)],fill=c[6])
 save(im,f'trim_{i}')
 # Recessed under-platform silhouette carries material character without a new walkable face.
 im=Image.new('RGBA',(64,48));d=ImageDraw.Draw(im)
 if kind in ['root','wood']:
  bez(d,[(2,1),(15,13),(39,2),(61,5)],c[0],11);bez(d,[(2,0),(17,9),(38,1),(61,2)],c[2],7);bez(d,[(2,0),(18,6),(40,1),(59,1)],c[4],2)
  for x in [9,26,48]:bez(d,[(x,3),(x+6,12),(x-9,27),(x+1,39)],c[0],6);bez(d,[(x,3),(x+5,13),(x-6,24),(x+1,37)],c[2],3)
  if kind=='wood':line(d,[(6,3),(29,32),(58,3)],c[2],6);line(d,[(7,3),(30,30),(58,3)],c[4],1)
 elif kind in ['cloth','skin','paper']:
  d.polygon([(2,0),(62,0),(58,29),(46,36),(33,30),(20,44),(5,29)],fill=c[0]);d.polygon([(5,0),(59,0),(55,26),(45,32),(34,26),(21,39),(8,27)],fill=c[2]);
  for x in [12,25,40,51]:bez(d,[(x,0),(x-3,14),(x+4,20),(x,29)],c[4] if x%2 else c[3],2)
  for x in range(7,58,6):line(d,[(x,2),(x+2,5)],c[5])
 elif kind=='machine':
  block(d,(2,0,61,7),c[0]);block(d,(4,1,59,5),c[3]);line(d,[(10,4),(30,30),(55,4)],c[0],8);line(d,[(10,4),(30,28),(55,4)],c[2],5);line(d,[(10,4),(30,27),(55,4)],c[4],1);gear(d,32,23,12,c)
 else:
  d.polygon([(0,0),(63,0),(61,12),(51,12),(49,23),(41,23),(36,38),(27,38),(22,23),(13,23),(10,12),(2,12)],fill=c[0]);d.polygon([(3,1),(60,1),(58,9),(48,10),(45,20),(37,22),(33,33),(29,33),(26,20),(16,20),(13,9),(5,9)],fill=c[2]);line(d,[(6,2),(56,2),(53,7)],c[4],2);line(d,[(18,10),(44,10)],c[3],2);line(d,[(27,22),(33,31)],c[4],1)
 save(im,f'apron_{i}')
 # 128x112 room-bay composition. Repeated over complete floor supports, never tied to a review camera.
 im=Image.new('RGBA',(128,112));d=ImageDraw.Draw(im);wall=Image.new('RGBA',(128,112),c[1]);stone(wall,[c[0],c[0],c[1],c[2],c[2],c[3],c[6]],rng);im.alpha_composite(wall)
 # Framing columns read as BACKGROUND: one value step below solid floors, dark outline.
 block(d,(0,0,6,111),c[0]);block(d,(121,0,127,111),c[0]);block(d,(7,0,10,111),c[2]);block(d,(117,0,120,111),c[2]);line(d,[(11,2),(116,2)],c[3],2)
 if i in [0,1,9,12,21]:
  for x in [18,75]:
   arch(d,x,13,34,61,c[0],c[2]);arch(d,x+4,18,26,51,c[2]);block(d,(x+7,28,x+27,61),c[3] if i==0 else c[1]);line(d,[(x+17,19),(x+17,70)],c[4]);line(d,[(x+5,40),(x+29,40)],c[2],3)
   for xx in [x-3,x+35]:block(d,(xx,29,xx+5,69),c[2]);line(d,[(xx+1,29),(xx+1,67)],c[3])
  block(d,(19,87,108,96),c[0]);block(d,(20,87,107,89),c[3]);block(d,(23,97,25,110),c[2]);block(d,(102,97,104,110),c[2])
  for x in [38,83]:oval(d,(x,76,x+8,83),c[3]);line(d,[(x+4,81),(x+4,87)],c[6])
 elif i==2:
  arch(d,27,8,73,94,c[0],c[2]);line(d,[(62,4),(62,25)],c[3],2);d.polygon([(46,28),(78,28),(84,60),(93,68),(34,68),(41,60)],fill=c[0]);d.polygon([(48,30),(75,30),(80,59),(86,64),(41,64),(46,58)],fill=c[3]);line(d,[(50,33),(49,55)],c[4],2);block(d,(37,65,90,68),c[6]);oval(d,(59,66,69,77),c[4]);gear(d,17,86,15,c);gear(d,106,26,13,c);line(d,[(16,30),(16,108)],c[2]);line(d,[(107,50),(107,108)],c[2])
 elif i==3:
  for x in [23,102]:gear(d,x,21,14,c);line(d,[(x-8,23),(x-8,95)],c[2],2);line(d,[(x+8,23),(x+8,95)],c[3],1)
  bez(d,[(15,26),(36,38),(88,40),(111,26)],c[4],2)
  for x,y in [(33,33),(59,36),(83,33)]:
   d.polygon([(x,y),(x+17,y),(x+19,y+44),(x+14,y+48),(x+4,y+41),(x-3,y+47)],fill=c[0]);d.polygon([(x+2,y),(x+15,y),(x+16,y+41),(x+9,y+39),(x,y+42)],fill=c[2]);line(d,[(x+4,y+2),(x+3,y+35)],c[3]);block(d,(x+4,y-2,x+6,y+3),c[6])
  block(d,(18,91,110,108),c[0]);block(d,(21,92,107,95),c[3]);line(d,[(22,98),(108,98)],c[2])
 elif i in [4,14]:
  for x in [28,67,103]:
   line(d,[(x,0),(x,22)],c[4]);d.polygon([(x-16,20),(x+11,20),(x+15,47),(x+6,73),(x-15,79),(x-20,48)],fill=c[0]);d.polygon([(x-13,23),(x+8,23),(x+12,47),(x+3,68),(x-12,74),(x-17,48)],fill=c[2]);bez(d,[(x-3,23),(x+4,38),(x-7,55),(x+1,70)],c[0],2)
   for y in range(25,69,7):line(d,[(x-5,y),(x+2,y+2)],c[4])
  gear(d,26,97,13,c);gear(d,98,99,12,c)
 elif i in [5,7,8,22]:
  # Environmental root greenhouse: big asymmetrical arches, irrigation and fruit/leaf silhouettes.
  for x in [12,112]:bez(d,[(x,112),(x-12,85),(x+16,42),(x,0)],c[0],14);bez(d,[(x,112),(x-9,85),(x+15,43),(x,0)],c[2],8);bez(d,[(x-1,112),(x-11,85),(x+12,43),(x-2,0)],c[3],2)
  for x,y in [(26,10),(73,4),(107,17)]:
   bez(d,[(x,0),(x-6,12),(x+8,18),(x,31)],c[2],3);oval(d,(x-7,y+28,x+7,y+46),c[0]);oval(d,(x-5,y+29,x+5,y+44),c[3]);line(d,[(x-2,y+30),(x-3,y+35)],c[4])
  if i in [7,8]:
   for x in range(18,125,24):line(d,[(x,0),(x,109)],c[2]);line(d,[(x+1,0),(x+1,109)],c[3])
   for y in [23,54,87]:line(d,[(8,y),(119,y)],c[2],2)
  block(d,(24,88,103,109),c[0]);line(d,[(26,89),(100,89)],c[4],2)
  for x in range(31,98,11):line(d,[(x,89),(x-3,68)],c[2],2);d.polygon([(x-3,76),(x-13,70),(x-10,66),(x-3,72),(x+4,63),(x+8,67)],fill=c[3])
 elif i==6:
  # Glazed ovens, flues and copper pots. It is a kitchen, not an orchard recolor.
  block(d,(18,43,111,110),c[0]);block(d,(20,45,109,49),c[4]);
  for x in [24,69]:arch(d,x,57,34,45,c[2],c[3]);arch(d,x+4,61,26,37,c[0]);d.arc((x+6,69,x+28,92),0,180,fill=c[6],width=3)
  for x in [31,96]:block(d,(x,1,x+9,42),c[0]);block(d,(x+2,2,x+6,40),c[3]);block(d,(x-4,10,x+12,14),c[2])
  for x,y in [(34,32),(72,32),(104,28)]:oval(d,(x-10,y-8,x+10,y+10),c[0]);oval(d,(x-8,y-7,x+8,y+8),c[6]);d.arc((x-13,y-14,x+13,y+6),180,360,fill=c[2],width=2);line(d,[(x-5,y-4),(x-5,y+4)],c[4])
  for x in [56,78]:line(d,[(x,0),(x,17)],c[4]);oval(d,(x-3,17,x+3,28),c[3])
 elif i in [10,11,19,23]:
  if i==11:
   for y in [29,60,91]:
    block(d,(16,y,114,y+3),c[3]);block(d,(17,y+4,114,y+6),c[0]);
    for x in range(20,108,9):block(d,(x,y-21,x+5,y-1),c[rng.choice([2,3,6])]);line(d,[(x+1,y-19),(x+4,y-19)],c[4]);line(d,[(x+1,y-6),(x+4,y-6)],c[1])
  else:
   for x in [20,73]:
    block(d,(x,13,x+35,94),c[0]);block(d,(x+2,15,x+33,92),c[3]);block(d,(x+5,18,x+30,89),c[2]);
    if i==19:
     for y in range(28,82,9):
      for xx in range(x+9,x+28,5):line(d,[(xx,y),(xx+1,y-2),(xx+3,y+2)],c[0])
    else:
     oval(d,(x+13,29,x+22,40),c[4]);d.polygon([(x+17,42),(x+7,76),(x+29,76),(x+22,43)],fill=c[3]);line(d,[(x+8,49),(x+29,53)],c[4],2)
   line(d,[(19,104),(111,104)],c[3],2)
 elif i==15:
  for x in [18,70]:
   d.polygon([(x+9,13),(x+28,13),(x+37,32),(x+31,98),(x+6,98),(x,32)],fill=c[0]);d.polygon([(x+11,17),(x+25,17),(x+32,32),(x+27,93),(x+10,93),(x+5,32)],fill=c[2]);line(d,[(x+10,19),(x+7,32),(x+12,90)],c[4],2);line(d,[(x+18,34),(x+18,74)],c[6],2);line(d,[(x+10,49),(x+27,49)],c[6],2)
 else:
  for x in [21,78]:
   arch(d,x,8,32,92,c[0],c[3]);arch(d,x+5,15,22,78,c[2]);line(d,[(x+16,17),(x+16,92)],c[3]);line(d,[(x+5,50),(x+27,50)],c[3]);
   for y in [30,64,81]:d.polygon([(x+16,y-6),(x+22,y),(x+16,y+6),(x+10,y)],fill=c[1]);d.point((x+16,y-2),fill=c[4])
  if i in [17,25]:gear(d,64,56,25,c)
  elif i==18:oval(d,(44,8,84,48),c[3],c[0],2);oval(d,(58,17,73,39),c[0]);line(d,[(64,47),(64,107)],c[2],2)
  elif i==20:d.polygon([(64,10),(107,50),(64,103),(21,50)],outline if False else c[2]);d.polygon([(64,16),(99,50),(64,95),(29,50)],fill=c[0]);oval(d,(54,41,75,67),c[4]);oval(d,(61,44,69,64),c[0])
 # Shading unifies the kit and keeps back-wall contrast below contact surfaces.
 save(im,f'bay_{i}')
im=Image.new('RGBA',(32,32));gear(ImageDraw.Draw(im),16,16,15,cols(3));save(im,'pulley')
manifest={'scope':'26 full-stage material/architecture profiles; 104 original textures. No screenshot IDs, frame coordinates or Nintendo files are input.','profiles':[{'id':i,'name':v[0],'material':v[1],'palette':v[2]} for i,v in enumerate(PROFILES)],'assets':[{'file':f.name,'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'size':list(Image.open(f).size)} for f in sorted(OUT.glob('*.png'))]}
(ROOT/'Documentation/BroadVisual/ART_MANIFEST.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print('BROAD_ART_KITS_EXPORTED',len(manifest['assets']))
