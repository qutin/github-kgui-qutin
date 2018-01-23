using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using UnityEngine;

namespace KGUI
{
    public class UIConfig
    {
        //默认字体
        public static string defaultFont;

        //UI制作时依据的分辨率
        public static Vector2 designResolution = Vector2.zero;

        //独立导出的素材超过此时间没有被使用，就会被回收。秒。
        public static int defaultExpiredTime = 60;

        //窗口在显示等待时使用的资源
        public static string windowModalWaiting;

        //在弹出modal窗口时，背景变暗
        public static uint modalLayerColor = 0x333333;
        //在弹出modal窗口时，背景变暗
        public static float modalLayerAlpha = 0.2F;

        public static string buttonSound; //按钮按下的声音

        public static float buttonSoundVolumeScale = 1f;

        //水平滚动条使用的资源
        public static string horizontalScrollBar;
        //垂直滚动条使用的资源
        public static string verticalScrollBar;
        //一次滚动的变化像素
        public static int defaultScrollSpeed = 50;
        //默认滚动条显示模式 ScrollBarDisplayConst
        public static ScrollBarDisplayType defaultScrollBarDisplay = ScrollBarDisplayType.Visible;
        //默认滚动容器内可按住内容拖动
        public static bool defaultScrollTouchEffect = false;
        //默认滚动容器内使用“回弹”效果
        public static bool defaultScrollBounceEffect = false;

        //弹出菜单使用的资源
        public static string popupMenu;

        public static string popupMenu_seperator;

        //loader在载入内容时，如果载入失败，将显示此内容，例如一个表达失败的图标
        public static string loaderErrorSign;

        public static float defaultGearTweenTime = 0.3f;
        public static string defaultGearTweenFunction = "ExpoOut";

        public static String globalModalWaiting;

        public static string tooltipsWin;

        public static int defaultComboBoxVisibleItemCount = 10;

        public static int clickDragSensitivity = 2;

        public static bool renderingTextBrighterOnDesktop = true;

        public static int touchDragSensitivity = 10;

        public static int touchScrollSensitivity = 20;

        public static bool allowSoftnessOnTopOrLeftSide = false;

        public enum ConfigKey
        {
            DefaultFont,
            ButtonSound,
            ButtonSoundVolumeScale,
            HorizontalScrollBar,
            VerticalScrollBar,
            DefaultScrollSpeed,
            DefaultScrollBarDisplay,
            DefaultScrollTouchEffect,
            DefaultScrollBounceEffect,
            TouchScrollSensitivity,
            WindowModalWaiting,
            GlobalModalWaiting,
            PopupMenu,
            PopupMenu_seperator,
            LoaderErrorSign,
            TooltipsWin,
            DefaultComboBoxVisibleItemCount,
            TouchDragSensitivity,
            ClickDragSensitivity,
            ModalLayerColor,
            RenderingTextBrighterOnDesktop,
            AllowSoftnessOnTopOrLeftSide,

            PleaseSelect = 100
        }
        [Serializable]
        public class ConfigValue
        {
            public bool valid;
            public string s;
            public int i;
            public float f;
            public bool b;
            public Color c;

            public void Reset()
            {
                valid = false;
                s = null;
                i = 0;
                f = 0;
                b = false;
                c = Color.black;
            }
        }

        public List<ConfigValue> Items = new List<ConfigValue>();
        public List<string> PreloadPackages = new List<string>();

        void Awake()
        {
            //if (Application.isPlaying)
            //{
            //    foreach (string packagePath in PreloadPackages)
            //    {
            //        UIPackage.AddPackage(packagePath);
            //    }

            //    Load();
            //}
        }

        public void Load()
        {
            int cnt = Items.Count;
            for (int i = 0; i < cnt; i++)
            {
                ConfigValue value = Items[i];
                if (!value.valid)
                    continue;

                switch ((UIConfig.ConfigKey)i)
                {
                    case ConfigKey.ButtonSound:
                        //if (Application.isPlaying)
                        //    UIConfig.buttonSound = UIPackage.GetItemAssetByURL(value.s) as AudioClip;
                        break;

                    case ConfigKey.ButtonSoundVolumeScale:
                        UIConfig.buttonSoundVolumeScale = value.f;
                        break;

                    case ConfigKey.ClickDragSensitivity:
                        UIConfig.clickDragSensitivity = value.i;
                        break;

                    case ConfigKey.DefaultComboBoxVisibleItemCount:
                        UIConfig.defaultComboBoxVisibleItemCount = value.i;
                        break;

                    case ConfigKey.DefaultFont:
                        UIConfig.defaultFont = value.s;
                        break;

                    case ConfigKey.DefaultScrollBarDisplay:
                        UIConfig.defaultScrollBarDisplay = (ScrollBarDisplayType)value.i;
                        break;

                    case ConfigKey.DefaultScrollBounceEffect:
                        UIConfig.defaultScrollBounceEffect = value.b;
                        break;

                    case ConfigKey.DefaultScrollSpeed:
                        UIConfig.defaultScrollSpeed = value.i;
                        break;

                    case ConfigKey.DefaultScrollTouchEffect:
                        UIConfig.defaultScrollTouchEffect = value.b;
                        break;

                    case ConfigKey.GlobalModalWaiting:
                        UIConfig.globalModalWaiting = value.s;
                        break;

                    case ConfigKey.HorizontalScrollBar:
                        UIConfig.horizontalScrollBar = value.s;
                        break;

                    case ConfigKey.LoaderErrorSign:
                        UIConfig.loaderErrorSign = value.s;
                        break;

                    case ConfigKey.ModalLayerColor:
                        //UIConfig.modalLayerColor = value.c;
                        break;

                    case ConfigKey.PopupMenu:
                        UIConfig.popupMenu = value.s;
                        break;

                    case ConfigKey.PopupMenu_seperator:
                        UIConfig.popupMenu_seperator = value.s;
                        break;

                    case ConfigKey.RenderingTextBrighterOnDesktop:
                        UIConfig.renderingTextBrighterOnDesktop = value.b;
                        break;

                    case ConfigKey.TooltipsWin:
                        UIConfig.tooltipsWin = value.s;
                        break;

                    case ConfigKey.TouchDragSensitivity:
                        UIConfig.touchDragSensitivity = value.i;
                        break;

                    case ConfigKey.TouchScrollSensitivity:
                        UIConfig.touchScrollSensitivity = value.i;
                        break;

                    case ConfigKey.VerticalScrollBar:
                        UIConfig.verticalScrollBar = value.s;
                        break;

                    case ConfigKey.WindowModalWaiting:
                        UIConfig.windowModalWaiting = value.s;
                        break;

                    case ConfigKey.AllowSoftnessOnTopOrLeftSide:
                        UIConfig.allowSoftnessOnTopOrLeftSide = value.b;
                        break;
                }
            }
        }

        public static void ClearResourceRefs()
        {
            UIConfig.defaultFont = "";
            UIConfig.buttonSound = null;
            UIConfig.globalModalWaiting = null;
            UIConfig.horizontalScrollBar = null;
            UIConfig.loaderErrorSign = null;
            UIConfig.popupMenu = null;
            UIConfig.popupMenu_seperator = null;
            UIConfig.tooltipsWin = null;
            UIConfig.verticalScrollBar = null;
            UIConfig.windowModalWaiting = null;
        }
    }
}
