using System;
using System.Collections;
using UnityEngine;

namespace KGUI
{
    public delegate void LoadCallBack1();
    public delegate void LoadCallBack2(WWWLoader loader);

    public class WWWLoader
    {
        public LoadCallBack2 OnComplete;
        public LoadCallBack1 OnError;
        public WWW www;
        public WWWLoader()
        {
            www = null;
        }

        public void Load(string path)
        {
            www = new WWW(path);
            if (www != null)
            {
#if UNITY_EDITOR
                EditorCoroutineRunner.StartEditorCoroutine(CoroutineLoadContent());
#else
                CoroutineRunner.StartCoroutine(CoroutineLoadContent());
#endif
            }
        }
        private IEnumerator CoroutineLoadContent()
        {
            if( !www.isDone)
                yield return www;
            if (!String.IsNullOrEmpty(www.error))
            {
                if (this.OnError != null)
                    this.OnError();
            }
            else
            {
                if (this.OnComplete != null)
                    this.OnComplete(this);    
            }
        }

    }
}
