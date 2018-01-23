using System;
using System.Collections.Generic;
using UnityEngine;
 

namespace KGUI
{
    public class Sprite : Container
    {
        private Rect?  _clipRect;
        private Rect? _hitArea;

        private static Vector2 sHelperPoint =Vector2.zero;
        private static Matrix sHelperMatrix = new Matrix();

        public Sprite()
            : base()
        {
            
        }

        public Rect? clipRect
        {
            get  { return _clipRect; }
            set { _clipRect = value; }
        }

        public Rect? hitArea
        {
            get { return _hitArea; }
            set { _hitArea = value; }
        }

        /** Returns the bounds of the container's clipRect in the given coordinate space, or
         *  null if the sprite doens't have a clipRect. */ 
        public Rect GetClipRect(DisplayObject targetSpace)
        {
            if (_clipRect == null) return new Rect();
            
            float x=0, y=0;
            float minX =  float.MaxValue;
            float maxX = -float.MaxValue;
            float minY =  float.MaxValue;
            float maxY = -float.MaxValue;
            Matrix transMatrix = GetTransformationMatrix(targetSpace, sHelperMatrix);
            
            Rect cliprect = (Rect) _clipRect;
            for (int i=0; i<4; ++i)
            {
                switch(i)
                {
                    case 0: x = cliprect.xMin;  y = cliprect.yMin;    break;
                    case 1: x = cliprect.xMin;  y = cliprect.yMax; break;
                    case 2: x = cliprect.xMax; y = cliprect.yMin;    break;
                    case 3: x = cliprect.xMax; y = cliprect.yMax; break;
                }
                Vector2 transformedPoint = transMatrix.transformCoords(x, y );
                sHelperPoint = transformedPoint;
                
                if (minX > transformedPoint.x) minX = transformedPoint.x;
                if (maxX < transformedPoint.x) maxX = transformedPoint.x;
                if (minY > transformedPoint.y) minY = transformedPoint.y;
                if (maxY < transformedPoint.y) maxY = transformedPoint.y;
            }
            
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        internal Rect GetClipRectNR(DisplayObject targetSpace)
        {
            if (_clipRect == null) return new Rect();
            
            float x=0, y=0;
            float minX =  float.MaxValue;
            float maxX = -float.MaxValue;
            float minY =  float.MaxValue;
            float maxY = -float.MaxValue;
            Matrix transMatrix = GetTransformationMatrixNR(targetSpace, sHelperMatrix);

            Rect cliprect = (Rect)_clipRect;
            for (int i=0; i<4; ++i)
            {
                switch(i)
                {
                    case 0: x = cliprect.xMin;  y = cliprect.yMin;    break;
                    case 1: x = cliprect.xMin;  y = cliprect.yMax; break;
                    case 2: x = cliprect.xMax; y = cliprect.yMin;    break;
                    case 3: x = cliprect.xMax; y = cliprect.yMax; break;
                }
                Vector2 transformedPoint = transMatrix.transformCoords(x, y);
                sHelperPoint = transformedPoint;
                if (minX > transformedPoint.x) minX = transformedPoint.x;
                if (maxX < transformedPoint.x) maxX = transformedPoint.x;
                if (minY > transformedPoint.y) minY = transformedPoint.y;
                if (maxY < transformedPoint.y) maxY = transformedPoint.y;
            }
            
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
        
        /** @inheritDoc */ 
        public override Rect GetBounds(DisplayObject targetSpace)
        {
            Rect bounds = base.GetBounds(targetSpace);
            
            // if we have a scissor rect, intersect it with our bounds
            if (_clipRect != null)
            {
                Rect clip = GetClipRect(targetSpace);
                bounds = ToolSet.Intersection(ref bounds, ref clip);
            }
            return bounds;
        }
        
        /** @inheritDoc */
        public override DisplayObject HitTest(Vector2 localPoint, bool forTouch=false)
        {
			if (forTouch && (!visible || !mouseEnabled || optimizeNotTouchable)) 
				return null;

            if (_clipRect != null && !((Rect)_clipRect).Contains(localPoint))
                return null;
            else
            {
                //important
                float localX = localPoint.x;
                float localY = localPoint.y;

                DisplayObject ret = base.HitTest(localPoint, forTouch);
                if (ret == null && _hitArea!=null)
                {
                    localPoint.x = localX;
                    localPoint.y = localY;
                    if ( ((Rect)_hitArea).Contains(localPoint))
                        ret = this;
                }

                return ret;
            }
        }

        override public void Render(RenderSupport support, float parentAlpha)
        {
            
            if (_clipRect != null)
            {
                
                Rect clipRect = support.PushClipRect(GetClipRectNR(stage));
                if (ToolSet.IsEmptyRect(ref clipRect))
                {
                    // empty clipping bounds - no need to render children.
                    support.PopClipRect();
                    return;
                }
            }
            base.Render(support, parentAlpha);

            if (_clipRect != null)
                support.PopClipRect();
        }
    }
}
