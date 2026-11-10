// Usage: node puzzle-playflow.cjs <extracted v1.2.1 web directory> <output directory>
// Requires Playwright and Chrome. All play uses isolated browser contexts; external requests are blocked.
const {chromium}=require('playwright');
const fs=require('node:fs'),path=require('node:path'),http=require('node:http');
const assert=require('node:assert/strict'),crypto=require('node:crypto');
const root=path.resolve(process.argv[2]||''),out=path.resolve(process.argv[3]||'puzzle-results');
if(!fs.existsSync(path.join(root,'game-assets.json')))throw Error('Provide the extracted game web directory');
fs.mkdirSync(out,{recursive:true});
const types={'.js':'text/javascript','.wasm':'application/wasm','.json':'application/json','.html':'text/html','.png':'image/png'};
const server=http.createServer((q,r)=>{const f=path.resolve(root,'.'+decodeURIComponent(new URL(q.url,'http://local').pathname).replace(/\/$/,'/index.html'));if(!f.startsWith(root+path.sep)||!fs.existsSync(f)){r.writeHead(404).end();return;}r.setHeader('Content-Type',types[path.extname(f)]||'application/octet-stream');fs.createReadStream(f).pipe(r);});
const report={startedAt:new Date().toISOString(),build:'itch.io upload package 1.2.1 (local)',method:'Real keyboard and pointer input; read-only IndexedDB assertions',viewport:{width:900,height:900},runs:[],files:{}};
for(const f of ['game-assets.json','game-loader.js'])report.files[f]=crypto.createHash('sha256').update(fs.readFileSync(path.join(root,f))).digest('hex');
async function readSave(p){return p.evaluate(()=>new Promise((resolve,reject)=>{const request=indexedDB.open('/idbfs');request.onerror=()=>reject(request.error);request.onsuccess=()=>{const db=request.result;const q=db.transaction('FILE_DATA').objectStore('FILE_DATA').getAll();q.onerror=()=>reject(q.error);q.onsuccess=()=>{const rows=q.result.filter(v=>v.contents).map(v=>{try{return JSON.parse(new TextDecoder().decode(v.contents))}catch{return null}}).filter(v=>v?.payload&&v.sequence).sort((a,b)=>b.sequence-a.sequence);db.close();resolve(rows.length?JSON.parse(rows[0].payload):null)};};}));}
(async()=>{
 await new Promise(r=>server.listen(0,'127.0.0.1',r));const url=`http://127.0.0.1:${server.address().port}/`;
 const browser=await chromium.launch({channel:'chrome',headless:true,args:['--use-gl=angle','--use-angle=swiftshader','--enable-unsafe-swiftshader']});report.browser=browser.version();
 try{for(let round=1;round<=3;round++){
  const context=await browser.newContext({viewport:report.viewport});const p=await context.newPage();
  const run={round,checks:[],errors:[],observations:[]};report.runs.push(run);
  p.on('pageerror',e=>run.errors.push(e.message));
  await p.route('**/*',r=>new URL(r.request().url()).hostname==='127.0.0.1'||r.request().url().startsWith('blob:')?r.continue():r.abort());
  const key=async k=>{await p.keyboard.down(k);await p.waitForTimeout(120);await p.keyboard.up(k);await p.waitForTimeout(1600)};
  const shot=async name=>{if(round===1)await p.screenshot({path:path.join(out,name+'.png')})};
  const save=async label=>{await p.waitForTimeout(700);const s=await readSave(p);run.observations.push({label,level:s?.levelId,moves:s?.current.moves,positions:s?.current.positions,wallet:s?.collection.wallet,completed:s?.completed,records:s?.collection.records,activity:s?.activity});return s};
  const check=(id,title,fn)=>{try{fn();run.checks.push({id,title,status:'PASS'})}catch(e){run.checks.push({id,title,status:'FAIL',detail:e.message});throw e}};
  try{
   await p.goto(url);await p.waitForFunction(()=>window.unityInstance,null,{timeout:120000});await p.locator('canvas').click();
   await p.getByRole('button',{name:/참여하지|Play without/}).click({timeout:30000});await p.waitForTimeout(2000);
   await p.mouse.click(450,615);await p.waitForTimeout(1800);const initial=await save('initial');await shot('pz-start');assert.equal(initial.collection.wallet,0);
   for(const k of ['ArrowUp','ArrowRight','ArrowDown'])await key(k);await p.waitForTimeout(4500);
   const cleared=await save('first-clear');await shot('pz-clear');
   check('PZ-E01','Fresh start → three moves → first reward',()=>{assert.equal(cleared.current.moves,3);assert(cleared.completed.includes('F0'));assert.equal(cleared.collection.wallet,3);assert.equal(cleared.collection.records[0].stars,3)});
   await p.mouse.click(450,643);await p.waitForTimeout(1800);
   await key('Escape');const beforePause=await save('pause-before');await key('ArrowUp');const afterPause=await save('pause-after');await shot('pz-paused');
   check('PZ-E02','Pause rejects movement input',()=>{assert.equal(afterPause.current.moves,beforePause.current.moves);assert.deepEqual(afterPause.current.positions,beforePause.current.positions)});
   await key('Escape');await key('ArrowUp');const moved=await save('moved');await key('z');const undone=await save('undo');await shot('pz-undo');
   check('PZ-E03','Input → undo restores position and move count',()=>{assert.equal(moved.current.moves,1);assert.equal(undone.current.moves,0);assert.deepEqual(undone.current.positions,initial.current.positions)});
   for(const k of ['ArrowUp','ArrowRight','ArrowDown'])await key(k);await p.waitForTimeout(4500);const repeat=await save('repeat-clear');await shot('pz-repeat');
   check('PZ-E04','Replay does not duplicate already earned stars',()=>{assert.equal(repeat.activity.stages.find(s=>s.room==='F0').clears,2);assert.equal(repeat.collection.wallet,3);assert.equal(repeat.collection.records[0].stars,3)});
   await p.reload();await p.waitForFunction(()=>window.unityInstance,null,{timeout:120000});await p.waitForTimeout(12000);const reload=await save('reload');await shot('pz-reload');
   check('PZ-E05','Reload preserves progress, best score and wallet',()=>{assert.deepEqual(reload.completed,repeat.completed);assert.deepEqual(reload.collection.records,repeat.collection.records);assert.equal(reload.collection.wallet,3)});
  }catch(e){run.failure=e.message;await shot('pz-failure');}finally{await context.close();fs.writeFileSync(path.join(out,'puzzle-results.json'),JSON.stringify(report,null,2));}
 }}finally{await browser.close();server.close();report.finishedAt=new Date().toISOString();report.total=report.runs.reduce((n,r)=>n+r.checks.length,0);report.passed=report.runs.reduce((n,r)=>n+r.checks.filter(c=>c.status==='PASS').length,0);fs.writeFileSync(path.join(out,'puzzle-results.json'),JSON.stringify(report,null,2));console.log(JSON.stringify({total:report.total,passed:report.passed,failures:report.runs.map(r=>r.failure).filter(Boolean)}));if(report.total!==15||report.passed!==15)process.exitCode=1;}
})().catch(e=>{console.error(e);server.close();process.exitCode=1});
