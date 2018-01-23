using System;
using System.Collections.Generic;
 

namespace KGUI
{
    public class GProgressBar : GComponent
    {
        private int _max;
		private int _value;
		private string _titleType;
        private bool _reverse;
		
		private GTextField _titleObject;
		private GAniObject _aniObject;
		private GObject _barObjectH;
		private GObject _barObjectV;
		private float _barMaxWidth;
		private float _barMaxHeight;
		
		public const string TITLE_PERCENT = "percent";
		public const string TITLE_VALUE_AND_MAX = "valueAndmax";
		public const string TITLE_VALUE_ONLY = "value";
		public const string  TITLE_MAX_ONLY = "max";

        public GProgressBar()
		{
			_titleType = TITLE_PERCENT;
			_value = 50;
			_max = 100;
		}

        override public GProgressBar asProgress
        {
            get { return this; }
        }
		
		public string titleType
		{

            get{
                return _titleType;
            }
			set{
                _titleType = value;
            }
		}

		public int max
		{
            get{
               return _max;
            }
			set{
                if(_max != value)
			    {
				    _max = value;
				    Update();
			    }
            }
		}		

		public int value
		{
            get{
                return _value;
            }			
            set{
                if(_value != value)
	    		{
    				_value = value;
				    Update();
			    }
            }
		}


        public void Update()
        {
            float percent = Math.Min((float)_value / _max, 1);
            if (_titleObject != null)
            {
                switch (_titleType)
                {
                    case TITLE_PERCENT:
                       _titleObject.text = (int)(percent*100)+"%";
                        break;

                    case TITLE_VALUE_AND_MAX:
                       _titleObject.text = _value + "/" + max;
                        break;

                    case TITLE_VALUE_ONLY:
                        _titleObject.text = ""+_value;
                        break;

                    case TITLE_MAX_ONLY:
                        _titleObject.text = ""+_max;
                        break;
                }
            }

            if(!_reverse)
			{
				if(_barObjectH!= null)
					_barObjectH.width = (int)(_barMaxWidth*percent);
				if(_barObjectV!= null)
					_barObjectV.height = (int)(_barMaxHeight*percent);
			}
			else
			{
				if(_barObjectH!= null)
				{
					int prevWidth = (int)(_barObjectH.width);
					_barObjectH.width = (int)(_barMaxWidth*percent);
					_barObjectH.x += (prevWidth-_barObjectH.width);
					
				}
				if(_barObjectV!= null)
				{
					int prevHeight = (int)(_barObjectH.height);
					_barObjectV.height = (int)(_barMaxHeight*percent);
					_barObjectH.y += (prevHeight-_barObjectH.height);
				}
			}
            //if(_aniObject !=null)
            //    _aniObject.frame = int(percent*100);
        }
				
		override public void ConstructFromXML(XML cxml)
		{
			base.ConstructFromXML(cxml);

            XML xml = cxml.GetNode("ProgressBar");

            string str;
            str = xml.GetAttribute("titleType");
            if(str!=null)
                _titleType = str;
            else
                _titleType = TITLE_PERCENT;
            str = (string)xml.GetAttribute("reverse");
            _reverse = str == "true";

			_titleObject = GetChildByName("title") as GTextField;
			_barObjectH = GetChildByName("bar");
			_barObjectV = GetChildByName("bar_v");
			_aniObject = GetChildByName("ani") as GAniObject;
			if(_barObjectH != null)
				_barMaxWidth = _barObjectH.width;
			if(_barObjectV != null)
				_barMaxHeight = _barObjectV.height;
			Update();
		}
		
        override public void Setup_AfterAdd(XML cxml)
		{
			base.Setup_AfterAdd(cxml);

            XMLList nodes = cxml.GetNode("Button").Elements();
            if (nodes == null)
                return;

            XML xml = nodes[0];

            _value = int.Parse((string)xml.GetAttribute("value"));
            _max = int.Parse((string)xml.GetAttribute("max"));
            Update();
		}
    }
}
