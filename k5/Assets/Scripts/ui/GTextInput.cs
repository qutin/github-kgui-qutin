using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace KGUI
{
    class GTextInput : GTextField
    {
        string _promptText;
        public GTextInput()
        {
            this.focusable = true;
           
        }

        public bool Editable
        {
            get
            {
                return _textField.input;
            }
            set
            {
                _textField.input = false;
            }
        }

        public int MaxLength
        {
            get
            {
                return _textField.maxLength;
            }
            set
            {
                _textField.maxLength = value;
            }
        }

        override protected void CreateDisplayObject()
        {
            base.CreateDisplayObject();
            base.CreateDisplayObject();
            _textField.autoSize = false;
            _textField.wordWrap = false;
            _textField.onChanged.AddCapture(__onChanged);
            _textField.onFocusIn.AddCapture(__onFocusIn);
            _textField.onFocusOut.AddCapture(__onFocusOut);
            _textField.input = true;
        }

        public string promptText
        {
            get
            {
                return _promptText;
            }
            set
            {
                _promptText = value;
                UpdateTextFieldText();
            }
        }
        public override void Setup_BeforeAdd(XML xml)
        {
            base.Setup_BeforeAdd(xml);

            _promptText = xml.GetAttribute("prompt");
        }

        public override void Setup_AfterAdd(XML xml)
        {
            base.Setup_AfterAdd(xml);

            if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_promptText))
            {
                _textField.displayAsPassword = false;
                _textField.htmlText = UBBParser.inst.Parse(XML.EncodeString(_promptText));
            }
        }

        override protected void UpdateTextFieldText()
        {
            if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_promptText))
            {
                _textField.displayAsPassword = false;
                _textField.htmlText = UBBParser.inst.Parse(XML.EncodeString(_promptText));
            }
            else
            {
                _textField.displayAsPassword = displayAsPassword;
                _textField.text = _text;
            }
        }
        void __onChanged(EventContext context)
        {
            _text = _textField.text;
            UpdateSize();
        }

        void __onFocusIn(EventContext context)
        {
            if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_promptText))
            {
                _textField.displayAsPassword = displayAsPassword;
                _textField.text = string.Empty;
            }
        }

        void __onFocusOut(EventContext context)
        {
            _text = _textField.text;
            if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_promptText))
            {
                _textField.displayAsPassword = false;
                _textField.htmlText = UBBParser.inst.Parse(XML.EncodeString(_promptText));
            }
        }
    }
}