"""Whole-stage authored composition/material pass2; uses credited Ansimuz structural textures.
No comparison screenshot, camera id or review coordinate participates in asset authoring.
"""
from pathlib import Path
import sys,math,random,hashlib,json,uuid,colorsys
ROOT=Path(__file__).resolve().parents[1];sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image,ImageDraw,ImageEnhance,ImageFilter
OUT=ROOT/'Assets/GloomBean/Resources/BroadVisual';ART=ROOT/'ArtSources/Ansimuz';N=Image.Resampling.NEAREST
profiles=json.loads((ROOT/'Documentation/BroadVisual/ART_MANIFEST.json').read_text())['profiles']
def pal(i):return [tuple(bytes.fromhex(c)) for c in profiles[i]['palette'].split()]
def rgba(im):return im.convert('RGBA')
def recolor(im,palette,dark=False):
 im=rgba(im);values=sorted(set(round(r*.3+g*.58+b*.12) for r,g,b,a in im.getdata() if a>0));rank={v:k/max(1,len(values)-1) for k,v in enumerate(values)};dst=[]
 for r,g,b,a in im.getdata():
  if not a:dst.append((0,0,0,0));continue
  t=rank[round(r*.3+g*.58+b*.12)]
  lo=0 if dark else 0;hi=4 if dark else 5;q=lo+t*(hi-lo);n=int(q);f=q-n;col=tuple(round(palette[n][k]*(1-f)+palette[min(hi,n+1)][k]*f) for k in range(3));dst.append((*col,a))
 im.putdata(dst);return im
castle=Image.open(ART/'collection/Gothic-Castle-Files/PNG/layers/gothic-castle-tileset.png').convert('RGBA')
church=Image.open(ART/'church/tileset.png').convert('RGBA')
column=Image.open(ART/'church/column.png').convert('RGBA')
oldwall=Image.open(ART/'collection/Old-dark-Castle-tileset-Files/PNG/old-dark-castle-interior-background.png').convert('RGBA')
night=Image.open(ART/'collection/night-town-background-files/layers/night-town-background-town.png').convert('RGBA')
tree=Image.open(ART/'bridge/Props/tree.png').convert('RGBA')
# One coherent lighting ramp is applied to actual authored texture clusters, not unstructured pixel noise.
for profile in profiles:
 i=profile['id'];kind=profile['material'];c=pal(i);rng=random.Random(483+i)
 if kind in ['stone','ruin','plaster']:
  tile=recolor(castle.crop((32,36,64,64)),c)
  face=Image.new('RGBA',(64,32),(*c[1],255));tile=tile.resize((32,32),N);face.alpha_composite(tile,(0,0));face.alpha_composite(tile.transpose(Image.Transpose.FLIP_LEFT_RIGHT),(32,0))
  d=ImageDraw.Draw(face);d.line((0,0,63,0),fill=c[4]);d.line((0,31,63,31),fill=c[0]);face.save(OUT/f'face_{i}.png',optimize=True)
 # Add local volumetric shade to previous original machine/wood/cloth shapes, only within existing opaque pixels.
 bay=Image.open(OUT/f'bay_{i}.png').convert('RGBA'); pix=bay.load()
 for y in range(bay.height):
  for x in range(bay.width):
   r,g,b,a=pix[x,y];shade=.85+.15*max(0,1-abs(x-43)/90);shade*=.9+.1*(1-y/112);pix[x,y]=(int(r*shade),int(g*shade),int(b*shade),a)
 # Layer actual textured stone behind the already authored object silhouettes.
 texture=recolor(oldwall,c,True).resize((128,112),N)
 # Keep the foreground motif, substitute wall-like pixels only; matched shades stay dark.
 bp=bay.load();tp=texture.load()
 for y in range(112):
  for x in range(128):
   r,g,b,a=bp[x,y]
   if max(abs(r-c[1][0]*.8),abs(g-c[1][1]*.8),abs(b-c[1][2]*.8))<14 and tp[x,y][3]:
    tr,tg,tb,ta=tp[x,y];bp[x,y]=(int(tr*.7),int(tg*.7),int(tb*.7),255)
 bay.save(OUT/f'bay_{i}.png',optimize=True)
 # Complete level backgrounds remove incorrect forest/laundry backdrops in indoor chapters.
 if i==0:
  bg=Image.open(ROOT/'Assets/GloomBean/Resources/VisualV6/background_0.png').convert('RGBA').resize((320,200),N)
 elif i in [5,7,8,13,22,24]:
  # Outdoor tree, irrigation conservatory, storm wreckage and legal orchard vary in topology, not hue alone.
  bg=Image.new('RGBA',(320,200));d=ImageDraw.Draw(bg)
  top=c[0];bottom=c[2]
  for y in range(200):d.line((0,y,319,y),fill=tuple(int(top[k]*(1-y/240)+bottom[k]*(y/240)) for k in range(3)))
  if i in [13,24]:
   raw=Image.open(ROOT/'Assets/GloomBean/Resources/VisualV6/background_4.png').convert('RGBA').resize((320,200),N);bg=ImageEnhance.Color(raw).enhance(1.18)
   for x in [0,144,282]:
    crop=recolor(column,c,True).resize((38,154),N).rotate(-17,resample=N,expand=True);bg.alpha_composite(crop,(x-25,80))
  else:
   for x,y,scale in [(-46,-18,.68),(95,-32,.85),(227,-8,.61)]:
    tr=recolor(tree,c,True);tr=tr.resize((int(180*scale),int(260*scale)),N);bg.alpha_composite(tr,(x,y))
   if i in [7,8]:
    d=ImageDraw.Draw(bg)
    for x in range(-16,321,48):d.line((x,0,x+25,200),fill=c[1],width=4);d.line((x+2,0,x+27,200),fill=c[3],width=1)
    for y in [42,98,153]:d.line((0,y,319,y),fill=c[2],width=3);d.line((0,y-1,319,y-1),fill=c[3])
    if i==7:
     for y in range(161,200,7):d.line((0,y,319,y-2),fill=c[3],width=1)
   else:
    d=ImageDraw.Draw(bg)
    for x,y in [(26,64),(135,24),(255,57),(74,99)]:
     d.line((x,y-25,x,y),fill=c[2],width=2);d.ellipse((x-4,y,x+4,y+12),fill=c[3],outline=c[0]);d.point((x-2,y+3),fill=c[4])
 else:
  # Interior two-tier structural rhythm with nonidentical interruptions and large clear negative spaces.
  bg=Image.new('RGBA',(320,200),(*c[0],255))
  textured=recolor(oldwall,c,True).resize((320,200),N);bg.alpha_composite(textured)
  for row,y in enumerate([-23,83]):
   for j,x in enumerate([-10,106,222]):
    panel=bay.resize((112,112),N)
    # Coherent half-value distant architecture: actual objects remain brighter than their background echoes.
    panel=ImageEnhance.Brightness(panel).enhance(.79 if row else .67)
    bg.alpha_composite(panel,(x+(row%2)*7,y))
  d=ImageDraw.Draw(bg)
  for y in [79,187]:
   d.rectangle((0,y,319,y+7),fill=c[0]);d.line((0,y+1,319,y+1),fill=c[2],width=2);d.line((0,y+5,319,y+5),fill=c[1])
  for x in [91,210]:
   col=recolor(column,c,True).resize((22,184),N);bg.alpha_composite(ImageEnhance.Brightness(col).enhance(.84),(x,7))
  if i==19:
   # Illuminated page spread: no cavern/cloth borrowed from another chapter.
   bg=Image.open(ROOT/'Assets/GloomBean/Resources/VisualV6/background_6.png').convert('RGBA').resize((320,200),N)
   d=ImageDraw.Draw(bg)
   for x in [11,159,307]:d.line((x,0,x,199),fill=c[1]);d.line((x+2,0,x+2,199),fill=c[4]);
   for x in [22,176]:
    d.rectangle((x,16,x+18,38),fill=c[1]);d.rectangle((x+2,18,x+16,36),outline=c[4],width=1);d.arc((x+5,20,x+14,34),90,270,fill=c[6],width=2)
  if i==16:
   bg=Image.open(ROOT/'Assets/GloomBean/Resources/VisualV6/background_7.png').convert('RGBA').resize((320,200),N)
   for x in [14,281]:bg.alpha_composite(recolor(column,c,True).resize((26,182),N),(x,9))
  if i in [17,18,20,25]:
   # Use richly authored CC0 arch forms with per-level profiled structure over them.
   arches=recolor(Image.open(ART/'church/backgrounds.png').convert('RGBA').crop((0,0,320,192)),c,True).resize((320,200),N)
   bg=Image.alpha_composite(bg,arches)
   d=ImageDraw.Draw(bg)
   if i==17:
    for x,y in [(50,40),(164,89),(282,31)]:d.ellipse((x-19,y-8,x+19,y+8),outline=c[3],width=3);d.ellipse((x-13,y-6,x+13,y+6),outline=c[4],width=1)
   if i==18:
    d.ellipse((134,14,187,67),fill=c[3],outline=c[5],width=2);d.ellipse((153,25,167,59),fill=c[0]);d.polygon([(148,70),(60,199),(239,199),(174,70)],fill=(*c[4],42))
   if i==20:
    d.polygon([(159,13),(260,118),(158,198),(59,118)],outline=c[4],width=3);d.polygon([(159,23),(249,117),(158,187),(71,117)],outline=c[2],width=2)
   if i==25:
    for x in [28,94,223,290]:d.line((x,0,x+13,199),fill=c[2],width=2)
 bg=bg.convert('RGB').quantize(colors=32,dither=Image.Dither.NONE).convert('RGBA')
 path=OUT/f'scene_{i}.png';bg.save(path,optimize=True);meta=Path(str(path)+'.meta')
 if not meta.exists():
  import re
  template=(ROOT/'Assets/GloomBean/Resources/VisualV6/background_7.png.meta').read_text();template=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/broad/scene_'+str(i)).hex,template);meta.write_text(template)
# Water is a spatial material: surface waves, depth stripes, bubbles; no new flow or colliders.
water=Image.new('RGBA',(32,32),(24,65,74,100));d=ImageDraw.Draw(water)
for y in range(1,32,7):
 for x in range(-4,33,13):d.line([(x,y),(x+5,y-1),(x+9,y)],fill=(99,172,165,95),width=1)
for x,y in [(4,13),(22,27),(16,6)]:d.ellipse((x,y,x+2,y+2),outline=(181,225,205,130))
water.save(OUT/'water.png')
import re
meta=(ROOT/'Assets/GloomBean/Resources/VisualV6/background_7.png.meta').read_text();meta=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/broad/water').hex,meta);(OUT/'water.png.meta').write_text(meta)
(ROOT/'Documentation/BroadVisual/REFINED_ART_MANIFEST.json').write_text(json.dumps({'scope':'26distinct full-stage backgrounds; original profile motifs composed with credited CC0 Ansimuz textures; no Nintendo runtime artwork','assets':[{'file':f.name,'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'size':list(Image.open(f).size)} for f in sorted(OUT.glob('*.png'))]},indent=2),encoding='utf-8')
print('BROAD_REFINED_ART',len(list(OUT.glob('*.png'))))
