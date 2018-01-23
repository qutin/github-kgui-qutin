using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class KeyboardEvent : EventContext
    {
        public const string KEY_DOWN = "keyDown";
        public const string KEY_UP = "keyUp";

        public KeyCode KeyCode;
        public bool CtrlKey;
        public bool AltKey;
        public bool ShiftKey;
        public bool CommandKey;

        public Event NativeEvent;

        public KeyboardEvent(string str, bool bubbles = false)
            : base(str, bubbles)
        {

        }

        internal void InitFrom(Event evt)
        {
            NativeEvent = evt;
            SetPreventDefault(false);

            this.KeyCode = evt.keyCode;
            this.CtrlKey = evt.control;
            this.AltKey = evt.alt;
            this.ShiftKey = evt.shift;
            this.CommandKey = evt.command;
        }
    }
}
