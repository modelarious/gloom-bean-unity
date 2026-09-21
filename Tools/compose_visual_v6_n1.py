"""V6 environmental composition. Offline authoring only; no runtime AI/dependencies.
Selected CC0 environment components: Luis Zuno / Ansimuz, provenance in ArtSources/Ansimuz.
Original Gloom Bean arrangement, palettes, manuscript, atmospheric layers and embellishment.
Run from this repository after restoring the selected source PNGs. Never reads WL4 references.
"""
from pathlib import Path
import sys, math, random, json, hashlib, uuid
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image, ImageDraw, ImageOps
SRC=ROOT/'ArtSources/Ansimuz'; OUT=ROOT/'Assets/GloomBean/Resources/VisualV6'
W,H=768,480
N=Image.Resampling.NEAREST
manifest=[]
def load(s): return Image.open(SRC/s).convert('RGBA')
def color(s):return tuple(bytes.fromhex(s.lstrip('#')))
def mix(a,b,t):return tuple(round(a[i]*(1-t)+b[i]*t) for i in range(3))
def remap(im,lo,hi,alpha=1):
    lo,hi=color(lo),color(hi);g=ImageOps.grayscale(im)
    t=Image.merge('RGBA',tuple(g.point([round(lo[c]+(hi[c]-lo[c])*i/255) for i in range(256)]) for c in range(3))+(im.getchannel('A').point(lambda x:round(x*alpha)),))
    return t

def place(im,asset,x,y,scale=1):
    if scale!=1:asset=asset.resize((round(asset.width*scale),round(asset.height*scale)),N)
    im.alpha_composite(asset,(round(x),round(y)))
def tile(im,asset,y=0):
    for x in range(0,im.width,asset.width):place(im,asset,x,y)
def base(lo,hi):
    im=Image.new('RGBA',(W,H));d=ImageDraw.Draw(im);a,b=color(lo),color(hi)
    for y in range(H):d.line((0,y,W,y),fill=mix(a,b,(y/H)**.8))
    return im

def save(im,name):
    path=OUT/(name+'.png');path.parent.mkdir(parents=True,exist_ok=True);im.save(path,optimize=True)
    m=Path(str(path)+'.meta')
    if not m.exists():m.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'gloombean/v6/'+name).hex+'\n',encoding='utf-8')
    manifest.append({'file':name+'.png','size':list(im.size),'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})

def vignette(im,strength=.14):
    # Edge framing uses palette steps, not a muddy blur over the playable plane.
    shade=Image.new('RGBA',(W,H));d=ImageDraw.Draw(shade)
    for i in range(0,50,2):
        a=round(255*strength*(1-i/50));d.rectangle((i,0,i+1,H),fill=(6,10,24,a));d.rectangle((W-2-i,0,W-1-i,H),fill=(6,10,24,a))
    return Image.alpha_composite(im,shade)

def pennants(im,y,bright=True):
    d=ImageDraw.Draw(im);palette=['e5ad69','ab596a','609793','b9bd83'] if bright else ['6c566c','716351','42666d']
    pts=[(x,round(y+12*math.sin(x/122))) for x in range(0,W+8,8)];d.line(pts,fill='#53566a',width=2)
    for i,x in enumerate(range(12,W,40)):
        yy=round(y+12*math.sin(x/122));d.polygon([(x,yy),(x+22,yy+2),(x+9,yy+24)],fill='#'+palette[i%len(palette)]);d.line([(x+2,yy+2),(x+10,yy+19)],fill='#'+('efd6a1' if bright else '887382'))

def glow(im,x,y,r,basecolor):
    layer=Image.new('RGBA',(W,H));d=ImageDraw.Draw(layer);c=color(basecolor)
    for rr in range(r,0,-3):d.ellipse((x-rr,y-rr,x+rr,y+rr),fill=(*c,round(20*(1-rr/max(1,r)))))
    im.alpha_composite(layer)

townbg=load('town/PNG/environment/layers/background.png')
mid=load('town/PNG/environment/layers/middleground.png')
houses=[load('town/PNG/environment/props-sliced/house-'+c+'.png') for c in 'abc']
castle=load('bridge/Props/castle.png').crop((0,0,316,256))
tree=load('bridge/Props/tree.png').crop((0,0,208,238))
old=load('collection/Old-dark-Castle-tileset-Files/PNG/old-dark-castle-interior-background.png')
night=load('collection/night-town-background-files/layers/night-town-background-town.png')
cloud=load('collection/night-town-background-files/layers/night-town-background-clouds.png')
# 0: bright, genuinely friendly opening. Background scenic houses are half the foreground scale.
im=base('a4c8ca','efe1bb');sc=remap(townbg,'73949e','f6e4b9');tile(im,sc.resize((768,576),N),-104)
place(im,remap(mid,'709695','b0ba96'),0,174,2)
for i,(x,y,s) in enumerate([(24,261,.75),(179,314,.6),(286,239,.9),(478,304,.65),(604,250,.8)]):
    place(im,remap(houses[i%3],'71828a','dfcc9a'),x,y,s)
d=ImageDraw.Draw(im);d.ellipse((571,42,623,94),fill='#f9e8a2');d.arc((580,53,614,88),5,160,fill='#c9b478',width=2)
pennants(im,124);pennants(im,171)
for x,y in [(70,390),(408,392),(564,421)]:d.rectangle((x,y,x+4,y+33),fill='#82947a');d.ellipse((x-7,y-4,x+10,y+10),fill='#bd8d8e')
save(vignette(im,.06),'background_0')
# 1: backstage laundry, mixed skyline and actual worn plaster/wood, warm light against desaturated plum.
im=base('151727','423144');tile(im,remap(cloud,'212535','585267',.65),62)
place(im,remap(night,'202a3b','6e6378'),0,299,1.5)
for x,y,s in [(-30,163,1.1),(531,130,1.3),(295,272,.6)]:place(im,remap(castle,'1b2332','676778'),x,y,s)
# Hanging fabrics and beams are architectural micro-stories, not giant masks.
d=ImageDraw.Draw(im)
for k,y in enumerate([98,175]):
    pts=[(x,round(y+19*math.sin(x/180+k))) for x in range(0,W+8,8)];d.line(pts,fill='#64535f',width=2)
    for i,x in enumerate(range(84+k*43,750,111)):
        yy=round(y+19*math.sin(x/180+k));c=['#545668','#665b6b','#47455d'][i%3]
        d.polygon([(x,yy),(x+25,yy+2),(x+32,yy+53),(x+16,yy+46),(x-4,yy+57)],fill=c);d.line((x+7,yy+5,x+13,yy+42),fill='#7f7780',width=2)
for x,y in [(87,293),(336,388),(628,262)]:glow(im,x,y,25,'cc985c');d.rectangle((x-2,y-5,x+2,y+4),fill='#cca171')
save(vignette(im),'background_1')
# 2: a deep orchard made from rooted trees and a distant inhabited chapel.
im=base('163238','546457');tile(im,remap(cloud,'2b4846','727965',.45),74)
place(im,remap(castle,'344c48','6e7360'),277,254,.75)
for i,(x,y,s) in enumerate([(-33,8,1.9),(97,168,1.15),(256,189,.95),(416,101,1.35),(612,-12,2.1)]):
    t=remap(tree,'102a30','4a6758') if i in (0,4) else remap(tree,'2d4845','71816a')
    place(im,t,x,y,s)
d=ImageDraw.Draw(im)
# Original small hanging pear silhouettes, each attached to a visible stem. These are background only.
for x,y in [(65,139),(180,230),(226,257),(437,175),(555,238),(705,182)]:
    d.line((x,y-30,x,y),fill='#617059',width=1);d.ellipse((x-7,y+3,x+7,y+18),fill='#6e7d59');d.ellipse((x-3,y-2,x+3,y+11),fill='#6e7d59');d.point((x-2,y+11),fill='#2b4440');d.point((x+3,y+10),fill='#2b4440')
save(vignette(im,.20),'background_2')
# 3: the fresco city has a stacked, vertically articulated silhouette, not an enlarged village wallpaper.
im=base('14293b','426374');tile(im,remap(cloud,'263d50','738187',.45),22)
for row in range(3):
    for i,x in enumerate(range(-40,810,119)):
        h=houses[(i+row)%3];place(im,remap(h,'263f50','637c84'),x,110+row*109-(i%2)*23,.7)
place(im,remap(castle,'193448','637b8b'),-153,79,1.9);place(im,remap(castle.transpose(Image.Transpose.FLIP_LEFT_RIGHT),'193448','6c8690'),491,54,1.9)
d=ImageDraw.Draw(im)
for x,y,r in [(232,76,31),(576,118,20)]:
    d.ellipse((x-r,y-r,x+r,y+r),outline='#929d95',width=2);d.ellipse((x-r+5,y-r+5,x+r-5,y+r-5),outline='#667f8b',width=1)
save(vignette(im,.10),'background_3')
# 4: a chasm, staggered architectural weight, small receding souls rather than repeated giant cartoons.
im=base('231c36','78505a');tile(im,remap(cloud,'302841','8b6b71',.45),29)
for x,y,s,a in [(-192,-5,1.8,17),(538,145,1.2,-13),(204,311,.52,21),(21,273,.68,-7),(371,384,.36,-20)]:
    aimg=remap(castle,'2b263c','786578').resize((round(castle.width*s),round(castle.height*s)),N).rotate(a,resample=N,expand=True)
    place(im,aimg,x,y)
d=ImageDraw.Draw(im);rng=random.Random(6064)
for i in range(42):
    x=rng.randrange(W);y=rng.randrange(H);scale=1 if i%3 else 2;c='#5b4156' if scale==1 else '#846574'
    d.ellipse((x,y,x+3*scale,y+4*scale),fill=c);d.line((x+scale,y+4*scale,x+3*scale,y+12*scale),fill=c,width=scale);d.line((x+scale,y+5*scale,x-3*scale,y+8*scale),fill=c,width=scale)
for x in [99,656]:
    for y in range(-6,H,15):d.ellipse((x,y,x+9,y+16),outline='#534257',width=2)
save(vignette(im,.16),'background_4')
# 5: stained-glass sanctuary with blue-black recesses; neither HUD nor platforms disappear into a white wash.
im=base('555668','c4b69b')
interior=remap(old.crop((0,0,480,304)),'55596a','e4d2a0')
# Preserve the original pixel grid: two articulated bays, not globally stretched Gothic clip art.
place(im,interior,0,43,1.6)
d=ImageDraw.Draw(im)
for x,y in [(177,63),(521,59),(367,222)]:
    d.ellipse((x-47,y-17,x+47,y+17),outline='#bda76c',width=3);d.ellipse((x-42,y-12,x+42,y+12),outline='#dbc58b',width=1)
save(vignette(im,.08),'background_5')
# 6: a genuinely different illuminated manuscript for Scripture; all text is decorative non-language.
im=base('c7b795','e2cfaa');d=ImageDraw.Draw(im);ink='#9e886c'
for x in [27,370,398,741]:d.line((x,0,x,H),fill='#b09b77',width=2)
for y in [18,39,441,462]:d.line((24,y,744,y),fill='#b09b77',width=2)
for x in [31,404]:
    # Elaborate original initial: eye surrounded by vine scrolls, subordinate to actual playable letterforms.
    d.rectangle((x+7,52,x+91,148),outline='#8e7660',width=3);d.rectangle((x+12,57,x+86,143),outline='#c0a66e',width=2)
    d.ellipse((x+26,76,x+71,121),outline='#8e7660',width=2);d.ellipse((x+42,85,x+57,114),fill='#a58d70')
    for k in range(5):d.arc((x+14+k*12,62,x+42+k*12,87),0,310,fill='#ad966d',width=2)
rng=random.Random(6070)
for col in [42,415]:
    for y in range(63,427,17):
        for x in range(col+(105 if y<155 else 0),col+295,12):
            yy=y+rng.randrange(-1,2);d.line((x,yy,x+2,yy-5,x+5,yy-6,x+6,yy+3,x+9,yy+1),fill=ink,width=1)
            if rng.random()<.35:d.line((x+4,yy-3,x+4,yy+6),fill=ink,width=1)
# Parchment midtone leaves semantic foreground letters, platforms and Hero free to carry the high contrast.
save(im,'background_6')
# Selected source textures remapped into the game's existing contact-surface vocabulary.
brick=load('town/PNG/environment/layers/sliced-tileset/ground-wall.png')
wood=load('town/PNG/environment/layers/sliced-tileset/top-wood.png')
for i,(lo,hi) in enumerate([('3a5144','cedc9b'),('27243d','c1a181'),('243a32','bfbc78'),('1c3347','c1d5d1'),('3a293d','d7b3a1'),('343e56','ecdb9c')]):
    source=wood if i in [1,2] else brick
    tex=remap(source,lo,hi).resize((64,64),N)
    save(tex,'fill_'+str(i))
    # A consistent dark edge and lit, real top surface. The underside retains the texture's actual chisel/wood marks.
    lip=Image.new('RGBA',(64,16));dd=ImageDraw.Draw(lip);dd.rectangle((0,0,63,15),fill='#'+lo)
    strip=tex.crop((0,0,64,16));lip.alpha_composite(strip)
    dd=ImageDraw.Draw(lip);dd.line((0,0,63,0),fill='#0f1626');dd.line((0,1,63,1),fill='#'+hi);dd.line((0,2,63,2),fill=(*mix(color(lo),color(hi),.7),255))
    if i==2:
        # Organic bark grain and small moss tufts, not brass cathedral trim on a tree branch.
        for x in range(1,64,7):dd.line((x,4,x+3,8,x+2,14),fill='#68704f');dd.line((x,3,x+3,3),fill='#bfbc78')
    save(lip,'lip_'+str(i))
report=ROOT/'Documentation/VisualV6/N1_ASSET_MANIFEST.json';report.write_text(json.dumps({'tool':'compose_visual_v6_n1.py','provenance':'Ansimuz CC0 components plus original arrangement/decoration; no Nintendo pixels and no generated screenshot evidence','assets':manifest},indent=2),encoding='utf-8')
print('COMPOSITION_PASS',len(manifest))
