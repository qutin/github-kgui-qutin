using System;
using System.Collections.Generic;
 

namespace KGUI
{
    public class GHtmlTextField : GObject
    {
        protected HtmlTextField _textField;
        protected bool _autoSize;

        private bool _updatingSize;

        public GHtmlTextField()
            : base()
        {
         
        }

        override protected void CreateDisplayObject()
        {
            _textField = new UIHtmlTextField(this);
            _textField.gOwner = this;
            _displayObject = _textField;
            _autoSize = false;
        }

        public string Text
        {
            set { 
                _textField.text = value;
                UpdateSize();
            }
            get { return _textField.text; }
        }

        public bool AutoSize
        {
            set {
                if (_autoSize != value)
                {
                    _autoSize = value;
                    UpdateSize();
                }
            }
            get { return _autoSize;}
        }

        private void UpdateSize()
        {
            if (_updatingSize)
                return;

            _updatingSize = true;

            float h;
            if (_autoSize)
            {
                h = _textField.textHeight / GRoot.contentScaleFactor;
                _textField.height = _textField.textHeight;
            }
            else
            {
                h = _height;
            }

            this.height = (int)Math.Ceiling(h);

            _updatingSize = false;
        }

        override protected void HandleSizeChanged()
        {
            if (!_updatingSize)
            {
                _textField.width = _width * GRoot.contentScaleFactor;

                if (_autoSize)
                {
                    float h = _textField.textHeight;
                    _textField.height = h;
                    _height = (int)(h / GRoot.contentScaleFactor);
                }
                else
                    _textField.height = _height * GRoot.contentScaleFactor;
            }
        }
    }
}
