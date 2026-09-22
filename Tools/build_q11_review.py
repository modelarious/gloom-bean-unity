# Reproducible ordinary HTML. All embedded game images are native backbuffers.
from pathlib import Path
import sys,json,hashlib,base64,io,html,argparse
ROOT=Path(__file__).resolve().parents[1]
sys.path[:0]=[str(ROOT/'.visual-tools'),r'C:\Users\micha\Projects\GloomBeanUnity\.visual-tools']
from PIL import Image
E=html.escape
ap=argparse.ArgumentParser();ap.add_argument('--before',type=Path,default=Path(r'C:\Users\micha\Projects\GloomBean QualityBar Q1\Reports\BroadVisual\Q10B'));args=ap.parse_args()
D=ROOT/'Documentation/QualityBarQ1/Q11Review';D.mkdir(parents=True,exist_ok=True)
current=ROOT/'Reports/BroadVisual/Q11B';before=args.before
shots=json.loads((current/'broad-result.json').read_text())['shots'];old=json.loads((before/'broad-result.json').read_text())['shots']
assert len(shots)==225 and {s['id'] for s in shots}=={s['id'] for s in old}
review=json.loads((D/'scores.json').read_text());native=json.loads((ROOT/'Reports/Orchard-v04/QualityBar-Q11B-regression/runner.json').read_text(encoding='utf-8-sig'))
package=json.loads((ROOT/'Reports/Delivery-WholeGame-Q11/runner.json').read_text(encoding='utf-8-sig'))
assert native['status']=='PASS' and len(native['results'])==16
assert package['status']=='PASS_NATIVE_RUNS_CAPTURE_REVIEW_PENDING'
cache={};image_count=0
def uri(path,game=False):
 key=(str(path),game)
 if key not in cache:
  im=Image.open(path).convert('RGB')
  if game:
   s=min(im.width//240,im.height//160);assert s>0;x=(im.width-240*s)//2;y=(im.height-160*s)//2
   im=im.crop((x,y,x+240*s,y+160*s)).resize((240,160),Image.Resampling.NEAREST)
  b=io.BytesIO();im.save(b,format='PNG',optimize=True);cache[key]='data:image/png;base64,'+base64.b64encode(b.getvalue()).decode()
 return cache[key]
def pic(path,caption,game=False):
 global image_count;image_count+=1
 return '<figure><img loading="lazy" src="'+uri(path,game)+'" alt="'+E(caption)+'"><figcaption>'+E(caption)+'</figcaption></figure>'
def para(text):return '<p>'+E(text)+'</p>'
refdir=ROOT/'Documentation/BroadVisual/References';extdir=ROOT/'Documentation/QualityBarQ1/ReferencesQ9'
refs=json.loads((refdir/'SOURCES.json').read_text())['files'];extra=json.loads((extdir/'SOURCES.json').read_text())['images'];refmap={r['file']:r for r in refs}
css='*{box-sizing:border-box}html{scroll-behavior:smooth}body{margin:0;background:#15131e;color:#f0e9db;font:16px/1.55 system-ui,sans-serif}main{max-width:1280px;margin:auto;padding:28px}h1{font-size:clamp(30px,5vw,52px);line-height:1.08;max-width:950px;margin:.4em 0}h2{font-size:28px;margin-top:45px}h3{font-size:22px;margin:.4em 0}h4{margin:20px 0 8px}.kicker{font-size:12px;letter-spacing:.12em;color:#c9a77a;text-transform:uppercase}a{color:#f5cb88}nav{display:flex;flex-wrap:wrap;gap:9px;margin:20px 0}nav a{padding:7px 12px;border:1px solid #665266;border-radius:5px;text-decoration:none;font-size:13px}.notice{background:#332336;border-left:4px solid #d07e9b;padding:16px 20px;margin:24px 0}.stats{display:grid;grid-template-columns:repeat(4,1fr);gap:12px;margin:24px 0}.stat{padding:14px;background:#242131}.stat strong{display:block;font-size:27px;color:#ffdca6}.stat span{font-size:13px;color:#ccc3d2}.stage{padding:22px;margin:28px 0;border:1px solid #574655;background:#1d1926;scroll-margin:16px}.stagehead{display:flex;justify-content:space-between;gap:16px;align-items:start}.grade{font-size:13px;color:#d6bfd3}.pair,.notes{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:16px}.grid{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:12px}figure{margin:0;background:#0b0a10}img{display:block;width:100%;height:auto;image-rendering:pixelated}figcaption{padding:8px 11px;font-size:12px;line-height:1.35;color:#d5ccd9}.muted,small{color:#b5a9bd}.deficit{color:#edbed0}details{border-top:1px solid #534052;margin-top:20px;padding-top:12px}summary{cursor:pointer;color:#ebc792;font-weight:650;margin-bottom:12px}table{width:100%;border-collapse:collapse;font-size:14px}td,th{text-align:left;vertical-align:top;padding:10px;border-bottom:1px solid #493c50}th{color:#dfb981}code{font-size:13px;overflow-wrap:anywhere}.refgrid{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:12px}.motion{border:1px solid #594959;padding:16px;margin:20px 0}video{display:block;width:100%;max-height:520px;background:black}footer{margin-top:40px;padding-top:18px;border-top:1px solid #534052;font-size:13px}@media(max-width:760px){main{padding:14px}.stage{padding:13px}.stats{grid-template-columns:repeat(2,1fr)}.pair,.notes{grid-template-columns:1fr}.grid,.refgrid{grid-template-columns:repeat(2,minmax(0,1fr))}.stagehead{display:block}.stage h3{font-size:19px}h2{font-size:24px}td,th{padding:7px;font-size:12px}}'
parts=['<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Gloom Bean Q11 — whole-game native review</title><style>'+css+'</style><body><main><header><div class="kicker">Native Unity / Q11 actor revision / 21 September 2026</div><h1>More expressive actors.<br>The whole game stays in view.</h1>',para('This is the actual built game, not a generated concept. Q11 retains the whole-stage materials and player-motion work and adds authored boss and patrol pose families driven by real state.'),'<div class="notice"><strong>The supplied concept-quality target is still not met.</strong> All25 stages remain, including sparse, repetitive and poorly framed examples. Coverage and passing tests do not certify extreme visual fidelity.</div><div class="stats">']
for n,t in [('25','all levels and bosses'),('225','current native views'),('62 + 6','WL4 gameplay + separate presentation'),('16 / 16','current native scenarios')]:parts.append('<div class="stat"><strong>'+n+'</strong><span>'+t+'</span></div>')
parts+=['</div><nav>',''.join('<a href="#world'+str(i)+'">'+n+'</a>' for i,n in enumerate(['Parish','Orchard','City','Fall','False Empyrean'],1)),'<a href="#motion">Actual motion</a><a href="#references">Reference corpus</a><a href="#evidence">Evidence</a></nav></header><section><h2>What actually changed</h2>',para('Five boss sheets give the Usher, Judge, Surveyor, falling congregation and final Host distinct faces and gestures. Ten patrol sheets separate each world and armour variant, with walking, stunned, carried and thrown poses. Walking follows distance travelled; boss gestures read the actual phase and attack state.'),para('The update is component-wide, including later-created off-camera actors, not a list of screenshot coordinates. Actorless areas intentionally retain Q10 environment art. A first-pass mass-size defect and obsolete decorative-eye overlap were corrected and recaptured.'),para('Grades are single-reviewer ordinal still-image judgements: style / character / clarity / polish, 0–5. Relevant WL4 references remain broadly4–5. No stage is a strict all-dimension win. Animation appeal and uncoached human/controller comprehension remain unverified.'),'</section>']
by={}
for s in shots:by.setdefault(s['stage'],[]).append(s)
for w in range(1,6):
 parts.append('<section id="world'+str(w)+'"><h2>'+['Parish','Orchard','City','Fall','False Empyrean'][w-1]+'</h2>')
 for stage in [f'GB-L{i:02}' for i in range(w*4-3,w*4+1)]+[f'GB-B{w}']:
  row=next(r for r in review['rows'] if r['stage']==stage);views=sorted(by[stage],key=lambda s:s['stratum']);hero=views[2]
  parts+=['<article class="stage" id="'+stage+'"><div class="stagehead"><div><div class="kicker">'+stage+'</div><h3>'+E(row['title'])+'</h3></div><div class="grade">S / C / C / P<br><strong>'+' / '.join(f'{x:g}' for x in row['current_grades'])+'</strong><br>WL4: '+' / '.join(f'{x:g}' for x in row['reference_grades'])+'<br><span class="deficit">Concept bar: unmet</span></div></div><div class="pair">',pic(before/(hero['id']+'.png'),'Q10 — same stage and retained anchor',True),pic(current/(hero['id']+'.png'),'Q11B — actual native capture',True),'</div><div class="notes"><div><h4>Visible strengths</h4>'+para(row['strength'])+'</div><div><h4 class="deficit">Still below target</h4>'+para(row['remaining_deficit'])+'</div></div><details><summary>All nine current views — weak examples retained</summary>'+para('A–I cover exploration, altered/return states and separated support samples. Art fixtures stage the body/form and camera; they are not route proofs. Moving supports and capture timing may shift coordinates while the named anchors remain unchanged.')+'<div class="grid">']
  for s in views:parts.append(pic(current/(s['id']+'.png'),s['id']+' | '+s['form']+' | '+('returned' if s['returned'] else 'exploration / encounter'),True))
  parts+=['</div></details><details><summary>All nine earlier Q10 views</summary><div class="grid">']
  for s in views:parts.append(pic(before/(s['id']+'.png'),'Q10 '+s['id'],True))
  parts+=['</div></details><details><summary>Relevant WL4 comparison examples</summary><div class="pair">']
  for name in row['reference_files']:parts.append(pic(refdir/name,refmap[name]['level']+' / Nintendo, comparison only'))
  parts+=['</div></details></article>']
 parts.append('</section>')
parts+=['<section id="motion"><h2>Actual input-driven motion</h2>',para('The exact packaged executable supplies these frames and clips. The read-only observer never changes actor, camera, physics or progression. It samples roughly every0.125seconds for up to240 observations. Videos preserve the recorded time spacing and are silent compressed derivatives, not full-frame-rate recordings. FinalHost opening imagery includes the distant noncolliding apparition, not necessarily the physical combat body.')]
for action in package['actions']:
 name=action['name'];folder=ROOT/'Reports/Delivery-WholeGame-Q11'/name/'MotionAction';ss=json.loads((folder/'native-action.json').read_text())['shots'];assert len(ss)==action['native_frames']
 parts+=['<article class="motion"><h3>'+E(name)+'</h3>'+para(str(action['checks'])+' route assertions; '+str(len(ss))+' captured frames; exit zero.')+'<div class="grid">']
 for i in [len(ss)//6,len(ss)//2,max(0,len(ss)-2)]:parts.append(pic(folder/ss[i]['file'],f"{ss[i]['time']:.2f}s | {ss[i]['form']} | {ss[i]['state']}",True))
 parts.append('</div>');video=D/'Motion'/(name+'.mp4')
 if video.exists():parts.append('<details><summary>Play native captured motion</summary><video controls preload="none" playsinline src="data:video/mp4;base64,'+base64.b64encode(video.read_bytes()).decode()+'"></video></details>')
 parts.append('</article>')
parts+=['</section><section id="references"><h2>The broader WL4 corpus stays visible</h2>',para('52 earlier broad references and ten later gameplay references remain. Decoded pixels were checked against the original nine-image set. Six cutscene/UI images are separated and are not environment gameplay scores. Nintendo pixels remain outside runtime Assets.'),'<details><summary>52 broad gameplay references</summary><div class="refgrid">']
for row in refs:parts.append('<div>'+pic(refdir/row['file'],row['file']+' — '+row['level'])+'<small><a href="'+E(row['url'],quote=True)+'">Original image</a></small></div>')
parts+=['</div></details><details><summary>10 additional gameplay references</summary><div class="refgrid">']
for row in extra:
 if row['category']=='gameplay-comparison':parts.append('<div>'+pic(extdir/row['file'],row['label'])+'<small><a href="'+E(row['url'],quote=True)+'">Original image</a></small></div>')
parts+=['</div></details><details><summary>6 presentation-only references — not gameplay scores</summary><div class="refgrid">']
for row in extra:
 if row['category']!='gameplay-comparison':parts.append(pic(extdir/row['file'],row['label']))
parts+=['</div></details></section><section id="evidence"><h2>Evidence without a false victory claim</h2><table><tr><th>Boundary</th><th>Observed result</th></tr>']
for title,desc in [('Current native suite','16/16 scenarios.801 mechanics/presentation assertions; zero unexpected failures/exceptions. Additional new boss routes are tested, not just the original opening.'),('Coverage','225 native views,25 stages. Existing175 anchors and50 newer separated H/I positions remain. Exact native pixel and no-added-visual-collider audits pass; this is not every possible frame.'),('Exact package','0/19/20-Mercy final-boss runs passed51/54/54 checks from unchanged genuinely earned earlier saves. This is not a relabelled fresh full-campaign test onQ11.'),('Actor integration','Real state and travel drive the view. Tests cover imported cells, Sliced-source bounds, disabled source/owner/view handling, off-camera late spawns and unchanged collider count.'),('Artistic result','New actor families and poses improve distinction. Repeated structures, empty spans and limited individual environment authorship remain below the supplied concept. Strict fidelity stays UNMET.')]:parts.append('<tr><td>'+title+'</td><td>'+desc+'</td></tr>')
parts+=['</table><h3>Exact identities</h3>'+para('Repository: modelarious/gloom-bean-unity · branch visual/qualitybar-q11')+'<p>Rendered runtime <code>'+E(review['current_runtime'])+'</code><br>Assembly <code>'+E(package['assembly_sha256'])+'</code><br>Frozen ZIP <code>'+E(package['archive_sha256'])+'</code></p><details><summary>Actual keyboard/UI and final ending backbuffers</summary><div class="grid">']
ui=ROOT/'Reports/Delivery-WholeGame-Q11/KeyboardUI';u=json.loads((ui/'ui-result.json').read_text(encoding='utf-8-sig'));assert len(u['screens'])==14 and u['isolated_save_has_no_progress']
for name in u['screens']:parts.append(pic(ui/(name+'.png'),'Actual keyboard: '+name,True))
for name in ['Ordinary','Nineteen','Restored']:parts.append(pic(ROOT/'Reports/Delivery-WholeGame-Q11/Endings'/name/'ending-screen.png','Actual earned ending: '+name,True))
parts+=['</div></details></section><footer>Gloom Bean source art and code coexist with separately credited CC0 environment components. Nintendo images are criticism only. No generated scene is native evidence. Human/controller approval is not claimed. The public source preserves original failures and a precise continuation checkpoint.</footer></main></body></html>']
out=D/'GloomBean_Q11_Visual_Review.html';out.write_text(''.join(parts),encoding='utf-8')
receipt={'format':'ordinary self-contained HTML','stages':25,'current_views':225,'before_views':225,'WL4_gameplay':62,'WL4_presentation_only':6,'embedded_image_instances':image_count,'source_runtime':review['current_runtime'],'sha256':hashlib.sha256(out.read_bytes()).hexdigest(),'bytes':out.stat().st_size,'strict_concept_fidelity':'UNMET','status':'BUILT_PENDING_BROWSER_AND_MOTION_REVIEW'}
(D/'REVIEW_IDENTITY.json').write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8');print('Q11_REVIEW_BUILT',json.dumps(receipt))
