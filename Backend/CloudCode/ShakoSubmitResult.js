// UGS Cloud Code JavaScript. Deploy with the same name. Public matches only.
// Both authenticated participants must report the same first-to-three result.
// This verifies agreement, not the host's combat simulation; see ONLINE_SETUP.md.
const { DataApi } = require('@unity-services/cloud-save-1.4');
const { LobbyApi } = require('@unity-services/lobby-1.2');
const { LeaderboardsApi } = require('@unity-services/leaderboards-2.0');

module.exports = async ({params, context}) => {
  const {sessionId, opponentId, wins, losses} = params;
  const me = context.playerId, project = context.projectId;
  if (!me || me === opponentId || !/^[A-Za-z0-9_-]{1,40}$/.test(sessionId || '') ||
      !Number.isInteger(wins) || !Number.isInteger(losses) ||
      !((wins === 3 && losses >= 0 && losses < 3) || (losses === 3 && wins >= 0 && wins < 3)))
    throw Error('Invalid match result');
  const cloud = new DataApi(context), boards = new LeaderboardsApi(context);
  const custom = 'match_' + sessionId;
  const read = async (id,key) => (await cloud.getPrivateCustomItems(project,id,[key])).data.results[0];
  const write = (id,key,value,lock) => cloud.setPrivateCustomItem(project,id,{key,value,...(lock?{writeLock:lock}:{})});
  let meta = await read(custom,'meta');
  if (!meta) {
    const lobby = (await new LobbyApi(context).getLobby(sessionId, undefined, me)).data;
    const ids = lobby.players.map(p=>p.id).sort();
    if (lobby.isPrivate || lobby.name !== 'shako-duel-v1' || ids.length !== 2 || !ids.includes(me) || !ids.includes(opponentId))
      throw Error('Not a public two-player match');
    const age = Date.now() - Date.parse(lobby.created);
    if (!Number.isFinite(age) || age < 0 || age > 2*60*60*1000) throw Error('Match expired');
    await write(custom,'meta',{players:ids,season:new Date().toISOString().slice(0,7),created:Date.now(),lease:0,done:false});
    meta = await read(custom,'meta');
  }
  if (!meta.value.players.includes(me) || !meta.value.players.includes(opponentId)) throw Error('Wrong participants');
  if (meta.value.season !== new Date().toISOString().slice(0,7)) throw Error('Season closed');
  const prior = await read(custom,me);
  if (prior && (prior.value.wins !== wins || prior.value.losses !== losses)) throw Error('Conflicting duplicate');
  if (!prior) await write(custom,me,{wins,losses});
  const other = await read(custom,opponentId);
  if (!other) return {status:'waiting'};
  if (other.value.wins !== losses || other.value.losses !== wins) throw Error('Disputed result');
  meta = await read(custom,'meta');
  if (meta.value.done) return {status:'confirmed'};
  if (meta.value.lease > Date.now()) return {status:'processing'};
  const record = meta.value; record.lease=Date.now()+30000;
  try { await write(custom,'meta',record,meta.writeLock); }
  catch(e) { if(e.response?.status===409)return {status:'processing'};throw e; }
  const ids=record.players;
  const initial={rating:1000,streak:0,best:0,receipts:[]};
  const states=[];
  for(const id of ids) states.push((await read('rank_'+id,record.season))?.value || JSON.parse(JSON.stringify(initial)));
  if(!record.delta){
    const firstWon=ids[0]===me?wins===3:losses===3;
    const change=Math.round(24*((firstWon?1:0)-1/(1+Math.pow(10,(states[1].rating-states[0].rating)/400))));
    record.delta=[change,-change];record.winner=firstWon?ids[0]:ids[1];
    const current=await read(custom,'meta');await write(custom,'meta',record,current.writeLock);
  }
  for(let i=0;i<ids.length;i++){
    const id=ids[i],key='rank_'+id;
    let saved;
    for(let attempt=0;attempt<6;attempt++){
      const current=await read(key,record.season);
      const state=current?.value || JSON.parse(JSON.stringify(initial));
      if(state.receipts.includes(sessionId)){saved=state;break;}
      state.rating=Math.max(0,state.rating+record.delta[i]);state.streak=id===record.winner?state.streak+1:0;state.best=Math.max(state.best,state.streak);state.receipts.push(sessionId);
      try{await write(key,record.season,state,current?.writeLock);saved=state;break;}
      catch(e){if(e.response?.status!==409)throw e;}
    }
    if(!saved)throw Error('Retry result submission');
    await boards.addLeaderboardPlayerScore(project,'shako_monthly_rating',id,{score:saved.rating});
    await boards.addLeaderboardPlayerScore(project,'shako_monthly_streak',id,{score:saved.best});
  }
  const current=await read(custom,'meta');record.done=true;record.lease=0;await write(custom,'meta',record,current.writeLock);
  return {status:'confirmed'};
};
module.exports.params={sessionId:{type:'String',required:true},matchId:{type:'String',required:true},opponentId:{type:'String',required:true},wins:{type:'Numeric',required:true},losses:{type:'Numeric',required:true}};
