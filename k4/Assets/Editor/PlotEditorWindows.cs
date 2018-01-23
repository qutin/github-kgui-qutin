using System.Collections;
using KGUI;
using UnityEditor;
using UnityEngine;

public class PlotEditorWindows : KEditorWindows
{    
   
    public static PlotEditorWindows inst { get; set; }
    private Rect _clearRect;
    private PlotWin _win;
    [MenuItem("Tools/剧情工具")]
    public static void TestWindows()
    {
        inst = (PlotEditorWindows)EditorWindow.GetWindow(typeof(PlotEditorWindows));
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
        if(_win != null)
            _win.Dispose();
    }
   
}
