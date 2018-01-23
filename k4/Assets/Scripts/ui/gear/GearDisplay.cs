using System;
using System.Collections.Generic;


namespace KGUI
{
    public class GearDisplay : GearBase
    {
        public GearDisplay(GObject owner):base(owner)
		{
            //super(owner);            
		}
		
		override protected Boolean connected
		{

            get{
                if (_controller!=null && !_pageSet.empty)
                    return _pageSet.ContainsId(_controller.selectedPageId);
                else
				    return true;
            }
			
		}

        protected override void AddStatus(string pageId, string value)
        {
        }

        protected override void Init()
        {
        }

        override public void Apply()
		{
            if (connected)
                _owner.internalVisible++;
            else
                _owner.internalVisible = 0;
		}

        public override void UpdateState()
        {
        }
    }
}
