from pathlib import Path
import sys,math,uuid,json,hashlib
ROOT=Path(__file__).resolve().parents[1];sys.path[:0]=[str(ROOT/'.visual-tools'),str(ROOT.parent/'GloomBeanUnity/.visual-tools')]
from PIL import Image,ImageDraw
O=ROOT/'Assets/GloomBean/Resources/QualityBar';A=ROOT/'ArtSources/Ansimuz';N=Image.Resampling.NEAREST
# Reusable individual garden tools, not a full scene. Source credit remains attached.
for name,path in [('garden_well','town/PNG/environment/props-sliced/well.png'),('fruit_cart','town/PNG/environment/props-sliced/wagon.png'),('pantry_crates','town/PNG/environment/props-sliced/crate-stack.png')]:
 im=Image.open(A/path).convert('RGBA');scale=min(1,96/max(im.size));im=im.resize((round(im.width*scale),round(im.height*scale)),N);im.save(O/(name+'.png'),optimize=True)
# A cutaway tenement facade. It is architectural art behind existing floor/collision surfaces, not an extra playable house.
im=Image.open(A/'town/PNG/environment/props-sliced/house-b.png').convert('RGBA').crop((5,104,205,244)).resize((128,90),N)
im.save(O/'tenement.png',optimize=True)
# The Garden of No Seasons needs a grafted tree and climate apparatus, not the manuscript's book motif.
im=Image.open(A/'bridge/Props/tree.png').convert('RGBA').resize((104,128),N);im.save(O/'season_tree.png',optimize=True)
# Original procession-carrier poses. The frame will be selected from actual carrier motion.
ink='#131220';deep='#342536';mid='#655061';light='#ae8390';rim='#d9ada3';bone='#efcfb0';skin='#bb9594';shadow='#705767'
for pose in range(4):
 im=Image.new('RGBA',(32,48));d=ImageDraw.Draw(im);step=[0,3,0,-3][pose]
 d.line((8,3,25,3),fill=ink,width=4);d.line((9,2,25,2),fill=rim)
 # Raised elbows support the actual coffin over their shoulders.
 d.line([(12,20),(4,13),(7,4)],fill=ink,width=5);d.line([(13,20),(6,13),(8,5)],fill=mid,width=3);d.line([(23,20),(28,12),(25,4)],fill=ink,width=5);d.line([(23,20),(26,12),(24,5)],fill=light,width=2)
 d.polygon([(12,17),(23,16),(26,33),(22,36),(10,36),(8,31)],fill=ink);d.polygon([(13,18),(22,18),(23,32),(19,35),(12,33)],fill=mid)
 d.line([(13,19),(11,30),(14,33)],fill=light);d.line([(20,20),(19,32),(21,33)],fill=deep,width=2)
 d.polygon([(12,8),(16,4),(23,7),(24,15),(20,22),(14,21),(10,16)],fill=ink)
 d.polygon([(14,9),(18,7),(22,9),(22,15),(19,18),(14,16)],fill=skin);d.line((14,10,19,10),fill=bone);d.line((13,12,20,12),fill=ink,width=2);d.polygon([(19,12),(21,16),(18,16)],fill=shadow)
 d.line([(12,8),(16,5),(23,8)],fill=light);d.line((14,20,21,19),fill=deep,width=2)
 for x,offset in [(13,step),(21,-step)]:
  d.line([(x,33),(x+offset,40),(x+offset-2,45)],fill=ink,width=5);d.line([(x,34),(x+offset,40),(x+offset-2,44)],fill=mid,width=3);d.line((x+offset-4,46,x+offset+2,46),fill=ink,width=3);d.line((x+offset-3,44,x+offset+1,44),fill=light)
 d.rectangle((6,3,10,6),fill=bone);d.rectangle((23,3,26,6),fill=skin)
 im.save(O/f'pallbearer_{pose}.png',optimize=True)
# Original seasonal wheel: spring bud, heat, rot and frost have separate silhouettes as well as color.
im=Image.new('RGBA',(64,64));d=ImageDraw.Draw(im)
d.ellipse((2,2,61,61),fill=ink);d.ellipse((5,5,58,58),fill='#b18b59');d.ellipse((9,9,54,54),fill=deep);d.arc((5,5,58,58),165,300,fill='#f6d596',width=2)
for x,y,kind in [(22,20,0),(43,22,1),(22,43,2),(43,43,3)]:
 if kind==0:
  d.line((x,y+7,x,y-4),fill='#96b466',width=2);d.polygon([(x,y),(x-8,y-5),(x-8,y-10),(x-2,y-8)],fill='#d0c981');d.polygon([(x,y-3),(x+5,y-8),(x+9,y-7),(x+6,y-1)],fill='#729e6d')
 elif kind==1:
  d.polygon([(x-6,y+5),(x-7,y),(x-2,y-10),(x,y-3),(x+6,y-7),(x+5,y+5)],fill='#e29557');d.polygon([(x-2,y+3),(x,y-5),(x+3,y+3)],fill='#ffde8d')
 elif kind==2:
  d.ellipse((x-7,y-6,x+7,y+7),fill='#775269');d.line((x-4,y-3,x+3,y+3),fill='#b48891',width=2);d.rectangle((x+2,y+3,x+6,y+5),fill='#423548');d.ellipse((x-1,y-7,x+5,y-2),fill='#a07486')
 else:
  for dx,dy in [(7,0),(0,7),(5,5),(-5,5)]:d.line((x-dx,y-dy,x+dx,y+dy),fill='#d7e0d9',width=1)
  d.ellipse((x-2,y-2,x+2,y+2),fill='#8dabb8')
d.ellipse((25,25,38,38),fill=ink);d.ellipse((28,28,35,35),fill='#bd9564');d.point((29,29),fill='#f4d59a');im.save(O/'season_wheel.png',optimize=True)
# Tall sewing spindle and spool for structural seams rather than another unrelated coffin.
im=Image.new('RGBA',(48,80));d=ImageDraw.Draw(im)
d.line((24,0,24,79),fill=ink,width=5);d.line((23,0,23,79),fill='#d6c6b4',width=2);d.ellipse((19,4,28,19),outline='#d6c6b4',width=2);d.ellipse((21,7,26,17),fill=ink)
d.rectangle((7,37,41,65),fill=ink)
for y in range(39,65,3):d.line((9,y,39,y),fill=['#b47083','#d4a59b','#774a66'][(y//3)%3])
for y in [32,64]:d.ellipse((2,y,46,y+10),fill=ink);d.ellipse((5,y+1,43,y+6),fill='#916373');d.line((9,y+2,38,y+2),fill='#d8ad9c')
im.save(O/'spindle.png',optimize=True)
for f in O.glob('*.png'):
 m=Path(str(f)+'.meta')
 if not m.exists():m.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/qualitybar/'+f.stem).hex+'\n',encoding='utf-8')
print('Q4_ATLAS_ALIGNED_OBJECTS',len(list(O.glob('*.png'))))
