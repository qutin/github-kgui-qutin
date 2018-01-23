using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class DisplayObject : EventDispatcher
    {
        private float _alpha;
        private float _x;
        private float _y;        
        private bool _visible;
        private float _scaleX;
        private float _scaleY;
        private float _pivotX;
        private float _pivotY;
        private float _skewX;
        private float _skewY;
        private float _rotation;
        protected bool _orientationChanged;
        private Matrix _transformationMatrix;
        private Matrix _transformationMatrixNR;//比_transformationMatrix少了rotaion，为了使用unity的旋转机制
        private bool _mouseEnabled;
        protected bool optimizeNotTouchable;

        public string name;

        public string internalId;
        private static uint INTENAL_INDEX = 0;

        private Container _parent;

        private static List<DisplayObject> sAncestors= new List<DisplayObject>();
        private static Matrix sHelperMatrix  = new Matrix();
        //
        public GObject gOwner { get; set; }
        public EventListener onClick { get; private set; }
        public EventListener onRightClick { get; private set; }
        public EventListener onTouchBegin { get; private set; }
        public EventListener onTouchEnd { get; private set; }
        public EventListener onRollOver { get; private set; }
        public EventListener onRollOut { get; private set; }
        public EventListener onMouseWheel { get; private set; }
        public EventListener onAddedToStage { get; private set; }
        public EventListener onRemovedFromStage { get; private set; }
        public EventListener onKeyDown { get; private set; }
        public EventListener onClickLink { get; private set; }
        public DisplayObject():base()
        {
            internalId = "DSO" + INTENAL_INDEX++;

            _alpha = 1;
            _x = 0;
            _y = 0;
            _visible = true;
            _scaleX = 1;
            _scaleY = 1;
            _pivotX = 0;
            _pivotY = 0;
            _skewX = 0;
            _skewY = 0;
            _rotation = 0;
            _parent = null;
            _transformationMatrix = new Matrix();
            _transformationMatrixNR = new Matrix();
            _mouseEnabled = true;
            //
            onClick = new EventListener(this, "onClick");
            onRightClick = new EventListener(this, "onRightClick");
            onTouchBegin = new EventListener(this, "onTouchBegin");
            onTouchEnd = new EventListener(this, "onTouchEnd");
            onRollOver = new EventListener(this, "onRollOver");
            onRollOut = new EventListener(this, "onRollOut");
            onMouseWheel = new EventListener(this, "onMouseWheel");
            onAddedToStage = new EventListener(this, "onAddedToStage");
            onRemovedFromStage = new EventListener(this, "onRemovedFromStage");
            onKeyDown = new EventListener(this, "onKeyDown");
            onClickLink = new EventListener(this, "onClickLink");

        }

        public float alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
        }

        public bool visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
            }
        }
        public float x
        {
            get { return _x; }
            set
            {
                if (_x != value)
                {
                    _x = value;
                    _orientationChanged = true;
                }
            }
        }
        public float y
        {
            get { return _y; }
            set 
            {
                if (_y != value)
                {
                    _y = value;
                    _orientationChanged = true;
                }
            }
        }
        public float pivotX
        {
            get { return _pivotX; }
            set
            {
                if (_pivotX != value)
                {
                    _pivotX = value;
                    _orientationChanged = true;
                }
            }
        }
        public float pivotY
        {
            get { return _pivotY; }
            set
            {
                if (_pivotY != value)
                {
                    _pivotY = value;
                    _orientationChanged = true;
                }
            }
        }
        virtual public float width 
        {
            get { return GetBounds(_parent).width; }
            set {
                scaleX = 1;
                float actualWidth = width;
                if (actualWidth != 0.0) scaleX = value / actualWidth;
            }
        }
        virtual public float height
        {
            get { return GetBounds(_parent).height; }
            set {
                scaleY = 1;
                float actualHeight = height;
                if (actualHeight != 0.0) scaleY = value / actualHeight;
            }
        }
        virtual public float scaleX
        {
            get { return _scaleX; }
            set {
                if(_scaleX!=value)
                {
                    _scaleX = value; 
                    _orientationChanged = true;
                }
            }
        }
        virtual public float scaleY
        {
            get { return _scaleY; }
            set {
                if (_scaleY != value)
                {
                    _scaleY = value;
                    _orientationChanged = true;
                }
            }
        }
        public float rotation
        {
            get { return _rotation; }
            set {
                //value = NormalizeAngle(value);
                if (_rotation != value)
                {
                    _rotation = value;
                    //rotation不再加到变换矩阵了，使用unity的机制实现旋转
                    //_orientationChanged = true;
                }
            }
        }

        private float NormalizeAngle(float angle)
        {
            // move into range [-180 deg, +180 deg]
            while (angle < -Math.PI) angle += (float)(Math.PI * 2.0);
            while (angle > Math.PI) angle -= (float)(Math.PI * 2.0);
            return angle;
        }

        public Container parent
        {
            get { return _parent; }
        }

        public void SetGrayed(bool value)
        {
        }

        internal void SetParent(Container value)
        {
            _parent = value;
        }

        public DisplayObject topmost
        {
            get
            {
                DisplayObject currentObject = this;
                while (currentObject._parent != null) currentObject = currentObject._parent;
                return currentObject;
            }
        }

        public DisplayObject root
        {
            get
            {
                DisplayObject currentObject = this;
                while (currentObject._parent != null)
                {
                    if (currentObject._parent is Stage) return currentObject;
                    else currentObject = currentObject._parent;
                }
                return null;
            }
        }

        public Stage stage
        {
            get { return topmost as Stage; }
        }

        public bool mouseEnabled
        {
            get { return _mouseEnabled; }
            set { _mouseEnabled = value; }
        }
        
        public Matrix transformationMatrix
        {
            get {
                if (_orientationChanged)
                {
                    UpdateMatrix();
                    UpdateMatrixNR();
                }
                return _transformationMatrix;
            }

            set
            {
                Matrix matrix = value;
                _orientationChanged = false;
                _transformationMatrix.copyFrom(matrix);

                _skewX = _skewY = 0;
            
                _x = matrix.tx;
                _y = matrix.ty;
                _scaleX = (float)Math.Sqrt(matrix.a * matrix.a + matrix.b * matrix.b);
                _skewY  = (float)Math.Acos(matrix.a / _scaleX);
            
                if (!IsEquivalent(matrix.b, _scaleX * Math.Sin(_skewY)))
                {
                    _scaleX *= -1;
                    _skewY = (float)Math.Acos(matrix.a / _scaleX);
                }
            
                _scaleY =(float) Math.Sqrt(matrix.c * matrix.c + matrix.d * matrix.d);
                _skewX  = (float)Math.Acos(matrix.d / _scaleY);
            
                if (!IsEquivalent(matrix.c, -_scaleY * Math.Sin(_skewX)))
                {
                    _scaleY *= -1;
                    _skewX = (float)Math.Acos(matrix.d / _scaleY);
                }
            
                if (IsEquivalent(_skewX, _skewY))
                {
                    _rotation = _skewX;
                    _skewX = _skewY = 0;
                }
                else
                {
                    _rotation = 0;
                }

                UpdateMatrixNR();
            }
        }

        public Matrix transformationMatrixNR
        {
            get {
                if (_orientationChanged)
                {
                    UpdateMatrix();
                    UpdateMatrixNR();
                }
                return _transformationMatrixNR;
            }
        }

        private void UpdateMatrix()
        {
            _orientationChanged = false;

            if (_skewX == 0.0 && _skewY == 0.0)
            {
                if (_rotation == 0.0)
                {
                    _transformationMatrix.setTo(_scaleX, 0, 0, _scaleY, 
                        _x - _pivotX * _scaleX, _y - _pivotY * _scaleY);
                    _transformationMatrixNR.copyFrom(_transformationMatrix);
                }
                else
                {
                    float cos = (float)Math.Cos(_rotation);
                    float sin = (float)Math.Sin(_rotation);
                    float a   = _scaleX *  cos;
                    float b   = _scaleX *  sin;
                    float c   = _scaleY * -sin;
                    float d   = _scaleY *  cos;
                    float tx  = _x - _pivotX * a - _pivotY * c;
                    float ty  = _y - _pivotX * b - _pivotY * d;
                        
                    _transformationMatrix.setTo(a, b, c, d, tx, ty);

                    cos = (float)Math.Cos(0);
                    sin = (float)Math.Sin(0);
                    a   = _scaleX *  cos;
                    b   = _scaleX *  sin;
                    c   = _scaleY * -sin;
                    d   = _scaleY *  cos;
                    tx  = _x - _pivotX * a - _pivotY * c;
                    ty  = _y - _pivotX * b - _pivotY * d;
                    _transformationMatrixNR.setTo(a, b, c, d, tx, ty);
                }
            }
            else
            {
                _transformationMatrix.identity();
                _transformationMatrix.scale(_scaleX, _scaleY);
                _transformationMatrix.skew(_skewX, _skewY);
                _transformationMatrix.rotate(_rotation);
                _transformationMatrix.translate(_x, _y);

                if (_pivotX != 0.0 || _pivotY != 0.0)
                {
                    // prepend pivot transformation
                    _transformationMatrix.tx = _x - _transformationMatrix.a * _pivotX
                                                  - _transformationMatrix.c * _pivotY;
                    _transformationMatrix.ty = _y - _transformationMatrix.b * _pivotX
                                                  - _transformationMatrix.d * _pivotY;
                }

                _transformationMatrixNR.identity();
                _transformationMatrixNR.scale(_scaleX, _scaleY);
                _transformationMatrixNR.skew(_skewX, _skewY);
                _transformationMatrixNR.translate(_x, _y);

                if (_pivotX != 0.0 || _pivotY != 0.0)
                {
                    // prepend pivot transformation
                    _transformationMatrixNR.tx = _x - _transformationMatrixNR.a * _pivotX
                                                  - _transformationMatrixNR.c * _pivotY;
                    _transformationMatrixNR.ty = _y - _transformationMatrixNR.b * _pivotX
                                                  - _transformationMatrixNR.d * _pivotY;
                }
            }
        }

        private void UpdateMatrixNR()
        {
            if (_skewX == 0.0 && _skewY == 0.0)
            {
                _transformationMatrixNR.setTo(_scaleX, 0, 0, _scaleY,
                     _x - _pivotX * _scaleX, _y - _pivotY * _scaleY);
            }
            else
            {
                _transformationMatrixNR.identity();
                _transformationMatrixNR.scale(_scaleX, _scaleY);
                _transformationMatrixNR.skew(_skewX, _skewY);
                _transformationMatrixNR.translate(_x, _y);

                if (_pivotX != 0.0 || _pivotY != 0.0)
                {
                    // prepend pivot transformation
                    _transformationMatrixNR.tx = _x - _transformationMatrixNR.a * _pivotX
                                                  - _transformationMatrixNR.c * _pivotY;
                    _transformationMatrixNR.ty = _y - _transformationMatrixNR.b * _pivotX
                                                  - _transformationMatrixNR.d * _pivotY;
                }
            }
        }

        private bool IsEquivalent(double a, double b, double epsilon=0.0001)
        {
            return (a - epsilon < b) && (a + epsilon > b);
        }

        /** Creates a matrix that represents the transformation from the local coordinate system 
         *  to another. If you pass a 'resultMatrix', the result will be stored in this matrix
         *  instead of creating a new object. */ 
        public Matrix GetTransformationMatrix(DisplayObject targetSpace,
                                                Matrix resultMatrix=null)
        {
            DisplayObject commonParent;
            DisplayObject currentObject;
            
            if (resultMatrix!=null) resultMatrix.identity();
            else resultMatrix = new Matrix();
            
            if (targetSpace == this)
            {
                return resultMatrix;
            }
            else if (targetSpace == _parent || (targetSpace == null && _parent == null))
            {
                resultMatrix.copyFrom(transformationMatrix);
                return resultMatrix;
            }
            else if (targetSpace == null || targetSpace == topmost)
            {
                // targetCoordinateSpace 'null' represents the target space of the base object.
                // -> move up from this to base
                
                currentObject = this;
                while (currentObject != targetSpace)
                {
                    resultMatrix.concat(currentObject.transformationMatrix);
                    currentObject = currentObject._parent;
                }
                
                return resultMatrix;
            }
            else if (targetSpace._parent == this) // optimization
            {
                targetSpace.GetTransformationMatrix(this, resultMatrix);
                resultMatrix.invert();
                
                return resultMatrix;
            }
            
            // 1. find a common parent of this and the target space
            
            commonParent = null;
            currentObject = this;
            
            while (currentObject!=null)
            {
                sAncestors.Add(currentObject);
                currentObject = currentObject._parent;
            }
            
            currentObject = targetSpace;
            while (currentObject!=null && sAncestors.IndexOf(currentObject) == -1)
                currentObject = currentObject._parent;
            
            sAncestors.Clear();
            
            if (currentObject!=null) commonParent = currentObject;
            else throw new Exception("Object not connected to target");
            
            // 2. move up from this to common parent
            
            currentObject = this;
            while (currentObject != commonParent)
            {
                resultMatrix.concat(currentObject.transformationMatrix);
                currentObject = currentObject._parent;
            }
            
            if (commonParent == targetSpace)
                return resultMatrix;
            
            // 3. now move up from target until we reach the common parent
            
            sHelperMatrix.identity();
            currentObject = targetSpace;
            while (currentObject != commonParent)
            {
                sHelperMatrix.concat(currentObject.transformationMatrix);
                currentObject = currentObject._parent;
            }
            
            // 4. now combine the two matrices
            
            sHelperMatrix.invert();
            resultMatrix.concat(sHelperMatrix);
            
            return resultMatrix;
        }

        internal Matrix GetTransformationMatrixNR(DisplayObject targetSpace,
                                                Matrix resultMatrix = null)
        {
            DisplayObject commonParent;
            DisplayObject currentObject;

            if (resultMatrix != null) resultMatrix.identity();
            else resultMatrix = new Matrix();

            if (targetSpace == this)
            {
                return resultMatrix;
            }
            else if (targetSpace == _parent || (targetSpace == null && _parent == null))
            {
                resultMatrix.copyFrom(transformationMatrixNR);
                return resultMatrix;
            }
            else if (targetSpace == null || targetSpace == topmost)
            {
                // targetCoordinateSpace 'null' represents the target space of the base object.
                // -> move up from this to base

                currentObject = this;
                while (currentObject != targetSpace)
                {
                    resultMatrix.concat(currentObject.transformationMatrixNR);
                    currentObject = currentObject._parent;
                }

                return resultMatrix;
            }
            else if (targetSpace._parent == this) // optimization
            {
                targetSpace.GetTransformationMatrixNR(this, resultMatrix);
                resultMatrix.invert();

                return resultMatrix;
            }

            // 1. find a common parent of this and the target space

            commonParent = null;
            currentObject = this;

            while (currentObject != null)
            {
                sAncestors.Add(currentObject);
                currentObject = currentObject._parent;
            }

            currentObject = targetSpace;
            while (currentObject != null && sAncestors.IndexOf(currentObject) == -1)
                currentObject = currentObject._parent;

            sAncestors.Clear();

            if (currentObject != null) commonParent = currentObject;
            else throw new Exception("Object not connected to target");

            // 2. move up from this to common parent

            currentObject = this;
            while (currentObject != commonParent)
            {
                resultMatrix.concat(currentObject.transformationMatrixNR);
                currentObject = currentObject._parent;
            }

            if (commonParent == targetSpace)
                return resultMatrix;

            // 3. now move up from target until we reach the common parent

            sHelperMatrix.identity();
            currentObject = targetSpace;
            while (currentObject != commonParent)
            {
                sHelperMatrix.concat(currentObject.transformationMatrixNR);
                currentObject = currentObject._parent;
            }

            // 4. now combine the two matrices

            sHelperMatrix.invert();
            resultMatrix.concat(sHelperMatrix);

            return resultMatrix;
        }

        /** Returns a rectangle that completely encloses the object as it appears in another 
         *  coordinate system. If you pass a 'resultRectangle', the result will be stored in this 
         *  rectangle instead of creating a new object. */ 
        virtual public Rect GetBounds(DisplayObject targetSpace)
        {
            return new Rect();
        }
        
        /** Returns the object that is found topmost beneath a point in local coordinates, or nil if 
         *  the test fails. If "forTouch" is true, untouchable and invisible objects will cause
         *  the test to fail. */
        virtual public DisplayObject HitTest(Vector2 localPoint, bool forTouch=false)
        {
            // on a touch test, invisible or untouchable objects cause the test to fail
            if (forTouch && (!_visible || !_mouseEnabled || optimizeNotTouchable)) return null;
            
            // otherwise, check bounding box
            if (GetBounds(this).Contains(localPoint)) return this;
            else return null;
        }
        
        /** Transforms a point from the local coordinate system to global (stage) coordinates.
         *  If you pass a 'resultPoint', the result will be stored in this point instead of 
         *  creating a new object. */
        public Vector2 LocalToGlobal(Vector2 localPoint)
        {
            GetTransformationMatrix(topmost, sHelperMatrix);
            return sHelperMatrix.transformCoords(localPoint.x, localPoint.y);
        }
        
        /** Transforms a point from global (stage) coordinates to the local coordinate system.
         *  If you pass a 'resultPoint', the result will be stored in this point instead of 
         *  creating a new object. */
        public Vector2 GlobalToLocal(Vector2 globalPoint)
        {
            GetTransformationMatrix(topmost, sHelperMatrix);
            sHelperMatrix.invert();
            return sHelperMatrix.transformCoords(globalPoint.x, globalPoint.y);
        }

        public bool hasVisibleArea
        {
            get
            {
                return _alpha != 0.0 && _visible && _scaleX != 0.0 && _scaleY != 0.0;
            }
        }

        public void RemoveFromParent()
        {
            if (_parent != null)
                _parent.RemoveChild(this);
        }

        virtual internal void HandleKeyEvent(KeyboardEvent evt)
        {
            DispatchEventObsolete(evt);
        }

        virtual public void Render(RenderSupport support, float parentAlpha)
        {

        }

        virtual public void Dispose()
        {
            RemoveEventListenersObsolete();
        }

    }
}
