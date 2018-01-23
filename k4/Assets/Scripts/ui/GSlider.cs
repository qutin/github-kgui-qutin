using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class GSlider : GComponent
    {
        private int _max;
		private int _value;
		private string _titleType;
		
		private GTextField _titleObject;
		private GAniObject _aniObject;
		private GObject _barObjectH;
		private GObject _barObjectV;
		private float _barMaxWidth;
		private float _barMaxHeight;
		private GObject _gripObject;
		private Vector2 _clickPos;
		private float _clickPercent;
		
		public const string TITLE_PERCENT = "percent";
		public const string TITLE_VALUE_AND_MAX = "valueAndmax";
		public const string TITLE_VALUE_ONLY = "value";
		public const string TITLE_MAX_ONLY = "max";
		
		public GSlider()
		{
			_titleType = TITLE_PERCENT;
			_value = 50;
			_max = 100;
			_clickPos = Vector2.zero;
		}

        override public GSlider asSlider
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
		
		private void Update()
		{
			float percent = Math.Min((float)_value/_max,1);
			UpdateWidthPercent(percent);
		}
		
		private void UpdateWidthPercent(float percent)
		{
			if(_titleObject!=null)
			{
				switch(_titleType)
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
			
			if(_barObjectH!=null)
				_barObjectH.width = (int)(_barMaxWidth*percent);
			if(_barObjectV!=null)
				_barObjectV.height = (int)(_barMaxHeight*percent);
			//if(_aniObject!=null)
				//_aniObject.frame = (int)(percent*100);
		}
		
		override public void ConstructFromXML(XML cxml)
		{
			base.ConstructFromXML(cxml);

            XML xml = cxml.GetNode("Slider").Elements()[0];
			
			string str;
            str = (string)xml.GetAttribute("titleType");
            if(str!=null)
                _titleType = str;
            else
                _titleType = TITLE_PERCENT;
			
			_titleObject = GetChildByName("title") as GTextField;
			_barObjectH = GetChildByName("bar");
			_barObjectV = GetChildByName("bar_v");
			_aniObject = GetChildByName("ani") as GAniObject;
			_gripObject = GetChildByName("grip");
			if(_barObjectH!=null)
				_barMaxWidth = _barObjectH.width;
			if(_barObjectV!=null)
				_barMaxHeight = _barObjectV.height;
			if(_gripObject!=null)
				_gripObject.displayObject.AddEventListenerObsolete(MouseEvent.MOUSE_DOWN, __gripMouseDown);
		}
		
		 override public void Setup_AfterAdd(XML cxml)
		{
			base.Setup_AfterAdd(cxml);

            XMLList nodes = cxml.GetNode("Slider").Elements();
            if (nodes == null)
                return;

            XML xml = nodes[0];
			 _value = int.Parse((string)xml.GetAttribute("value"));
            _max = int.Parse((string)xml.GetAttribute("max"));
            Update();
		}
		
		private void __gripMouseDown(object obj)
		{
            MouseEvent evt = (MouseEvent)obj;
			_clickPos.x = evt.StageX;
			_clickPos.y = evt.StageY;
			_clickPercent = (float)_value/_max;
			_gripObject.displayObject.stage.AddEventListenerObsolete(MouseEvent.MOUSE_MOVE, __gripMouseMove);
			_gripObject.displayObject.stage.AddEventListenerObsolete(MouseEvent.MOUSE_UP, __gripMouseUp);
		}
		
		private void __gripMouseMove(object obj)
		{
            MouseEvent evt = (MouseEvent)obj;
			float deltaX = evt.StageX-_clickPos.x;
            float deltaY = evt.StageY - _clickPos.y;
			
			float percent;
			if(_barObjectH!=null)
				percent = _clickPercent + deltaX/_barMaxWidth;
			else
				percent = _clickPercent + deltaY/_barMaxHeight;
			if(percent>1)
				percent = 1;
			else if(percent<0)
				percent = 0;

			int newValue = (int)Math.Round(percent*_max);
			if(newValue!=_value)
			{
				_value = newValue;
				DispatchEventObsolete(new StateChangeEvent(StateChangeEvent.CHANGED));
			}
			UpdateWidthPercent(percent);
		}
		
		private void __gripMouseUp(object obj)
		{
            MouseEvent evt = (MouseEvent)obj;
			float percent = (float)_value/_max;
			UpdateWidthPercent(percent);
			_gripObject.displayObject.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_MOVE, __gripMouseMove);
			_gripObject.displayObject.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_UP, __gripMouseUp);
		}
    }
}
