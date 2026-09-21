"""Audit the actual native screenshot pixel grid, with controls that must fail.
Pillow is an offline QA dependency only. No game assets or screenshots are altered.
"""
from __future__ import annotations
import argparse,hashlib,json,sys,tempfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image,ImageChops

def measure(path:Path)->dict:
    im=Image.open(path).convert('RGB');w,h=im.size;scale=min(w//240,h//160)
    if scale<1:raise ValueError('Screenshot is smaller than the logical framebuffer')
    x=(w-240*scale)//2;y=(h-160*scale)//2;viewport=im.crop((x,y,x+240*scale,y+160*scale))
    logical=viewport.resize((240,160),Image.Resampling.NEAREST)
    rebuilt=logical.resize(viewport.size,Image.Resampling.NEAREST)
    differences=ImageChops.difference(viewport,rebuilt)
    colors=len(logical.getcolors(240*160) or [])
    return {'name':path.name,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'screen':[w,h],'logical':[240,160],'scale':scale,'viewport':[x,y,240*scale,160*scale],'mixed_pixel_blocks':differences.getbbox() is not None,'distinct_logical_colors':colors,'pass':differences.getbbox() is None and colors>=16}

def controls()->list[str]:
    with tempfile.TemporaryDirectory() as d:
        d=Path(d);im=Image.new('RGB',(240,160));im.putdata([((x//8)*7%256,(y//8)*11%256,64) for y in range(160) for x in range(240)])
        good=Image.new('RGB',(1280,800));good.paste(im.resize((1200,800),Image.Resampling.NEAREST),(40,0));good.save(d/'good.png')
        assert measure(d/'good.png')['pass'],'Positive control rejected'
        damaged=good.copy();damaged.putpixel((42,8),(255,255,255));damaged.save(d/'damaged.png');assert not measure(d/'damaged.png')['pass'],'Single subpixel mutation escaped'
        black=Image.new('RGB',(1280,800));black.save(d/'black.png');assert not measure(d/'black.png')['pass'],'Empty frame passed'
        smooth=Image.new('RGB',(1280,800));smooth.paste(im.resize((1200,800),Image.Resampling.BILINEAR),(40,0));smooth.save(d/'smooth.png');assert not measure(d/'smooth.png')['pass'],'Filtered pixels passed'
        # Real client sizes that are not exact multiples remain integer-scaled and letterboxed.
        odd=Image.new('RGB',(1274,783));odd.paste(im.resize((960,640),Image.Resampling.NEAREST),(157,71));odd.save(d/'odd.png');assert measure(d/'odd.png')['pass'],'Odd-size viewport rejected'
    return ['positive native frame','single changed subpixel rejected','empty frame rejected','bilinear frame rejected','odd client-size integer viewport']

def main()->int:
    ap=argparse.ArgumentParser();ap.add_argument('directory',nargs='?',type=Path);ap.add_argument('--ui',action='store_true');ap.add_argument('--self-test',action='store_true');ap.add_argument('--compare-geometry',type=Path);args=ap.parse_args()
    tests=controls()
    if args.self_test and not args.directory:print('GBA_PIXEL_CONTROLS_PASS',len(tests));return 0
    if not args.directory:ap.error('A captured report directory is required')
    root=args.directory.resolve();receipt=root/('ui-result.json' if args.ui else 'visual-result.json');meta=json.loads(receipt.read_text(encoding='utf-8-sig'))
    names=[n+'.png' for n in meta['screens']] if args.ui else [s['id']+'.png' for s in meta['shots']]
    if not names or len(names)!=len(set(names)):raise ValueError('Missing/duplicated declared images')
    rows=[measure(root/n) for n in names];geometry=[]
    if args.compare_geometry:
        for file in sorted(args.compare_geometry.glob('*-geometry.txt')):
            if file.name.startswith('GBA-'):continue
            other=root/file.name
            def lines(f):return sorted(f.read_text(encoding='utf-8-sig').splitlines())
            same=other.exists() and lines(file)==lines(other)
            geometry.append({'file':file.name,'same':same,'colliders':len(lines(file))})
        if len(geometry)!=10:raise ValueError('Expected the full ten legacy geometry inventories')
    result={'status':'PASS' if all(r['pass'] for r in rows) and all(g['same'] for g in geometry) else 'FAIL','scope':'Exact integer pixel blocks/nonempty actual screenshots and optional collider name/type/layer/trigger census. Not an art ranking, collider-bounds proof or gameplay certificate.','controls':tests,'images':rows,'geometry':geometry}
    (root/'gba-pixel-audit.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    if result['status']!='PASS':print(json.dumps(result,indent=2));return 1
    print('GBA_PIXEL_AUDIT_PASS',len(rows),'images;',len(geometry),'unchanged inventories;',len(tests),'oracle controls');return 0
if __name__=='__main__':raise SystemExit(main())
