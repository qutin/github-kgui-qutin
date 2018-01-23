using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class GButton : GComponent
    {
        private string _mode;
		private bool _selected;
		private PageOption _pageOption;
		
		private Controller _buttonController;

        protected GTextField _titleObject;
		protected GLoader _iconObject;
		protected Controller _relatedController;
		
		private bool _down;
        private bool _over;
		
		public const string COMMON = "Common";
		public const string CHECK = "Check";
		public const string RADIO = "Radio";

        public const string UP = "up";
        public const string DOWN = "down";
        public const string OVER = "over";
        public const string SELECTED_OVER = "selectedOver";
        //
        public const string DISABLED = "disabled";
        public const string SELECTED_DISABLED = "selectedDisabled";
        public AudioClip sound;
        public bool changeStateOnClick;
        public EventListener onChanged { get; private set; }
        public GObject linkedPopup;

		public GButton()
		{
			_mode = COMMON;
			_pageOption = new PageOption();
            //
            //sound = UIConfig.buttonSound;
            changeStateOnClick = true;
            onChanged = new EventListener(this, "onChanged");
		}

        override public GButton asButton
        {
            get { return this; }
        }

        public string icon
        {
            get
            {
                if (_iconObject != null)
                    return _iconObject.url;
                else
                    return null;
            }

            set
            {
                if (_iconObject != null)
                    _iconObject.url = value;
            }
        }

        public string title
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

        public uint titleColor
        {
            get
            {
                if (_titleObject != null)
                    return _titleObject.color;
                else
                    return 0;
            }
            set
            {
                if (_titleObject != null)
                    _titleObject.color = value;
            }
        }

		public bool selected
		{
            get{
        	    return _selected;
            }

			set{
                if(_mode==COMMON)
				return;
			
			    if(_selected!=value)
			    {
                    _selected = value;
                    SetCurrentState();
                    if(_selected)
				        SetState(_over?SELECTED_OVER:DOWN);
                    else
                        SetState(_over?OVER:UP);
                   
                    if (_relatedController != null
                       && parent != null && !parent._buildingDisplayList)
                    {
                        if (_selected)
                        {
                            _relatedController.selectedPageId = _pageOption.id;
                        }
                        else if( _mode == CHECK && _relatedController.selectedPageId == pageOption.id)
                        {
                            _relatedController.oppositePageId = pageOption.id;
                        }
                        
                    }
                   
			    }

            }
		}
				
		public string mode
		{
            get{
                return _mode;
            }
			set{
                if(_mode!=value)
			    {
				    if(value==COMMON)
					    this.selected = false;
				    _mode = value;
			    }
            }
		}

		public Controller relatedController
		{
            get{
                return _relatedController;
            }			
            set{
                if(value!=_relatedController)
			    {
				    _relatedController = value;
                    _pageOption.controller = value;
                    _pageOption.Clear();
			    }
            }
		}
		
		public PageOption pageOption
		{
            get{
                return _pageOption;
            }			
		}

        public void AddStateListener(Function listener)
		{
            AddEventListener(StateChangeEvent.CHANGED, listener);
		}

        public void RemoveStateListener(Function listener)
		{
            RemoveEventListener(StateChangeEvent.CHANGED, listener);
		}
		
		private void SetState(string val)
		{
			if(_buttonController != null)
				_buttonController.selectedPage = val;
		}
        protected void SetCurrentState()
        {
            if (this.grayed && _buttonController != null && _buttonController.HasPage(DISABLED))
            {
                if (_selected)
                    SetState(SELECTED_DISABLED);
                else
                    SetState(DISABLED);
            }
            else
            {
                if (_selected)
                    SetState(_over ? SELECTED_OVER : DOWN);
                else
                    SetState(_over ? OVER : UP);
            }
        }
        override public void HandleControllerChanged(Controller c)
		{
			base.HandleControllerChanged(c);
			
			if(_relatedController==c)
				this.selected = _pageOption.id==c.selectedPageId;
		}

		override public void ConstructFromXML(XML cxml)
		{
			base.ConstructFromXML(cxml);

		    XML xml = cxml.GetNode("Button");

            string str;
            str = (string)xml.GetAttribute("mode");
            if (str != null)
                _mode = str;
            else
                _mode = COMMON;

			_buttonController = GetController("button");
            _titleObject = GetChildByName("title") as GTextField;
			_iconObject = GetChildByName("icon") as GLoader;

            if (_mode == COMMON)
                SetState(UP);

            _displayObject.AddEventListenerObsolete(MouseEvent.ROLL_OVER, __rolloverObsolete);
            _displayObject.AddEventListenerObsolete(MouseEvent.ROLL_OUT, __rolloutObsolete);
            _displayObject.AddEventListenerObsolete(MouseEvent.MOUSE_DOWN, __mousedown);
            AddClickListener(__clickObsolete);
            //
            displayObject.onRollOver.Add(__rollover);
            displayObject.onRollOut.Add(__rollout);
            displayObject.onTouchBegin.Add(__touchBegin);
            displayObject.onRemovedFromStage.Add(__removedFromStage);
            displayObject.onClick.Add(__click);
		}

        override public void Setup_AfterAdd(XML cxml)
		{
			base.Setup_AfterAdd(cxml);

            XML xml = cxml.GetNode("Button");
            if (xml == null)
            {
                return;
            }

            this.title = (string)xml.GetAttribute("title");
            this.icon = (string)xml.GetAttribute("icon");
            string str;
            str = (string)xml.GetAttribute("titleColor");
            if (str != null)
                this.titleColor = UtilsStr.ConvertFromHtmlColor(str);
            str = (string)xml.GetAttribute("controller");
            if(str!=null)
                _relatedController = parent.GetController(str);
            else
                _relatedController = null;
            _pageOption.id = (string)xml.GetAttribute("page");
            this.selected = (string)xml.GetAttribute("checked") == "true";
		}

        private void __rolloverObsolete(object obj)
        {
            _over = true;
            if (_down)
                return;

            SetState(_selected ? SELECTED_OVER: OVER);
        }
		
        private void __rolloutObsolete(object obj)
        {
            _over = false;
            if(_down)
                return;
			
            SetState(_selected?DOWN:UP);
        }
		
        private void __mousedown(object obj)
        {
            _down = true;
            appContext.stage.AddEventListenerObsolete(MouseEvent.MOUSE_UP, __mouseup);

            if (_mode == COMMON)
                SetState(DOWN);
        }
		
        private void __mouseup(object obj)
        {
            if(_down)
            {
                appContext.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_UP, __mouseup);
                _down = false;

                if (_mode == COMMON)
                {
                    if (_over)
                        SetState(OVER);
                    else
                        SetState(UP);
                }
            }
        }

        private void __clickObsolete(object obj)
        {
            if (_mode == CHECK)
            {
                this.selected = !_selected;
                DispatchEventObsolete(new StateChangeEvent(StateChangeEvent.CHANGED));
            }
            else if (_mode == RADIO)
            {
                if (!_selected)
                {
                    this.selected = true;
                    DispatchEventObsolete(new StateChangeEvent(StateChangeEvent.CHANGED));
                }
            }
        }
        //
        private void __touchBegin()
        {
            _down = true;
            appContext.stage.onTouchEnd.Add(__touchEnd);

            if (_mode == COMMON)
            {
                if (this.grayed && _buttonController != null && _buttonController.HasPage(DISABLED))
                    SetState(SELECTED_DISABLED);
                else
                    SetState(DOWN);
            }

            if (linkedPopup != null)
            {
                if (linkedPopup is Window)
                    ((Window)linkedPopup).ToggleStatus();
                else
                    this.root.TogglePopup(linkedPopup, this);
            }

        }

        private void __touchEnd()
        {
            if (_down)
            {
                appContext.stage.onTouchEnd.Remove(__touchEnd);
                _down = false;

                if (_mode == COMMON)
                {
                    if (this.grayed && _buttonController != null && _buttonController.HasPage(DISABLED))
                        SetState(DISABLED);
                    else if (_over)
                        SetState(OVER);
                    else
                        SetState(UP);
                }
                else
                {
                    if (!_over
                        && _buttonController != null
                        && (_buttonController.selectedPage == OVER || _buttonController.selectedPage == SELECTED_OVER))
                    {
                        SetCurrentState();
                    }
                }
                if (Event.current.type == EventType.MouseUp)
                    Event.current.type = EventType.used;
            }
        }
        private void __rollover()
        {
            _over = true;
            if (_down)
                return;

            SetState(_selected ? SELECTED_OVER : OVER);
        }

        private void __rollout()
        {
            _over = false;
            if (_down)
                return;

            SetState(_selected ? DOWN : UP);
        }
        private void __removedFromStage()
        {
            if (_over)
                __rollout();
        }
        private void __click()
        {
            //if (sound != null)
            //    Stage.inst.PlayOneShotSound(sound, soundVolumeScale);

            if (!changeStateOnClick)
                return;

            if (_mode == CHECK)
            {
                this.selected = !_selected;
                onChanged.Call();
            }
            else if (_mode == RADIO)
            {
                if (!_selected)
                {
                    this.selected = true;
                    onChanged.Call();
                }
            }
            if (Event.current.type == EventType.MouseUp)
                Event.current.type = EventType.used;
        }
    }
}
