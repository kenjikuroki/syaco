using Input = MantisPunch.PrototypeInput;
using UnityEngine;

namespace MantisPunch
{
    // Shared pointer routing for mouse, multi-touch and the runtime smoke test.
    public sealed class MantisControls
    {
        public Vector2 Stick { get; private set; }
        public Vector2 Move { get; private set; }
        public bool BattleLayoutEnabled;
        public Vector2 StickCenter => BattleLayoutEnabled ? BattleLayout.ScreenPoint(BattleLayout.Stick) : new Vector2(Screen.safeArea.xMin + 118 * Scale, Screen.safeArea.yMin + 120 * Scale);
        public Vector2 AttackCenter => new Vector2(Screen.safeArea.xMax - 116 * Scale, Screen.safeArea.yMin + 120 * Scale);
        public float Scale => Mathf.Clamp(Mathf.Min(Screen.width / 1100f, Screen.height / 650f), .65f, 2f);
        public float Radius => BattleLayoutEnabled ? BattleLayout.Radius * BattleLayout.Scale * .65f : 76 * Scale;
        public int MovePointer { get; private set; } = int.MinValue;
        bool attack;
        bool dodge;
        public bool Duel;
        public Vector2 DodgeCenter => AttackCenter + new Vector2(-155 * Scale, 0);
        public bool ConsumeDodge() { bool result = dodge; dodge = false; return result; }
        public bool ConsumeAttack() { bool result = attack; attack = false; return result; }

        public void RoutePointer(int id, Vector2 point, TouchPhase phase, bool blocked = false)
        {
            if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
            {
                if (id == MovePointer) { MovePointer = int.MinValue; Stick = Vector2.zero; }
                return;
            }
            if (phase == TouchPhase.Began)
            {
                if (blocked) return;
                if (!BattleLayoutEnabled && Duel && Vector2.Distance(point, DodgeCenter) < 62 * Scale) { dodge = true; return; }
                if (!BattleLayoutEnabled && Vector2.Distance(point, AttackCenter) < 66 * Scale) { attack = true; return; }
                if (MovePointer == int.MinValue && Vector2.Distance(point, StickCenter) < Radius * 1.5f) MovePointer = id;
            }
            if (id == MovePointer)
            {
                Vector2 raw = Vector2.ClampMagnitude((point - StickCenter) / Radius, 1);
                Stick = raw.magnitude < .10f ? Vector2.zero : raw.normalized * Mathf.InverseLerp(.10f, 1, raw.magnitude);
            }
        }

        public void Poll(Rect blockedGuiRect)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                RoutePointer(touch.fingerId, touch.position, touch.phase, blockedGuiRect.Contains(ToGui(touch.position)));
            }
            if (!Input.HasActiveTouches)
            {
                var point = (Vector2)Input.mousePosition;
                if (Input.GetMouseButtonDown(0)) RoutePointer(-7, point, TouchPhase.Began, blockedGuiRect.Contains(ToGui(point)));
                else if (Input.GetMouseButton(0)) RoutePointer(-7, point, TouchPhase.Moved);
                if (Input.GetMouseButtonUp(0)) RoutePointer(-7, point, TouchPhase.Ended);
            }
            Vector2 keys = new Vector2(
                (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0),
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0));
            var pad=UnityEngine.InputSystem.Gamepad.current; Vector2 analog=pad==null?Vector2.zero:pad.leftStick.ReadValue(); Move = Vector2.ClampMagnitude(keys + Stick + analog, 1);
            if (UnityEngine.InputSystem.Gamepad.current?.buttonWest.wasPressedThisFrame == true || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.J)) attack = true;
        }

        public void Reset() { Stick = Move = Vector2.zero; MovePointer = int.MinValue; attack = dodge = false; }
        public static Vector2 ToGui(Vector2 point) => new Vector2(point.x, Screen.height - point.y);
    }
}



