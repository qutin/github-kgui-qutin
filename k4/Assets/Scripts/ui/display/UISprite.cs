using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace KGUI
{
    public class UISprite : Sprite
    {
        private GObject _owner;

        public UISprite(GObject owner)
        {
            _owner = owner;
        }
    }
}
