using System;
using System.Collections.Generic;
using UnityEngine;
 

namespace KGUI
{
    public class TextField : DisplayObject
    {
        private TextFormat _textFormat;
        private bool _input;
        private string _text;
        private bool _autoSize;
        private bool _wordWrap;
        private bool _displayAsPassword;
        private bool _textChanged;
        private float _width;
        private float _height;
        private float _textWidth;
        private float _textHeight;
        private int _maxLength;
        protected bool _singleLine;

        private static Rect sHelperRect = new Rect();
        private static Matrix sHelperMatrix = new Matrix();
        private static EventContext sChange = new EventContext(EventContext.CHANGE);

        //
        IMobileInputAdapter _mobileInputAdapter;
        int _caretPosition;
        CharPosition? _selectionStart;
        List<LineInfo> _lines;
        public EventListener onFocusIn { get; private set; }
        public EventListener onFocusOut { get; private set; }
        public EventListener onChanged { get; private set; }

        public TextField()
        {
            _textFormat = new TextFormat();
            wordWrap = false;
            _displayAsPassword = false;
            optimizeNotTouchable = true;
            _maxLength = int.MaxValue;
            _singleLine = true;
            _text = string.Empty;

            //
            _lines = new List<LineInfo>();
            onFocusIn = new EventListener(this, "onFocusIn");
            onFocusOut = new EventListener(this, "onFocusOut");
            onChanged = new EventListener(this, "onChanged");
        }

        public TextFormat defaultTextFormat
        {
            get { return _textFormat; }
            set {
                value.style.wordWrap = _wordWrap;
                _textFormat = value;
                _textChanged = true;
            }
        }
        public bool input
        {
            get { return _input; }
            set { 
                //_input = value;
                //optimizeNotTouchable = !_input;
                if (_input != value)
                {
                    _input = value;
                    optimizeNotTouchable = !_input;

                    if (_input)
                    {
                        onFocusIn.Add(__focusIn);
                        onFocusOut.AddCapture(__focusOut);

                        if (Stage.touchScreen && _mobileInputAdapter == null)
                        {
#if !(UNITY_WEBPLAYER || UNITY_WEBGL)
                            _mobileInputAdapter = new MobileInputAdapter();
#endif
                        }
                    }
                    else
                    {
                        onFocusIn.Remove(__focusIn);
                        onFocusOut.RemoveCapture(__focusOut);

                        if (_mobileInputAdapter != null)
                        {
                            _mobileInputAdapter.CloseKeyboard();
                            _mobileInputAdapter = null;
                        }
                    }
                }
            }
        }
        public string text
        {
            get { return _text; }
            set { _text = value; _textFormat.style.richText = false; _textChanged = true; }
        }
        public string htmlText
        {
            get { return _text; }
            set { _text = value; _textFormat.style.richText = true;  _textChanged = true; }
        }
        public bool autoSize
        {
            get { return _autoSize; }
            set { _autoSize = value; _textChanged = true; }
        }
        public bool wordWrap
        {
            get { return _wordWrap; }
            set 
            { 
                _wordWrap = value;
                _textFormat.style.wordWrap = _wordWrap;
                _textChanged = true;
            }
        }
        public bool displayAsPassword
        {
            get { return _displayAsPassword; }
            set { _displayAsPassword = value; }
        }

        public int maxLength
        {
            get { return _maxLength; }
            set { _maxLength = value; }
        }

        public bool singleLine
        {
            get { return _singleLine; }
            set
            {
                _singleLine = value;
            }
        }

        public float textWidth
        {
            get 
            {
                if (_textChanged)
                    Update();

                return _textWidth;
; 
            }
        }
        public float textHeight
        {
            get {
                if (_textChanged)
                    Update();

                return _textHeight;
            }
        }
        public override float width
        {
            get
            {
                if (_textChanged)
                    Update();
                return base.width;
            }
            set
            {
                _width = value;
                _textChanged = true;
            }
        }

        public override float height
        {
            get
            {
                return base.height;
            }
            set
            {
                _height = value;
                _textChanged = true;
            }
        }

        void OpenKeyboard()
        {
            _mobileInputAdapter.OpenKeyboard(_text, 0, false, displayAsPassword ? false : !_singleLine, displayAsPassword, false, null);
        }

        void __focusIn(EventContext context)
        {
            if (_input)
            {
                if (_mobileInputAdapter != null)
                {
                    OpenKeyboard();
                    onTouchBegin.AddCapture(__touchBegin2);
                }
                else
                {
                    //_caret = Stage.inst.inputCaret;
                    //_caret.quadBatch.sortingOrder = this.renderingOrder + 1;
                    //_caret.SetParent(cachedTransform);
                    //_caret.SetSizeAndColor(_textFormat.size, _textFormat.color);

                    //_highlighter = Stage.inst.highlighter;
                    //_highlighter.quadBatch.sortingOrder = this.renderingOrder + 2;
                    //_highlighter.SetParent(cachedTransform);

                    onKeyDown.AddCapture(__keydown);
                    onTouchBegin.AddCapture(__touchBegin);
                }
            }
        }

        void __focusOut(EventContext contxt)
        {
            if (_mobileInputAdapter != null)
            {
                _mobileInputAdapter.CloseKeyboard();
                onTouchBegin.RemoveCapture(__touchBegin2);
            }

            //if (_caret != null)
            //{
            //    _caret.SetParent(null);
            //    _caret = null;
            //    _highlighter.SetParent(null);
            //    _highlighter = null;
            //    onKeyDown.RemoveCapture(__keydown);
            //    onTouchBegin.RemoveCapture(__touchBegin);
            //}
            onKeyDown.RemoveCapture(__keydown);
            onTouchBegin.RemoveCapture(__touchBegin);
        }
        void __keydown(EventContext context)
        {
            if (context.isDefaultPrevented)
                return;

            InputEvent evt = context.inputEvent;

            switch (evt.keyCode)
            {
                case KeyCode.Backspace:
                    {
                        context.PreventDefault();
                        if (_selectionStart != null)
                        {
                            DeleteSelection();
                            onChanged.Call();
                        }
                        else if (_caretPosition > 0)
                        {
                            int tmp = _caretPosition; //this.text 会修改_caretPosition
                            _caretPosition--;
                            this.text = _text.Substring(0, tmp - 1) + _text.Substring(tmp);
                            onChanged.Call();
                        }

                        break;
                    }

                case KeyCode.Delete:
                    {
                        context.PreventDefault();
                        if (_selectionStart != null)
                        {
                            DeleteSelection();
                            onChanged.Call();
                        }
                        else if (_caretPosition < _text.Length)
                        {
                            this.text = _text.Substring(0, _caretPosition) + _text.Substring(_caretPosition + 1);
                            onChanged.Call();
                        }

                        break;
                    }

                case KeyCode.LeftArrow:
                    {
                        context.PreventDefault();
                        if (evt.shift)
                        {
                            if (_selectionStart == null)
                                _selectionStart = GetCharPosition(_caretPosition);
                        }
                        else
                            ClearSelection();
                        if (_caretPosition > 0)
                        {
                            CharPosition cp = GetCharPosition(_caretPosition - 1);
                            //调整输入光标
                            //AdjustCaret(cp);
                        }
                        break;
                    }

                case KeyCode.RightArrow:
                    {
                        context.PreventDefault();
                        if (evt.shift)
                        {
                            if (_selectionStart == null)
                                _selectionStart = GetCharPosition(_caretPosition);
                        }
                        else
                            ClearSelection();
                        if (_caretPosition < _text.Length)
                        {
                            CharPosition cp = GetCharPosition(_caretPosition + 1);
                            //AdjustCaret(cp);
                        }
                        break;
                    }

                case KeyCode.UpArrow:
                    {
                        context.PreventDefault();
                        if (evt.shift)
                        {
                            if (_selectionStart == null)
                                _selectionStart = GetCharPosition(_caretPosition);
                        }
                        else
                            ClearSelection();

                        CharPosition cp = GetCharPosition(_caretPosition);
                        if (cp.lineIndex == 0)
                            return;

                        LineInfo line = _lines[cp.lineIndex - 1];
                        //cp = GetCharPosition(new Vector3(_caret.cachedTransform.localPosition.x + this.pivotX, line.y, 0));
                        //AdjustCaret(cp);
                        break;
                    }


                case KeyCode.DownArrow:
                    {
                        context.PreventDefault();
                        if (evt.shift)
                        {
                            if (_selectionStart == null)
                                _selectionStart = GetCharPosition(_caretPosition);
                        }
                        else
                            ClearSelection();

                        CharPosition cp = GetCharPosition(_caretPosition);
                        if (cp.lineIndex == _lines.Count - 1)
                            return;

                        LineInfo line = _lines[cp.lineIndex + 1];
                        //cp = GetCharPosition(new Vector3(_caret.cachedTransform.localPosition.x + this.pivotX, line.y, 0));
                        //AdjustCaret(cp);
                        break;
                    }

                case KeyCode.PageUp:
                    {
                        context.PreventDefault();
                        ClearSelection();

                        break;
                    }

                case KeyCode.PageDown:
                    {
                        context.PreventDefault();
                        ClearSelection();

                        break;
                    }

                case KeyCode.Home:
                    {
                        context.PreventDefault();
                        ClearSelection();

                        CharPosition cp = GetCharPosition(_caretPosition);
                        LineInfo line = _lines[cp.lineIndex];
                        //cp = GetCharPosition(new Vector3(int.MinValue, line.y, 0));
                        //AdjustCaret(cp);
                        break;
                    }

                case KeyCode.End:
                    {
                        context.PreventDefault();
                        ClearSelection();

                        CharPosition cp = GetCharPosition(_caretPosition);
                        LineInfo line = _lines[cp.lineIndex];
                        //cp = GetCharPosition(new Vector3(int.MaxValue, line.y, 0));
                        //AdjustCaret(cp);

                        break;
                    }

                //Select All
                case KeyCode.A:
                    {
                        if (evt.ctrl)
                        {
                            context.PreventDefault();
                            _selectionStart = GetCharPosition(0);
                            //AdjustCaret(GetCharPosition(_text.Length));
                        }
                        break;
                    }

                // Copy
                case KeyCode.C:
                    {
                        if (evt.ctrl && !_displayAsPassword)
                        {
                            context.PreventDefault();
                            string s = GetSelection();
                            if (!string.IsNullOrEmpty(s))
                                ToolSet.clipboard = s;
                        }
                        break;
                    }

                // Paste
                case KeyCode.V:
                    {
                        if (evt.ctrl)
                        {
                            context.PreventDefault();
                            string s = ToolSet.clipboard;
                            if (!string.IsNullOrEmpty(s))
                                InsertText(s);
                        }
                        break;
                    }

                // Cut
                case KeyCode.X:
                    {
                        if (evt.ctrl && !_displayAsPassword)
                        {
                            context.PreventDefault();
                            string s = GetSelection();
                            if (!string.IsNullOrEmpty(s))
                            {
                                ToolSet.clipboard = s;
                                DeleteSelection();
                                onChanged.Call();
                            }
                        }
                        break;
                    }

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    {
                        if (!evt.ctrl && !evt.shift)
                        {
                            context.PreventDefault();

                            if (!_singleLine)
                                InsertText("\n");
                        }
                        break;
                    }
            }
        }

        void __touchBegin(EventContext context)
        {
            if (_lines.Count == 0)
                return;

            ClearSelection();

            CharPosition cp;
            if (_textChanged) //maybe the text changed in user's touchBegin
            {
                cp.charIndex = 0;
                cp.lineIndex = 0;
            }
            else
            {
                Vector3 v = Stage.touchPosition;
                v = this.GlobalToLocal(v);
                v.x += this.pivotX;
                v.y += this.pivotY;
                cp = GetCharPosition(v);
            }

            //AdjustCaret(cp);
            _selectionStart = cp;
            //Stage.inst.onTouchMove.AddCapture(__touchMove);
            gOwner.appContext.stage.onTouchMove.AddCapture(__touchMove);
            //Stage.inst.onTouchEnd.AddCapture(__touchEnd);
            gOwner.appContext.stage.onTouchEnd.AddCapture(__touchEnd);
        }

        void __touchBegin2(EventContext context)
        {
            OpenKeyboard();
        }

        void __touchMove(EventContext context)
        {
            if (_selectionStart == null)
                return;

            Vector3 v = Stage.touchPosition;
            v = this.GlobalToLocal(v);
            v.x += this.pivotX;
            v.y += this.pivotY;
            CharPosition cp = GetCharPosition(v);
            //if (cp.charIndex != _caretPosition)
            //    AdjustCaret(cp);
        }

        void __touchEnd(EventContext context)
        {
            if (_selectionStart != null && ((CharPosition)_selectionStart).charIndex == _caretPosition)
                _selectionStart = null;
            //Stage.inst.onTouchMove.RemoveCapture(__touchMove);
            gOwner.appContext.stage.onTouchMove.RemoveCapture(__touchMove);
            gOwner.appContext.stage.onTouchEnd.RemoveCapture(__touchEnd);
        }

        void ClearSelection()
        {
            if (_selectionStart != null)
            {
                //if (_highlighter != null)
                //    _highlighter.Clear();
                _selectionStart = null;
            }
        }

        void DeleteSelection()
        {
            if (_selectionStart == null)
                return;

            CharPosition cp = (CharPosition)_selectionStart;
            if (cp.charIndex < _caretPosition)
            {
                this.text = _text.Substring(0, cp.charIndex) + _text.Substring(_caretPosition);
                _caretPosition = cp.charIndex;
            }
            else
                this.text = _text.Substring(0, _caretPosition) + _text.Substring(cp.charIndex);
            ClearSelection();
        }

        string GetSelection()
        {
            if (_selectionStart == null)
                return string.Empty;

            CharPosition cp = (CharPosition)_selectionStart;
            if (cp.charIndex < _caretPosition)
                return _text.Substring(cp.charIndex, _caretPosition - cp.charIndex);
            else
                return _text.Substring(_caretPosition, cp.charIndex - _caretPosition);
        }

        void InsertText(string value)
        {
            if (_selectionStart != null)
                DeleteSelection();
            this.text = _text.Substring(0, _caretPosition) + value + _text.Substring(_caretPosition);
            _caretPosition += value.Length;
            onChanged.Call();
        }
        CharPosition GetCharPosition(int charIndex)
        {
            CharPosition cp;
            cp.charIndex = charIndex;

            LineInfo line;
            int lineCount = _lines.Count;
            int i;
            int len;
            for (i = 0; i < lineCount; i++)
            {
                line = _lines[i];
                len = line.text.Length;
                if (charIndex - len < 0)
                    break;

                charIndex -= len;
            }
            if (i == lineCount)
                i = lineCount - 1;

            cp.lineIndex = i;
            return cp;
        }
        CharPosition GetCharPosition(Vector3 location)
        {
            CharPosition result;
            int lineCount = _lines.Count;
            int charIndex = 0;
            LineInfo line;
            int last = 0;
            int i;
            for (i = 0; i < lineCount; i++)
            {
                line = _lines[i];
                charIndex += last;

                if (line.y + line.height > location.y)
                    break;

                last = line.text.Length;
            }
            if (i == lineCount)
                i = lineCount - 1;

            result.lineIndex = i;
            line = _lines[i];
            int textLen = line.text.Length;
            //Vector3 v;
            //if (textLen > 0)
            //{
            //    for (i = 0; i < textLen; i++)
            //    {
            //        v = quadBatch.vertices[charIndex * 4 + 2];
            //        if (v.x > location.x)
            //            break;

            //        charIndex++;
            //    }
            //    if (i == textLen && result.lineIndex != lineCount - 1)
            //        charIndex--;
            //}
            
            //算出字符位置

            result.charIndex = charIndex;
            return result;
        }

        private void Update()
        {
            _textChanged = false;

            if (_autoSize)
            {
                if (_wordWrap)
                {
                    _textHeight = _textFormat.style.CalcHeight(new GUIContent(_text), _width);
                    _height = _textHeight;
                }
                else
                {
                    Vector2 v2 = _textFormat.style.CalcSize(new GUIContent(_text));
                    _textWidth = v2.x;
                    _textHeight = v2.y;
                    _width = _textWidth;
                    _height = _textHeight;
                }
            }
            else
            {
                _textWidth = _width;
                _textHeight = _textFormat.style.CalcHeight(new GUIContent(_text), _width);
            }
        }

        override public Rect GetBounds(DisplayObject targetSpace)
        {
            Rect resultRect = new Rect();
            if (targetSpace == this) // optimization
            {
                resultRect.Set(0, 0, _width, _height);
            }
            else if (targetSpace == parent && rotation == 0.0) // optimization
            {
                float scaleX = this.scaleX;
                float scaleY = this.scaleY;
                resultRect.Set(x - pivotX * scaleX, y - pivotY * scaleY, _width * scaleX, _height * scaleY);
                if (scaleX < 0) { resultRect.width *= -1; resultRect.x -= resultRect.width; }
                if (scaleY < 0) { resultRect.height *= -1; resultRect.y -= resultRect.height; }
            }
            else
            {
                GetTransformationMatrix(targetSpace, sHelperMatrix);
                resultRect = sHelperMatrix.transformRect(0, 0, _width, _height);
            }
            return resultRect;
        }

        internal override void HandleKeyEvent(KeyboardEvent evt)
        {
            if (_input) //eat this event, dont bubbles
            {
                evt.SetTarget(this);
                evt.StopsPropagation = false;

                this.InvokeEvent(evt, true);
                this.InvokeEvent(evt, false);
            }
        }

        public override void Render(RenderSupport support, float parentAlpha)
        {

            if (_text!=null)
            {
                Color c = GUI.color;
                c.a = parentAlpha * this.alpha;
                GUI.color = c;

                Rect rect = support.GetNativeDrawRect(_width, _height, false);
                sHelperRect.x = rect.x;
                sHelperRect.y = rect.y;
                sHelperRect.width = rect.width;
                sHelperRect.height = rect.height;
                support.PushClipRect(sHelperRect);
                rect.x += support.projectionMatrix.tx;
                rect.y += support.projectionMatrix.ty;

                GUIStyle lastStyle;
                if (_input)
                {
                    GUI.SetNextControlName(internalId);
                    if (_singleLine)
                    {
                        lastStyle = GUI.skin.textField;
                        GUI.skin.textField = _textFormat.style;
                        _text = GUI.TextField(rect, _text, _maxLength);
                        if (GUI.changed)
                        {
                            DispatchEventObsolete(sChange);
                            onChanged.Call();
                        }

                        GUI.skin.textField = lastStyle;
                    }
                    else if(_displayAsPassword)
                    {
                        _text = GUI.PasswordField(rect, _text, '*', _maxLength);
                        if (GUI.changed)
                        {
                            DispatchEventObsolete(sChange);
                            onChanged.Call();
                        }
                    }
                    else
                    {
                        lastStyle = GUI.skin.textArea;
                        GUI.skin.textArea = _textFormat.style;
                        _text = GUI.TextArea(rect, _text, _maxLength);
                        if (GUI.changed)
                        {
                            DispatchEventObsolete(sChange);
                            onChanged.Call();
                        }

                        GUI.skin.textArea = lastStyle;
                    }
                }
                else
                {
                    lastStyle = GUI.skin.label;
                    GUI.skin.label = _textFormat.style;
                    GUI.Label(rect, _text);
                    GUI.skin.label = lastStyle;
                }
                support.PopClipRect();

                c.a = 1;
                GUI.color = c;
            }     

        }
        class LineInfo
        {
            public float width;
            public float height;
            public float textHeight;
            public string text;
            public float y;
            public int quadCount;
            public bool visible;

            static Stack<LineInfo> pool = new Stack<LineInfo>();

            public static LineInfo Borrow()
            {
                if (pool.Count > 0)
                {
                    LineInfo ret = pool.Pop();
                    ret.width = 0;
                    ret.height = 0;
                    ret.textHeight = 0;
                    ret.text = null;
                    ret.y = 0;
                    ret.quadCount = 0;
                    ret.visible = false;
                    return ret;
                }
                else
                    return new LineInfo();
            }

            public static void Return(LineInfo value)
            {
                pool.Push(value);
            }

            public static void Return(List<LineInfo> values)
            {
                int cnt = values.Count;
                for (int i = 0; i < cnt; i++)
                    pool.Push(values[i]);
            }
        }
        struct CharPosition
        {
            public int charIndex;
            public int lineIndex;
        }
    }
}
