from pathlib import Path
import sys,math,uuid,re,json,hashlib
ROOT=Path(__file__).resolve().parents[1];sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image,ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/BroadVisual';N=Image.Resampling.NEAREST
for phase in range(3):
 im=Image.new('RGBA',(48,48));d=ImageDraw.Draw(im)
 # Living source of corruption: anatomy-like lobes, a recessed eye, hooked vessels.
 d.line([(16,17),(12,8),(17,2),(21,3)],fill='#130f28',width=7);d.line([(16,17),(14,8),(18,3),(21,3)],fill='#8e386b',width=3)
 d.line([(31,14),(34,5),(41,7),(44,14)],fill='#150f28',width=7);d.line([(31,14),(35,7),(40,9),(43,14)],fill='#ca5c87',width=3)
 d.polygon([(9,15),(15,9),(24,12),(32,10),(40,15),(43,25),(36,34),(27,45),(19,44),(9,34),(4,24)],fill='#110f25')
 d.polygon([(10,17),(16,12),(24,15),(32,13),(38,17),(40,25),(33,33),(26,41),(20,40),(12,32),(7,24)],fill='#5a244e')
 d.polygon([(11,17),(16,13),(23,17),(20,29),(26,38),(21,38),(14,30),(9,24)],fill='#a24676')
 d.polygon([(27,17),(32,14),(37,18),(37,24),(31,31),(27,37),(23,29)],fill='#d76b8b')
 d.line([(14,16),(12,21),(15,25)],fill='#f09da3',width=2)
 d.line([(31,15),(35,18),(35,22)],fill='#ffd0ac',width=2)
 d.ellipse((15,18,32,33),fill='#211429');d.ellipse((17,20,30,31),fill='#e2b887');d.ellipse((19,21,28,29),fill='#ffe7b0');d.ellipse((23,21,26,30),fill='#120f23')
 for pts in [[(12,27),(18,30),(17,35)],[(33,28),(29,34),(30,37)],[(23,35),(25,38),(23,41)]]:d.line(pts,fill='#4f2348',width=2)
 if phase==1:d.arc((14,17,34,36),5,140,fill='#ffb2b3',width=2)
 if phase==2:
  d.line([(24,12),(21,19),(27,27),(22,35),(25,43)],fill='#160f24',width=3);d.line([(24,13),(22,19),(28,27)],fill='#ffd0aa',width=1)
 f=OUT/f'heart_{phase}.png';im.resize((96,96),N).save(f);s=(ROOT/'Assets/GloomBean/Resources/VisualV6/background_7.png.meta').read_text();s=re.sub(r'guid: [a-f0-9]+','guid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/broad/heart_'+str(phase)).hex,s);Path(str(f)+'.meta').write_text(s)
print('BROAD_HEART_ART_EXPORTED 3')
