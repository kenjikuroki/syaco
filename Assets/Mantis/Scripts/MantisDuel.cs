using UnityEngine;
using UnityEngine.InputSystem;

namespace MantisPunch
{
    public sealed class MantisDuel : MonoBehaviour
    {
        public MantisPlayer player, cpu;
        public Camera arenaCamera;
        public Shader indicatorShader;
        public DuelFighter Player { get; private set; }
        public DuelFighter Enemy { get; private set; }
        public bool Testing;
        public int playerWins, cpuWins;
        public bool RoundOver => Player && (!Player.Alive || !Enemy.Alive);
        public bool MatchOver => playerWins >= 3 || cpuWins >= 3;
        readonly DuelBrain brain = new DuelBrain();
        readonly MantisControls controls = new MantisControls();
        Vector3 cameraVelocity;
        Quaternion playerModelRotation, cpuModelRotation;
        bool scored, inRange;
        float roundTime;
        LineRenderer ring;
        Material ringMaterial;
        CombatEffects effects;
        float resultAge;
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
            playerWins = cpuWins = 0; NextRound();
        }
        public void NextRound()
        {
            Player.ResetRound(new Vector3(0, 0, -1.6f), Quaternion.identity);
            Enemy.ResetRound(new Vector3(0, 0, 2.4f), Quaternion.Euler(0, 180, 0));
            player.visual.transform.localRotation = playerModelRotation;
            cpu.visual.transform.localRotation = cpuModelRotation;
            scored = false; roundTime = 0; controls.Reset(); brain.Reset();
            resultAge = 0; Demo = 0; if (effects) effects.Clear();
            bufferedCounter = false;
        }
        void Start()
        {
            Player = player.GetComponent<DuelFighter>(); Enemy = cpu.GetComponent<DuelFighter>();
            if (!Player) Player = player.gameObject.AddComponent<DuelFighter>();
            if (!Enemy) Enemy = cpu.gameObject.AddComponent<DuelFighter>();
            Player.Initialize(); Enemy.Initialize();
            playerModelRotation = player.visual.transform.localRotation; cpuModelRotation = cpu.visual.transform.localRotation;
            effects = gameObject.AddComponent<CombatEffects>(); effects.Initialize(indicatorShader);
            arenaCamera.fieldOfView = 52;
            ShakeStrength = Mathf.Clamp01(PlayerPrefs.GetFloat("Mantis.Shake", .6f));
            var fill = new GameObject("Shoulder camera fill").AddComponent<Light>();
            fill.transform.SetParent(arenaCamera.transform, false); fill.type = LightType.Directional;
            fill.intensity = .65f; fill.color = new Color(.78f, .9f, 1); fill.shadows = LightShadows.None;
            ring = new GameObject("Reach indicator").AddComponent<LineRenderer>();
            ringMaterial = new Material(indicatorShader ? indicatorShader : player.visual.GetComponentInChildren<Renderer>().sharedMaterial.shader);
            ring.sharedMaterial = ringMaterial; ring.useWorldSpace = true; ring.positionCount = 49; ring.widthMultiplier = .025f;
            Restart();
        }
        Rect ActionRect(int index)
        {
            float s = controls.Scale;
            Vector2 center = MantisControls.ToGui(controls.AttackCenter + Vector2.left * (index * 137 * s));
            return new Rect(center.x - 58 * s, center.y - 58 * s, 116 * s, 116 * s);
        }
        DuelAction PointerAction(Vector2 screen)
        {
            Vector2 p = MantisControls.ToGui(screen);
            for (int i = 0; i < 3; i++) if (ActionRect(i).Contains(p)) return i == 0 ? DuelAction.Attack : i == 1 ? DuelAction.Feint : DuelAction.Parry;
            return DuelAction.None;
        }
        DuelAction Poll()
        {
            controls.Poll(new Rect(15 * controls.Scale, Screen.height - 265 * controls.Scale, 175 * controls.Scale, 65 * controls.Scale));
            DuelAction action = controls.ConsumeAttack() ? DuelAction.Attack : DuelAction.None;
            var k = Keyboard.current;
            if (k?.kKey.wasPressedThisFrame == true) action = DuelAction.Feint;
            if (k?.lKey.wasPressedThisFrame == true) action = DuelAction.Parry;
            if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            { var pointer = PointerAction(Mouse.current.position.ReadValue()); if (pointer != DuelAction.None) action = pointer; }
            if (Touchscreen.current != null)
                foreach (var t in Touchscreen.current.touches)
                    if (t.press.wasPressedThisFrame) { var pointer = PointerAction(t.position.ReadValue()); if (pointer != DuelAction.None) action = pointer; }
            return action;
        }
        void Update()
        {
            if (!Player || Testing) return;
            if (Keyboard.current?.rKey.wasPressedThisFrame == true) { Restart(); return; }
            if (GetComponent<HitComparison>()?.Active != true)
            {
            if (Keyboard.current?.digit1Key.wasPressedThisFrame == true) StartDemo(1);
            if (Keyboard.current?.digit2Key.wasPressedThisFrame == true) StartDemo(2);
            if (Keyboard.current?.digit3Key.wasPressedThisFrame == true) StartDemo(3);
            if (Keyboard.current?.digit4Key.wasPressedThisFrame == true) StartDemo(4);
            }
            if (RoundOver) { Score(); resultAge += Time.deltaTime; return; }
            DuelAction input = Poll();
            BufferCounter(input);
            if (effects.HitStop > 0) return;
            if (bufferedCounter)
            { if (Player.Available) Player.Begin(DuelAction.Attack); bufferedCounter = false; }
            float dt = Time.deltaTime * effects.CombatSpeed;
            if (Demo != 0)
            {
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
            Demo = kind; demoAge = 0; demoResponded = false; demoHits = 0;
        }
        public void Advance(float dt, Vector2 humanMove, Vector2 cpuMove)
        {
            Player.Step(dt, humanMove, Enemy); Enemy.Step(dt, cpuMove, Player);
            DuelFighter.Resolve(Player, Enemy);
            if (RoundOver) Score();
        }
        void Score()
        {
            if (scored) return; scored = true;
            if (!Player.Alive) cpuWins++; else playerWins++;
        }
        void LateUpdate()
        {
            if (!Player) return;
            Vector3 front = player.transform.forward, right = player.transform.right;
            float gap = Vector3.Distance(player.transform.position, cpu.transform.position);
            Vector3 center = player.transform.position + front * Mathf.Clamp(gap * .55f, .8f, 2.8f) + Vector3.up * .85f;
            float distance = Mathf.Clamp(3.3f + gap * .2f, 3.7f, 6);
            if (RoundOver)
            { center = Vector3.Lerp(center, (Player.presentation.VisiblePosition + Enemy.presentation.VisiblePosition) * .5f + Vector3.up * .6f, .65f); distance = 5.3f; }
            Vector3 desiredCamera = player.transform.position - front * distance + right * 1.8f + Vector3.up * 2.8f;
            arenaCamera.transform.position = Vector3.SmoothDamp(arenaCamera.transform.position, desiredCamera, ref cameraVelocity, .17f);
            arenaCamera.transform.LookAt(center);

            arenaCamera.fieldOfView = Mathf.Lerp(arenaCamera.fieldOfView, RoundOver && effects.Zoom < .01f ? 59 : 52 - effects.Zoom, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 28));
            if (effects && effects.Shake > 0) arenaCamera.transform.position += (effects.VerticalShake ? arenaCamera.transform.up : arenaCamera.transform.right) * Mathf.Sin(Time.unscaledTime * 93) * effects.Shake * ShakeStrength;
            inRange = Player.CanReach(Enemy, inRange ? .035f : 0);
            ringMaterial.SetColor("_BaseColor", inRange ? new Color(.4f, 1, .85f) : new Color(.18f, .35f, .4f));
            ring.widthMultiplier = inRange ? .045f : .02f;
            for (int i = 0; i < 49; i++)
            { float angle = i / 48f * (inRange ? 360 : 295) * Mathf.Deg2Rad; ring.SetPosition(i, player.transform.position + new Vector3(Mathf.Cos(angle) * .7f, .025f, Mathf.Sin(angle) * .7f)); }
        }
        void OnApplicationFocus(bool focus) { if (!focus) controls.Reset(); }
        void OnDestroy() { if (ring) Destroy(ring.gameObject); if (ringMaterial) Destroy(ringMaterial); }
        void Fill(Rect rect, Color color) { Color old = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old; }
        void Bar(Rect rect, DuelFighter fighter, string title, GUIStyle label)
        {
            GUI.Label(new Rect(rect.x, rect.y - 29, rect.width, 28), title, label);
            Fill(rect, new Color(.08f, .12f, .16f));
            Color color = fighter.Guard < 35 ? new Color(1, .3f, .25f) : fighter.Guard < 68 ? new Color(1, .75f, .2f) : new Color(.3f, .88f, .75f);
            Fill(new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * fighter.Guard / 100, rect.height - 4), color);
            float breakAge = Time.unscaledTime - fighter.GuardBreakAt;
            if (breakAge >= 0 && breakAge < .65f)
                for (int i = 0; i < 12; i++)
                {
                    float x = rect.x + rect.width * i / 12 + (i - 5.5f) * breakAge * 13;
                    float y = rect.y + breakAge * (35 + i % 3 * 23) + 90 * breakAge * breakAge;
                    Fill(new Rect(x, y, rect.width / 15, 6), new Color(1, .3f, .12f, 1 - breakAge / .65f));
                }
        }
        void Status(DuelFighter f, GUIStyle label)
        {
            Vector3 screen = arenaCamera.WorldToScreenPoint(f.transform.position + Vector3.up * 1.1f);
            string status = f.State == DuelState.Stunned ? "*  *  *\nDIZZY" : f.FeedbackLeft > 0 ? f.Feedback : f.State == DuelState.Guard ? "GUARD" : f.State == DuelState.Parry ? "PARRY" : f.State == DuelState.Recovery ? "OPEN" : f.Age < .2f ? "" : f.State == DuelState.Feint ? "FEINT" : f.State == DuelState.Attack ? "PUNCH" : "";
            GUI.Label(new Rect(screen.x - 110, Screen.height - screen.y - 35, 220, 70), status, label);
            if (f.State == DuelState.Stunned)
                for (int i = 0; i < 3; i++)
                {
                    float angle = Time.unscaledTime * 5 + i * Mathf.PI * 2 / 3;
                    GUI.Label(new Rect(screen.x + Mathf.Cos(angle) * 35 - 12, Screen.height - screen.y + Mathf.Sin(angle) * 9 - 55, 24, 24), "*", label);
                }
        }
        void OnGUI()
        {
            if (!Player) return;
            float s = controls.Scale;
            var label = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(20 * s), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            var small = new GUIStyle(label) { fontSize = Mathf.RoundToInt(13 * s), fontStyle = FontStyle.Normal };
            Fill(new Rect(0, 0, Screen.width, 105 * s), new Color(.02f, .06f, .08f, .95f));
            Bar(new Rect(25 * s, 43 * s, Screen.width * .29f, 18 * s), Player, "YOU / GUARD", label);
            Bar(new Rect(Screen.width * .71f - 25 * s, 43 * s, Screen.width * .29f, 18 * s), Enemy, "CPU / GUARD", label);
            GUI.Label(new Rect(Screen.width * .34f, 8 * s, Screen.width * .32f, 60 * s), playerWins + "  :  " + cpuWins + "\nFIRST TO 3", label);
            var comparison = GetComponent<HitComparison>();
            GUI.Label(new Rect(0, 76 * s, Screen.width, 25 * s), comparison && comparison.Active ? comparison.Caption : "WASD MOVE   J ATTACK   K FEINT   L PARRY   R RESET     DEMO: 1 GUARD / 2 PARRY / 3 HIT / 4 BREAK", small);
            GUI.Label(new Rect(22 * s, Screen.height - 260 * s, 160 * s, 25 * s), "SHAKE " + Mathf.RoundToInt(ShakeStrength * 100) + "%", small);
            float shake = GUI.HorizontalSlider(new Rect(30 * s, Screen.height - 230 * s, 145 * s, 22 * s), ShakeStrength, 0, 1);
            if (Mathf.Abs(shake - ShakeStrength) > .001f) { ShakeStrength = shake; PlayerPrefs.SetFloat("Mantis.Shake", shake); }
            for (int i = 0; i < 3; i++)
            {
                Rect rect = ActionRect(i); Fill(rect, Player.Available ? new Color(.1f, .3f, .34f, .9f) : new Color(.13f, .17f, .19f, .8f));
                GUI.Label(rect, i == 0 ? "ATTACK\nJ / SPACE" : i == 1 ? "FEINT\nK" : "PARRY\nL", small);
            }
            Vector2 stick = MantisControls.ToGui(controls.StickCenter);
            Fill(new Rect(stick.x - 75 * s, stick.y - 75 * s, 150 * s, 150 * s), new Color(.07f, .22f, .26f, .8f));
            Vector2 knob = MantisControls.ToGui(controls.StickCenter + controls.Stick * controls.Radius);
            Fill(new Rect(knob.x - 18 * s, knob.y - 18 * s, 36 * s, 36 * s), new Color(.5f, .9f, .8f));
            GUI.Label(new Rect(Screen.width * .3f, Screen.height - 42 * s, Screen.width * .4f, 28 * s), inRange ? "IN RANGE - READ YOUR OPPONENT" : "CONTROL THE DISTANCE", small);
            Status(Player, label); Status(Enemy, label);
            if (RoundOver && resultAge > 1.75f)
            {
                Rect box = new Rect(Screen.width * .3f, Screen.height * .28f, Screen.width * .4f, 180 * s);
                Fill(box, new Color(.02f, .06f, .08f, .96f));
                GUI.Label(new Rect(box.x, box.y + 15 * s, box.width, 60 * s), (!Player.Alive ? "CPU TAKES THE ROUND" : "IPPON! YOU WIN") + (MatchOver ? "\nMATCH OVER" : ""), label);
                if (GUI.Button(new Rect(box.x + 25 * s, box.y + 100 * s, box.width - 50 * s, 50 * s), MatchOver ? "REMATCH" : "NEXT ROUND")) { if (MatchOver) Restart(); else NextRound(); }
            }
        }
    }
}





