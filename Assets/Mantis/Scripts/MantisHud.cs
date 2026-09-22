using Input = MantisPunch.PrototypeInput;
using UnityEngine;

namespace MantisPunch
{
    public sealed class MantisHud : MonoBehaviour
    {
        public MantisPlayer player;
        public MantisCamera followCamera;
        public bool tuning;
        Texture2D disc;
        GUIStyle title, label, small, button;
        public Rect PanelRect => tuning ? new Rect(Screen.width - 352 * player.controls.Scale, 65 * player.controls.Scale, 336 * player.controls.Scale, 330 * player.controls.Scale) : new Rect();
        void Awake()
        {
            Application.targetFrameRate = 60;
            disc = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[128 * 128];
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(63.5f, 63.5f));
                pixels[y * 128 + x] = new Color(1, 1, 1, Mathf.Clamp01(64 - d));
            }
            disc.SetPixels(pixels); disc.Apply();
        }
        void Update()
        {
            float s = player.controls.Scale;
            player.InputBlock = tuning ? PanelRect : new Rect(Screen.width - 220 * s, 0, 220 * s, 65 * s);
            if (Input.GetKeyDown(KeyCode.Tab)) tuning = !tuning;
        }
        void Styles(float s)
        {
            title = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(25 * s), fontStyle = FontStyle.Bold };
            label = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(17 * s) };
            small = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(13 * s) };
            button = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(16 * s) };
            title.normal.textColor = label.normal.textColor = small.normal.textColor = new Color(.88f, .96f, .95f);
        }
        void Box(Rect rect, Color color) { GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white; }
        void Circle(Vector2 center, float radius, Color color)
        {
            Vector2 p = MantisControls.ToGui(center);
            GUI.color = color; GUI.DrawTexture(new Rect(p.x - radius, p.y - radius, radius * 2, radius * 2), disc); GUI.color = Color.white;
        }
        void OnGUI()
        {
            if (!player) return;
            float s = player.controls.Scale; Styles(s);
            Box(new Rect(0, 0, Screen.width, 62 * s), new Color(.025f, .07f, .09f, .88f));
            GUI.Label(new Rect(22 * s, 8 * s, 350 * s, 34 * s), "MANTIS / PUNCH LAB", title);
            GUI.Label(new Rect(23 * s, 39 * s, 720 * s, 23 * s), "WASD / arrows: move    SPACE / J: punch    R: reset targets    TAB: tuning", small);
            if (GUI.Button(new Rect(Screen.width - 160 * s, 14 * s, 140 * s, 34 * s), tuning ? "CLOSE TUNING" : "TUNING", button)) tuning = !tuning;
            Circle(player.controls.StickCenter, player.controls.Radius, new Color(.07f, .20f, .23f, .72f));
            Circle(player.controls.StickCenter, player.controls.Radius - 3 * s, new Color(.13f, .34f, .37f, .65f));
            Circle(player.controls.StickCenter + player.controls.Stick * player.controls.Radius * .75f, 27 * s, new Color(.66f, .87f, .80f, .95f));
            Circle(player.controls.AttackCenter, 62 * s, player.Attacking ? new Color(.40f, .32f, .20f, .90f) : new Color(.88f, .56f, .24f, .94f));
            var centered = new GUIStyle(label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            Vector2 a = MantisControls.ToGui(player.controls.AttackCenter);
            GUI.Label(new Rect(a.x - 62 * s, a.y - 26 * s, 124 * s, 52 * s), player.Attacking ? "PUNCHING" : "PUNCH", centered);
            GUI.Label(new Rect(Screen.width * .5f - 150 * s, Screen.height - 44 * s, 300 * s, 30 * s), "HITS  " + player.TotalHits + "     /     Practice on the buoys", centered);
            if (Time.time - player.LastHitTime < .45f)
                GUI.Label(new Rect(Screen.width * .5f - 70 * s, Screen.height * .35f, 140 * s, 40 * s), "HIT!", new GUIStyle(title) { alignment = TextAnchor.MiddleCenter });
            foreach (TrainingTarget target in FindObjectsByType<TrainingTarget>(FindObjectsSortMode.None))
            {
                Vector3 screen = Camera.main.WorldToScreenPoint(target.transform.position + Vector3.up * 1.35f);
                if (screen.z <= 0) continue;
                Rect bar = new Rect(screen.x - 30 * s, Screen.height - screen.y, 60 * s, 6 * s);
                Box(bar, new Color(.03f, .06f, .07f, .8f)); bar.width *= target.Health / (float)target.maximumHealth;
                Box(bar, new Color(.92f, .70f, .34f));
            }
            if (tuning) DrawTuning(s);
        }
        void DrawTuning(float s)
        {
            Rect panel = PanelRect; Box(panel, new Color(.025f, .07f, .09f, .96f));
            GUILayout.BeginArea(new Rect(panel.x + 16 * s, panel.y + 10 * s, panel.width - 32 * s, panel.height - 20 * s));
            GUI.Label(new Rect(0, 0, panel.width, 30 * s), "ADJUST THE FEEL", label);
            GUILayout.Space(34 * s);
            player.moveSpeed = Slider("Move speed", player.moveSpeed, .5f, 6, s);
            player.turnSpeed = Slider("Turn speed", player.turnSpeed, 90, 1080, s);
            player.windup = Slider("Punch windup", player.windup, .06f, .5f, s);
            player.recovery = Slider("Recovery", player.recovery, .12f, .8f, s);
            followCamera.distance = Slider("Camera distance", followCamera.distance, 3, 10, s);
            if (GUILayout.Button("RESET TARGETS", button, GUILayout.Height(30 * s))) TrainingTarget.ResetAll();
            GUILayout.Label("Play-mode changes are temporary.", small);
            GUILayout.EndArea();
        }
        float Slider(string name, float value, float min, float max, float s)
        {
            GUILayout.Label(name + "   " + value.ToString("0.00"), small, GUILayout.Height(22 * s));
            float result = GUILayout.HorizontalSlider(value, min, max, GUILayout.Height(20 * s));
            return result;
        }
        void OnDestroy() { if (disc) Destroy(disc); }
    }
}

