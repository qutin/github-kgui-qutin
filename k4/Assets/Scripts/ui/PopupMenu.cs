using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

namespace KGUI
{
    public class PopupMenu
    {
        protected GComponent _contentPane;
		protected GList _list;
		
		public PopupMenu(int width=100, string resourceURL=null)
		{
			if(resourceURL==null)
				resourceURL = UIConfig.popupMenu;

            _contentPane = UIPackage.CreateObjectFromURL(resourceURL).asCom;
            _contentPane.AddEventListener(EventContext.ADDED_TO_STAGE, __addedToStage);

            _contentPane.width = width;
            _list = _contentPane.GetChildByName("list").asList;
            _list.RemoveChildrenToPool();

            _list.AddRelation(_contentPane, RelationType.Width);
            _contentPane.AddRelation(_list, RelationType.Height);

			_list.AddEventListener(ItemEvent.CLICK, __clickItem);
		}

        public void AddItem(string caption, string name = null, Function callback = null)
		{
			GComponent item = _list.AddItemFromPool().asCom;
			item.GetChildByName("title").asTextField.text = caption;
			item.name = name;
			item.data = callback;
		}
		
		public void SetItemText(string name, string caption) 
		{
            GComponent item = _list.GetChildByName(name).asCom;
			item.GetChildByName("title").asTextField.text = caption;
		}

        public void SetItemVisible(string name, bool visible)
        {
            GComponent item = _list.GetChildByName(name).asCom;
            if (item.visible != visible)
            {
                item.visible = visible;
                _list.SetBoundsChangedFlag();
            }
        }
		
		public void RemoveItem(string name)
		{
            GComponent item = _list.GetChildByName(name).asCom;
			int index = _list.GetChildIndex(item);
            _list.RemoveChildAt(index);
		}
		
		public void ClearItems()
		{
            _list.RemoveChildrenToPool();
		}
		
		public GComponent contentPane
		{
			get { return _contentPane; }
		}
		
		public GList list
		{
			get {return _list;}
		}

		private void __clickItem(object obj)
		{
            ItemEvent evt = (ItemEvent)obj;
			GRoot.inst.HidePopup();
            GComponent item = _list.GetChildAt(evt.Index).asCom;
			if(item.data!=null)
				((Function)item.data)(obj);
		}

        private void __addedToStage(object obj)
        {
            _list.ResizeToFit(int.MaxValue, 10);
        }
    }
}
