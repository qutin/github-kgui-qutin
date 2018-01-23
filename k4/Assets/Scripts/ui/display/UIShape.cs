using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace KGUI
{
    public class UIShape : Shape
    {
        private GObject _owner;

        public UIShape(GObject owner)
        {
            _owner = owner;
        }
    }
}
