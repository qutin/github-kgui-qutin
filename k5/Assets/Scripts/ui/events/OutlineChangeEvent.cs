using System;
using System.Collections.Generic;
 

namespace KGUI
{
    public class OutlineChangeEvent : EventContext
    {
        public const string XY = "___xyChange";
        public const string SIZE = "___sizeChange";

        public OutlineChangeEvent(string str, bool bubbles = false)
            : base(str, bubbles)
        {

        }
    }
}
