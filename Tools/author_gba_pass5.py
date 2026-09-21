"""P5 original boss silhouettes and credited architectural palette composition.
All pixels are offline exports. Runtime physics, hitboxes and gameplay predicates never read this script.
"""
from pathlib import Path
import sys,math,json,hashlib
ROOT=Path(__file__).resolve().parents[1];sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image,ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/VisualV6'
N=Image.Resampling.NEAREST
C={'ink':'#161727','deep':'#292035','shadow':'#4d314b','cloth':'#77536b','fold':'#a57080','rim':'#d9a197','skinDark':'#705765','skin':'#b99a9a','skinLight':'#e4c3a9','bone':'#fff0cc','gold':'#cf9c60','dullGold':'#806956','pink':'#d64379','hot':'#ff8193','pupil':'#100f20','teal':'#729caa'}
def poly(d,pts,c):d.polygon(pts,fill=C.get(c,c))
def line(d,pts,c,w=1):d.line(pts,fill=C.get(c,c),width=w)
def ellipse(d,box,c):d.ellipse(box,fill=C.get(c,c))
def curve(d,pts,c,w=1):
 a,b,cc,e=pts;xy=[]
 for k in range(25):
  t=k/24;u=1-t;xy.append((round(u*u*u*a[0]+3*u*u*t*b[0]+3*u*t*t*cc[0]+t*t*t*e[0]),round(u*u*u*a[1]+3*u*u*t*b[1]+3*u*t*t*cc[1]+t*t*t*e[1])))
 line(d,xy,c,w)
def head(im,x,y,w,h,tilt=0,scream=False,hood=True):
 # Angular planes and an asymmetric nose replace circular emoji masks.
 layer=Image.new('RGBA',(w+10,h+10));d=ImageDraw.Draw(layer);cx=(w+10)//2
 if hood:
  poly(d,[(cx,0),(3,6),(0,h+6),(w+9,h+8),(w+7,6)],'ink');poly(d,[(cx,2),(5,7),(3,h+3),(w+5,h+6),(w+5,7)],'cloth');line(d,[(cx-1,3),(5,8),(4,h+3)],'rim',1)
 pts=[(5,7),(8,4),(w+1,5),(w+4,10),(w+2,h),(cx,h+5),(5,h-1)]
 poly(d,pts,'skinDark');poly(d,[(6,8),(9,5),(w,6),(w+1,12),(w-1,h),(cx,h+2),(7,h-1)],'skin')
 poly(d,[(7,8),(9,6),(cx+1,6),(cx-1,h-2),(8,h-1)],'skinLight');line(d,[(8,6),(cx+2,6)],'bone')
 ey=max(8,h//2);line(d,[(7,ey),(cx-1,ey+1)],'ink',2);line(d,[(cx+3,ey),(w+1,ey-1)],'ink',2)
 poly(d,[(cx,ey),(cx+2,ey+5),(cx-1,ey+5)],'skinDark');line(d,[(cx,ey+1),(cx,ey+4)],'bone')
 if scream:ellipse(d,(cx-2,h-2,cx+3,h+3),'ink');line(d,[(cx-1,h-2),(cx+2,h-2)],'bone')
 else:line(d,[(cx-3,h),(cx+1,h+1),(cx+3,h)],'deep')
 if tilt:layer=layer.rotate(tilt,resample=N,expand=True)
 im.alpha_composite(layer,(round(x-layer.width/2),round(y-layer.height/2)))
def limb(d,pts,width=6):
 curve(d,pts,'ink',width+3);curve(d,pts,'skinDark',width+1);curve(d,[(x-1,y-1) for x,y in pts],'skin',width-1);curve(d,[(x-2,y-1) for x,y in pts],'skinLight',max(1,width//3))
def hand(d,x,y,flip=False):
 s=-1 if flip else 1;poly(d,[(x-2,y-3),(x+4,y-2),(x+5,y+4),(x-2,y+5)],'ink');poly(d,[(x-1,y-2),(x+3,y-1),(x+3,y+4),(x-1,y+4)],'skinLight')
 for k in range(3):line(d,[(x-1+k*2,y+2),(x-1+k*2+s,y+7-k)],'skinLight',1);line(d,[(x+k*2,y+3),(x+k*2+s,y+6-k)],'skinDark',1)
for phase in range(3):
 im=Image.new('RGBA',(64,64));d=ImageDraw.Draw(im)
 # Interlocked torsos form a diagonal avalanche, not circles with faces pasted on them.
 poly(d,[(13,12),(22,5),(32,2),(46,6),(51,16),(60,24),(62,37),(57,48),(46,57),(29,62),(13,56),(5,43),(2,28)],'ink')
 poly(d,[(13,14),(24,8),(34,6),(45,10),(46,20),(57,27),(58,37),(51,48),(41,54),(26,58),(13,51),(9,40),(6,29)],'deep')
 folds=[([(12,26),(16,15),(25,13),(37,31),(48,34),(44,47),(32,43),(25,34),(16,41)],'cloth'), ([(38,15),(45,12),(53,23),(49,29),(59,40),(54,49),(39,37),(29,26)],'shadow'), ([(12,42),(24,30),(31,35),(29,42),(41,49),(38,58),(24,52)],'cloth')]
 for pts,c in folds:poly(d,pts,c)
 for pts in [[(15,27),(21,21),(30,32),(36,35)],[(10,43),(17,37),(20,39),(17,46)],[(30,42),(35,44),(44,51),(38,56)],[(45,19),(48,23),(45,30),(50,33)]]:line(d,pts,'fold',2)
 for pts in [[(19,19),(26,28),(32,33)],[(12,42),(15,40),(18,39)],[(30,44),(38,49),(40,53)]]:line(d,pts,'rim')
 # Grasping arms and knees read as real limbs even at half source resolution.
 limb(d,[(15,24),(0,21),(1,39),(14,40)],5);hand(d,13,37)
 limb(d,[(43,27),(63,12),(61,12),(59,32)],4);hand(d,57,30,True)
 limb(d,[(27,37),(18,51),(42,62),(50,49)],5);hand(d,47,47,True)
 limb(d,[(35,29),(46,36),(36,45),(24,44)],4);hand(d,24,42)
 head(im,19,17,10,13,-19,True)
 head(im,40,12,9,12,18,False)
 head(im,33,31,14,16,-12,True)
 head(im,51,41,10,12,23,True)
 head(im,17,48,9,11,-32,False)
 d=ImageDraw.Draw(im)
 # Cloth tears communicate progressive stress without changing the footprint or solution.
 for k in range(phase+1):line(d,[(9+k*19,39-k*5),(13+k*16,43-k*4),(11+k*17,48-k*3)],'pink',1)
 im.resize((128,128),N).save(OUT/f'boss_4_{phase}.png',optimize=True)
for phase in range(3):
 im=Image.new('RGBA',(64,64));d=ImageDraw.Draw(im)
 # Unequal radial cables and hooked hands, in a broad silhouette rather than hairline scribbles.
 for pts in [[(25,32),(0,24),(6,6),(15,11)],[(25,22),(15,3),(38,-3),(39,8)],[(43,25),(60,5),(68,26),(54,32)],[(47,39),(70,36),(58,60),(50,55)],[(23,44),(2,59),(-1,38),(9,36)]]:
  curve(d,pts,'ink',6);curve(d,[(x-1,y-1) for x,y in pts],'skinDark',3);curve(d,[(x-1,y-2) for x,y in pts],'skinLight',1)
 poly(d,[(15,22),(21,11),(36,8),(49,16),(55,30),(51,49),(43,57),(28,60),(17,49),(12,33)],'ink')
 poly(d,[(18,23),(23,15),(36,12),(46,19),(50,31),(47,47),(41,52),(28,55),(20,46),(16,34)],'shadow')
 poly(d,[(20,25),(26,17),(37,17),(43,23),(43,44),(34,48),(24,41)],'cloth')
 poly(d,[(23,22),(26,16),(37,13),(44,18),(46,23),(36,22),(30,28)],'fold')
 # Large accusing left eye, a small displaced second eye; retains Gloom Bean's identity family.
 ellipse(d,(18,21,37,43),'ink');ellipse(d,(20,23,35,41),'gold');ellipse(d,(22,24,33,39),'bone');ellipse(d,(25,26,30,38),'pupil');d.rectangle((25,27,26,30),fill=C['skinLight'])
 ellipse(d,(36,13,50,29),'ink');ellipse(d,(38,15,48,27),'gold');ellipse(d,(39,16,46,25),'bone');ellipse(d,(41,17,45,24),'pupil')
 # Folded eyebrows and asymmetric mouth make the face an actor rather than an eye icon.
 poly(d,[(17,23),(22,18),(34,21),(38,27),(27,24)],'ink');line(d,[(21,19),(34,22)],'rim',2)
 poly(d,[(36,16),(43,10),(51,17),(43,15)],'ink');line(d,[(40,12),(48,15)],'gold')
 poly(d,[(27,42),(39,30),(48,33),(49,45),(40,53),(30,50)],'ink')
 poly(d,[(31,44),(40,36),(46,38),(44,46),(37,50)],'pink')
 for x,y in [(33,39),(39,35),(44,35)]:poly(d,[(x-2,y),(x+2,y-1),(x+1,y+6)],'bone')
 for x,y in [(35,49),(41,47),(45,44)]:poly(d,[(x-1,y),(x+2,y),(x,y-4)],'skinLight')
 curve(d,[(38,49),(50,61),(25,61),(34,53)],'ink',7);curve(d,[(38,49),(49,60),(26,59),(34,54)],'pink',4);curve(d,[(39,49),(44,57),(29,58),(32,55)],'hot',1)
 for x,y in [(11,13),(40,7),(55,30),(50,54),(11,36)]:ellipse(d,(x-3,y-3,x+3,y+3),'ink');ellipse(d,(x-2,y-2,x+2,y+2),'gold');line(d,[(x,y-1),(x,y+1)],'pupil')
 for k in range(phase):line(d,[(19+k*23,37-k*12),(16+k*21,43-k*12),(20+k*20,47-k*10)],'hot',1)
 im.resize((128,128),N).save(OUT/f'boss_5_{phase}.png',optimize=True)
# Pale False Empyrean: masonry planes, violet recesses, gold frames and uncomfortably clean light.
source=Image.open(ROOT/'ArtSources/Ansimuz/church/backgrounds.png').convert('RGBA')
columns=Image.open(ROOT/'ArtSources/Ansimuz/church/column.png').convert('RGBA')
palette={(39,38,56):(68,61,87),(45,39,75):(98,86,105),(56,50,97):(145,125,139),(73,66,122):(186,165,159),(96,89,136):(215,198,175),(136,137,176):(255,238,190), (57,43,53):(92,67,79),(69,46,62):(132,93,94),(95,57,47):(168,119,84),(112,69,50):(194,146,88),(129,81,35):(217,168,88),(136,103,46):(238,203,124),(157,43,21):(169,69,96),(226,57,11):(244,153,118)}
def recolor(im):
 out=Image.new('RGBA',im.size);out.putdata([(*palette.get((r,g,b),(r,g,b)),a) for r,g,b,a in im.getdata()]);return out
im=Image.new('RGBA',(384,240),'#c4b5ac');d=ImageDraw.Draw(im)
for y in range(0,240,16):
 for x in range(-16 if y%32 else 0,384,32):
  d.rectangle((x,y,x+31,y+15),fill=['#beb0a8','#c9bdb0','#b7a8a3'][(x//32+y//16)%3]);d.line((x,y+15,x+31,y+15),fill='#a09097');d.line((x+31,y,x+31,y+15),fill='#a09097');d.line((x+2,y+1,x+29,y+1),fill='#e2d1ba')
# Repeated whole alcoves, not enlarged pieces cut away by the camera.
window=recolor(source.crop((0,0,160,192)))
for x in (-12,216):im.alpha_composite(window,(x,26))
column=recolor(columns).resize((82,190),N)
for x in (120,346):im.alpha_composite(column,(x,18))
d=ImageDraw.Draw(im)
for y in (12,219):
 d.rectangle((0,y,383,y+5),fill='#6b5b75');d.line((0,y,383,y),fill='#fff0c0');d.line((0,y+2,383,y+2),fill='#b68d64')
 for x in range(5,384,16):d.rectangle((x,y+1,x+3,y+3),fill='#dbcba8')
for x in (58,185,300):
 d.ellipse((x-23,8,x+23,16),outline='#806a69',width=2);d.arc((x-23,6,x+23,15),180,360,fill='#fff0bd',width=2)
# Reduce exported scenery bank deterministically; source credit remains separate.
im=im.convert('RGB').quantize(colors=32,dither=Image.Dither.NONE).convert('RGBA');im.save(OUT/'background_5.png',optimize=True)
# Ivory foreground stone is darker/warmer than the far light and has a sharp gold contact edge.
fill=Image.new('RGBA',(32,32),'#574258');d=ImageDraw.Draw(fill)
for y in range(0,32,8):
 for x in range(-8 if y%16 else 0,32,16):
  d.rectangle((x,y,x+14,y+6),fill='#b18f83');d.line((x+1,y,x+13,y),fill='#f4dbaa');d.line((x+1,y+1,x+13,y+1),fill='#dbc09b');d.line((x+13,y+2,x+13,y+6),fill='#806475');d.line((x+2,y+5,x+11,y+5),fill='#967583');d.point((x+3,y+2),fill='#fff0bd')
fill.save(OUT/'fill_5.png',optimize=True)
manifest=[]
for n in ['background_5','fill_5']+[f'boss_{w}_{a}' for w in (4,5) for a in range(3)]:
 f=OUT/(n+'.png');manifest.append({'name':f.name,'size':list(Image.open(f).size),'sha256':hashlib.sha256(f.read_bytes()).hexdigest()})
(ROOT/'Documentation/VisualV6/GBA_P5_ASSETS.json').write_text(json.dumps({'scope':'Original boss pixel silhouettes and CC0-fragment False Empyrean composition; no collider or source-rule changes','assets':manifest},indent=2),encoding='utf-8')
print('GBA_PASS5_ART_EXPORTED',len(manifest))
