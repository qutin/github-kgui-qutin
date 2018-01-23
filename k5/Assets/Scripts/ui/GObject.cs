using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class GObject : EventDispatcher
    {
	    public object data;

        protected float _x;
		protected float _y;
		protected float _width;
		protected float _height;
		protected int _sourceWidth;
		protected int _sourceHeight;
        protected int _initWidth;
		protected int _initHeight;
		protected float _alpha;
        protected float _rotation;
		protected bool _visible;
        int _internalVisible;
		protected bool _touchable;
		protected bool _hidden;
		protected bool _grayed;
        protected bool _draggable;
        protected Rect? _dragBounds;

		protected Relations _relations;
		protected GGroup _group;
		
		protected GearDisplay _gearDisplay;
		protected GearXY _gearXY;
		protected GearSize _gearSize;
        public GearLook gearLook { get; private set; }
        internal bool _gearLocked;
		
		protected GComponent _parent;
        protected DisplayObject _displayObject;

        private DragInitiator _dragInitiator;
        private DragHelper _dragHelper;

        internal string _id;
        internal string _name;
        internal float _dx;
        internal float _dy;
        internal PackageItem _packageItem;

        //
        bool _focusable;
        string _tooltips;
        internal protected bool underConstruct;
        internal XML constructingData { get; set; }

        private static OutlineChangeEvent XY_CHANGE = new OutlineChangeEvent(OutlineChangeEvent.XY);
        private static OutlineChangeEvent SIZE_CHANGE = new OutlineChangeEvent(OutlineChangeEvent.SIZE);

        public AppContext appContext { get; protected set; }
        public EventListener onClick { get; private set; }
        public EventListener onRightClick { get; private set; }
        public EventListener onTouchBegin { get; private set; }
        public EventListener onTouchEnd { get; private set; }
        public EventListener onRollOver { get; private set; }
        public EventListener onRollOut { get; private set; }
        public EventListener onAddedToStage { get; private set; }
        public EventListener onRemovedFromStage { get; private set; }
        public EventListener onKeyDown { get; private set; }
        public EventListener onClickLink { get; private set; }
        public EventListener onPositionChanged { get; private set; }
        public EventListener onSizeChanged { get; private set; }
        public EventListener onDragStart { get; private set; }
        public EventListener onDragEnd { get; private set; }

		public GObject()
		{
			_width = 0;
			_height = 0;
			_name = "";
			_alpha = 1;
			_rotation = 0;
			_visible = true;
            _internalVisible = 1;
			_touchable = true;

			
			
			_relations = new Relations(this);
			
			_gearDisplay = new GearDisplay(this);
            _gearXY = new GearXY(this);
			_gearSize = new GearSize(this);
            gearLook = new GearLook(this);

            onClick = new EventListener(this, "onClick");
            onRightClick = new EventListener(this, "onRightClick");
            onTouchBegin = new EventListener(this, "onTouchBegin");
            onTouchEnd = new EventListener(this, "onTouchEnd");
            onRollOver = new EventListener(this, "onRollOver");
            onRollOut = new EventListener(this, "onRollOut");
            onAddedToStage = new EventListener(this, "onAddedToStage");
            onRemovedFromStage = new EventListener(this, "onRemovedFromStage");
            onKeyDown = new EventListener(this, "onKeyDown");
            onClickLink = new EventListener(this, "onClickLink");

            onPositionChanged = new EventListener(this, "onPositionChanged");
            onSizeChanged = new EventListener(this, "onSizeChanged");
            onDragStart = new EventListener(this, "onDragStart");
            onDragEnd = new EventListener(this, "onDragEnd");
		}

        virtual public void OnCreate(AppContext context)
        {
            appContext = context;
            CreateDisplayObject();
        }
		public string id
		{
            get
            {
                return _id;
            }
	
		}

		public string name
		{
            get
            {
                return _name;
            }
			set
            {
                _name = value;
            }
		}		

		public float x
		{
            get
            {
                return _x;
            }		
	        set
            {
                SetXY(value, _y);
            }
		}
		
		
		public float y
		{
            get{
                return _y;
            }

			set{
                SetXY(_x, value);
            }
		}
		
		public void SetXY(float xv, float yv)
		{
			if(_x!=xv || _y!=yv)
			{
				_dx = xv-_x;
				_dy = yv-_y;
				_x = xv;
				_y = yv;
				
				HandlePositionChanged();
				
				if(_parent != null)
				{
					if(!_relations.handling)
						_relations.Refresh();

					_parent.SetBoundsChangedFlag();
				}

                if (_gearXY.controller != null)
                    _gearXY.UpdateState();

                DispatchEventObsolete(XY_CHANGE);
			}
		}
		
		public float height
		{

            get{
                return _height;
            }
			set{
                SetSize(_width, value);
            }
		}
		
		public float width
		{

            get{
                return _width;
            }
            set{
                SetSize(value, _height);
            }
			
		}
					
		public void SetSize(float wv, float hv)
		{
			if(_width!=wv || _height!=hv)
			{
				_width = wv;
				_height = hv;
				
				HandleSizeChanged();
				
				if(_parent != null)
				{
					_relations.ApplyOnSelfResized();
					
					if(!_relations.handling)
						_relations.Refresh();

					_parent.SetBoundsChangedFlag();
				}

                if (_gearSize.controller != null)
                    _gearSize.UpdateState();

                DispatchEventObsolete(SIZE_CHANGE);
			}
		}
        public Vector2 size
        {
            get { return new Vector2(width, height); }
            set { SetSize(value.x, value.y); }
        }
		public int sourceHeight
		{

            get{
                return _sourceHeight;
            }        			
		}

		public int sourceWidth
		{

            get{
                return _sourceWidth;
            }
		}

        public int initHeight
        {

            get
            {
                return _initHeight;
            }
        }

        public int initWidth
        {
            get
            {
                return _initWidth;
            }
        }

        public int actualHeight
		{
            get
            {
			    return (int)(this.height*GRoot.contentScaleFactor);
            }
		}
		
		public int actualWidth
		{
            get
            {
			    return (int)(this.width*GRoot.contentScaleFactor);
            }
		}
		
		public bool touchable
		{

            get{
                return _touchable;
            }
            set
            {
                _touchable = value;
                if(_displayObject!=null)
                    _displayObject.mouseEnabled = _touchable;
            }	
		}

        public bool grayed
        {

            get
            {
                return _grayed;
            }
            set
            {
                if (_grayed != value)
                {
                    _grayed = value;
                    HandleGrayedChanged();

                    if (gearLook.controller != null)
                        gearLook.UpdateState();
                }
            }
        }

        public bool enabled
        {

            get
            {
                return !_grayed && _touchable;
            }
            set
            {
                this.grayed = !value;
                this.touchable = value; 
            }
        }

        public float rotation
		{
            get{
                return _rotation;
            }
            set
            {
                _rotation = value;
                if (_displayObject != null)
                    _displayObject.rotation = _rotation;
                if (gearLook.controller != null)
                    gearLook.UpdateState();
            }
		}				
		
		public float alpha
		{

            get{
                return _alpha;
            }

			set{
                _alpha = value;
                if (_displayObject != null)
                    _displayObject.alpha = _alpha;
            }
		}
        virtual protected void UpdateAlpha()
        {
            if (displayObject != null)
                displayObject.alpha = _alpha;

            if (gearLook.controller != null)
                gearLook.UpdateState();
        }
		
		public bool visible
		{
            get{
                return _visible;
            }

            set{
                if(_visible!=value)
			    {
                    _visible = value;
                    if (_displayObject!=null)
                        _displayObject.visible = _visible;
                    if (_parent != null)
                        _parent.ChildStateChanged(this);
			    }        
            }
			
		}
        internal int internalVisible
        {
            get { return _internalVisible; }
            set
            {
                if (value < 0)
                    value = 0;
                bool oldValue = _internalVisible > 0;
                bool newValue = value > 0;
                _internalVisible = value;
                if (oldValue != newValue)
                {
                    if (_parent != null)
                        _parent.ChildStateChanged(this);
                }
            }
        }
		internal bool hidden
		{

            set
            {
                if(_hidden!=value)
			    {
				    _hidden = value;
                    if (_parent!=null)
                        _parent.ChildStateChanged(this);
			    }
            }
		}
		
		internal bool finalVisible
		{
            get
            {
                return _visible && _internalVisible > 0 && (_group == null || _group.finalVisible);
            }
		}
		
		public bool displayObjectAdded
		{
            get
            {
                return _displayObject != null && _displayObject.parent!=null;                
            }
		}
		
		public GGroup group
		{
            get{
                return _group;
            }

            set{
                _group = value;
            }			
		}
		
		public GearDisplay gearDisplay
		{
            get
            {
                return _gearDisplay;
            }			
		}
		
		public GearXY gearXY
		{
            get{
        	    return _gearXY;
            }		
		}
		
		public GearSize gearSize
		{
            get
            {
                return _gearSize;
            }			
		}

		virtual public void HandleControllerChanged(Controller c)
		{
            if (_gearDisplay.controller == c)
                _gearDisplay.Apply();
            if (_gearXY.controller == c)
                _gearXY.Apply();
            if (_gearSize.controller == c)
                _gearSize.Apply();
            if (gearLook.controller == c)
                gearLook.Apply();
		}
		
		public Relations relations
		{
            get{
                return _relations;
            }			
		}

        public void AddRelation(GObject target, RelationType relationType, bool usePercent = false)
        {
            _relations.Add(target, relationType, usePercent);
        }

        public DisplayObject displayObject
        {
            get { return _displayObject; }
            protected set { _displayObject = value; }
        }
		
		public GComponent parent
		{
            get{
                return _parent;
            }
            set
            {
                _parent = value;
            }
		}
        //FairyGUI
        public GRoot root
        {
            get
            {
                if (this is GRoot)
                    return (GRoot)this;

                GObject p = parent;
                while (p != null)
                {
                    if (p is GRoot)
                        return (GRoot)p;
                    p = p.parent;
                }
                //return GRoot.inst;
                return appContext.groot;
            }
        }
		public void RemoveFromParent()
		{
			if(_parent != null)
				_parent.RemoveChild(this);
		}

        virtual public void Dispose()
		{
            if (_displayObject != null)
            {
                _displayObject.gOwner = null;
                _displayObject.Dispose();
                _displayObject = null;
            }
            _relations.Dispose();
		}

        public Vector2 LocalToGlobal(Vector2? pt=null)
		{
			Vector2 ret;
            if (pt != null)
                ret = (Vector2) pt;
			else
				ret = new Vector2( 0, 0);
			GComponent aParent = this.parent;
			float ax=_x, ay=_y;
			while(aParent!=null)
			{
				ret.x += ax;
				ret.y += ay;
				ax = aParent.x;
				ay = aParent.y;
				aParent = aParent.parent;
			}
			return ret;
		}
        public Vector2 GlobalToLocal(Vector2 pt)
        {
            return displayObject.GlobalToLocal(pt);
        }
        public Rect LocalToGlobal(Rect rect)
        {
            Rect ret = new Rect();
            Vector2 v = this.LocalToGlobal(new Vector2(rect.xMin, rect.yMin));
            ret.xMin = v.x;
            ret.yMin = v.y;
            v = this.LocalToGlobal(new Vector2(rect.xMax, rect.yMax));
            ret.xMax = v.x;
            ret.yMax = v.y;
            return ret;
        }

        public Rect GlobalToLocal(Rect rect)
        {
            Rect ret = new Rect();
            Vector2 v = this.GlobalToLocal(new Vector2(rect.xMin, rect.yMin));
            ret.xMin = v.x;
            ret.yMin = v.y;
            v = this.GlobalToLocal(new Vector2(rect.xMax, rect.yMax));
            ret.xMax = v.x;
            ret.yMax = v.y;
            return ret;
        }
        public Vector2 LocalToRoot(Vector2 pt, GRoot r)
        {
            pt = displayObject.LocalToGlobal(pt);
            if (r == null || r == appContext.groot)//GRoot.inst
            {
                //fast
                pt.x /= GRoot.contentScaleFactor;
                pt.y /= GRoot.contentScaleFactor;
            }
            else
                return r.GlobalToLocal(pt);

            return pt;
        }

        public bool focusable
        {
            get { return _focusable; }
            set { _focusable = value; }
        }
        public bool focused
        {
            get
            {
                return this.root.focus == this;
            }
        }
        public void RequestFocus()
        {
            GObject p = this;
            while (p != null && !p._focusable)
                p = p.parent;
            if (p != null)
                this.root.focus = p;
        }
        public string tooltips
        {
            get { return _tooltips; }
            set
            {
                if (!string.IsNullOrEmpty(_tooltips))
                {
                    this.onRollOver.Remove(__rollOver);
                    this.onRollOut.Remove(__rollOut);
                }

                _tooltips = value;
                if (!string.IsNullOrEmpty(_tooltips))
                {
                    this.onRollOver.Add(__rollOver);
                    this.onRollOut.Add(__rollOut);
                }
            }
        }
        private void __rollOver()
        {
            this.root.ShowTooltips(tooltips);
        }

        private void __rollOut()
        {
            this.root.HideTooltips();
        }
        virtual public GComponent asCom
        {
            get
            {
                throw new Exception("I'm not component");
            }
        }
		
        virtual public GButton asButton
        {
            get { throw new Exception("I'm not button");  }
        }
		
        virtual public GProgressBar asProgress
        {
            get { throw new Exception("I'm not progress"); }            
        }

        virtual public GSlider asSlider
        {
            get { throw new Exception("I'm not slider"); }
        }

        virtual public GComboBox asComboBox
        {
            get { throw new Exception("I'm not combobox"); }
        }
		
        virtual public GTextField asTextField
        {
            get { throw new Exception("I'm not textfield"); }            
        }

        virtual public GLoader asLoader
        {
            get { throw new Exception("I'm not loader"); }
        }
		
        virtual public GList asList
        {
            get { throw new Exception("I'm not list"); }
        }
		
        virtual public GGraph asGraph
        {
            get { throw new Exception("I'm not graph"); }
        }
		
        virtual public GGroup asGroup
        {
            get { throw new Exception("I'm not group"); }
        }

        public void AddClickListener(Function listener)
		{
            AddEventListener(MouseEvent.CLICK, listener);
		}

        public void RemoveClickListener(Function listener)
        {
            RemoveEventListener(MouseEvent.CLICK, listener);
        }

        public  void AddEventListener(string strType, Function listener, bool useCapture = false)
        {
            base.AddEventListenerObsolete(strType, listener, useCapture);
            if (_displayObject!=null)
                _displayObject.AddEventListenerObsolete(strType, __reDispatch, useCapture);
        }

        public  void RemoveEventListener(string strType, Function listener, bool useCapture = false)
        {
            base.RemoveEventListenerObsolete(strType, listener, useCapture);
            if (_displayObject!=null && !this.HasEventListener(strType, useCapture))
                _displayObject.RemoveEventListenerObsolete(strType, __reDispatch, useCapture);
        }

        public void RemoveEventListeners(string strType = null, bool useCapture = false)
        {
            base.RemoveEventListenersObsolete(strType, useCapture);
            if (_displayObject != null)
                _displayObject.RemoveEventListenerObsolete(strType, __reDispatch, useCapture);
        }

        private void __reDispatch(object obj)
        {
            DispatchEventObsolete((EventContext)obj);
        }

        virtual public string text
        {
            get { return null; }
            set { }
        }

        public bool draggable
        {
            get { return _draggable; }
            set
            {
                if (_draggable != value)
                {
                    _draggable = value;
                    if (_draggable)
                    {
                        if (_dragInitiator == null)
                            _dragInitiator = new DragInitiator(this);
                        else
                            _dragInitiator.enabled = true;
                    }
                    else
                    {
                        if (_dragInitiator != null)
                            _dragInitiator.enabled = false;
                    }
                }
            }
        }

        public Rect? dragBounds
        {
            get { return _dragBounds; }
            set
            {
                _dragBounds = value;
            }
        }

        public void StartDrag(Rect? bounds=null)
        {
            if (_displayObject.stage==null)
                return;

            if (_dragHelper == null)
                _dragHelper = new DragHelper(this);
            _dragHelper.startDrag(bounds);
        }

        public void StopDrag()
        {
            if (_dragHelper != null)
                _dragHelper.stopDrag();
        }

		virtual protected void CreateDisplayObject()
		{
			
		}
		
		virtual protected void HandlePositionChanged()
		{
            if (_displayObject!=null)
            {
                _displayObject.x = _x * GRoot.contentScaleFactor;
                _displayObject.y = _y * GRoot.contentScaleFactor;
            }
		}
		
		virtual protected void HandleSizeChanged()
		{
		}
        virtual protected void HandleGrayedChanged()
        {
            if (_displayObject != null)
                _displayObject.SetGrayed(_grayed);
        }
        public String resourceURL
		{
            get
            {
			    if(_packageItem!=null)
				    return "ui://"+_packageItem.owner.Id+_packageItem.id;
			    else
				    return null;
            }
		}

        virtual public void ConstructFromResource(PackageItem pkgItem)
		{
			_packageItem = pkgItem;
		}

        virtual public void Setup_BeforeAdd(XML xml)
		{
            string str;
			
            _id = (string)xml.GetAttribute("id");
            _name = (string)xml.GetAttribute("name");
			
            
            str = (string)xml.GetAttribute("alpha");
            if(str!=null)
                this.alpha = float.Parse(str);
			
            str = (string)xml.GetAttribute("rotation");
            if (str != null)
                this.rotation = int.Parse(str);
                 //this.rotation = (float)(Math.PI * int.Parse(str) / 180);

            str = (string)xml.GetAttribute("xy");
            string[] arr;
            if (str!=null)
            {
                arr = str.Split( new char[]{','} );
                this.SetXY(int.Parse(arr[0]), int.Parse(arr[1]));
            }            			
            str = (string)xml.GetAttribute("size");
            if (str != null)
            {
                arr = str.Split(new char[] { ',' });
                _initWidth = int.Parse(arr[0]);
                _initHeight = int.Parse(arr[1]);
                SetSize(_initWidth, _initHeight);
            }
            this.touchable = (string)xml.GetAttribute("touchable") != "false";
            this.visible = (string)xml.GetAttribute("visible") != "false";
            this.grayed = xml.GetAttributeBool("grayed", false);
		}

        virtual public void Setup_AfterAdd(XML xml)
        {
            XML cxml=null;

            string s = (string)xml.GetAttribute("group");
            if (s != null)
                _group = _parent.GetChildById(s) as GGroup;

            cxml = xml.GetNode("gearDisplay");
            if (cxml != null)
                _gearDisplay.Setup(cxml);

            cxml = xml.GetNode("gearXY");
            if (cxml != null)
                _gearXY.Setup(cxml);

            cxml = xml.GetNode("gearSize");
            if (cxml != null)
                _gearSize.Setup(cxml);
            cxml = xml.GetNode("gearLook");
            if (cxml != null)
                gearLook.Setup(cxml);

            _relations.Setup(xml);
		}
    }

    class DragInitiator
    {
        private GObject _owner;
        private Vector2 _startPoint;
        private bool _enabled;

        public DragInitiator(GObject obj)
        {
            _owner = obj;
            _owner.displayObject.AddEventListenerObsolete(MouseEvent.MOUSE_DOWN, __mouseDown);

            _startPoint = new Vector2( 0.0f, 0.0f);
            _enabled = true;
        }

        public bool enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public void dispose()
        {
            _owner.displayObject.RemoveEventListenerObsolete(MouseEvent.MOUSE_DOWN, __mouseDown);
            _owner = null;
        }

        private void reset()
        {
            _owner.appContext.timers.Remove(__startDrag);
            _owner.appContext.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_UP, __mouseUp);
            _owner.appContext.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_MOVE, __mouseMove);
        }

        private void __mouseDown(object obj)
        {
            if (!_enabled)
                return;

            MouseEvent evt = (MouseEvent)obj;
            _startPoint.x = evt.StageX;
            _startPoint.y = evt.StageY;

            _owner.appContext.timers.Add(0.150f, 1, __startDrag); //avoid simple click
            _owner.appContext.stage.AddEventListenerObsolete(MouseEvent.MOUSE_UP, __mouseUp);
            _owner.appContext.stage.AddEventListenerObsolete(MouseEvent.MOUSE_MOVE, __mouseMove);
        }

        private void __mouseUp(object obj)
        {
            reset();
        }

        private void __mouseMove(object obj)
        {
            MouseEvent evt = (MouseEvent)obj;
            if (Math.Abs(evt.StageX - _startPoint.x) > 1 || Math.Abs(evt.StageY - _startPoint.y) > 1)
            {
                __startDrag(null);
            }
        }

        private void __startDrag(object param)
        {
            reset();

            DragEvent evt = new DragEvent(DragEvent.DRAG_START);
            _owner.DispatchEventObsolete(evt);
            if (!evt.isDefaultPrevented)
                _owner.StartDrag(_owner.dragBounds);
        }
    }

    class DragHelper
    {
        private GObject _owner;

        private static GObject sDragging;
        private static Vector2 sGlobalDragStart = new Vector2(0,0);
        private static Vector2 sLocalDragStart = new Vector2(0,0);
        private static Rect sDragBounds = new Rect();

        public DragHelper(GObject obj)
        {
            _owner = obj;
        }

        public void dispose()
        {
            _owner = null;
        }

        public void startDrag(Rect? bounds = null)
        {
            if (sDragging != null)
                sDragging.StopDrag();

            if (bounds != null)
                sDragBounds = (Rect) bounds;
            else
                sDragBounds.Set(0,0,0,0);

            sGlobalDragStart.x = _owner.displayObject.stage.mouseX / GRoot.contentScaleFactor;
            sGlobalDragStart.y = _owner.displayObject.stage.mouseY / GRoot.contentScaleFactor;
            sLocalDragStart.x = _owner.x;
            sLocalDragStart.y = _owner.y;

            sDragging = _owner;
            _owner.appContext.stage.AddEventListenerObsolete(MouseEvent.MOUSE_MOVE, __mouseMove);
            _owner.appContext.stage.AddEventListenerObsolete(MouseEvent.MOUSE_UP, __mouseUp);
        }

        public void stopDrag()
        {
            if (sDragging!=null)
            {
                _owner.appContext.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_UP, __mouseUp);
                _owner.appContext.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_MOVE, __mouseMove);
                sDragging = null;
            }
        }

        private void __mouseUp(object obj)
        {
            if (sDragging != null)
            {
                stopDrag();
                _owner.DispatchEventObsolete(new DragEvent(DragEvent.DRAG_END));
            }
        }

        private void __mouseMove(object obj)
        {
            MouseEvent evt = (MouseEvent)obj;
            float xx = evt.StageX/GRoot.contentScaleFactor;
		    float yy = evt.StageY/GRoot.contentScaleFactor;

            if (! ToolSet.IsEmptyRect( ref sDragBounds))
            {
                if (xx < sDragBounds.x || yy < sDragBounds.y
                    || xx + _owner.width > sDragBounds.xMax
                    || yy + _owner.height > sDragBounds.yMax)
                    return;
            }

            _owner.SetXY((int)(sLocalDragStart.x + xx - sGlobalDragStart.x),
                (int)(sLocalDragStart.y + yy - sGlobalDragStart.y));
        }
    }
}
