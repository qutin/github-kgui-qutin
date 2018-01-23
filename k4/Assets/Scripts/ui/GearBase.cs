using System;
using System.Collections;
using System.Collections.Generic;
 
using DG.Tweening;

namespace KGUI
{
    // 变为抽象类了，但会导致slua导出现问题
    public abstract class GearBase 
    {
        public static bool disableAllTweenEffect = false;

        protected PageOptionSet _pageSet;
		protected bool _tween;
        public Ease easeType;
        protected float _tweenTime;

        public float delay;

        protected GObject _owner;
        protected Controller _controller;
		
        protected static char[] jointChar0 = new char[] { ',' };
        protected static char[] jointChar1 = new char[] { '|' };
		
		public GearBase(GObject owner)
		{
			_owner = owner;
            _pageSet = new PageOptionSet();
            easeType = Ease.OutQuad;
            _tweenTime = UIConfig.defaultGearTweenTime;

		    delay = 0;
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
                    if(_controller != null)
                        UpdateState();
			    }  
            }
			
		}
		
		public PageOptionSet pageSet
		{
            get { return _pageSet; }
		}
		
		public bool tween
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
                tween = true;

            str = xml.GetAttribute("duration");
            if (str != null)
                tweenTime = float.Parse(str);

            str = xml.GetAttribute("delay");
            if (str != null)
                delay = float.Parse(str);

            str = xml.GetAttribute("values");
            string[] values = null;
            if(str!=null)
                values = str.Split(jointChar1);
			
            int i;

            if (values != null)
            {
                for (i = 0; i < values.Length; i++)
                {
                    str = values[i];
                    if (str != "-")
                    {
                        AddStatus(pages[i], str);
                    }
                }
            }
            str = (string)xml.GetAttribute("default");
            if (str != null)
            {
                AddStatus(null, str);
            }
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
