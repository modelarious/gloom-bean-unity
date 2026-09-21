"""Build the actual native-frame review; no generated comparison artwork or font files.
Requires Pillow and ReportLab in the offline developer .visual-tools directory.
"""
from pathlib import Path
import sys,json,io,hashlib
ROOT=Path(__file__).resolve().parents[1];sys.path.insert(0,str(ROOT/'.visual-tools'))
from PIL import Image
from reportlab.pdfgen import canvas
from reportlab.lib.colors import HexColor
from reportlab.lib.utils import ImageReader
from reportlab.platypus import Paragraph
from reportlab.lib.styles import ParagraphStyle
D=ROOT/'Documentation/VisualV6';R=D/'Review';score=json.loads((R/'scores.json').read_text(encoding='utf-8'));v=json.loads((R/'verification.json').read_text(encoding='utf-8'))
assert v['packaged_status']=='PASS','Do not publish a pending package as a passed report'
OUT=R/'GloomBean_V6_GBA_Visual_Review.pdf';W,H=842,595
c=canvas.Canvas(str(OUT),pagesize=(W,H),pageCompression=1);c.setTitle('Gloom Bean V6 / Native GBA Visual Review');c.setAuthor('Gloom Bean development / comparative visual review')
colors={'paper':'F7F4EB','ink':'261F30','muted':'696174','plum':'60406B','line':'CDC5BA','soft':'EAE5DD','warn':'AC305E'}
colors={k:'#'+v for k,v in colors.items()}
styles={n:ParagraphStyle('s'+str(n),fontName='Helvetica',fontSize=n,leading=n*1.32,textColor=HexColor(colors['ink'])) for n in [7,8,9,10,11,12]}
page=0
def rect(x,y,w,h,col):c.setFillColor(HexColor(colors[col]));c.rect(x,y,w,h,fill=1,stroke=0)
def text(x,y,t,size=10,bold=False,col='ink'):
 c.setFillColor(HexColor(colors[col]));c.setFont('Helvetica-Bold' if bold else 'Helvetica',size);c.drawString(x,y,t)
def para(x,top,w,t,size=9):
 q=Paragraph(t,styles[size]);_,h=q.wrap(w,900);q.drawOn(c,x,top-h);return h
def start(label,title,subtitle=''):
 global page;page+=1;rect(0,0,W,H,'paper');rect(0,H-7,W,7,'plum');text(30,H-30,label.upper(),8,True,'plum');text(30,H-61,title,24,True)
 if subtitle:para(30,H-74,782,subtitle,9)
def end():
 rect(30,27,782,.5,'line');text(30,15,'GLOOM BEAN / V6 GBA VISUAL REVIEW / 21 SEPTEMBER 2026',7,col='muted');text(782,15,str(page),8,True,'plum');c.showPage()
def native(path):
 im=Image.open(path).convert('RGB');s=min(im.width//240,im.height//160);x=(im.width-240*s)//2;y=(im.height-160*s)//2;return im.crop((x,y,x+240*s,y+160*s)).resize((240,160),Image.Resampling.NEAREST)
def pic(path,x,y,w,h,logical=False):
 im=native(path) if logical else Image.open(path).convert('RGB');rect(x,y,w,h,'soft');scale=min(w/im.width,h/im.height);tw,th=im.width*scale,im.height*scale;c.drawImage(ImageReader(im),x+(w-tw)/2,y+(h-th)/2,tw,th,mask='auto')
P=ROOT/'Reports/VisualV6/gba_p7';B=ROOT/'Reports/VisualV6/pass2_n1';REF=D/'References';shots=json.loads((P/'visual-result.json').read_text(encoding='utf-8'))['shots'][:10]
start('Actual native development / criticism, not a victory certificate','A GBA-sized game, not a resized mockup.','Eight committed presentation rounds. Ten preselected comparisons. The strict Wario Land 4 superiority target remains open.')
pic(B/'01-cute-parade.png',30,267,380,225);pic(P/'GBA-01-cute-parade.png',432,267,380,225,True);text(30,254,'PRE-GBA V6',8,True,'muted');text(432,254,'CURRENT SCORED WORLD FRAME / P7',8,True,'plum')
for x,big,small in [(30,'240 x 160','native world and bitmap UI'),(296,'10','preselected pairs retained'),(562,'0 / 10','strict four-dimension sweeps')]:rect(x,196,250,42,'soft');text(x+8,216,big,18,True,'plum');text(x+8,203,small,8,col='muted')
para(30,174,369,'The strongest changes are a real low-resolution framebuffer, exact integer pixel blocks, consistent lettering, readable state, distinctive cure objects and stronger scenic framing. The work is in the playable native build.',11)
para(432,174,380,'The benchmark still leads. More coherent rendering has not supplied the hand-authored foreground composition, expressive character posing and distinctive moving machinery that these comparisons demand.',11);text(30,43,'PUBLIC SOURCE: modelarious/gloom-bean-unity / visual/v6',8,col='muted');end()
start('Method / stable sample','How these comparisons were made.','The grades are one reviewer\'s ordinal still-image judgements, not an objective instrument or a human panel.')
notes=[('KEEP THE SAMPLE','The ten stage/reference pairs were fixed before this continuation. The original camera centres and lenses remain alongside explicitly labelled tighter GBA views. The image aspect changes to3:2; old lens does not imply an identical crop.'),('USE REAL PIXELS','The images are native Unity backbuffers or attributed real WL4 screenshots. Nintendo images are comparison-only outside Assets. No generated poster or repainted screenshot counts as implementation.'),('SEPARATE EVIDENCE','Art fixtures stage a body/form and camera. Actual input-driven action and complete earned-save route tests are different evidence. A screenshot count is not proof of quality or playability.'),('DO NOT OVERGRADE','Still images cannot certify animation feel, camera comfort, human comprehension or horror effectiveness. A more complex image is not automatically better.'),('TIES ARE NOT WINS','Every dimension must be strictly ahead in every selected pair. One lost dimension keeps the strict gate open. No difficult example was removed.')]
y=487
for h,t in notes:text(30,y,h,10,True,'plum');para(30,y-12,770,t,10);y-=78
text(30,78,'SCORE ANCHORS',9,True,'plum');para(30,66,770,'0 missing/broken / 1 placeholder / 2 readable but crude / 3 coherent development art / 4 polished professional / 5 exceptional authored example. Half-point increments; means are not percentage improvements.',8);end()
start('Comparison ledger','The benchmark is still ahead.','Order: style / character / clarity / polish. Means summarize ordinal judgements and are not laboratory measurements.')
x=[30,235,437,637,771];y=480
for xx,t in zip(x,['SCENE','PRE-GBA V6','CURRENT P7','WL4','VERDICT']):text(xx,y,t,8,True,'plum')
y-=27
for i,row in enumerate(score['rows']):
 if i%2==0:rect(26,y-13,790,34,'soft')
 text(30,y,str(i+1).zfill(2)+' '+row['level'],8,True)
 for xx,key in zip(x[1:4],['pre_gba_scores','current_scores','wl4_scores']):text(xx,y,' / '.join(f'{n:g}' for n in row[key]),8)
 text(772,y,'WL4',7,True,'warn');y-=35
rect(26,60,790,42,'soft');text(34,79,'MEAN OF40 GRADES',9,True)
for xx,key in zip(x[1:4],['pre_gba_mean','current_mean','wl4_mean']):text(xx,76,f"{score['summary'][key]:.1f} / 5",14,True,'plum')
text(756,77,'UNMET',8,True,'warn');end()
start('Iteration history','Every pass exposed the next weakness.','Builds, original failures, screenshots, scripts and source identities are retained. New does not automatically mean better.')
items=[('P1','One real pixel system','Native240x160 framebuffer, original bitmap HUD/menu text, integer display and stronger contact edges.'),('P2','Objects and state','Distinct parade fronts, cure objects, plate/lever state, cathedral interior and conserved-form status.'),('P3','Repair observed failures','Restore missing world signs; correct clipped F1 text, menu footer and unpainted outer borders.'),('P4','Frame complete compositions','Reveal the full orchard canopy, fallen city, rose windows and illuminated page instead of giant cropped fragments.'),('P5','Material and character','Interlocking falling bodies; asymmetrical final entity; more explicit ivory/gold/violet architecture.'),('P6','Contain the apparition','Keep the distant noncolliding manifestation legible inside the unchanged view; actual boss collider untouched.'),('P7','Preserve landing space','Move the form card away from the floor. Add read-only backbuffer observation during actual input-driven action.'),('P8','Finish the ending text','Actual package screens exposed clipped final sentences. Short display copy now fits; the restoration rule is unchanged.')]
y=482
for label,h,t in items:rect(30,y-28,40,34,'plum');c.setFillColor(HexColor('#FFFFFF'));c.setFont('Helvetica-Bold',10);c.drawString(39,y-13,label);text(84,y,h,10,True);para(84,y-11,720,t,8);y-=52
end()
for row,shot in zip(score['rows'],shots):
 start('Pair '+str(page-3).zfill(2)+' / 10 / '+row['reference_level'],row['level'],'Current world image: native P7, labelled GBA framing. The original wide-lens witness remains below. P8 changes ending text only.')
 pic(REF/(shot['reference']+'.png'),30,267,381,224);pic(P/('GBA-'+shot['id']+'.png'),431,267,381,224,True)
 text(30,257,'WARIO LAND 4 / '+row['reference_level'].upper(),8,True,'muted');text(431,257,'GLOOM BEAN / NATIVE P7',8,True,'plum')
 for xx,grades in [(30,row['wl4_scores']),(431,row['current_scores'])]:
  rect(xx,219,381,30,'soft')
  for j,(name,value) in enumerate(zip(['STYLE','CHARACTER','CLARITY','POLISH'],grades)):text(xx+7+j*95,239,name,6,True,'muted');text(xx+7+j*95,226,f'{value:g}/5',10,True,'plum')
 pic(B/(shot['id']+'.png'),30,94,181,114);pic(P/(shot['id']+'.png'),228,94,183,114,True);text(30,82,'PRE-GBA / SAME SCENE',7,True,'muted');text(228,82,'P7 / ORIGINAL LENS RETAINED',7,True,'muted')
 y=210
 for label,key in [('Improved.','gloom_improvement'),('Still behind.','remaining_wl4_advantage'),('Next work.','next_art_work')]:y-=para(431,y,381,'<b>'+label+'</b> '+row[key],8)+9
 text(30,54,'VERDICT: NO FOUR-CRITERION SWEEP',9,True,'warn');para(30,43,782,'Nintendo reference: comparative criticism only, never runtime art. P7 images are fixed-camera fixtures; input-driven action is separate.',7);end()
start('Actual-action supplement','Staged art fixtures are not gameplay proof.','These frames were captured during production-input witnesses, with normal simulation and enabled camera follow. No actor or time was staged by the observer.')
actions=[('Reports/V6-GBA-Packaged-P7/Rain-action/NativeAction/03-GB-L13.png','Actual jump; field contracts.'),('Reports/V6-GBA-Packaged-P7/Rain-action/NativeAction/16-GB-L13.png','Falling figures become supports.'),('Reports/V6-GBA-Packaged-P7-R3/Ordinary/NativeAction/07-GB-B5.png','Real final-boss adaptation.'),('Reports/V6-GBA-Packaged-P7-R3/Ordinary/NativeAction/10-GB-B5.png','Real final combination circuit.')]
for i,(path,caption) in enumerate(actions):
 x=30+(i%2)*401;y=292-(i//2)*210;pic(ROOT/path,x,y,300,200,True);para(x+310,y+183,72,caption,8)
end()
start('Technical result / explicit scopes','Correct pixels are necessary, not sufficient.','This is a Unity Windows development game with a GBA-style presentation, not a GBA ROM or a hardware-emulation claim.')
left=[('240 x 160','Actual point-filtered render target and bitmap UI; no mixed-resolution gameplay heading.'),('Integer scaling','1280x800 produces1200x800 at5x; an actual1274x783 client uses960x640 at4x with letterboxing.'),('Failing controls','The auditor rejects a changed subpixel, blurred pixels and an empty frame; valid odd-size client passes.'),('Protected logic','154 protected runtime sources, MovementTuning, Boot, Packages and ProjectSettings remain unchanged. Reviewed rendering/test edits and actual gameplay are separate evidence.')]
y=480
for h,t in left:text(30,y,h,11,True,'plum');y-=15;y-=para(30,y,350,t,10)+20
text(431,480,'VERIFICATION OUTCOMES',11,True,'plum');y=458
for item in v['test_summary']:text(431,y,item['name'],10,True);y-=13;y-=para(431,y,380,item['result'],9)+16
para(30,73,782,'P4\'s complete54-case run belongs to its pinned source. P7 rendering has a separate focused suite and frozen-package runs; P8 is an ending-copy-only correction with its own exact-package ending proof. No earlier certificate is silently relabelled.',8);end()
start('Open visual target','The next work is authorship, not resolution.','The all-example superiority gate remains open. These improvements are specified without changing the original game requirements.')
items=[('Foreground form and machinery','Author actual laundry cars, weight-bearing branches, furnished rooms, articulated joints and slab/catch apparatus. Preserve the real collision boundaries.'),('Expressive action poses','Build silhouettes for anticipation, impact, recovery and transformations. Review real motion, not a bigger portrait.'),('Visible game rules','Make cures, sources, material state and force relationships readable without depending on memorized coloured labels.'),('Finish and re-score the same sample','Keep all ten pairs. Grade the weakest frames and actual action, not just flattering captures. Retain every loss until the art genuinely improves.')]
y=477
for i,(h,t) in enumerate(items):rect(30,y-22,36,32,'plum');c.setFillColor(HexColor('#FFFFFF'));c.setFont('Helvetica-Bold',12);c.drawString(42,y-9,str(i+1));text(83,y,h,12,True);para(83,y-16,721,t,11);y-=83
rect(30,42,782,59,'soft');para(41,87,760,'<b>Verified:</b> source publication, native GBA presentation, captured improvements and scoped software regression. <b>Not verified:</b> universal visual superiority, human controller feel, uncoached readability, animation quality, audio balance or horror effectiveness.',10);end()
start('Provenance / no false originality','What is original, borrowed and only compared.','Runtime source, artwork provenance, reference images and grade data are committed separately so the comparison remains auditable.')
para(30,480,373,'<b>Original project work.</b> Player reference adaptation, possession features, bitmap alphabet, UI, cure objects, parade fronts, cathedral composition, falling-crowd/final-entity revision, code and authored interactions. Player animation remains procedural; no new hand-authored animation library is claimed.<br/><br/><b>Credited CC0 components.</b> Selected environment fragments by Luis Zuno / Ansimuz. Author pages and archive hashes are retained in ArtSources/Ansimuz. Composed adaptations are not falsely described as wholly original source art. No third-party music, game code or character assets were imported.<br/><br/><b>Comparison only.</b> Nintendo WL4 screenshots remain outside Assets. Exact filenames, URLs and hashes are retained in References/SOURCES.json.',10)
text(432,480,'EXACT DELIVERY SCOPE',11,True,'plum');y=458
for h,t in [('Public repository','modelarious/gloom-bean-unity'),('Review branch','visual/v6'),('Scored world runtime',score['current_source']),('Final preview runtime',v['final_runtime']),('Full regression candidate',v['full_candidate']),('Final assembly SHA256',v['final_assembly']),('Strict superiority','UNMET / 0 OF 10 PAIRS')]:text(432,y,h,8,True,'muted');y-=12;y-=para(432,y,380,t,8)+13
para(30,115,376,'Author source pages: opengameart.org/content/gothicvania-town; gothicvania-patreons-collection; gothicvania-church-pack. Nintendo GBASP hardware documentation specifies a240x160 screen. This project does not claim the original hardware\'s memory or scanline constraints.',8)
para(432,93,380,'Reproduction: native fixtures and source provenance, ordinal grade JSON, side-by-side boards, this builder and the complete authored game history are preserved in Git. Human acceptance is not manufactured by a passing script.',8);end()
c.save();print('GBA_REVIEW_PDF_BUILT',page,OUT.stat().st_size,hashlib.sha256(OUT.read_bytes()).hexdigest())
