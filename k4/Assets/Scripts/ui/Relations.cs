using System;
using System.Collections.Generic;
 

namespace KGUI
{
    public class Relations
    {
        private GObject _owner;
		private List<RelationItem> _items;

		internal bool handling;

        static string[] RELATION_NAMES = new string[] {
			"left-left",//0
			"left-center",
			"left-right",
			"center-center",
			"right-left",
			"right-center",
			"right-right",
			"top-top",//7
			"top-middle",
			"top-bottom",
			"middle-middle",
			"bottom-top",
			"bottom-middle",
			"bottom-bottom",
			"width-width",//14
			"height-height",//15
			"leftext-left",//16
			"leftext-right",
			"rightext-left",
			"rightext-right",
			"topext-top",//20
			"topext-bottom",
			"bottomext-top",
			"bottomext-bottom"//23
		};
        //private static string[] SIDE_NAMES = {"left","center","right","width",
        //        "top","middle","bottom","height"};

        static char[] jointChar0 = new char[] { ',' };

		public Relations(GObject owner)
		{
			_owner = owner;
			_items = new List<RelationItem>();
		}

        public void Add(GObject target, RelationType relationType, bool usePercent=false)
		{
            foreach(RelationItem item in _items)
            {
                if(item.target==target)
                {
                    item.Add(relationType, usePercent);
                    return;
                }
            }
            RelationItem newItem = new RelationItem(_owner);
            newItem.target = target;
            newItem.Add(relationType, usePercent);
            _items.Add(newItem);
		}

        public void AddItems(GObject target, string sidePairs)
        {
            string[] arr = sidePairs.Split(jointChar0);
            string s;
            bool usePercent;
            int tid;

            int cnt = arr.Length;
            for (int i = 0; i < cnt; i++)
            {
                s = arr[i];
                if (string.IsNullOrEmpty(s))
                    continue;

                if (s[s.Length - 1] == '%')
                {
                    s = s.Substring(0, s.Length - 1);
                    usePercent = true;
                }
                else
                    usePercent = false;
                int j = s.IndexOf('-');
                if (j == -1)
                    s = s + "-" + s;

                tid = Array.IndexOf(RELATION_NAMES, s);
                if (tid == -1)
                    throw new ArgumentException("invalid relation type: " + s);

                RelationType relationType = (RelationType)tid;
                Add(target, relationType, usePercent);
            }
        }

        public void Remove(GObject target, RelationType relationType)
		{
            int cnt = _items.Count;
            int i = 0;
            while (i < cnt)
            {
                RelationItem item = _items[i];
                if (item.target == target)
                {
                    item.Remove(relationType);
                    if (item.isEmpty)
                    {
                        item.Dispose();
                        _items.RemoveAt(i);
                        cnt--;
                    }
                }
                else
                    i++;
            }
		}

		public bool Contains(GObject target)
		{
            foreach (RelationItem item in _items)
			{
                if (item.target == target)
					return true;
			}
			return false;
		}

        public void ClearFor(GObject target)
        {
            int cnt = _items.Count;
            int i = 0;
            while (i < cnt)
            {
                RelationItem item = _items[i];
                if (item.target == target)
                {
                    item.Dispose();
                    _items.RemoveAt(i);
                    cnt--;
                }
                else
                    i++;
            }
        }

        public void ClearAll()
        {
            foreach (RelationItem item in _items)
            {
                item.Dispose();
            }
            _items.Clear();
        }
		
        public void CopyFrom(Relations source)
        {
            ClearAll();

            List<RelationItem> arr = source._items;
            foreach (RelationItem ri in arr)
            {
                RelationItem item = new RelationItem(_owner);
                item.CopyFrom(ri);
                _items.Add(item);
            }
        }

        public void Dispose()
        {
            ClearAll();
        }
      
		public void Apply()
		{
            foreach (RelationItem item in _items)
			{
				item.Apply();
			}
		}

		public void ApplyOnSelfResized()
		{
            foreach (RelationItem item in _items)
			{
                item.ApplyOnSelfResized();
			}
		}
		
		public void Refresh()
		{
            foreach (RelationItem item in _items)
			{
                item.Refresh();
			}
		}
		
		public bool empty
		{
            get{
                return _items.Count==0;
            }
		}
		
		public void Setup(XML xml)
		{

            XMLList col = xml.Elements("relation");
            if (col == null)
                return;
            string targetId;
            GObject target;
            foreach (XML cxml in col)
            {
                targetId = (string)cxml.GetAttribute("target");
                if (_owner.parent!=null)
                {
                    if (targetId != null && targetId != "")
                        target = _owner.parent.GetChildById(targetId);
                    else
                        target = _owner.parent;
                }
                else
                {
                    //call from component construction
                    target = ((GComponent)_owner).GetChildById(targetId);
                }
                if (target!=null)
                    AddItems(target, (string)cxml.GetAttribute("sidePair"));
            }
		}
    }
}
