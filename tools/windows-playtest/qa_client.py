import subprocess,sys,json,time,ctypes,os
from pathlib import Path
from ctypes import wintypes as w
ROOT=Path(os.environ.get('PORTFOLIO_REPOS_ROOT',str(Path(__file__).resolve().parents[3])));OUT=Path(__file__).resolve().parents[2]/'outputs/player-qa';OUT.mkdir(exist_ok=True)
EXES={'Capstone':'TrashBoy','Puzzle':'DegulDegul','FlyEditor':'FlyEditor','Roguelike-Defense':'Rushitadel','Unfinished-Sketchbook':'StillDrawn','Book-tower':'AFoldOfLight','GameEngine_Pearl-Thieves':'PearlThieves'}
class Player:
 def __init__(self,repo):self.repo=repo;self.folder=OUT/repo;self.folder.mkdir(exist_ok=True)
 def start(self):
  build=ROOT/self.repo/'Builds/PlayerQA-20261114';si=subprocess.STARTUPINFO();si.dwFlags=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
  if (self.folder/'command.json').exists():(self.folder/'command.json').unlink()
  p=subprocess.Popen([str(build/(EXES[self.repo]+'.exe')),*(['-force-d3d11'] if os.environ.get('QA_D3D11') else []),'-playerQa',str(self.folder),'-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',str(self.folder/'player.log')],cwd=build,startupinfo=si,creationflags=subprocess.CREATE_NO_WINDOW)
  (self.folder/'pid').write_text(str(p.pid));time.sleep(8);return self.command('ping')
 def command(self,op,**kwargs):
  id=time.time_ns()//1000000%2000000000;data={'id':id,'op':op,**kwargs};target=self.folder/'command.json';tmp=self.folder/'command.tmp';tmp.write_text(json.dumps(data),encoding='utf-8')
  for retry in range(30):
   try:tmp.replace(target);break
   except PermissionError:time.sleep(.03)
  limit=time.time()+15
  while time.time()<limit:
   try:
    r=json.loads((self.folder/'reply.json').read_text(encoding='utf-8-sig'))
    if r['id']==id:
     with (self.folder/'actions.jsonl').open('a',encoding='utf-8') as f:f.write(json.dumps({'command':data,'reply':r},ensure_ascii=False)+'\n')
     if r.get('error'):raise RuntimeError(r['error'])
     return r
   except (FileNotFoundError,json.JSONDecodeError):pass
   time.sleep(.005)
  raise TimeoutError(str(data))
 def tap(self,*keys,duration=.15):
  self.command('keys',keys=list(keys));time.sleep(duration);self.command('keys',keys=[]);time.sleep(.2);return self.command('ping')
 def mouse(self,x,y,down=False,button=0):return self.command('mouse',x=x,y=y,down=down,button=button)
 def click(self,x,y):
  self.mouse(x,y);time.sleep(.005);self.mouse(x,y,True);time.sleep(.15);self.mouse(x,y,False);time.sleep(.3);return self.command('ping')
 def native_click(self,x,y):
  pid=int((self.folder/'pid').read_text());handles=[];u=ctypes.windll.user32
  @ctypes.WINFUNCTYPE(w.BOOL,w.HWND,w.LPARAM)
  def enum(hwnd,param):
   got=w.DWORD();u.GetWindowThreadProcessId(hwnd,ctypes.byref(got));r=w.RECT();u.GetClientRect(hwnd,ctypes.byref(r))
   if got.value==pid and r.right>100 and r.bottom>100:handles.append(hwnd)
   return True
  u.EnumWindows(enum,0)
  for hwnd in handles:
   dpi=u.GetDpiForWindow(hwnd) or 96;ratio=96/dpi;pos=(int(y*ratio)<<16)|(int(x*ratio)&0xffff)
   u.PostMessageW(hwnd,0x200,0,pos);u.PostMessageW(hwnd,0x201,1,pos);time.sleep(.15);u.PostMessageW(hwnd,0x202,0,pos)
  time.sleep(.4);return self.command('ping')
 def native_key(self,vk,duration=.15):
  pid=int((self.folder/'pid').read_text());u=ctypes.windll.user32;handles=[]
  @ctypes.WINFUNCTYPE(w.BOOL,w.HWND,w.LPARAM)
  def enum(hwnd,param):
   got=w.DWORD();u.GetWindowThreadProcessId(hwnd,ctypes.byref(got))
   if got.value==pid:handles.append(hwnd)
   return True
  u.EnumWindows(enum,0);scan=u.MapVirtualKeyW(vk,0)
  for hwnd in handles:u.PostMessageW(hwnd,0x100,vk,1|(scan<<16))
  time.sleep(duration)
  for hwnd in handles:u.PostMessageW(hwnd,0x101,vk,1|(scan<<16)|0xc0000000)
  time.sleep(.2);return self.command('ping')
 def render_offscreen(self):
  pid=int((self.folder/'pid').read_text());u=ctypes.windll.user32;handles=[]
  @ctypes.WINFUNCTYPE(w.BOOL,w.HWND,w.LPARAM)
  def enum(hwnd,param):
   got=w.DWORD();u.GetWindowThreadProcessId(hwnd,ctypes.byref(got));r=w.RECT();u.GetClientRect(hwnd,ctypes.byref(r))
   if got.value==pid and r.right>100 and r.bottom>100:handles.append(hwnd)
   return True
  u.EnumWindows(enum,0)
  for hwnd in handles:
   u.SetWindowPos(hwnd,1,-16000,-16000,0,0,0x0011|0x0040)
   u.ShowWindow(hwnd,4)
   u.PostMessageW(hwnd,0x0006,1,0)
   u.PostMessageW(hwnd,0x0007,0,0)
  time.sleep(2);return handles
 def read(self,type,member):return self.command('read',type=type,member=member)['value']
 def button(self,name):
  buttons=json.loads(self.command('buttons')['value'])['items'];matches=[b for b in buttons if b['name']==name or b['text']==name]
  if len(matches)!=1:raise RuntimeError(str(matches)+' / '+str([(b['name'],b['text']) for b in buttons]))
  b=matches[0];assert b['enabled'];return self.click(b['x'],b['y'])
 def capture(self,name):
  self.command('capture',name=name);time.sleep(1);p=self.folder/(name+'.png');return {'path':str(p),'exists':p.exists()}
if __name__=='__main__':
 sys.stdout.reconfigure(encoding='utf-8');p=Player(sys.argv[1]);op=sys.argv[2];args=sys.argv[3:]
 if op=='start':result=p.start()
 elif op=='tap':result=p.tap(*args)
 elif op=='hold':result=p.tap(*args[1:],duration=float(args[0]))
 elif op=='click':result=p.click(*map(float,args))
 elif op=='native_click':result=p.native_click(*map(float,args))
 elif op=='native_key':result=p.native_key(int(args[0]))
 elif op=='read':result=p.read(*args)
 elif op=='button':result=p.button(' '.join(args))
 elif op=='capture':result=p.capture(*args)
 elif op=='offscreen':result=p.render_offscreen()
 elif op in ('objects','members'):result=p.command(op,type=args[0] if args else '')
 else:result=p.command(op)
 print(json.dumps(result,ensure_ascii=False))

