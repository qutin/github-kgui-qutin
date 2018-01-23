using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KGUI
{
	public class EventListener
	{
		public EventDispatcher owner { get; private set; }

		EventBridge _bridge;
		string _type;

		public EventListener(EventDispatcher owner, string type)
		{
			this.owner = owner;
			this._type = type;
		}

		public string type
		{
			get { return _type; }
		}

		public void AddCapture(EventCallback1 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.AddCapture(callback);
		}

		public void RemoveCapture(EventCallback1 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.RemoveCapture(callback);
		}

		public void Add(EventCallback1 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Add(callback);
		}

		public void Remove(EventCallback1 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Remove(callback);
		}

		public void Add(EventCallback0 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Add(callback);
		}

		public void Remove(EventCallback0 callback)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Remove(callback);
		}

		public bool isEmpty
		{
			get
			{
				if (_bridge == null)
					_bridge = this.owner.GetEventBridge(_type);
				return _bridge.isEmpty;
			}
		}

		public bool isDispatching
		{
			get
			{
				if (_bridge == null)
					_bridge = this.owner.GetEventBridge(_type);
				return _bridge._dispatching;
			}
		}

		public void Clear()
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			_bridge.Clear();
		}

		public bool Call()
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			return owner.InternalDispatchEvent(this._type, _bridge, null);
		}

		public bool Call(object data)
		{
			if (_bridge == null)
				_bridge = this.owner.GetEventBridge(_type);

			return owner.InternalDispatchEvent(this._type, _bridge, data);
		}

		public bool BubbleCall(object data)
		{
			return owner.BubbleEvent(_type, data);
		}

		public bool BubbleCall()
		{
			return owner.BubbleEvent(_type, null);
		}

		public bool BroadcastCall(object data)
		{
			return owner.BroadcastEvent(_type, data);
		}

		public bool BroadcastCall()
		{
			return owner.BroadcastEvent(_type, null);
		}
	}
}
