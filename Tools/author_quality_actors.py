"""Q11 original fixed-cell character poses, offline. No ROM art or runtime model."""
from pathlib import Path
import sys,math,json,hashlib,uuid,ast
ROOT=Path(__file__).resolve().parents[1]
sys.path[:0]=[str(ROOT/'.visual-tools'),r'C:\Users\micha\Projects\GloomBeanUnity\.visual-tools']
from PIL import Image,ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/QualityBar';OUT.mkdir(parents=True,exist_ok=True)
N=Image.Resampling.NEAREST
C={'ink':'#151427','deep':'#2b243a','cloth':'#584358','fold':'#856477','rim':'#bc8b93','skinD':'#86676d','skin':'#c39c92','cream':'#f1d1b0','white':'#fff0cb','goldD':'#77614d','gold':'#c09960','goldH':'#efd18e','pink':'#b93e6c','hot':'#f06b91','steel':'#7899a4','steelH':'#bfd0c9','green':'#83965b','greenD':'#455d4e','leaf':'#b6c788'}
def co(c):return C.get(c,c)
def line(d,p,c,w=1):d.line(p,fill=co(c),width=w)
def poly(d,p,c):d.polygon(p,fill=co(c))
def rect(d,b,c):d.rectangle(b,fill=co(c))
def ell(d,b,c):d.ellipse(b,fill=co(c))
def curve(d,p,c,w=1):
 a,b,cc,e=p;xy=[]
 for i in range(25):
  t=i/24;u=1-t;xy.append((round(u**3*a[0]+3*u*u*t*b[0]+3*u*t*t*cc[0]+t**3*e[0]),round(u**3*a[1]+3*u*u*t*b[1]+3*u*t*t*cc[1]+t**3*e[1])))
 line(d,xy,c,w)
def limb(d,p,w=5):
 curve(d,p,'ink',w+3);curve(d,p,'skinD',w);curve(d,[(x-1,y-1) for x,y in p],'skin',max(1,w-2));curve(d,[(x-1,y-2) for x,y in p],'cream',1)
def hand(d,x,y,point=False):
 poly(d,[(x-3,y-2),(x+2,y-3),(x+4,y),(x+2,y+3),(x-3,y+3)],'ink');poly(d,[(x-2,y-1),(x+2,y-2),(x+2,y+2),(x-2,y+2)],'cream')
 for i in range(3):line(d,[(x-2+i*2,y+1),(x-2+i*2+(2 if point and i==1 else 0),y+5+(3 if point and i==1 else 0))],'cream');line(d,[(x-1+i*2,y+2),(x-1+i*2,y+4)],'skinD')
def eye(d,x,y,r=4,look=0,closed=False):
 ell(d,(x-r-1,y-r-2,x+r+1,y+r+2),'ink')
 if closed:line(d,[(x-r,y),(x,y+1),(x+r,y)],'goldH',2);return
 ell(d,(x-r,y-r-1,x+r,y+r+1),'gold');ell(d,(x-r+1,y-r,x+r-1,y+r),'white');ell(d,(x-1+look,y-r+1,x+1+look,y+r-1),'ink')
def head(im,x,y,w=12,h=14,tilt=0,scream=False):
 a=Image.new('RGBA',(w+8,h+10));d=ImageDraw.Draw(a);cx=(w+8)//2
 poly(d,[(3,6),(7,1),(w+2,3),(w+6,8),(w+3,h+2),(cx,h+8),(3,h+1)],'ink')
 poly(d,[(5,6),(8,3),(w+1,5),(w+3,8),(w+1,h),(cx,h+5),(5,h)],'skinD')
 poly(d,[(6,6),(9,4),(cx+2,5),(cx,h+3),(6,h)],'cream');poly(d,[(cx+2,5),(w+1,6),(w+2,h),(cx,h+3)],'skin')
 ey=h//2;line(d,[(6,ey),(cx-1,ey+1)],'ink',2);line(d,[(cx+3,ey),(w+1,ey-1)],'ink',2);line(d,[(cx,ey),(cx-1,ey+5),(cx+2,ey+4)],'skinD')
 if scream:ell(d,(cx-2,h-1,cx+3,h+4),'ink');line(d,[(cx-1,h),(cx+1,h)],'white')
 else:line(d,[(cx-3,h+1),(cx+2,h+2)],'deep')
 if tilt:a=a.rotate(tilt,resample=N,expand=True)
 im.alpha_composite(a,(int(x-a.width/2),int(y-a.height/2)))
def boss(world,phase,pose,f):
 im=Image.new('RGBA',(64,64));d=ImageDraw.Draw(im);a=f*math.pi/4
 blink=pose==0 and f==6;breath=1 if f in (2,3,4) else 0;strain=pose==2;look=-1 if f<4 else 1
 if world==1:
  poly(d,[(24,23),(40,23),(45,37),(59,61),(42,58),(33,53),(22,60),(5,61),(17,38)],'ink');poly(d,[(25,27),(38,27),(40,39),(51,57),(39,54),(32,48),(24,55),(13,57),(21,39)],'cloth')
  for p in [[(24,30),(18,49),(14,56)],[(28,34),(25,47),(20,54)],[(38,31),(42,47),(49,56)],[(35,34),(36,49),(40,53)]]:line(d,p,'fold',2)
  for p in [[(23,34),(19,48)],[(39,35),(44,50)],[(27,39),(27,48)]]:line(d,p,'rim')
  poly(d,[(24,25),(31,30),(38,25),(35,36),(29,37)],'goldD');poly(d,[(26,25),(31,29),(35,25),(32,35)],'goldH')
  ly=36-(4 if pose==1 else 0)+breath;ry=40-(10 if pose==1 else 0)
  limb(d,[(22,30),(11,28),(5,38),(10,ly)],3);hand(d,9,ly,pose==1);limb(d,[(42,30),(48,33),(55,24 if pose==1 else 42),(55,ry)],3);hand(d,54,ry,pose==1)
  ell(d,(18,9+breath,44,32+breath),'ink');ell(d,(20,11+breath,42,30+breath),'skinD');poly(d,[(22,12+breath),(33,12+breath),(32,29+breath),(25,28+breath),(21,23+breath)],'cream')
  eye(d,27,19+breath,3,look,blink or strain);eye(d,37,18+breath,2,look,blink);poly(d,[(31,19+breath),(30,24+breath),(35,23+breath)],'goldD');line(d,[(24,25+breath),(31,28+breath),(39,24+breath)],'ink',2)
  for x,y in [(26,26),(29,27),(33,27),(36,25)]:line(d,[(x,y+breath),(x,y+1+breath)],'white')
  poly(d,[(15,12+breath),(48,10+breath),(46,14+breath),(16,16+breath)],'ink');poly(d,[(24,10),(24,1),(40,2),(43,10)],'ink');rect(d,(26,2,38,9),'deep');line(d,[(27,3),(37,3)],'fold');line(d,[(26,8),(40,8)],'gold',2)
  for k in range(3-phase):ell(d,(30,39+k*4,32,41+k*4),'goldH')
 elif world==2:
  poly(d,[(27,8),(37,8),(41,18),(45,26),(49,44),(43,57),(20,58),(13,48),(16,30),(23,18)],'ink');poly(d,[(27,11),(35,11),(38,21),(42,29),(44,44),(40,52),(23,53),(18,46),(20,30),(26,20)],'greenD');poly(d,[(27,13),(32,12),(31,48),(24,51),(20,44),(24,28)],'green');line(d,[(27,15),(24,30),(23,40)],'leaf',2)
  for x,y in [(24,35),(27,44),(38,40),(33,48),(39,28)]:ell(d,(x,y,x+2,y+3),'goldD');d.point((x,y),fill=co('goldH'))
  poly(d,[(18,35),(28,39),(34,36),(45,33),(43,52),(33,59),(20,53)],'deep');poly(d,[(20,38),(27,41),(31,39),(31,55),(22,51)],'cloth');line(d,[(19,35),(29,40),(42,35)],'gold',2)
  head(im,32,24+breath,12,12,-4,strain);d=ImageDraw.Draw(im);rect(d,(21,9,42,12),'ink');rect(d,(23,9,40,10),'gold')
  for x,y in [(24,5),(31,2),(39,4)]:poly(d,[(x-2,10),(x-1,y),(x+1,y),(x+3,10)],'gold');d.point((x,y),fill=co('goldH'))
  tilt=(f%4-1)*.25+(2 if phase==1 else 0);limb(d,[(19,27),(9,23),(8,27),(9,34)],3);limb(d,[(44,27),(51,24),(55,24),(55,33)],3)
  for x,s in [(7,1),(55,-1)]:
   yy=36+int(tilt*s);line(d,[(x,27),(x,yy+6)],'gold');line(d,[(x-6,yy),(x+6,yy)],'ink',2);line(d,[(x-5,yy),(x,yy+5),(x+5,yy)],'gold');line(d,[(x-5,yy),(x+5,yy)],'goldH');hand(d,x,28)
  for x in (22,38):poly(d,[(x,53),(x-3,59),(x+7,61),(x+9,58)],'ink');line(d,[(x,58),(x+6,59)],'gold')
 elif world==3:
  for x in (13,50):line(d,[(x,4),(x,55)],'ink',4);line(d,[(x-1,5),(x-1,53)],'gold')
  poly(d,[(26,25),(37,25),(40,40),(50,61),(39,60),(31,44),(24,61),(13,61),(23,41)],'ink');poly(d,[(26,28),(31,32),(28,42),(22,57),(18,59),(26,38)],'steel');poly(d,[(34,28),(38,29),(38,40),(45,58),(41,58),(33,42)],'cloth');line(d,[(26,30),(26,42),(20,55)],'steelH')
  for pts in [[(25,29),(16,26),(10,31),(10,37)],[(39,29),(49,27),(52,22 if pose==1 else 32),(53,28 if pose==1 else 40)]]:limb(d,pts,3)
  poly(d,[(20,6),(38,2),(47,11),(45,27),(35,34),(21,25),(18,15)],'ink');poly(d,[(22,8),(36,5),(35,29),(25,24),(21,16)],'steelH');poly(d,[(37,6),(44,12),(42,24),(36,29)],'steel');line(d,[(22,8),(36,5),(43,12)],'white')
  eye(d,27,15+breath,3,look,blink or strain);eye(d,38,14+breath,2,look,blink);poly(d,[(32,14),(32,23),(38,21)],'goldD');line(d,[(27,25),(33,27),(38,24)],'ink');line(d,[(18,6),(43,3)],'ink',3);line(d,[(21,4),(39,1)],'gold')
  ell(d,(27,35,37,45),'ink');ell(d,(29,37,35,43),'gold');line(d,[(32,37),(32,43)],'steelH');line(d,[(29,40),(35,40)],'steelH')
  for k in range(phase+1):line(d,[(40-k*7,7),(36-k*5,16),(40-k*4,21)],'pink')
 elif world==4:
  poly(d,[(11,14),(22,5),(35,3),(48,11),(52,19),(61,30),(58,47),(47,56),(28,62),(13,56),(4,40),(2,25)],'ink');poly(d,[(14,17),(24,10),(34,8),(44,14),(47,22),(56,32),(52,47),(39,54),(26,57),(14,49),(8,30)],'deep')
  for p in [[(12,25),(22,18),(37,32),(44,48),(34,43),(26,35),(15,40)],[(34,17),(44,14),(52,23),(46,31),(56,43),(51,51),(36,35)],[(10,41),(25,33),(32,40),(42,53),(35,59),(23,51)]]:poly(d,p,'cloth')
  for p in [[(15,25),(21,20),(31,32),(38,36)],[(12,41),(18,36),(23,39),(20,46)],[(30,45),(38,50),(38,55)],[(43,21),(48,25),(44,30)]]:line(d,p,'fold',2);line(d,p[:2],'rim')
  pull=int(round(math.sin(a)*(2 if pose==1 else 1)));limb(d,[(16,25),(1,21),(1,39),(13,40+pull)],5);hand(d,12,37+pull);limb(d,[(42,28),(61,10),(60,18),(57,31-pull)],4);hand(d,55,29-pull);limb(d,[(24,37),(18,50),(41,64),(50,50)],5);hand(d,46,49,phase>0);limb(d,[(37,32),(46,36),(35,45),(24,43)],4);hand(d,23,41)
  for x,y,w,h,tilt in [(18,16,9,12,-17),(39,13,8,11,16),(31,31+breath,12,15,-10),(50,41,8,11,19),(17,48,8,10,-27)]:head(im,x,y,w,h,tilt,strain or (y>25 and phase>0))
  d=ImageDraw.Draw(im)
  for k in range(phase+1):line(d,[(12+k*15,33),(9+k*15,40),(13+k*15,46)],'pink')
 else:
  for k,p in enumerate([[(25,29),(0,24),(4,4),(13,11)],[(27,21),(14,1),(36,-3),(40,8)],[(44,24),(61,5),(67,26),(55,32)],[(45,39),(69,38),(59,63),(49,55)],[(23,43),(0,60),(-1,38),(10,35)]]):
   ex,ey=p[-1];p=p[:-1]+[(ex,ey+int(math.sin(a+k)*1.5))];curve(d,p,'ink',6);curve(d,p,'skinD',3);curve(d,[(x-1,y-1) for x,y in p],'cream',1)
  poly(d,[(17,19),(25,9),(39,9),(50,20),(55,32),(49,48),(43,55),(29,58),(17,47),(12,32)],'ink');poly(d,[(20,22),(26,13),(38,13),(46,22),(50,32),(45,46),(39,51),(29,53),(21,45),(17,32)],'cloth');poly(d,[(23,21),(27,15),(38,14),(44,19),(40,24),(30,25)],'fold');line(d,[(24,19),(30,16),(38,16)],'rim',2)
  eye(d,27,31+breath,8,look,blink or strain);eye(d,42,21,5,look,blink);poly(d,[(18,22),(25,19),(33,21),(37,28),(27,25)],'ink');line(d,[(22,20),(31,22)],'rim')
  mouth=2 if pose==1 else 0;poly(d,[(29,43),(40,32),(49,35),(49,44+mouth),(40,52+mouth),(31,49)],'ink');poly(d,[(33,44),(40,37),(46,38),(44,46),(39,49+mouth)],'pink')
  for x,y in [(33,39),(40,35),(45,36)]:poly(d,[(x-2,y),(x+2,y),(x,y+6)],'white')
  for x,y in [(36,49+mouth),(42,47),(46,44)]:poly(d,[(x-1,y),(x+2,y),(x,y-4)],'cream')
  tip=int(round(math.sin(a)*3));curve(d,[(39,49),(49,60),(25+tip,61),(32+tip,54)],'ink',7);curve(d,[(39,49),(48,59),(27+tip,60),(32+tip,54)],'pink',4);curve(d,[(39,49),(44,56),(28+tip,59),(32+tip,54)],'hot')
  for x,y in [(13,11),(40,7),(55,32),(49,55),(10,35)]:ell(d,(x-3,y-3,x+3,y+3),'ink');ell(d,(x-2,y-2,x+2,y+2),'gold');line(d,[(x,y-1),(x,y+1)],'ink')
  for k in range(phase):line(d,[(18+k*22,37-k*13),(16+k*22,43-k*12),(20+k*21,46-k*12)],'hot')
 if strain:
  for x,y in [(10,12),(52,8)]:line(d,[(x,y),(x-2,y-4)],'goldH')
 return im.resize((128,128),N)
def enemy(world,state,f,armor):
 im=Image.new('RGBA',(32,32));d=ImageDraw.Draw(im);a=f*math.pi/4;step=int(round(math.sin(a)*2)) if state==1 else 0;bob=1 if state==1 and f%4 in(1,2) else 0
 stunned=state==2;carried=state==3;thrown=state==4;coat=['cloth','greenD','steel','cloth','goldD'][world-1]
 for x,dy in [(10,step),(22,-step)]:
  line(d,[(x,22+bob),(x+dy,27)],'ink',4);line(d,[(x,23+bob),(x+dy,27)],'skinD',2);poly(d,[(x+dy-4,27),(x+dy+2,26),(x+dy+4,29),(x+dy+3,30),(x+dy-4,30)],'ink');line(d,[(x+dy-3,28),(x+dy+1,27),(x+dy+3,29)],'fold')
 poly(d,[(9,13+bob),(22,13+bob),(26,24),(21,27),(9,26),(6,23)],'ink');poly(d,[(10,15+bob),(21,15+bob),(23,23),(20,25),(10,24),(8,22)],coat);line(d,[(10,17+bob),(9,22),(13,24)],'rim' if world!=2 else 'green');line(d,[(15,17),(15,23)],'goldD')
 if world==1:
  poly(d,[(8,8+bob),(11,3+bob),(22,4+bob),(26,10+bob),(23,20+bob),(13,22+bob),(7,17+bob)],'ink');poly(d,[(10,8+bob),(12,5+bob),(21,6+bob),(23,10+bob),(21,18+bob),(14,20+bob),(10,17+bob)],'skin');poly(d,[(11,8+bob),(13,6+bob),(17,6+bob),(15,18+bob),(12,17+bob)],'cream');rect(d,(5,5+bob,27,7+bob),'ink');rect(d,(10,1+bob,21,5+bob),'deep');line(d,[(11,4+bob),(22,4+bob)],'gold')
 elif world==2:
  ell(d,(6,5+bob,25,23+bob),'ink');ell(d,(8,6+bob,23,21+bob),'greenD');ell(d,(9,7+bob,20,20+bob),'green');line(d,[(11,7+bob),(11,14+bob)],'leaf');line(d,[(15,7),(17,2),(20,1)],'goldD',2);poly(d,[(17,4),(12,1),(9,2),(14,5)],'leaf')
  for x,y in [(9,15),(20,18),(11,19)]:ell(d,(x,y,x+2,y+2),'goldD')
 elif world==3:
  poly(d,[(6,7+bob),(11,3+bob),(24,4+bob),(27,10+bob),(23,21+bob),(11,20+bob),(6,15+bob)],'ink');poly(d,[(9,7+bob),(13,5+bob),(22,6+bob),(24,11+bob),(20,18+bob),(11,17+bob)],'steel');line(d,[(10,7+bob),(13,5+bob),(22,6+bob)],'steelH');rect(d,(5,3,27,5),'ink');rect(d,(10,0,22,3),'deep');line(d,[(11,1),(21,1)],'gold')
 elif world==4:
  poly(d,[(5,20+bob),(7,7+bob),(15,1+bob),(24,6+bob),(28,23+bob),(19,26+bob),(8,24+bob)],'ink');poly(d,[(8,19+bob),(9,9+bob),(15,4+bob),(22,8+bob),(24,21+bob),(18,24+bob)],'cloth');line(d,[(9,10+bob),(15,5+bob),(20,8+bob)],'rim');ell(d,(11,8+bob,22,20+bob),'skinD');poly(d,[(12,9+bob),(16,8+bob),(17,19+bob),(12,17+bob)],'cream')
 else:
  ell(d,(6,4+bob,26,24+bob),'ink');ell(d,(8,6+bob,24,22+bob),'gold');ell(d,(10,7+bob,22,20+bob),'cream');line(d,[(5,3),(15,1),(27,3)],'goldH');line(d,[(5,4),(15,3),(27,4)],'goldD')
 y=12+bob
 if stunned or carried:
  for x in (13,21):line(d,[(x-2,y-1),(x+2,y+2)],'ink');line(d,[(x+2,y-1),(x-2,y+2)],'ink')
 else:
  for x in (13,21):ell(d,(x-2,y-2,x+2,y+3),'ink');line(d,[(x,y-1),(x,y+2)],'goldH' if world==4 else 'white')
 line(d,[(15,18+bob),(19,18+bob)],'ink')
 if armor:
  poly(d,[(5,9+bob),(8,3+bob),(22,2+bob),(28,8+bob),(25,11+bob),(7,11+bob)],'ink');poly(d,[(8,8+bob),(10,4+bob),(21,3+bob),(25,8+bob)],'steel');line(d,[(10,4+bob),(21,3+bob),(24,6+bob)],'steelH');line(d,[(17,3+bob),(17,10+bob)],'gold');d.point((8,8+bob),fill=co('white'));d.point((23,8+bob),fill=co('white'))
 hy=8 if carried else 18+(step if state==1 else 0)
 for x in(4,28):line(d,[(8 if x==4 else 24,18+bob),(x,hy)],'ink',3);ell(d,(x-2,hy-2,x+2,hy+2),'ink');rect(d,(x-1,hy-1,x+1,hy+1),'cream')
 if stunned:
  for k in range(2):x=3+k*24;y=3+(f+k)%3;line(d,[(x-1,y),(x+1,y)],'goldH');line(d,[(x,y-1),(x,y+1)],'goldH')
 if thrown:im=im.rotate((f%4)*90,resample=N,expand=False)
 return im
def save(im,name):
 f=OUT/(name+'.png');im.save(f,optimize=True);m=OUT/(name+'.png.meta')
 if not m.exists():m.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/q11/'+name).hex+'\n',encoding='utf-8')
for w in range(1,6):
 sheet=Image.new('RGBA',(1024,1152))
 for phase in range(3):
  for pose in range(3):
   for f in range(8):sheet.alpha_composite(boss(w,phase,pose,f),(f*128,(phase*3+pose)*128))
 save(sheet,'actor_boss_'+str(w))
 for armor in (0,1):
  sheet=Image.new('RGBA',(256,160))
  for state in range(5):
   for f in range(8):sheet.alpha_composite(enemy(w,state,f,armor),(f*32,state*32))
  save(sheet,f'actor_patrol_{w}_{armor}')
manifest=[{'name':f.name,'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'size':list(Image.open(f).size)} for f in sorted(OUT.glob('actor_*.png'))]
f=ROOT/'Documentation/QualityBarQ1/Q11_ASSETS.json';f.parent.mkdir(parents=True,exist_ok=True);f.write_text(json.dumps({'method':'Original offline authored pixels; layered fixed-cell poses, no source-game art or generative screenshot content','actors':manifest,'runtime_contract':'5 boss sheets:8 frames x9 rows,128px cells;10 patrol sheets:8 frames x5 rows,32px cells'},indent=2)+'\n',encoding='utf-8')
print('Q11_ACTOR_ASSETS_PASS',len(manifest))
