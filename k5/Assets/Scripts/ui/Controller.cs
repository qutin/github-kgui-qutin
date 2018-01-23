using System;
using System.Collections.Generic;
 

namespace KGUI
{
    public class Controller : EventDispatcher
    {
        private string _name;
		private int _selectedIndex;
		private int _previousIndex;
		private List<string> _pageIds;
		private List<string> _pageNames;
		private int _nextPageId;
		
		internal GComponent _parent;
		
		public Controller()
		{
			_pageIds = new List<string>();
			_pageNames = new List<string>();
			_selectedIndex = -1;
			_previousIndex = -1;
		}
		
		public string name
		{
            get{
                return _name;
            }

			set{
                _name = value;
            }
		}
		
		public GComponent parent
		{
            get{
                return _parent;
            }			
		}

		public int selectedIndex
		{
            get{
                return _selectedIndex;
            }			
            set{
                if(_selectedIndex!=value)
			    {
				    _previousIndex = _selectedIndex;
				    _selectedIndex = value;
				    if(_parent != null)
					    _parent.ApplyController(this);

                    this.DispatchEventObsolete(new StateChangeEvent(StateChangeEvent.CHANGED));
			    }
            }
		}				
		
		//功能和设置selectedIndex一样，但不会触发事件
		public void SetSelectedIndex(int value)
		{
			if(_selectedIndex!=value)
			{
				_previousIndex = _selectedIndex;
				_selectedIndex = value;
				if(_parent != null)
					_parent.ApplyController(this);
			}
		}
		
		public int previsousIndex
		{

            get{
                return _previousIndex;
            }
			
		}

		public string selectedPage
		{
            get{
                if(_selectedIndex==-1)
				    return null;
			    else
				    return _pageNames[_selectedIndex];
            }			
            set{
                int i = _pageNames.IndexOf(value);
			    this.selectedIndex = i;
            }

		}				
		
		public string previousPage
		{
            get{
                if(_previousIndex==-1)
				    return null;
			    else
				    return _pageNames[_previousIndex];
            }			
		}
		
		public int pageCount
		{
            get{
                return _pageIds.Count;
            }			
		}
		
		public string GetPageName(int index)
		{
			return _pageNames[index];
		}
		
		public void AddPage(string name="")
		{
            AddPageAt(name, _pageIds.Count);
		}
		
		public void AddPageAt(string name, int index)
		{
			string nid = ""+(_nextPageId++);
			if(index==_pageIds.Count)
			{
				_pageIds.Add(nid);
				_pageNames.Add(name);
			}
			else
			{
                _pageIds.Insert(index, nid);
                _pageNames.Insert(index, name);
			}
			if(_selectedIndex==-1)
				this.selectedIndex = 0;
		}
		
		public void RemovePage(string name)
		{
			int i = _pageNames.IndexOf(name);
			if(i!=-1)
			{
                _pageIds.RemoveAt(i);
				_pageNames.RemoveAt(i);
				if(_selectedIndex>=_pageIds.Count)
					this.selectedIndex = _selectedIndex-1;
				else
					_parent.ApplyController(this);
			}
		}
		
		public void RemovePageAt(int index)
		{
            _pageIds.RemoveAt(index);
            _pageNames.RemoveAt(index);
			if(_selectedIndex>=_pageIds.Count)
				this.selectedIndex = _selectedIndex-1;
			else
				_parent.ApplyController(this);
		}
		
		public void ClearPages()
		{
			_pageIds.Clear();
			_pageNames.Clear();
			if(_selectedIndex!=-1)
				this.selectedIndex = -1;
			else
				_parent.ApplyController(this);
		}
        public bool HasPage(string aName)
        {
            return _pageNames.IndexOf(aName) != -1;
        }
        internal int GetPageIndexById(string aId)
		{
			return _pageIds.IndexOf(aId);
		}
		
		public string GetPageIdByName(string aName)
		{
			return _pageIds[_pageNames.IndexOf(aName)];
		}
		
		internal string GetPageNameById(string aId)
		{
			return _pageNames[_pageIds.IndexOf(aId)];
		}

        internal string GetPageId(int index)
		{
			return _pageIds[index];
		}

        internal string selectedPageId
		{

            get{
                if(_selectedIndex==-1)
				    return null;
			    else
				    return _pageIds[_selectedIndex];
            }
			set{
                int i = _pageIds.IndexOf(value);
			    this.selectedIndex = i;
            }
		}
        internal string oppositePageId
        {
            set
            {
                int i = _pageIds.IndexOf(value);
                if (i > 0)
                    this.selectedIndex = 0;
                else if (_pageIds.Count > 1)
                    this.selectedIndex = 1;
            }
        }

        public void Setup(XML xml)
		{
            _name = (string)xml.GetAttribute("name");
            int i;
            int k;
            string str = (string)xml.GetAttribute("pages");
            if(str!=null)
            {
                string[] arr = str.Split(new char[]{','});
                int cnt = arr.Length;
                for(i=0;i<cnt;i+=2)
                {
                    string nid = arr[i];
                    k = int.Parse(nid);
                    if(k>=_nextPageId)
                        _nextPageId = k+1;
                    _pageIds.Add(nid);
                    _pageNames.Add(arr[i+1]);
                }
                str = (string)xml.GetAttribute("selected");
                if (str == null)
                    _selectedIndex = 0;
                else
                 _selectedIndex = int.Parse(str);
                if(_selectedIndex>=_pageIds.Count)
                    _selectedIndex = -1;
            }
            else
                _selectedIndex = -1;
            if(_parent!=null&&_selectedIndex>=0)
                _selectedIndex = 0;
		}
    }
}
