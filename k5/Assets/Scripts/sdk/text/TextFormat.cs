using System;
using System.Collections.Generic;
using UnityEngine;
 

namespace KGUI
{
    public class TextFormat
    {
        private AlignType _align;
        private int _size;
        private bool _bold;
        private string _font;
        private uint _color;
        private int _leading;
        private int _leftMargin;
        private int _rightMargin;
        private int _letterSpacing;
        private bool _underline;
        private string _url;

        private GUIStyle _guiStyle;

        public TextFormat()
        {
            _align = AlignType.Left;
            _size = 12;
            _bold = false;
            _font = "Times New Roman";
            _color = 0x000000;
            _leading = 0;
            _leftMargin = 0;
            _rightMargin = 0;
            _letterSpacing = 0;
            _underline = false;
            
            _guiStyle = new GUIStyle();
        }
        public AlignType align
        {
            get { return _align; }
            set
            {
                _align = value;
                _guiStyle.alignment  = (TextAnchor)value;
            }
        }
        public int size
        {
            get { return _size; }
            set
            {
                _size = value;
                _guiStyle.fontSize = _size;
            }
        }
        public bool bold
        {
            get { return _bold; }
            set
            {
                _bold = value;
                if (_bold)
                    _guiStyle.fontStyle = FontStyle.Bold;
            }
        }
        public string font
        {
            get { return _font; }
            set { 
                _font = value;
                /*if (!_guiStyle.font)
                    _guiStyle.font = new Font(_font);
                else
                    _guiStyle.font.name = _font;*/
            }
        }
        public uint color
        {
            get { return _color; }
            set
            {
                _color = value;
                if (_color == 0)
                {
                    _guiStyle.normal.textColor = Color.black;
                }
                else
                {
                    uint rr = (_color >> 16) & 0x0000ff;
                    uint gg = (_color >> 8) & 0x0000ff;
                    uint bb = _color & 0x0000ff;
                    float r = rr / 255.0f;
                    float g = gg / 255.0f;
                    float b = bb / 255.0f;
                    _guiStyle.normal.textColor = new Color(r, g, b, 1);
                }          
            }
        }
        public int leading
        {
            get { return _leading; }
            set { _leading = value; }
        }
        public int leftMargin
        {
            get{return _leftMargin;}
            set{_leftMargin = value;}
        }
        public int rightMargin
        {
            get{ return _rightMargin;}
            set{ _rightMargin = value;}
        }
        public int letterSpacing 
        {
            get{ return _letterSpacing; }
            set{ _letterSpacing = value;}
        }
        public bool underline
        {
            get { return _underline; }
            set { _underline = value; }
        }
        public string url
        {
            get { return _url; }
            set { _url = value; }
        }
        public GUIStyle style
        {
            get { return _guiStyle;}
        }
    }
}
