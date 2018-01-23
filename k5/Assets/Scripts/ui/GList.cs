using System;
using System.Collections;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public delegate void ListItemRenderer(int index, GObject item);
    public class GList : GComponent
    {
		private string _layoutObsolete;
        private int _lineGap;
        private int _columnGap;
        private string _defaultItem;

        private IDictionary _pool;
        private Controller _selection;
		
		public const string SINGLE_COLUMN = "column";
		public const string SINGLE_ROW = "row";
        public const string FLOW_HZ = "flow_hz";
		public const string FLOW_VT = "flow_vt";

        //
        bool _selectionHandled;
        int _lastSelectedIndex;
        ListLayoutType _layout;
        public ListSelectionMode selectionMode;
        //Virtual List support
        bool _virtual;
        bool _loop;
        int _numItems;
        int _firstIndex; //the top left index
        int _viewCount; //item count in view
        int _curLineItemCount; //item count in one line
        Vector2 _itemSize;
        ScrollPane _virtualScrollPane;
        public EventListener onClickItem { get; private set; }
        public ListItemRenderer itemRenderer;

		public GList()
            :base()
		{
			_pool = new Dictionary<string, object>();
            _trackBounds = true;
			_layoutObsolete = SINGLE_COLUMN;
			_selection = new Controller();
            _selection.AddEventListenerObsolete(StateChangeEvent.CHANGED, __stateChanged);
			
			AddController(_selection);
            //
            onClickItem = new EventListener(this, "onClickItem");
		}

        public override void Dispose()
		{
			foreach(ArrayList arr in _pool)
			{
				int cnt = arr.Count;
				for(int i=0;i<cnt;i++)
					((GObject)arr[i]).Dispose();
			}
			base.Dispose();
		}

        public ListLayoutType layout
        {
            get { return _layout; }
            set
            {
                if (_layout != value)
                {
                    _layout = value;
                    SetBoundsChangedFlag();
                }
            }
        }

		public string defaultItem
		{
            get { return _defaultItem; }
            set { _defaultItem = value;  }
		}

		public int lineGap
		{
			get{return _lineGap;}
            set
            {
                if(_lineGap != value)
			    {
				    _lineGap = value;
				    SetBoundsChangedFlag();
			    }
            }
		}

		public int columnGap
		{
			get{ return _columnGap;}
            set
            {
                if(_columnGap != value)
			    {
				    _columnGap = value;
				    SetBoundsChangedFlag();
			    }
            }
		}

        public GObject GetFromPool(string url)
		{
            if (url==null)
                url = _defaultItem;

            ArrayList arr = (ArrayList)_pool[url];
            if (arr != null && arr.Count > 0)
            {
                GObject tempGobject = (GObject)arr[arr.Count-1];
                arr.RemoveAt(arr.Count-1);
                return tempGobject;
            }

            GObject child = UIPackage.CreateObjectFromURL(url);
			if(child==null)
				throw new Exception(url + " not exists");

			return child;
		}
		
		private void ReturnToPool(GObject obj)
		{
            string url = obj.resourceURL;
			ArrayList arr = (ArrayList)_pool[url];
			if(arr==null)
			{
				arr = new ArrayList();
				_pool[url] = arr;
			}
			arr.Add(obj);
		}

		public GObject AddItemFromPool(string url=null)
		{
            return AddChild(GetFromPool(url));
		}
		
		override public GObject AddChildAt(GObject obj, int index)
		{
            if (_layoutObsolete == SINGLE_COLUMN)
                obj.SetSize(this.viewWidth, obj.initHeight);
            else if (_layoutObsolete == SINGLE_ROW)
                obj.SetSize(obj.initWidth, this.viewHeight);
            else
                obj.SetSize(obj.initWidth, obj.initHeight);

			base.AddChildAt(obj, index);
            if (obj is GButton)
			{
				GButton button = (GButton)(obj);
				button.relatedController = _selection;
				_selection.AddPageAt("", index);
				button.pageOption.index = index;
				
				button.AddClickListener(__clickItemObsolete);
                //
                button.onTouchBegin.Add(__itemTouchBegin);
                button.onClick.Add(__clickItem);
			}
			return obj;
		}
        override public GObject RemoveChildAt(int index, bool dispose = false)
		{
			GObject child =base.RemoveChildAt(index, dispose);
            if (child is GButton)
			{
                GButton button = (GButton)(child);
				_selection.RemovePageAt(button.pageOption.index);
				button.relatedController = null;
				
				button.RemoveClickListener(__clickItemObsolete);
			}
            //
            child.onTouchBegin.Remove(__itemTouchBegin);
            child.onClick.Remove(__clickItem);

			ReturnToPool(child);

            return child;
		}			
        public void RemoveChildToPoolAt(int index)
        {
            GObject child = base.RemoveChildAt(index);
            ReturnToPool(child);
        }
        public void RemoveChildToPool(GObject child)
        {
            base.RemoveChild(child);
            ReturnToPool(child);
        }
        public void RemoveChildrenToPool()
        {
            RemoveChildrenToPool(0, -1);
        }
        public void RemoveChildrenToPool(int beginIndex, int endIndex)
        {
            if (endIndex < 0 || endIndex >= _children.Count)
                endIndex = _children.Count - 1;

            for (int i = beginIndex; i <= endIndex; ++i)
                RemoveChildToPoolAt(beginIndex);
        }
		public int itemCount
		{
			get{return numChildren;}
		}
		
		public int selectedIndex
		{
			get{ return _selection.selectedIndex; }
            set{ _selection.SetSelectedIndex(value); }
		}
		
        public void ResizeToFit(int itemCount, int minSize=0)
		{
            EnsureBoundsCorrect();
			
			int curCount = this.numChildren;
			if(itemCount>curCount)
				itemCount = curCount;

            if (_virtual)
            {
                int lineCount = Mathf.CeilToInt((float)itemCount / _curLineItemCount);
                if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
                    this.viewHeight = lineCount * _itemSize.y + Math.Max(0, lineCount - 1) * _lineGap;
                else
                    this.viewWidth = lineCount * _itemSize.x + Math.Max(0, lineCount - 1) * _columnGap;
            }
            else if (itemCount == 0)
            {
                if (_layoutObsolete == SINGLE_COLUMN || _layoutObsolete == FLOW_HZ)
                    this.viewHeight = minSize;
                else
                    this.viewWidth = minSize;
            }
            else
            {
                int i = itemCount - 1;
                GObject obj = null;
                while (i >= 0)
                {
                    obj = this.GetChildAt(i);
                    if (obj.visible)
                        break;
                    i--;
                }
                if (i < 0)
                {
                    if (_layoutObsolete == SINGLE_COLUMN || _layoutObsolete == FLOW_HZ)
                        this.viewHeight = minSize;
                    else
                        this.viewWidth = minSize;
                }
                else
                {
                    float size;
                    if (_layoutObsolete == SINGLE_COLUMN || _layoutObsolete == FLOW_HZ)
                    {
                        size = obj.y + obj.height;
                        if (size < minSize)
                            size = minSize;
                        this.viewHeight = size;
                    }
                    else
                    {
                        size = obj.x + obj.width;
                        if (size < minSize)
                            size = minSize;
                        this.viewWidth = size;
                    }
                }
            }
		}

		override public GList asList
		{
			get{return this;}
		}
		
        override protected void HandleSizeChanged()
		{
			base.HandleSizeChanged();

            AdjustItemSize();
			
			if(_layoutObsolete==FLOW_VT || _layoutObsolete==FLOW_HZ)
				SetBoundsChangedFlag();
		}

        private void AdjustItemSize()
		{
			if(_layoutObsolete==SINGLE_COLUMN)
			{
				int cnt = numChildren;
                float cw = this.viewWidth;
				for(int i=0;i<cnt;i++)
				{
					GObject child = GetChildAt(i);
					child.width = cw;
				}
			}
			else if(_layoutObsolete==SINGLE_ROW)
			{
				int cnt = numChildren;
                float ch = this.viewHeight;
				for(int i=0;i<cnt;i++)
				{
                    GObject child = GetChildAt(i);
					child.height = ch;
				}
			}
		}
        override protected void UpdateBounds()
		{
			int cnt = numChildren;
			int i;
			GObject child;
			float curX = 0;
			float curY = 0;
			float cw, ch;
            float maxWidth = 0;
            float maxHeight = 0;
			if(_layoutObsolete==SINGLE_COLUMN)
			{
				for(i=0;i<cnt;i++)
				{
					child = GetChildAt(i);
                    if (!child.visible)
                        continue;

                    if (curY != 0)
						curY += _lineGap;
					child.SetXY(curX, curY);
					curY += child.height;
					if(child.width>maxWidth)
						maxWidth = child.width;
				}
				cw = curX+maxWidth;
				ch = curY;
			}
			else if(_layoutObsolete==SINGLE_ROW)
			{
				for(i=0;i<cnt;i++)
				{
					child = GetChildAt(i);
                    if (!child.visible)
                        continue;

					if(curX!=0)
						curX += _columnGap;
					child.SetXY(curX, curY);
					curX += child.width;
					if(child.height>maxHeight)
						maxHeight = child.height;
				}
				cw = curX;
				ch = curY+maxHeight;
			}
            else if(_layoutObsolete==FLOW_HZ)
			{
                cw = this.viewWidth;
				for(i=0;i<cnt;i++)
				{
					child = GetChildAt(i);
                    if (!child.visible)
                        continue;

					if(curX!=0)
						curX += _columnGap;
					
					if(curX+child.width>cw && maxHeight!=0)
					{
						//new line
						curX = 0;
						curY += maxHeight + _lineGap;
						maxHeight = 0;
					}
					child.SetXY(curX, curY);
					curX += child.width;
					if(child.height>maxHeight)
						maxHeight = child.height;
				}
				ch = curY+maxHeight;
			}
			else
			{
                ch = this.viewHeight;
				for(i=0;i<cnt;i++)
				{
					child = GetChildAt(i);
                    if (!child.visible)
                        continue;

					if(curY!=0)
						curY += _lineGap;
					
					if(curY+child.height>ch && maxWidth!=0)
					{
						curY = 0;
						curX += maxWidth + _columnGap;
						maxWidth = 0;
					}
					child.SetXY(curX, curY);
					curY += child.height;
					if(child.width>maxWidth)
						maxWidth = child.width;
				}
				cw = curX+maxWidth;
			}
			SetBounds(0,0,cw,ch);
		}
		
		private void __clickItemObsolete(object evt)
		{
			this.DispatchEventObsolete(new ItemEvent(ItemEvent.CLICK, false, _selection.selectedIndex));
		}
        void __itemTouchBegin(EventContext context)
        {
            GButton item = context.sender as GButton;
            if (item == null || selectionMode == ListSelectionMode.None)
                return;

            _selectionHandled = false;

            if (UIConfig.defaultScrollTouchEffect
                && (this.scrollPane != null || this.parent != null && this.parent.scrollPane != null))
                return;

            if (selectionMode == ListSelectionMode.Single)
            {
                SetSelectionOnEvent(item, (InputEvent)context.data);
            }
            else
            {
                if (!item.selected)
                    SetSelectionOnEvent(item, (InputEvent)context.data);
                //如果item.selected，这里不处理selection，因为可能用户在拖动
            }
        }
        void __clickItem(EventContext context)
        {
            GObject item = context.sender as GObject;
            if (!_selectionHandled)
                SetSelectionOnEvent(item, (InputEvent)context.data);
            _selectionHandled = false;

            if (scrollPane != null)
                scrollPane.ScrollToView(item, true);

            onClickItem.Call(item);
        }
        void SetSelectionOnEvent(GObject item, InputEvent evt)
        {
            if (!(item is GButton) || selectionMode == ListSelectionMode.None)
                return;

            _selectionHandled = true;
            bool dontChangeLastIndex = false;
            GButton button = (GButton)item;
            int index = GetChildIndex(item);

            if (selectionMode == ListSelectionMode.Single)
            {
                if (!button.selected)
                {
                    ClearSelectionExcept(button);
                    button.selected = true;
                }
            }
            else
            {
                if (evt.shift)
                {
                    if (!button.selected)
                    {
                        if (_lastSelectedIndex != -1)
                        {
                            int min = Math.Min(_lastSelectedIndex, index);
                            int max = Math.Max(_lastSelectedIndex, index);
                            max = Math.Min(max, _children.Count - 1);
                            for (int i = min; i <= max; i++)
                            {
                                GButton obj = GetChildAt(i).asButton;
                                if (obj != null && !obj.selected)
                                    obj.selected = true;
                            }

                            dontChangeLastIndex = true;
                        }
                        else
                        {
                            button.selected = true;
                        }
                    }
                }
                else if (evt.ctrl || selectionMode == ListSelectionMode.Multiple_SingleClick)
                {
                    button.selected = !button.selected;
                }
                else
                {
                    if (!button.selected)
                    {
                        ClearSelectionExcept(button);
                        button.selected = true;
                    }
                    else
                        ClearSelectionExcept(button);
                }
            }

            if (!dontChangeLastIndex)
                _lastSelectedIndex = index;
        }
        void ClearSelectionExcept(GObject obj)
        {
            int cnt = _children.Count;
            for (int i = 0; i < cnt; i++)
            {
                GButton button = _children[i].asButton;
                if (button != null && button != obj && button.selected)
                    button.selected = false;
            }
        }
		private void __stateChanged(object evt)
		{
            this.DispatchEventObsolete(new StateChangeEvent(StateChangeEvent.CHANGED));
		}

        public void ScrollToView(int index)
        {
            ScrollToView(index, false);
        }
        public void ScrollToView(int index, bool ani)
        {
            if (_virtual)
            {
                if (this.scrollPane != null)
                    scrollPane.ScrollToView(GetItemRect(index), ani);
                else if (parent != null && parent.scrollPane != null)
                    parent.scrollPane.ScrollToView(GetItemRect(index), ani);
            }
            else
            {
                GObject obj = GetChildAt(index);
                if (this.scrollPane != null)
                    scrollPane.ScrollToView(obj, ani);
                else if (parent != null && parent.scrollPane != null)
                    parent.scrollPane.ScrollToView(obj, ani);
            }
        }
        public override int GetFirstChildInView()
        {
            int ret = base.GetFirstChildInView();
            if (ret != -1)
            {
                ret += _firstIndex;
                if (_loop && ret >= _numItems)
                    ret -= _numItems;
                return ret;
            }
            else
                return -1;
        }
        public void SetVirtual()
        {
            SetVirtual(false);
        }
        public void SetVirtualAndLoop()
        {
            SetVirtual(true);
        }

        void SetVirtual(bool loop)
        {
            //if (!_virtual)
            //{
            //    if (loop)
            //    {
            //        if (this.scrollPane == null)
            //            throw new Exception("Loop list must be scrollable!");

            //        if (_layout == ListLayoutType.FlowHorizontal || _layout == ListLayoutType.FlowVertical)
            //            throw new Exception("Only single row or single column layout type is supported for loop list!");

            //        this.scrollPane.bouncebackEffect = false;
            //    }

            //    _virtual = true;
            //    _loop = loop;
            //    RemoveChildrenToPool();

            //    GObject obj = GetFromPool(null);
            //    _itemSize = obj.size;
            //    ReturnToPool(obj);

            //    if (this.scrollPane != null)
            //    {
            //        if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
            //            this.scrollPane.scrollSpeed = _itemSize.y;
            //        else
            //            this.scrollPane.scrollSpeed = _itemSize.x;

            //        _virtualScrollPane = this.scrollPane;
            //    }
            //    else if (parent != null && parent.scrollPane != null)
            //    {
            //        _virtualScrollPane = parent.scrollPane;
            //        parent.onSizeChanged.Add(RefreshVirtualList);
            //    }
            //    else
            //        throw new Exception("Virtual list must be scrollable or in scrollable container!");

            //    _virtualScrollPane.onScroll.AddCapture(__scrolled);
            //    HandleVirtualListSizeChanged();
            //}
        }
        public int numItems
        {
            get
            {
                if (_virtual)
                    return _numItems;
                else
                    return _children.Count;
            }
            set
            {
                if (_virtual)
                {
                    _numItems = value;
                    RefreshVirtualList();
                }
                else
                {
                    int cnt = _children.Count;
                    if (value > cnt)
                    {
                        for (int i = cnt; i < value; i++)
                            AddItemFromPool();
                    }
                    else
                    {
                        RemoveChildrenToPool(value, cnt);
                    }

                    if (itemRenderer != null)
                    {
                        for (int i = 0; i < value; i++)
                            itemRenderer(i, GetChildAt(i));
                    }
                }
            }
        }
        void HandleVirtualListSizeChanged()
        {
            //float vw = _virtualScrollPane.viewWidth;
            //float vh = _virtualScrollPane.viewHeight;
            //if (_virtualScrollPane != this.scrollPane) //scroll support from parent
            //{
            //    vw = Mathf.Min(vw, this.ViewWidth);
            //    vh = Mathf.Min(vh, this.ViewHeight);
            //}

            //if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
            //{
            //    if (_layout == ListLayoutType.SingleColumn)
            //        _curLineItemCount = 1;
            //    else if (_lineItemCount != 0)
            //        _curLineItemCount = _lineItemCount;
            //    else
            //        _curLineItemCount = Mathf.FloorToInt((vw + _columnGap) / (_itemSize.x + _columnGap));
            //    _viewCount = (Mathf.CeilToInt((vh + _lineGap) / (_itemSize.y + _lineGap)) + 1) * _curLineItemCount;
            //    int numChildren = _children.Count;
            //    if (numChildren < _viewCount)
            //    {
            //        for (int i = numChildren; i < _viewCount; i++)
            //            this.AddItemFromPool();
            //    }
            //    else if (numChildren > _viewCount)
            //        this.RemoveChildrenToPool(_viewCount, numChildren);
            //}
            //else
            //{
            //    if (_layout == ListLayoutType.SingleRow)
            //        _curLineItemCount = 1;
            //    else if (_lineItemCount != 0)
            //        _curLineItemCount = _lineItemCount;
            //    else
            //        _curLineItemCount = Mathf.FloorToInt((vh + _lineGap) / (_itemSize.y + _lineGap));
            //    _viewCount = (Mathf.CeilToInt((vw + _columnGap) / (_itemSize.x + _columnGap)) + 1) * _curLineItemCount;
            //    int numChildren = _children.Count;
            //    if (numChildren < _viewCount)
            //    {
            //        for (int i = numChildren; i < _viewCount; i++)
            //            this.AddItemFromPool();
            //    }
            //    else if (numChildren > _viewCount)
            //        this.RemoveChildrenToPool(_viewCount, numChildren);
            //}

            //RefreshVirtualList();
        }

        void RefreshVirtualList()
        {
            //EnsureBoundsCorrect();

            //if (_layout == ListLayoutType.SingleColumn || _layout == ListLayoutType.FlowHorizontal)
            //{
            //    if (this.scrollPane != null)
            //    {
            //        float ch;
            //        if (_layout == ListLayoutType.SingleColumn)
            //        {
            //            ch = _numItems * _itemSize.y + Math.Max(0, _numItems - 1) * _lineGap;
            //            if (_loop && ch > 0)
            //                ch = ch * 2 + _lineGap;
            //        }
            //        else
            //        {
            //            int lineCount = Mathf.CeilToInt((float)_numItems / _curLineItemCount);
            //            ch = lineCount * _itemSize.y + Math.Max(0, lineCount - 1) * _lineGap;
            //        }

            //        this.scrollPane.SetContentSize(this.scrollPane.contentWidth, ch);
            //    }

            //    __scrolled(null);
            //}
            //else
            //{
            //    if (this.scrollPane != null)
            //    {
            //        float cw;
            //        if (_layout == ListLayoutType.SingleRow)
            //        {
            //            cw = _numItems * _itemSize.x + Math.Max(0, _numItems - 1) * _columnGap;
            //            if (_loop && cw > 0)
            //                cw = cw * 2 + _columnGap;
            //        }
            //        else
            //        {
            //            int lineCount = Mathf.CeilToInt((float)_numItems / _curLineItemCount);
            //            cw = lineCount * _itemSize.x + Math.Max(0, lineCount - 1) * _columnGap;
            //        }

            //        this.scrollPane.SetContentSize(cw, this.scrollPane.contentHeight);
            //    }

            //    __scrolled(null);
            //}
        }

        void RenderItems(int beginIndex, int endIndex)
        {
            for (int i = 0; i < _viewCount; i++)
            {
                GObject obj = GetChildAt(i);
                int j = _firstIndex + i;
                if (_loop && j >= _numItems)
                    j -= _numItems;

                if (j < _numItems)
                {
                    obj.visible = true;
                    if (i >= beginIndex && i < endIndex)
                        itemRenderer(j, obj);
                }
                else
                    obj.visible = false;
            }
        }
        Rect GetItemRect(int index)
        {
            Rect rect = new Rect();
            int index1 = index / _curLineItemCount;
            int index2 = index % _curLineItemCount;
            switch (_layout)
            {
                case ListLayoutType.SingleColumn:
                    rect = new Rect(0, index1 * _itemSize.y + Math.Max(0, index1 - 1) * _lineGap,
                        this.viewWidth, _itemSize.y);
                    break;

                case ListLayoutType.FlowHorizontal:
                    rect = new Rect(index2 * _itemSize.x + Math.Max(0, index2 - 1) * _columnGap,
                        index1 * _itemSize.y + Math.Max(0, index1 - 1) * _lineGap,
                        _itemSize.x, _itemSize.y);
                    break;

                case ListLayoutType.SingleRow:
                    rect = new Rect(index1 * _itemSize.x + Math.Max(0, index1 - 1) * _columnGap, 0,
                        _itemSize.x, this.viewHeight);
                    break;

                case ListLayoutType.FlowVertical:
                    rect = new Rect(index1 * _itemSize.x + Math.Max(0, index1 - 1) * _columnGap,
                        index2 * _itemSize.y + Math.Max(0, index2 - 1) * _lineGap,
                        _itemSize.x, _itemSize.y);
                    break;
            }
            return rect;
        }
        override public void Setup_BeforeAdd(XML xml)
		{
            base.Setup_BeforeAdd(xml);
			
			string str;
			str = (string)xml.GetAttribute("layout");
			if(str!=null)
				_layoutObsolete = str;
			else
				_layoutObsolete = SINGLE_COLUMN;
			
			str = (string)xml.GetAttribute("margin");
			_margin.Parse(str);

            OverflowType overflow;
            str = (string)xml.GetAttribute("overflow");
            if (str != null)
                overflow = FieldTypes.parseOverflowType(str);
            else
                overflow = OverflowType.Visible;

            ScrollType scroll;
            str = (string)xml.GetAttribute("scroll");
            if (str != null)
                scroll = FieldTypes.parseScrollType(str);
            else
                scroll = ScrollType.Vertical;

            str = (string)xml.GetAttribute("scrollSpeed");
            int scrollSpeed = 0;
            if(str!=null)
                scrollSpeed  = int.Parse(str);

            ScrollBarDisplayType scrollBarDisplay;
            str = (string)xml.GetAttribute("scrollBar");
            if (str != null)
                scrollBarDisplay = FieldTypes.parseScrollBarDisplayType(str);
            else
                scrollBarDisplay = ScrollBarDisplayType.Default;

			str = (string)xml.GetAttribute("lineGap");
			if(str!=null)
				_lineGap = int.Parse(str);
			else
				_lineGap = 0;
			
			str = (string)xml.GetAttribute("colGap");
			if(str!=null)
				_columnGap = int.Parse(str);
			else
				_columnGap = 0;

            str = (string)xml.GetAttribute("defaultItem");
            if (str != null)
                _defaultItem = str;

            SetupOverflowAndScroll(overflow, _margin, scroll, scrollSpeed, scrollBarDisplay);
			
			str = (string)xml.GetAttribute("items");
			if(str!=null)
			{
				string[] items = str.Split(new char[]{ ','});
				int cnt = items.Length;
				for(int i=0;i<cnt;i++)
				{
					string s = items[i];
					if(s!=null)
                        AddChild(GetFromPool(s));
				}
			}
		}
    }
}
