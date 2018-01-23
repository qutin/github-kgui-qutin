using System;
using System.Collections;
using System.Collections.Generic;
 
using UnityEngine;

namespace KGUI
{
    public class UIPackage
    {
        private string _id;
		private string _name;
        private List<PackageItem> _items;
        private Dictionary<string, PackageItem> _itemsById;
        private Dictionary<string, PackageItem> _itemsByName;
		private ZipBuffer _desc;                  
		private ZipBuffer _files;
        private int _expiredTime;
        public AppContext appContext { get; private set; }
        private DecodeSupport _decodeSupport;

        private static Dictionary<string, UIPackage> _packageInstById = new Dictionary<string, UIPackage>();
        private static Dictionary<string, UIPackage> _packageInstByName = new Dictionary<string, UIPackage>();
        private static List<UIPackage> _packageList = new List<UIPackage>();
        //private static var _jtaLoader:JtaLoader = new JtaLoader();
        private static Dictionary<string, System.Type> _userClasses = new Dictionary<string, System.Type>();

        internal static int _constructing;
		public  UIPackage(AppContext context)
		{
            appContext = context;
            _decodeSupport = new DecodeSupport(context);
			_items = new List<PackageItem>();
            _expiredTime = UIConfig.defaultExpiredTime;
		}
		
		public static UIPackage GetById(string id)
		{
            UIPackage pkg = null;
            _packageInstById.TryGetValue(id, out pkg);
            return pkg;
		}
		
		public static UIPackage GetByName(string name)
		{
            UIPackage pkg = null;
            _packageInstByName.TryGetValue(name, out pkg);
		    return pkg;
		}
		
		public static UIPackage AddPackage( byte[] desc , byte[] res, AppContext context)
		{
			var pkg = new UIPackage(context);
			pkg.Create(desc, res);
			_packageInstById[pkg.Id] = pkg;
			_packageInstByName[pkg.Name] = pkg;
            _packageList.Add(pkg);
		    return pkg;
		}

        public static void RemovePackage(string packageIdOrName)
		{
            UIPackage pkg = null;
            if (!_packageInstById.TryGetValue(packageIdOrName, out pkg))
            {
                if (!_packageInstByName.TryGetValue(packageIdOrName, out pkg))
                    throw new Exception("FairyGUI: '" + packageIdOrName + "' is not a valid package id or name.");
            }

            _packageInstById.Remove(pkg.Id);
            _packageInstByName.Remove(pkg.Name);
            _packageList.Remove(pkg);
            pkg.Dispose();
		}
        public static void RemoveAllPackages()
        {
            if (_packageInstById.Count > 0)
            {
                UIPackage[] pkgs = _packageList.ToArray();

                foreach (UIPackage pkg in pkgs)
                {
                    RemovePackage(pkg.Id);
                }
            }
            _packageList.Clear();
            _packageInstById.Clear();
            _packageInstByName.Clear();
        }
		public static GObject CreateObject(string pkgName, string resName)
		{
            UIPackage pkg = GetByName(pkgName);
			if(pkg!=null)
                return pkg.CreateObject(resName);
			else
				return null;
		}
		
		public static GObject CreateObjectFromURL(string url)
		{
			PackageItem pi = GetItemByURL(url);
			if(pi!=null)
				return pi.owner.CreateObject2(pi);
			else
				return null;	
		}
		
		public static string GetItemURL(string pkgName, string resName)
		{
			UIPackage pkg = GetByName(pkgName);
            if (pkg == null)
                return null;

            PackageItem pi = (PackageItem)pkg._itemsByName[resName];
            if (pi == null)
                return null;

			return "ui://"+pkg.Id+pi.id;
		}
		
		public static PackageItem GetItemByURL(string url)
		{
			if(UtilsStr.StartsWith(url, "ui://"))
			{
				string pkgId = url.Substring( 5,8);
				string srcId = url.Substring(13);
                UIPackage pkg = GetById(pkgId);
				if(pkg != null)
					return pkg.GetItem(srcId);
			}
			return null;
		}

        public static void RegisterUserClass(System.Type type)
        {
            _userClasses.Add(type.ToString(), type);
        }

		private void Create(byte[] desc, byte[] res)   //ByteArray
		{
			_desc = new ZipBuffer(desc);
			_desc.CreateNameIndex();
			if(res != null && res.Length > 0 )
			{
				_files = new ZipBuffer(res);
				_files.CreateNameIndex();
			}
			
            //XML.ignoreWhitespace = true;
			byte[] buf = _desc.GetEntryData(_desc.GetEntryByName("package.xml"));
            ByteBuffer ba = new ByteBuffer(buf);            
		    string str = ba.ReadString(ba.length);
            XML xml = new XML(str);

            _id = (string)xml.GetAttribute("id");
            _name = (string)xml.GetAttribute("name");
            string[] arr = null;

            XML rxml = xml.GetNode("resources");
            if (rxml == null)
                throw new Exception("Invalid package xml");

            XMLList resources = rxml.Elements();
            _itemsById = new Dictionary<string, PackageItem>();
			_itemsByName = new Dictionary<string, PackageItem>();;
			PackageItem pi;

            foreach (XML cxml in resources)
			{                
				pi = new PackageItem();
                pi.type = cxml.name;    
				pi.id = cxml.GetAttribute("id");
                pi.name = cxml.GetAttribute("name");
                pi.file = cxml.GetAttribute("file");
                str = cxml.GetAttribute("size");                
                if (str != null)
                {      
					arr = str.Split(new char[] { ',' });
				    pi.width = int.Parse(arr[0]);
				    pi.height = int.Parse(arr[1]);
                }
				pi.standalone = cxml.GetAttribute("standalone")=="true";
				if(pi.type=="image")
				{
                    pi.scale = cxml.GetAttribute("scale");
                    if (pi.scale == "9grid")
                    {
                        str = cxml.GetAttribute("scale9grid");
                        if (str != null)
                        {
                            pi.scale9Grid = new Rect();
                            arr = str.Split(new char[] { ',' });
                            pi.scale9Grid.x = int.Parse(arr[0]);
                            pi.scale9Grid.y = int.Parse(arr[1]);
                            pi.scale9Grid.width = int.Parse(arr[2]);
                            pi.scale9Grid.height = int.Parse(arr[3]);
                        }
                    }
				}
				else if(pi.type=="jta")
				{
                    str = cxml.GetAttribute("origin");					
                    if (str!= null)
					{
						arr = str.Split(new char[]{','});
                        pi.origin.x = int.Parse(arr[0]);
                        pi.origin.y = int.Parse(arr[1]);						
                    }
				}
				
				pi.owner = this;
				_items.Add(pi);
				_itemsById[pi.id] = pi;
				_itemsByName[pi.name] = pi;
			}

            appContext.timers.Add(60 + new System.Random().Next(10), 0, Gc);
		}
		
        public void LoadAll()
		{
			int cnt=_items.Count;
			for(int i=0;i<cnt;i++)
			{
                PackageItem pi = _items[i];
				if(pi.imageData!=null || pi.loading || pi.type!="image" )
					continue;

                _decodeSupport.Add(pi);
			}
		}
		
		public void LoadNoneStandAlones()
		{
			int cnt=_items.Count;
			for(int i=0;i<cnt;i++)
			{
                PackageItem pi = _items[i];
				if(pi.standalone || pi.imageData!=null || pi.loading || pi.type!="image")
					continue;

                _decodeSupport.Add(pi);
			}
		}
		
		public void Dispose()
		{
			int cnt=_items.Count;
			for(int i=0;i<cnt;i++)
			{
                PackageItem pi = _items[i];
			    if (pi.imageData != null && pi.imageData.Texture != null)
			    {
#if UNITY_EDITOR
                    Texture.DestroyImmediate(pi.imageData.Texture); 
#else
                    Texture.Destroy(pi.imageData.Texture);//编辑器模式不可用
#endif
                }
                    
            }
            appContext.timers.Remove(Gc);
		}

        public string Id
		{
			get 
            {
                return _id;
            }
		}
		
		public string Name
		{
            get
            {
			    return _name;
            }
		}

        public int ExpiredTime
		{
            set
            {
                _expiredTime = value;
            }
			
		}
		
		public GObject CreateObject(string resName )
		{
            PackageItem pi = (PackageItem)_itemsByName[resName];
            if (pi == null)
                throw new Exception("资源不存在-" + resName);                   //throw new Error("资源不存在-"+resName);
			
			return CreateObject2(pi);
		}
		
		internal GObject CreateObject2(PackageItem pi )
		{
            GObject g;
			if(pi.type==PackageItem.COMPONENT)
			{
				XML xml = GetComponentData(pi);
				string extention = (string) xml.GetAttribute("extention");
				string userClass = (string) xml.GetAttribute("userClass");

                if (userClass != null)
                {
                    System.Type type;
                    if (!_userClasses.TryGetValue(userClass, out type))
                        throw new Exception("cant create '" + userClass + "', please registerUserClass first");
                    g = (GComponent)type.Assembly.CreateInstance(userClass);
                    if(g==null)
                        throw new Exception("'" + userClass + "' is not GComponent");
                }
                else if (extention != null)
                    g = NewObject(extention, appContext);
                else
                    g = NewObject(pi.type, appContext);
			}
			else
                g = NewObject(pi.type, appContext);

            _constructing++;
            if (g == null)
                throw new Exception("'" + pi.type + "' is not GComponent");
            g.ConstructFromResource(pi);
            _constructing--;
            if (_constructing == 0 && g.sourceWidth == UIConfig.designResolution.x
                && g.sourceHeight == UIConfig.designResolution.y)
            {	//only top level and fullscreen object should be apply these logics
                g.SetSize(appContext.groot.width, appContext.groot.height);
            }

            return g;
		}
		
		public byte[] GetResRawData(string resName)        //:ByteArray
		{
            PackageItem pi = (PackageItem)_itemsByName[resName];
			if(pi != null)
				return GetEntryData(pi);
			else
				return null;
		}
		
		public byte[] GetResRawDataById(string itemId)        //:ByteArray
		{
            PackageItem pi = (PackageItem)_itemsById[itemId];
			if(pi != null)
                return GetEntryData(pi);
            else
                return null;
		}

        public byte[] GetEntryData(PackageItem pi)
		{
			ZipEntry entry = _files.GetEntryByName(pi.file);
			if(entry!=null)
				return _files.GetEntryData(entry);
			else
				return null;
		}

        public byte[] GetImageMaskData(PackageItem pi)
		{
            ZipEntry entry = _files.GetEntryByName(pi.id + "_mask.png");
			if(entry != null)
				return _files.GetEntryData(entry);
			else
				return null;
		}

        public void AddCallback(string resName, Function callback)
		{
            PackageItem pi = (PackageItem)_itemsByName[resName];
            if(pi!=null)
				AddItemCallback(pi, callback);
        }

        public Function RemoveCallback(string resName, Function callback)
        {
            PackageItem pi = (PackageItem)_itemsByName[resName];
            if (pi != null)
                return RemoveItemCallback(pi, callback);
            else
                return null;
        }

        public void AddItemCallback(PackageItem pi, Function callback)
        {
			pi.access();
            int i = pi.callbacks.IndexOf(callback);
			if(i==-1)
				pi.callbacks.Add(callback);
			
			if(pi.type=="image")
			{
                _decodeSupport.Add(pi);
			}
			else
			{
                //swf:not support yet
                //byte[] ba = _files.getEntryData(_files.getEntryByName(pi.file));
                //object props = new {id = pi.id, callback = callback, rawContent = ba};
                //lc.allowCodeImport = true;
                //props.context = lc;
                //EasyLoader.load("", props, __swfLoaded);
			}
		}

        public Function RemoveItemCallback(PackageItem pi, Function callback)
        {
            int i = pi.callbacks.IndexOf(callback);
            if (i != -1)
            {
                pi.callbacks.RemoveAt(i);
                return callback;
            }
            else
                return null;
        }
		
		public PackageItem GetItem(string itemId)
		{
            return (PackageItem)_itemsById[itemId];
		}
		
		public XML GetComponentData(PackageItem pi)
		{
			if(pi.componentData == null)
			{
				byte[] buf = _desc.GetEntryData(_desc.GetEntryByName(pi.id+".xml"));				
                //XML.ignoreWhitespace = true;
                ByteBuffer ba = new ByteBuffer(buf);
                string str = ba.ReadString(ba.length);                
                XML xml= new XML(str);
                pi.componentData = xml;
			}
			
			return pi.componentData;
		}

        private void __swfLoaded(Loader l)
        {
            //Function callback = removeCallback(l.props.id, l.props.callback);
            //if (callback != null)
            //    callback(l.content);
        }

        public static GObject NewObject(string type, AppContext context )
		{
			GObject cls = null;
			switch(type)
			{
				case "image":
					cls = new GImage();
					break;
				case "movieclip":
			        cls = new GMovieClip();
                    break;

				case "swf":
					cls = new GSwfObject();
					break;
				
				case "jta":
					cls = new GJtaObject();
					break;
				
				case "component":
					cls  = new GComponent();
					break;
				
				case "text":
					cls  = new GTextField();
					break;
				
				case "group":
					cls  = new GGroup();
					break;
				
				case "list":
					cls = new GList();
					break;
				
				case "graph":
                    cls = new GGraph();
					break;
				
				case "loader":
                    cls = new GLoader();
					break;

                //component derived
                case "Button":
                    cls = new GButton();
                    break;

                case "Label":
                    cls = new GLabel();
                    break;

                case "ProgressBar":
                    cls = new GProgressBar();
                    break;

                case "Slider":
                    cls = new GSlider();
                    break;

                case "ScrollBar":
                    cls = new GScrollBar();
                    break;

                case "ComboBox":
                    cls = new GComboBox();
                    break;
			}
            cls.OnCreate(context);
			return cls;
		}

		private void Gc(object obj=null)
		{
			/*float now = Timers.time;
			int cnt = 0;
			foreach(PackageItem pi in _itemsById) 
			{
				if(pi.imageData!=null
					&& pi.imageData.refCount==0
					&& (now-pi.lastVisitTime)>=_expiredTime)
				{
					cnt += pi.imageData.width*pi.imageData.height*(pi.imageData.transparent?4:3);
					pi.imageData.data.dispose();
					pi.imageData = null;
				}
			}
			if(cnt>0)
				Debug.Log("UIPackage gc: " + _name + ", collect " + cnt);*/
		}
    }
}
