from pathlib import Path
import sys,json,hashlib,random
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image,ImageDraw,ImageEnhance
OUT=ROOT/'Assets/GloomBean/Resources/VisualV6'
N=Image.Resampling.NEAREST
# Read immutable prior composition through Git instead of re-filtering a filtered export.
import subprocess,io
manifest=[]
for w in range(7):
    data=subprocess.check_output(['git','-C',str(ROOT),'show','4424d5c:Assets/GloomBean/Resources/VisualV6/background_'+str(w)+'.png'])
    im=Image.open(io.BytesIO(data)).convert('RGB').resize((384,240),N)
    im=ImageEnhance.Color(im).enhance(1.30 if w in (0,2,3) else 1.15)
    im=ImageEnhance.Contrast(im).enhance(1.14)
    # A constrained 32-color scenic bank, not millions of airbrushed intermediate colors.
    im=im.quantize(colors=32,method=Image.Quantize.MEDIANCUT,dither=Image.Dither.NONE).convert('RGBA')
    im.save(OUT/f'background_{w}.png',optimize=True)
# Foreground banks: light contact edge, textured midtone, dark recess. 32px per 2m tile.
palettes=[
 ['272447','555370','8e7790','c7a29d','fbe3a5','82b070'],
 ['171729','42304d','765166','ab786c','e8bf87','ab975d'],
 ['18272c','3b3c34','75634b','b39261','ecd395','89aa67'],
 ['101c37','30415c','536983','8b9aa2','dcc6a4','9b8091'],
 ['1c152e','452c48','805463','b38481','eac0a1','8a7279'],
 ['28233d','62586c','9a9095','c9b9a0','fff0bd','b3a05c']]
for w,colors in enumerate(palettes):
    c=['#'+x for x in colors];im=Image.new('RGBA',(32,32),c[1]);d=ImageDraw.Draw(im)
    for y in range(0,32,8):
        for x in range(-16 if y%16 else 0,32,16):
            d.rectangle((x,y,x+15,y+7),fill=c[2]);d.line((x,y+7,x+15,y+7),fill=c[0]);d.line((x+15,y,x+15,y+7),fill=c[0]);d.line((x+1,y+1,x+13,y+1),fill=c[3]);d.point((x+2,y+2),fill=c[4]);d.line((x+2,y+5,x+8,y+5),fill=c[1])
    if w in (1,2):
        # Horizontal carved timbers, not the city masonry painted green.
        im=Image.new('RGBA',(32,32),c[1]);d=ImageDraw.Draw(im)
        for y in range(0,32,8):
            d.rectangle((0,y,31,y+6),fill=c[2]);d.line((0,y,31,y),fill=c[3]);d.line((0,y+7,31,y+7),fill=c[0]);
            for k in range(3):d.line((3+k*9,y+2,8+k*9,y+2+k%2),fill=c[1]);d.point((4+k*9,y+1),fill=c[3])
            d.rectangle((1,y+1,2,y+2),fill=c[4]);d.rectangle((29,y+1,30,y+2),fill=c[0])
        if w==2:
            for x in (6,18,26):d.line((x,0,x+2,4),fill=c[5]);d.point((x-1,2),fill=c[5])
    im.save(OUT/f'fill_{w}.png',optimize=True)
    lip=Image.new('RGBA',(32,8),c[1]);d=ImageDraw.Draw(lip)
    d.rectangle((0,0,31,0),fill=c[0]);d.rectangle((0,1,31,2),fill=c[4]);d.rectangle((0,3,31,4),fill=c[3]);d.rectangle((0,5,31,6),fill=c[2]);d.line((0,7,31,7),fill=c[0])
    for x in range(0,32,8):d.line((x,3,x,6),fill=c[1]);d.point((x+1,3),fill=c[4])
    lip.save(OUT/f'lip_{w}.png',optimize=True)
for f in sorted(OUT.glob('*.png')):manifest.append({'name':f.name,'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'size':list(Image.open(f).size)})
(ROOT/'Documentation/VisualV6/GBA_ASSETS.json').write_text(json.dumps({'scope':'Offline GBA pixel-scale preparation; CC0 component credits unchanged; source N1 preserved in Git','assets':manifest},indent=2),encoding='utf-8')
print('GBA_ASSET_PREPARATION_PASS',len(manifest))
