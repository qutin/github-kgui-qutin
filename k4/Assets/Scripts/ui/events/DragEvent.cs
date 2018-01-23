using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace KGUI
{
    public class DragEvent : EventContext
    {
        public const string DRAG_START = "__dragStart";
        public const string DRAG_END = "__dragEnd";

        public DragEvent(string str, bool bubbles = false)
            : base(str, bubbles)
        {

        }
    }
}
