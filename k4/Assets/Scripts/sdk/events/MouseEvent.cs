using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class MouseEvent : EventContext
    {
        public const string CLICK = "click";
        public const string MOUSE_DOWN = "mouseDown";
        public const string MOUSE_UP = "mouseUp";
        public const string ROLL_OVER = "rollOver";
        public const string ROLL_OUT = "rollOut";
        public const string RIGHT_CLICK = "rightClick";
        public const string MOUSE_MOVE = "mouseMove";
        public const string MOUSE_WHEEL = "mouseWheel";

        public float StageX;
        public float StageY;
        public int Delta;
        public int ClickCount;
        public bool CtrlKey;
        public bool AltKey;
        public bool ShiftKey;
        public bool CommandKey;

        public Event NativeEvent;

        public MouseEvent(string str, bool bubbles = false)
            : base(str, bubbles)
        {

        }

        internal void InitFrom(Event evt)
        {
            NativeEvent = evt;
            SetPreventDefault(false);

            this.StageX = evt.mousePosition.x;
            this.StageY = evt.mousePosition.y;
            this.Delta = (int) evt.delta.y;
            this.ClickCount = evt.clickCount;
            this.CtrlKey = evt.control;
            this.AltKey = evt.alt;
            this.ShiftKey = evt.shift;
            this.CommandKey = evt.command;
        }

        private static Vector2 sHelperPoint = Vector2.zero;
        public Vector2 GetLocalPoint(DisplayObject targetSpace)
        {
            sHelperPoint.x = StageX;
            sHelperPoint.y = StageY;
            sHelperPoint  = targetSpace.GlobalToLocal(sHelperPoint );
            return sHelperPoint;
        }
    }
}
