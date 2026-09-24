using System.Runtime.InteropServices;
using UnityEngine;

public static class GameInput
{
    private const int VirtualKeyC = 0x43;
    private const int VirtualKeyX = 0x58;
    private const int VirtualKeyZ = 0x5A;
    private const int VirtualKeyA = 0x41;
    private const int VirtualKeyLeft = 0x25;
    private const int VirtualKeyUp = 0x26;
    private const int VirtualKeyRight = 0x27;
    private const int VirtualKeyDown = 0x28;
    private const int VirtualKeyZero = 0x30;
    private const int VirtualKeyNumpadZero = 0x60;

    private static int lastUpdatedFrame = -1;
    private static KeyState cKey;
    private static KeyState xKey;
    private static KeyState zKey;
    private static KeyState aKey;
    private static KeyState leftKey;
    private static KeyState rightKey;
    private static KeyState upKey;
    private static KeyState downKey;
    private static KeyState zeroKey;
    private static KeyState numpadZeroKey;

    public static bool ConfirmPressed
    {
        get
        {
            Refresh();
            return cKey.Pressed;
        }
    }

    public static bool ConfirmHeld
    {
        get
        {
            Refresh();
            return cKey.Current;
        }
    }

    public static bool CancelPressed
    {
        get
        {
            Refresh();
            return xKey.Pressed;
        }
    }

    public static bool MenuPressed
    {
        get
        {
            Refresh();
            return zKey.Pressed;
        }
    }

    public static bool DialogueAdvancePressed
    {
        get
        {
            Refresh();
            return zKey.Pressed || xKey.Pressed || cKey.Pressed;
        }
    }

    public static bool UpPressed
    {
        get
        {
            Refresh();
            return upKey.Pressed;
        }
    }

    public static bool DownPressed
    {
        get
        {
            Refresh();
            return downKey.Pressed;
        }
    }

    public static bool LeftPressed
    {
        get
        {
            Refresh();
            return leftKey.Pressed;
        }
    }

    public static bool RightPressed
    {
        get
        {
            Refresh();
            return rightKey.Pressed;
        }
    }

    public static bool UpHeld
    {
        get
        {
            Refresh();
            return upKey.Current;
        }
    }

    public static bool DownHeld
    {
        get
        {
            Refresh();
            return downKey.Current;
        }
    }

    public static bool LeftHeld
    {
        get
        {
            Refresh();
            return leftKey.Current;
        }
    }

    public static bool RightHeld
    {
        get
        {
            Refresh();
            return rightKey.Current;
        }
    }

    public static bool DebugZeroPressed
    {
        get
        {
            Refresh();
            return zeroKey.Pressed || numpadZeroKey.Pressed;
        }
    }

    public static Vector2 MovementVector
    {
        get
        {
            Refresh();

            float x = 0f;
            float y = 0f;

            if (leftKey.Current)
            {
                x -= 1f;
            }

            if (rightKey.Current)
            {
                x += 1f;
            }

            if (downKey.Current)
            {
                y -= 1f;
            }

            if (upKey.Current)
            {
                y += 1f;
            }

            return new Vector2(x, y).normalized;
        }
    }

    private static void Refresh()
    {
        int frame = Time.frameCount;
        if (lastUpdatedFrame == frame)
        {
            return;
        }

        lastUpdatedFrame = frame;

        cKey.Update(IsKeyHeld(VirtualKeyC, KeyCode.C));
        xKey.Update(IsKeyHeld(VirtualKeyX, KeyCode.X));
        zKey.Update(IsKeyHeld(VirtualKeyZ, KeyCode.Z));
        aKey.Update(IsKeyHeld(VirtualKeyA, KeyCode.A));
        leftKey.Update(IsKeyHeld(VirtualKeyLeft, KeyCode.LeftArrow));
        rightKey.Update(IsKeyHeld(VirtualKeyRight, KeyCode.RightArrow));
        upKey.Update(IsKeyHeld(VirtualKeyUp, KeyCode.UpArrow));
        downKey.Update(IsKeyHeld(VirtualKeyDown, KeyCode.DownArrow));
        zeroKey.Update(IsKeyHeld(VirtualKeyZero, KeyCode.Alpha0));
        numpadZeroKey.Update(IsKeyHeld(VirtualKeyNumpadZero, KeyCode.Keypad0));
    }

    private static bool IsKeyHeld(int windowsVirtualKey, KeyCode fallbackKey)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return (GetAsyncKeyState(windowsVirtualKey) & 0x8000) != 0;
#else
        return Input.GetKey(fallbackKey);
#endif
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
#endif

    private struct KeyState
    {
        public bool Previous;
        public bool Current;
        public bool Pressed => Current && !Previous;

        public void Update(bool value)
        {
            Previous = Current;
            Current = value;
        }
    }
}
