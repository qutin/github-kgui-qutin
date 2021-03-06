﻿using System;
using System.Collections.Generic;
 
using GlideTween;
using UnityEngine;

namespace KGUI
{
    public class GearXY : GearBase
    {
        private Boolean _applying;
        private Glide _tweener;

        public GearXY(GObject owner)
            : base(owner)
		{			
		}
		
		override protected object ReadValue(string value)
		{
			string[] arr = value.Split( new char[]{','});
			return new Vector2( int.Parse(arr[0]), int.Parse(arr[1]) );
		}

        protected override void AddStatus(string pageId, string value)
        {
            throw new NotImplementedException();
        }

        protected override void Init()
        {
            throw new NotImplementedException();
        }

        override public void Apply()
		{
			_applying = true;

            Vector2 pt;
            if(connected)
            {
                pt = (Vector2)_storage[_controller.selectedPageId];
                if (pt == null)
                    pt = (Vector2)(_default);
            }
            else
                pt = (Vector2)_default;

            if (pt != null)
            {
                if(_tween!=null)
                {
                    if (_tweener != null)
                        _tweener.Cancel();
                    _tweener = Glide.Tweener.Tween(_owner, new { x = pt.x, y = pt.y }, _tweenTime, 0.0f);
                    _tweener.OnComplete(() =>
                        {
                            _applying = false;
                            _tweener = null;
                        }
                     );
                    Func<float, float> func;
                    if (Ease.EaseFunctions.TryGetValue(_tween, out func))
                        _tweener.Ease(func);
                    return;
                }
                else
                    _owner.SetXY((int)((Vector2)pt).x, (int)((Vector2)pt).y);
            }
			
			_applying = false;
		}
		
		override public void UpdateState()
		{
			if(_applying)
				return;

            Vector2 pt;
			if(connected)
			{
                pt = (Vector2)_storage[_controller.selectedPageId];
                if (pt == null)
                {
                    pt = Vector2.zero;
                    _storage[_controller.selectedPageId] = pt;
                }

			}
			else
			{
                if( _default == null)
                    _default = Vector2.zero;
                pt = (Vector2)_default;
			}
			
			pt.x = _owner.x;
			pt.y = _owner.y;
		}

    }
}
