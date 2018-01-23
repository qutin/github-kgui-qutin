using System;
using System.Collections;
using System.Collections.Generic;


namespace KGUI
{
    public class PageOptionSet
    {
        private Controller _controller;
		private ArrayList _items;
		
		public PageOptionSet()
		{
			_items = new ArrayList();
		}
		
		public Controller controller
		{
            set{ _controller = value;}			
		}
		
		public void Add(int pageIndex)
		{
			string id = _controller.GetPageId(pageIndex);
			int i = _items.IndexOf(id);
			if(i==-1)
				_items.Add(id);
		}
		
		public void Remove(int pageIndex)
		{
			string id = _controller.GetPageId(pageIndex);
			int i = _items.IndexOf(id);
			if(i!=-1)
				_items.RemoveAt(i);//_items.splice(i,1);
		}
		
		public void AddByName(string pageName)
		{
			string id = _controller.GetPageIdByName(pageName);
			int i = _items.IndexOf(id);
			if(i!=-1)
				_items.Add(id);
		}
		
		public void RemoveByName(string pageName)
		{
			string id = _controller.GetPageIdByName(pageName);
			int i = _items.IndexOf(id);
			if(i!=-1)
				_items.RemoveAt(i);
		}
		
		public void Clear()
		{
			_items.Clear();
		}
		
		public bool empty
		{
			get{ return _items.Count==0; }
		}
		
		internal void AddById(string id)
		{
			_items.Add(id);
		}

        internal bool ContainsId(string id)
		{
			return _items.IndexOf(id)!=-1;
		}
    }
}
