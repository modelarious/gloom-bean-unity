from pathlib import Path
import sys,math,json,hashlib,uuid
ROOT=Path(__file__).resolve().parents[1]
sys.path[:0]=[str(ROOT/'.visual-tools'),str(ROOT.parent/'GloomBeanUnity/.visual-tools')]
from PIL import Image,ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/QualityBar'
# Construction plate with large hand-resolved color clusters; previous malformed source crop rejected.
im=Image.new('RGBA',(32,32),'#151623');d=ImageDraw.Draw(im)
d.rectangle((1,1,30,30),fill='#2c3041');d.line((2,1,29,1),fill='#77737c');d.line((1,2,1,29),fill='#555465');d.line((30,2,30,30),fill='#101321');d.line((2,30,29,30),fill='#101321')
d.rectangle((4,4,27,27),fill='#414353');d.line((5,4,26,4),fill='#69616b');d.line((4,5,4,26),fill='#555a6b');d.line((27,5,27,27),fill='#262336');d.line((5,27,26,27),fill='#201e31')
for x,y in [(3,3),(28,3),(3,28),(28,28)]:
 d.rectangle((x-1,y-1,x+1,y+1),fill='#0d1220');d.point((x-1,y-1),fill='#c6b5a1');d.point((x,y),fill='#7c8492')
for y in (10,14,18,22):d.line((9,y,23,y),fill='#1f2133');d.line((10,y+1,23,y+1),fill='#646374')
d.line((6,6,6,10),fill='#858083');d.line((21,6,24,6),fill='#77707a');im.save(OUT/'solid_1.png',optimize=True)
# Recessed stone has chipped, shaded planes, rather than a uniform face with a printed tile outline.
ramps={3:['151c2d','303747','535668','777384','b4a39d','ddc6a8'],4:['1e1628','413143','6b4859','946978','c99892','f0c7a4'],5:['2c2035','644b61','947180','c29e96','e6c69f','ffecbb']}
for group,c in ramps.items():
 c=['#'+v for v in c];im=Image.new('RGBA',(32,32),c[0]);d=ImageDraw.Draw(im)
 for row,y in enumerate(range(0,32,8)):
  for x in range(-8 if row%2 else 0,32,16):
   d.polygon([(x+1,y+1),(x+12,y),(x+15,y+2),(x+14,y+6),(x+11,y+7),(x,y+6)],fill=c[2]);d.polygon([(x+1,y+1),(x+12,y+1),(x+13,y+2),(x+3,y+3)],fill=c[4]);d.line((x+1,y+2,x+1,y+5),fill=c[3]);d.polygon([(x+12,y+3),(x+15,y+2),(x+14,y+6),(x+10,y+7)],fill=c[1]);d.line((x+3,y+6,x+10,y+6),fill=c[1]);d.point((x+4,y+1),fill=c[5]);d.line((x+7,y+4,x+9,y+4),fill=c[3])
 im.save(OUT/f'solid_{group}.png',optimize=True)
# Thin translucent vellum/felt art must not receive a generic stone face.
Image.open(ROOT/'ArtSources/QualityBarQ1/cloth.png').convert('RGBA').resize((32,32),Image.Resampling.NEAREST).save(OUT/'solid_cloth.png',optimize=True)
# Conserved edible chunks keep an explicit cake/material silhouette, not wood or stone.
im=Image.new('RGBA',(32,32),'#381e2d');d=ImageDraw.Draw(im)
for y in [1,12,23]:
 d.rectangle((1,y,30,min(31,y+8)),fill='#965758');d.line((2,y,29,y),fill='#eab291');d.line((3,y+3,27,y+3),fill='#b77267');d.line((2,y+8,29,y+8),fill='#603547')
 for x in [5,14,24]:d.rectangle((x,y+5,x+2,min(31,y+6)),fill='#efd5a5')
d.line((0,0,31,0),fill='#fff0c4');d.line((0,1,31,1),fill='#f3d29d');im.save(OUT/'solid_food.png',optimize=True)
# Light remains on the GBA pixel grid. Five hard-stepped value regions; no blur or post processing.
for group in range(6):
 light=Image.new('RGBA',(64,96));d=ImageDraw.Draw(light)
 for i in range(6):
  inset=i*5;d.polygon([(29,0),(35,0),(63-inset,95),(inset,95)],fill=(255,207,129,35+i*13))
 light.save(OUT/f'light_{group}.png',optimize=True)
# Extracted independent objects are runtime assets, never screenshot evidence.
for src,dest in [('shrine','statue'),('halo_detail','halo')]:Image.open(ROOT/'ArtSources/QualityBarQ1'/(src+'.png')).save(OUT/(dest+'.png'),optimize=True)
for f in OUT.glob('*.png'):
 m=Path(str(f)+'.meta')
 if not m.exists():m.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/qualitybar/'+f.stem).hex+'\n',encoding='utf-8')
assets=[{'name':f.stem,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()} for f in sorted(OUT.glob('*.png'))]
(ROOT/'Documentation/QualityBarQ1/Q3_ASSETS.json').write_text(json.dumps({'scope':'Explicit original material authoring and concept-object adapters; no full-screen picture replacement','assets':assets},indent=2)+'\n',encoding='utf-8')
print('Q3_AUTHORED_MATERIAL_AND_OBJECT_PASS',len(assets))
