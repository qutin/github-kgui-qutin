using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using KGUI;
public class KEditorWindows : EditorWindow
{

    private bool _bFirst;
    
    private GUISkin guiSkin;
    private Color color;
    private Color backgroundColor;
    private Color contentColor;

    protected Activity _activity;

    void OnEnable()
    {
        _activity = new Activity();
    }
    void Start()
    {
        this.guiSkin = GUI.skin;
        this.color = GUI.color;
        this.backgroundColor = GUI.backgroundColor;
        this.contentColor = GUI.contentColor;     
        Stage.isRenderEditorWindow = true;
        OnInit();
    }

    void OnGUI()
    {
        if (!_bFirst)
        {
            Start();
            _bFirst = true;
        }
        _activity.stage.EditorHandleOnGUI();
        Repaint();
    }
    void Update()
    {
        _activity.timers.OnUpdate();
    }

    void OnDestroy()
    {
        GUI.color = this.color;
        GUI.skin = this.guiSkin;
        GUI.backgroundColor = this.backgroundColor;
        GUI.contentColor = this.contentColor;
        UIPackage.RemoveAllPackages();
        OnDispose();
    }
    protected virtual void OnInit()
    { }
    protected virtual void OnDispose()
    { }
}
