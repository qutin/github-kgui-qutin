using System;
using UnityEngine;

namespace KGUI
{
    public class UtilsFile
    {
        private byte[] _data;

        public void LoadByteArray(string fn) 
        {            
            WWWLoader wl = new WWWLoader();
            wl.OnComplete += this.__complete;
            wl.Load(fn);
		}
        public byte[] ByteArray
        {
            get { return _data; }
        }

        public void Dispose()
        {
            _data = null;
        }

        private void __complete(WWWLoader loader)
        {

            
            loader.OnComplete -= this.__complete;
            _data = loader.www.bytes;
        }
    }
}
