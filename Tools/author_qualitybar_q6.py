"""Q6 correction of observed Q5 material regression and repeated flat prop silhouettes."""
from pathlib import Path
import sys,subprocess,io,json,math,hashlib
ROOT=Path(__file__).resolve().parents[1];sys.path[:0]=[str(ROOT/'.visual-tools'),str(ROOT.parent/'GloomBeanUnity/.visual-tools')]
from PIL import Image,ImageDraw,ImageEnhance
O=ROOT/'Assets/GloomBean/Resources/QualityBar';B=ROOT/'Assets/GloomBean/Resources/BroadVisual';N=Image.Resampling.NEAREST
base='77a46bb7160b25c39713bf7b23dda2fe069e55d7'
# Q5's new foreground masonry was too regular and flat. Restore the better actual Q4
# chamfered clusters, retaining the new Q5 room composition/contact/cable silhouette.
for w in [3,4,5]:
 path='Assets/GloomBean/Resources/QualityBar/solid_'+str(w)+'.png';im=Image.open(io.BytesIO(subprocess.check_output(['git','-C',str(ROOT),'show',base+':'+path]))).convert('RGBA');im=ImageEnhance.Color(im).enhance(1.08);im.save(O/f'solid_{w}.png',optimize=True)
# Warm bronze bell: light planes and engraved seams around an actual curved casting.
ink='#191320';shadow='#3e293a';bronze='#765043';mid='#aa7751';gold='#d6ac73';light='#f5d6a1'
im=Image.new('RGBA',(96,112));d=ImageDraw.Draw(im)
d.rectangle((41,0,54,13),fill=ink);d.rectangle((44,1,51,12),fill=bronze);d.arc((38,4,58,23),175,365,fill=gold,width=3)
poly=[(40,12),(55,12),(65,25),(70,51),(76,68),(89,78),(92,92),(78,102),(19,102),(4,92),(7,78),(20,68),(26,51),(30,25)]
d.polygon(poly,fill=ink);d.polygon([(40,16),(54,16),(60,25),(67,57),(74,73),(85,81),(86,88),(74,95),(21,95),(11,88),(13,81),(23,74),(31,57),(35,25)],fill=bronze)
d.polygon([(37,26),(44,18),(49,18),(46,68),(32,89),(18,87),(25,73),(31,51)],fill=mid)
d.polygon([(41,27),(45,21),(46,59),(38,76),(29,83),(34,67)],fill=gold)
d.polygon([(56,26),(62,31),(65,59),(74,77),(68,87),(53,86),(54,65)],fill=shadow)
d.arc((31,13,64,37),185,355,fill=gold,width=2)
for y in [34,68,84]:
 d.arc((25 if y==34 else 9,y-8,72 if y==34 else 87,y+10),0,180,fill=ink,width=3)
 d.arc((26 if y==34 else 10,y-9,71 if y==34 else 86,y+8),0,180,fill=gold,width=2)
d.ellipse((11,81,85,99),fill=ink);d.arc((9,79,87,99),0,180,fill=mid,width=4);d.arc((13,81,83,97),0,180,fill=gold,width=1)
d.ellipse((40,90,56,109),fill=ink);d.ellipse((43,94,52,105),fill=mid);d.line((44,96,45,102),fill=light,width=2)
# Uneven grooves are engraved marks, not text gibberish.
for x in range(31,68,7):d.line([(x,45),(x-1,53),(x+2,54),(x+3,45)],fill=shadow,width=1);d.point((x+1,45),fill=gold)
d.line([(64,24),(59,36),(63,43),(57,52),(61,61)],fill=ink,width=2);d.line([(65,25),(60,36),(64,43)],fill=gold,width=1)
im.save(O/'bell.png',optimize=True)
# Sarcophagus: asymmetrical bevel, strapped timbers, worn recessed face and sculpted hands.
im=Image.new('RGBA',(96,112));d=ImageDraw.Draw(im)
outer=[(29,1),(65,1),(90,30),(83,83),(63,109),(30,107),(10,81),(5,31)]
d.polygon(outer,fill=ink);d.polygon([(30,4),(64,4),(85,31),(78,81),(61,104),(31,102),(15,79),(10,32)],fill=bronze)
d.polygon([(30,5),(61,5),(57,11),(33,11),(19,35),(23,77),(37,97),(30,101),(15,79),(11,32)],fill=gold)
d.polygon([(65,6),(84,31),(77,82),(60,103),(55,96),(69,77),(74,33),(60,12)],fill=shadow)
inner=[(34,12),(57,12),(74,34),(69,78),(54,97),(37,97),(23,77),(19,36)]
d.polygon(inner,fill=ink);d.polygon([(35,15),(56,15),(70,35),(66,77),(53,92),(39,92),(27,75),(23,37)],fill='#604356')
for x in [33,42,52,62]:
 d.line([(x,25),(x-2,45),(x+2,68),(x,83)],fill='#422c40',width=2);d.line([(x+2,27),(x+1,44),(x+3,63)],fill='#8f6971')
for y in [30,76]:
 d.polygon([(18,y),(76,y+1),(75,y+5),(19,y+4)],fill=ink);d.line((21,y+1,73,y+2),fill=gold,width=2)
 for x in [24,66]:d.ellipse((x,y-1,x+4,y+3),fill=shadow);d.point((x+1,y),fill=light)
# Relief instead of a flat cross icon; small face and folded hands are tonal, not an enemy.
d.ellipse((38,36,54,56),fill=ink);d.ellipse((40,37,52,53),fill='#a68a84');d.line((41,42,51,42),fill=shadow,width=2);d.line((46,42,48,48),fill=gold);d.line((43,50,49,50),fill=shadow)
d.polygon([(37,57),(54,55),(58,68),(53,73),(41,73),(34,66)],fill=shadow);d.line([(36,61),(45,65),(52,59)],fill='#b5978c',width=3);d.line([(55,63),(47,69),(40,64)],fill=gold,width=2)
d.line([(28,20),(33,26),(29,38)],fill=ink);d.line([(58,81),(55,85),(57,92)],fill=ink)
im.save(O/'coffin.png',optimize=True)
# Eliminate flat colour walls between apartment rooms using retained textured masonry
# while preserving the newly authored windows, furniture and cutaway room proportions.
texture=Image.open(ROOT/'ArtSources/Ansimuz/collection/Old-dark-Castle-tileset-Files/PNG/old-dark-castle-interior-background.png').convert('RGBA').resize((320,200),N)
for i in [9,12,23]:
 im=Image.open(B/f'scene_{i}.png').convert('RGBA');pix=im.load();tex=texture.load()
 for y in range(200):
  for x in range(320):
   r,g,b,a=pix[x,y]
   # Only medium blue masonry. Dark furnished recesses and white frames are untouched.
   if 38<r<95 and 48<g<106 and b>g and y%102>83:
    rr,gg,bb,aa=tex[x,y];t=(rr+gg+bb)/765;v=.73+.32*t;pix[x,y]=(int(r*v),int(g*v),int(b*v),a)
 im.save(B/f'scene_{i}.png',optimize=True)
manifest=[]
for f in [O/'bell.png',O/'coffin.png',*[O/f'solid_{w}.png' for w in [3,4,5]],*[B/f'scene_{i}.png' for i in [9,12,23]]]:manifest.append({'path':str(f.relative_to(ROOT)),'sha256':hashlib.sha256(f.read_bytes()).hexdigest()})
(ROOT/'Documentation/QualityBarQ1/Q6_ASSETS.json').write_text(json.dumps({'scope':'Q5 visual regression corrected; original dimensional bronze and sarcophagus motifs used throughout matching stages','files':manifest},indent=2))
print('QUALITY_Q6_ASSETS',len(manifest))
