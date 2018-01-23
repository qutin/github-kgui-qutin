using System;
using System.Collections.Generic;
using UnityEngine;
using HTMLEngine;
using HTMLEngine.Unity3D;
 

namespace KGUI
{
    public class HtmlTextField : DisplayObject
    {
        private HtCompiler _compiler;
        private bool _changed;
        private string _text;

        private float _width;
        private float _height;
        private float _textHeight;

        private static Rect sHelperRect = new Rect();
        private static Matrix sHelperMatrix = new Matrix();

        public HtmlTextField()
        {
            HtEngine.RegisterLogger(new Unity3DLogger());            
            HtEngine.RegisterDevice(new Unity3DDevice());            
            _compiler = HtEngine.GetCompiler();
            _text = string.Empty;

            AddEventListenerObsolete(MouseEvent.MOUSE_DOWN, __mouseDown, true);
        }

        public string text
        {
            get { return this._text; }
            set
            {
                this._text = value;
                this._changed = true;
            }
        }

        public override float width
        {
            get
            {
                if (_changed)
                    Update();
                return base.width;
            }
            set
            {
                _width = value;
                _changed = true;
            }
        }

        public override float height
        {
            get
            {
                if (_changed)
                    Update();
                return base.height;
            }
            set
            {
                _height = value;
                _changed = true;
            }
        }
        
        public float textHeight
        {
            get {
                if (_changed)
                    Update();
                return _textHeight;
            }
        }

        private void Update()
        {
            _changed = false;

            int w = (int)this.width;
            if (w > 0)
            {
                _compiler.Compile(_text, w);
                _textHeight = _compiler.CompiledHeight;
            }
            else
                _textHeight = 0;
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

        public override void Render(RenderSupport support, float parentAlpha)
        {
            if (_changed)
                Update();

            if (_textHeight == 0)
                return;

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
             
            GUI.BeginGroup(rect);
            this._compiler.Draw(Time.deltaTime);
            GUI.EndGroup();

            support.PopClipRect();

            c.a = 1;
            GUI.color = c;
        }

        public override void Dispose()
        {
            // we need to dispose _compiler to prevent GC
            if (_compiler != null)
            {
                _compiler.Dispose();
                _compiler = null;
            }
        }

        private void __mouseDown(object obj)
        {
            MouseEvent evt = (MouseEvent)obj;
            Vector2 pt = evt.GetLocalPoint(this);
            string link =  _compiler.GetLink((int)pt.x, (int)pt.y);
            if (link != null)
            {
                evt.StopPropagation();
                DispatchEventObsolete(new TextEvent(TextEvent.LINK, false, link));
            }
        }
    }    
}
