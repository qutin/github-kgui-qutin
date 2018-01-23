using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class TextEvent : EventContext
    {
        public const string LINK = "link";

        public string text;

        public TextEvent(string str, bool bubbles = false, string text=null)
            : base(str, bubbles)
        {
            this.text = text;
        }
    }
}
