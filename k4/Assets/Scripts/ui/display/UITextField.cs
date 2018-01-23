using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace KGUI
{
    public class UITextField : TextField
    {
        private GObject _owner;

        public UITextField(GObject owner)
        {
            _owner = owner;
        }
    }
}
