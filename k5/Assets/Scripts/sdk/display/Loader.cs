 
using UnityEngine;

namespace KGUI
{
	public class Loader : EventDispatcher
	{
		protected string _url;
		protected object _content;

		public void load( string url )
		{
		    _url = url;
			WWWLoader loader = new WWWLoader();
			loader.OnComplete += this.OnLoadComplete;
			loader.OnError += this.OnLoadError;
			loader.Load( url );
		}

		private void OnLoadComplete( WWWLoader loader )
		{
			loader.OnComplete -= this.OnLoadComplete;
			loader.OnError -= this.OnLoadError;

            Texture2D texture = (Texture2D)loader.www.assetBundle.mainAsset;
            Image bm = new Image(texture);
			this._content = bm;

			this.DispatchEventObsolete( new EventContext( EventContext.COMPLETE ) );
		}

		private void OnLoadError( )
		{
			this.DispatchEventObsolete( new EventContext( EventContext.ERROR ) );
		}

		public object content
		{
			get { return _content; }
		}

		public string url
		{
			get { return _url; }
		}

		public void close()
		{

		}
	}
}
