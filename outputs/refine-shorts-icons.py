from pathlib import Path
import shutil,re
src=Path(r'C:\Users\doun0\.codex\generated_images\01a18698-0ad4-73d1-a4f3-59233fb683a8')
out=Path('assets/auto-shorts/icons');out.mkdir(parents=True,exist_ok=True)
files={'01-script':'59db6a76-7b65-4ba7-9f73-054aea97fd94','02-review':'bbcc7171-5984-4049-b75c-4f5521973803','03-video':'a66a294d-4ad3-45d0-b67f-bf383514bc01','04-edit':'af526597-fc38-4181-9f4c-d7e204418c6d','05-captions':'3818e678-535c-4e5e-9bea-60af10eaf031','06-tts':'c0006cc5-6c7c-4707-b6a0-d5a2bbfe66a3','07-final-qa':'c0906ea3-a2ac-4e60-83ad-c8d5a3488036','08-upload':'f43e338d-dd63-49ea-8bfe-03e53c236ba4'}
for name,fileid in files.items():shutil.copy2(src/f'exec-{fileid}.png',out/f'{name}.png')
p=Path('qa/index.html');s=p.read_text(encoding='utf-8')
old='<img class="project-cover shorts-cover" src="../assets/auto-shorts/pipeline-overview-v1.png" alt="AI Shorts 자동화 파이프라인의 대본 생성부터 업로드까지 8단계 설명 이미지" loading="lazy" width="1672" height="941">'
preview='<div class="project-cover shorts-cover-preview" role="img" aria-label="대본 생성에서 영상 제작, 업로드로 이어지는 자동화 파이프라인"><span class="shorts-preview-label">AI SHORTS / AUTOMATION</span><div class="shorts-preview-flow">'
for i,(name,label) in enumerate([('01-script','대본'),('03-video','영상'),('08-upload','업로드')]):
 if i:preview+='<span class="shorts-preview-arrow" aria-hidden="true">→</span>'
 preview+=f'<span class="shorts-preview-step"><img src="../assets/auto-shorts/icons/{name}.png" alt="" loading="lazy" width="1280" height="1280"><span>{label}</span></span>'
preview+='</div></div>'
assert old in s;s=s.replace(old,preview)
start=s.index('<section class="shorts-overview"><h4>대본부터 업로드까지')
end=s.index('</section>',start)+len('</section>')
section=s[start:end]
section=re.sub(r'<figure>.*?</figure>','<p class="shorts-flow-intro">대본 생성부터 두 차례 검수와 업로드까지, 8단계를 자동으로 연결했습니다.</p>',section,count=1,flags=re.S)
steps=re.findall(r'<li>.*?</li>',section,re.S);assert len(steps)==8
for step,name in zip(steps,files):
 new=step.replace('<h5>',f'<img class="shorts-step-icon" src="../assets/auto-shorts/icons/{name}.png" alt="" width="1280" height="1280" loading="lazy"><h5>',1)
 section=section.replace(step,new,1)
s=s[:start]+section+s[end:]
start=s.index('/* AI Shorts pipeline */');end=s.index('</style>',start)
css='''/* AI Shorts: separate flat icons and responsive stage flow */
.project-cover.shorts-cover-preview{display:flex;flex-direction:column;justify-content:center;gap:12px;padding:18px 20px;background:#f4f7ff;border-bottom:1px solid var(--line);color:#10162f}
.shorts-preview-label{font-family:var(--f6);font-size:9px;letter-spacing:.12em;color:#315bff}
.shorts-preview-flow{display:flex;align-items:center;justify-content:space-between;gap:8px}.shorts-preview-step{display:flex;flex-direction:column;align-items:center;gap:4px;font-family:var(--f6);font-size:11px}.shorts-preview-step img{display:block;width:clamp(46px,5.2vw,72px);height:clamp(46px,5.2vw,72px);object-fit:contain}.shorts-preview-arrow{font-size:20px;color:#315bff}
#ov .shorts-overview{margin-top:32px}#ov .shorts-overview h4,#ov .shorts-strengths h4{font-size:20px;margin-bottom:16px}
#ov .shorts-overview figure{margin:0}#ov .shorts-overview .zoom-photo{width:100%;display:block;border:1px solid var(--line);padding:0;background:#f4f7ff;cursor:zoom-in}#ov .shorts-overview figure img{display:block;width:100%;height:auto}#ov .shorts-overview figcaption,#ov .shorts-flow-intro{font-size:13px;color:#a9b7d2;line-height:1.7;margin:10px 0 18px}
#ov .shorts-flow{list-style:none;padding:0;margin:24px 0;display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:24px 20px}
#ov .shorts-flow li{position:relative;border:1px solid #cfd8ec;padding:16px;background:#fff;min-width:0;color:#10162f}
#ov .shorts-flow li::before{content:none}#ov .shorts-flow li:not(:nth-child(4n))::after{content:'→';position:absolute;right:-19px;top:80px;width:18px;text-align:center;color:#86a5ff;font-size:18px}
#ov .shorts-step-num{display:block;font-family:var(--f6);font-size:11px;letter-spacing:.08em;color:#315bff}
#ov img.shorts-step-icon{display:block;width:96px;height:96px;object-fit:contain;margin:4px auto 12px}
#ov .shorts-flow h5,#ov .shorts-strengths h5{font-size:16px;margin:8px 0}#ov .shorts-flow h5{color:#10162f}
#ov .shorts-flow p,#ov .shorts-strengths p{font-size:13px;line-height:1.7;margin:0}#ov .shorts-flow p{color:#26354c}#ov .shorts-strengths p{color:#c8d0e2}
#ov .shorts-strengths{margin:32px 0}#ov .shorts-strengths>div{display:grid;grid-template-columns:1fr 1fr;gap:16px}#ov .shorts-strengths article{border-left:3px solid var(--ac);padding:4px 16px}
@media(max-width:800px){#ov .shorts-flow{grid-template-columns:repeat(2,minmax(0,1fr))}#ov .shorts-flow li:nth-child(2n)::after{content:none}.shorts-preview-step img{width:64px;height:64px}}
@media(max-width:480px){#ov .shorts-flow{grid-template-columns:1fr;gap:24px}#ov .shorts-flow li{display:grid;grid-template-columns:64px 1fr;column-gap:14px;padding:16px}#ov .shorts-step-num{grid-column:2}#ov img.shorts-step-icon{grid-column:1;grid-row:1/4;width:64px;height:64px;margin:0;align-self:center}#ov .shorts-flow h5,#ov .shorts-flow p{grid-column:2}#ov .shorts-flow h5{margin:4px 0 6px}#ov .shorts-flow li:not(:last-child)::after{content:'↓';right:50%;top:auto;bottom:-24px;transform:translateX(50%);height:24px;line-height:24px}#ov .shorts-strengths>div{grid-template-columns:1fr}}
'''
s=s[:start]+css+s[end:]
assert 'pipeline-overview-v1.png' not in s
p.write_text(s,encoding='utf-8',newline='')
print('Saved 8 separate icons; replaced collage with icon flow and three-icon project preview.')
