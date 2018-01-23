using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class GImage : GObject, IColorGear
    {
        private Image _content;
        private TextureRef _source;
        private bool _loading;

		public GImage()
		{
		}
		
		override protected void CreateDisplayObject()
		{
			_content = new UIImage(this);
		    _content.gOwner = this;
			_displayObject = _content;
		}

        override public void Dispose()
		{
            if (_source != null)
            {
                _source.ReleaseRef();
            }
            else if (_loading)
                _packageItem.owner.RemoveItemCallback(_packageItem, __imageLoaded);
			base.Dispose();
		}

        override public void ConstructFromResource(PackageItem pkgItem)
		{
            _packageItem = pkgItem;
            _sourceWidth = _packageItem.width;
            _sourceHeight = _packageItem.height;
            _initWidth = _sourceWidth;
            _initHeight = _sourceHeight;
            _content.scale9Grid = _packageItem.scale9Grid;
			
			SetSize(_sourceWidth, _sourceHeight);

            if (_packageItem.imageData != null)
                __imageLoaded(_packageItem.imageData);
            else
            {
                _loading = true;
                _packageItem.owner.AddItemCallback(_packageItem, __imageLoaded);
            }
		}

		private void __imageLoaded(object content)
		{
            _loading = false;
            if (content == null)
            {
                _source = null;
                _content.texture = null;
            }
            else
            {
                _source = (TextureRef)content;
                _source.AddRef();
                _content.texture = _source.Texture;
            }
            HandleSizeChanged();
		}
		
		override protected void HandleSizeChanged()
		{
            _content.scale9Width = _width;
            _content.scale9Height = _height;
            _content.scaleX = (float)((float)_width / _sourceWidth * GRoot.contentScaleFactor);
            _content.scaleY = (float)((float)_height / _sourceHeight * GRoot.contentScaleFactor);
		}

        public Color color { get; set; }
    }
}
