using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class GLoader : GObject
    {
        protected string _url;
		protected AlignType _align;
		protected VertAlignType _verticalAlign;
		protected bool _autoSize;
		protected OverflowType _overflow;
		protected bool _showErrorSign;
		protected int _jtaAction;
		protected int _jtaDir;
		protected bool _playing;
		
		protected string _contentType;
		protected PackageItem _contentItem;
		protected int _contentSourceWidth;
		protected int _contentSourceHeight;
		protected int _contentWidth;
		protected int _contentHeight;
        protected TextureRef _contentData;
        protected Loader _loader;
        //protected _jtSprite:JtSprite;
        //protected _sound:SoundExt;

        protected UISprite _container;
		protected object _content;
        protected Image _image;
		protected GObject _errorSign;
		protected int _loading;
		
		protected bool _updatingLayout;

        private static List<GObject> _errorSignPool = new List<GObject>();
		
		public GLoader()
		{
			_playing = true;
			_url = "";
            _align = AlignType.Left;
            _verticalAlign = VertAlignType.Top;
			_overflow = OverflowType.Visible;
            _jtaDir = 1;
            _loading = 0;
            _showErrorSign = true;
		}
		
		override protected void CreateDisplayObject()
		{
			_container = new UISprite(this);
		    _container.gOwner = this;
            _container.hitArea = new Rect();
            _container.scaleX = GRoot.contentScaleFactor;
            _container.scaleY = GRoot.contentScaleFactor;
			_displayObject = _container;
		}

       override  public void Dispose()
		{
			if(_loading==2)
                _contentItem.owner.RemoveItemCallback(_contentItem, __uiResLoaded);
			//else if(_loading==3)
			//	_jtSprite.dispose();
			else if(_content!=null)
			{
				/*if(_content==_jtSprite)
					_jtSprite.dispose();
				else if(_content is SoundExt)
					_sound.dispose();*/
			}
            if (_contentData != null)
            {
                _contentData.ReleaseRef();
            }
				
			base.Dispose();
		}

		public string url
		{
			get{return _url;}
            set{
                if(_url==value)
				    return;
			
			    _url = value;
			    LoadContent();
            }
		}
	
		public AlignType align
		{
			get{return _align;}
            set{
                if(_align!=value)
			    {
			    	_align = value;
				    UpdateLayout();
			    }
            }
		}		
		
		public VertAlignType verticalAlign
		{
			get{return _verticalAlign;}
            set{
                if(_verticalAlign!=value)
			    {
				    _verticalAlign = value;
				    UpdateLayout();
			    }
            }
		}
				
		public OverflowType overflow
		{
			get{return _overflow;}
            set
            {
                if(_overflow!=value)
			    {
				    _overflow = value;
				    UpdateLayout();
			    }      
            }
		}
		
		public bool autoSize
		{
			get{return _autoSize;}
            set
            {
                if(_autoSize!=value)
			    {
			    	_autoSize = value;
			    	UpdateLayout();
		    	}   
            }
		}
		
		public bool playing
		{
			get{return _playing;}
            set{
                if(_playing!=value)
			    {
			    	_playing = value;
		    		if(_content!=null)
	    			{
                        //if(_content==_jtSprite)
                        //    _jtSprite.playing = _playing;
                        //else if(_content is MovieClip)
                        //    MovieClip(_content).gotoAndStop(1);
                        //else if(_content is SoundExt)
                        //{
                        //    if(_playing)
                        //        _sound.playLoop();
                        //    else
                        //        _sound.stopLoop();
                        //}
			    	}
		    	}   
            }
		}
		
		public int jtaAction
		{
			get{return _jtaAction;}
            set{
                if(_jtaAction!=value)
			    {
			    	_jtaAction = value;
                    //if(_content==_jtSprite)
                    //    _jtSprite.action = value;
    			}   
            }
		}
		
		public int jtaDir
		{
			get{return _jtaDir;}
            set{
                if(_jtaDir!=value)
			    {
			    	_jtaDir = value;
                    //if(_content==_jtSprite)
                    //    _jtSprite.direction = value;
    			}
            }
		}
		
		public bool showErrorSign
		{
			get{return _showErrorSign;}
            set{
                _showErrorSign = value;
            }   
		}
		
		override public GLoader asLoader
		{
			get{return this;}
		}
		
		protected void LoadContent()
		{
			ClearContent();
			
			if(_url==null || _url=="" )
				return;
			 
			if(UtilsStr.StartsWith(_url, "ui://"))
			{
				string pkgId = _url.Substring(5,8);
				string itemid = _url.Substring(13);
                _contentItem = UIPackage.GetItemByURL(_url);
				bool error = false;
                if (_contentItem != null)
				{
                    _contentType = _contentItem.type;
                    _contentSourceWidth = _contentItem.width;
                    _contentSourceHeight = _contentItem.height;
						
					if(_contentType=="jta")
					{
						LoadJtaContent();
					}
					else if(_contentType=="sound")
					{
                        byte[] ba = _contentItem.owner.GetResRawDataById(_contentItem.id);
						if(ba!=null)
						{
                            //_sound = new SoundExt(ba);
                            //setContent(_sound);
						}
						else
							error = true;
					}
                    else if (_contentItem.imageData != null)
                        __uiResLoaded(_contentItem.imageData);
					else
					{
                        _contentItem.owner.AddItemCallback(_contentItem, __uiResLoaded);
						_loading = 2;
					}
				}
				else
					error = true;
				if(error)
				{
                    _contentItem = null;
					SetErrorState();
				}
				return;
			}

            _contentType = UtilsStr.GetFileExt(_url);
            if(_contentType=="png" || _contentType=="jpg")
                _contentType = "image";
            else if(_contentType=="wav" || _contentType=="mp3")
                _contentType = "sound";

            if(_contentType=="jta")
            {
            //    loadJtaContent();
            }
            else if(_contentType=="sound")
            {
            //    _sound = new SoundExt(_url);
            //    setContent(_sound);
            }
            else
            {
                if (_loader==null)
                {
                    _loader = new Loader();
                    _loader.AddEventListenerObsolete(EventContext.COMPLETE, __etcLoaded);
                    _loader.AddEventListenerObsolete(EventContext.ERROR, __etcLoadFailed);
                }
                _loader.load(_url);
                _loading = 1;
            }
		}
		
		protected void LoadJtaContent()
		{
            //if(!_jtSprite)
            //{
            //    _jtSprite = new JtSprite();
            //    _jtSprite.topLeftOrigin = true;
            //    _jtSprite.iBitmap.allowEmpty = true;
            //    _jtSprite.updateOnlyOnStage = true;
            //    _jtSprite.addEventListener(JtSprite.ACTION_READY, __jtaLoaded);
            //    _jtSprite.addEventListener(JtSprite.LAYOUT_CHANGED, __jtaLayoutChanged);
            //}
            //_jtSprite.create(_url);
            //_jtSprite.playing = true;
			_loading = 3;
		}
		
		private void __etcLoaded(object evt) 
		{
            _contentSourceWidth = (int)((DisplayObject)_loader.content).width;
            _contentSourceHeight = (int)((DisplayObject)_loader.content).height;
            SetContent(_loader.content);
		}
		
		private void __etcLoadFailed(object obj)
		{
            EventContext evt = (EventContext)obj;
            SetErrorState();
		}
		
		private void __uiResLoaded(object content)
		{
			if(content==null)
			{
				SetErrorState();
				return;
			}
			
			if(_contentType=="image")
			{
                _contentData = (TextureRef)content;
                _contentData.AddRef();
                if (_image==null)
                    _image = new Image();
                _image.texture = _contentData.Texture;
                _image.scale9Grid = _contentItem.scale9Grid;
                SetContent(_image);
			}
			else
				SetContent( (DisplayObject)(content));
		}
		
		private void __jtaLoaded(object evt)
		{			
            //_contentSourceWidth = _jtSprite.getWidth();
            //_contentSourceHeight = _jtSprite.getHeight();
            //_jtSprite.action = _jtaAction;
            //_jtSprite.direction = _jtaDir;
            //_jtSprite.playing = _playing;
            //setContent(_jtSprite);
		}
		
		private void __jtaLayoutChanged(object evt)
		{
            //if(_content==_jtSprite)
            //{
            //    _contentSourceWidth = _jtSprite.getWidth();
            //    _contentSourceHeight = _jtSprite.getHeight();
            //    updateLayout();
            //}
		}
		
		protected void SetContent(object content)
		{
			_loading = 0;
			_content = content;
			
            //if(!_playing && (_content is MovieClip))
            //    MovieClip(_content).gotoAndStop(1);
            //else if(_playing && _content is SoundExt)
            //    (content as SoundExt).playLoop();
			
			if(_content is DisplayObject)
				_container.AddChild((DisplayObject)(_content));
			
			UpdateLayout();
		}
		
		protected void SetErrorState()
		{
			_loading = 0;
            if (!_showErrorSign)
                return;

            if (_errorSign == null)
            {
                int cnt = _errorSignPool.Count;
                if (cnt > 0)
                {
                    _errorSign = _errorSignPool[cnt - 1];
                    _errorSignPool.RemoveAt(cnt - 1);
                }
                else if (UIConfig.loaderErrorSign != null)
                {
                    _errorSign = UIPackage.CreateObjectFromURL(UIConfig.loaderErrorSign);
                }
            }

            if (_errorSign != null)
            {
                _errorSign.width = this.width;
                _errorSign.height = this.height;
                _container.AddChild(_errorSign.displayObject);
            }
		}

        protected void ClearErrorState()
        {
            if (_errorSign != null)
            {
                _container.RemoveChild(_errorSign.displayObject);
                _errorSignPool.Add(_errorSign);
                _errorSign = null;
            }
        }
		
		protected void UpdateLayout()
		{
			if(_content==null || !(_content is DisplayObject))
			{
				if(_autoSize)
				{
					_updatingLayout = true;
					this.SetSize(50, 30);
					_updatingLayout = false;
				}
				return;
			}
			
			_contentWidth = _contentSourceWidth;
			_contentHeight = _contentSourceHeight;
			
			if(_autoSize)
			{
				_updatingLayout = true;
				if(_contentWidth==0)
					_contentWidth = 50;
				if(_contentHeight==0)
					_contentHeight = 30;
				this.SetSize(_contentWidth, _contentHeight);
                if(_content==_image)
                {
                    _image.scaleX = 1;
                    _image.scaleY = 1;
                }
				_updatingLayout = false;
			}
			else
			{
                float sx = 1, sy = 1;
                if (_overflow == OverflowType.Scale || _overflow == OverflowType.ScaleFree)
                {
                    if (_contentSourceWidth > _width)
                        sx = (float)_width / _contentSourceWidth;
                    if (_contentSourceHeight > _height)
                        sy = (float)_height / _contentSourceHeight;

                    if (sx != 1 || sy != 1)
                    {
                        if (_overflow == OverflowType.Scale)
                        {
                            if (sx > sy)
                                sx = sy;
                            else
                                sy = sx;
                        }
                        _contentWidth = (int)(_contentSourceWidth * sx);
                        _contentHeight = (int)(_contentSourceHeight * sy);
                    }
                }

                if (_content == _image)
                {
                    _image.scale9Width = _contentWidth;
                    _image.scale9Height = _contentHeight;
                    _image.scaleX = sx;
                    _image.scaleY = sy;
                }
                else
                {
                    ((DisplayObject)_content).scaleX = sx;
                    ((DisplayObject)_content).scaleY = sy;
                }

                if (_align == AlignType.Center)
                    ((DisplayObject)_content).x = (int)((_width - _contentWidth) / 2);
                else if (_align == AlignType.Right)
                    ((DisplayObject)_content).x = (int)(_width - _contentWidth);
                else
                    ((DisplayObject)_content).x = 0;
                if (_verticalAlign == VertAlignType.Middle)
                    ((DisplayObject)_content).y = (int)((_height - _contentHeight) / 2);
                else if (_verticalAlign == VertAlignType.Bottom)
                    ((DisplayObject)_content).y = (int)(_height - _contentHeight);
                else
                    ((DisplayObject)_content).y = 0;
			}
		}
		
		protected void ClearContent()
        {
            ClearErrorState();
			
            switch(_loading)
            {
                case 1:
                    _loader.close();
                    break;
				
                case 2:
                    _contentItem.owner.RemoveItemCallback(_contentItem, __uiResLoaded);
                    break;
				
                case 3:
                    //_jtSprite.dispose();
                    break;
            }
			
            //if(_sound)
            //{
            //    _sound.dispose();
            //    _sound = null;
            //}
			
			if(_content!=null) 
			{
				if((_content is DisplayObject) && ((DisplayObject)_content).parent != null)
					((DisplayObject)_content).parent.RemoveChild((DisplayObject)_content);
				_content = null;
			}

            _contentItem = null;
			_loading = 0;
		}
		
		override protected void HandleSizeChanged()
		{
			if(!_updatingLayout)
				UpdateLayout();
            if (_container.hitArea != null)
                _container.hitArea = new Rect(0, 0, this.width, this.height);
		}
		
        override public void Setup_BeforeAdd(XML xml)
		{
            base.Setup_BeforeAdd(xml);
			
			string str;
			str = (string)xml.GetAttribute("url");
			if(str!=null)
				url = str;
			else
				url = "";

            str = (string)xml.GetAttribute("align");
            if (str != null)
                _align = FieldTypes.parseAlign(str);

            str = (string)xml.GetAttribute("vAlign");
            if (str != null)
                _verticalAlign = FieldTypes.parseVerticalAlign(str);

            str = (string)xml.GetAttribute("overflow");
            if (str != null)
                _overflow = FieldTypes.parseOverflowType(str);
            else
                _overflow = OverflowType.Visible;
			
			_autoSize = (string)xml.GetAttribute("autoSize")=="true";

            str = (string)xml.GetAttribute("errorSign");
			_showErrorSign = str=="true";

            str = (string)xml.GetAttribute("jtaAction");
			if(str!=null)
				_jtaAction = int.Parse(str);
			else
				_jtaAction = 0;

            str = (string)xml.GetAttribute("jtaDir");
			if(str!=null)
			{
				_jtaDir = int.Parse(str);
				if(_jtaDir==0)
					_jtaDir = 1;
			}
			else
				_jtaDir = 1;
            _playing = (string)xml.GetAttribute("playing") != "false";
			
			if(_url!=null)
				LoadContent();
        }
    }
}
