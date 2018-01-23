using System;
using System.Collections.Generic;
 

namespace KGUI
{
    public class ItemEvent: EventContext
    {
        public const string CLICK = "___itemClick";
        public int Index;

        public ItemEvent(string str, bool bubbles = false, int index=0)
            : base(str, bubbles)
        {
            this.Index = index;
        }
    }
}
