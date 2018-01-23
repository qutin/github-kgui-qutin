using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace KGUI
{
    public class UIHtmlTextField : HtmlTextField
    {
        private GObject _owner;

        public UIHtmlTextField(GObject owner)
        {
            _owner = owner;
        }
    }
}
