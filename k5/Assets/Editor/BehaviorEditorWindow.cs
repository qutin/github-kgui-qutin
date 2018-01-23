using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KGUI;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class BehaviorEditorWindow : KEditorWindows
    {
        public static BehaviorEditorWindow inst { get; set; }
        private Rect _clearRect;
        private PlotWin _win;
        [MenuItem("Tools/行为工具")]
        public static void TestWindows()
        {
            inst = (BehaviorEditorWindow)EditorWindow.GetWindow(typeof(BehaviorEditorWindow));
        }
        protected override void OnInit()
        {
            _win = new PlotWin(_activity);
            _win.kEditorWindows = this;
            _win.AddUISource(new KUISource("Basic", _activity));
            _win.AddUISource(new KUISource("KGUI", _activity));
            _win.Show();
        }

        protected override void OnDispose()
        {
            if (_win != null)
                _win.Dispose();
        }
    }
}
