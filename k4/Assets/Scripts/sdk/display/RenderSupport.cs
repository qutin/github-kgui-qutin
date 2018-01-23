using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using UnityEngine;

namespace KGUI
{
    public class RenderSupport
    {
        private Matrix mProjectionMatrix;
        private Matrix mModelViewMatrix;
        private Matrix mMvpMatrix;
        private List<Matrix> mMatrixStack;
        private int mMatrixStackSize;
        private List<Rect> mClipRectStack;
        private int mClipRectStackSize;
        private bool clipped;

        private static Rect sHelperRect = new Rect();
        private static Rect sGroupRect = new Rect();

        public RenderSupport()
        {
            mProjectionMatrix = new Matrix();
            mModelViewMatrix = new Matrix();
            mMvpMatrix = new Matrix();
            mMatrixStack = new List<Matrix>();
            mMatrixStackSize = 0;
            mClipRectStack = new List<Rect>();

            mModelViewMatrix.identity();
        }

        public void Reset()
        {
            ApplyClipRect();
        }

         /** Pushes the current modelview matrix to a stack from which it can be restored later. */
        public void PushMatrix()
        {
            if (mMatrixStack.Count < mMatrixStackSize + 1)
                mMatrixStack.Add(new Matrix());

            mMatrixStack[(int)(mMatrixStackSize++)].copyFrom(mModelViewMatrix);
        }
        
        /** Restores the modelview matrix that was last pushed to the stack. */
        public void PopMatrix()
        {
            mModelViewMatrix.copyFrom(mMatrixStack[(int)(--mMatrixStackSize)]);
        }

        public void TransformMatrix(DisplayObject obj)
        {
            mModelViewMatrix.prependMatrix(obj.transformationMatrixNR);
        }

        public Matrix mvpMatrix
        {
            get
            {
                mMvpMatrix.copyFrom(mModelViewMatrix);
                mMvpMatrix.concat(mProjectionMatrix);
                return mMvpMatrix;
            }
        }

        public Matrix projectionMatrix
        {
            get
            {
                return mProjectionMatrix;
            }
        }

         /** The clipping rectangle can be used to limit rendering in the current render target to
         *  a certain area. This method expects the rectangle in stage coordinates. Internally,
         *  it uses the 'scissorRectangle' of stage3D, which works with pixel coordinates. 
         *  Any pushed rectangle is intersected with the previous rectangle; the method returns
         *  that intersection. */ 
        //public Rectangle PushClipRect(Rectangle rectangle)
        //{
        //    if (mClipRectStack.Count < mClipRectStackSize + 1)
        //        mClipRectStack.Add(new Rectangle());
            
        //    mClipRectStack[mClipRectStackSize].copyFrom(rectangle);
        //    rectangle = mClipRectStack[mClipRectStackSize];
            
        //    // intersect with the last pushed clip rect
        //    if (mClipRectStackSize > 0)
        //        rectangle.intersect(mClipRectStack[mClipRectStackSize-1], 
        //                                rectangle);
            
        //    ++mClipRectStackSize;
        //    ApplyClipRect();
            
        //    // return the intersected clip rect so callers can skip draw calls if it's empty
        //    return rectangle;
        //}
        public Rect PushClipRect(Rect rectangle)
        {
            if (mClipRectStack.Count < mClipRectStackSize + 1)
                mClipRectStack.Add(new Rect());

            mClipRectStack[mClipRectStackSize] = rectangle;

            // intersect with the last pushed clip rect
            if (mClipRectStackSize > 0)
            {
                 Rect clip = mClipRectStack[mClipRectStackSize - 1];
                 rectangle = ToolSet.Intersection(ref clip, ref rectangle);
                 mClipRectStack[mClipRectStackSize] = rectangle;
            }
                
            ++mClipRectStackSize;
            ApplyClipRect();

            // return the intersected clip rect so callers can skip draw calls if it's empty
            return rectangle;
        }
        
        /** Restores the clipping rectangle that was last pushed to the stack. */
        public void PopClipRect()
        {
            if (mClipRectStackSize > 0)
            {
                --mClipRectStackSize;
                ApplyClipRect();
            }
        }

        public void ApplyClipRect()
        {
            if (clipped)
            {
                clipped = false;
                GUI.EndGroup();
                mProjectionMatrix.identity();
            }
            
            if (mClipRectStackSize > 0)
            {
                Rect rect = mClipRectStack[mClipRectStackSize-1];
                sGroupRect.x = rect.x;
                sGroupRect.y = rect.y;
                sGroupRect.width = rect.width;
                sGroupRect.height = rect.height;

                clipped = true;
                GUI.BeginGroup(sGroupRect);
                mProjectionMatrix.translate(-sGroupRect.x, -sGroupRect.y);
            }
        }

        public Rect GetNativeDrawRect(float width, float height, bool applyProjection=true)
        {
            Rect result = new Rect();

            mMvpMatrix.copyFrom(mModelViewMatrix);
            if(applyProjection)
                mMvpMatrix.concat(mProjectionMatrix);
            sHelperRect = mMvpMatrix.transformRectNoRotate(0, 0, width, height);
            result.x = sHelperRect.x;
            result.y = sHelperRect.y;
            result.width = sHelperRect.width;
            result.height = sHelperRect.height;
            return result;
        }
    }
}
