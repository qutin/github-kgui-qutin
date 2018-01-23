using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
   class GearColorValue
    {
        public Color color;

        public GearColorValue(Color color)
        {
            this.color = color;
        }
    }
    public class GearColor : GearBase
	{
		private bool _applying;
		
		public GearColor(GObject owner)
            :base(owner)
		{
		}
		
		override protected object ReadValue(string value)
		{
			return UtilsStr.ConvertFromHtmlColor(value);
		}

        protected override void AddStatus(string pageId, string value)
        {
            //Color col = ToolSet.ConvertFromHtmlColor(value);
            //if (pageId == null)
            //    _default.color = col;
            //else
            //    _storage[pageId] = new GearColorValue(col);
        }

        protected override void Init()
        {
            //_default = new GearColorValue(((IColorGear)_owner).color);
            //_storage = new Dictionary<string, GearColorValue>();
        }

        override public void Apply()
		{
			_applying = true;
			
			if(connected)
			{
				object data = _storage[_controller.selectedPageId];
				if(data!=null)
					((GTextField)_owner).color = (uint)(data);
				else
					((GTextField)_owner).color = (uint)(_default);
			}
			else
				((GTextField)_owner).color = (uint)(_default);
			
			_applying = false;
		}
		
		override public void UpdateState()
		{
			if(_applying)
				return;

			if(connected)
                _storage[_controller.selectedPageId] = ((GTextField)_owner).color;
			else
                _default = ((GTextField)_owner).color;
		}
	}
}
