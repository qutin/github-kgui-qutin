using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace KGUI
{
    public class UIImage : Image
    {
        private GObject _owner;

        public UIImage(GObject owner)
        {
            _owner = owner;
        }
    }
}
