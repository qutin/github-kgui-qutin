using System;
using System.Collections.Generic;
 
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace KGUI
{

    public class GTextField : GObject, IColorGear
    {
        protected TextField _textField;
		protected bool _ubbEnabled;
		protected AutoSizeType _autoSize;
		protected bool _widthAutoSize;
		protected bool _heightAutoSize;
		protected TextFormat _textFormat;
		protected string _text;
		protected string _font;
		protected int _fontSize;
		protected AlignType _align;
		protected VertAlignType _verticalAlign;
		protected int _leading;
		protected int _letterSpacing;
		protected bool _underline;

        public GearColor gearColor { get; private set; }
		
		protected bool _updatingFormat;
		protected bool _updatingSize;
        //protected int _yOffset;
        protected int _textWidth;
		protected int _textHeight;

        private static UnityUBBParser ubbParser = new UnityUBBParser();
		
        //
        public EventListener onFocusIn { get; private set; }
        public EventListener onFocusOut { get; private set; }
        public EventListener onChanged { get; private set; }
		public GTextField() : base()
		{
			
			
			_fontSize = 12;
			_align = AlignType.Left;
            _verticalAlign = VertAlignType.Top;
			_text = "";
			_leading = 3;

            _autoSize = AutoSizeType.Both;
            _widthAutoSize = true;
            _heightAutoSize = true;
			
			
			gearColor = new GearColor(this);

            //
            onFocusIn = new EventListener(this, "onFocusIn");
            onFocusOut = new EventListener(this, "onFocusOut");
            onChanged = new EventListener(this, "onChanged");

		}
		
		override protected void CreateDisplayObject()
		{
			_textField = new UITextField(this);
            _textFormat = new TextFormat();
            _textField.defaultTextFormat = _textFormat;
		    _textField.gOwner = this;
            _textField.autoSize = true;
			_displayObject = _textField;
		}

		override public string text
		{
            set
            {
                if (value == null)
                    value = "";
                _text = value;
                _textField.width = this.width;
                if(_ubbEnabled)
                    _textField.htmlText = ubbParser.Parse(UtilsStr.EncodeHTML(value));
                else
                    _textField.text = value;
                UpdateTextFieldText();
                UpdateSize();
            }

            get
            {
                if(_textField.input)
				    _text = _textField.text;
			    return _text;
            }
		}
				
		public string font
		{
			get{return _font;}
            set
            {
                if(_font!=value)
			    {
				    _font = value;
				    if(!_updatingFormat)
				    	UpdateTextFormat();
			    }
            }
		}
		
		public int fontSize
		{
			get{return _fontSize;}
            set
            {
                if(value<0)
			    	return;
			
			    if(_fontSize!=value)
			    {
				    _fontSize = value;
				    if(!_updatingFormat)
					    UpdateTextFormat();
			    }
            }
		}

        public Color color
		{
			get{return _textFormat.color;}
            set
            {
                if (!_textFormat.color.Equals(value))
                {
                    _textFormat.color = value;
				    if(gearColor.controller!=null)
					    gearColor.UpdateState();
				
				    if(!_updatingFormat)
				    	UpdateTextFormat();
			    }
            }
		}

        public AlignType align
		{
			get{return _align;}
            set{
                if(_align!=value)
			    {
				    _align = value;
			    	UpdateTextFormat();
			    }
            }
		}
		
		public VertAlignType verticalAlign
		{
			get{return _verticalAlign;}
            set{
                if(_verticalAlign!=value)
			    {
				    _verticalAlign = value;
			    	DoAlign();
			    }
            }
		}
		
		public int leading
		{
			get{return _leading;}
            set{
                if(_leading!=value)
			    {
				    _leading = value;
				    UpdateTextFormat();
			    }
            }
		}
		
		public int letterSpacing
		{
			get{return _letterSpacing;}
            set{
                if(_letterSpacing!=value)
			    {
				    _letterSpacing = value;
				    UpdateTextFormat();
			    }
            }
		}

		public bool underline
		{
			get{return _underline;}
            set{
                if(_underline!=value)
			    {
				    _underline = value;
				    UpdateTextFormat();
			    }
            }
		}
		
		public bool UBBEnabled
		{
            get{
			    return _ubbEnabled;                
            }
            set{
                if(_ubbEnabled!=value)
			    {
				    _ubbEnabled = value;
				    this.text = _text;
			    }
            }
		}

        public AutoSizeType autoSize
		{
            set{
				if(_autoSize!=value)
				{
					_autoSize = value;
					_widthAutoSize = value==AutoSizeType.Both;
					_heightAutoSize = value==AutoSizeType.Both||value==AutoSizeType.Height;
					if(_widthAutoSize)
					{
	                    _textField.autoSize = true;
						_textField.wordWrap = false;
					}
					else
					{
	                    _textField.autoSize = false;
						_textField.wordWrap = true;
					}
					UpdateSize();
				}
            }
            get{
                return _autoSize;
            }
		}
        virtual protected void UpdateTextFieldText()
        {
            if (_ubbEnabled)
                _textField.htmlText = UBBParser.inst.Parse(XML.EncodeString(_text));
            else
                _textField.text = _text;
        }
		public bool displayAsPassword
		{
			get{return _textField.displayAsPassword;}
            set{
                _textField.displayAsPassword = value;
            }
		}

        public bool singleLine
        {
            get { return _textField.singleLine; }
            set
            {
                _textField.singleLine = value;
            }
        }
		
		override public GTextField asTextField
		{
			get{return this;}
		}
		
		override public void HandleControllerChanged(Controller c)
		{
			base.HandleControllerChanged(c);
			
			if(gearColor.controller==c)
                gearColor.Apply();
		}

        virtual protected void UpdateSize()
		{
			if(_updatingSize)
				return;
			
			_updatingSize = true;

            _textWidth = (int)Math.Ceiling(_textField.textWidth);
            //if (_textWidth > 0)
            //    _textWidth += 4;
            _textHeight = (int)Math.Ceiling(_textField.textHeight);
            //if (_textHeight > 0)
            //    _textHeight += 4;

			float w, h;
			if(_widthAutoSize)
				w = _textWidth/GRoot.contentScaleFactor;
			else
				w = _width;

			if(_heightAutoSize)
			{
				h = _textHeight/GRoot.contentScaleFactor;
				if(!_widthAutoSize)
					_textField.height = _textHeight;
			}
			else
			{
				h = _height;
				if(_textHeight>_height*GRoot.contentScaleFactor)
					_textHeight = (int)Math.Ceiling(_height*GRoot.contentScaleFactor);
                _textField.height = _textHeight;
			}
			
			this.SetSize((int)w,(int)h);
			DoAlign();

			_updatingSize = false;
		}

		private void UpdateTextFormat()
		{
			if(_font!=null)
				_textFormat.font = _font;
			else
                _textFormat.font = UIConfig.defaultFont;
			_textFormat.size =  (int)(_fontSize*GRoot.contentScaleFactor);;
			_textFormat.bold = false;
			_textFormat.align = _align;
            _textFormat.leading = (int)(_leading * GRoot.contentScaleFactor);
            _textFormat.letterSpacing = (int)(_letterSpacing * GRoot.contentScaleFactor);
			_textFormat.underline = _underline;
			_textField.defaultTextFormat = _textFormat;
			if(_ubbEnabled)
				this.text = _text;

			UpdateSize();
		}
		
		override protected void HandlePositionChanged()
		{
            _displayObject.x = _x * GRoot.contentScaleFactor;
            _displayObject.y = _y * GRoot.contentScaleFactor + _yOffset;
		}
		
		override protected void HandleSizeChanged()
		{
			if(!_updatingSize)
			{
				if(!_widthAutoSize)
				{
					_textField.width = _width*GRoot.contentScaleFactor;
					
					//int h = (int)(_textField.textHeight+4);
                    float h = _textField.textHeight;
                    float h2 = _height * GRoot.contentScaleFactor;
					if(_heightAutoSize)
					{
						_textField.height = h;
                        _height = (int)(h / GRoot.contentScaleFactor);
					}
                    else if (h > h2)
                        _textField.height = h2;
					else
						_textField.height = h;
				}
				DoAlign();
			}
		}
		
		private void DoAlign()
		{
			if(_verticalAlign==VertAlignType.Top)
				_yOffset = 0;
            else if (_verticalAlign == VertAlignType.Middle)
				_yOffset = (int)(((float)_height*GRoot.contentScaleFactor-_textHeight)/2);
			else
				_yOffset = (int)(_height*GRoot.contentScaleFactor-_textHeight);
			_displayObject.y = _y*GRoot.contentScaleFactor+_yOffset;
		}

        override public void Setup_BeforeAdd(XML xml)
        {
            base.Setup_BeforeAdd(xml);

			string str;
			this.displayAsPassword = (string)xml.GetAttribute("password")=="true";
			str = (string)xml.GetAttribute("font");
			if(str!=null)
				_font = str;

            str = (string)xml.GetAttribute("fontSize");
			if(str!=null)
				_fontSize = int.Parse(str);

            str = (string)xml.GetAttribute("color");
			if(str!=null)
				_textFormat.color = ToolSet.ConvertFromHtmlColor(str);

            str = (string)xml.GetAttribute("align");
            if (str != null)
                _align = FieldTypes.parseAlign(str);

            str = (string)xml.GetAttribute("vAlign");
			if(str!=null)
				_verticalAlign = FieldTypes.parseVerticalAlign(str);

            str = (string)xml.GetAttribute("leading");
			if(str!=null)
				_leading = int.Parse(str);
			else
				_leading = 3;

            str = (string)xml.GetAttribute("letterSpacing");
			if(str!=null)
				_letterSpacing = int.Parse(str);
			
			_ubbEnabled = (string) xml.GetAttribute("ubb")=="true";

            this.displayAsPassword = (string)xml.GetAttribute("password")=="true";

            str = (string)xml.GetAttribute("autoSize");
			_updatingSize = true;
            if (str != null)
                this.autoSize = FieldTypes.parseAutoSizeType(str);

			UpdateTextFormat();
			_updatingSize = false;
			
            str = (string)xml.GetAttribute("text");
			if( str!=null && str.Length > 0)
                this.text = (string)xml.GetAttribute("text");
			else
                this.text = (string)xml.GetAttribute("demoText");
		}

        override public void Setup_AfterAdd(XML xml)
        {
            base.Setup_AfterAdd(xml);
			
			XML cxml = xml.GetNode("gearColor");
			if (cxml != null)
                gearColor.Setup(cxml);
		}
    }

    class UnityUBBParser : UBBParser
    {
        public UnityUBBParser()
        {
            iHandlers["COLOR"] = onTag_COLOR2;
            iHandlers["SIZE"] = onTag_SIZE2;
        }

        private string onTag_COLOR2(string tagName, bool end, string attr)
        {
            if (!end)
                return "<color=" + attr.ToLower() + ">";
            else
                return "</color>";
        }

        private string onTag_SIZE2(string tagName, bool end, string attr)
        {
            if (!end)
            {
                if (attr == "normal")
                    attr = "" + normalFontSize;
                else if (attr == "small")
                    attr = "" + smallFontSize;
                else if (attr == "large")
                    attr = "" + largeFontSize;
                else if (attr.Length > 0 && attr[0] == '+')
                    attr = "" + (smallFontSize + int.Parse(attr.Substring(1)));
                else if (attr.Length > 0 && attr[0] == '-')
                    attr = "" + (smallFontSize - int.Parse(attr.Substring(1)));
                return "<size=" + attr + ">";
            }
            else
                return "</size>";
        }
    }
}
