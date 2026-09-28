using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace MantisPunch {
[Serializable] public sealed class PeerIdentity {
 public string name,id;public int robot,boxer,unicorn;public bool crown;public int[] colors;
 public static PeerIdentity Local()=>new PeerIdentity{name=CharacterProfile.Name,id=OnlineServices.PlayerId,robot=RobotEquipment.Mask,boxer=BoxerEquipment.Mask,unicorn=UnicornEquipment.Mask,crown=MissionStore.Get().crownEquipped,colors=MantisAppearance.Load()};
 public bool Valid()=>!string.IsNullOrWhiteSpace(name)&&name.Length<=256&&colors!=null&&colors.Length==6&&robot>=0&&robot<=63&&boxer>=0&&boxer<=63&&unicorn>=0&&unicorn<=63&&(robot&boxer)==0&&(robot&unicorn)==0&&(boxer&unicorn)==0;
 public void Apply(MantisVisual visual){var body=visual.GetComponent<MantisModularBody>();var load=visual.GetComponent<MantisLoadout>()??visual.gameObject.AddComponent<MantisLoadout>();load.SetCrown(crown);MantisAppearance.Apply(body,colors);load.SetRobot(robot);load.SetBoxer(boxer);load.SetUnicorn(unicorn);}
}
public sealed class OnlineMatch:MonoBehaviour {
 static OnlineMatch instance;public static OnlineMatch Instance{get{if(!instance){instance=new GameObject("Online match").AddComponent<OnlineMatch>();DontDestroyOnLoad(instance.gameObject);}return instance;}}
 public static OnlineMatch Current=>instance;
 public static bool OpenSearchOnHome;
 public MatchKind Kind{get;private set;}=MatchKind.Cpu;
 public bool Active{get;private set;} public bool Busy{get;private set;} public bool Host=>manager&&manager.IsHost;
 public bool Connected=>manager&&manager.IsConnectedClient&&peer!=ulong.MaxValue;
 public bool GameReady=>Active&&duel&&remoteReady;
 public bool LocalRematch{get;private set;} public bool RemoteRematch{get;private set;}
 public string Status{get;private set;}="";public string Code=>session?.Code??"";public string MatchId{get;private set;}="";public PeerIdentity Remote{get;private set;}
 public Vector2 RemoteMove{get;private set;} public DuelAction TakeAction(){var a=remoteAction;remoteAction=DuelAction.None;return a;}
 NetworkManager manager;ISession session;ulong peer=ulong.MaxValue;MantisDuel duel;DuelAction remoteAction;int generation;bool leaving,remoteReady,sceneStarted;float lastHello,lastInput,lastSnapshot,lastSeen,lastPing,remoteInputAt;bool direct;
 static string T(string ja,string en)=>MantisDuel.Tr(ja,en);
 void Setup(){if(manager)return;var go=new GameObject("Mantis network");DontDestroyOnLoad(go);var transport=go.AddComponent<UnityTransport>();manager=go.AddComponent<NetworkManager>();manager.NetworkConfig=new NetworkConfig{NetworkTransport=transport,EnableSceneManagement=false,ConnectionApproval=true,TickRate=30};
  manager.ConnectionApprovalCallback=(request,response)=>{response.Approved=manager.ConnectedClientsIds.Count<2;response.CreatePlayerObject=false;response.Pending=false;};
  manager.OnClientConnectedCallback+=id=>{if(id!=manager.LocalClientId){peer=id;lastSeen=Time.realtimeSinceStartup;}if(!manager.IsHost){peer=NetworkManager.ServerClientId;lastSeen=Time.realtimeSinceStartup;}Register();};
  manager.OnClientDisconnectCallback+=id=>{if(leaving)return;if(Active){Status=T("相手との接続が切れました。試合は無効です。","Opponent disconnected. Match void.");Active=false;RemoteMove=Vector2.zero;if(duel)duel.NetworkDisconnected();}else if(Busy&&id==peer){peer=ulong.MaxValue;Remote=null;}};
 }
 void Register(){manager.CustomMessagingManager.RegisterNamedMessageHandler("shako",Receive);}
 public async Task StartSearch(MatchKind kind,string code=null){
  if(Busy||Active)return;Busy=true;Kind=kind;int token=++generation;Status=T("接続中…","Connecting…");
  try{await OnlineServices.SignIn();if(token!=generation)return;Setup();
   var options=new SessionOptions{MaxPlayers=2,IsPrivate=kind==MatchKind.Friend,Name="shako-duel-v1"}.WithRelayNetwork();
   ISession found;
   if(kind==MatchKind.Friend)found=string.IsNullOrEmpty(code)?await MultiplayerService.Instance.CreateSessionAsync(options):await MultiplayerService.Instance.JoinSessionByCodeAsync(code.Trim().ToUpperInvariant());
   else found=await MultiplayerService.Instance.MatchmakeSessionAsync(new QuickJoinOptions{Timeout=TimeSpan.FromSeconds(2),CreateSession=true,Filters=new System.Collections.Generic.List<FilterOption>{new FilterOption(FilterField.Name,"shako-duel-v1",FilterOperation.Equal)}},options);
   if(token!=generation){await found.LeaveAsync();return;}session=found;Register();Status=kind==MatchKind.Friend?T("ルームコードを友達に伝えてください： ","Share this room code: ")+Code:T("対戦相手を探しています…","Searching for an opponent…");
   if(manager.IsHost)foreach(var id in manager.ConnectedClientsIds)if(id!=manager.LocalClientId)peer=id;
   else if(manager.IsConnectedClient)peer=NetworkManager.ServerClientId;
   float deadline=Time.realtimeSinceStartup+25;
   while(token==generation&&!Active){if(kind==MatchKind.Public&&Time.realtimeSinceStartup>deadline){await Leave();Kind=MatchKind.Cpu;Status=T("相手が見つからないためBOTと対戦します。","No opponent found. Starting a BOT match.");SceneManager.LoadScene("Duel");return;}await Task.Delay(100);}
  }catch(Exception ex){if(token==generation){await Leave();Status=T("オンライン接続に失敗しました。再試行できます。","Online connection failed. Please retry.");Debug.LogWarning("Match connection: "+ex.GetType().Name);}}
  finally{if(token==generation)Busy=false;}
 }
 public void StartDirect(bool host){Setup();direct=true;Kind=MatchKind.Friend;Busy=true;manager.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1",7789);if(host)manager.StartHost();else manager.StartClient();Register();}
 void Update(){
  if(!Busy&&!Active)return;if(!Connected)return;
  if(Time.realtimeSinceStartup-lastPing>2){lastPing=Time.realtimeSinceStartup;Send("ping","");}
  if(Time.realtimeSinceStartup-remoteInputAt>.3f)RemoteMove=Vector2.zero;
  if(Remote==null&&Time.realtimeSinceStartup-lastHello>.5f){lastHello=Time.realtimeSinceStartup;Send("hello",JsonUtility.ToJson(PeerIdentity.Local()));}
  if(Host&&Remote!=null&&!sceneStarted){MatchId=Guid.NewGuid().ToString("N");Send("start",MatchId);Launch();}
  if(Active&&duel&&!remoteReady&&Time.realtimeSinceStartup-lastHello>.3f){lastHello=Time.realtimeSinceStartup;Send("ready","");}
  if(Active&&Time.realtimeSinceStartup-lastSeen>15){Status=T("通信が途切れました。試合は無効です。","Connection lost. Match void.");Active=false;duel?.NetworkDisconnected();}
  if(Host&&GameReady&&Time.realtimeSinceStartup-lastSnapshot>=1f/20){lastSnapshot=Time.realtimeSinceStartup;Send("state",JsonUtility.ToJson(duel.CaptureNetwork()));}
  if(Host&&GameReady&&Kind==MatchKind.Friend&&LocalRematch&&RemoteRematch){LocalRematch=RemoteRematch=false;MatchId=Guid.NewGuid().ToString("N");Send("rematch",MatchId);duel.Restart();}
 }
 void Launch(){if(sceneStarted)return;sceneStarted=true;Active=true;Busy=false;lastSeen=Time.realtimeSinceStartup;OnlineRanking.ResultStatus="";LockSession();TutorialProgress.Requested=false;SceneManager.LoadScene("Duel");}
 async void LockSession(){try{if(session!=null&&Host){session.AsHost().IsLocked=true;await session.AsHost().SavePropertiesAsync();}}catch(Exception ex){Debug.LogWarning("Session lock: "+ex.GetType().Name);}}
 public void Bind(MantisDuel game){duel=game;Remote.Apply(game.cpu.visual);game.SetOnlineName(Remote.name);Send("ready","");}
 public void Input(Vector2 move,DuelAction action){if(Host||!Active)return;if(action==DuelAction.None&&Time.realtimeSinceStartup-lastInput<1f/30)return;lastInput=Time.realtimeSinceStartup;Send("input",JsonUtility.ToJson(new NetInput{x=move.x,y=move.y,action=(int)action}));}
 public void VoteRematch(){if(!MatchRules.CanRematch(Kind,Connected)||!duel||!duel.MatchOver)return;LocalRematch=true;Send("vote","");}
 public void Send(string kind,string payload){if(!Busy&&!Active)return;if(!Connected)return;string data=kind+"\n"+payload;using(var writer=new FastBufferWriter(16384,Allocator.Temp)){writer.WriteValueSafe(data);manager.CustomMessagingManager.SendNamedMessage("shako",peer,writer,NetworkDelivery.ReliableFragmentedSequenced);}}
 void Receive(ulong sender,FastBufferReader reader){
  if(sender!=peer||reader.Length>16384)return;try{reader.ReadValueSafe(out string data);int cut=data.IndexOf('\n');if(cut<0)return;string kind=data.Substring(0,cut),value=data.Substring(cut+1);lastSeen=Time.realtimeSinceStartup;
   if(kind=="hello"){var identity=JsonUtility.FromJson<PeerIdentity>(value);if(identity==null||!identity.Valid())return;if(Remote==null){Remote=identity;Send("hello",JsonUtility.ToJson(PeerIdentity.Local()));}}
   else if(kind=="start"&&!Host&&Remote!=null){MatchId=value;Launch();}
   else if(kind=="ready"){remoteReady=true;if(duel)Send("readyAck","");}
   else if(kind=="readyAck")remoteReady=true;
   else if(kind=="input"&&Host&&GameReady){var input=JsonUtility.FromJson<NetInput>(value);if(!float.IsFinite(input.x)||!float.IsFinite(input.y)||input.action<0||input.action>3)return;remoteInputAt=Time.realtimeSinceStartup;RemoteMove=Vector2.ClampMagnitude(new Vector2(input.x,input.y),1);if(input.action!=0)remoteAction=(DuelAction)input.action;}
   else if(kind=="state"&&!Host&&duel)duel.ApplyNetwork(JsonUtility.FromJson<NetDuel>(value));
   else if(kind=="vote"&&Kind==MatchKind.Friend&&duel&&duel.MatchOver)RemoteRematch=true;
   else if(kind=="rematch"&&!Host&&Kind==MatchKind.Friend&&LocalRematch){MatchId=value;LocalRematch=RemoteRematch=false;duel.Restart();}
  }catch(Exception ex){Debug.LogWarning("Invalid network message: "+ex.GetType().Name);}
 }
 public async Task Leave(){generation++;leaving=true;Active=Busy=false;var old=session;session=null;try{if(old!=null)await old.LeaveAsync();}catch(Exception){ }finally{if(manager){manager.Shutdown();Destroy(manager.gameObject);manager=null;}peer=ulong.MaxValue;Remote=null;duel=null;remoteReady=sceneStarted=LocalRematch=RemoteRematch=false;remoteAction=DuelAction.None;RemoteMove=Vector2.zero;leaving=false;Kind=MatchKind.Cpu;}}
 public async void GoHome(){await Leave();Time.timeScale=1;SceneManager.LoadScene("Home");}
 public string SessionId=>session?.Id??"";
 public bool Direct=>direct;
}
[Serializable] public sealed class NetInput {public float x,y;public int action;}
}
