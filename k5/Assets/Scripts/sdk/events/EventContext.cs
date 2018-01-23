using System;
using System.Collections.Generic;

namespace KGUI
{
    public class EventContext
    {
        public const string RESIZE = "resize";
        public const string COMPLETE = "complete";
        public const string ERROR = "error";
        public const string RENDER = "render";
        public const string ADDED_TO_STAGE = "addedToStage";
        public const string REMOVED_FROM_STAGE = "removedFromStage";
        public const string CHANGE = "change";

        private string _type;
        private EventDispatcher _target;
        private EventDispatcher _sender;
        private bool _bubbles;
        //
        internal bool _stopsPropagation;
        internal bool _defaultPrevented;
        internal List<EventBridge> callChain = new List<EventBridge>();
       
         public EventContext( )
        { }
        public EventContext(string str, bool bubbles=false)
        {
            _type = str;
            _bubbles = bubbles;
        }

        public string Type
        {
            get
            {
                return _type;
            }        
        }

        public EventDispatcher Sender
        {
            get
            {
                return _sender;
            }
        }
        public EventDispatcher Target
        {
            get
            {
                return _target;
            }
        }

        public bool Bubbles
        {
            get
            {
                return _bubbles;
            }
        }

        /** Prevents listeners at the next bubble stage from receiving the event. */
        public void StopPropagation()
        {
            _stopsPropagation = true;            
        }

        public void PreventDefault()
        {
            _defaultPrevented = true;
        }

        public bool isDefaultPrevented
        {
            get { return _defaultPrevented; }
        }

        internal void SetTarget(EventDispatcher value) { _target = value; }
        
        internal void SetSender(EventDispatcher value) { _sender = value; }

       

        /** @private */
        internal bool StopsPropagation { get { return _stopsPropagation; } set { _stopsPropagation = value; } }

        internal void SetPreventDefault(bool value) { _defaultPrevented = value; }
        //
        //
        public object initiator { get; internal set; }
        public EventDispatcher sender
        {
            get { return _sender; }
            internal set { _sender = value; }
        }
        public string type
        {
            get { return _type; }
            set { _type = value; }
        }
        public object data { get; set; }
        public InputEvent inputEvent
        {
            get { return (InputEvent)data; }
        }

        static Stack<EventContext> pool = new Stack<EventContext>();
        internal static EventContext Get()
        {
            if (pool.Count > 0)
                return pool.Pop();
            else
                return new EventContext();
        }

        internal static void Return(EventContext value)
        {
            pool.Push(value);
        }
    }
}
