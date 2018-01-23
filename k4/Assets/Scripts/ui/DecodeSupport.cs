using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 

namespace KGUI
{
    public class DecodeSupport 
    {
        private List<PackageItem> _queue;
		//private int _loadingCount;
		
		public  int imagesToLoadOneRound = 10;
        private AppContext _appContext;
		public DecodeSupport( AppContext context)
		{
		    _appContext = context;
			_queue = new List<PackageItem>();
		}
		
		public void Add(PackageItem pi) 
        {
			pi.queued = true;
			_queue.Add(pi);
            if (!_appContext.timers.Exists(__timer))
                _appContext.timers.AddByFrame(2, 0, __timer);
		}
		
		public Boolean Busy
		{
            get
            {
                //return (_queue.Count >0) || (_loadingCount>0);
                return (_queue.Count > 0);
            }
		}
		
		public void Reset()
        {
            //_queue.length = 0;
            _queue.Clear();
		}
		
		public void __timer(object ojb=null)
        {
			int i = 0;
			PackageItem pi;
			while(i<imagesToLoadOneRound) {
				if(_queue.Count > 0)
				{
					pi = _queue[0];// .shift();
                    _queue.RemoveAt(0);
				}
				else
				{
                    _appContext.timers.Remove(__timer);                    
					return;
				}
				
				if(pi.imageData!=null || pi.loading || pi.type!="image" )
					continue;

				//pi.loading = true;
				//_loadingCount++;
				byte[] raw = pi.owner.GetEntryData(pi);
				byte[] rawAlpha = pi.owner.GetImageMaskData(pi);

                Texture2D texRaw = new Texture2D(pi.width, pi.height, TextureFormat.ARGB32, false);
                texRaw.LoadImage(raw);

                if (rawAlpha != null)
                {
                    Texture2D texRawAlpha = new Texture2D(pi.width, pi.height, TextureFormat.ARGB32, false);
                    texRawAlpha.LoadImage(rawAlpha);
                    Merge(texRawAlpha, texRaw, texRawAlpha.width, texRawAlpha.height, 256, 256, 256, 0);
#if UNITY_EDITOR
                    Texture.DestroyImmediate(texRaw);
#else
                    Texture.Destroy(texRaw);
#endif
                    texRaw = texRawAlpha;
                }

                texRaw.filterMode = FilterMode.Point;

                pi.imageData = new TextureRef(texRaw);
                foreach (Function callback in pi.callbacks)
                    callback(pi.imageData);
                pi.callbacks.Clear();
                i++;
			}
		}

        private void Merge(Texture2D src, Texture2D another, int width, int height, uint redMultiplier, uint greenMultiplier, uint blueMultiplier, uint alphaMultiplier)
        {
            Color[] corDest = src.GetPixels(0, 0, width, height);
            Color[] corSrc = another.GetPixels(0, 0, width, height);

            for (int i = 0; i < corDest.Length; i++)
            {
                corDest[i].r = ((corSrc[i].r * redMultiplier) + (corDest[i].r * (256 - redMultiplier))) / 256.0f;
                corDest[i].g = ((corSrc[i].g * greenMultiplier) + (corDest[i].g * (256 - greenMultiplier))) / 256.0f;
                corDest[i].b = ((corSrc[i].b * blueMultiplier) + (corDest[i].b * (256 - blueMultiplier))) / 256.0f;
                corDest[i].a = ((corSrc[i].a * alphaMultiplier) + (corDest[i].a * (256 - alphaMultiplier))) / 256.0f;
            }
            src.SetPixels(0, 0, width, height, corDest);
            src.Apply(true);
        }
    }
}
