using System;
using System.Diagnostics;
using KGUI;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PlotWin : Window
{
    private GUISkin _skin;
    public KEditorWindows kEditorWindows { get;  set; }
    public PlotWin(AppContext context) : base(context)
    {
    }

    protected override void OnInit()
    {
        contentPane = UIPackage.CreateObject("KGUI", "角色行为技能") as GComponent;
        contentPane.GetChild("behaviorHero").asGraph.SetNativeObject(new GoWrapperExt(this.WrapBehaviorPanel, this.DisposeWrapBehaviorPanel));

        kEditorWindows.position = new Rect(kEditorWindows.position.x,
            kEditorWindows.position.y,
            contentPane.width,
            contentPane.height);
    }

    protected override void OnShown()
    {
        
    }
    private float _sliderValue;
    private void WrapBehaviorPanel(DisplayObject obj, Rect r)
    {
        _skin = GUI.skin;
        GUI.SetNextControlName(obj.internalId);
        _sliderValue = EditorGUI.Slider(new Rect(0, 0, r.width, r.height), _sliderValue, 1, 100);        
        GUI.skin = _skin;
    }

    private void DisposeWrapBehaviorPanel()
    {

    }
}
