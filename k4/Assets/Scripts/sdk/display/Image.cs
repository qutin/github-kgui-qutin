using System;
using System.Collections.Generic;
using UnityEngine;
 

namespace KGUI
{
    public class Image : DisplayObject
    {
        private Texture _texture;
        private float _width;
        private float _height;
        private Rect? _scale9Grid;
        private float _scale9Width;
        private float _scale9Height;
        private bool _scale9Changed;

        private float[] _rows;
        private float[] _cols;
        private float[] _dRows;
        private float[] _dCols;

        private static Matrix sHelperMatrix = new Matrix();

        public Image(Texture texture = null)
            : base()
        {
            _texture = texture;
            if (_texture != null)
            {
                _width = _texture.width;
                _height = _texture.height;
            }
            else
            {
                _width = 0;
                _height = 0;
            }
            optimizeNotTouchable = true;            

        }

        public Texture texture
        {
            get { return _texture; }
            set 
            {
                if (_texture != value)
                {
                    _texture = value;

                    if (_texture != null)
                    {
                        _width = _texture.width;
                        _height = _texture.height;
                    }
                    else
                    {
                        _width = 0;
                        _height = 0;
                    }
                    sacle9Changed = true;
                }
            }
        }

        private bool sacle9Changed
        {
            get{return _scale9Changed;}
            set
            {
                _scale9Changed = value;
            }
        }

        public Rect? scale9Grid
        {
            get { return _scale9Grid; }
            set {
                if (_scale9Grid != value)
                {
                    _scale9Grid = value;
                    sacle9Changed = true;
                }
            }
        }

        public float scale9Width
        {
            get { return _scale9Width; }
            set {
                if (_scale9Width != value)
                {
                    _scale9Width = value;
                    sacle9Changed = true;
                }
            }
        }
        public float scale9Height
        {
            get { return _scale9Height; }
            set {
                if (_scale9Height != value)
                {
                    _scale9Height = value;
                    sacle9Changed = true;
                }
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
                if (scaleX < 0) { resultRect.width  *= -1; resultRect.x -= resultRect.width;  }
                if (scaleY < 0) { resultRect.height *= -1; resultRect.y -= resultRect.height; }
            }
            else
            {
                GetTransformationMatrix(targetSpace, sHelperMatrix);
                resultRect = sHelperMatrix.transformRect(0, 0, _width, _height );
            }
            return resultRect;
        }

        private static Rect sHelperRect1 = new Rect();
        private static Rect sHelperRect2 = new Rect();
        private static Vector2 sPoint = new Vector2();

        public override void Render(RenderSupport support, float parentAlpha)
        {
            if (_texture == null)
                return;

            Rect rect = support.GetNativeDrawRect(_width, _height);

            int deg = 0;
            if (this.rotation != 0.0)
            {
                //deg = (int)(this.rotation * 180 / Math.PI);
                deg = (int) this.rotation;
                if(deg!=0)
                {
                    sPoint.x = rect.center.x;
                    sPoint.y = rect.center.y;                   
                    GUIUtility.RotateAroundPivot(deg, sPoint);

                }
            }
            Color c = GUI.color;
            c.a = parentAlpha * this.alpha;
            GUI.color = c;
            if (_scale9Grid == null || (rect.width == _width && rect.height == _height))
            {
                GUI.DrawTexture(rect, _texture);
            }
            else
            {
                if (_scale9Changed)
                {
                    _scale9Changed = false;
                    if (_scale9Width == 0)
                        _scale9Width = _width;
                    if (_scale9Height == 0)
                        _scale9Height = _height;

                    Rect tmpScale9Grid = (Rect) _scale9Grid;
                    _rows = new float[] { 0, tmpScale9Grid.yMin, tmpScale9Grid.yMax, _height };
                    _cols = new float[] { 0, tmpScale9Grid.xMin, tmpScale9Grid.xMax, _width };
                    _dRows = new float[] { 0, tmpScale9Grid.yMin, _scale9Height - (_height - tmpScale9Grid.yMax), _scale9Height };
                    _dCols = new float[] { 0, tmpScale9Grid.xMin, _scale9Width - (_width - tmpScale9Grid.xMax), _scale9Width };
                    //if (_scale9Height >= tmpScale9Grid.height)
                    //    _dRows = new float[] { 0, tmpScale9Grid.yMin, _scale9Height - (_height - tmpScale9Grid.yMax), _scale9Height };
                    //else
                    //{
                    //    float tmp = tmpScale9Grid.yMin / (_height - tmpScale9Grid.yMax);
                    //    tmp = _scale9Height * tmp / (1 + tmp);//不明算法
                    //    _dRows = new float[] { 0, tmp, tmp, _scale9Height }; //胡来缩放时就会不正确
                    //}

                    //if (_scale9Width >= tmpScale9Grid.width)
                    //    _dCols = new float[] { 0, tmpScale9Grid.xMin, _scale9Width - (_width - tmpScale9Grid.xMax), _scale9Width };
                    //else
                    //{
                    //    float tmp = tmpScale9Grid.xMin / (_width - tmpScale9Grid.xMax);
                    //    tmp = _scale9Width * tmp / (1 + tmp);
                    //    _dCols = new float[] { 0, tmp, tmp, _scale9Width };
                    //}
                }
                float scaleXLeft = rect.width / _scale9Width;
                float scaleYLeft = rect.height / _scale9Height;

                for (int cx = 0; cx < 3; cx++)
                {
                    for (int cy = 0; cy < 3; cy++)
                    {
                        //origin
                        //the texture coordinates are normalized. So in your case you would have 1/8, 2/8, 3/8... positions. i.e a percentage.
                        //the texCoord Y-axis is opposite to the destination.
                        sHelperRect1.x = _cols[cx] / _width;
                        sHelperRect1.y = 1 - _rows[cy + 1] / _height;//_rows[cy+1]/height
                        sHelperRect1.width = (_cols[cx + 1] - _cols[cx]) / _width;
                        sHelperRect1.height = (_rows[cy + 1] - _rows[cy]) / _height;
                        //sHelperRect1 确定九宫格原始比例,针对纹理坐标且纹理是原点是在左下角的
                        //sHelperRect2 位置和缩放功能，屏幕坐标原点左上角
                        //draw
                        sHelperRect2.x = (rect.x + _dCols[cx]) * scaleXLeft;
                        sHelperRect2.y = (rect.y + _dRows[cy]) * scaleYLeft;
                        sHelperRect2.width = (_dCols[cx + 1] - _dCols[cx]) * scaleXLeft;
                        sHelperRect2.height = (_dRows[cy + 1] - _dRows[cy]) * scaleYLeft;

                        if (sHelperRect2.width != 0 && sHelperRect2.height != 0)
                        {
                            GUI.DrawTextureWithTexCoords(sHelperRect2, _texture, sHelperRect1);
                        }                            
                    }
                }
            }

            c.a = 1;
            GUI.color = c;

            if (deg != 0)
                GUIUtility.RotateAroundPivot(-deg, sPoint);
        }
    }
}
