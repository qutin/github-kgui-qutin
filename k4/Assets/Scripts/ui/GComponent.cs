using System;
using System.Collections;
using System.Collections.Generic;
 
using UnityEngine;
using Sprite = KGUI.Sprite;

namespace KGUI
{
	public class GComponent : GObject
	{
		protected List<GObject> _children;
		protected List<Controller> _controllers;

        //protected uint _instNextId;
		internal bool _buildingDisplayList;

		protected bool _trackBounds;
		protected bool _boundsChanged;
		internal Margin _margin;
		internal Rect _bounds;

        public UISprite rootContainer{ get; private set; }
		public Sprite container{ get; private set; }
		internal ScrollPane _scrollPane;

        //
        public EventListener onDrop { get; private set; }

		public GObject this[string gName]
		{
			get { return this.GetChild( gName ); }
		}

		public GComponent()
		{
			_bounds = new Rect();
			_children = new List<GObject>();
			_controllers = new List<Controller>();
			_margin = new Margin();

            //
            onDrop = new EventListener(this, "onDrop");
		}

		override protected void CreateDisplayObject()
		{
			container = new Sprite();
			rootContainer = new UISprite(this);
		    rootContainer.gOwner = this;
			rootContainer.hitArea = new Rect();
			rootContainer.AddChild( container );

			_displayObject = rootContainer;
		}

		override public void Dispose()
		{
			int numChildren = _children.Count;
			for ( int i = numChildren - 1; i >= 0; --i )
				_children[i].Dispose();

			base.Dispose();
		}

		override public GComponent asCom
		{
			get
			{
				return this;
			}
		}

		public GObject AddChild( GObject child )
		{
			AddChildAt( child, _children.Count );
			return child;
		}

		virtual public GObject AddChildAt( GObject child, int index )
		{
            //if ( child._id == null || child._id == "" )
            //    child._id = "n" + ( _instNextId++ );
            //if ( child._name == null || child._name == "" )
            //    child._name = child._id;
			int numChildren = _children.Count;

			if ( index >= 0 && index <= numChildren )
			{
                if (child.parent == this)
                {
                    SetChildIndex(child, index);
                }
                else
                {
                    child.RemoveFromParent();
                    child.parent = this;

                    if (index == numChildren)
                        _children.Add(child);
                    else
                        _children.Insert(index, child);

                    ChildStateChanged(child);
                    SetBoundsChangedFlag();
                }
				return child;
			}
			else
			{
				throw new Exception( "Invalid child index" );

			}
		}

		public GObject RemoveChild( GObject child, bool dispose = false )
		{
			int childIndex = _children.IndexOf( child );
			if ( childIndex != -1 )
			{
				RemoveChildAt( childIndex, dispose );
			}
			return child;
		}

		virtual public GObject RemoveChildAt( int index, bool dispose = false )
		{
			if ( index >= 0 && index < numChildren )
			{
				GObject child = _children[index];
				child.parent = null;
				//_children.splice(index, 1);
				_children.RemoveAt( index );
				if ( child.displayObjectAdded )
					container.RemoveChild( child.displayObject );
				if ( dispose )
					child.Dispose();
				SetBoundsChangedFlag();
				return child;
			}
			else
				throw new Exception( "Invalid child index" );
		}

		public void RemoveChildren( int beginIndex = 0, int endIndex = -1, bool dispose = false )
		{
			if ( endIndex < 0 || endIndex >= numChildren )
				endIndex = numChildren - 1;

			for ( int i = beginIndex; i <= endIndex; ++i )
				RemoveChildAt( beginIndex, dispose );
		}

		public GObject GetChildAt( int index )
		{
			if ( index >= 0 && index < numChildren )
				return _children[index];
			else
				throw new Exception( "Invalid child index" );
		}

		public GObject GetChildByName( string name )
		{
			int numChildren = _children.Count;
			for ( int i = 0; i < numChildren; ++i )
			{
				if ( _children[i].name == name )
					return _children[i];
			}

			return null;
		}

		//short for getChildByName
		public GObject GetChild( string name )
		{
			int numChildren = _children.Count;
			for ( int i = 0; i < numChildren; ++i )
			{
				if ( _children[i].name == name )
					return _children[i];
			}

			return null;
		}

		internal GObject GetChildById( string id )
		{
			int numChildren = _children.Count;
			for ( int i = 0; i < numChildren; ++i )
			{
				if ( _children[i].id == id )
					return _children[i];
			}

			return null;
		}

		public int GetChildIndex( GObject child )
		{
			return _children.IndexOf( child );
		}

		public void SetChildIndex( GObject child, int index )
		{
            int oldIndex = _children.IndexOf(child);
            if (oldIndex == -1)
                throw new ArgumentException("Not a child of this container");

            if (oldIndex == index) 
                return;

            _children.RemoveAt(oldIndex);
            if (index >= _children.Count)
                _children.Add(child);
            else
                _children.Insert(index, child);

			if ( child.displayObjectAdded )
			{
				int displayIndex = 0;
				for ( int i = 0; i < index; i++ )
				{
					GObject g = _children[i];
					if ( g.displayObjectAdded )
						displayIndex++;
				}
				container.SetChildIndex( child.displayObject, displayIndex );
			}
		}

		public void SwapChildren( GObject child1, GObject child2 )
		{
			int index1 = _children.IndexOf( child1 );
			int index2 = _children.IndexOf( child2 );
			if ( index1 == -1 || index2 == -1 )
				throw new Exception( "Not a child of this container" );
			SwapChildrenAt( index1, index2 );
		}

		public void SwapChildrenAt( int index1, int index2 )
		{
			GObject child1 = _children[index1];
			GObject child2 = _children[index2];

			SetChildIndex( child1, index2 );
			SetChildIndex( child2, index1 );
		}

		public int numChildren
		{
			get
			{
				return _children.Count;
			}
		}

		public void AddController( Controller controller )
		{
			_controllers.Add( controller );
			controller._parent = this;
			ApplyController( controller );
		}

		public Controller GetController( string name )
		{
			int cnt = _controllers.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				Controller c = _controllers[i];
				if ( c.name == name )
					return c;
			}

			return null;
		}

		public void RemoveController( Controller c )
		{
			int index = _controllers.IndexOf( c );
			if ( index == -1 )
				throw new Exception( "controller not exists" );//throw new Error("controller not exists");

			c._parent = null;
			//_controllers.splice(index,1);
			_controllers.RemoveAt( index );

			foreach ( GObject child in _children )
				child.HandleControllerChanged( c );
		}

		public List<Controller> controllers
		{
			get
			{
				return _controllers;
			}
		}

		public void ChildStateChanged( GObject child )
		{
            if (_buildingDisplayList)
                return;

			if ( child is GGroup )
			{
				foreach ( GObject g in _children )
				{
					if ( g.group == child )
						ChildStateChanged( g );
				}
				return;
			}

			if ( child.displayObject == null )
				return;

			if ( child.finalVisible )
			{
				if ( child.displayObject.parent == null )
				{
					int index = 0;
					foreach ( GObject g in _children )
					{
						if ( g == child )
							break;

						if ( g.displayObject != null && g.displayObject.parent != null )
							index++;
					}
					container.AddChildAt( child.displayObject, index );
				}
			}
			else
			{
				if ( child.displayObject.parent != null )
					container.RemoveChild( child.displayObject );
			}
		}

		public void ApplyController( Controller c )
		{
			foreach ( GObject child in _children )
				child.HandleControllerChanged( c );
		}

		public void ApplyAllControllers()
		{
			int cnt = _controllers.Count;
			for ( int i = 0; i < cnt; ++i )
			{
				ApplyController( _controllers[i] );
			}
		}

		public ScrollPane scrollPane
		{
			get { return _scrollPane; }
		}

		public void SetupOverflowAndScroll( OverflowType overflow, Margin margin, ScrollType scroll, 
            int scrollSpeed, ScrollBarDisplayType scrollBarDisplay )
		{
			_margin = margin;
			if ( overflow == OverflowType.Hidden )
			{
                container.clipRect = new Rect(0, 0, _width * GRoot.contentScaleFactor, _height * GRoot.contentScaleFactor);
				if ( _scrollPane != null )
				{
					_scrollPane.Dispose();
					_scrollPane = null;
				}
                container.x = margin.left * GRoot.contentScaleFactor;
                container.y = margin.top * GRoot.contentScaleFactor;
			}
			else if ( overflow == OverflowType.Scroll )
			{
				container.clipRect = null;
				if ( _scrollPane == null )
					_scrollPane = new ScrollPane( this, scroll, margin, scrollSpeed, scrollBarDisplay );
			}
			else
			{
				container.clipRect = null;
				if ( _scrollPane != null )
				{
					_scrollPane.Dispose();
					_scrollPane = null;
				}
                container.x = margin.left * GRoot.contentScaleFactor;
                container.y = margin.top * GRoot.contentScaleFactor;
			}

			SetBoundsChangedFlag();
		}

		override protected void HandleSizeChanged()
		{
			if ( _scrollPane != null )
				_scrollPane.SetSize( _width, _height );
			else if ( container.clipRect != null )
			{
                //_container.clipRect.width = _width * GRoot.contentScaleFactor;
                //_container.clipRect.height = _height * GRoot.contentScaleFactor;
                container.clipRect = new Rect( ((Rect)container.clipRect).x, ((Rect)container.clipRect).y, _width * GRoot.contentScaleFactor, _height * GRoot.contentScaleFactor);
			}
			if ( rootContainer.hitArea != null )
                rootContainer.hitArea = new Rect(0, 0, this.width * GRoot.contentScaleFactor, this.height * GRoot.contentScaleFactor);
		}

		public void SetBoundsChangedFlag()
		{
			if ( _scrollPane == null && !_trackBounds )
				return;

            //if ( !_boundsChanged )
            //{
            //    _boundsChanged = true;

            //    if ( _displayObject.stage != null )
            //    {
            //        appContext.stage.AddEventListenerObsolete( EventContext.RENDER, __render );
            //        appContext.stage.Invalidate();
            //    }
            //    else
            //    {
            //        _displayObject.AddEventListenerObsolete( EventContext.ADDED_TO_STAGE, __addedToStage );
            //    }
            //}

            _boundsChanged = true;
		}
        public void EnsureBoundsCorrect()
        {
            if (_boundsChanged)
                UpdateBounds();
        }
        virtual protected void UpdateBounds()
		{
			float ax = int.MaxValue, ay = int.MaxValue;
			float ar = int.MinValue, ab = int.MinValue;
			float tmp;
			foreach ( GObject child in _children )
			{
				tmp = child.x;
				if ( tmp < ax )
					ax = tmp;
				tmp = child.y;
				if ( tmp < ay )
					ay = tmp;
				tmp = child.x + child.width;
				if ( tmp > ar )
					ar = tmp;
				tmp = child.y + child.height;
				if ( tmp > ab )
					ab = tmp;
			}
			float aw = ar - ax;
			float ah = ab - ay;
			if ( !Mathf.Approximately(ax, _bounds.x) || !Mathf.Approximately(ay, _bounds.y)|| !Mathf.Approximately(aw, _bounds.width) || !Mathf.Approximately(ah, _bounds.height) )
				SetBounds( ax, ay, aw, ah );
		}

		public void SetBounds( float ax, float ay, float aw, float ah )
		{
			_boundsChanged = false;
            _bounds.Set(ax,ay, aw, ah);

			if ( _scrollPane != null )
				_scrollPane.SetContentSize( ( int )_bounds.width, ( int )_bounds.height );
		}

		public Rect bounds
		{
			get { return _bounds; }
		}

        public float viewWidth
        {
            get
            {
                if (_scrollPane != null)
                    return _scrollPane.viewWidth;
                else
                    return this.width - _margin.left - _margin.right;
            }

            set
            {
                if (_scrollPane != null)
                    _scrollPane.viewWidth = value;
                else
                    this.width = value + _margin.left + _margin.right;
            }
        }

        public float viewHeight
        {
            get
            {
                if (_scrollPane != null)
                    return _scrollPane.viewHeight;
                else
                    return this.height - _margin.top - _margin.bottom;
            }

            set
            {
                if (_scrollPane != null)
                    _scrollPane.viewHeight = value;
                else
                    this.height = value + _margin.top + _margin.bottom;
            }
        }
        public bool IsChildInView(GObject child)
        {
            if (scrollPane != null)
            {
                return scrollPane.IsChildInView(child);
            }
            else if (rootContainer.clipRect != null)
            {
                return child.x + child.width >= 0 && child.x <= this.width
                    && child.y + child.height >= 0 && child.y <= this.height;
            }
            else
                return true;
        }
        virtual public int GetFirstChildInView()
        {
            int cnt = _children.Count;
            for (int i = 0; i < cnt; ++i)
            {
                GObject child = _children[i];
                if (IsChildInView(child))
                    return i;
            }
            return -1;
        }
        

        //private void __addedToStage( object obj )
        //{
        //    _displayObject.RemoveEventListenerObsolete( EventContext.ADDED_TO_STAGE, __addedToStage );
        //    if ( _boundsChanged )
        //        UpdateBounds();
        //}

        //private void __render( object obj )
        //{
        //    appContext.stage.RemoveEventListenerObsolete( EventContext.RENDER, __render );
        //    if ( _boundsChanged )
        //        UpdateBounds();
        //}

		override public void ConstructFromResource( PackageItem pkgItem )
		{
			_packageItem = pkgItem;

			XML xml = _packageItem.owner.GetComponentData( _packageItem );
			ConstructFromXML( xml );
		}

		virtual public void ConstructFromXML( XML xml )
		{
			string str;
            string[] arr;

            underConstruct = true;

            arr = xml.GetAttributeArray("size");
            _sourceWidth = int.Parse(arr[0]);
            _sourceHeight = int.Parse(arr[1]);            
            _initWidth = _sourceWidth;
            _initHeight = _sourceHeight;

            SetSize(_sourceWidth, _sourceHeight);

            OverflowType overflow;
            str = (string)xml.GetAttribute("overflow");
            if (str != null)
                overflow = FieldTypes.parseOverflowType(str);
            else
                overflow = OverflowType.Visible;

            str = (string)xml.GetAttribute("margin");
            if (str != null)
                _margin.Parse(str);

            ScrollType scroll;
            str = (string)xml.GetAttribute("scroll");
            if (str != null)
                scroll = FieldTypes.parseScrollType(str);
            else
                scroll = ScrollType.Vertical;

            str = (string)xml.GetAttribute("scrollSpeed");
            int scrollSpeed = 0;
            if (str != null)
                scrollSpeed = int.Parse(str);

            ScrollBarDisplayType scrollBarDisplay;
            str = (string)xml.GetAttribute("scrollBar");
            if (str != null)
                scrollBarDisplay = FieldTypes.parseScrollBarDisplayType(str);
            else
                scrollBarDisplay = ScrollBarDisplayType.Default;

			
            
            //_instNextId = uint.Parse( ( string )xml.GetAttribute("idnum"));//旧格式定义
			

            _buildingDisplayList = true;

            XMLList col = xml.Elements("controller");
			Controller controller;
            if (col != null)
            {
                foreach (XML cxml in col)
                {
                    controller = new Controller();
                    _controllers.Add(controller);
                    controller._parent = this;
                    controller.Setup(cxml);
                }
            }
            XML listNode = xml.GetNode("displayList");
            col = listNode.Elements();
			GObject u;
			ArrayList tmp = new ArrayList();
			if ( col != null )
			{
				foreach ( XML cxml in col )
				{
					u = ConstructChild( cxml );
					if ( u == null )
						continue;

					u.Setup_BeforeAdd( cxml );
					AddChild( u );
					tmp.Add( u );
					tmp.Add( cxml );
				}
			}

			this.relations.Setup( xml );

			int cnt = tmp.Count;
			for ( int i = 0; i < cnt; i += 2 )
			{
				u = ( GObject )tmp[i];
				u.Setup_AfterAdd( ( XML )tmp[i + 1] );
			}

			ApplyAllControllers();

            _buildingDisplayList = false;
		    underConstruct = false;

            //build real display list
            foreach (GObject child in _children)
            {
                if (child.displayObject != null && child.finalVisible)
                    container.AddChild(child.displayObject);
            }

			SetupOverflowAndScroll( overflow, _margin, scroll, scrollSpeed, scrollBarDisplay );
		}

		private GObject ConstructChild( XML xml )
		{
			string pkgId = ( string )xml.GetAttribute("pkg");
			UIPackage thisPkg = _packageItem.owner;
			UIPackage pkg;
			if ( pkgId != null && pkgId != thisPkg.Id )
			{
				pkg = UIPackage.GetById( pkgId );
				if ( pkg == null )
					return null;
			}
			else
				pkg = thisPkg;

			string src = ( string )xml.GetAttribute("src");
			if ( src != null )
			{
				PackageItem pi = pkg.GetItem( src );
				if ( pi == null )
					return null;

				GObject g = pkg.CreateObject2( pi );
				return g;
			}
			else
			{
				string ename = ( string )xml.name;
				GObject g;
			    if (ename == "text" && (string) xml.GetAttribute("input") == "true")
			    {
                    g = new GTextInput();
                    g.OnCreate(appContext);
			    }
				else
					g = UIPackage.NewObject( ename , appContext);
				return g;
			}
		}
	}
}
