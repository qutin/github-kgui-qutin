using System;
using System.Collections.Generic;
using UnityEngine;


namespace KGUI
{
    public class GLabel : GComponent
    {
        protected GTextField _titleObject;
        protected GLoader _iconObject;

        public GLabel()
        {
        }

        public string icon
        {
            get
            {
                if (_iconObject != null)
                    return _iconObject.url;
                else
                    return null;
            }

            set
            {
                if(_iconObject!=null)
                    _iconObject.url = value;
            }
        }

        public string title
        {
            get
            {
                if (_titleObject != null)
                    return _titleObject.text;
                else
                    return null;
            }
            set
            {
                if (_titleObject != null)
                    _titleObject.text = value;
            }
        }
        override public string text
        {
            get { return this.title; }
            set { this.title = value; }
        }

        public Color titleColor
        {
            get
            {
                if (_titleObject != null)
                    return _titleObject.color;
                else
                    return Color.black;
            }
            set
            {
                if (_titleObject != null)
                    _titleObject.color = value;
            }
        }

        override public void ConstructFromXML(XML cxml)
        {
            base.ConstructFromXML(cxml);

            _titleObject = GetChildByName("title") as GTextField;
            _iconObject = GetChildByName("icon") as GLoader;
        }

        override public void Setup_AfterAdd(XML cxml)
        {
            base.Setup_AfterAdd(cxml);

            XML xml = cxml.GetNode("Label");
            if (xml == null)
                return;


            this.title = (string)xml.GetAttribute("title");
            this.icon = (string)xml.GetAttribute("icon");
            string str = (string)xml.GetAttribute("titleColor");
            if(str!=null)
                this.titleColor = ToolSet.ConvertFromHtmlColor(str);
        }
    }
}
