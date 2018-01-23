using System;
using System.Collections;
using System.Collections.Generic;
 
using DG.Tweening;

namespace KGUI
{
    // 变为抽象类了，但会导致slua导出现问题
    public abstract class GearBase 
    {
        protected PageOptionSet _pageSet;
		protected string _tween;
        public Ease easeType;
        protected float _tweenTime;

        protected GObject _owner;
        protected Controller _controller;
		
		protected IDictionary _storage;
		protected object _default;

        protected static char[] jointChar0 = new char[] { ',' };
        protected static char[] jointChar1 = new char[] { '|' };
		
		public GearBase(GObject owner)
		{
			_owner = owner;
            _storage = new Dictionary<string, object>();
            _pageSet = new PageOptionSet();
            easeType = Ease.OutQuad;
			_tween = null;
            _tweenTime = UIConfig.defaultGearTweenTime;
		}
		
		public Controller controller
		{

            get{
                return _controller;
            }

            set{
                if( value !=_controller)
			    {
				    _controller = value;
                    _pageSet.controller = value;
                    _pageSet.Clear();
                    _storage = new Dictionary<string, object>();
                    if(_controller != null)
                        UpdateState();
			    }  
            }
			
		}
		
		public PageOptionSet pageSet
		{
            get { return _pageSet; }
		}
		
		public string tween
		{
            get{
                return _tween;
            }

            set{
                _tween = value;
            }
		}

        public float tweenTime
        {

            get
            {
                return _tweenTime;
            }

            set
            {
                _tweenTime = value;
            }

        }
		
		virtual protected object ReadValue( string value)
		{
			return null;
		}

        public void Setup(XML xml)
		{
            string str;
            _controller = _owner.parent.GetController( xml.GetAttribute("controller"));
            if (_controller == null)
                return;

            Init();
            
            string[] pages = xml.GetAttributeArray("pages");
            if (pages != null)
            {
                foreach (string s in pages)
                    _pageSet.AddById(s);
            }

            str = xml.GetAttribute("tween");
            if (str != null)
            {
                _tween = str;
                if (_tween == "ease")
                {
                    _tween = UIConfig.defaultGearTweenFunction;
                    easeType = FieldTypes.ParseEaseType(str);
                }
                    
            }
            else
                _tween = null;

            str = xml.GetAttribute("duration");
            if (str != null)
                tweenTime = float.Parse(str);

            str = xml.GetAttribute("values");
            string[] values = null;
            if(str!=null)
                values = str.Split(jointChar1);
			
            int i;

            _storage = new Dictionary<string, object>();
            if (values != null)
            {
                for (i = 0; i < values.Length; i++)
                {
                    str = values[i];
                    if (str != "-")
                    {
                        _storage[pages[i]] = ReadValue(str);
                        AddStatus(pages[i], str);
                    }
                }
            }
            str = (string)xml.GetAttribute("default");
            if (str != null)
            {
                _default = ReadValue(str);
                AddStatus(null, str);
            }
            else
                _default = null;
		}

		virtual protected Boolean connected
		{
            get
            {
                if (_controller!=null && !_pageSet.empty)
                    return _pageSet.ContainsId(_controller.selectedPageId);
                else
				    return false;
            }
			
		}
        abstract protected void AddStatus(string pageId, string value);
        abstract protected void Init();
        //virtual public void Apply()
        //{
        //}
        abstract public void Apply();
        abstract public void UpdateState();

    }
}
