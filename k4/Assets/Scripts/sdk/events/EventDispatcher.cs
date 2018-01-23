using System;
using System.Collections.Generic;
 

namespace KGUI
{
    public delegate void Function(object param);
    //
    public delegate void EventCallback0();
    public delegate void EventCallback1(EventContext context);
    public class EventDispatcher : IEventDispatcher
    {
        private Dictionary<string, Function> _dicObsolete;
        private Dictionary<string, Function> _dicCapture;
        private string _dispatching;
        private static List<List<EventDispatcher>> sBubbleChains = new List<List<EventDispatcher>>();
        //
        Dictionary<string, EventBridge> _dic;


        [Obsolete("use AddEventListener")]
        public void AddEventListenerObsolete(string strType, Function listener, bool useCapture = false)
        {
            if (strType == null)
                throw new Exception("event type cant be null");

            Dictionary<string, Function> dic;
            if (!useCapture)
            {
                if (_dicObsolete == null)
                    _dicObsolete = new Dictionary<string, Function>();
                dic = _dicObsolete;
            }
            else
            {
                if (_dicCapture == null)
                    _dicCapture = new Dictionary<string, Function>();
                dic = _dicCapture;
            }

            Function f = null;
            if (dic.ContainsKey(strType))
            {
                if (dic.TryGetValue(strType, out f))
                {
                    f -= listener;
                    if (f != null && f.GetInvocationList().Length > 0)
                        f += listener;
                    else
                        f = listener;
                    dic[strType] = f;
                }
            }
            else
            {
                dic.Add(strType, listener);
            }
        }
        [Obsolete("use RemoveEventListener")]
        public void RemoveEventListenerObsolete(string strType, Function listener, bool useCapture = false)
        {
            Dictionary<string, Function> dic;
            if (!useCapture)
            {
                if (_dicObsolete == null)
                    return;

                dic = _dicObsolete;
            }
            else
            {
                if (_dicCapture == null)
                    return;

                dic = _dicCapture;
            }

            Function f = null;
            if (dic.ContainsKey(strType))
            {
                if (dic.TryGetValue(strType, out f))
                {
                    f = (Function)f.Clone();
                    f -= listener;
                    dic.Remove(strType);
                    if (f!=null && f.GetInvocationList().Length > 0)
                        dic.Add(strType, f);
                }
            }        
        }
        [Obsolete("use RemoveEventListeners")]
        public void RemoveEventListenersObsolete(string strType = null, bool useCapture = false)
        {
            if (strType != null)
            {
                if (!useCapture)
                {
                    if (_dicObsolete != null && _dicObsolete.ContainsKey(strType))
                        _dicObsolete.Remove(strType);
                }
                else
                {
                    if (_dicCapture != null && _dicCapture.ContainsKey(strType))
                        _dicCapture.Remove(strType);
                }
            }
            else
            {
                if (!useCapture)
                    _dicObsolete = null;
                else
                    _dicCapture = null;
            }
        }
        [Obsolete("use DispatchEvent")]
        public void DispatchEventObsolete(EventContext evt)
        {
            bool bubbles = evt.Bubbles;
            
            if (!bubbles 
                && (_dicObsolete == null || !_dicObsolete.ContainsKey(evt.Type))
                && (_dicCapture == null || !_dicCapture.ContainsKey(evt.Type))
                )
                return; // no need to do anything

            if (_dispatching == evt.Type) //avoid dead loop
                return;

            _dispatching = evt.Type;
            
            EventDispatcher previousTarget = evt.Target;
            evt.SetTarget(this);
            evt.StopsPropagation = false;

            if (bubbles && this is DisplayObject)
                BubbleEventObsolete(evt);
            else
            {
                if(!InvokeEvent(evt, true))
                    InvokeEvent(evt, false);
            }
            
            if (previousTarget!=null) evt.SetTarget(previousTarget);

            _dispatching = null;
        }
        
        /** @private
         *  Invokes an event on the current object. This method does not do any bubbling, nor
         *  does it back-up and restore the previous target on the event. The 'dispatchEvent' 
         *  method uses this method internally. */
        internal bool InvokeEvent(EventContext evt, bool inCapture)
        {
            Function f = null;
            string strType = evt.Type;
            if (!inCapture && _dicObsolete != null && _dicObsolete.TryGetValue(strType, out f)
                || inCapture && _dicCapture != null && _dicCapture.TryGetValue(strType, out f)
                )
            {
                evt.SetSender(this);
                f.Invoke(evt);
                return evt.StopsPropagation;
            }

            return false;
        }

        [Obsolete("use BubbleEvent")]
        internal void BubbleEventObsolete(EventContext evt)
        {
            // we determine the bubble chain before starting to invoke the listeners.
            // that way, changes done by the listeners won't affect the bubble chain.
            
            List<EventDispatcher> chain;
            DisplayObject element = this as DisplayObject;
            int length = 1;

            if (sBubbleChains.Count > 0) 
            {
                chain = sBubbleChains[sBubbleChains.Count - 1];
                sBubbleChains.RemoveAt(sBubbleChains.Count - 1);
                chain.Add(element);
            }
            else 
            {
                chain = new List<EventDispatcher>();
                chain.Add(element);
            }

            while ((element = element.parent) != null)
            {
                length++;
                chain.Add(element);
            }

            bool stopPropagation = false;
            for (int i = length-1; i >=0; i--)
            {
                stopPropagation = chain[i].InvokeEvent(evt, true);
                if (stopPropagation) break;
            }

            if (!stopPropagation)
            {
                for (int i = 0; i < length; ++i)
                {
                    stopPropagation = chain[i].InvokeEvent(evt, false);
                    if (stopPropagation) break;
                }
            }

            chain.Clear();
            sBubbleChains.Add(chain);
        }
        
        /** Returns if there are listeners registered for a certain event type. */
        public bool HasEventListener(string strType, bool useCapture=false)
        {
            return !useCapture && _dicObsolete != null && _dicObsolete.ContainsKey(strType)
                || useCapture && _dicCapture != null && _dicCapture.ContainsKey(strType);
        }
        //
        public void AddEventListener(string strType, EventCallback0 callback)
        {
            if (strType == null)
                throw new Exception("event type cant be null");

            if (_dic == null)
                _dic = new Dictionary<string, EventBridge>();

            EventBridge bridge = null;
            if (!_dic.TryGetValue(strType, out bridge))
            {
                bridge = new EventBridge(this);
                _dic[strType] = bridge;
            }
            bridge.Add(callback);
        }

        public void AddEventListener(string strType, EventCallback1 callback)
        {
            if (strType == null)
                throw new Exception("event type cant be null");

            if (_dic == null)
                _dic = new Dictionary<string, EventBridge>();

            EventBridge bridge = null;
            if (!_dic.TryGetValue(strType, out bridge))
            {
                bridge = new EventBridge(this);
                _dic[strType] = bridge;
            }
            bridge.Add(callback);
        }

        public void RemoveEventListener(string strType, EventCallback0 callback)
        {
            if (_dic == null)
                return;

            EventBridge bridge = null;
            if (_dic.TryGetValue(strType, out bridge))
                bridge.Remove(callback);
        }

        public void RemoveEventListener(string strType, EventCallback1 callback)
        {
            if (_dic == null)
                return;

            EventBridge bridge = null;
            if (_dic.TryGetValue(strType, out bridge))
                bridge.Remove(callback);
        }

        public void RemoveEventListeners()
        {
            RemoveEventListeners(null);
        }

        public void RemoveEventListeners(string strType)
        {
            if (_dic == null)
                return;

            if (strType != null)
            {
                EventBridge bridge;
                if (_dic.TryGetValue(strType, out bridge))
                    bridge.Clear();
            }
            else
            {
                foreach (KeyValuePair<string, EventBridge> kv in _dic)
                    kv.Value.Clear();
            }
        }

        internal EventBridge TryGetEventBridge(string strType)
        {
            if (_dic == null)
                return null;

            EventBridge bridge = null;
            _dic.TryGetValue(strType, out bridge);
            return bridge;
        }

        internal EventBridge GetEventBridge(string strType)
        {
            if (_dic == null)
                _dic = new Dictionary<string, EventBridge>();

            EventBridge bridge = null;
            if (!_dic.TryGetValue(strType, out bridge))
            {
                bridge = new EventBridge(this);
                _dic[strType] = bridge;
            }
            return bridge;
        }

        public bool DispatchEvent(string strType)
        {
            return DispatchEvent(strType, null);
        }

        public bool DispatchEvent(string strType, object data)
        {
            return InternalDispatchEvent(strType, TryGetEventBridge(strType), data);
        }

        internal bool InternalDispatchEvent(string strType, EventBridge bridge, object data)
        {
            EventBridge gBridge = null;
            if ((this is DisplayObject) && ((DisplayObject)this).gOwner != null)
                gBridge = ((DisplayObject)this).gOwner.TryGetEventBridge(strType);

            bool b1 = bridge != null && !bridge.isEmpty;
            bool b2 = gBridge != null && !gBridge.isEmpty;
            if (b1 || b2)
            {
                EventContext context = EventContext.Get();
                context.initiator = this;
                context._stopsPropagation = false;
                context._defaultPrevented = false;
                context.type = strType;
                context.data = data;

                if (b1)
                {
                    bridge.CallCaptureInternal(context);
                    bridge.CallInternal(context);
                }

                if (b2)
                {
                    gBridge.CallCaptureInternal(context);
                    gBridge.CallInternal(context);
                }

                EventContext.Return(context);
                context.initiator = null;
                context.sender = null;
                context.data = null;

                return context._defaultPrevented;
            }
            else
                return false;
        }

        public bool DispatchEvent(EventContext context)
        {
            EventBridge bridge = TryGetEventBridge(context.type);
            EventBridge gBridge = null;
            if ((this is DisplayObject) && ((DisplayObject)this).gOwner != null)
                gBridge = ((DisplayObject)this).gOwner.TryGetEventBridge(context.type);

            EventDispatcher savedSender = context.sender;

            if (bridge != null && !bridge.isEmpty)
            {
                bridge.CallCaptureInternal(context);
                bridge.CallInternal(context);
            }

            if (gBridge != null && !gBridge.isEmpty)
            {
                gBridge.CallCaptureInternal(context);
                gBridge.CallInternal(context);
            }

            context.sender = savedSender;
            return context._defaultPrevented;
        }

        public bool BubbleEvent(string strType, object data)
        {
            EventContext context = EventContext.Get();
            context.initiator = this;
            context._stopsPropagation = false;
            context._defaultPrevented = false;
            context.type = strType;
            context.data = data;
            List<EventBridge> bubbleChain = context.callChain;

            EventBridge bridge = TryGetEventBridge(strType);
            if (bridge != null && !bridge.isEmpty)
                bubbleChain.Add(bridge);

            if ((this is DisplayObject) && ((DisplayObject)this).gOwner != null)
            {
                bridge = ((DisplayObject)this).gOwner.TryGetEventBridge(strType);
                if (bridge != null && !bridge.isEmpty)
                    bubbleChain.Add(bridge);
            }

            if (this is DisplayObject)
            {
                DisplayObject element = (DisplayObject)this;
                while ((element = element.parent) != null)
                {
                    bridge = element.TryGetEventBridge(strType);
                    if (bridge != null && !bridge.isEmpty)
                        bubbleChain.Add(bridge);

                    if (element.gOwner != null)
                    {
                        bridge = element.gOwner.TryGetEventBridge(strType);
                        if (bridge != null && !bridge.isEmpty)
                            bubbleChain.Add(bridge);
                    }
                }
            }
            else if (this is GObject)
            {
                GObject element = (GObject)this;
                while ((element = element.parent) != null)
                {
                    bridge = element.TryGetEventBridge(strType);
                    if (bridge != null && !bridge.isEmpty)
                        bubbleChain.Add(bridge);
                }
            }

            int length = bubbleChain.Count;
            for (int i = length - 1; i >= 0; i--)
                bubbleChain[i].CallCaptureInternal(context);

            for (int i = 0; i < length; ++i)
            {
                bubbleChain[i].CallInternal(context);
                if (context._stopsPropagation)
                    break;
            }

            bubbleChain.Clear();
            EventContext.Return(context);
            context.initiator = null;
            context.sender = null;
            context.data = null;
            return context._defaultPrevented;
        }

        public bool BroadcastEvent(string strType, object data)
        {
            EventContext context = EventContext.Get();
            context.initiator = this;
            context._stopsPropagation = false;
            context._defaultPrevented = false;
            context.type = strType;
            context.data = data;
            List<EventBridge> bubbleChain = context.callChain;

            if (this is Container)
                GetChildEventBridges(strType, (Container)this, bubbleChain);
            else if (this is GComponent)
                GetChildEventBridges(strType, (GComponent)this, bubbleChain);

            int length = bubbleChain.Count;
            for (int i = 0; i < length; ++i)
                bubbleChain[i].CallInternal(context);

            bubbleChain.Clear();
            EventContext.Return(context);
            context.initiator = null;
            context.sender = null;
            context.data = null;
            return context._defaultPrevented;
        }

        static void GetChildEventBridges(string strType, Container container, List<EventBridge> bridges)
        {
            EventBridge bridge = container.TryGetEventBridge(strType);
            if (bridge != null)
                bridges.Add(bridge);
            if (container.gOwner != null)
            {
                bridge = container.gOwner.TryGetEventBridge(strType);
                if (bridge != null && !bridge.isEmpty)
                    bridges.Add(bridge);
            }

            int count = container.numChildren;
            for (int i = 0; i < count; ++i)
            {
                DisplayObject obj = container.GetChildAt(i);
                if (obj is Container)
                    GetChildEventBridges(strType, (Container)obj, bridges);
                else
                {
                    bridge = obj.TryGetEventBridge(strType);
                    if (bridge != null && !bridge.isEmpty)
                        bridges.Add(bridge);

                    if (obj.gOwner != null)
                    {
                        bridge = obj.gOwner.TryGetEventBridge(strType);
                        if (bridge != null && !bridge.isEmpty)
                            bridges.Add(bridge);
                    }
                }
            }
        }

        static void GetChildEventBridges(string strType, GComponent container, List<EventBridge> bridges)
        {
            EventBridge bridge = container.TryGetEventBridge(strType);
            if (bridge != null)
                bridges.Add(bridge);

            int count = container.numChildren;
            for (int i = 0; i < count; ++i)
            {
                GObject obj = container.GetChildAt(i);
                if (obj is GComponent)
                    GetChildEventBridges(strType, (GComponent)obj, bridges);
                else
                {
                    bridge = obj.TryGetEventBridge(strType);
                    if (bridge != null)
                        bridges.Add(bridge);
                }
            }
        }
    }

    public class EventTypeAndFunc
    {
        public string eventType;
        public Function f;
    }
}
