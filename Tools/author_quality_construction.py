"""Original stepped-pixel construction kit; no gameplay data or review camera coordinates.
Render-only modules, hardware and hanging materials for the existing world geometry.
"""
from pathlib import Path
import sys, math, json, hashlib, uuid
ROOT=Path(__file__).resolve().parents[1]
for p in [ROOT/'.visual-tools', ROOT.parent/'GloomBeanUnity/.visual-tools']:
    sys.path.insert(0,str(p))
from PIL import Image, ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/QualityBar';OUT.mkdir(parents=True,exist_ok=True)
P=['11111e','232036','403049','604355','966673','c49191','eed3b1','ffedbb',
   '514458','89738b','b7a2ad','d0a366','8c654a','c88851','5a747c','87a6a0',
   '344643','576850','91a06a','cb596f','913f60','38325d','665589','9685b0']
C=['#'+x for x in P]
manifest=[]
def canvas(w,h):
 im=Image.new('RGBA',(w,h));return im,ImageDraw.Draw(im)
def box(d,b,k):d.rectangle(b,fill=C[k])
def line(d,p,k,w=1):d.line(p,fill=C[k],width=w)
def poly(d,p,k):d.polygon(p,fill=C[k])
def ell(d,b,k):d.ellipse(b,fill=C[k])
def rimbox(d,b,mid=3,rim=5):
 x,y,xx,yy=b;box(d,b,0);box(d,(x+1,y+1,xx-1,yy-1),mid);line(d,[(x+2,yy-2),(x+2,y+2),(xx-2,y+2)],rim);line(d,[(xx-2,y+3),(xx-2,yy-2),(x+3,yy-2)],1)
def nail(d,x,y):box(d,(x-1,y-1,x+1,y+1),0);box(d,(x-1,y-1,x,y),11);d.point((x-1,y-1),fill=C[6])
def planks(d,b,vertical=False,bank=0):
 x,y,xx,yy=b;box(d,b,1)
 for n in range(x if vertical else y,xx+1 if vertical else yy+1,8):
  if vertical:
   box(d,(n,y,min(n+6,xx),yy),3);line(d,[(n+1,y),(n+1,yy)],4);line(d,[(n+5,y),(n+5,yy)],2)
   for k in range(y+5,yy-4,19):line(d,[(n+3,k),(n+4,k+4),(n+3,k+8)],2)
  else:
   box(d,(x,n,xx,min(n+6,yy)),3);line(d,[(x,n+1),(xx,n+1)],4);line(d,[(x,n+5),(xx,n+5)],2)
   for k in range(x+7,xx-5,29):line(d,[(k,n+3),(k+6,n+4),(k+9,n+3)],2)
def masonry(d,b,ivory=False):
 x,y,xx,yy=b;box(d,b,1)
 for r,by in enumerate(range(y,yy+1,10)):
  for bx in range(x-(r%2)*12,xx+1,24):
   l=max(x,bx);right=min(xx,bx+22);bottom=min(yy,by+8)
   if l>right:continue
   box(d,(l,by,right,bottom),8 if ivory else 2);line(d,[(l+1,by+1),(right-1,by+1)],10 if ivory else 3)
   if bx>x:line(d,[(l+2,bottom-2),(l+4,bottom-2)],9 if ivory else 1)
def arch(d,b,ivory=False,glass=False):
 x,y,xx,yy=b;w=xx-x;apex=y;cy=y+w//2
 poly(d,[(x,yy),(x,cy),(x+3,cy-7),(x+w//2,apex),(xx-3,cy-7),(xx,cy),(xx,yy)],0)
 poly(d,[(x+2,yy),(x+2,cy),(x+5,cy-6),(x+w//2,y+3),(xx-5,cy-6),(xx-2,cy),(xx-2,yy)],11 if ivory else 4)
 poly(d,[(x+5,yy),(x+5,cy+1),(x+7,cy-4),(x+w//2,y+7),(xx-7,cy-4),(xx-5,cy+1),(xx-5,yy)],1)
 if glass:
  poly(d,[(x+7,yy-3),(x+7,cy+2),(x+9,cy-2),(x+w//2,y+10),(xx-9,cy-2),(xx-7,cy+2),(xx-7,yy-3)],13)
  line(d,[(x+w//2,y+10),(x+w//2,yy-3)],7,2)
  for gy in range(cy+6,yy-3,12):
   line(d,[(x+7,gy),(xx-7,gy)],12,2);line(d,[(x+7,gy),(x+w//2,gy-7),(xx-7,gy)],11)
  for gx in [x+10,xx-10]:line(d,[(gx,cy+5),(gx,yy-4)],11)
 line(d,[(x+2,yy),(x+2,cy),(x+w//2,y+3)],6 if ivory else 5)
 for ny in range(cy+5,yy,14):line(d,[(x,ny),(x+4,ny)],0);line(d,[(xx-4,ny),(xx,ny)],0)
def pipe(d,pts,w=8,bank=14):
 line(d,pts,0,w+3);line(d,pts,8,w);line(d,[(x-2,y-1) for x,y in pts],bank,max(1,w//2));line(d,[(x-2,y-2) for x,y in pts],15 if bank==14 else 11,1)
 for x,y in pts[1:-1]:rimbox(d,(x-5,y-4,x+5,y+4),8,9);nail(d,x,y)
def gear(d,x,y,r=10):
 for a in range(12):
  t=a*math.pi/6
  poly(d,[(x+int(math.cos(t)*r),y+int(math.sin(t)*r)),(x+int(math.cos(t+.1)*(r+3)),y+int(math.sin(t+.1)*(r+3))),
  (x+int(math.cos(t+.3)*(r+3)),y+int(math.sin(t+.3)*(r+3))),(x+int(math.cos(t+.4)*r),y+int(math.sin(t+.4)*r))],0)
 ell(d,(x-r,y-r,x+r,y+r),0);ell(d,(x-r+2,y-r+2,x+r-2,y+r-2),8);d.arc((x-r+2,y-r+2,x+r-2,y+r-2),180,315,fill=C[11],width=2)
 ell(d,(x-r+5,y-r+5,x+r-5,y+r-5),1)
 for a in range(6):
  t=a*math.pi/3;line(d,[(x,y),(x+int(math.cos(t)*(r-3)),y+int(math.sin(t)*(r-3)))],12,2)
 ell(d,(x-4,y-4,x+4,y+4),0);ell(d,(x-2,y-2,x+2,y+2),11);d.point((x-1,y-1),fill=C[7])
def curtain(d,x,y,w,h,k=4):
 poly(d,[(x,y),(x+w,y),(x+w-2,y+h),(x+w*.7,y+h-4),(x+w*.5,y+h+2),(x+w*.3,y+h-3),(x,y+h)],0)
 for a in range(x+2,x+w-1,5):
  yy=y+h-2+(a//5)%3;poly(d,[(a,y+1),(a+4,y+1),(a+3,yy),(a,yy-3)],k);line(d,[(a,y+3),(a,yy-4)],5 if k==4 else 3)
 for a in range(x+3,x+w-1,7):nail(d,a,y+2)
def cabinet(d,x,y,w,h,kind=0):
 rimbox(d,(x,y,x+w,y+h),3,11);box(d,(x+3,y+4,x+w-3,y+h-4),1)
 for by in range(y+6,y+h-3,12):
  if kind==1:
   for bx in range(x+5,x+w-4,6):
    hh=6+(bx//6)%3;box(d,(bx,by,bx+4,by+hh),[12,4,21,14][(bx//6+by)%4]);line(d,[(bx+1,by+2),(bx+3,by+2)],11);line(d,[(bx+1,by+hh-1),(bx+3,by+hh-1)],11)
  else:
   rimbox(d,(x+4,by,x+w-4,min(by+9,y+h-4)),8,9);ell(d,(x+w//2-2,by+3,x+w//2+2,by+5),11)
  line(d,[(x+2,by+10),(x+w-2,by+10)],0)
def lamp(d,x,y):
 line(d,[(x,y-14),(x,y-5)],8,2);ell(d,(x-3,y-7,x+3,y-2),0)
 poly(d,[(x-5,y-3),(x+5,y-3),(x+9,y+3),(x-9,y+3)],0);line(d,[(x-8,y+2),(x+8,y+2)],11);ell(d,(x-3,y+3,x+3,y+8),11);ell(d,(x-1,y+3,x+1,y+6),7)
def face(d,x,y,r=5):
 ell(d,(x-r-1,y-r-2,x+r+1,y+r+2),0);ell(d,(x-r,y-r,x+r,y+r),5);line(d,[(x-2,y-2),(x-2,y)],0);line(d,[(x+2,y-2),(x+2,y)],0);ell(d,(x-1,y+2,x+1,y+3),1)
def save(im,name):
 f=OUT/(name+'.png');im.save(f,optimize=True);meta=Path(str(f)+'.meta')
 if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/Q9-construction/'+name).hex+'\n',encoding='utf-8')
 manifest.append({'file':f.name,'size':list(im.size),'sha256':hashlib.sha256(f.read_bytes()).hexdigest()})

for kind in range(12):
 im,d=canvas(128,112)
 masonry(d,(3,3,124,111),kind>=9)
 # Layered jambs, cornice, wall kick plates and bolt-capped supports.
 for x in (2,119):
  rimbox(d,(x,0,x+6,111),8,9);rimbox(d,(x-1,8,x+7,13),12,11);rimbox(d,(x-1,96,x+7,101),12,11)
  for y in (5,18,45,72,104):nail(d,x+3,y)
 rimbox(d,(0,0,127,7),8,11);rimbox(d,(1,101,126,111),8,9)
 if kind==0:
  arch(d,(21,10,105,100),True);curtain(d,16,12,23,77,19);curtain(d,88,12,23,77,19)
  for x in [47,64,82]:
   face(d,x,63,5);line(d,[(x,70),(x,85)],11);poly(d,[(x,71),(x-8,90),(x+8,90)],4)
  line(d,[(31,18),(96,18)],11);face(d,64,25,9)
 elif kind in (1,2,3):
  planks(d,(11,10,114,96),True)
  pipe(d,[(18,15),(18,88),(52,88)],6);pipe(d,[(107,14),(79,14),(79,47)],6,12)
  if kind==1:
   rimbox(d,(41,18,85,26),12,11);line(d,[(63,22),(63,39)],0,5)
   poly(d,[(51,36),(46,56),(38,62),(40,68),(87,68),(89,61),(81,56),(76,36)],0)
   poly(d,[(54,38),(50,57),(43,61),(85,61),(76,56),(73,38)],12)
   poly(d,[(56,39),(54,56),(48,60),(62,60),(62,39)],11);line(d,[(45,65),(84,65)],6,2);ell(d,(60,65,68,75),0);ell(d,(61,66,65,73),11)
   gear(d,30,32,10);line(d,[(31,45),(31,93)],11)
  elif kind==2:
   for x in [37,88]:
    rimbox(d,(x-19,44,x+19,94),8,9);ell(d,(x-17,49,x+17,86),0);ell(d,(x-14,51,x+14,83),12);ell(d,(x-11,54,x+11,80),1);d.arc((x-10,55,x+10,79),185,285,fill=C[15],width=2)
    for k in range(3):poly(d,[(x-8+k*4,71-k*3),(x+k*4,64),(x+5,75+k),(x-3,78)],5)
    for dx,dy in [(-13,-1),(12,0),(-13,35),(12,35)]:nail(d,x+dx,53+dy)
    rimbox(d,(x-14,89,x+14,93),1,8)
   line(d,[(29,18),(97,18)],0,4);line(d,[(29,18),(97,18)],11)
   for x in [37,65,87]:curtain(d,x-8,23,16,24,4 if x==65 else 9)
  else:
   cabinet(d,20,44,32,53);curtain(d,60,17,40,65,5);line(d,[(57,14),(108,14)],11,2)
   for y in range(23,74,7):line(d,[(75,y),(80,y+4)],1);d.point((77,y+1),fill=C[7])
   gear(d,98,91,8)
  lamp(d,64,10)
 elif kind in (4,5):
  planks(d,(11,10,114,100));
  for x in [19,105]:
   poly(d,[(x-6,108),(x-2,51),(x-7,17),(x-1,11),(x+4,47),(x+7,106)],0);line(d,[(x,103),(x+1,52),(x-4,17)],16,5);line(d,[(x-1,99),(x-1,50)],18)
  if kind==4:
   for by in [14,38,62]:
    line(d,[(26,by),(99,by)],1,5);line(d,[(26,by),(99,by)],12,2)
    for x in range(30,100,17):
     line(d,[(x,by),(x,by+9)],0);ell(d,(x-5,by+6,x+6,by+20),0);ell(d,(x-4,by+7,x+4,by+17),17);line(d,[(x-2,by+8),(x-3,by+12)],18);d.point((x+1,by+12),fill=C[0])
   cabinet(d,37,83,50,16,1)
  else:
   arch(d,(30,28,98,101));poly(d,[(37,95),(39,65),(49,47),(64,40),(83,54),(91,72),(93,96)],0)
   for k in range(8):poly(d,[(40+k*6,95),(42+k*6,77-(k%3)*8),(46+k*6,94)],13 if k%2 else 19)
   rimbox(d,(27,95,100,101),8,11)
   pipe(d,[(62,29),(62,8),(93,8)],10,12);cabinet(d,15,63,14,30)
 elif kind in (6,7,8):
  # Readable inhabited interiors: mirror landing, plumbing room, chained archive.
  arch(d,(17,9,65,71),False,kind==6);curtain(d,68,13,39,50,21)
  planks(d,(11,84,115,100));line(d,[(11,86),(114,86)],9)
  if kind==6:
   rimbox(d,(75,51,108,97),12,11);box(d,(78,55,105,89),21)
   poly(d,[(79,56),(89,56),(104,76),(104,86)],23);line(d,[(82,86),(100,66)],10)
   rimbox(d,(15,71,55,78),8,9);line(d,[(19,78),(19,95)],8,3);line(d,[(50,78),(50,95)],8,3)
   face(d,42,50,5);line(d,[(42,57),(42,66)],12)
   # stairs to a landing are low-contrast back-wall cues, not solid gameplay steps.
   for n in range(5):line(d,[(17+n*7,96-n*5),(34+n*7,96-n*5)],8,3)
  elif kind==7:
   pipe(d,[(21,39),(21,83),(103,83),(103,19)],6)
   poly(d,[(27,62),(91,62),(87,77),(76,88),(40,88),(29,78)],0);poly(d,[(31,65),(87,65),(84,77),(74,84),(43,84),(33,76)],9);line(d,[(32,66),(85,66)],6,2)
   ell(d,(57,61,67,68),14);line(d,[(58,62),(62,57),(65,61)],15)
   for x in [41,77]:line(d,[(x,86),(x-2,94)],12,3)
  else:
   cabinet(d,15,16,34,79,1);cabinet(d,70,21,40,74)
   for x in (59,65):line(d,[(x,8),(x,99)],0,3);line(d,[(x,9),(x,99)],11)
   face(d,60,45,8);line(d,[(58,51),(49,74),(67,82)],4,4)
  lamp(d,90,12)
 else:
  arch(d,(16,10,109,100),True,kind==9);pipe(d,[(113,14),(113,92)],6,12)
  if kind==9:
   # Pressure organ / counterweighted altar, visible pipes and shut-off wheels.
   for x in range(24,104,13):
    top=25+abs(62-x)//4;rimbox(d,(x,top,x+8,87),8,10);line(d,[(x+2,top+2),(x+2,84)],6)
    poly(d,[(x+1,top+6),(x+4,top+2),(x+7,top+6)],0);box(d,(x+2,74,x+6,82),1)
   rimbox(d,(17,89,110,99),12,11);gear(d,63,60,13)
  elif kind==10:
   curtain(d,17,11,29,84,21);curtain(d,81,11,29,84,21);line(d,[(20,15),(105,15)],11,2)
   face(d,65,49,10);line(d,[(65,60),(65,95)],8,5);poly(d,[(63,64),(49,94),(80,94),(68,62)],3);line(d,[(59,64),(53,89)],5)
   for x in [40,89]:ell(d,(x-5,91,x+5,94),11);line(d,[(x,87),(x,92)],6,2)
  else:
   cabinet(d,15,20,45,78,1);cabinet(d,76,17,35,81,1)
   poly(d,[(50,45),(73,43),(78,77),(56,79)],0);poly(d,[(53,47),(70,46),(75,73),(57,76)],6);line(d,[(58,51),(66,50),(70,57)],12);line(d,[(60,61),(69,61)],8);line(d,[(61,66),(70,66)],8)
   line(d,[(58,79),(54,98)],11,3)
 save(im,f'construction_room_{kind}')

# Real rail hardware art: separate housings/belt faces anchored by runtime geometry.
im,d=canvas(64,32);rimbox(d,(0,4,63,27),8,9);rimbox(d,(5,8,58,23),1,2)
for x in range(6,57,13):line(d,[(x,9),(x+12,22),(x+12,9)],12,2);line(d,[(x+1,9),(x+12,20)],11)
for x in (3,60):nail(d,x,8);nail(d,x,24)
line(d,[(1,4),(62,4)],6);line(d,[(1,27),(62,27)],0,2);save(im,'construction_girder')
im,d=canvas(48,56);rimbox(d,(4,7,43,47),8,9);rimbox(d,(12,0,34,11),12,11)
gear(d,24,29,17);rimbox(d,(17,43,31,55),12,11);nail(d,7,12);nail(d,39,12);nail(d,7,42);nail(d,39,42);save(im,'construction_winch')
im,d=canvas(48,48);rimbox(d,(3,5,44,41),1,8);gear(d,24,23,15);pipe(d,[(24,40),(24,47)],6,12);save(im,'construction_cogbox')
for kind in range(4):
 im,d=canvas(32,48);line(d,[(15,0),(15,5)],11);line(d,[(15,5),(4,12),(28,12),(15,5)],9);nail(d,15,5)
 if kind==0:
  poly(d,[(5,13),(27,13),(25,40),(20,43),(17,39),(11,46),(6,43)],0);curtain(d,7,13,18,27,5)
 elif kind==1:
  poly(d,[(6,13),(25,13),(27,23),(23,26),(22,43),(12,43),(11,26),(5,24)],0);poly(d,[(8,14),(22,14),(25,22),(20,23),(20,40),(14,40),(13,24),(8,23)],14);line(d,[(15,16),(16,36)],15)
 elif kind==2:
  poly(d,[(8,12),(23,12),(24,27),(27,43),(20,43),(16,30),(14,43),(7,43),(9,26)],0);poly(d,[(10,14),(21,14),(21,27),(24,41),(21,41),(16,26),(11,41),(9,41)],4);line(d,[(12,16),(11,29),(10,36)],5)
 else:
  face(d,16,18,7);line(d,[(16,26),(14,34),(18,44)],8,3);line(d,[(14,30),(6,36)],9,2);line(d,[(16,30),(24,34)],9,2)
 save(im,f'construction_hanging_{kind}')
# Structural arches and carved root braces, shaped supports placed behind actual floor spans.
for group in range(6):
 im,d=canvas(96,64);mid=17 if group==2 else (12 if group==5 else 8);hi=18 if group==2 else (11 if group==5 else 9)
 if group==2:
  poly(d,[(1,0),(12,0),(17,18),(45,40),(59,44),(86,62),(78,64),(48,50),(26,41),(1,11)],0)
  poly(d,[(3,1),(10,1),(14,19),(44,43),(56,46),(79,61),(49,48),(28,39),(4,10)],16);line(d,[(4,3),(14,20),(43,44),(56,47),(78,60)],17,4);line(d,[(5,3),(16,22),(44,46)],18)
  for x,y in [(20,26),(46,45),(65,52)]:line(d,[(x,y),(x+8,y-4),(x+15,y-4)],17,2)
 else:
  poly(d,[(0,0),(95,0),(95,8),(69,8),(37,29),(12,55),(12,64),(0,64)],0)
  poly(d,[(2,2),(92,2),(92,5),(67,5),(34,27),(8,55),(8,61),(3,61)],mid);line(d,[(3,2),(91,2)],hi,2);line(d,[(7,57),(33,29),(66,7)],hi,2)
  if group in (3,4,5):
   d.arc((9,8,89,86),182,272,fill=C[hi],width=2);line(d,[(15,41),(15,10),(57,10)],mid,3)
   face(d,17,18,6)
  else:
   for x,y in [(7,9),(7,46),(36,21),(74,5)]:nail(d,x,y)
 if group!=2:
  d.polygon([(13,12),(57,12),(32,29),(13,47)],fill=(0,0,0,0))
  line(d,[(13,12),(56,12),(32,28),(13,47)],hi,1)
 save(im,f'construction_brace_{group}')

# Small original enemy gait sprite bank: do not change the actors' collision model.
for armored in range(2):
 for frame in range(4):
  im,d=canvas(48,40);step=[-4,0,4,0][frame]
  for x,y in [(15+step,33),(31-step,33)]:
   poly(d,[(x-7,y-3),(x+3,y-3),(x+7,y),(x+6,y+4),(x-8,y+4)],0);box(d,(x-6,y-1,x+4,y+2),12);line(d,[(x-5,y-1),(x+2,y-1)],11)
  poly(d,[(10,31),(8,17),(14,6),(24,3),(35,8),(40,22),(36,34)],0);poly(d,[(12,29),(11,17),(16,8),(24,6),(33,11),(36,22),(34,31)],3 if not armored else 8)
  poly(d,[(14,16),(17,9),(23,7),(25,28),(18,31),(13,26)],4 if not armored else 9)
  if armored:
   rimbox(d,(11,13,37,22),8,11);box(d,(14,17,34,19),0);line(d,[(23,5),(21,12)],6,2)
  else:
   for x in [19,30]:ell(d,(x-5,13,x+4,26),0);ell(d,(x-4,14,x+2,23),11);box(d,(x-1,17,x+1,23),0);d.point((x-3,15),fill=C[7])
  poly(d,[(18,28),(31,26),(30,32),(22,32)],0);line(d,[(21,28),(23,30),(25,28)],6)
  save(im,f'construction_patrol_{armored}_{frame}')

m=ROOT/'Documentation/QualityBarQ1/Q9_CONSTRUCTION_ASSETS.json';m.parent.mkdir(parents=True,exist_ok=True)
m.write_text(json.dumps({'method':'Original offline stepped-pixel drawing; no Nintendo/third-party/concept screenshot pixels. Construction selected by material/object identity, never screenshot positions.','assets':manifest},indent=2)+'\n',encoding='utf-8')
print('QUALITY_CONSTRUCTION_EXPORT_PASS',len(manifest))
if __name__=='__main__':pass
