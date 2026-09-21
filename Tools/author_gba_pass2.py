"""Original small-palette GBA scenery/interaction icons. Offline authoring, no gameplay dependency."""
from pathlib import Path
import sys,math,uuid,json,hashlib
ROOT=Path(__file__).resolve().parents[1];sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image,ImageDraw
OUT=ROOT/'Assets/GloomBean/Resources/VisualV6'
I='#18152d';P='#42304f';M='#92697c';G='#dba766';W='#fff0bc';S='#bcc9ba';T='#608b87';R='#bd527d'
def save(im,name):
    f=OUT/(name+'.png');im.save(f,optimize=True);m=Path(str(f)+'.meta')
    if not m.exists():m.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/v6/gba/'+name).hex+'\n',encoding='utf-8')
def ellipse(d,xy,c,outline=None,w=1):d.ellipse(xy,fill=c,outline=outline,width=w)
# A family of genuinely friendly, constructed parade fronts. These disappear with the existing infection event.
for k in range(3):
    im=Image.new('RGBA',(64,96));d=ImageDraw.Draw(im);wall=['#dbb291','#abc2a1','#d6b5c1'][k];trim=['#965e88','#5c7e8a','#976553'][k]
    d.polygon([(1,95),(1,28),(7,28),(7,18),(32,1),(57,18),(57,28),(62,28),(62,95)],fill=I)
    d.rectangle((4,28,59,94),fill=wall);d.polygon([(5,26),(8,19),(32,4),(55,19),(59,26)],fill=trim)
    for x in range(9,58,10):d.polygon([(x,19),(x+7,22),(x+5,26),(x-2,26)],fill=G)
    d.line([(7,27),(56,27)],fill=W,width=2);d.rectangle((5,89,59,92),fill=trim);d.line((6,89,58,89),fill=W)
    for x in [8,51]:d.rectangle((x,30,x+4,88),fill=trim);d.line((x,31,x,87),fill=W)
    # Paired upstairs windows and a lower curved stage opening.
    for x in [17,39]:
        d.rectangle((x,34,x+9,48),fill=I);ellipse(d,(x,30,x+9,39),I);d.line((x+4,32,x+4,47),fill=G);d.line((x,40,x+9,40),fill=G)
    d.rectangle((17,66,46,88),fill=I);ellipse(d,(17,52,46,79),I)
    for x in range(18,45,6):d.polygon([(x,61),(x+5,61),(x+3,74)],fill=trim)
    # A sunny smiling puppet medallion, not a horror mask during the cute opening.
    ellipse(d,(23,9,41,25),I);ellipse(d,(25,11,39,23),W);d.rectangle((28,15,29,17),fill=P);d.rectangle((35,15,36,17),fill=P);d.arc((29,15,36,21),0,180,fill=P)
    for y in [53,83]:d.line((8,y,13,y),fill=G);d.line((51,y,56,y),fill=G)
    save(im,f'parade_front_{k}')
# Distinct mechanical icon states, exact material cues for the underlying logical states.
for pressed in range(2):
    im=Image.new('RGBA',(32,8));d=ImageDraw.Draw(im);d.rectangle((0,3,31,7),fill=I);d.rectangle((1,4,30,6),fill=P)
    top=2 if pressed else 0;d.rectangle((2,top,29,top+3),fill=T if pressed else G);d.line((3,top,28,top),fill=S if pressed else W)
    for x in [5,15,26]:d.rectangle((x,top+1,x+1,top+2),fill=I)
    save(im,f'plate_{pressed}')
for state in range(2):
    im=Image.new('RGBA',(24,24));d=ImageDraw.Draw(im);d.rectangle((3,18,20,23),fill=I);d.rectangle((5,18,18,20),fill=M);ellipse(d,(8,13,15,20),P,outline=G)
    x=18 if state else 5;d.line((12,17,x,5),fill=I,width=5);d.line((12,16,x,5),fill=S,width=2);ellipse(d,(x-4,1,x+4,8),I);ellipse(d,(x-2,2,x+2,6),T if state else R)
    save(im,f'lever_{state}')
# Cures are depicted as objects, not fifteen differently named turquoise arches.
for k in range(16):
    im=Image.new('RGBA',(32,48));d=ImageDraw.Draw(im)
    if k in (1,7):
        cloth='#60476e' if k==1 else '#28213e';d.rectangle((2,3,29,45),fill=I);d.rectangle((5,6,26,44),fill=cloth);d.line((2,3,29,3),fill=G,width=2)
        for x in range(7,27,5):d.line((x,7,x-1,43),fill=M if k==1 else '#48405d');d.rectangle((x-1,4,x+1,7),fill=S)
    elif k in (2,11):
        d.line((5,7,24,39),fill=I,width=5);d.line((26,7,7,39),fill=I,width=5);d.line((5,7,24,38),fill=S,width=2);d.line((26,7,7,38),fill=S,width=2)
        ellipse(d,(0,33,13,46),R,outline=I,w=2);ellipse(d,(18,33,31,46),R,outline=I,w=2);ellipse(d,(4,36,9,43),None,outline=G);ellipse(d,(22,36,27,43),None,outline=G);ellipse(d,(13,21,18,26),G)
    elif k==3:
        d.rectangle((12,3,19,29),fill=I);d.rectangle((14,4,17,29),fill=G);d.rectangle((3,26,28,44),fill=I);d.rectangle((5,27,26,32),fill=M)
        for x in range(5,28,3):d.line((x,33,x,45),fill=W,width=2)
    elif k==4:
        d.rectangle((5,29,26,34),fill=I);d.arc((5,21,26,37),0,180,fill=G,width=3);d.rectangle((13,35,18,45),fill=S);d.rectangle((7,44,24,46),fill=I)
        d.polygon([(16,4),(10,15),(11,20),(16,22),(21,19),(22,15)],fill=I);d.polygon([(16,7),(12,15),(13,18),(17,19),(19,15)],fill='#73b6bc');d.point((14,15),fill=W)
    elif k in (5,6):
        if k==5:
            for x,dx in [(11,-5),(16,0),(22,5)]:d.line((16,40,x+dx,13),fill=T,width=2);ellipse(d,(x+dx-4,10,x+dx+4,18),'#8aa06c',outline=I);d.line((x+dx-3,25,x+dx+5,19),fill=G,width=2)
            d.rectangle((12,35,21,44),fill=M)
        else:
            d.polygon([(2,43),(9,29),(14,26),(17,32),(24,31),(30,44)],fill=I);d.polygon([(5,42),(12,29),(19,37),(24,34),(27,42)],fill=W)
            for x,y in [(6,41),(14,36),(18,41),(24,40)]:d.rectangle((x,y,x+2,y+2),fill=S)
    elif k in (0,8,9):
        d.rectangle((3,4,28,44),outline=I,width=3);d.rectangle((5,6,26,42),outline=G,width=2)
        if k==9:d.rectangle((7,9,24,39),fill='#e9e2c7');d.rectangle((11,17,20,28),fill=I)
    elif k==10:
        d.rectangle((5,3,7,45),fill=I);d.line((7,7,18,6),fill=G,width=2);d.polygon([(13,5),(28,11),(22,19),(11,16)],fill=S,outline=I)
        for y in (24,30,36):d.line((12,y,28,y-2),fill=S)
    elif k in (12,13,14):
        d.rectangle((3,14,8,45),fill=I);d.rectangle((24,14,29,45),fill=I);d.arc((3,3,29,26),180,360,fill=I,width=7);d.arc((5,5,27,24),180,360,fill=G if k==13 else M,width=3)
        for x in (5,26):d.line((x,17,x,44),fill=W if k==13 else M,width=2)
        if k==12:
            for x in (8,14,20):d.polygon([(x,11),(x+5,11),(x+2,21)],fill=W)
            d.rectangle((7,42,24,46),fill=M)
        if k==14:ellipse(d,(10,6,23,19),G);ellipse(d,(10,5,20,17),I)
    else:
        ellipse(d,(10,2,22,14),I);ellipse(d,(12,4,20,12),S);d.polygon([(12,12),(5,44),(28,44),(20,12)],fill=I);d.polygon([(13,15),(9,40),(25,40),(18,15)],fill=W)
        d.rectangle((12,23,21,31),fill=S);d.line((13,25,20,25),fill=P);d.line((13,28,19,28),fill=P)
    save(im,f'cure_{k}')
# Cathedral interior: a separate giant rib-vault, with clean foreground space and muted far planes.
im=Image.new('RGBA',(384,240),'#242239');d=ImageDraw.Draw(im)
for y in range(240):d.line((0,y,383,y),fill=tuple(round(a+(b-a)*y/239) for a,b in zip((31,31,53),(78,56,76))))
for x in [-9,103,215,327]:
    d.rectangle((x,0,x+28,239),fill='#3d354e');d.rectangle((x+4,0,x+8,239),fill='#66526a');d.rectangle((x+25,0,x+27,239),fill='#191d32')
    for y in range(15,240,34):d.rectangle((x-5,y,x+34,y+5),fill='#766276');d.line((x-4,y,x+32,y),fill='#a0878e')
    for y in range(-45,240,110):
        d.arc((x+9,y,x+140,y+184),182,273,fill='#8f7382',width=3);d.arc((x+16,y+7,x+137,y+182),180,273,fill='#342b45',width=3)
for x,y,rad in [(70,103,39),(284,140,43)]:
    ellipse(d,(x-rad,y-rad,x+rad,y+rad),'#27243d',outline='#837388',w=3)
    for k in range(8):
        a=k*math.pi/4;xx=x+int(math.cos(a)*rad*.55);yy=y+int(math.sin(a)*rad*.55);ellipse(d,(xx-8,yy-12,xx+8,yy+12),'#7c6978',outline='#ad9296');d.line((x,y,xx,yy),fill='#bdac9c',width=1)
    ellipse(d,(x-8,y-8,x+8,y+8),'#c2ac95',outline='#584357')
for x in (34,348):
    for y in range(-5,240,12):d.ellipse((x,y,x+5,y+13),outline='#a38b95',width=1)
im=im.convert('RGB').quantize(colors=28,dither=Image.Dither.NONE).convert('RGBA');save(im,'background_7')
manifest=[{'name':f.name,'size':list(Image.open(f).size),'sha256':hashlib.sha256(f.read_bytes()).hexdigest()} for f in sorted(OUT.glob('*.png'))]
(ROOT/'Documentation/VisualV6/GBA_P2_ASSETS.json').write_text(json.dumps({'provenance':'Original pixel icons and cathedral/parade compositions; other CC0 source components retain their credits','assets':manifest},indent=2),encoding='utf-8')
print('GBA_PASS2_ASSETS_PASS',len(manifest))
