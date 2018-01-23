using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KGUI
{    
    public delegate void RenderCallback( DisplayObject obj, Rect rect);
    public delegate void DisposeCallback();
    public class GoWrapper : DisplayObject
    {
        protected RenderCallback _onGUI;
        protected DisposeCallback _onDispose;
        protected float _width;
        protected float _height;

        public GoWrapper(RenderCallback onGUI = null, DisposeCallback onDispose = null):base()
        {
            this._onGUI = onGUI;
            this._onDispose = onDispose;
            this._width = 1024;
            this._height = 1024;
        }
        override public Rect GetBounds(DisplayObject targetSpace)
        {
            return new Rect(0, 0, _width, _height);
        }
        //重写因为真的没有什么需要缩放
        override public float width 
        {
            get { return _width; }
            set { _width = value; }
        }
        override public float height
        {
            get { return _height; }
            set { _height = value; }
        }
        override public void Render(RenderSupport support, float parentAlpha)
        {
            
            Rect rect = support.GetNativeDrawRect(this.width, this.height);
            if (this._onGUI != null)
                this._onGUI(this, rect);
        }

        public override void Dispose()
        {
            base.Dispose();
            this._onGUI = null;
            if (this._onDispose != null)
                this._onDispose();
            this._onDispose = null;

        }

     
    }
}
