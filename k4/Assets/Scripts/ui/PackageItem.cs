using System;
using System.Collections;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class PackageItem
    {
        public const String IMAGE = "image";
		public const String SWF = "swf";
		public const String JTA = "jta";
		public const String SOUND = "sound";
		public const String COMPONENT = "component";
		public const String MISC = "misc";
		
		public String type;
		public String id;
		public String name;
		public int width;
		public int height;
		public String file;
		public Boolean standalone;
		
		//image
		public String scale;
        public Rect scale9Grid;
        public TextureRef imageData;
		//jta
        public Vector2 origin = Vector2.zero;
		
		//componenet
        public XML componentData;

        public List<Function> callbacks = new List<Function>();
		public float lastVisitTime;
        public Boolean queued;
		public Boolean loading;
		
		public UIPackage owner;
		
		public void access()
		{
            //lastVisitTime = Timers.time;
		    lastVisitTime = Time.time;
		}
    }
}
