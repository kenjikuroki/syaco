using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.TouchPhase;

namespace MantisPunch
{
    public static class PrototypeInput
    {
        public struct Pointer { public int fingerId; public Vector2 position; public TouchPhase phase; }
        public static int touchCount => Touchscreen.current == null ? 0 : Touchscreen.current.touches.Count;
        public static bool HasActiveTouches
        {
            get
            {
                if (Touchscreen.current == null) return false;
                foreach (var t in Touchscreen.current.touches)
                    if (t.press.isPressed || t.press.wasReleasedThisFrame) return true;
                return false;
            }
        }
        public static Pointer GetTouch(int i)
        {
            var t = Touchscreen.current.touches[i];
            var phase = t.press.wasReleasedThisFrame ? TouchPhase.Ended : t.press.wasPressedThisFrame ? TouchPhase.Began : t.press.isPressed ? TouchPhase.Moved : TouchPhase.Canceled;
            return new Pointer { fingerId = i, position = t.position.ReadValue(), phase = phase };
        }
        public static Vector3 mousePosition => Mouse.current == null ? Vector3.zero : (Vector3)Mouse.current.position.ReadValue();
        public static bool GetMouseButtonDown(int _) => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        public static bool GetMouseButton(int _) => Mouse.current != null && Mouse.current.leftButton.isPressed;
        public static bool GetMouseButtonUp(int _) => Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
        static UnityEngine.InputSystem.Controls.KeyControl Control(KeyCode code)
        {
            var k = Keyboard.current;
            if (k == null) return null;
            switch (code)
            {
                case KeyCode.W: return k.wKey; case KeyCode.A: return k.aKey;
                case KeyCode.S: return k.sKey; case KeyCode.D: return k.dKey;
                case KeyCode.J: return k.jKey; case KeyCode.R: return k.rKey;
                case KeyCode.Space: return k.spaceKey; case KeyCode.Tab: return k.tabKey;
                case KeyCode.UpArrow: return k.upArrowKey; case KeyCode.DownArrow: return k.downArrowKey;
                case KeyCode.LeftArrow: return k.leftArrowKey; case KeyCode.RightArrow: return k.rightArrowKey;
                default: return null;
            }
        }
        public static bool GetKey(KeyCode code) => Control(code)?.isPressed ?? false;
        public static bool GetKeyDown(KeyCode code) => Control(code)?.wasPressedThisFrame ?? false;
    }
}
