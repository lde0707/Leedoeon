from pathlib import Path
from playwright.sync_api import sync_playwright
root=Path.cwd()
with sync_playwright() as p:
 b=p.chromium.launch(channel='chrome',headless=True)
 for width in (1440,390):
  page=b.new_page(viewport={'width':width,'height':1000})
  page.goto((root/'qa/index.html').as_uri()+'#project-ai-shorts',wait_until='domcontentloaded')
  page.evaluate('document.fonts.ready')
  page.locator('#introword').wait_for(state='detached')
  page.wait_for_function("document.querySelector('#blinds').classList.contains('done')")
  section=page.locator('#ov .shorts-channel-feature')
  image=section.locator('img'); image.scroll_into_view_if_needed(); image.evaluate('e=>e.decode()')
  assert image.evaluate('e=>e.naturalWidth===1897')
  assert section.evaluate('e=>e.parentElement.querySelector("section")===e')
  assert section.locator('a').get_attribute('href')=='https://www.youtube.com/channel/UCXEhq6pza56xGDjwuZDQFSA'
  assert section.evaluate('e=>e.scrollWidth<=e.clientWidth')
  section.screenshot(path=str(root/'outputs'/f'shorts-channel-detail-top-{width}.png'))
  section.locator('button').click()
  assert page.locator('#photo-viewer').evaluate('e=>e.open')
  page.keyboard.press('Escape')
  assert not page.locator('#photo-viewer').evaluate('e=>e.open')
  assert page.locator('#ov .shorts-flow li').count()==8
  print(f'{width}: image loaded, first detail section, channel link, zoom, 8 stages PASS')
  page.close()
 b.close()

