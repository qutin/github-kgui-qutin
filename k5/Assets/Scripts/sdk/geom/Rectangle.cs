using System;
using System.Collections.Generic;
using UnityEngine;

namespace KGUI
{
    public class Rectangle
    {
        private float _x;
        private float _y;
        private float _width;
        private float _height;

        public Rectangle( Rect rect )
        {
            _x = rect.x;
            _y = rect.y;
            _width = rect.width;
            _height = rect.height;
        }

        public Rectangle(float x = 0, float y = 0, float width = 0, float height = 0)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public float x
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }

        public float y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }

        public float width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }

        public float height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }
        public float bottom
        {
            get
            {
                return _y+_height;
            }
            set { _height = value - _y; }
        }
        public float top
        {
            get
            {
                return _y;
            }
        }
        public Point bottomRight
        {
            get
            {
                return new Point(_x + _width, _y + _height);
            }
        }
        public Point topLeft
        {
            get
            {
                return new Point(_x , _y );
            }
        }
        public float left
        {
            get { return _x; }
        }
        public float right
        {
            get { return _x + _width; }
            set { _width = value - _x; }
        }

        public bool isEmpty()
        {
            return _x==0 && _y==0 && _width == 0 || _height == 0;
        }

        public bool containsPoint(Vector2 pt)
        {
            return pt.x >= _x && pt.y >= _y && pt.x < _x + _width && pt.y < _y + _height;
        }

        public void setTo(float x = 0, float y = 0, float width = 0, float height = 0)
        {
            _x = x;
            _y = y;
            _width = width;
            _height = height;
        }

        public void copyFrom(Rectangle rect)
        {
            _x = rect.x;
            _y = rect.y;
            _width = rect.width;
            _height = rect.height;
        }

        public Rectangle clone()
        {
            return new Rectangle(_x, _y, _width, _height);
        }

        public void setEmpty()
        {
            _x = 0;
            _y = 0;
            _width = 0;
            _height = 0;
        }

        public Rectangle intersect(Rectangle rect2, Rectangle resultRect=null)
        {
            if (resultRect == null) resultRect = new Rectangle();

            float left = this.x > rect2.x ? this.x : rect2.x;
            float right = this.right < rect2.right ? this.right : rect2.right;
            float top = this.y > rect2.y ? this.y : rect2.y;
            float bottom = this.bottom < rect2.bottom ? this.bottom : rect2.bottom;
            
            if (left > right || top > bottom)
                resultRect.setEmpty();
            else
                resultRect.setTo(left, top, right-left, bottom-top);
            
            return resultRect;
        }
    }
}
