using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class Shape : DisplayObject
    {
        private int _lineSize;
        private Color _lineColor;
        private Color _fillColor;
        private float _width;
        private float _height;
        private bool _empty;
        private Texture2D _texture;
        private GUIStyle _style;

        public Shape()
        {
            _empty = true;
            optimizeNotTouchable = true;
        }

        override public Rect GetBounds(DisplayObject targetSpace)
        {
            return new Rect(0, 0, _width, _height);
        }

        public bool empty
        {
            get { return _empty; }
        }

        public void DrawRect(float width, float height, int lineSize, uint lineColor, uint fillColor)
        {
            _empty = false;
            optimizeNotTouchable = false;
            _width = width;
            _height = height;
            _lineSize = lineSize;
            _lineColor = TranslateColor(lineColor);
            _fillColor = TranslateColor(fillColor);
            if (_texture == null)
                _texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            

            if (_style == null)
                _style = new GUIStyle();
            _texture.SetPixel(0, 0, _fillColor);
            _texture.Apply();
            _style.normal.background = _texture;
        }

        public void clear()
        {
            _empty = true;
            optimizeNotTouchable = true;
        }

        override public void Dispose()
        {
            base.Dispose();
            if (_texture != null)
            {
#if UNITY_EDITOR
                Texture2D.DestroyImmediate(_texture);
#else
                Texture2D.Destroy(_texture);
#endif
                _texture = null;
            }
        }

        private Color TranslateColor(uint color)
        {
            uint aa = (color >> 24) & 0x0000ff;
            uint rr = (color >> 16) & 0x0000ff;
            uint gg = (color >> 8) & 0x0000ff;
            uint bb = color & 0x0000ff;
            float a = aa / 255.0f;
            float r = rr / 255.0f;
            float g = gg / 255.0f;
            float b = bb / 255.0f;
            return new Color(r, g, b, a);
        }

        private static Vector2 sPoint = new Vector2();
        public override void Render(RenderSupport support, float parentAlpha)
        {

            if(_empty || _fillColor.a == 0.0)
                return;

            Rect rect = support.GetNativeDrawRect(_width, _height);

            int deg = 0;
            if (this.rotation != 0.0)
            {
                //deg = (int)(this.rotation * 180 / Math.PI);
                deg = (int) this.rotation;
                if(deg!=0)
                {
                    sPoint.x = rect.x;
                    sPoint.y = rect.y;
                    GUIUtility.RotateAroundPivot(deg, sPoint);
                }
            }

            Color c = GUI.color;
            c.a = parentAlpha * this.alpha;
            GUI.color = c;
            GUI.Box(rect, GUIContent.none, _style);

            c.a = 1;
            GUI.color = c;

            if (deg != 0)
                GUIUtility.RotateAroundPivot(-deg, sPoint);

        }
    }
}
