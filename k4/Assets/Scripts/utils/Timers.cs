using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlideTween;
 

namespace KGUI
{
    public class Timers
    {
        private Dictionary<Function, Anymous_T> _items;
        private Dictionary<Function, Anymous_T> _toAdd;
        private IList<Anymous_T> _toRemove;
        private List<Anymous_T> _pool;
        private float _lastTime;

        public  int repeat { get; protected set; }
        public float time { get; protected set; }

        public Timers()
        {
            _items = new Dictionary<Function, Anymous_T>();
            _toAdd = new Dictionary<Function, Anymous_T>();
            _toRemove = new List<Anymous_T>();
            _pool = new List<Anymous_T>(100);
            _lastTime = Time.time;
        }

        /**
         * @interval in mini seconds
         * @repeat 0 indicate loop infinitely, otherwise the run count
         **/
        public void Add(float inerval, int repeat, Function callback, object callbackParam = null)
        {
            Anymous_T t;
            if (_items.TryGetValue(callback, out t))
            {
                t.set( inerval , repeat, callback, callbackParam, false);
                t.elapsed = 0;
                t.deleted = false;
                return;
            }
            
            if (_toAdd.TryGetValue(callback, out t))
            {
                t.set(inerval , repeat, callback, callbackParam, false);
                return;
            }
            
            t = GetFromPool();
            t.inerval = inerval;
            t.repeat =  repeat;
            t.callback = callback;
            t.param = callbackParam;
            t.useFrame = false;
            _toAdd[callback] = t;
		}

        public void AddByFrame(int frameCountInterval, int repeat, Function callback, object callbackParam = null)
        {
            Anymous_T t;
            if (_items.TryGetValue(callback, out t))
            {
                t.set(frameCountInterval, repeat, callback, callbackParam, true);
                t.elapsed = 0;
                return;
            }

            if (_toAdd.TryGetValue(callback, out t))
            {
                t.set(frameCountInterval, repeat, callback, callbackParam, true);
                return;
            }

            t = GetFromPool();
            t.inerval = frameCountInterval;
            t.repeat = repeat;
            t.callback = callback;
            t.param = callbackParam;
            t.useFrame = true;
            _toAdd[callback] = t;
        }

        public bool Exists(Function callback)
        {
            if (_toAdd.ContainsKey(callback))
                return true;

            Anymous_T at;
            if (_items.TryGetValue(callback, out at))
                return !at.deleted;

            return false;
		}

        public void Remove(Function callback)
        {
            Anymous_T t;
            if (_toAdd.TryGetValue(callback, out t))
            {
                _toAdd.Remove(callback);
                ReturnToPool(t);
            }

            if (_items.TryGetValue(callback, out t))
                t.deleted = true;
		}

        private Anymous_T GetFromPool()
        {
            Anymous_T t;
            int cnt = _pool.Count;
            if (cnt > 0)
            {
                t = _pool[cnt - 1];
                _pool.RemoveAt(cnt - 1);
                t.deleted = false;
                t.elapsed = 0;
            }
            else
                t = new Anymous_T();
            return t;
        }

        private void ReturnToPool(Anymous_T t)
        {
            t.callback = null;
            _pool.Add(t);
        }

        public void OnUpdate()
        {
            time = Time.time;
            float elapsed = time - _lastTime;
            _lastTime = time;

            Glide.Tweener.Update(elapsed);

            foreach (KeyValuePair<Function, Anymous_T> kvp in _items)
            {
                Anymous_T i = kvp.Value;
                if (i.deleted)
                {
                    _toRemove.Add(i);
                    continue;
                }

                if (i.useFrame)
                {
                    i.elapsed++;
                    if (i.elapsed < i.inerval)
                        continue;

                    i.elapsed = 0;
                }
                else
                {
                    i.elapsed += elapsed;
                    if (i.elapsed < i.inerval)
                        continue;

                    i.elapsed -= i.inerval;
                    if (i.elapsed < 0 || i.elapsed > 0.03f)
                        i.elapsed = 0;
                }

                if (i.repeat > 0)
                {
                    i.repeat--;
                    if (i.repeat == 0)
                    {
                        i.deleted = true;
                        _toRemove.Add(i);
                    }
                }
                repeat = i.repeat;
                i.callback(i.param);
            }

            foreach (Anymous_T i in _toRemove)
            {
                if (i.deleted)
                {
                    _items.Remove(i.callback);
                    ReturnToPool(i);
                }
            }
            _toRemove.Clear();

            foreach (KeyValuePair<Function, Anymous_T> kvp in _toAdd)
                _items.Add(kvp.Key, kvp.Value);
            _toAdd.Clear();
        }
    }

    public class Anymous_T
    {
        public float inerval;
        public int repeat;
        public Function callback;
        public object param;
        public bool useFrame;

        public float elapsed;
        public bool deleted;

        public void set(float interval, int repeat, Function callback, object param, bool useFrame)
        {
            this.inerval = interval;
            this.repeat = repeat;
            this.callback = callback;
            this.param = param;
            this.useFrame = useFrame;
        }
    }    
    
}
