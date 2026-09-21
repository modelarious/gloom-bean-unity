"""Q8 original material-specific object shapes, not a global colour filter."""
from pathlib import Path
import sys,math,json,hashlib,uuid
ROOT=Path(__file__).resolve().parents[1];sys.path[:0]=[str(ROOT/'.visual-tools'),str(ROOT.parent/'GloomBeanUnity/.visual-tools')]
from PIL import Image,ImageDraw
O=ROOT/'Assets/GloomBean/Resources/QualityBar'
C=['#171422','#352638','#61465a','#967480','#c3a293','#eed8b2'];G=['#30232c','#69483b','#a77650','#d3ab75','#f5d6a4']
def save(im,name):
 f=O/(name+'.png');im.save(f,optimize=True);m=Path(str(f)+'.meta')
 if not m.exists():m.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/qualitybar/'+name).hex+'\n',encoding='utf-8')
def bar(d,box):
 x,y,xx,yy=box;d.rectangle(box,fill=C[0]);d.rectangle((x+1,y+1,xx-1,yy-1),fill=C[2]);d.line((x+1,y+1,xx-1,y+1),fill=G[3]);d.line((x+1,y+2,x+1,yy-1),fill=G[2]);
 for px,py in [(x+3,y+3),(xx-3,y+3),(x+3,yy-3),(xx-3,yy-3)]:d.point((px,py),fill=G[4])
def chain(d,x,y,yy):
 for j in range(y,yy,6):d.ellipse((x,j,x+3,j+6),outline=G[1]);d.line((x,j+1,x,j+4),fill=G[3])
def wheel(d,x,y,r):
 d.ellipse((x-r,y-r,x+r,y+r),fill=C[0]);d.ellipse((x-r+2,y-r+2,x+r-2,y+r-2),fill=G[1],outline=G[3],width=2)
 for k in range(8):
  a=k*math.pi/4;d.line((x,y,x+int(math.cos(a)*(r-3)),y+int(math.sin(a)*(r-3))),fill=C[0],width=2)
 d.ellipse((x-3,y-3,x+3,y+3),fill=C[0]);d.point((x-1,y-1),fill=G[4])
# Ragged, weighed cloth with actual folds: the nose and eye cavities are recesses, not flat black circles.
im=Image.new('RGBA',(96,112));d=ImageDraw.Draw(im)
bar(d,(7,4,89,11));chain(d,16,0,5);chain(d,77,0,5)
outline=[(23,10),(72,10),(80,28),(75,45),(87,93),(77,92),(73,108),(64,98),(57,107),(46,97),(36,109),(30,99),(20,104),(14,91),(19,53),(15,32)]
d.polygon(outline,fill=C[0]);d.polygon([(26,12),(68,12),(75,31),(69,47),(82,90),(74,88),(70,101),(61,92),(56,99),(45,90),(35,102),(29,92),(22,97),(19,87),(25,52),(21,33)],fill=C[2])
for points,fill in [([(29,12),(34,14),(28,44),(29,72),(24,91),(21,87),(25,51)],C[4]), ([(48,13),(54,13),(48,40),(50,76),(44,87),(43,62)],C[3]), ([(64,14),(68,18),(68,45),(78,88),(73,92),(62,65)],C[4]), ([(36,16),(40,16),(36,51),(38,76),(31,96),(33,65)],C[1]), ([(55,15),(60,17),(53,57),(60,92),(53,99),(47,73)],C[1])]:d.polygon(points,fill=fill)
for pts in [[(28,15),(25,31),(26,54)],[(65,18),(63,32),(66,49)],[(42,58),(40,78),(36,95)]]:d.line(pts,fill=C[5])
for x,y in [(33,30),(57,28)]:d.polygon([(x-7,y),(x-2,y-4),(x+4,y-2),(x+5,y+6),(x-1,y+12),(x-6,y+8)],fill=C[0]);d.line([(x-5,y-1),(x-1,y-3),(x+4,y-1)],fill=C[4]);d.line((x+3,y+3,x+1,y+8),fill=C[1])
d.polygon([(44,35),(48,39),(46,50),(41,50)],fill=C[1]);d.line((44,37,42,46),fill=C[4]);d.line([(29,64),(38,60),(47,63),(58,61),(67,66)],fill=C[0],width=2)
for x in range(30,68,5):d.line((x,60,x+2,66),fill=G[3]);d.point((x+2,66),fill=C[0])
for x,y in [(22,92),(35,100),(56,97),(72,95)]:
 for k in range(3):d.line((x+k*2,y,x+k*2-1,y+6-k),fill=C[3])
save(im,'skin')
# A different material object for adjacent fittings: sleeves/skin bundle on a taut clothesline.
im=Image.new('RGBA',(96,112));d=ImageDraw.Draw(im);chain(d,8,0,13);chain(d,82,0,13);bar(d,(5,11,91,17))
for idx,x in enumerate([8,39,65]):
 y=18+idx*4;col=[C[3],C[2],C[4]][idx];d.polygon([(x+3,y),(x+20,y),(x+27,y+12),(x+22,y+34),(x+17,y+27),(x+20,y+62),(x+11,y+73),(x,y+62),(x+3,y+27),(x-2,y+34),(x-6,y+13)],fill=C[0]);d.polygon([(x+4,y+2),(x+18,y+2),(x+24,y+12),(x+20,y+28),(x+15,y+20),(x+17,y+60),(x+10,y+66),(x+3,y+60),(x+6,y+20),(x,y+26),(x-3,y+13)],fill=col)
 d.line([(x+8,y+3),(x+7,y+35),(x+10,y+61)],fill=C[5] if idx==2 else C[4]);d.line([(x+14,y+9),(x+13,y+42),(x+16,y+55)],fill=C[1]);d.line((x+10,y+24,x+17,y+28),fill=G[3]);bar(d,(x+6,y-3,x+10,y+5))
save(im,'garment_rack')
# Large richly constructed book, keeping the existing broad physical footprint but no huge flat page rectangles.
im=Image.new('RGBA',(96,112));d=ImageDraw.Draw(im)
d.polygon([(3,19),(34,12),(48,23),(62,11),(93,19),(91,94),(59,102),(48,94),(35,102),(5,95)],fill=C[0]);d.polygon([(5,22),(34,16),(47,26),(49,93),(35,97),(8,90)],fill='#826450');d.polygon([(49,27),(62,16),(91,22),(88,91),(60,97),(49,91)],fill='#a68e70')
for x0,x1,y0 in [(9,42,23),(53,87,23)]:
 d.polygon([(x0,y0),(x0+22,y0-5),(x1,y0+4),(x1,87),(x0+21,93),(x0,87)],fill='#d3bb8c');d.line((x0+1,y0+1,x0+1,86),fill='#f4e0ac');d.line((x1-1,y0+7,x1-1,85),fill='#8b6856')
 for y in range(37,83,5):
  for k in range(4):
   xx=x0+3+k*7;d.line((xx,y+(k%2),xx+3+(k+y)%3,y+(k%2)),fill='#826b5b');d.point((xx,y+1),fill='#b49976')
 # illuminated red initial and gold margin
 d.rectangle((x0+3,y0+4,x0+12,y0+16),fill='#694352');d.line((x0+5,y0+6,x0+9,y0+6),fill='#e3bd75');d.line((x0+5,y0+6,x0+5,y0+14),fill='#e3bd75');d.line((x0+5,y0+10,x0+9,y0+10),fill='#e3bd75')
 d.line([(x0+2,31),(x0+2,83),(x1-3,86)],fill='#a57057')
d.line((47,27,48,91),fill=C[0],width=3);d.line((49,26,50,90),fill=G[4]);d.polygon([(50,31),(53,33),(54,102),(50,107),(48,102)],fill='#844259')
for x,y in [(3,20),(83,19),(5,86),(81,88)]:bar(d,(x,y,x+10,y+6))
# Pedestal with carved load path not another rectangular floating block.
bar(d,(21,103,76,110));d.polygon([(39,98),(57,98),(66,104),(29,104)],fill=G[1]);d.line((35,101,63,101),fill=G[3]);save(im,'book')
# Alternate shut chained volume is a different silhouette, not the same book tinted.
im=Image.new('RGBA',(96,112));d=ImageDraw.Draw(im);d.polygon([(17,8),(74,1),(84,21),(78,100),(25,110),(10,96)],fill=C[0]);d.polygon([(19,13),(71,6),(78,23),(74,96),(25,104),(16,92)],fill='#423644');d.polygon([(21,14),(68,8),(71,13),(24,21),(19,95),(16,91)],fill=G[3]);d.polygon([(24,22),(73,15),(69,91),(24,101)],fill='#674859');d.polygon([(29,28),(65,23),(61,84),(28,91)],outline=C[0],fill='#483346')
d.ellipse((37,42,59,72),fill=C[0],outline=G[2],width=2);d.ellipse((41,48,55,66),fill='#bba074');d.ellipse((46,50,51,64),fill=C[0])
for y in [31,78]:d.line((16,y,76,y-8),fill=C[0],width=5);d.line((17,y-1,75,y-9),fill=G[3],width=2)
chain(d,33,11,98);save(im,'bound_volume')
# Wringer/wash basket: different silhouettes to the drum, appropriate to the laundry's actual theme.
im=Image.new('RGBA',(96,112));d=ImageDraw.Draw(im);bar(d,(9,38,21,106));bar(d,(75,36,87,106));bar(d,(14,42,81,66))
for y in [48,59]:d.rounded_rectangle((21,y-4,78,y+4),radius=4,fill=C[0]);d.line((24,y-2,74,y-2),fill=C[4],width=2);d.line((23,y+1,74,y+1),fill=G[2])
d.polygon([(37,54),(57,54),(52,86),(63,99),(48,95),(35,103),(31,92)],fill=C[0]);d.polygon([(38,57),(54,57),(49,85),(56,94),(46,91),(37,98),(34,91)],fill=C[3]);d.line([(41,59),(39,84),(40,92)],fill=C[5]);d.line([(48,60),(43,86),(48,89)],fill=C[1]);wheel(d,85,50,9);d.line((83,46,87,32),fill=G[2],width=3);d.ellipse((84,29,91,36),fill=C[0],outline=G[3]);bar(d,(6,104,90,110));save(im,'wringer')
im=Image.new('RGBA',(96,112));d=ImageDraw.Draw(im);d.polygon([(13,56),(84,54),(79,103),(22,108)],fill=C[0]);d.polygon([(17,59),(79,58),(75,98),(25,102)],fill=G[1])
for y in range(61,102,6):d.line((19,y,76,y-3),fill=G[2],width=2);d.line((20,y+2,76,y-1),fill=C[0])
for x in [26,41,57,72]:d.line((x,60,x-1,99),fill=G[3]);d.line((x+2,61,x+1,98),fill=G[0])
for k,(x,y) in enumerate([(16,45),(37,38),(54,42)]):d.polygon([(x,y+12),(x+7,y),(x+18,y+2),(x+22,y+16),(x+15,y+21),(x+19,y+29),(x+10,y+25),(x+4,y+27)],fill=C[0]);d.polygon([(x+3,y+12),(x+8,y+3),(x+16,y+4),(x+19,y+15),(x+13,y+20),(x+16,y+25),(x+7,y+22)],fill=C[3+k%2]);d.line((x+8,y+5,x+7,y+16),fill=C[5])
d.line((15,58,82,56),fill=G[3],width=3);save(im,'wash_basket')
manifest=[{'name':n,'sha256':hashlib.sha256((O/(n+'.png')).read_bytes()).hexdigest()} for n in ['skin','garment_rack','book','bound_volume','wringer','wash_basket']]
(ROOT/'Documentation/QualityBarQ1/Q8_ASSETS.json').write_text(json.dumps({'scope':'Dimensional cloth and book materials plus distinct laundry/library silhouettes. Original art only, no gameplay/fixture coordinate input.','files':manifest},indent=2))
print('QUALITY_Q8_ART',len(manifest))
