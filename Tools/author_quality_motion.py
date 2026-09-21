#!/usr/bin/env python3
"""Original keyed Gloom Bean motion sprites; offline authoring, not runtime physics.
64px cells, 8 columns, 16 action rows. Identity: purple bean, cream eyes,
red shoes, pale gloves; permanent open/corrupt variant is never a power-up toggle.
"""
from pathlib import Path
import argparse,json,math,hashlib,sys
try:
 from PIL import Image,ImageDraw
except ImportError:
 root=Path(__file__).resolve().parents[1]
 for path in (root/'.visual-tools',root.parent/'GloomBeanUnity/.visual-tools'):sys.path.insert(0,str(path))
 from PIL import Image,ImageDraw
C={
 'outline':'#140e25','cast':'#211632','deep':'#2d1645','shade':'#482058','body':'#632978','mid':'#864298','high':'#b363b6','edge':'#d896d0',
 'cavity':'#130f22','redshade':'#8b2357','red':'#c22d68','pink':'#ed4a89','rose':'#ff91a0',
 'goldshadow':'#b9853c','gold':'#eabb64','cream':'#fff0b8','white':'#fffbed',
 'glovedark':'#696883','gloveshade':'#a7adc0','glove':'#dfe7e2','sole':'#342b41','teeth':'#ffe2b0',
 'cyan':'#8bcbce'
}
POSES=['idle','tackle','swim','pound','hurt','walk','run','rise','fall','windup','carry','crouch','brake','swimdash','land','dead']
# Explicit gait key poses. Contacts/lift/front-back ordering are designed, not a whole-sprite bob.
WALK=[(-7,4,5,3),(-3,3,3,1),(1,1,-1,0),(5,0,-5,2),(7,3,-5,4),(3,1,-3,3),(-1,0,1,1),(-5,2,5,0)]
RUN=[(-10,4,8,-4),(-5,4,6,0),(2,3,-3,1),(9,-3,-9,5),(11,-5,-8,4),(5,0,-6,4),(-2,1,3,3),(-9,5,9,-3)]
def poly(d,pts,color):d.polygon([(round(x),round(y)) for x,y in pts],fill=C.get(color,color))
def ellipse(d,box,color):d.ellipse(tuple(round(v) for v in box),fill=C.get(color,color))
def line(d,pts,color,width=1):d.line([(round(x),round(y)) for x,y in pts],fill=C.get(color,color),width=width)
def shoe(im,x,y,back=False,angle=0):
 s=Image.new('RGBA',(22,13));d=ImageDraw.Draw(s)
 poly(d,[(1,6),(4,2),(10,1),(14,4),(19,5),(21,9),(19,12),(3,12),(0,10)],'outline')
 poly(d,[(3,6),(5,3),(10,3),(13,6),(18,6),(19,9),(4,9),(2,8)],'redshade' if back else 'red')
 poly(d,[(5,4),(10,4),(12,6),(7,6),(4,8),(3,7)],'red' if back else 'pink')
 line(d,[(6,4),(9,4),(10,5)],'pink' if back else 'rose')
 line(d,[(4,10),(18,10)],'sole');line(d,[(7,11),(19,11)],'glovedark')
 if angle:s=s.rotate(angle,Image.Resampling.NEAREST,expand=True)
 im.alpha_composite(s,(round(x-s.width/2),round(y-s.height/2)))
def glove(im,x,y,mode='fist',back=False,angle=0):
 s=Image.new('RGBA',(19,18));d=ImageDraw.Draw(s)
 if mode=='open':
  poly(d,[(2,9),(0,5),(2,3),(5,5),(4,0),(7,0),(9,4),(10,0),(13,1),(13,6),(16,3),(18,5),(17,11),(14,15),(7,17),(2,13)],'outline')
  poly(d,[(3,9),(2,5),(4,6),(7,8),(6,2),(7,2),(10,8),(11,3),(12,7),(14,10),(17,6),(15,12),(12,14),(7,15),(4,12)],'gloveshade' if back else 'glove')
  poly(d,[(6,7),(8,9),(12,8),(14,10),(12,12),(7,12),(4,10)],'glove' if back else 'white')
 else:
  poly(d,[(1,6),(3,2),(8,1),(11,2),(14,2),(17,6),(18,11),(15,15),(8,17),(3,15),(0,10)],'outline')
  poly(d,[(3,6),(5,3),(10,3),(12,4),(14,4),(15,7),(16,11),(13,13),(8,15),(4,13),(2,9)],'gloveshade' if back else 'glove')
  poly(d,[(4,6),(6,4),(10,4),(10,6),(14,6),(14,9),(11,11),(5,10),(3,8)],'glove' if back else 'white')
  line(d,[(10,11),(11,13),(14,12)],'glovedark')
  if mode=='brace':line(d,[(5,6),(6,9)],'gloveshade');line(d,[(9,5),(10,8)],'gloveshade')
 line(d,[(7,15),(12,13)],'glovedark');line(d,[(6,14),(10,13)],'gloveshade')
 if angle:s=s.rotate(angle,Image.Resampling.NEAREST,expand=True)
 im.alpha_composite(s,(round(x-s.width/2),round(y-s.height/2)))
def eye(d,x,y,rx,ry,look=2,blink=0,corrupt=False):
 ellipse(d,(x-rx-2,y-ry-2,x+rx+2,y+ry+2),'outline')
 if corrupt:ellipse(d,(x-rx-1,y-ry-1,x+rx+1,y+ry+1),'red')
 ellipse(d,(x-rx,y-ry,x+rx,y+ry),'goldshadow')
 ellipse(d,(x-rx+1,y-ry+1,x+rx-1,y+ry-1),'gold')
 ellipse(d,(x-rx+2,y-ry+2,x+rx-1,y+ry-3),'cream')
 # A single flat ink pupil; the bright glint is deliberately only two pixels.
 ellipse(d,(x+look-3,y-ry*.56,x+look+3,y+ry*.55),'cavity')
 d.rectangle((round(x+look-1),round(y-ry*.48),round(x+look),round(y-ry*.48+2)),fill=C['white'])
 if blink:
  lid=round(ry*2*blink)
  for yy in range(-ry,min(ry,lid-ry)+1):
   reach=round(rx*math.sqrt(max(0,1-(yy/ry)**2)))
   line(d,[(x-reach,y+yy),(x+reach,y+yy)],'shade')
  yy=min(ry-1,lid-ry);reach=round(rx*math.sqrt(max(0,1-(yy/ry)**2)))
  line(d,[(x-reach,y+yy),(x+reach,y+yy)],'outline',1)
def draw(corrupt,pose,index):
 im=Image.new('RGBA',(64,64));d=ImageDraw.Draw(im)
 # Local skeleton and silhouette scale, selected from the authored pose sequence.
 cx,cy=32,33;lean=0;rx,ry=18,18;back_hand=(8,39);front_hand=(56,39);left=(22,55);right=(43,55);angles=(0,0);handmode='fist';blink=0;look=2
 if pose=='idle':
  cy+=[0,0,-1,-1,0,0,0,0][index];blink=.88 if index==6 else .30 if index==5 or index==7 else 0
  front_hand=(56,39+([0,0,-1,0,0,1,0,0][index]));left=(22,55);right=(43,55)
 elif pose in ['walk','run','carry']:
  gait=RUN[index] if pose=='run' else WALK[index];a,ya,b,yb=gait
  left=(22+a,54+ya);right=(42+b,54+yb);angles=(-a*1.1,b*1.1)
  if pose=='run':cx+=3;cy-=1;lean=3;ry=17;rx=19
  cy+=[0,-1,-2,-1,0,-1,-2,-1][index] if pose=='run' else [0,0,-1,-1,0,0,-1,-1][index]
  back_hand=(9+b*.38,39-ya*.7);front_hand=(54+a*.45,38-yb)
  if pose=='carry':back_hand=(11,16);front_hand=(52,16);handmode='open';ry=17;cy=35
 elif pose=='tackle':
  lean=5;cx=31;cy=35;rx=21;ry=16;look=3;back_hand=(7,27);front_hand=(53,38);left=(19,55);right=(42,54);angles=(-18,8)
  front_hand=(53+(index%2),36);blink=.18
 elif pose in ['rise','fall']:
  cx=31;ry=19 if pose=='rise' else 18;cy=31;look=3 if pose=='rise' else 2
  back_hand=(8,22 if pose=='rise' else 27);front_hand=(55,20 if pose=='rise' else 29);handmode='open'
  left=(21,51 if pose=='rise' else 53);right=(43,49 if pose=='rise' else 53);angles=(-22,25)
 elif pose=='windup':
  cy=28;ry=17;rx=20;left=(20,43);right=(46,42);angles=(-35,35);back_hand=(11,21);front_hand=(55,20);handmode='open';blink=.16
 elif pose=='pound':
  cy=28;rx=17;ry=21;left=(23,54);right=(42,54);back_hand=(12,34);front_hand=(52,32);handmode='brace';look=1;blink=.20
 elif pose in ['crouch','land']:
  cy=39;rx=21;ry=12 if pose=='crouch' else [11,12,14,17,18,18,18,18][index];left=(21,55);right=(43,55);back_hand=(8,45);front_hand=(55,44);blink=.32 if pose=='land' and index<3 else .22
 elif pose=='brake':
  lean=-4;cx=27;ry=18;cy=33;left=(18,54);right=(46,54);angles=(-5,12);back_hand=(7,28);front_hand=(53,36);handmode='open';look=4
 elif pose in ['swim','swimdash']:
  cy=34;cx=30;rx=20;ry=15;lean=4;left=(11,48+index%3);right=(20,51-index%3);angles=(-25,-30);handmode='open';back_hand=(9,26);front_hand=(53,27+(index%4)*3);look=3
  if pose=='swimdash':rx=21;ry=14;front_hand=(55,34);back_hand=(10,37)
 elif pose in ['hurt','dead']:
  cx=29;cy=33;lean=-3;left=(17,51);right=(46,52);angles=(-30,28);back_hand=(8,25);front_hand=(55,22);handmode='open';look=-2;blink=.40
  if pose=='dead':ry=12;rx=21;cy=42;back_hand=(6,46);front_hand=(54,47)
 # Back limb and legs provide depth, not flattened identical sticker gloves.
 line(d,[(cx-11,cy+4),back_hand],'outline',5);line(d,[(cx-12,cy+3),back_hand],'shade',3)
 glove(im,*back_hand,mode=handmode,back=True,angle=-12 if handmode=='open' else 0);d=ImageDraw.Draw(im)
 line(d,[(cx-7,cy+ry-2),left],'outline',5);line(d,[(cx+7,cy+ry-2),right],'outline',5)
 shoe(im,*left,back=True,angle=angles[0]);shoe(im,*right,angle=angles[1]);d=ImageDraw.Draw(im)
 # Stem reacts via small key-pose changes, not a uniform spin of the whole character.
 stem=[(cx-7,cy-ry+3),(cx-10+lean,cy-ry-7),(cx-6+lean,cy-ry-13),(cx+lean,cy-ry-13),(cx+3+lean,cy-ry-10),(cx+lean,cy-ry-8)]
 line(d,stem,'outline',6);line(d,stem,'mid',3);line(d,stem[1:4],'high',1)
 # Irregular bean silhouette: rounded back, bowed forward cheek and a distinct underside.
 ellipse(d,(cx-rx-2,cy-ry-2,cx+rx+2,cy+ry+2),'outline')
 ellipse(d,(cx-rx,cy-ry,cx+rx,cy+ry),'shade')
 ellipse(d,(cx-rx+1,cy-ry+1,cx+rx-2,cy+ry-3),'body')
 ellipse(d,(cx-rx+2,cy-ry+2,cx+rx-7,cy+ry-9),'mid')
 ellipse(d,(cx-rx+4,cy-ry+3,cx+rx-13,cy+ry-16),'high')
 # Designed broad shade clusters prevent an airbrushed shaded-ball appearance.
 poly(d,[(cx-rx+4,cy+5),(cx-rx+9,cy+11),(cx+4,cy+ry-1),(cx+13,cy+ry-6),(cx+rx-1,cy+3),(cx+rx-2,cy+ry-5),(cx+7,cy+ry),(cx-7,cy+ry-1)],'deep')
 line(d,[(cx-rx+4,cy-ry+8),(cx-rx+7,cy-ry+5),(cx-9,cy-ry+4)],'edge',1)
 if corrupt:
  eye(d,cx-2+lean*.28,cy-2,10,14,look,blink,True)
  # The errant second eye sits above the face, connected by visible torn magenta tissue.
  eye(d,cx+17,cy-ry+1,7,9,1,blink*.3,True)
  curve=[(cx+8,cy+1),(cx+15,cy+2),(cx+19,cy+7),(cx+17,cy+14),(cx+10,cy+16),(cx+8,cy+10)]
  poly(d,curve,'redshade');poly(d,[(cx+9,cy+2),(cx+16,cy+3),(cx+19,cy+7),(cx+16,cy+11),(cx+11,cy+12)],'pink')
  poly(d,[(cx+12,cy+6),(cx+16,cy+5),(cx+18,cy+8),(cx+16,cy+11),(cx+12,cy+10)],'cavity')
  d.rectangle((round(cx+12),round(cy+6),round(cx+13),round(cy+7)),fill=C['teeth'])
  poly(d,[(cx-9,cy+9),(cx-5,cy+10),(cx-5,cy+ry+3),(cx-7,cy+ry+6),(cx-9,cy+ry+2)],'redshade')
  line(d,[(cx-8,cy+10),(cx-8,cy+ry+2)],'pink',2)
  poly(d,[(cx+3,cy+11),(cx+6,cy+12),(cx+5,cy+ry+7),(cx+1,cy+ry+6)],'pink')
  line(d,[(cx+12,cy+12),(cx+16,cy+18),(cx+21,cy+18),(cx+22,cy+14)],'red',3);line(d,[(cx+13,cy+12),(cx+17,cy+17),(cx+21,cy+17)],'rose',1)
 else:
  eye(d,cx+12+lean*.15,cy-4,7,10,2,blink)
  eye(d,cx-4+lean*.3,cy,10,14,look,blink)
  line(d,[(cx+9,cy+12),(cx+13,cy+13),(cx+15,cy+11)],'outline',1)
 if pose in ['tackle','pound','brake']:
  line(d,[(cx-12,cy-11),(cx-2,cy-7),(cx+6,cy-9)],'outline',2)
 if pose in ['hurt','dead']:
  ex=cx-4;ey=cy;line(d,[(ex-4,ey-4),(ex+3,ey+3)],'cream',2);line(d,[(ex+3,ey-4),(ex-4,ey+3)],'cream',2)
 # Near arm: authored knuckles and cuff dominate the action silhouette.
 line(d,[(cx+13,cy+6),front_hand],'outline',5);line(d,[(cx+13,cy+5),front_hand],'body',3)
 glove(im,*front_hand,mode=handmode,angle=16 if pose in ['rise','carry'] else -5)
 return im

def main():
 ap=argparse.ArgumentParser();ap.add_argument('--root',type=Path,default=Path(__file__).resolve().parents[1]);args=ap.parse_args();out=args.root/'Assets/GloomBean/Resources/QualityBar';out.mkdir(parents=True,exist_ok=True)
 manifest={'schema':1,'method':'Original deterministic keyed body/limb/face pixel drawing. No third-party or reference screenshot pixels. Animation reads actor state; it never changes gameplay.','poses':POSES,'cell':[64,64],'frames':8,'sheets':[]}
 for corrupted in (False,True):
  sheet=Image.new('RGBA',(8*64,len(POSES)*64))
  for row,pose in enumerate(POSES):
   for frame in range(8):sheet.paste(draw(corrupted,pose,frame),(frame*64,row*64))
  name='host_motion_'+('open' if corrupted else 'original')+'.png';sheet.save(out/name,optimize=True)
  manifest['sheets'].append({'name':name,'bytes':(out/name).stat().st_size,'sha256':hashlib.sha256((out/name).read_bytes()).hexdigest()})
 doc=args.root/'Documentation/QualityBarQ1';doc.mkdir(parents=True,exist_ok=True);(doc/'Q9_MOTION_ASSETS.json').write_text(json.dumps(manifest,indent=2)+'\n')
 print('QUALITY_MOTION_EXPORT_PASS',len(POSES)*8*2)
if __name__=='__main__':main()
