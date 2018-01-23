using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class GRoot : GComponent
    {
        private GGraph _modalLayer;
		private string _orientation;
        private GObject _curPopup;
		private GObject _lastClosedPopup;

        private static Stage _nativeStage;

		public static GRoot inst;
		public static Boolean touchScreen;
		public static float contentScaleFactor = 1;
        //
        List<GObject> _popupStack;
        List<GObject> _justClosedPopups;
        public EventListener onFocusChanged { get; private set; }
        bool _focusManagement;
        GObject _tooltipWin;
        GObject _defaultTooltipWin;

		public GRoot() 
		{
			_modalLayer = new GGraph();
            _modalLayer.shape.DrawRect(10, 10, 0, 0, (((uint)(255.0f * UIConfig.modalLayerAlpha)) << 24) + UIConfig.modalLayerColor);

            _popupStack = new List<GObject>();
            _justClosedPopups = new List<GObject>();

            onFocusChanged = new EventListener(this, "onFocusChanged");

            
		}
		
		public static void InitObsolete(string orientation ="none")
		{
			inst = new GRoot();
            //inst.Init(orientation);//暂时为了避免报错
		}
		
		public  Stage nativeStage
		{
            get
            {
                return _nativeStage;
            }
			
		}

		//orientation:landscape,portrait,auto
        public void Init(AppContext context ,string orientation)
        {
            OnCreate(context);
            //_nativeStage = Stage.inst;
            _nativeStage = this.appContext.stage;
            _nativeStage.AddChild(_displayObject);
            _orientation = orientation;
            touchScreen = false;

            _nativeStage.AddEventListenerObsolete(EventContext.RESIZE, __winResize);
            _nativeStage.AddEventListenerObsolete(MouseEvent.MOUSE_DOWN, __stageMouseDown, true);
            //Stage.inst.onTouchBegin.AddCapture(__stageTouchBegin);
            nativeStage.onTouchBegin.AddCapture(__stageTouchBegin);
            __winResize(null);
        }

        public void ShowWindow(Window win) 
        {
            if(win.parent==this)
                SetChildIndex(win, this.numChildren-1);
            else
                AddChild(win);

            if(win.x>this.width)
                win.x = this.width - win.width;
            if(win.y>this.height)
                win.y = this.height - win.height;
			
            AdjustModalLayer();
        }
		
        public void HideWindow(Window win)
        {
            if(win.parent==this)
            {
                RemoveChild(win);
            }
			
            AdjustModalLayer();
        }
		
        public void CloseAllExceptModals()
        {
            int cnt = this.numChildren;
            int i=0;
            while(i<cnt) {
                GObject g = GetChildAt(i);
                if((g is Window) && !(g as Window).modal)  {
                    (g as Window).Hide();
                    cnt--;
                }
                else
                    i++;
            }
        }
		
        public Window GetTopWindow()
        {
            int cnt = this.numChildren;
            for(int i=cnt-1;i>=0;i--) {
                GObject g = this.GetChildAt(i);
                if(g is Window) {
                    return (Window)(g);
                }
            }
			
            return null;
        }
		
        public bool isModal
        {
            get { return _modalLayer.parent != null; }
        }

        public GObject objectUnderMouse
        {
            get
            {
                return DisplayObjectToGObject(_nativeStage.objectUnderMouse);
            }
        }

        public GObject DisplayObjectToGObject(DisplayObject obj)
        {
            while (obj != null)
            {
                if (obj.gOwner != null)
                    return obj.gOwner;

                obj = obj.parent;
            }
            return null;
        }

        public GObject focus
        {
            get
            {
                return DisplayObjectToGObject(_nativeStage.focus);
            }
            set
            {
                _nativeStage.focus = value.displayObject;
            }
        }
		
        private void AdjustModalLayer()
        {
            int cnt = this.numChildren;
            for(int i=cnt-1;i>=0;i--) {
                GObject g = this.GetChildAt(i);
                if(g!=_modalLayer && (g is Window) && (g as Window).modal) {
                    if(_modalLayer.parent==null)
                        AddChildAt(_modalLayer, i);
                    else if(i>0)
                        SetChildIndex(_modalLayer, i-1);
                    else
                        AddChildAt(_modalLayer, 0);
                    return;
                }
            }
			
            if(_modalLayer.parent!=null)
                RemoveChild(_modalLayer);
        }

        public void ShowPopup(GObject popup, GObject target=null, object downward=null) 
		{
			if(_curPopup!=null && _curPopup!=popup)
				HidePopup();

            _popupStack.Add(popup);
            AddChild(popup);

			Vector2 pos;
			float sizeW = 0;
            float sizeH = 0;
			if(target!=null)
			{
				pos = target.LocalToGlobal();
				sizeW = target.width;
				sizeH = target.height;
			}
			else
			{
				pos = new Vector2((float)(nativeStage.mouseX/contentScaleFactor), (float)(nativeStage.mouseY/contentScaleFactor));
			}
			float xx, yy;
			xx = pos.x;
			if(xx + popup.width>this.width)
				xx = xx+sizeW-popup.width;
			yy = pos.y+sizeH;
			if((downward==null && yy + popup.height>this.height)
                || downward!=null && (Boolean)downward == false)
            {
				yy = pos.y - popup.height - 1;
				if(yy<0) {
					yy = 0;
					xx += sizeW/2;
				}
			}
			
			popup.x = (int) xx;
			popup.y = (int) yy;
			_curPopup = popup;
		}
		
		public void HidePopup()
		{
            //if(_curPopup!=null)
            //{
            //    if(_curPopup.parent!=null)
            //        RemoveChild(_curPopup);
            //    _curPopup = null;
            //}
            HidePopup(null);
		}
        public void HidePopup(GObject popup)
        {
            if (popup != null)
            {
                int k = _popupStack.IndexOf(popup);
                if (k != -1)
                {
                    for (int i = _popupStack.Count - 1; i >= k; i--)
                    {
                        int last = _popupStack.Count - 1;
                        ClosePopup(_popupStack[last]);
                        _popupStack.RemoveAt(last);
                    }
                }
            }
            else
            {
                foreach (GObject obj in _popupStack)
                    ClosePopup(obj);
                _popupStack.Clear();
            }
        }
		//FairyGUI
        public void TogglePopup(GObject popup, GObject target)
        {
            TogglePopup(popup, target, null);
        }
        public void TogglePopup(GObject popup, GObject target, object downward)
        {
            if (_justClosedPopups.IndexOf(popup) != -1)
                return;

            ShowPopup(popup, target, downward);
        }
        void __stageTouchBegin(EventContext context)
        {
            if (_focusManagement)
            {
                //DisplayObject mc = Stage.inst.touchTarget as DisplayObject;
                DisplayObject mc = nativeStage.touchTarget as DisplayObject;
                //while (mc != Stage.inst && mc != null)
                while (mc != nativeStage && mc != null)
                {
                    GObject gg = mc.gOwner;
                    if (gg != null && gg.touchable && gg.focusable)
                    {
                        this.focus = gg;
                        break;
                    }
                    mc = mc.parent;
                }
            }

            if (_tooltipWin != null)
                HideTooltips();

            CheckPopups();
        }
        void CheckPopups()
        {
            _justClosedPopups.Clear();
            if (_popupStack.Count > 0)
            {
                //DisplayObject mc = Stage.inst.touchTarget as DisplayObject;
                DisplayObject mc = nativeStage.touchTarget as DisplayObject;
                bool handled = false;
                //while (mc != Stage.inst && mc != null)
                while (mc != nativeStage && mc != null)
                {
                    if (mc.gOwner != null)
                    {
                        int k = _popupStack.IndexOf(mc.gOwner);
                        if (k != -1)
                        {
                            for (int i = _popupStack.Count - 1; i > k; i--)
                            {
                                int last = _popupStack.Count - 1;
                                GObject popup = _popupStack[last];
                                ClosePopup(popup);
                                _justClosedPopups.Add(popup);
                                _popupStack.RemoveAt(last);
                            }
                            handled = true;
                            break;
                        }
                    }
                    mc = mc.parent;
                }

                if (!handled)
                {
                    for (int i = _popupStack.Count - 1; i >= 0; i--)
                    {
                        GObject popup = _popupStack[i];
                        ClosePopup(popup);
                        _justClosedPopups.Add(popup);
                    }
                    _popupStack.Clear();
                }
            }
        }
        void ClosePopup(GObject target)
        {
            if (target.parent != null)
            {
                if (target is Window)
                    ((Window)target).Hide();
                else
                    RemoveChild(target);
            }
        }
        public void ShowTooltips(string msg)
        {
            if (_defaultTooltipWin == null)
            {
                string resourceURL = UIConfig.tooltipsWin;
                if (string.IsNullOrEmpty(resourceURL))
                {
                    Debug.LogWarning("FairyGUI: UIConfig.tooltipsWin not defined");
                    return;
                }

                _defaultTooltipWin = UIPackage.CreateObjectFromURL(resourceURL);
            }

            _defaultTooltipWin.text = msg;
            ShowTooltipsWin(_defaultTooltipWin);
        }
		public GObject LastClosedPopup
		{
            get { return _lastClosedPopup; }
		}
		
        private void __stageMouseDown(object obj) 
        {
            EventContext evt = (EventContext)obj;
            _lastClosedPopup = null;
			if(_curPopup!=null) {
                DisplayObject check = _curPopup.displayObject;
                DisplayObject mc = evt.Target as DisplayObject;
                while(mc!=displayObject.stage && mc!=null) {
                    if(mc==check)
                        return;
                    mc = mc.parent;
                }
               _lastClosedPopup = _curPopup;
				HidePopup();
            }
        }
		
        private void __winResize(object obj)
        {
            EventContext evt = (EventContext)obj;
            int w, h;
            w = _nativeStage.stageWidth;
            h = _nativeStage.stageHeight;
            if(_orientation=="landscape" && w<h || _orientation=="portrait" && w>h)
            {
                int t = w;
                w = h;
                h = t;
            }
            this.SetSize(w,h);

            _modalLayer.width = w;
            _modalLayer.height = h;
        }

        //
        public void ShowTooltipsWin(GObject tooltipWin)
        {
            HideTooltips();

            _tooltipWin = tooltipWin;
            appContext.timers.Add(0.1f, 1, __showTooltipsWin);
        }
        public void HideTooltips()
        {
            if (_tooltipWin != null)
            {
                if (_tooltipWin.parent != null)
                    RemoveChild(_tooltipWin);
                _tooltipWin = null;
            }
        }
       
        void __showTooltipsWin(object param)
        {
            if (_tooltipWin == null)
                return;

            float xx = Stage.touchPosition.x + 10;
            float yy = Stage.touchPosition.y + 20;

            Vector2 pt = this.GlobalToLocal(new Vector2(xx, yy));
            xx = pt.x;
            yy = pt.y;

            if (xx + _tooltipWin.width > this.width)
                xx = xx - _tooltipWin.width;
            if (yy + _tooltipWin.height > this.height)
            {
                yy = yy - _tooltipWin.height - 1;
                if (yy < 0)
                    yy = 0;
            }

            _tooltipWin.x = Mathf.RoundToInt(xx);
            _tooltipWin.y = Mathf.RoundToInt(yy);
            AddChild(_tooltipWin);
        }
    }
}
