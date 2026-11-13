from pathlib import Path
from playwright.sync_api import sync_playwright
import json
root=Path.cwd();channel='https://www.youtube.com/channel/UCXEhq6pza56xGDjwuZDQFSA'
expected=['데굴데굴 퍼즐','진주도둑들','AI Shorts 자동화','StillDrawn','Rushitadel','쓰레기 집게 소년','접힌 빛','GreenWay','FlyEditor']
with sync_playwright() as p:
 b=p.chromium.launch(channel='chrome',headless=True)
 for width in [1440,390]:
  page=b.new_page(viewport={'width':width,'height':1000},reduced_motion='reduce')
  page.goto((root/'qa/index.html').as_uri(),wait_until='domcontentloaded');page.evaluate('document.fonts.ready')
  names=page.locator('.project-copy h3').all_text_contents();assert names==expected,names
  assert page.locator('.project-item').count()==9
  card=page.locator('.project-item[data-project-slug="ai-shorts"]')
  card.locator('.project-copy p').first.click()
  assert page.locator('#ov').get_attribute('aria-hidden')=='false'
  assert page.url.endswith('#project-ai-shorts')
  assert page.locator('#ov .shorts-flow li').count()==8
  assert page.locator('#ov a').first.get_attribute('href')==channel
  images=page.locator('#ov .shorts-overview img')
  for img in images.all():
   img.scroll_into_view_if_needed();img.evaluate('e=>e.decode()');assert img.evaluate('e=>e.naturalWidth>0')
  page.locator('#ov').evaluate('e=>e.scrollTop=0')
  page.screenshot(path=str(root/'outputs'/f'shorts-detail-{width}.png'))
  page.locator('#ov .shorts-flow').scroll_into_view_if_needed()
  assert page.locator('#ov .shorts-flow').evaluate('e=>e.scrollWidth<=e.clientWidth')
  page.screenshot(path=str(root/'outputs'/f'shorts-flow-{width}.png'))
  page.locator('#ov .zoom-photo').first.click()
  assert page.locator('#photo-viewer').evaluate('e=>e.open')
  page.keyboard.press('Escape');assert not page.locator('#photo-viewer').evaluate('e=>e.open')
  page.locator('#ovClose').click();assert page.locator('#ov').get_attribute('aria-hidden')=='true'
  page.locator('#projects .project-grid').scroll_into_view_if_needed()
  if width==1440:page.locator('#projects .project-grid').screenshot(path=str(root/'outputs'/'shorts-project-grid.png'))
  for slug,title in [('stilldrawn','StillDrawn'),('trash-boy','쓰레기 집게 소년')]:
   page.goto((root/'qa/index.html').as_uri()+'#project-'+slug,wait_until='domcontentloaded')
   assert page.locator('#ov').get_attribute('aria-hidden')=='false'
   assert title.casefold() in page.locator('#ov').inner_text().casefold()
  print(json.dumps({'width':width,'order':names,'steps':8,'images':2,'lightbox':'pass','deep_links':'pass'},ensure_ascii=True))
  page.close()
 b.close()
