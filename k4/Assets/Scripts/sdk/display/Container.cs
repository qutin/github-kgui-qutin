using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

namespace KGUI
{
    public class Container : DisplayObject
    {
        private List<DisplayObject> _children;

        private static Vector2 sHelperPoint = Vector2.zero;
        private static Matrix sHelperMatrix = new Matrix();
        private static List<DisplayObject> sBroadcastListeners = new List<DisplayObject>();
        private static EventContext sHelperEvent1 = new EventContext(EventContext.ADDED_TO_STAGE, false);
        private static EventContext sHelperEvent2 = new EventContext(EventContext.REMOVED_FROM_STAGE, false);

        //由于渲染和事件不同步所以需要缓存
        private bool _isRendering;
        private List<int> _cacheAddListIdx;
        private List<DisplayObject> _cacheAddList;
        private List<DisplayObject> _cahcheRemoveList;
        public Container():base()
        {
            _children = new List<DisplayObject>();
            _cacheAddListIdx = new List<int>();
            _cacheAddList = new List<DisplayObject>();
            _cahcheRemoveList = new List<DisplayObject>();
        }
        public int numChildren
        {
            get { return _children.Count; }
        }

        private void CacheAddList(int idx, DisplayObject obj)
        {
            _cacheAddListIdx.Add(idx);
            _cahcheRemoveList.Add(obj);
        }

        private void CacheClear()
        {
            _cacheAddList.Clear();
            _cacheAddListIdx.Clear();
            _cahcheRemoveList.Clear();
        }
        public DisplayObject AddChild(DisplayObject child)
        {

            AddChildAt(child, _children.Count);
            return child;
        }
        public DisplayObject AddChildAt(DisplayObject child, int index)
        {
            int numChildren =  _children.Count;
            if (index >= 0 && index <= numChildren)
            {
                if (child.parent == this)
                {
                    SetChildIndex(child, index);
                }
                else
                {
                    child.RemoveFromParent();
                    if (index == numChildren)
                    {
                        if(_isRendering)
                            CacheAddList(_children.Count, child);
                        else
                            _children.Add(child);
                    }
                    else
                    {
                        if (_isRendering)
                            CacheAddList(index, child);
                        else
                            _children.Insert(index, child);
                    }
                    child.SetParent(this);

                    if (stage != null)
                    {
                        Container container = child as Container;
                        if (container != null)
                        {
                            //container.BroadcastEvent(sHelperEvent1);
                            container.onAddedToStage.BroadcastCall();
                        }
                        else
                        {
                            //child.DispatchEventObsolete(sHelperEvent1);
                            child.onAddedToStage.Call();
                        }
                    }
                }
                return child;
            }
            else
            {
                throw new Exception("Invalid child index");
            }
        }
        public bool Contains(DisplayObject child)
        {
            return _children.Contains(child);
        }
        public DisplayObject GetChildAt(int index)
        {
            return _children[index];
        }
        public int GetChildIndex(DisplayObject child)
        {
            return _children.IndexOf(child);
        }        
        public DisplayObject RemoveChild(DisplayObject child)
        {
            if (child.parent != this)
                throw new Exception("obj is not a child");

            int i = _children.IndexOf(child);
            return RemoveChildAt(i);
        }

        public DisplayObject RemoveChildAt(int index)
        {
            if (index >= 0 && index < _children.Count)
            {
                DisplayObject child = _children[index];

                if (stage != null)
                {
                    Container container = child as Container;
                    if (container != null)
                    {
                        //container.BroadcastEvent(sHelperEvent2);
                        container.onRemovedFromStage.BroadcastCall();
                    }
                    else
                    {
                        //child.DispatchEventObsolete(sHelperEvent2);
                        child.onRemovedFromStage.Call();
                    }
                }


                if (_isRendering)
                    _cahcheRemoveList.Add(child);
                else
                    _children.Remove(child);

                child.SetParent(null);

                return child;
            }
            else
                throw new Exception("Invalid child index");
        }

        public void RemoveChildren(int beginIndex=0, int endIndex = int.MaxValue)
        {
			if (endIndex < 0 || endIndex >= numChildren) 
				endIndex = numChildren - 1;

            for (int i = beginIndex; i <= endIndex; ++i)
				RemoveChildAt(beginIndex);           
        }

        public void SetChildIndex(DisplayObject child, int index)
        {
            int oldIndex = _children.IndexOf(child);
            if (oldIndex == index) return;
            if (oldIndex == -1) throw new ArgumentException("Not a child of this container");
            _children.RemoveAt(oldIndex);
            if (index >= _children.Count)
            {
                if (_isRendering)
                    CacheAddList(_children.Count, child);
                else
                    _children.Add(child);
            }
            else
            {
                if (_isRendering)
                    CacheAddList(index, child);
                else
                    _children.Insert(index, child);
            }
        }

        public void SwapChildren(DisplayObject child1, DisplayObject child2)
        {
            int index1 = _children.IndexOf(child1);
            int index2 = _children.IndexOf(child2);
            if (index1 == -1 || index2 == -1)
                throw new Exception("Not a child of this container");
            SwapChildrenAt(index1, index2);
        }

        public void SwapChildrenAt(int index1, int index2)
        {
            DisplayObject obj1 = _children[index1];
            DisplayObject obj2 = _children[index2];
            _children[index1] = obj2;
            _children[index2] = obj1;
        }

        private static Vector2 sPoint = new Vector2();
        override public void Render(RenderSupport support, float parentAlpha)
        {
            int deg = 0;
            if (this.rotation != 0.0)
            {
                //deg = (int)(this.rotation * 180 / Math.PI);
                deg = (int) this.rotation;
                if (deg != 0)
                {
                    sHelperPoint = support.mvpMatrix.transformCoords(pivotX, pivotY);
                    sPoint.x = sHelperPoint.x;
                    sPoint.y = sHelperPoint.y;
                    GUIUtility.RotateAroundPivot(deg, sPoint);
                }
            }

            float alpha = parentAlpha * this.alpha;
            _isRendering = true;
            foreach (DisplayObject child in _children)
            {
                if (child.hasVisibleArea)
                {
                    support.PushMatrix();
                    support.TransformMatrix(child);
                    child.Render(support, alpha);
                    support.PopMatrix();
                }
            }
            _isRendering = false;
            for (int i = 0; i < _cacheAddList.Count; i++)
            {
                int idx = _cacheAddListIdx[i];
                if(idx == _children.Count)
                    _children.Add(_cacheAddList[i]);
                else
                    _children.Insert(idx, _cacheAddList[i]);
            }
            foreach (var obj in _cahcheRemoveList)
            {
                _children.Remove(obj);
            }
            CacheClear();
            if (deg != 0)
                GUIUtility.RotateAroundPivot(-deg, sPoint);

        }

        override public Rect GetBounds(DisplayObject targetSpace)
        {
            Rect resultRect = new Rect();
            
            int numChildren = _children.Count;
            
            if (numChildren == 0)
            {
                GetTransformationMatrix(targetSpace, sHelperMatrix);
                sHelperPoint = sHelperMatrix.transformCoords(0, 0);
                resultRect.Set(sHelperPoint.x, sHelperPoint.y, 0, 0);
            }
            else if (numChildren == 1)
            {
                resultRect = _children[0].GetBounds(targetSpace);
            }
            else
            {
                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;
                
                for (int i=0; i<numChildren; ++i)
                {
                    resultRect = _children[i].GetBounds(targetSpace);
                    minX = minX < resultRect.x ? minX : resultRect.x;
                    maxX = maxX > resultRect.xMax ? maxX : resultRect.xMax;
                    minY = minY < resultRect.y ? minY : resultRect.y;
                    maxY = maxY > resultRect.yMax ? maxY : resultRect.yMax;
                }
                
                resultRect.Set(minX, minY, maxX - minX, maxY - minY);
            }                
            
            return resultRect;
        }

        override public DisplayObject HitTest(Vector2 localPoint, bool forTouch = false)
        {
			if (forTouch && (!visible || !mouseEnabled || optimizeNotTouchable)) 
				return null;

            float localX = localPoint.x;
            float localY = localPoint.y;

            for (int i = numChildren - 1; i >= 0; --i) // front to back!
            {
                DisplayObject child = _children[i];
                GetTransformationMatrix(child, sHelperMatrix);

                sHelperPoint = sHelperMatrix.transformCoords(localX, localY);
                DisplayObject target = child.HitTest(sHelperPoint, forTouch);

                if (target != null) return target;
            }

            return null;
        }

        /** Dispatches an event on all children (recursively). The event must not bubble. */
        //public void BroadcastEvent(EventContext evt)
        //{
        //    if (evt.Bubbles)
        //        throw new Exception("Broadcast of bubbling events is prohibited");
            
        //    // The event listeners might modify the display tree, which could make the loop crash. 
        //    // Thus, we collect them in a list and iterate over that list instead.
        //    // And since another listener could call this method internally, we have to take 
        //    // care that the static helper vector does not get currupted.
            
        //    int fromIndex = sBroadcastListeners.Count;
        //    GetChildEventListeners(this, evt.Type, sBroadcastListeners);
        //    int toIndex = sBroadcastListeners.Count;
            
        //    for (int i=fromIndex; i<toIndex; ++i)
        //        sBroadcastListeners[i].DispatchEventObsolete(evt);

        //    sBroadcastListeners.RemoveRange(fromIndex, sBroadcastListeners.Count - fromIndex);
        //}

            /** @private */
        internal void GetChildEventListeners(DisplayObject obj, string eventType, 
                                                 List<DisplayObject> listeners)
        {
            Container container = obj as Container;

            if (obj.HasEventListener(eventType))
                listeners.Add(obj);

            if (container!=null)
            {
                List<DisplayObject> children = container._children;
                int numChildren = children.Count;
                
                for (int i=0; i<numChildren; ++i)
                    GetChildEventListeners(children[i], eventType, listeners);
            }
        }
    }
}
