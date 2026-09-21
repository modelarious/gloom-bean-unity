"""Original32/64px boss forms: palette-stepped volume, readable silhouette, phase expression."""
from pathlib import Path
import sys,math,uuid,re,json,hashlib
ROOT=Path(__file__).resolve().parents[1];sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image,ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/BroadVisual';N=Image.Resampling.NEAREST
C={'ink':'#13162a','void':'#080c1b','shade':'#34203e','cloth':'#653e66','fold':'#a2628a','rim':'#df92a6','bone':'#fbe9bb','boneShade':'#9b7482','gold':'#c58c4b','shine':'#ffe5a2','rose':'#e34878','steel':'#577b88','steelLight':'#98b8b3'}
def col(c):return C.get(c,c)
def poly(d,pts,c):d.polygon(pts,fill=col(c))
def line(d,pts,c,w=1):d.line(pts,fill=col(c),width=w)
def ell(d,box,c):d.ellipse(tuple(round(v) for v in box),fill=col(c))
def volume(im,box,ramp):
 x0,y0,x1,y1=box;d=ImageDraw.Draw(im);ell(d,box,'ink');px=im.load();cx=(x0+x1)/2;cy=(y0+y1)/2;rx=(x1-x0)/2-1;ry=(y1-y0)/2-1;colors=[tuple(bytes.fromhex(x.lstrip('#'))) for x in ramp]
 for y in range(max(0,round(y0)+1),min(im.height,round(y1))):
  for x in range(max(0,round(x0)+1),min(im.width,round(x1))):
   xx=(x-cx)/rx;yy=(y-cy)/ry;dd=xx*xx+yy*yy
   if dd>1:continue
   shade=max(0,min(.999,(math.sqrt(1-dd)*.62-xx*.28-yy*.19+.2)));k=min(len(colors)-1,int(shade*len(colors)));px[x,y]=(*colors[k],255)
def eye(im,x,y,rx,ry,tilt=0):
 d=ImageDraw.Draw(im);volume(im,(x-rx-1,y-ry-1,x+rx+1,y+ry+1),['#8c4c4e','#be8b47','#efc878','#fff0b5']);ell(d,(x-2,y-ry+3,x+2,y+ry-2),'void');d.point((x-1,y-ry+4),fill=col('bone'))
def stitch(d,pts):
 line(d,pts,'ink',2)
 for x,y in pts:line(d,[(x-2,y),(x+2,y+1)],'boneShade')
for boss in [1,2,3]:
 for phase in range(3):
  im=Image.new('RGBA',(64,64));d=ImageDraw.Draw(im)
  if boss==1:
   poly(d,[(21,27),(10,53),(3,57),(14,63),(32,58),(49,63),(61,55),(55,49),(43,27)],'ink')
   poly(d,[(23,28),(15,52),(29,47),(32,57),(45,51),(53,55),(42,29)],'cloth')
   for x in [21,28,36,42]:line(d,[(x,32),(x+(x-32)//3,53)],'fold',2);line(d,[(x-1,33),(x-1+(x-32)//3,51)],'rim')
   volume(im,(16,13,48,42),['#50334f','#987177','#cfa794','#ffdfb0']);eye(im,25,24,5,6);eye(im,39,22,5,6)
   poly(d,[(22,31),(29,33),(44,28),(41,36),(31,40),(24,37)],'void')
   for x in range(25,42,4):poly(d,[(x,32),(x+2,32),(x+1,36+phase)],'bone')
   poly(d,[(20,14),(20,4),(26,1),(44,5),(45,15),(53,17),(13,19),(11,16)],'ink')
   poly(d,[(23,5),(26,3),(41,6),(43,14),(23,14)],'shade');line(d,[(24,6),(25,12)],'fold',2);line(d,[(20,14),(45,14)],'gold',2);line(d,[(14,16),(50,16)],'fold')
   for sign in [-1,1]:
    x=32+sign*21;y=37-phase*3;line(d,[(32+sign*11,33),(32+sign*20,43),(x,y)],'ink',6);line(d,[(32+sign*11,33),(32+sign*20,43),(x,y)],'cloth',3);volume(im,(x-5,y-5,x+5,y+4),['#615268','#a3939e','#eee0bb']);
    for k in range(3):line(d,[(x-2+k*2,y-2),(x-2+k*2,y+2)],'boneShade')
   if phase>0:stitch(d,[(33,14),(31,20),(34,28),(31,31)])
   if phase==2:eye(im,34,47,4,5)
  elif boss==2:
   # Bent pear magistrate, broad volume, hanging pans and gavel emphasize weight.
   line(d,[(31,11),(27,4),(34,1)],'ink',4);line(d,[(31,11),(29,5),(33,2)],'gold',2)
   poly(d,[(30,7),(37,1),(45,3),(42,7),(34,9)],'#78965a');line(d,[(34,6),(42,4)],'#b8b47b')
   volume(im,(17,11,47,52),['#343747','#616643','#8c9956','#c7c477','#f8de9e']);volume(im,(13,29,50,54),['#3a3b36','#686843','#a4a266','#dcc882','#fee4a2'])
   # pear neck is deliberately not a giant gold ellipse: brows and cheeks break it into planes.
   eye(im,25,32,5,7);eye(im,39,31,6,7);poly(d,[(28,41),(34,39),(36,42),(33,45)],'#764f44');line(d,[(23,45),(27,48),(38,47),(42,42)],'ink',2)
   for x,y in [(18,35),(22,40),(43,42),(46,36),(28,22),(39,22)]:d.point((x,y),fill='#8d8552');d.point((x+1,y+2),fill='#c2b573')
   line(d,[(11,13),(52,13)],'ink',5);line(d,[(13,12),(50,12)],'gold',2);poly(d,[(19,10),(19,6),(25,8),(29,5),(34,8),(42,6),(45,11)],'gold')
   for x in [6,57]:
    line(d,[(x,27),(x,46)],'boneShade');line(d,[(x-5,44),(x,49),(x+5,44)],'gold',2);line(d,[(x-5,44),(x+5,44)],'bone');line(d,[(x,28),(32,20)],'ink',2)
   for x in [22,42]:poly(d,[(x,50),(x-8,58),(x-6,61),(x+6,61),(x+4,52)],'ink');line(d,[(x-3,58),(x+4,58)],'gold',2)
   if phase>0:line(d,[(41,12),(43,23),(39,29)],'rose',1)
   if phase==2:poly(d,[(24,45),(39,41),(40,48),(31,51)],'void');line(d,[(27,46),(35,45)],'bone',2)
  else:
   # Measuring legs, ruled coat, pointed mask and one disengaged eye.
   poly(d,[(25,25),(15,53),(5,62),(21,61),(31,39),(43,61),(59,62),(50,52),(42,23)],'ink')
   poly(d,[(26,28),(21,48),(31,43),(43,53),(40,27)],'steel');line(d,[(28,30),(25,43)],'steelLight',2);line(d,[(35,28),(41,47)],'boneShade',2)
   for x in [10,54]:line(d,[(x,2),(x,48)],'ink',5);line(d,[(x-1,3),(x-1,47)],'gold',2)
   for y in range(8,44,5):line(d,[(9,y),(12,y)],'boneShade')
   poly(d,[(24,7),(37,4),(45,14),(42,25),(31,31),(21,21),(18,14)],'ink');poly(d,[(25,9),(36,7),(42,15),(38,24),(31,27),(23,21),(21,14)],'boneShade');poly(d,[(25,9),(32,8),(32,24),(25,19),(23,14)],'bone')
   eye(im,27,16,4,4);eye(im,38,14,3,4);poly(d,[(31,17),(36,24),(30,23)],'gold');line(d,[(28,26),(36,25)],'ink')
   line(d,[(22,31),(10,37),(14,43)],'ink',5);line(d,[(22,31),(10,37),(14,43)],'steel',3);line(d,[(42,31),(53,34),(49,45)],'ink',5);line(d,[(42,31),(53,34),(49,45)],'steel',3)
   volume(im,(26,36,40,49),['#233b4a','#457481','#7aa4a2','#c4cfb4']);line(d,[(30,42),(36,42)],'gold');line(d,[(33,39),(33,46)],'bone')
   for i in range(phase):line(d,[(24+i*11,10),(21+i*11,18),(26+i*8,23)],'rose')
  path=OUT/f'boss_{boss}_{phase}.png';im.resize((128,128),N).save(path,optimize=True);meta=Path(str(path)+'.meta')
  if not meta.exists():
   s=(ROOT/'Assets/GloomBean/Resources/VisualV6/background_7.png.meta').read_text();s=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/broad/boss_'+str(boss)+'_'+str(phase)).hex,s);meta.write_text(s)
(ROOT/'Documentation/BroadVisual/BOSS_ART_MANIFEST.json').write_text(json.dumps({'scope':'Original three-phase artwork for Usher/Judge/Surveyor. Last two bosses retain already-authored P5 sprites. No new boss behavior or colliders.','assets':[{'file':f.name,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()} for f in sorted(OUT.glob('boss_*.png'))]},indent=2),encoding='utf-8')
print('BROAD_BOSS_ART',len(list(OUT.glob('boss_*.png'))))
