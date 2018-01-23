using KGUI;
using UnityEngine;
using System.Collections;
 

public class UIBoot : MonoBehaviour
{

    private MyWindow win;
    //private PlotWin _win;

    private Activity _activity;
	// Use this for initialization
	void Start () {		

		
		Application.runInBackground = true;
		DontDestroyOnLoad(this.gameObject);			
			
        _activity = new Activity();
		StartCoroutine(this.test());
	    Stage.isRenderEditorWindow = false;

	}
	private IEnumerator test()
	{
        //string[] pkgs = new string[] { "basic", "KGUI" };
        string[] pkgs = new string[] { "Common" };
		for(int i=0;i<pkgs.Length;i++)
		{
			string name = pkgs[i];
#if UNITY_EDITOR
            string s1 = "file://" + Application.dataPath + "/StreamingAssets/" + name + ".kgui";
            string s2 = "file://" + Application.dataPath + "/StreamingAssets/" + name + "@res.kgui";
#else
			
            string s1 = "jar:file://" + Application.dataPath + "!/assets/" + name + ".zip";
            string s2 = "jar:file://" + Application.dataPath + "!/assets/" + name + "_res.zip";
#endif
			UtilsFile uf = new UtilsFile();
			uf.LoadByteArray( s1 );
			
			byte[] s1ByteArray = null;	
			while( s1ByteArray == null )
			{
				s1ByteArray = uf.ByteArray;
				yield return 0;
			}
			uf.Dispose();
	
			uf.LoadByteArray( s2 );
			byte[] s2ByteArray = null;
			while( s2ByteArray == null )
			{
				s2ByteArray = uf.ByteArray;
				yield return 0;
			}
			
			UIPackage.AddPackage(s1ByteArray, s2ByteArray, _activity);
		}				

        win = new MyWindow(_activity);
        win.Show();

        //_win = new PlotWin();
        //_win.Show();

	}

    void OnDestroy()
    {
        if( win != null)
            win.Dispose();
        //if (_win != null)
        //    _win.Dispose();  
    }
	void OnGUI()
	{
        _activity.stage.HandleOnGUI();
	}
	void Update()
	{
        _activity.timers.OnUpdate();
	}
}
