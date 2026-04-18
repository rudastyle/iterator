using System;

namespace TimeLoop
{
    public enum InputAction { MoveLeft, MoveRight, Jump }

    [Serializable]
    public readonly struct InputEvent
    {
        public readonly float       Time;
        public readonly InputAction Action;
        public readonly bool        Pressed;

        public InputEvent(float time, InputAction action, bool pressed)
        {
            Time    = time;
            Action  = action;
            Pressed = pressed;
        }
    }
}
