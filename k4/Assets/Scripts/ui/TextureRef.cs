using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
using UnityEngine;

namespace KGUI
{
    public class TextureRef
    {
        private Texture _texture;
		private int _ref;

        public TextureRef(Texture texture)
		{
            _texture = texture;
		}

        public Texture Texture
		{
            get { return _texture; }
            set { _texture = value; }
		}
		
		public void AddRef()
		{
			_ref++;
		}
		
		public void ReleaseRef()
		{
			if(_ref>0)
				_ref--;
		}
		
		public int Ref
		{
			get { return _ref; }
            set { _ref = value; }
		}
		
		public int Width
		{
            get { return _texture.width; }
		}

		public int Height
		{
            get { return _texture.height; }
		}
    }
}
