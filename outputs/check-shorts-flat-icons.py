from pathlib import Path
from playwright.sync_api import sync_playwright
import json
root=Path.cwd()
expected=['대본 생성','대본 검수','영상 생성','영상 편집','자막 생성','TTS 적용','최종 검수','자동 업로드']
with sync_playwright() as p:
 b=p.chromium.launch(channel='chrome',headless=True)
 for width in [1440,768,390]:
  page=b.new_page(viewport={'width':width,'height':1050},reduced_motion='reduce')
  page.goto((root/'qa/index.html').as_uri()+'#project-ai-shorts',wait_until='domcontentloaded');page.evaluate('document.fonts.ready')
  assert page.locator('#ov .shorts-flow h5').all_text_contents()==expected
  icons=page.locator('#ov .shorts-step-icon');assert icons.count()==8
  assert len(set(icons.evaluate_all('es=>es.map(e=>e.src)')))==8
  for icon in icons.all():
   icon.scroll_into_view_if_needed();icon.evaluate('e=>e.decode()');assert icon.evaluate('e=>e.naturalWidth>0')
  assert page.locator('#ov .shorts-flow').evaluate('e=>e.scrollWidth<=e.clientWidth')
  for paragraph in page.locator('#ov .shorts-flow li p').all():
   assert paragraph.evaluate('e=>e.getBoundingClientRect().height/parseFloat(getComputedStyle(e).lineHeight)<=3.1')
  page.locator('#ov .shorts-flow').scroll_into_view_if_needed()
  page.screenshot(path=str(root/'outputs'/f'shorts-flat-flow-{width}.png'))
  if width==1440:page.locator('#ov .shorts-flow').screenshot(path=str(root/'outputs'/'shorts-flat-pipeline.png'))
  page.locator('#ov .zoom-photo').click();assert page.locator('#photo-viewer').evaluate('e=>e.open')
  page.keyboard.press('Escape');page.locator('#ovClose').click()
  card=page.locator('.project-item[data-project-slug="ai-shorts"]');assert card.locator('.shorts-cover-preview img').count()==3
  card.scroll_into_view_if_needed()
  for img in card.locator('.shorts-cover-preview img').all():img.evaluate('e=>e.decode()')
  card.screenshot(path=str(root/'outputs'/f'shorts-flat-card-{width}.png'))
  card.locator('.shorts-cover-preview').click();assert page.locator('#ov').get_attribute('aria-hidden')=='false'
  print(json.dumps({'width':width,'icons':8,'steps':'ordered','paragraphs':'at most 3 lines','preview':'3 icons','channel_lightbox':'pass'},ensure_ascii=True))
  page.close()
 b.close()
