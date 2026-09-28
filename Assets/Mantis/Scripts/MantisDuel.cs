using UnityEngine;
using UnityEngine.InputSystem;

namespace MantisPunch
{
    public sealed partial class MantisDuel : MonoBehaviour
    {
        public MantisPlayer player, cpu;
        public Camera arenaCamera;
        public Shader indicatorShader;
        public DuelFighter Player { get; private set; }
        public DuelFighter Enemy { get; private set; }
        public bool Testing; public BattleHud Hud {get;private set;} public Vector2 StickInput=>controls.Stick; public bool InReach=>inRange; public bool ShowResult=>RoundOver&&MatchOver&&resultAge>3.2f; public float ResultAge=>resultAge; public const float FightDisplayDuration=.8f; public const float FirstReadyDuration=1.2f+FightDisplayDuration; public const float NextReadyDuration=.55f+FightDisplayDuration; public float ReadyAge=>readyLeft; float readyLeft; public void ResetInput()=>controls.Reset();
        public int playerWins, cpuWins;
        public bool RoundOver => Player && (!Player.Alive || !Enemy.Alive);
        public bool MatchOver => playerWins >= 3 || cpuWins >= 3;
        readonly DuelBrain brain = new DuelBrain();
        readonly MantisControls controls = new MantisControls();
        Vector3 cameraVelocity;
        Quaternion playerModelRotation, cpuModelRotation;
        bool scored, inRange; bool missionDemo; bool rankingMatchRecorded, rankingMatchDemo;
        float roundTime;
        LineRenderer ring;
        Material ringMaterial;
        CombatEffects effects;
        float resultAge; public int MatchShellReward {get;private set;}
        public int Demo { get; private set; }
        float demoAge;
        bool demoResponded;
        int demoHits;
        bool bufferedCounter;
        public void BufferCounter(DuelAction action)
        {
            if (effects.ParryCinematic && Player.Available && Player.Feedback == "PARRY!" && action == DuelAction.Attack)
                bufferedCounter = true;
        }
        public float ShakeStrength = .6f;
        public void Restart()
        {
            rankingMatchRecorded = rankingMatchDemo = false; MatchShellReward=0; playerWins = cpuWins = 0; RollOpponent();NextRound();
        }
        public void NextRound()
        {
            GetComponent<ReefMusic>()?.StartRound();
            Player.ResetRound(new Vector3(0, 0, -1.6f), Quaternion.identity);
            Enemy.ResetRound(new Vector3(0, 0, 2.4f), Quaternion.Euler(0, 180, 0));
            player.visual.transform.localRotation = playerModelRotation;
            cpu.visual.transform.localRotation = cpuModelRotation;
            missionDemo = false; scored = false; roundTime = 0; controls.Reset(); brain.ResetRound(Player.Actions);
            readyLeft = playerWins + cpuWins == 0 ? FirstReadyDuration : NextReadyDuration; resultAge = 0; Demo = 0; if (effects) effects.Clear();
            bufferedCounter = false;
        }
        void Start()
        {
            gameObject.AddComponent<DuelFinishPresentation>(); Player = player.GetComponent<DuelFighter>(); Enemy = cpu.GetComponent<DuelFighter>();
            if (!Player) Player = player.gameObject.AddComponent<DuelFighter>();
            if (!Enemy) Enemy = cpu.gameObject.AddComponent<DuelFighter>();
            Player.Initialize(); Enemy.Initialize();
            playerModelRotation = player.visual.transform.localRotation; cpuModelRotation = cpu.visual.transform.localRotation;
            effects = gameObject.AddComponent<CombatEffects>(); effects.Initialize(indicatorShader);
            arenaCamera.usePhysicalProperties=false; arenaCamera.fieldOfView = 52; controls.BattleLayoutEnabled=true; Hud=gameObject.AddComponent<BattleHud>(); Hud.duel=this;
            ShakeStrength = Mathf.Clamp01(PlayerPrefs.GetFloat("Mantis.Shake", .6f));
            var fill = new GameObject("Shoulder camera fill").AddComponent<Light>();
            fill.transform.SetParent(arenaCamera.transform, false); fill.type = LightType.Directional;
            fill.intensity = .65f; fill.color = new Color(.78f, .9f, 1); fill.shadows = LightShadows.None;
            ring = new GameObject("Reach indicator").AddComponent<LineRenderer>();
            ringMaterial = new Material(indicatorShader ? indicatorShader : player.visual.GetComponentInChildren<Renderer>().sharedMaterial.shader);
            ring.sharedMaterial = ringMaterial; ring.useWorldSpace = true; ring.positionCount = 49; ring.widthMultiplier = .025f;
            Restart();
            if(OnlineMatch.Current&&OnlineMatch.Current.Active)OnlineMatch.Current.Bind(this);
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-boxerBonusTest")>=0)StartCoroutine(VerifyBoxerCombatBonus());
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-cpuVarietyTest")>=0)StartCoroutine(VerifyCpuVariety());
            if(TutorialProgress.Requested){TutorialProgress.Requested=false;Tutorial=true;ResetLesson();if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-tutorialTest")>=0)StartCoroutine(VerifyTutorial());}
        }
        Rect ActionRect(int index) => BattleLayout.GuiRect(BattleLayout.Action(index));
        public DuelAction PointerAction(Vector2 screen)
        {
            Vector2 p = MantisControls.ToGui(screen);
            for (int i = 0; i < 3; i++) if (ActionRect(i).Contains(p)) return i == 0 ? DuelAction.Attack : i == 1 ? DuelAction.Feint : DuelAction.Parry;
            return DuelAction.None;
        }
        DuelAction Poll()
        {
            controls.Poll(new Rect(0,0,Screen.width,(BattleLayout.Portrait?188:110)*BattleLayout.Scale));
            DuelAction action = controls.ConsumeAttack() ? DuelAction.Attack : DuelAction.None;
            var k = Keyboard.current;
            if (Gamepad.current?.buttonNorth.wasPressedThisFrame == true || k?.kKey.wasPressedThisFrame == true) action = DuelAction.Feint;
            if (Gamepad.current?.buttonEast.wasPressedThisFrame == true || k?.lKey.wasPressedThisFrame == true) action = DuelAction.Parry;
            if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            { var pointer = PointerAction(Mouse.current.position.ReadValue()); if (pointer != DuelAction.None) action = pointer; }
            if (Touchscreen.current != null)
                foreach (var t in Touchscreen.current.touches)
                    if (t.press.wasPressedThisFrame) { var pointer = PointerAction(t.position.ReadValue()); if (pointer != DuelAction.None) action = pointer; }
            return action;
        }
        void Update()
        {
            if (!Player || Testing || Hud && Hud.Paused) return;
            if(networkEnded)return;
            if(OnlineMatch.Current&&OnlineMatch.Current.Active){TickNetwork();return;}
            if(Tutorial){if((!LessonStarted||LessonPassed)&&(Keyboard.current?.enterKey.wasPressedThisFrame==true||Gamepad.current?.buttonSouth.wasPressedThisFrame==true))TutorialNext();TickTutorial(Poll(),Time.deltaTime);return;}
            if (Keyboard.current?.rKey.wasPressedThisFrame == true) { Restart(); return; }
            if (GetComponent<HitComparison>()?.Active != true)
            {
            if (Keyboard.current?.digit1Key.wasPressedThisFrame == true) StartDemo(1);
            if (Keyboard.current?.digit2Key.wasPressedThisFrame == true) StartDemo(2);
            if (Keyboard.current?.digit3Key.wasPressedThisFrame == true) StartDemo(3);
            if (Keyboard.current?.digit4Key.wasPressedThisFrame == true) StartDemo(4);
            if (Keyboard.current?.digit5Key.wasPressedThisFrame == true) StartDemo(5);
            }
            if (RoundOver) { Score(); resultAge += Time.deltaTime; if(!MatchOver && resultAge>=1.65f && !missionDemo)NextRound(); return; }
            if(readyLeft>0&&Demo==0){readyLeft=Mathf.Max(0,readyLeft-Time.deltaTime);controls.Reset();return;}
            DuelAction input = Poll();
            BufferCounter(input);
            if (effects.HitStop > 0) return;
            if (bufferedCounter)
            { if (Player.Available) Player.Begin(DuelAction.Attack); bufferedCounter = false; }
            float dt = Time.deltaTime * effects.CombatSpeed;
            if (Demo != 0)
            {
                if (Demo == 5) dt *= .25f;
                demoAge += dt;
                if (Demo == 4)
                {
                    if (demoHits < 3 && demoAge >= .6f + demoHits * .8f && Player.Begin(DuelAction.Attack)) demoHits++;
                    Advance(dt, Vector2.zero, Vector2.zero);
                    if (demoAge > 3.9f) Demo = 0;
                    return;
                }
                if (demoAge >= .6f && !demoResponded)
                { if (Demo == 2) Enemy.Begin(DuelAction.Attack); else Player.Begin(DuelAction.Attack); if (Demo == 3) Enemy.Begin(DuelAction.Feint); demoResponded = true; }
                if (Demo == 2 && demoAge >= .75f && demoAge < .86f) Player.Begin(DuelAction.Parry);
                Advance(dt, Vector2.zero, Vector2.zero);
                if (Demo == 5 && demoAge > 1.3f) { StartDemo(5); return; }
                if (demoAge > 2.3f) Demo = 0;
                return;
            }
            roundTime += dt;
            DuelAction action = effects.ParryCinematic ? DuelAction.None : input;
            brain.Decide(dt, Enemy, Player, out Vector2 movement, out DuelAction enemyAction);
            if (effects.ParryCinematic) enemyAction = DuelAction.None;
            Player.Begin(action); Enemy.Begin(enemyAction);
            Vector3 forward = arenaCamera.transform.forward; forward.y = 0; forward.Normalize();
            Vector3 right = arenaCamera.transform.right; right.y = 0; right.Normalize();
            Vector3 worldMove = forward * controls.Move.y + right * controls.Move.x;
            Advance(dt, new Vector2(worldMove.x, worldMove.z), movement);
        }
        public void StartDemo(int kind)
        {
            NextRound();
            Player.ResetRound(Vector3.zero, Quaternion.identity);
            Enemy.ResetRound(new Vector3(0, 0, 2.4f), Quaternion.Euler(0, 180, 0));
            rankingMatchDemo = true; missionDemo = true; Demo = kind; demoAge = 0; demoResponded = false; demoHits = 0;
            if (kind == 5) demoAge = .5f;
        }
        public void Advance(float dt, Vector2 humanMove, Vector2 cpuMove)
        {
            Player.Step(dt, humanMove, Enemy); Enemy.Step(dt, cpuMove, Player);
            int parriesBefore = Player.SuccessfulParries;
            DuelFighter.Resolve(Player, Enemy);
            if (!Testing && !missionDemo && Player.SuccessfulParries > parriesBefore) MissionStore.Record(2, Player.SuccessfulParries - parriesBefore);
            if (RoundOver) Score();
        }
        void Score()
        {
            if(Tutorial)return;
            if (scored) return; scored = true;
            if (!Player.Alive) cpuWins++; else playerWins++;
            if (missionDemo) rankingMatchDemo = true;
            if (!Testing && !missionDemo) { MissionStore.Record(0); if (Player.Alive) MissionStore.Record(1); }
            if (MatchOver && !rankingMatchRecorded) { rankingMatchRecorded = true; if (!Testing && !rankingMatchDemo) { if(OnlineMatch.Current&&OnlineMatch.Current.Active)RecordOnlineResult();else {RankingStore.RecordCpuMatch(playerWins >= 3); MatchShellReward=MissionStore.CompleteMatch(playerWins>=3);} } }
        }
        void LateUpdate()
        {
            if (!Player || Hud && Hud.Paused) return; if(MatchOver&&RoundOver&&resultAge>=.8f&&!Testing){ring.enabled=false;return;}ring.enabled=true;
            Vector3 front = player.transform.forward, right = player.transform.right;
            float gap = Vector3.Distance(player.transform.position, cpu.transform.position);
            Vector3 center = player.transform.position + front * Mathf.Clamp(gap * .55f, .8f, 2.8f) + Vector3.up * .85f;
            float distance = Mathf.Clamp(3.3f + gap * .2f, 3.7f, 6);
            if (RoundOver)
            { center = Vector3.Lerp(center, (Player.presentation.VisiblePosition + Enemy.presentation.VisiblePosition) * .5f + Vector3.up * .6f, .65f); distance = 5.3f; }
            Vector3 desiredCamera = player.transform.position - front * distance + right * 1.8f + Vector3.up * 2.8f;
            if (BattleLayout.Portrait) { desiredCamera=player.transform.position-front*(distance+1.5f)+right*.9f+Vector3.up*3.6f; if(!RoundOver)center=player.transform.position+front*1.0f+Vector3.up*.35f; }
            if (Demo == 5) { center = player.transform.position + front * .65f + Vector3.up * .65f; desiredCamera = center + right * 3.2f + Vector3.up * .7f; }
            arenaCamera.transform.position = Vector3.SmoothDamp(arenaCamera.transform.position, desiredCamera, ref cameraVelocity, .17f);
            arenaCamera.transform.LookAt(center);

            arenaCamera.fieldOfView = Mathf.Lerp(arenaCamera.fieldOfView, RoundOver && effects.Zoom < .01f ? 59 : 52 - effects.Zoom, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 28));
            if (effects && effects.Shake > 0) arenaCamera.transform.position += (effects.VerticalShake ? arenaCamera.transform.up : arenaCamera.transform.right) * Mathf.Sin(Time.unscaledTime * 93) * effects.Shake * ShakeStrength;
            inRange = Player.CanReach(Enemy, inRange ? .035f : 0);
            ringMaterial.SetColor("_BaseColor", inRange ? new Color(.4f, 1, .85f) : new Color(.18f, .35f, .4f));
            ring.widthMultiplier = inRange ? .045f : .02f;
            for (int i = 0; i < 49; i++)
            { float angle = i / 48f * 360 * Mathf.Deg2Rad; ring.SetPosition(i, player.transform.position + new Vector3(Mathf.Cos(angle) * .7f, .025f, Mathf.Sin(angle) * .7f)); }
        }
        void OnApplicationFocus(bool focus) { if (!focus) controls.Reset(); }
        void OnDestroy() { if (ring) Destroy(ring.gameObject); if (ringMaterial) Destroy(ringMaterial); }
    }
}





