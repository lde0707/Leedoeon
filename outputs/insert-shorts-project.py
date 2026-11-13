from pathlib import Path
from html.parser import HTMLParser
import re
p=Path('qa/index.html');s=p.read_text(encoding='utf-8')
class Cards(HTMLParser):
 def __init__(self,text):
  super().__init__();self.offsets=[0]+[m.end() for m in re.finditer('\n',text)];self.depth=0;self.start=None;self.cards=[]
 def pos(self):
  line,col=self.getpos();return self.offsets[line-1]+col
 def handle_starttag(self,tag,attrs):
  if tag=='article':
   if self.start is not None:self.depth+=1
   elif dict(attrs).get('class')=='project-item':self.start=self.pos();self.depth=1
 def handle_endtag(self,tag):
  if tag=='article' and self.start is not None:
   self.depth-=1
   if self.depth==0:self.cards.append((self.start,self.pos()+len('</article>')));self.start=None
parser=Cards(s);parser.feed(s);assert len(parser.cards)==9
cards=[s[a:b] for a,b in parser.cards]
names=[re.search(r'<h3>(.*?)</h3>',c).group(1) for c in cards]
assert names==['데굴데굴 퍼즐','진주도둑들','쓰레기 집게 소년','Rushitadel','StillDrawn','접힌 빛','Lucky Dice','GreenWay','FlyEditor'],names
steps=[('대본 생성','게임 지식 주제를 영상용 대본으로 생성합니다.'),('대본 검수','생성된 대본을 검수하고 영상 제작으로 넘깁니다.'),('영상 생성','대본에 맞는 장면과 영상 소스를 생성합니다.'),('영상 편집','장면을 연결하고 Shorts 형식으로 편집합니다.'),('자막 생성','대본 내용을 화면에 읽기 쉬운 자막으로 만듭니다.'),('TTS 적용','대본을 음성으로 변환해 영상에 적용합니다.'),('최종 검수','영상·자막·음성이 합쳐진 결과물을 검수합니다.'),('자동 업로드','검수된 결과물을 YouTube 채널에 업로드합니다.')]
flow=''.join(f'<li><span class="shorts-step-num">{i:02d}</span><h5>{title}</h5><p>{body}</p></li>' for i,(title,body) in enumerate(steps,1))
channel='https://www.youtube.com/channel/UCXEhq6pza56xGDjwuZDQFSA'
new=f'''<article class="project-item">
  <img class="project-cover shorts-cover" src="../assets/auto-shorts/pipeline-overview-v1.png" alt="AI Shorts 자동화 파이프라인의 대본 생성부터 업로드까지 8단계 설명 이미지" loading="lazy" width="1672" height="941">
  <div class="project-copy"><small>개인 프로젝트 · AI 콘텐츠 자동화 · 채널 운영</small><h3>AI Shorts 자동화</h3><a class="publish-link project-publish-link" href="{channel}" target="_blank" rel="noopener noreferrer">Undo 게임 지식 채널 보기 ↗</a><p>대본 생성부터 검수·영상 제작·업로드까지 연결한 YouTube Shorts 자동화 파이프라인입니다.</p><p class="project-qa"><b>AI 활용 · 제작 전 과정 자동화</b><span>대본·영상·자막·TTS와 두 차례 검수를 하나의 실행 흐름으로 연결</span></p><button class="project-open" type="button" aria-haspopup="dialog">프로젝트 자세히 보기 →</button></div>
  <template>
<div class="tp">개인 프로젝트 · YouTube Shorts · Undo 게임 지식</div><h3>AI Shorts 자동화</h3><p class="visual-intro">AI로 콘텐츠를 만들고, 검수부터 업로드까지 자동으로 이어가는 제작 파이프라인.</p><p class="visual-role">직접 구축 · 대본·영상·자막·TTS·검수·업로드 자동화</p>
<section class="publish-panel"><h4>운영 중인 채널</h4><a class="publish-link" href="{channel}" target="_blank" rel="noopener noreferrer">Undo 게임 지식 · YouTube 바로가기 ↗</a></section>
<section class="shorts-overview"><h4>대본부터 업로드까지, 하나의 흐름으로</h4><figure><button type="button" class="zoom-photo" aria-haspopup="dialog" aria-label="AI Shorts 8단계 파이프라인 이미지 확대"><img src="../assets/auto-shorts/pipeline-overview-v1.png" alt="대본 생성, 대본 검수, 영상 생성, 영상 편집, 자막 생성, TTS 적용, 최종 검수, 자동 업로드 순서" width="1672" height="941" loading="lazy"></button><figcaption>8단계 제작 흐름을 시각화한 설명 이미지 · 클릭하면 확대됩니다.</figcaption></figure><ol class="shorts-flow">{flow}</ol></section>
<section class="shorts-strengths"><h4>이 프로젝트에서 보여주는 역량</h4><div><article><h5>AI를 실행 흐름에 연결</h5><p>대본·영상·음성 생성 결과를 다음 단계로 넘겨 제작과 업로드를 자동화했습니다.</p></article><article><h5>검수를 포함한 자동화</h5><p>대본 단계와 최종 영상 단계에 검수를 배치해 생성부터 확인까지 연결했습니다.</p></article></div></section>
<section class="shorts-overview"><h4>실제 채널과 콘텐츠</h4><figure><button type="button" class="zoom-photo" aria-haspopup="dialog" aria-label="Undo 게임 지식 실제 채널 화면 확대"><img src="../assets/auto-shorts/undo-channel-20261113.png" alt="Undo 게임 지식 채널의 코요테 타임, 체력바, 점프 버퍼링 Shorts 목록" width="1897" height="939" loading="lazy"></button><figcaption>코요테 타임·체력바·점프 버퍼링 등 게임 속 원리를 설명하는 Shorts를 제작합니다.</figcaption></figure><a class="publish-link" href="{channel}" target="_blank" rel="noopener noreferrer">채널에서 실제 영상 보기 ↗</a></section>
  </template>
</article>'''
order=[cards[0],cards[1],new,cards[4],cards[3],cards[2],cards[5],cards[7],cards[8]]
s=s[:parser.cards[0][0]]+'\n'.join(order)+s[parser.cards[-1][1]:]
s=s.replace("const projectSlugs=['degul-degul','pearl-thieves','trash-boy','rushitadel','stilldrawn','book-tower','lucky-dice','greenway','flyeditor'];","const projectSlugs=['degul-degul','pearl-thieves','ai-shorts','stilldrawn','rushitadel','trash-boy','book-tower','greenway','flyeditor'];")
s=s.replace('아래 9개 기준 · 1인: 게임 3개 + 보드게임 1개 + Figma 프로토타입 1개','아래 9개 기준 · 1인: 게임 3개 + 콘텐츠 자동화 1개 + Figma 프로토타입 1개')
css='''
/* AI Shorts pipeline */
.project-cover.shorts-cover{object-fit:contain;background:#eef5ff;padding:8px}
#ov .shorts-overview{margin-top:32px}#ov .shorts-overview h4,#ov .shorts-strengths h4{font-size:20px;margin-bottom:16px}
#ov .shorts-overview figure{margin:0}#ov .shorts-overview .zoom-photo{width:100%;display:block;border:1px solid var(--line);padding:0;background:#f4f7ff;cursor:zoom-in}#ov .shorts-overview img{display:block;width:100%;height:auto}#ov .shorts-overview figcaption{font-size:13px;color:var(--gray);line-height:1.7;margin:10px 0 18px}
#ov .shorts-flow{list-style:none;padding:0;margin:24px 0;display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:12px}#ov .shorts-flow li{border:1px solid var(--line);padding:18px;background:#fff}#ov .shorts-step-num{font-family:var(--f8);font-size:13px;color:var(--ac)}#ov .shorts-flow h5,#ov .shorts-strengths h5{font-size:16px;margin:8px 0}#ov .shorts-flow p,#ov .shorts-strengths p{font-size:13px;line-height:1.7;color:var(--ink2);margin:0}
#ov .shorts-strengths{margin:32px 0}#ov .shorts-strengths>div{display:grid;grid-template-columns:1fr 1fr;gap:16px}#ov .shorts-strengths article{border-left:3px solid var(--ac);padding:4px 16px}
@media(max-width:800px){#ov .shorts-flow{grid-template-columns:repeat(2,minmax(0,1fr))}}@media(max-width:480px){#ov .shorts-flow{grid-template-columns:1fr}#ov .shorts-flow li{padding:14px 16px}#ov .shorts-strengths>div{grid-template-columns:1fr}}
'''
assert s.count('</style>')==1;s=s.replace('</style>',css+'</style>')
assert 'Lucky Dice' not in s and 'lucky-dice' not in s
p.write_text(s,encoding='utf-8',newline='')
print('Added AI Shorts as third card; replaced Lucky Dice; swapped StillDrawn/trash-boy; synchronized project hashes.')
