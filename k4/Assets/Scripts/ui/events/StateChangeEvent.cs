using System;
using System.Collections.Generic;
 

namespace KGUI
{
    public class StateChangeEvent : EventContext
    {
        public const string CHANGED = "___stateChange";
         
        public StateChangeEvent(string str, bool bubbles = false)
            : base(str, bubbles)
        {

        }
    }
}
