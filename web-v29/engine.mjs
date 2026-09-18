export const WORLD={width:20,height:9.45,gravity:9.81,radius:.46,spawn:{x:3.5,y:1.15}};
export const LEVEL_COUNT=200;
// Difficulty is mechanical: tougher anchor targets, moving targets, jump walls,
// overhead blockers and a progressively shorter aiming guide. No random layouts.
const FORMATIONS=[
 ['هدية البداية',[0,.6]],
 ['درج القنينات',[0,1.35,2.25]],
 ['وادي الألعاب',[1.75,.15,1.75]],
 ['القمة والتلة',[2.35,.25,1.1]],
 ['سلم الهدايا',[0,.95,1.9,2.65]],
 ['أمواج المرح',[0,1.45,.15,1.65]],
 ['الجسر العالي',[1.35,1.35,1.35,1.35]],
 ['الدرج المعكوس',[2.5,1.65,.8,0]],
 ['بوابة النجوم',[1.8,.15,.75,2.05]],
 ['موكب المفاجآت',[0,.7,1.55,.7,0]],
 ['حصن القصر',[.2,1.55,2.35,1.15]],
 ['ممر الأبطال',[1.85,.25,2.2,.55]]
];
export const LEVELS=Array.from({length:LEVEL_COUNT},(_,index)=>{
 const chapter=Math.floor(index/10),pattern=index%FORMATIONS.length;
 const challenge=Math.max(0,index-9),d=index<10?0:(index-10)/189;
 const tier=index<10?0:Math.min(5,1+Math.floor((index-10)/38));
 const [name,original]=FORMATIONS[pattern];
 const count=index===0?1:index<10?Math.min(4,Math.max(2,original.length)):Math.min(5,3+Math.floor((index-10)/48));
 const heights=Array.from({length:count},(_,i)=>{
   const base=original[i%original.length];
   if(index<10)return base;
   const rhythm=.34*Math.sin(index*1.21+i*2.17)+.18*Math.cos(index*.71+i*1.37);
   return Math.max(0,Math.min(3.05,base+rhythm+d*.28*(i%2)));
 });
 const world=['jungle','mountains','coast'][Math.floor(index/7)%3];
 const targets=heights.map((height,id)=>{
  const type=index===0?'crate':(index>=24&&id===0&&pattern%3===0?'tnt':['crate','bottle','toy'][(pattern+chapter+id)%3]);
  const shrink=index<10?1:1-d*.16;
  const size=(type==='crate'?(count===1?3.05:2.05):type==='tnt'?1.62:type==='toy'?1.9:1.72)*shrink;
  const hh=size*(type==='bottle'?.66:.55);
  const board=.62+height*(.58+d*.11)+(id%2)*d*.14;
  let hp=1;
  if(index>=10)hp+=tier>=2&&((id+pattern)%3===0)?1:0;
  if(tier>=4&&id===0)hp+=1;
  if(tier>=5&&id===count-1)hp+=1;
  const moving=index>=10&&((id+pattern)%2===1||tier>=4);
  const motion=moving?Math.min(.34,.14+d*.24+(tier>=4?.05:0)):0;
  const shield=index>=18&&(id===count-1||(tier>=3&&id===1)||(tier>=5&&id===Math.floor(count/2)));
  return {type,x:0,y:board+hh,size,hp,motion,
    spinSpeed:.75+(id%3)*.23+d*.95,
    shield,
    shieldPeriod:Math.max(2.15,3.65-d*1.15-(tier>=4?.22:0)),
    shieldOpenRatio:Math.max(.28,.6-d*.25-(tier>=4?.06:0)),
    shieldPhase:(index%9)*.41+id*.37};
 });
 const gap=Math.max(.12,.24-d*.08+(pattern%3)*.035),half=t=>targetBounds(t).hw+t.motion+.1;
 const total=targets.reduce((n,t)=>n+2*half(t),0)+gap*(count-1);
 let cursor=index===0?10.3:Math.max(7.15,19.45-total);
 for(const t of targets){t.x=cursor+half(t);cursor+=2*half(t)+gap;}
 // If a dense late-game pattern would overrun the world, compress only the horizontal centres.
 const last=targets.at(-1),lastRight=last?last.x+half(last):0;
 if(lastRight>19.55){
   const first=targets[0],firstLeft=first.x-half(first),span=lastRight-firstLeft,scale=(19.55-firstLeft)/span;
   for(const t of targets)t.x=firstLeft+(t.x-firstLeft)*scale;
 }
 const obstacles=[];
 if(index>=10){
   const first=targets[0],right=first.x-half(first)-.38;
   obstacles.push({id:'jump-wall',x:Math.max(5.3,right-1.05),hw:.18,top:1.15+d*1.85,bottom:0,
     motionY:index>=55?.14+d*.23:0,speed:1.0+d*1.35});
 }
 if(index>=24||(index>=10&&pattern%4===1)){
   obstacles.push({id:'ceiling',x:12.6+(pattern%3)*.55,hw:.72+d*.5,top:7.55,bottom:7.22,
     motionY:index>=85?.12+d*.17:0,speed:1.15+d*1.45});
 }
 if(index>=58){
   obstacles.push({id:'mid-gate',x:10.25+(pattern%2)*.8,hw:.16,top:2.1+d*1.15,bottom:0,
     motionY:index>=95?.15+d*.22:0,speed:1.35+d*1.4});
 }
 if(index>=108){
   obstacles.push({id:'upper-gate',x:14.25-(pattern%3)*.42,hw:.9+d*.25,top:6.85,bottom:6.52,
     motionY:.16+d*.22,speed:1.45+d*1.55});
 }
 if(index>=155){
   obstacles.push({id:'second-wall',x:7.25+(pattern%3)*.3,hw:.17,top:1.75+d*.85,bottom:0,
     motionY:.18+d*.25,speed:1.7+d*1.5});
 }
 const hazards=[];
 if(index>=20){
   hazards.push({id:'swing-a',type:'swinger',anchorX:15.9-(pattern%3)*.75,anchorY:7.8,length:1.55+d*.55,radius:.34+d*.08,speed:1.05+d*1.25,phase:(pattern%5)*.55});
 }
 if(index>=72){
   hazards.push({id:'swing-b',type:'swinger',anchorX:11.8+(pattern%2)*.8,anchorY:7.25,length:1.25+d*.5,radius:.32+d*.08,speed:1.25+d*1.4,phase:1.4+(pattern%4)*.5});
 }
 if(index>=138){
   hazards.push({id:'swing-c',type:'swinger',anchorX:8.4+(pattern%3)*.45,anchorY:6.85,length:1.1+d*.45,radius:.3+d*.08,speed:1.45+d*1.55,phase:2.1});
 }
 const windMag=index<10?0:.34+d*2.15;
 const wind=index<10?0:windMag*((pattern+chapter)%2===0?1:-1)*(world==='mountains'?1.1:.9);
 const guideSeconds=index<10?2.5:Math.max(.3,.98-d*.68);
 const tip=index<10?'خذ وقتك في التصويب.':
   tier===1?'الحاجز الأول بدأ يتحرك؛ اضبط القوس والتوقيت.':
   tier===2?'راقب الحماية الذهبية والرياح قبل الرمي.':
   tier===3?'استغل الارتدادات؛ الطريق المباشر لن يكفي دائمًا.':
   tier===4?'الأهداف أسرع وأمتن؛ خطط للرميات قبل أن تبدأ.':
   'مرحلة أبطال: رياح قوية، حواجز متعددة ونوافذ حماية قصيرة.';
 return {name:`${name} · ${chapter+1}`,chapter,formation:pattern,tier,world,difficulty:index+1,
  wind,motionSpeed:1.15+d*2.2,guideSeconds,obstacles,hazards,tip,targets};
});
export const WORLDS={jungle:{name:'أدغال المرح',color:'#6ca441',ground:'#638945'},mountains:{name:'قمم المغامرة',color:'#46a6c5',ground:'#77a1a0'},coast:{name:'شاطئ الألعاب',color:'#f0b357',ground:'#d9a265'}};
export function launchVelocity(dx,dy){if(!Number.isFinite(dx)||!Number.isFinite(dy))throw new Error('Invalid aim');const length=Math.hypot(dx,dy);if(length<.12)return null;const power=3+12*Math.min(length/3.4,1),nx=dx/length,ny=Math.max(dy/length,.28),norm=Math.hypot(nx,ny);return {vx:power*nx/norm,vy:power*ny/norm};}
export function targetBounds(t){return {hw:t.size*(t.type==='bottle'?.27:t.type==='tnt'?.42:.47),hh:t.size*(t.type==='bottle'?.66:t.type==='tnt'?.55:.55)};}
// The same world-space rectangles drive rendering, collision and aim prediction.
export function createPlatforms(targets){return targets.map(t=>{const {hw,hh}=targetBounds(t),top=t.baseY-hh;return {id:t.id,x:t.baseX,top,bottom:Math.max(0,top-.22),hw:hw+(t.motion||0)+.14};});}
export function bounceOnPlatform(ball,platform){
 const left=platform.x-platform.hw,right=platform.x+platform.hw;
 const closestX=Math.max(left,Math.min(right,ball.x)),closestY=Math.max(platform.bottom,Math.min(platform.top,ball.y));
 let nx=ball.x-closestX,ny=ball.y-closestY,distance=Math.hypot(nx,ny),penetration=WORLD.radius-distance;
 if(penetration<=0)return null;
 if(distance>1e-8){nx/=distance;ny/=distance;}
 else{
  const faces=[{d:ball.x-left,nx:-1,ny:0},{d:right-ball.x,nx:1,ny:0},{d:ball.y-platform.bottom,nx:0,ny:-1},{d:platform.top-ball.y,nx:0,ny:1}];
  const face=faces.reduce((a,b)=>a.d<b.d?a:b);nx=face.nx;ny=face.ny;penetration=WORLD.radius+face.d;
 }
 ball.x+=nx*(penetration+.00001);ball.y+=ny*(penetration+.00001);
 const incoming=ball.vx*nx+ball.vy*ny;if(incoming>=0)return null;
 const restitution=incoming<-.6?.78:0;
 const tangentX=ball.vx-incoming*nx,tangentY=ball.vy-incoming*ny;
 ball.vx=tangentX*.985-incoming*restitution*nx;ball.vy=tangentY*.985-incoming*restitution*ny;
 return {type:'bounce',platformId:platform.id,x:ball.x-nx*WORLD.radius,y:ball.y-ny*WORLD.radius,impact:-incoming};
}
export function bounceOnCircle(ball,circle){
 const dx=ball.x-circle.x,dy=ball.y-circle.y,limit=WORLD.radius+circle.radius,dist=Math.hypot(dx,dy);
 if(dist>=limit)return null;
 let nx=dist>1e-7?dx/dist:1,ny=dist>1e-7?dy/dist:0;
 ball.x=circle.x+nx*(limit+.0001);ball.y=circle.y+ny*(limit+.0001);
 const incoming=ball.vx*nx+ball.vy*ny;if(incoming>=0)return null;
 const tx=ball.vx-incoming*nx,ty=ball.vy-incoming*ny;
 ball.vx=tx*.96-incoming*.82*nx;ball.vy=ty*.96-incoming*.82*ny;
 return {type:'hazard',hazardId:circle.id,x:ball.x-nx*WORLD.radius,y:ball.y-ny*WORLD.radius,impact:-incoming};
}
export function shieldIsOpen(target,clock){if(!target.shield)return true;const phase=((clock+target.shieldPhase)%target.shieldPeriod)/target.shieldPeriod;return phase<target.shieldOpenRatio;}
export class Game{
 constructor(level=0,mode='family'){this.mode=mode==='challenge'?'challenge':'family';this.reset(level);}
 reset(level=this.levelIndex??0){if(!Number.isInteger(level)||level<0||level>=LEVELS.length)throw new Error('Invalid level');this.levelIndex=level;this.level=LEVELS[level];const hpTotal=this.level.targets.reduce((n,t)=>n+t.hp,0);this.totalBalls=level<10?(this.mode==='family'?7:5):Math.max(3,Math.ceil(hpTotal*(this.mode==='family'?.72:.62))+(this.mode==='family'?1:0));this.balls=this.totalBalls;this.state='aiming';this.paused=false;this.time=0;this.clock=0;this.still=0;this.cooldown=0;this.clearTime=null;this.escaped=false;this.throwHit=false;this.events=[];this.targets=this.level.targets.map((t,id)=>({...t,id,baseX:t.x,y:Math.max(t.y,targetBounds(t).hh+.12),baseY:Math.max(t.y,targetBounds(t).hh+.12),alive:true,struck:false,maxHp:t.hp}));this.platforms=createPlatforms(this.targets);this.obstacles=this.level.obstacles.map(p=>({...p,baseTop:p.top,baseBottom:p.bottom}));this.hazards=(this.level.hazards||[]).map(h=>({...h,x:h.anchorX,y:h.anchorY-h.length}));this.ball={...WORLD.spawn,vx:0,vy:0};}
 setMode(mode){if(!['family','challenge'].includes(mode))throw new Error('Invalid mode');this.mode=mode;this.reset();}
 get remaining(){return this.targets.filter(t=>t.alive).length;}
 get stars(){if(this.state!=='win')return 0;const used=this.totalBalls-this.balls,perfect=Math.max(2,...this.targets.map(t=>t.maxHp));return used<=perfect?3:used<=perfect+1?2:1;}
 launch(dx,dy){if(this.state!=='aiming'||this.paused||this.balls===0)return false;const v=launchVelocity(dx,dy);if(!v)return false;Object.assign(this.ball,v);this.balls--;this.state='flying';this.time=0;this.still=0;this.escaped=false;this.clearTime=null;this.throwHit=false;this.targets.forEach(t=>t.struck=false);this.events.push({type:'throw'});return true;}
 step(dt){if(!Number.isFinite(dt)||dt<=0||dt>.1)return;const count=Math.ceil(dt*120);for(let i=0;i<count;i++)this.advanceStep(dt/count);}
 advanceStep(dt){if(this.paused||this.state==='win'||this.state==='fail')return;this.clock+=dt;for(const h of this.hazards){const a=Math.sin(this.clock*h.speed+h.phase)*1.05;h.x=h.anchorX+Math.sin(a)*h.length;h.y=h.anchorY-Math.cos(a)*h.length;}for(const p of this.obstacles){const offset=p.motionY?Math.sin(this.clock*p.speed)*p.motionY:0;p.top=p.baseTop+offset;p.bottom=p.baseBottom+offset;}for(const t of this.targets)if(t.motion&&t.alive)t.x=t.baseX+Math.sin(this.clock*(this.level.motionSpeed||1.4)+t.id)*t.motion;
 if(this.state==='resetting'){this.cooldown-=dt;if(this.cooldown<=0){this.escaped=false;this.ball={...WORLD.spawn,vx:0,vy:0};this.state=this.balls?'aiming':'fail';}return;}if(this.state!=='flying')return;
 this.time+=dt;const b=this.ball;b.vx+=(this.level.wind||0)*dt;b.vy-=WORLD.gravity*dt;b.x+=b.vx*dt;b.y+=b.vy*dt;
 if(b.x>WORLD.width||b.x<0||b.y>WORLD.height){this.escaped=true;this.events.push({type:'exit'});if(this.remaining===0){this.state='win';this.events.push({type:'win'});}else{this.state='resetting';this.cooldown=.45;}return;}
 for(const platform of [...this.platforms,...this.obstacles]){const collision=bounceOnPlatform(b,platform);if(collision&&collision.impact>.7)this.events.push(collision);} for(const h of this.hazards){const collision=bounceOnCircle(b,h);if(collision&&collision.impact>.7)this.events.push(collision);}
 for(const t of this.targets){if(!t.alive)continue;const {hw,hh}=targetBounds(t),radius=WORLD.radius+(this.mode==='family'?.09:0);if(Math.hypot(Math.max(Math.abs(b.x-t.x)-hw,0),Math.max(Math.abs(b.y-t.y)-hh,0))>radius)continue;
 if(!shieldIsOpen(t,this.clock)){const collision=bounceOnPlatform(b,{id:'shield-'+t.id,x:t.x,hw,top:t.y+hh,bottom:t.y-hh});if(collision&&collision.impact>.7)this.events.push({...collision,type:'shield',targetId:t.id});continue;}
 if(t.struck)continue;
 t.struck=true;t.hp--;this.throwHit=true;this.events.push({type:'hit',targetId:t.id,x:t.x,y:t.y,kind:t.type,size:t.size,vx:b.vx*.3,vy:Math.max(1.2,Math.abs(b.vy)*.35),destroyed:t.hp<=0});
 if(t.hp<=0){
   t.alive=false;
   if(t.type==='tnt'){
     const radius=2.75;this.events.push({type:'explosion',x:t.x,y:t.y,radius});
     for(const other of this.targets){
       if(!other.alive||other.id===t.id)continue;
       if(Math.hypot(other.x-t.x,other.y-t.y)<=radius){other.hp=0;other.alive=false;this.events.push({type:'hit',targetId:other.id,x:other.x,y:other.y,kind:other.type,size:other.size,vx:0,vy:2.4,destroyed:true,chain:true});}
     }
   }
 }
 b.vx*=.9;b.vy*=.87;if(this.remaining===0&&this.clearTime===null){this.clearTime=this.time;this.events.push({type:'cleared'});}}
 if(b.y<WORLD.radius){b.y=WORLD.radius;const impact=-b.vy;b.vy=impact>.7?impact*.48:0;if(impact>.7){b.vx*=.96;this.events.push({type:'bounce',platformId:'floor',x:b.x,y:0,impact});}}if(b.y<=WORLD.radius+.001&&Math.abs(b.vy)<.2)b.vx*=Math.pow(.13,dt);this.still=Math.hypot(b.vx,b.vy)<.15?this.still+dt:0;
 const settled=this.still>.4||this.time>=6||b.x< -2||b.x>22;
 if(this.clearTime!==null){if(settled||this.time-this.clearTime>3.5){this.state='win';this.events.push({type:'win'});}return;}
 if(settled){this.state='resetting';this.cooldown=.28;}}
 quickReady(){if(this.paused||this.remaining===0||!this.throwHit||!['flying','resetting'].includes(this.state)||this.balls===0)return false;this.escaped=false;this.ball={...WORLD.spawn,vx:0,vy:0};this.state='aiming';this.time=0;this.still=0;this.cooldown=0;this.clearTime=null;this.throwHit=false;this.events.push({type:'quick-ready'});return true;}
 snapshot(){return {level:this.levelIndex+1,mode:this.mode,state:this.state,balls:this.balls,remaining:this.remaining,stars:this.stars,paused:this.paused,quickReady:this.throwHit&&['flying','resetting'].includes(this.state)&&this.balls>0,ball:{...this.ball}};}
}
export function predict(game,dx,dy){
 if(!launchVelocity(dx,dy))return [];
 const simulation=Object.assign(new Game(game.levelIndex,game.mode),game,{
  targets:game.targets.map(t=>({...t})),platforms:game.platforms.map(p=>({...p})),obstacles:game.obstacles.map(p=>({...p})),
  ball:{...WORLD.spawn,vx:0,vy:0},events:[],paused:false,state:'aiming'
 });
 if(!simulation.launch(dx,dy))return [];
 const points=[];
 for(let i=0;i<(game.mode==='family'?720:420);i++){
  simulation.step(1/120);
  const bounced=simulation.events.some(e=>e.type==='bounce'||e.type==='shield');simulation.events.length=0;
  if(i%6===0||bounced)points.push({x:simulation.ball.x,y:simulation.ball.y,bounce:bounced,time:(i+1)/120});
  if(simulation.state!=='flying')break;
 }
 return points;
}
