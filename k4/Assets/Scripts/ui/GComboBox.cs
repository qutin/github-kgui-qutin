using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using UnityEngine;

namespace KGUI
{
    public class GComboBox : GComponent
    {
		private int _visibleItemCount;
		private string[] _items;
        protected string[] _values;
		private bool _itemsUpdated;
		private int _selectedIndex;
		private Controller _buttonController;
		protected GTextField _titleObject;
		protected GComponent _dropdownObject;
		protected GList _list;
		private bool _down;

        //
        bool _over;
        public EventListener onChanged { get; private set; }
		
		public GComboBox()
		{
			_visibleItemCount = 10;
			_itemsUpdated = true;
            _selectedIndex = -1;
            _items = new string[0];
            _values = new string[0];

            //
            onChanged = new EventListener(this, "onChanged");
		}

        override public GComboBox asComboBox
        {
            get { return this; }
        }

        override public string text
        {
            get
            {
                if (_titleObject != null)
                    return _titleObject.text;
                else
                    return null;
            }
            set
            {
                if (_titleObject != null)
                    _titleObject.text = value;
            }
        }

        public Color titleColor
        {
            get
            {
                if (_titleObject != null)
                    return _titleObject.color;
                else
                    return Color.black;
            }
            set
            {
                if (_titleObject != null)
                    _titleObject.color = value;
            }
        }
		
		public int visibleItemCount
		{
			get{
                return _visibleItemCount;
            }			
            set{
                _visibleItemCount = value;
            }
        }
		
		public string[] items
		{
			get{
                return _items;
            }			
            set{
                _items = value;
                _itemsUpdated = true;
            }
		}
        public string[] values
        {
            get
            {
                return _values;
            }
            set
            {
                if (value == null)
                    _values = new string[0];
                else
                    _values = (string[])value.Clone();
            }
        }

        public int selectedIndex
		{
			get{
                return _selectedIndex;
            }			
            set{
                if(_items==null || _selectedIndex==value)
				    return;
			
			    _selectedIndex = value;
			    if(selectedIndex>=0 && selectedIndex<_items.Length)
				    this.text = (string)_items[_selectedIndex];
            }
		}

        public string value
        {
            get
            {
                if (_selectedIndex >= 0 && _selectedIndex < _values.Length)
                    return _values[_selectedIndex];
                else
                    return null;
            }
            set
            {
                this.selectedIndex = Array.IndexOf(_values, value);
            }
        }
		protected void SetState(string value)
		{
			if(_buttonController!=null)
				_buttonController.selectedPage = value;
		}
		
		override public void ConstructFromXML(XML cxml)
		{
			base.ConstructFromXML(cxml);

            XML xml = cxml.GetNode("ComboBox");
			
			string str;
			
			_buttonController = GetController("button");
			_titleObject = GetChildByName("title") as GTextField;

			str = (string) xml.GetAttribute("dropdown");
            if (string.IsNullOrEmpty(str))
            {
                Debug.Log(this.resourceURL+": 需要定义下拉框资源");
                return;
            }
			
			_dropdownObject = UIPackage.CreateObjectFromURL(str) as GComponent;
            if (_dropdownObject == null)
            {
                Debug.Log(this.resourceURL + ": 下拉框必须为元件");
                return;
            }

            _list = _dropdownObject.GetChildByName("list") as GList;
            if (_list == null)
            {
                Debug.Log(this.resourceURL + ": 下拉框的弹出元件里必须包含名为list的列表");
                return;
            }
			_list.AddEventListener(ItemEvent.CLICK, __clickItemObsolete);
            _list.onClickItem.Add(__clickItem);

            _list.AddRelation(_dropdownObject, RelationType.Width);
            _dropdownObject.AddRelation(_list, RelationType.Height);
			
			_displayObject.AddEventListenerObsolete(MouseEvent.ROLL_OVER, __rolloverObsolete);
			_displayObject.AddEventListenerObsolete(MouseEvent.ROLL_OUT, __rolloutObsolete);
			_displayObject.AddEventListenerObsolete(MouseEvent.MOUSE_DOWN, __mousedown);

            displayObject.onRollOver.Add(__rollover);
            displayObject.onRollOut.Add(__rollout);
            displayObject.onTouchBegin.Add(__touchBegin);
		}

		override public void Setup_AfterAdd(XML cxml)
		{
			base.Setup_AfterAdd(cxml);

            XML xml = cxml.GetNode("ComboBox");
            if (xml == null)
                return;

			string str;
            str = (string)xml.GetAttribute("titleColor");
            if (str != null)
                this.titleColor = ToolSet.ConvertFromHtmlColor(str);
            str = (string)xml.GetAttribute("visibleItemCount");
			if(str!=null)
				_visibleItemCount = int.Parse(str);
            XMLList col = xml.Elements("item");
            _items = new string[col.Count];
            _values = new string[col.Count];
            int i = 0;
            foreach (XML ix in col)
            {
                _items[i] = ix.GetAttribute("title");
                _values[i] = ix.GetAttribute("value");
                i++;
            }
            this.text = (string)xml.GetAttribute("title");
            if (str != null && str.Length > 0)
            {
                _selectedIndex = Array.IndexOf(_items, this.text);
            }
            else if (_items.Length > 0)
            {
                _selectedIndex = 0;
            }
            else
                _selectedIndex = -1;

		}
		
		override protected void HandleSizeChanged()
		{
			base.HandleSizeChanged();
			
			if(_dropdownObject!=null)
				_dropdownObject.width = this.width;
		}
		
		protected void ShowDropdown()
		{
			if(_itemsUpdated)
			{
				_itemsUpdated = false;

                _list.RemoveChildrenToPool();
                if (_items != null)
                {
                    int cnt = _items.Length;
                    for (int i = 0; i < cnt; i++)
                        ((GComponent)_list.AddItemFromPool()).GetChildByName("title").asTextField.text = _items[i];
                    _list.ResizeToFit(_visibleItemCount);
                }
			}
			_list.selectedIndex = -1;
            _dropdownObject.width = this.width;

            //GRoot.inst.ShowPopup(_dropdownObject, this, true);
            this.root.TogglePopup(_dropdownObject, this, true);
			if(_dropdownObject.parent!=null)
			{
				_dropdownObject.displayObject.AddEventListenerObsolete(EventContext.REMOVED_FROM_STAGE, __popupWinClosedObsolete);
                _dropdownObject.displayObject.onRemovedFromStage.Add(__popupWinClosed);
				SetState("down");
			}
		}
        virtual protected void RenderDropdownList()
        {
            _list.RemoveChildrenToPool();
            int cnt = _items.Length;
            for (int i = 0; i < cnt; i++)
            {
                GObject item = _list.AddItemFromPool();
                item.text = _items[i];
                item.name = i < _values.Length ? _values[i] : string.Empty;
            }
        }
		private void __popupWinClosedObsolete(object obj)
		{
            _dropdownObject.displayObject.RemoveEventListenerObsolete(EventContext.REMOVED_FROM_STAGE, __popupWinClosedObsolete);
			SetState("up");
		}
        private void __popupWinClosed(object obj)
        {
            _dropdownObject.displayObject.onRemovedFromStage.Remove(__popupWinClosed);
            if (_over)
                SetState(GButton.OVER);
            else
                SetState(GButton.UP);
        }
		
		private void __clickItemObsolete(object obj)
		{
            ItemEvent evt = (ItemEvent)obj;
            //GRoot.inst.HidePopup();
            appContext.groot.HidePopup();
            _selectedIndex = evt.Index;
            if (_selectedIndex >= 0)
                this.text = _items[_selectedIndex];
            else
                this.text = string.Empty;
			DispatchEventObsolete(new StateChangeEvent(StateChangeEvent.CHANGED));
		}
        private void __clickItem(EventContext context)
        {
            if (_dropdownObject.parent is GRoot)
                ((GRoot)_dropdownObject.parent).HidePopup(_dropdownObject);
            _selectedIndex = _list.GetChildIndex((GObject)context.data);
            if (_selectedIndex >= 0)
                this.text = _items[_selectedIndex];
            else
                this.text = string.Empty;

            onChanged.Call();
        }
		private void __rolloverObsolete(object obj)
		{
			if(_down || _dropdownObject.parent!=null)
				return;
			
			SetState("over");
		}
		
		private void __rolloutObsolete(object obj)
		{
            if (_down || _dropdownObject.parent != null)
				return;
			
			SetState("up");
		}
        private void __rollover()
        {
            _over = true;
            if (_down || _dropdownObject != null && _dropdownObject.parent != null)
                return;

            SetState(GButton.OVER);
        }

        private void __rollout()
        {
            _over = false;
            if (_down || _dropdownObject != null && _dropdownObject.parent != null)
                return;

            SetState(GButton.UP);
        }

        private void __mousedown(object obj)
		{
			_down = true;
			appContext.stage.AddEventListenerObsolete(MouseEvent.MOUSE_UP, __mouseup);
			
            //if(_dropdownObject!=GRoot.inst.LastClosedPopup)
            if (_dropdownObject != appContext.groot.LastClosedPopup)
				ShowDropdown();
		}

        private void __mouseup(object obj)
		{
			if(_down)
			{
				appContext.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_UP, __mouseup);
				_down = false;
				
				if(_dropdownObject.parent!=null)
					SetState("up");
			}
		}
        private void __touchBegin()
        {
            _down = true;

            appContext.stage.onTouchEnd.Add(__touchEnd);

            if (_dropdownObject != null)
                ShowDropdown();
        }

        private void __touchEnd(EventContext context)
        {
            if (_down)
            {
                appContext.stage.onTouchEnd.Remove(__touchEnd);
                _down = false;

                if (_dropdownObject != null && _dropdownObject.parent != null)
                {
                    if (_over)
                        SetState(GButton.OVER);
                    else
                        SetState(GButton.UP);
                }
            }
        }
    }
}
