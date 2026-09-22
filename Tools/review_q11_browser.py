from pathlib import Path
import sys,json,hashlib,traceback
sys.path.insert(0,r'C:\ProgramData\SentinelX\workspace\q11-browser-tools')
from playwright.sync_api import sync_playwright
root=Path(__file__).resolve().parents[1];pagefile=root/'Documentation/QualityBarQ1/Q11Review/GloomBean_Q11_Visual_Review.html';out=root/'Reports/Q11-browser-reproducible';out.mkdir(exist_ok=True)
result={'status':'RUNNING','artifact_sha256':hashlib.sha256(pagefile.read_bytes()).hexdigest()}
try:
 with sync_playwright() as pw:
  browser=pw.chromium.launch(executable_path=r'C:\Program Files\Google\Chrome\Application\chrome.exe',headless=True,chromium_sandbox=True)
  context=browser.new_context(viewport={'width':1440,'height':1000});page=context.new_page();errors=[];page.on('pageerror',lambda e:errors.append(str(e)))
  page.goto(pagefile.as_uri(),wait_until='load',timeout=25000)
  page.evaluate("document.querySelectorAll('img').forEach(i=>i.loading='eager')")
  page.wait_for_function("Array.from(document.images).every(i=>i.complete)",timeout=20000)
  result['desktop']=page.evaluate("({images:document.images.length,broken:[...document.images].filter(i=>!i.naturalWidth).length,stages:document.querySelectorAll('article.stage').length,videos:document.querySelectorAll('video').length,overflow:document.documentElement.scrollWidth>innerWidth,externalImages:[...document.images].filter(i=>!i.src.startsWith('data:')).length})")
  page.screenshot(path=str(out/'desktop-top.png'));page.locator('#GB-L09').scroll_into_view_if_needed();page.screenshot(path=str(out/'desktop-city.png'))
  result['videos']=[]
  for i in range(page.locator('video').count()):
   video=page.locator('video').nth(i);video.evaluate("v=>{v.closest('details').open=true;v.muted=true;v.load()}")
   page.wait_for_function('(i)=>document.querySelectorAll("video")[i].readyState>=2',arg=i,timeout=10000)
   video.evaluate("v=>{v.currentTime=2;v.play()}");page.wait_for_timeout(450)
   v=video.evaluate("v=>({width:v.videoWidth,height:v.videoHeight,time:v.currentTime,ready:v.readyState,error:v.error?v.error.code:null})")
   assert v['width']==480 and v['height']==320 and v['time']>2.1 and not v['error'];v['index']=i;result['videos'].append(v);video.evaluate('v=>v.pause()')
  page.set_viewport_size({'width':390,'height':844});page.evaluate('scrollTo(0,0)');page.screenshot(path=str(out/'mobile-top.png'))
  result['mobile']=page.evaluate("({overflow:document.documentElement.scrollWidth>innerWidth,width:innerWidth,documentWidth:document.documentElement.scrollWidth})")
  page.locator('#GB-B4').scroll_into_view_if_needed();page.screenshot(path=str(out/'mobile-boss.png'))
  result['errors']=errors
  assert result['desktop']['stages']==25 and result['desktop']['broken']==0 and result['desktop']['externalImages']==0 and not result['desktop']['overflow'] and not result['mobile']['overflow']
  assert len(result['videos'])==10 and result['desktop']['images']==663 and not errors
  result['status']='PASS';browser.close()
except Exception as exc:
 result['status']='FAIL';result['error']=str(exc);result['traceback']=traceback.format_exc()
(out/'browser-result.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print('Q11_BROWSER_QA_'+result['status'])

if result["status"]!="PASS":raise SystemExit(1)
