using System.Collections;
using UnityEngine;

namespace KGUI
{
    public class KUISource : IUISource
    {
        private string _fileName;
        private bool _loading;
        private bool _loaded;
        private UILoadCallback _callback;
        private AppContext _appContext;
        public KUISource(string fileName, AppContext context)
        {
            this._appContext = context;
            this._fileName = fileName;
            this._loaded = false;
            if (UIPackage.GetByName(_fileName) != null)
                this._loaded = true;
        }

        public string fileName
        {
            get { return this._fileName; }
            set { this._fileName = value; }
        }

        public bool loaded
        {
            get { return this._loaded; }
        }

        public void Load(UILoadCallback callback)
        {
            if (this._loading)
                return;

            this._callback = callback;
            this._loading = true;
            EditorCoroutineRunner.StartEditorCoroutine(this.loader());
        }
        private IEnumerator loader()
        {
            string s1 = "file://" + Application.dataPath + "/KGUI/" + this._fileName + ".kgui";
            string s2 = "file://" + Application.dataPath + "/KGUI/" + this._fileName + "@res.kgui";

            UtilsFile uf = new UtilsFile();
            uf.LoadByteArray(s1);

            byte[] s1ByteArray = null;
            while (s1ByteArray == null)
            {
                s1ByteArray = uf.ByteArray;
                yield return null;
            }
            uf.Dispose();

            uf.LoadByteArray(s2);
            byte[] s2ByteArray = null;
            while (s2ByteArray == null)
            {
                s2ByteArray = uf.ByteArray;
                yield return null;
            }
            this._loaded = true;
            UIPackage.AddPackage(s1ByteArray, s2ByteArray, _appContext);
            if (_callback != null)
                _callback.Invoke();
        }
    }
}
