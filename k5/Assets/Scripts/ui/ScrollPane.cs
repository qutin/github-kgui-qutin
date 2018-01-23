using System;
using System.Collections.Generic;
using DG.Tweening;
 
using GlideTween;
using UnityEngine;
using Ease = GlideTween.Ease;
using Sprite = KGUI.Sprite;

namespace KGUI
{
    public class ScrollPane : EventDispatcher
    {
        private GComponent _owner;
        private Sprite _container;
		private Sprite _maskHolder;
		private Sprite _maskContentHolder;
		private DisplayObject _maskContent;

        private float _width;
        private float _height;
        private float _maskWidth;
        private float _maskHeight;
        private float _contentWidth;
        private float _contentHeight;
	
		private ScrollType _scrollType;
        private float _scrollSpeed;
        private float _mouseWheelSpeed;
		private Margin _margin;
		private bool _bouncebackEffect;
        private bool _touchEffect;
		private bool _scrollBarDisplayAuto;

        private float _yPerc;
        private float _xPerc;
		private bool _vScroll;
		private bool _hScroll;
		
        private static Func<float, float>  _easeTypeFunc;
		
		private float _time1, _time2;
        private float _y1, _y2;
        private int _yOverlap, _yOffset;
        private float _x1, _x2;
        private int _xOverlap, _xOffset;
		
		private bool _isMouseMoved;
		private Vector2 _holdAreaPoint;
		private bool _isHoldAreaDone;
		private int _holdArea;  
        private bool _aniFlag;

		private GScrollBar _hzScrollBar;
		private GScrollBar _vtScrollBar;

        private Glide _tweener;

        //
        bool _needRefresh;
        public EventListener onScroll { get; private set; }
		public ScrollPane(GComponent owner,
                                   ScrollType scrollType,
								   Margin margin, 
								   int scrollSpeed,
								   ScrollBarDisplayType scrollBarDisplay)
		{
            if (_easeTypeFunc==null)
                _easeTypeFunc = Ease.CubeOut;

            _owner = owner;
            _container = _owner.rootContainer;
            _maskContent = _owner.container;
			_maskContent.x = 0;
			_maskContent.y = 0;
			
			_maskHolder = new Sprite();
			_container.AddChild(_maskHolder);
	
			_maskContentHolder = new Sprite();
			_maskContentHolder.AddChild(_maskContent);
			_maskHolder.AddChild(_maskContentHolder);
            
			if(GRoot.touchScreen)
				_holdArea = 20;
			else
				_holdArea = 10;
			_holdAreaPoint = Vector2.zero;
			_margin = new Margin();
            _bouncebackEffect = UIConfig.defaultScrollBounceEffect;
            _touchEffect = UIConfig.defaultScrollTouchEffect;
			_xPerc = 0;
			_yPerc = 0;
            _aniFlag = true;

			_scrollType = scrollType;
			_scrollSpeed = scrollSpeed;
			if(_scrollSpeed==0)
                _scrollSpeed = UIConfig.defaultScrollSpeed;
			_mouseWheelSpeed = _scrollSpeed*2;
			
			if(scrollBarDisplay==ScrollBarDisplayType.Default)
                scrollBarDisplay = UIConfig.defaultScrollBarDisplay;
			
			if(scrollBarDisplay!=ScrollBarDisplayType.Hidden)
			{
				if(_scrollType==ScrollType.Both || _scrollType==ScrollType.Vertical)
				{
                    if (UIConfig.verticalScrollBar != null)
					{
                        _vtScrollBar = UIPackage.CreateObjectFromURL(UIConfig.verticalScrollBar) as GScrollBar;
                        if (_vtScrollBar==null)
                            throw new Exception("cannot create scrollbar from " + UIConfig.verticalScrollBar);
						_vtScrollBar.SetScrollPane(this, true);
						owner.rootContainer.AddChild(_vtScrollBar.displayObject);
					}
				}
				if(_scrollType==ScrollType.Both || _scrollType==ScrollType.Horizontal)
				{
                    if (UIConfig.horizontalScrollBar != null)
					{
                        _hzScrollBar = UIPackage.CreateObjectFromURL(UIConfig.horizontalScrollBar) as GScrollBar;
                        if (_hzScrollBar == null)
                            throw new Exception("cannot create scrollbar from " + UIConfig.horizontalScrollBar);
						_hzScrollBar.SetScrollPane(this, false);
						owner.rootContainer.AddChild(_hzScrollBar.displayObject);
					}
				}
				
				_scrollBarDisplayAuto = scrollBarDisplay==ScrollBarDisplayType.Auto;
				if(_scrollBarDisplayAuto)
				{
					if(_vtScrollBar!=null)
						_vtScrollBar.displayObject.visible = false;
					if(_hzScrollBar!=null)
						_hzScrollBar.displayObject.visible = false;
					
					owner.rootContainer.AddEventListenerObsolete(MouseEvent.ROLL_OVER, __rollOver);
					owner.rootContainer.AddEventListenerObsolete(MouseEvent.ROLL_OUT, __rollOut);
				}
			}
			
			_margin.left = margin.left;
			_margin.top = margin.top;
			_margin.right = margin.right;
			_margin.bottom = margin.bottom;
			
			_maskHolder.x = _margin.left;
			_maskHolder.y = _margin.top;
			
			SetSize(owner.width, owner.height);
			SetContentSize((int)owner.bounds.width, (int) owner.bounds.height);
			
			_container.AddEventListenerObsolete(MouseEvent.MOUSE_WHEEL, __mouseWheel);
			_container.AddEventListenerObsolete(MouseEvent.MOUSE_DOWN, __mouseDown);
            //
            onScroll = new EventListener(this, "onScroll");
		}
		
		public void Dispose()
		{
			_container.RemoveEventListenerObsolete(MouseEvent.MOUSE_WHEEL, __mouseWheel);
			_container.RemoveEventListenerObsolete(MouseEvent.MOUSE_DOWN, __mouseDown);
			_container.RemoveChildren();
			_maskContent.x = 0;
			_maskContent.y = 0;
			_container.AddChild(_maskContent);
		}
		
		public bool bouncebackEffect
		{
			get { return _bouncebackEffect; }
            set { _bouncebackEffect = value; }
		}

        public bool touchEffect
        {
            get { return _touchEffect; }
            set { _touchEffect = value; }
        }


        public float percX
        {
            get { return _xPerc; }
            set { SetPercX(value, false); }
        }

        public void SetPercX(float value, bool ani = false)
        {
            if (value > 1)
                value = 1;
            else if (value < 0)
                value = 0;
            if (value != _xPerc)
            {
                _xPerc = value;
                PosChanged(ani);
            }
        }

        public float percY
		{
			get { return _yPerc; }
            set { SetPercY(value, false); }
		}

        public void SetPercY(float value, bool ani = false)
        {
            if (value > 1)
                value = 1;
            else if (value < 0)
                value = 0;
            if (value != _yPerc)
            {
                _yPerc = value;
                PosChanged(ani);
            }
        }

        public float posX
        {
            get { return _xPerc*_contentWidth;}
            set { SetPosX(value, false); }
        }

        public void SetPosX(float value, bool ani = false)
        {
            this.SetPercX(value / _contentWidth, ani);
        }

        public float posY
        {
            get { return  _yPerc*_contentHeight;}
            set { SetPosY(value, false); }
        }

        public void SetPosY(float value, bool ani = false)
        {
            this.SetPercY(value / _contentHeight, ani);
        }
		
		public bool isBottomMost
        {
			get { return _yPerc==1 || _contentHeight<=_maskHeight;}
		}
		
		public bool isRightMost
        {
			get { return _xPerc==1 || _contentWidth<=_maskWidth; }
		}

        public int contentWidth
		{
			get { return (int)(_contentWidth / GRoot.contentScaleFactor); }
		}

        public int contentHeight
		{
			get { return (int)(_contentHeight / GRoot.contentScaleFactor);}
		}

        public float viewWidth
        {
            //get { return (_maskWidth / GRoot.contentScaleFactor); }
            get { return _maskWidth; }
            set
            {
                value = value + _margin.left + _margin.right;
                if (_vtScrollBar != null)
                    value += _vtScrollBar.width;
                _owner.width = value;
            }
        }

        public float viewHeight
        {
            //get { return (_maskHeight / GRoot.contentScaleFactor); }
            get { return _maskHeight; }
            set
            {
                value = value + _margin.top + _margin.bottom;
                if (_hzScrollBar != null)
                    value += _hzScrollBar.height;
                _owner.height = value;
            }
        }
        public float scrollSpeed
        {
            get { return _scrollSpeed; }
            set
            {
                _scrollSpeed = value;
                if (_scrollSpeed == 0)
                    _scrollSpeed = UIConfig.defaultScrollSpeed;
                _mouseWheelSpeed = _scrollSpeed * 2;
            }
        }
        private float GetDeltaX(float move)
		{
            return move * GRoot.contentScaleFactor *_contentWidth / (_contentWidth - _maskWidth) / _contentWidth;
		}

        private float GetDeltaY(float move)
		{
            return move * GRoot.contentScaleFactor * _contentHeight / (_contentHeight - _maskHeight) / _contentHeight;
		}

        public void ScrollTop(bool ani = false)
        {
            this.SetPercY(0, ani);
        }

        public void ScrollBottom(bool ani = false)
        {
            this.SetPercY(1, ani);
        }

        public void ScrollUp(bool ani = false) 
        {
            this.SetPercY(_yPerc - GetDeltaY(_scrollSpeed), ani);
		}

        public void ScrollDown(bool ani = false) 
        {
            this.SetPercY(_yPerc + GetDeltaY(_scrollSpeed), ani);
		}

        public void ScrollLeft(bool ani = false)
        {
            this.SetPercX(_xPerc - GetDeltaX(_scrollSpeed), ani);
		}

        public void ScrollRight(bool ani = false)
        {
            this.SetPercX(_xPerc + GetDeltaX(_scrollSpeed), ani);
		}

        public void ScrollToView(GObject obj, bool ani = false)
        {
            //float top = _contentHeight*percY;
            //float bottom = _contentHeight * percY + _maskHeight;
            //float yy = obj.y * GRoot.contentScaleFactor;
            //float hh = obj.height * GRoot.contentScaleFactor;
            //if (yy < top)
            //    this.SetPercY(Math.Max(1, yy / _contentHeight), ani);
            //else if (yy + hh > bottom)
            //    this.SetPercY((yy + hh) / _contentHeight, ani);

            _owner.EnsureBoundsCorrect();
            if (_needRefresh)
                Refresh();

            Rect rect = new Rect(obj.x, obj.y, obj.width, obj.height);
            if (obj.parent != _owner)
            {
                rect = obj.parent.LocalToGlobal(rect);
                rect = _owner.GlobalToLocal(rect);
            }
            ScrollToView(rect, ani);
		}
        public void ScrollToView(Rect rect, bool ani)
        {
            _owner.EnsureBoundsCorrect();
            if (_needRefresh)
                Refresh();

            if (_vScroll)
            {
                float top = this.posY;
                float bottom = top + _maskHeight;
                if (rect.y <= top || rect.height >= _maskHeight)
                    SetPosY(rect.y, ani);
                else if (rect.y + rect.height > bottom)
                {
                    if (rect.height <= _maskHeight / 2)
                        SetPosY(rect.y + rect.height * 2 - _maskHeight, ani);
                    else
                        SetPosY(rect.y + rect.height - _maskHeight, ani);
                }
            }
            if (_hScroll)
            {
                float left = this.posX;
                float right = left + _maskWidth;
                if (rect.x <= left || rect.width >= _maskWidth)
                    SetPosX(rect.x, ani);
                else if (rect.x + rect.width > right)
                {
                    if (rect.width <= _maskWidth / 2)
                        SetPosX(rect.x + rect.width * 2 - _maskWidth, ani);
                    else
                        SetPosX(rect.x + rect.width - _maskWidth, ani);
                }
            }

            if (!ani && _needRefresh)
                Refresh();
        }
        public bool IsChildInView(GObject obj)
        {
            if (_vScroll)
            {
                float top = this.posY;
                float bottom = top + _maskHeight;
                if (obj.y + obj.height < top || obj.y > bottom)
                    return false;
            }
            if (_hScroll)
            {
                float left = this.posX;
                float right = left + _maskWidth;
                if (obj.x + obj.width < left || obj.x > right)
                    return false;
            }

            return true;
        }

        internal void SetSize(float aWidth, float aHeight)
        {
            _width = (float)aWidth * GRoot.contentScaleFactor;
            _height = (float)aHeight * GRoot.contentScaleFactor;

            float w, h;
			w = aWidth;
			h = aHeight;
			if(_hzScrollBar!=null)
			{
				h -= _hzScrollBar.height;
				_hzScrollBar.y = h;
				if(_vtScrollBar!=null)
					_hzScrollBar.width = w - _vtScrollBar.width;
				else
					_hzScrollBar.width = w;
			}
			if(_vtScrollBar!=null)
			{
				w -= _vtScrollBar.width;
				_vtScrollBar.x = w;				
				_vtScrollBar.height = h;
			}
			w -= (_margin.left+_margin.right);
			h -= (_margin.top+_margin.bottom);
            _maskWidth = (float)w * GRoot.contentScaleFactor;
            _maskHeight = (float)h * GRoot.contentScaleFactor;
            _maskHolder.clipRect = new Rect(_margin.left, _margin.top, _maskWidth, _maskHeight);

			if(_vtScrollBar!=null)
				_vtScrollBar.displayPerc = Math.Min(1, _maskHeight/_contentHeight);
			if(_hzScrollBar!=null)
				_hzScrollBar.displayPerc = Math.Min(1, _maskWidth/_contentWidth);

			SetScroll();
            PosChanged(false);
		}

		internal void SetContentSize(int aWidth, int aHeight)
		{
            float w = aWidth*GRoot.contentScaleFactor;
			float h = aHeight*GRoot.contentScaleFactor;
			if(_contentWidth==aWidth && _contentHeight==aHeight)
				return;
			
			_contentWidth = w;
			_contentHeight = h;
			if(_vtScrollBar!=null)
				_vtScrollBar.displayPerc = Math.Min(1, _maskHeight/_contentHeight);
			if(_hzScrollBar!=null)
				_hzScrollBar.displayPerc = Math.Min(1, _maskWidth/_contentWidth);
			SetScroll();
            PosChanged(false);
		}
		
		private void SetScroll()
		{
			switch(_scrollType)
			{
				case ScrollType.Both:
					
					if(_contentWidth > _maskWidth && _contentHeight <= _maskHeight)
					{
						_hScroll = true;
						_vScroll = false;
					}
					else if(_contentWidth <= _maskWidth && _contentHeight > _maskHeight)
					{
						_hScroll = false;
						_vScroll = true;
					}
					else if(_contentWidth > _maskWidth && _contentHeight > _maskHeight)
					{
						_hScroll = true;
						_vScroll = true;
					}
					else
					{
						_hScroll = false;
						_vScroll = false;
					}
					break;
				
				case ScrollType.Vertical:
					
					if(_contentHeight > _maskHeight)
					{
						_hScroll = false;
						_vScroll = true;
					}
					else
					{
						_hScroll = false;
						_vScroll = false;
					}
					break;
				
				case ScrollType.Horizontal:
					
					if(_contentWidth > _maskWidth)
					{
						_hScroll = true;
						_vScroll = false;
					}
					else
					{
						_hScroll = false;
						_vScroll = false;
					}
					break;
			}
		}

        private void PosChanged(bool ani)
		{
			if(_aniFlag)
				_aniFlag = ani;
			_owner.appContext.timers.AddByFrame(1,1,Refresh);
		}
		
		private void Refresh(object obj=null)
		{
			if(_isMouseMoved)
			{
                _owner.appContext.timers.AddByFrame(1, 1, Refresh);
				return;
			}
			float contentYLoc;
			float contentXLoc;

			contentYLoc = _yPerc * (_contentHeight - _maskHeight);
			contentXLoc = _xPerc * (_contentWidth - _maskWidth);
			
			if(_aniFlag)
			{
                float toX = _maskContentHolder.x, toY = _maskContentHolder.y;
			
				if(_vScroll)
				{
					toY = -contentYLoc;
				}
				else
				{
					if(_maskContentHolder.y!=0)
						_maskContentHolder.y = 0;
				}
				if(_hScroll)
				{
					toX = -contentXLoc;
				}
				else
				{
					if(_maskContentHolder.x!=0)
						_maskContentHolder.x = 0;
				}
				
				if(toX!=_maskContentHolder.x || toY!=_maskContentHolder.y)
				{
					_maskHolder.mouseEnabled = false;
                    if (_tweener != null)
                        _tweener.Cancel();
                    _tweener = Glide.Tweener.Tween(_maskContentHolder,
                        new { x = toX, y = toY }, 0.5f, 0.0f);
                    _tweener.OnUpdate(__tweenUpdate);
                    _tweener.OnComplete(__tweenComplete);
                    _tweener.Ease(_easeTypeFunc);
				}
			}
			else
            {
                if (_tweener != null)
                {
                    _tweener.Cancel();
                    _tweener = null;
                }
				if(_vScroll)
					_maskContentHolder.y = -contentYLoc;
				else
					_maskContentHolder.y = 0;
				if(_hScroll)
					_maskContentHolder.x = -contentXLoc;
				else
					_maskContentHolder.x = 0;
                if (_vtScrollBar!=null)
                    _vtScrollBar.scrollPerc = _yPerc;
                if (_hzScrollBar!=null)
                    _hzScrollBar.scrollPerc = _xPerc;
			}

            _aniFlag = true;
		}
       
		private float CalcYPerc()
		{
            if (!_vScroll)
                return 0;

            float diff = _contentHeight - _maskHeight;

            float currY;
            if (_maskContentHolder.y > 0)
                currY = 0;
            else if (-_maskContentHolder.y > diff)
                currY = diff;
            else
                currY = -_maskContentHolder.y;
			
			return currY / diff;
		}

        private float CalcXPerc()
		{
            if (!_hScroll)
                return 0;

            float diff = _contentWidth - _maskWidth;

            float currX;
			if (_maskContentHolder.x > 0)
				currX = 0;
			else if ( - _maskContentHolder.x > diff)
				currX = diff;
            else
                currX = -_maskContentHolder.x;

			return currX / diff;
		}
		
		private void OnScrolling()
		{
			if(_vtScrollBar!=null)
			{
                _vtScrollBar.scrollPerc = CalcYPerc();
				if(_scrollBarDisplayAuto)
					ShowScrollBar(true);
			}
			if(_hzScrollBar!=null)
			{
				_hzScrollBar.scrollPerc = CalcXPerc();
				if(_scrollBarDisplayAuto)
					ShowScrollBar(true);
			}
		}
		
		private void OnScrollEnd()
		{
			if(_vtScrollBar!=null)
			{
				if(_scrollBarDisplayAuto)
					ShowScrollBar(false);
			}
			if(_hzScrollBar!=null)
			{
				if(_scrollBarDisplayAuto)
					ShowScrollBar(false);
			}
		}

		private void __mouseDown(object obj)
		{
            if (!_touchEffect)
                return;

            MouseEvent evt = (MouseEvent)obj;
            Vector2 pt = evt.GetLocalPoint(_container);
			//TweenLite.killTweensOf(_maskContentHolder, true);
			
			_y1 = _y2 =  _maskContentHolder.y;
            _yOffset = (int)(pt.y - _maskContentHolder.y);
			_yOverlap = (int)Math.Max(0, _contentHeight - _maskHeight);

			_x1 = _x2 = _maskContentHolder.x;
            _xOffset = (int)(pt.x - _maskContentHolder.x);
            _xOverlap = (int)Math.Max(0, _contentWidth - _maskWidth);
			
            //_time1 = _time2 = Timers.time;
            _time1 = _time2 = _owner.appContext.timers.time;
            _holdAreaPoint.x = pt.x;
            _holdAreaPoint.y = pt.y;
			_isHoldAreaDone = false;

			_container.stage.AddEventListenerObsolete(MouseEvent.MOUSE_MOVE, __mouseMove);
			_container.stage.AddEventListenerObsolete(MouseEvent.MOUSE_UP, __mouseUp);
		}
		
		private void __mouseMove(object obj)
		{
            MouseEvent evt = (MouseEvent)obj;
            Vector2 pt = evt.GetLocalPoint(_container);

            float diff;
			bool sv=false, sh=false, st=false;
			
			if (_scrollType == ScrollType.Vertical) 
			{
				if (!_isHoldAreaDone)
				{
                    diff = _holdAreaPoint.y - pt.y;
					diff = (float)Math.Sqrt(Math.Pow(diff, 2));
					if (diff < _holdArea)
						return;
				}
				
				sv = true;
			}
			else if (_scrollType == ScrollType.Horizontal) 
			{
				if (!_isHoldAreaDone)
				{
                    diff = _holdAreaPoint.x - pt.x;
					diff = (float)Math.Sqrt(Math.Pow(diff, 2));
					if (diff < _holdArea)
						return;
				}
				
				sh = true;
			}
			else
			{
				if (!_isHoldAreaDone)
				{
                    diff = _holdAreaPoint.y - pt.y;
                    diff += _holdAreaPoint.x - pt.x;
					diff = (float)Math.Sqrt(Math.Pow(diff, 2));
					if (diff < _holdArea)
						return;
				}
				
				sv = sh = true;
			}
			
            //float t = Timers.time;
            float t = _owner.appContext.timers.time;
			if (t - _time2 > 0.05f)
			{
				_time2 = _time1;
				_time1 = t;
				st = true;
			}
			
			if(sv)
			{
                float y = pt.y - _yOffset;
				if (y > 0) 
				{
					if (!_bouncebackEffect)
						_maskContentHolder.y = 0;
					else
                        _maskContentHolder.y = (int)(y * 0.5);
				}
				else if (y < -_yOverlap) 
				{
					if (!_bouncebackEffect)
						_maskContentHolder.y = -_yOverlap;
					else
						_maskContentHolder.y = (int)((y- _yOverlap) * 0.5);
				}
				else 
				{
					_maskContentHolder.y = y;
				}
				
				if (st)
				{
					_y2 = _y1;
					_y1 = _maskContentHolder.y;
				}

                _yPerc = CalcYPerc(); 
			}
			
			if(sh)
			{
                float x = pt.x - _xOffset;
				if (x > 0) 
				{
					if (!_bouncebackEffect)
						_maskContentHolder.x = 0;
					else
						_maskContentHolder.x = (int)(x * 0.5);
				}
				else if (x < 0 - _xOverlap) 
				{
					if (!_bouncebackEffect)
						_maskContentHolder.x = -_xOverlap;
					else
						_maskContentHolder.x = (int)((x - _xOverlap) * 0.5);
				}
				else 
				{
					_maskContentHolder.x = x;
				}

				if (st)
				{
					_x2 = _x1;
					_x1 = _maskContentHolder.x;
				}

                _xPerc = CalcXPerc();
			}
			
			_maskHolder.mouseEnabled = false;
			_isHoldAreaDone = true;
			_isMouseMoved = true;
			OnScrolling();
		}
		
		private void __mouseUp(object e)
		{
			_container.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_MOVE, __mouseMove);
			_container.stage.RemoveEventListenerObsolete(MouseEvent.MOUSE_UP, __mouseUp);
			
			if (!_isMouseMoved)
				return;

            if (!_touchEffect)
			{
				_isMouseMoved = false;
				return;
			}

            //float time =Timers.time - _time2;
		    float time = _owner.appContext.timers.time - _time2;
            float yVelocity = (_maskContentHolder.y - _y2) / time;
            float xVelocity = (_maskContentHolder.x - _x2) / time;

            float minDuration = _bouncebackEffect ?(float) .3 : 0;
			int overShoot =  _bouncebackEffect?1:0;
			
			/*ThrowPropsPlugin.to(_maskContentHolder, {throwProps:{
				y:{velocity:yVelocity, max:0, min:-_yOverlap, resistance:300},
				x:{velocity:xVelocity, max:0, min:-_xOverlap, resistance:300}
			}, onUpdate:__tweenUpdate2, 
				onComplete:__tweenComplete2,
				ease:_easeTypeFunc
			}, 0.5, minDuration, overShoot);*/
		}
		
		private void __mouseWheel(object obj)
		{
            MouseEvent evt = (MouseEvent)obj;
			int delta = evt.Delta;
            if (_hScroll && !_vScroll)
            {
                if (delta < 0)
                    this.SetPercX(_xPerc - GetDeltaX(_mouseWheelSpeed), false);
                else
                    this.SetPercX(_xPerc + GetDeltaX(_mouseWheelSpeed), false);
            }
            else
            {
                if (delta < 0)
                    this.SetPercY(_yPerc - GetDeltaY(_mouseWheelSpeed), false);
                else
                    this.SetPercY(_yPerc + GetDeltaY(_mouseWheelSpeed), false);
            }
		}
		
		private void __rollOver(object e)
		{
			ShowScrollBar(true);
		}
		
		private void __rollOut(object e)
		{
			ShowScrollBar(false);
		}
		
		private void ShowScrollBar(bool val)
		{
            if (val)
            {
                __showScrollBar(true);
                _owner.appContext.timers.Remove(__showScrollBar);
            }
            else
                _owner.appContext.timers.Add(0.5f, 1, __showScrollBar, val);
		}

        private void __showScrollBar(object obj = null)
		{
            bool val = (bool)obj;
			if(_vtScrollBar!=null)
				_vtScrollBar.displayObject.visible = val;
			if(_hzScrollBar!=null)
				_hzScrollBar.displayObject.visible = val;
		}
		
		private void __tweenUpdate()
		{
			OnScrolling();
		}
		
		private void __tweenComplete()
		{
			_maskHolder.mouseEnabled = true;
			OnScrollEnd();
		}

		private void __tweenUpdate2()
		{
            if (_scrollType == ScrollType.Vertical)
                _yPerc = CalcYPerc();
            else if (_scrollType == ScrollType.Horizontal)
                _xPerc = CalcXPerc();
            else
            {
                _yPerc = CalcYPerc();
                _xPerc = CalcXPerc();
            }
			
			OnScrolling();
		}
		
		private void __tweenComplete2()
		{
            if (_scrollType == ScrollType.Vertical)
                _yPerc = CalcYPerc();
            else if (_scrollType == ScrollType.Horizontal)
                _xPerc = CalcXPerc();
            else
            {
                _yPerc = CalcYPerc();
                _xPerc = CalcXPerc();
            }

			_isMouseMoved = false;
			_maskHolder.mouseEnabled = true;
			OnScrollEnd();
		}
    }
}
