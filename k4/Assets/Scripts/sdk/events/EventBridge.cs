using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KGUI
{
	class EventBridge
	{
		EventDispatcher _owner;
		EventCallback0 _callback0;
		EventCallback1 _callback1;
		EventCallback1 _captureCallback;
		internal bool _dispatching;

		public EventBridge(EventDispatcher owner)
		{
			_owner = owner;
		}

		public void AddCapture(EventCallback1 callback)
		{
			_captureCallback -= callback;
			_captureCallback += callback;
		}

		public void RemoveCapture(EventCallback1 callback)
		{
			_captureCallback -= callback;
		}

		public void Add(EventCallback1 callback)
		{
			_callback1 -= callback;
			_callback1 += callback;
		}

		public void Remove(EventCallback1 callback)
		{
			_callback1 -= callback;
		}

		public void Add(EventCallback0 callback)
		{
			_callback0 -= callback;
			_callback0 += callback;
		}

		public void Remove(EventCallback0 callback)
		{
			_callback0 -= callback;
		}

		public bool isEmpty
		{
			get { return _callback1 == null && _callback0 == null && _captureCallback == null; }
		}

		public void Clear()
		{
			_callback1 = null;
			_callback0 = null;
			_captureCallback = null;
		}

		public void CallInternal(EventContext context)
		{
			if (_dispatching)
				return;

			_dispatching = true;
			context.sender = _owner;
			try
			{
				if (_callback1 != null)
					_callback1(context);
				if (_callback0 != null)
					_callback0();
			}
			finally
			{
				_dispatching = false;
			}
		}

		public void CallCaptureInternal(EventContext context)
		{
			if (_captureCallback == null)
				return;

			if (_dispatching)
				return;

			_dispatching = true;
			context.sender = _owner;
			try
			{
				_captureCallback(context);
			}
			finally
			{
				_dispatching = false;
			}
		}
	}
}
