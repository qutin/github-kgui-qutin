using System;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    class RelationData
    {
	    public float leftDist;
	    public float rightDist;
	    public float centerDist;
	    public float topDist;
	    public float middleDist;
	    public float bottomDist;
	    public float targetWidth;
	    public float targetHeight;
    }

    delegate void func(GObject u1, GObject u2, RelationData data, bool percent);

    class RelationDef
    {
        public bool affectBySelfSizeChanged;
        public bool percent;
        public func setFunction;

        public void copyFrom(RelationDef source)
        {
            this.affectBySelfSizeChanged = source.affectBySelfSizeChanged;
            this.percent = source.percent;
            this.setFunction = source.setFunction;
        }
    }

    class RelationItem
    {
        private GObject _owner;
        private GObject _target;
        private List<RelationDef> _defs;
        private RelationData _data;

        private static Dictionary<int, func> SIDE_FUNCTIONS = new Dictionary<int, func>
            {
                {(int) RelationType.Left_Left,f_left_left },//0
                {(int) RelationType.Left_Center,f_left_center},
                {(int) RelationType.Left_Right,f_left_right},
                {(int) RelationType.Center_Center,f_center_center},
                {(int) RelationType.Right_Left,f_right_left},
                {(int) RelationType.Right_Center,f_right_center},//5
                {(int) RelationType.Right_Right,f_right_right},
                {(int) RelationType.Top_Top,f_top_top},
                {(int) RelationType.Top_Middle,f_top_middle},
                {(int) RelationType.Top_Bottom,f_top_bottom},
                {(int) RelationType.Middle_Middle,f_middle_middle},//10
                {(int) RelationType.Bottom_Top,f_bottom_top},
                {(int) RelationType.Bottom_Middle,f_bottom_middle},
                {(int) RelationType.Bottom_Bottom,f_bottom_bottom},
                {(int) RelationType.Width,f_width_width},
                {(int) RelationType.Height,f_height_height},
                {(int) RelationType.LeftExt_Left,f_leftext_left},
                {(int) RelationType.LeftExt_Right,f_leftext_right},
                {(int) RelationType.RightExt_Left,f_rightext_left},
                {(int) RelationType.RightExt_Right,f_rightext_right},
                {(int) RelationType.TopExt_Top,f_topext_top},//20
                {(int) RelationType.TopExt_Bottom,f_topext_bottom},
                {(int) RelationType.BottomExt_Top,f_bottomext_top},
                {(int) RelationType.BottomExt_Bottom,f_bottomext_bottom}//23
            };
		
		public RelationItem(GObject owner)
		{
			_owner = owner;
			_defs = new List<RelationDef>();
            _data = new RelationData();
		}

        public GObject target
        {
            get { return _target; }
            set
            {
                if (_target != value)
                {
                    if (_target != null)
                        ReleaseRefTarget(_target);
                    _target = value;
                    if (_target != null)
                        AddRefTarget(_target);
                    Refresh();
                }
            }
        }

        public void Add(RelationType relationType, bool usePercent)
        {
            if (relationType == RelationType.Size)
            {
                Add(RelationType.Width, usePercent);
                Add(RelationType.Height, usePercent);
                return;
            }

            func f = SIDE_FUNCTIONS[(int)relationType];
            foreach (RelationDef def in _defs)
            {
                if (def.setFunction == f)
                    return;
            }

            RelationDef info = new RelationDef();
            info.affectBySelfSizeChanged = relationType >= RelationType.Center_Center && relationType <= RelationType.Right_Right
                || relationType >= RelationType.Middle_Middle && relationType <= RelationType.Bottom_Bottom;
            info.percent = usePercent;
            info.setFunction = f;
            _defs.Add(info);
        }
       
        public void Remove(RelationType relationType)
        {
            if (relationType == RelationType.Size)
            {
                Remove(RelationType.Width);
                Remove(RelationType.Height);
                return;
            }

            func f = SIDE_FUNCTIONS[(int)relationType];
            int dc = _defs.Count;
            for (int k = 0; k < dc; k++)
            {
                if (_defs[k].setFunction == f)
                {
                    _defs.RemoveAt(k);
                    break;
                }
            }
        }

        public void CopyFrom(RelationItem source)
        {
            this.target = source.target;

            _defs.Clear();
            foreach (RelationDef info in source._defs)
            {
                RelationDef info2 = new RelationDef();
                info2.copyFrom(info);
                _defs.Add(info2);
            }
        }

        public void Dispose()
        {
            if (_target != null)
            {
                ReleaseRefTarget(_target);
                _target = null;
            }
        }

        public bool isEmpty
        {
            get { return _defs.Count == 0; }
        }

        public void Refresh()
        {
            if (_target == null)
                return;

            float x, y;
            if (_target != _owner.parent)
            {
                x = _target.x;
                y = _target.y;
            }
            else
            {
                x = 0;
                y = 0;
            }
            float ox, oy;
            if (_owner != target.parent)
            {
                ox = _owner.x;
                oy = _owner.y;
            }
            else
            {
                ox = 0;
                oy = 0;
            }


            float tw = _target.width;
            float th = _target.height;
            _data.leftDist = ox - x;
            _data.rightDist = ox + _owner.width - (x + tw);
            _data.centerDist = ox + _owner.width / 2 - (x + tw / 2);

            _data.topDist = oy - y;
            _data.bottomDist = oy + _owner.height - (y + th);
            _data.middleDist = oy + _owner.height / 2 - (y + th / 2);

            _data.targetWidth = tw;
            _data.targetHeight = th;
        }

		public void Apply()
		{
            foreach (RelationDef info in _defs)
                info.setFunction(_owner, _target, _data, info.percent);
		}
		
		public void ApplyOnSelfResized()
		{
            foreach (RelationDef info in _defs)
            {
                if(info.affectBySelfSizeChanged)
                    info.setFunction(_owner, _target, _data, info.percent);
            }
		}
		
		private void AddRefTarget(GObject target)
		{
            if (target != _owner.parent)
                target.AddEventListener(OutlineChangeEvent.XY, __targetChanged);
            target.AddEventListener(OutlineChangeEvent.SIZE, __targetChanged);
		}
		
		private void ReleaseRefTarget(GObject target)
		{
            target.RemoveEventListener(OutlineChangeEvent.XY, __targetChanged);
            target.RemoveEventListener(OutlineChangeEvent.SIZE, __targetChanged);
		}
		
		private void __targetChanged(object obj)
		{
            if (_owner.relations.handling)
                return;

            _owner.relations.handling = true;
			Apply();
            _owner.relations.handling = false;
		}
       
		//u1是自己，u2是目标
		//----------------------
		private static void f_left_left(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float x = u2!=u1.parent?u2.x:0;
			float w = u2.width;
            float v = data.leftDist;
			if(percent)
				v = v/data.targetWidth*w;
			u1.x = x+v;
		}
		private static void f_left_center(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float x = u2!=u1.parent?u2.x:0;
			float w = u2.width;
            float v = data.leftDist - data.targetWidth / 2;
			if(percent)
				v = v/data.targetWidth*w;
            u1.x = x + w / 2 + v;
		}
		private static void f_left_right(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float x = u2!=u1.parent?u2.x:0;
			float w = u2.width;
            float v = data.leftDist - data.targetWidth;
			if(percent)
				v = v/data.targetWidth*w;
            u1.x = x + w + v;
		}
		//----------------------
		private static void f_center_center(GObject u1, GObject u2, RelationData data, bool percent)
        {
			float x = u2!=u1.parent?u2.x:0;
			float w = u2.width;
            float v = data.centerDist;
			if(percent)
				v = v/data.targetWidth*w;
            u1.x = x + w / 2 + v - u1.width / 2;
		}
		//----------------------
		private static void f_right_left(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float x = u2!=u1.parent?u2.x:0;
			float w = u2.width;
            float v = data.rightDist + data.targetWidth;
			if(percent)
				v = v/data.targetWidth*w;
            u1.x = x + v - u1.width;
		}
		private static void f_right_center(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float x = u2!=u1.parent?u2.x:0;
			float w = u2.width;
            float v = data.rightDist + data.targetWidth / 2;
			if(percent)
				v = v/data.targetWidth*w;
            u1.x = x + w / 2 + v - u1.width;
		}
		private static void f_right_right(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float x = u2!=u1.parent?u2.x:0;
			float w = u2.width;
            float v = data.rightDist;
			if(percent)
				v = v/data.targetWidth*w;
            u1.x = x + w + v - u1.width;
		}
		//----------------------
		private static void f_top_top(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float y = u2!=u1.parent?u2.y:0;
			float h = u2.height;
            float v = data.topDist;
			if(percent)
				v = v/data.targetHeight*h;
            u1.y = y + v;
		}
		private static void f_top_middle(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float y = u2!=u1.parent?u2.y:0;
			float h = u2.height;
            float v = data.topDist - data.targetHeight / 2;
			if(percent)
				v = v/data.targetHeight*h;
            u1.y = y + h / 2 + v;
		}
		private static void f_top_bottom(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float y = u2!=u1.parent?u2.y:0;
			float h = u2.height;
            float v = data.topDist - data.targetHeight;
			if(percent)
				v = v/data.targetHeight*h;
            u1.y = y + h + v;
		}
		//----------------------
		private static void f_middle_middle(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float y = u2!=u1.parent?u2.y:0;
			float h = u2.height;
            float v = data.middleDist;
			if(percent)
				v = v/data.targetHeight*h;
            u1.y = y + h / 2 + v - u1.height / 2;
		}
		//----------------------
		private static void f_bottom_top(GObject u1, GObject u2, RelationData data, bool percent)
		{
		    float y = u2!=u1.parent?u2.y:0;
			float h = u2.height;
            float v = data.bottomDist + data.targetHeight;
			if(percent)
				v = v/data.targetHeight*h;
            u1.y = y + v - u1.height;
		}
		
		private static void f_bottom_middle(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float y = u2!=u1.parent?u2.y:0;
			float h = u2.height;
            float v = data.bottomDist + data.targetHeight / 2;
			if(percent)
				v = v/data.targetHeight*h;
            u1.y = y + h / 2 + v - u1.height;
		}
		private static void f_bottom_bottom(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float y = u2!=u1.parent?u2.y:0;
			float h = u2.height;
            float v = data.bottomDist;
			if(percent)
				v = v/data.targetHeight*h;
            u1.y = y + h + v - u1.height;
		}
		//----------------------
		private static void f_width_width(GObject u1, GObject u2, RelationData data, bool percent)
		{
            float v = data.rightDist - data.leftDist;
			if(percent)
				v = v/data.targetWidth*u2.width;
            u1.width = u2.width + v;
		}
		private static void f_height_height(GObject u1, GObject u2, RelationData data, bool percent)
		{
			float v = data.bottomDist-data.topDist;
			if(percent)
				v = v/data.targetHeight*u2.height;
            u1.height = u2.height + v;
		}
        //----------------------
        private static void f_leftext_left(GObject u1, GObject u2, RelationData data, bool percent)
        {
            //
        }
        private static void f_leftext_right(GObject u1, GObject u2, RelationData data, bool percent)
        {
            float x = u2!=u1.parent?u2.x:0;
			float w = u2.width;
            float v = data.leftDist - data.targetWidth;
			if(percent)
				v = v/data.targetWidth*w;
            float tmp = u1.x;
            u1.x = x + w + v;
            u1.width = u1.width - (u1.x - tmp);
            Debug.Log("f_leftext_right  x:" + u1.x + " w:" + u1.width);
        }
        private static void f_rightext_left(GObject u1, GObject u2, RelationData data, bool percent)
        {
           //
        }

        private static void f_rightext_right(GObject u1, GObject u2, RelationData data, bool percent)
        {
            float x = u2 != u1.parent ? u2.x : 0;
            float w = u2.width;
            float v = data.rightDist;
            if (percent)
                v = v/data.targetWidth*w;
            u1.width =  w + v ;
            Debug.Log("f_rightext_right" + u1.width);
        }
        private static void f_topext_top(GObject u1, GObject u2, RelationData data, bool percent)
        {
           //
        }
        private static void f_topext_bottom(GObject u1, GObject u2, RelationData data, bool percent)
        {
            float h = u2.height;
            float v = data.topDist - data.targetHeight;
            if (percent)
                v = v / data.targetHeight * h;
            u1.height = u1.height + h + v;
            Debug.Log("f_topext_bottom" + u1.height);
        }
        private static void f_bottomext_top(GObject u1, GObject u2, RelationData data, bool percent)
        {
           //
        }
        private static void f_bottomext_bottom(GObject u1, GObject u2, RelationData data, bool percent)
        {
            float h = u2.height;
            float v = data.bottomDist;
            if (percent)
                v = v / data.targetHeight * h;
            u1.height = u1.height + h + v - u1.height;
            Debug.Log("f_bottomext_bottom" + u1.height);
        }
   
    }
}
