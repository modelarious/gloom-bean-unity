from pathlib import Path
import sys,json,subprocess,io,re,uuid
ROOT=Path(__file__).resolve().parents[1];sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image,ImageDraw,ImageEnhance
OUT=ROOT/'Assets/GloomBean/Resources/BroadVisual';P=json.loads((ROOT/'Documentation/BroadVisual/ART_MANIFEST.json').read_text())['profiles'];BASE='c70ea3b3d6013362ad2a97ad634d795bc533db6d';N=Image.Resampling.NEAREST
for p in P:
 i=p['id'];pal=[tuple(bytes.fromhex(x)) for x in p['palette'].split()]
 def original(name):return Image.open(io.BytesIO(subprocess.check_output(['git','-C',str(ROOT),'show',BASE+':Assets/GloomBean/Resources/BroadVisual/'+name+'.png']))).convert('RGBA')
 face=original('face_'+str(i));face=ImageEnhance.Color(face).enhance(1.14);face=ImageEnhance.Brightness(face).enhance(1.17);face.save(OUT/f'face_{i}.png',optimize=True)
 # Compact endcap is only a visual bevel aligned with the actual collider edge.
 cap=Image.new('RGBA',(4,16),pal[1]);d=ImageDraw.Draw(cap);d.line((0,0,0,15),fill=pal[0]);d.line((1,0,1,14),fill=pal[4]);d.line((2,1,2,14),fill=pal[3]);d.line((3,2,3,15),fill=pal[1]);cap.save(OUT/f'cap_{i}.png')
 meta=(ROOT/'Assets/GloomBean/Resources/VisualV6/background_7.png.meta').read_text();meta=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/broad/cap_'+str(i)).hex,meta);(OUT/f'cap_{i}.png.meta').write_text(meta)
 # Unique near architecture is quieter than playable platforms. Dim the whole backing material, not actor/HUD.
 bay=original('bay_'+str(i));bay=ImageEnhance.Brightness(bay).enhance(.79);bay.save(OUT/f'bay_{i}.png',optimize=True)
 # The repeating architectural theme stays, but distant repetitions must not masquerade as floors.
 bg=original('scene_'+str(i))
 if i not in [0,5,7,8,13,19,22,24]:bg=ImageEnhance.Brightness(bg).enhance(.77)
 bg.save(OUT/f'scene_{i}.png',optimize=True)
# Root substrate carries actual wet/dry state through material, rather than large flat translucent blocks.
for wet in [0,1]:
 colors=['#24373e','#426659','#779079','#a5b797'] if wet else ['#352638','#5e4254','#997265','#b9a483']
 im=Image.new('RGBA',(32,32),colors[0]);d=ImageDraw.Draw(im)
 for y in range(0,32,8):
  for x in range(-3 if y%16 else 1,32,11):d.polygon([(x,y+2),(x+5,y),(x+9,y+3),(x+8,y+6),(x+2,y+7)],fill=colors[1]);d.line([(x+2,y+2),(x+5,y+1),(x+7,y+3)],fill=colors[2])
 for x in [5,17,29]:d.line([(x,0),(x-3,8),(x+2,15),(x-2,24),(x,31)],fill=colors[3] if wet else colors[2],width=1)
 im.save(OUT/f'soil_{wet}.png');meta=(ROOT/'Assets/GloomBean/Resources/VisualV6/background_7.png.meta').read_text();meta=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/broad/soil_'+str(wet)).hex,meta);(OUT/f'soil_{wet}.png.meta').write_text(meta)
print('BROAD_CLARITY_MATERIALS_EXPORTED')
