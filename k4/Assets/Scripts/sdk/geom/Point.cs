using System;
using System.Collections.Generic;

using UnityEngine;

namespace KGUI
{
    public class Point
    {
        private float _x;
        private float _y;

        public Point(Point pt)
        {
            _x = pt.x;
            _y = pt.y;
        }

        public Point(Vector2 vec2)
        {
            if (vec2 == null)
            {
                _x = 0;
                _y = 0;
            }
            else
            {
                _x = vec2.x;
                _y = vec2.y;
            }
        }

        public Point(float x = 0, float y = 0)
        {
            _x = x;
            _y = y;
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

        public Vector2 toVector2()
        {
            return new Vector2(_x, _y);
        }

        public void copyFrom(Point sourcePoint)
        {
            _x = sourcePoint.x;
            _y = sourcePoint.y;
        }

    }
}
