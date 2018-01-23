using System;
using System.Collections.Generic;
 

namespace KGUI
{
    public class Window : GComponent
    {
        private GComponent _contentPane;
        private GObject _modalWaitPane;
        private GObject _closeButton;
        private GObject _dragArea;

        private bool _modal;
        protected int _requestingCmd;


        List<IUISource> _uiSources;
        bool _inited;
        bool _loading;

		public Window(AppContext context):base()
		{
		    this.appContext = context;
            _uiSources = new List<IUISource>();

           OnCreate(context);
		}

        public override void OnCreate(AppContext context)
        {
            base.OnCreate(context);
            //_displayObject.AddEventListenerObsolete(EventContext.ADDED_TO_STAGE, _onShown);
            //_displayObject.AddEventListenerObsolete(EventContext.REMOVED_FROM_STAGE, _onHidden);
            //_displayObject.AddEventListenerObsolete(MouseEvent.MOUSE_DOWN, __mouseDown, true);

            _displayObject.onAddedToStage.Add(__onShown);
            _displayObject.onRemovedFromStage.Add(__onHidden);
            _displayObject.onTouchBegin.Add(__mouseDown);
        }

        public void AddUISource(IUISource source)
        {
            _uiSources.Add(source);
        }
		public GComponent contentPane
		{
            set{
                if (_contentPane != value)
                {
                    if (_contentPane != null)
                        RemoveChild(_contentPane);
                    _contentPane = value;
                    if (_contentPane != null)
                    {
                        AddChild(_contentPane);
                        this.SetSize(_contentPane.width, _contentPane.height);
                        _contentPane.AddRelation(this, RelationType.Size);
                        GComponent frame = _contentPane.GetChild("frame") as GComponent;
                        if (frame != null)
                        {
                            this.closeButton = frame.GetChild("closeButton");
                            this.dragArea = frame.GetChild("dragArea");
                        }
                    }
                }
            }			
            get{
                return _contentPane;
            }

		}

        public GObject closeButton
        {
            get { return _closeButton; }
            set {
                //_closeButton = value;
                //if (_closeButton != null)
                //    _closeButton.AddClickListener(__closeEventHandler); 
                if (_closeButton != null)
                    _closeButton.onClick.Remove(closeEventHandler);
                _closeButton = value;
                if (_closeButton != null)
                    _closeButton.onClick.Add(closeEventHandler);
            }
        }

        public GObject dragArea
        {
            get { return _dragArea; }
            set
            {
                if (_dragArea != value)
                {
                    if (_dragArea != null)
                        _dragArea.draggable = false;
                }
                _dragArea = value;
                if (_dragArea != null)
                {
                    if ((_dragArea is GGraph) && ((GGraph)_dragArea).displayObject == null)
                        ((GGraph)_dragArea).shape.DrawRect(_dragArea.width, _dragArea.height, 0, 0, 0);
                    _dragArea.draggable = true;
                    _dragArea.AddEventListener(DragEvent.DRAG_START, __dragStart);
                }
            }
        }
		
		public void Show()
		{
            //GRoot.inst.ShowWindow(this);
            appContext.groot.ShowWindow(this);
		}

		public void Hide()
		{			
            //GRoot.inst.HideWindow(this);
            appContext.groot.HideWindow(this);
		}
		
		public void Center()
		{
			this.SetXY((appContext.groot.width-this.width)/2, (appContext.groot.height-this.height)/2);
		}

		public void ToggleStatus()
		{
			if (isTop)
				Hide();
			else
				Show();
		}

		public bool isShowing
		{
            get{
                return parent != null;
            }			
		}

		public bool isTop
		{
            get{
                return parent != null && parent.GetChildIndex(this) == parent.numChildren - 1;
            }
		}
		
		public bool modal
		{
            get{
                return _modal;
            }			
            set{
                _modal=value;
            }
		}

		public void BringToFront()
		{
            appContext.groot.ShowWindow(this);
		}

		public void ShowModalWait(int requestingCmd=0)
		{
			if (requestingCmd != 0)
				_requestingCmd=requestingCmd;

            if (UIConfig.windowModalWaiting != null)
            {
                if (_modalWaitPane==null)
                    _modalWaitPane = UIPackage.CreateObjectFromURL(UIConfig.windowModalWaiting);

                _modalWaitPane.SetSize(this.width, this.height);

                AddChild(_modalWaitPane);
            }
		}

		public bool CloseModalWait(int requestingCmd=0)
		{
			if (requestingCmd != 0)
			{
				if (_requestingCmd != requestingCmd)
					return false;
			}
			_requestingCmd=0;

            if (_modalWaitPane!=null && _modalWaitPane.parent!=null)
                RemoveChild(_modalWaitPane);
			
			return true;
		}

		public bool modalWaiting
		{
            get{
                return (_modalWaitPane != null) && _modalWaitPane.displayObjectAdded;
            }
		}
        public void Init()
        {
            if (_inited || _loading)
                return;

            if (_uiSources.Count > 0)
            {
                _loading = false;
                int cnt = _uiSources.Count;
                for (int i = 0; i < cnt; i++)
                {
                    IUISource lib = _uiSources[i];
                    if (!lib.loaded)
                    {
                        lib.Load(__uiLoadComplete);
                        _loading = true;
                    }
                }

                if (!_loading)
                    _init();
            }
            else
                _init();
        }
        void __uiLoadComplete()
        {
            int cnt = _uiSources.Count;
            for (int i = 0; i < cnt; i++)
            {
                IUISource lib = _uiSources[i];
                if (!lib.loaded)
                    return;
            }

            _loading = false;
            _init();
        }

        void _init()
        {
            _inited = true;
            OnInit();

            if (this.isShowing)
                DoShowAnimation();
        }
      

        public override void Dispose()
        {
            //_displayObject.RemoveEventListenersObsolete();
            if (parent != null)
                Hide();
            base.Dispose();
        }
        virtual protected void OnInit()
        {
        }
		virtual protected void OnShown()
		{
		}

		virtual protected void OnHide()
		{
		}
        virtual protected void DoShowAnimation()
        {
            OnShown();
        }
        private void __onShown()
        {
            //OnShown();
            if (!_inited)
                Init();
            else
                DoShowAnimation();
        }

        private void __onHidden()
        {
            CloseModalWait();
            OnHide();
        }

        private void __mouseDown()
        {
            if (this.isShowing)
            {
                appContext.groot.ShowWindow(this);
            }
        }
        //private void _onShown(object obj)
        //{
        //    //OnShown();
        //    if (!_inited)
        //        Init();
        //    else
        //        DoShowAnimation();
        //}

        //private void _onHidden(object obj)
        //{
        //    CloseModalWait();
        //    OnHide();
        //}

        //private void __mouseDown(object obj)
        //{
        //    if (this.isShowing)
        //    {
        //        appContext.groot.ShowWindow(this);
        //    }
        //}

        //protected void __closeEventHandler(object obj)
        //{
        //    Hide();
        //}
        protected void closeEventHandler()
        {
            Hide();
        }
        private void __dragStart(object obj)
        {
            DragEvent evt = (DragEvent)obj;
            evt.PreventDefault();

            this.StartDrag();
        }
    }
}
