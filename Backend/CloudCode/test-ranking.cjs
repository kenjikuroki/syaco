const assert=require('node:assert/strict'),fs=require('node:fs'),vm=require('node:vm');
const values=new Map(),boards=new Map();let revision=0,privateLobby=false;
const clone=x=>JSON.parse(JSON.stringify(x));
class DataApi{
 async getPrivateCustomItems(project,id,keys){return {data:{results:keys.map(k=>values.get(id+'/'+k)).filter(Boolean).map(clone)}};}
 async setPrivateCustomItem(project,id,item){const key=id+'/'+item.key,old=values.get(key);if(item.writeLock&&old?.writeLock!==item.writeLock){const e=Error('conflict');e.response={status:409};throw e;}values.set(key,{key:item.key,value:clone(item.value),writeLock:String(++revision)});return {data:{}};}
}
class LobbyApi{async getLobby(){return {data:{name:'shako-duel-v1',isPrivate:privateLobby,players:[{id:'alice'},{id:'bob'}],created:new Date().toISOString()}};}}
class LeaderboardsApi{async addLeaderboardPlayerScore(p,b,id,value){boards.set(b+'/'+id,value.score);}}
const context={module:{exports:{}},require:id=>id.includes('cloud-save')?{DataApi}:id.includes('lobby')?{LobbyApi}:{LeaderboardsApi},Date,Math,Number,Error};
vm.runInNewContext(fs.readFileSync(__dirname+'/ShakoSubmitResult.js','utf8'),context);
const submit=(me,wins,losses,sessionId='s1',opponent=me==='alice'?'bob':'alice')=>context.module.exports({params:{sessionId,matchId:'ignored',opponentId:opponent,wins,losses},context:{playerId:me,projectId:'project'}});
(async()=>{
 assert.equal((await submit('alice',3,1)).status,'waiting');assert.equal(boards.size,0);
 assert.equal((await submit('bob',1,3)).status,'confirmed');assert.equal(boards.get('shako_monthly_rating/alice'),1012);assert.equal(boards.get('shako_monthly_rating/bob'),988);assert.equal(boards.get('shako_monthly_streak/alice'),1);
 await submit('alice',3,1);assert.equal(boards.get('shako_monthly_rating/alice'),1012);
 await assert.rejects(()=>submit('alice',2,1,'bad'));await assert.rejects(()=>submit('alice',3,1,'other','mallory'));await assert.rejects(()=>submit('alice',1,3));
 await submit('alice',3,0,'disputed');await assert.rejects(()=>submit('bob',3,0,'disputed'));
 privateLobby=true;await assert.rejects(()=>submit('alice',3,0,'private'));privateLobby=false;
 await Promise.all([submit('alice',3,2,'concurrent'),submit('bob',2,3,'concurrent')]);await submit('alice',3,2,'concurrent');assert.equal(boards.get('shako_monthly_streak/alice'),2);
 console.log('PASS: paired reports, Elo, streak, duplicate protection, invalid scores, outsiders, disputes, private-room rejection, concurrent submission');
})().catch(e=>{console.error(e);process.exitCode=1;});
