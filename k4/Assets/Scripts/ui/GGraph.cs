using System;
using System.Collections.Generic;
using System.Runtime.Remoting;


namespace KGUI
{
    public class GGraph : GObject
    {
        private UIShape _shape;

        public GGraph()
        {
        }

		override public GGraph asGraph
		{
            get
            {
                return this;
            }			
		}

        public void ReplaceMe(GObject target)
        {
            if (_parent == null)
                throw new Exception("parent not set");
            
            target.name = this.name;
            target.alpha = _alpha;
            target.rotation = _rotation;
            target.visible = _visible;
            target.touchable = _touchable;
            target.grayed = _grayed;
            target.SetXY(this.x, this.y);
            target.SetSize(this.width, this.height);

            int index = _parent.GetChildIndex(this);
            _parent.AddChildAt(target, index);
            target.relations.CopyFrom(this.relations);

            _parent.RemoveChild(this, true);
        }

        public void AddBeforeMe(GObject target)
        {
            if (_parent == null)
                throw new Exception("parent not set");
            
            int index = _parent.GetChildIndex(this);
            _parent.AddChildAt(target, index);
        }

        public void AddAfterMe(GObject target)
        {
            if (_parent == null)
                throw new Exception("parent not set");
            
            int index = _parent.GetChildIndex(this);
            index++;
            _parent.AddChildAt(target, index);
        }

        public void SetNativeObject(DisplayObject obj)
        {
            if (_displayObject == obj)
                return;

            if (_displayObject!=null)
            {
                obj.internalId = _displayObject.internalId;
                _displayObject.Dispose();
                _shape = null;
                _displayObject = null;
            }
            _displayObject = obj;
            _displayObject.alpha = _alpha;
            _displayObject.rotation = _rotation;
            _displayObject.visible = _visible;
            _displayObject.mouseEnabled = _touchable;
            _displayObject.gOwner = this;

            if (_parent != null)
                _parent.ChildStateChanged(this);
            HandlePositionChanged();//只重设位置 
            //所以原生GUI方式是否应确立范围区域
            HandleSizeChanged();
        }

        public Shape shape
        {
            get
            {
                if (_shape != null)
                    return _shape;

                if (_displayObject != null)
                    _displayObject.Dispose();

                _shape = new UIShape(this);
                _shape.gOwner = this;
                _displayObject = _shape;
                if (_parent != null)
                    _parent.ChildStateChanged(this);
                HandlePositionChanged();
                _displayObject.alpha = _alpha;
                _displayObject.rotation = _rotation;
                _displayObject.visible = _visible;

                return _shape;
            }
        }

		override protected void HandleSizeChanged()
		{
            if (_shape != null)
            {
                _shape.width = _width * GRoot.contentScaleFactor;
                _shape.height = _height * GRoot.contentScaleFactor;
            }
            if (_displayObject != null)
            {
                _displayObject.width = _width * GRoot.contentScaleFactor;
                _displayObject.height = _height * GRoot.contentScaleFactor;
            }
		}

        override public void Setup_BeforeAdd(XML xml)
		{
		    string str;
			string type = (string)xml.GetAttribute("type");
            if (type != null && type != "empty")
            {
                _shape = new UIShape(this);
                _shape.gOwner = this;
                _displayObject = _shape;
            }

            base.Setup_BeforeAdd(xml);

            if (_shape != null)
			{
				int lineSize;
				str = (string)xml.GetAttribute("lineSize");
				if(str != null)
					lineSize = int.Parse(str);
				else
					lineSize = 1;
				
				uint lineColor;
				str = (string)xml.GetAttribute("lineColor");
				if(str!=null)
					lineColor = UtilsStr.ConvertFromHtmlColor(str,true);
				else
					lineColor = 0xFF000000;
				
				uint fillColor;
				str = (string)xml.GetAttribute("fillColor");
				if(str!=null)
					fillColor = UtilsStr.ConvertFromHtmlColor(str,true);
				else
					fillColor = 0xFFFFFFFF;
				
				string corner;
				str = (string)xml.GetAttribute("corner");
				if(str!=null)
					corner = str;

                _shape.DrawRect(this.width, this.height, lineSize, lineColor, fillColor);
			}
		}
    }
}
