using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    class GScrollBar : GComponent
    {
        private GObject _grip;
		private GObject _arrowButton1;
		private GObject _arrowButton2;
		private GObject _bar;
		private ScrollPane _target;

        private bool _vertical;
		private float _scrollPerc;
		
		private bool _dragging;
		private Vector2 _dragOffset;
		
		public GScrollBar()
		{
			_dragOffset =Vector2.zero;
            _scrollPerc = 0;
		}

        public void SetScrollPane(ScrollPane target, bool vertical)
		{
            _target = target;
			_vertical = vertical;
		}

		public float displayPerc
		{
            set
            {
			    if(_vertical)
			    {
                    _grip.height = (int)(value * _bar.height);
				    _grip.y = (int)(_bar.y+(_bar.height-_grip.height)*_scrollPerc);
			    }
			    else
			    {
                    _grip.width = (int)(value * _bar.width);
				    _grip.x = (int)(_bar.x+(_bar.width-_grip.width)*_scrollPerc);
			    }
            }
		}

        public float scrollPerc
		{
            set
            {
			    _scrollPerc = value;
			    if(_vertical)
				    _grip.y = (int)(_bar.y+(_bar.height-_grip.height)*_scrollPerc);
			    else
				    _grip.x = (int)(_bar.x+(_bar.width-_grip.width)*_scrollPerc);
            }
		}
		
		override public void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);
			
			_grip = GetChildByName("grip");
            if (_grip == null)
            {
                Debug.Log(this.resourceURL + ": 需要定义grip");
                return;
            }
			
			_bar = GetChildByName("bar");
			if(_bar==null)
            {
				Debug.Log(this.resourceURL+": 需要定义bar");
                return;
            }
			
			_arrowButton1 = GetChildByName("arrow1");
			_arrowButton2 = GetChildByName("arrow2");
			
			_grip.displayObject.AddEventListenerObsolete(MouseEvent.MOUSE_DOWN, __gripMouseDown);
			if(_arrowButton1!=null)
				_arrowButton1.displayObject.AddEventListenerObsolete(MouseEvent.CLICK, __arrowButton1Click);
            if (_arrowButton2 != null)
				_arrowButton2.displayObject.AddEventListenerObsolete(MouseEvent.CLICK, __arrowButton2Click);
		}

		private void __gripMouseDown(object obj)
		{
			if(_bar==null)
				return;

            MouseEvent evt = (MouseEvent)obj;
            evt.StopPropagation();

			_dragOffset.x = evt.StageX-_grip.x;
			_dragOffset.y = evt.StageY-_grip.y;
			_dragging = true;
			_grip.displayObject.stage.AddEventListenerObsolete(MouseEvent.MOUSE_MOVE, __stageMouseMove);
			_grip.displayObject.stage.AddEventListenerObsolete(MouseEvent.MOUSE_UP, __stageMouseUp);
		}
		
		private void __stageMouseMove(object obj)
		{
            MouseEvent evt = (MouseEvent)obj;
			if(_vertical)
			{
				float curY = evt.StageY-_dragOffset.y;
				_target.percY = (float)( (curY-_bar.y)/(_bar.height-_grip.height));
			}
			else
			{
                float curX = evt.StageX - _dragOffset.x;
				_target.percX = (float)((curX-_bar.x)/(_bar.width-_grip.width));
			}
		}

        private void __stageMouseUp(object evt)
		{
			if(_dragging) {
				_dragging = false;
				_grip.displayObject.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_UP, __stageMouseUp);
				_grip.displayObject.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_MOVE, __stageMouseMove);
			}
		}

        private void __arrowButton1Click(object evt)
		{
            ((EventContext)evt).StopPropagation();
			if(_vertical)
				_target.ScrollUp();
			else
				_target.ScrollLeft();
		}

        private void __arrowButton2Click(object evt)
		{
            ((EventContext)evt).StopPropagation();
			if(_vertical)
				_target.ScrollDown();
			else
				_target.ScrollRight();
		}
    }
}
