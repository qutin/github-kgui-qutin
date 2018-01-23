using System;
using System.Collections.Generic;
using UnityEngine;


namespace KGUI
{
    public class Stage : Container
    {
        private int _stageHeight;
        private int _stageWidth;
        private bool _invalidated;
        private float _mouseX;
        private float _mouseY;

        private Vector2 _lastMousePos;
        private DisplayObject _lastRollOver;
        private DisplayObject _objectUnderMouse;
        private Vector2 _lastMouseDownPos;
        private bool _isMouseDown;
        private int _clickCount;
        private DisplayObject _focused;
        private bool _ignoreNextKeydown;

        private static MouseEvent eMouseClick = new MouseEvent(MouseEvent.CLICK, true);
        private static MouseEvent eMouseDown = new MouseEvent(MouseEvent.MOUSE_DOWN, true);
        private static MouseEvent eMouseUp = new MouseEvent(MouseEvent.MOUSE_UP, true);
        private static MouseEvent eMouseOver = new MouseEvent(MouseEvent.ROLL_OVER, true);
        private static MouseEvent eMouseOut = new MouseEvent(MouseEvent.ROLL_OUT, true);
        private static MouseEvent eMouseRightClick = new MouseEvent(MouseEvent.RIGHT_CLICK, true);
        private static MouseEvent eMouseMove = new MouseEvent(MouseEvent.MOUSE_MOVE, true);
        private static MouseEvent eMouseWheel = new MouseEvent(MouseEvent.MOUSE_WHEEL, true);
        private static EventContext eResize = new EventContext(EventContext.RESIZE, false);
        private static KeyboardEvent eKeyDown = new KeyboardEvent(KeyboardEvent.KEY_DOWN, true);
        private static KeyboardEvent eKeyUp = new KeyboardEvent(KeyboardEvent.KEY_UP, true);
        private static EventContext eRender = new EventContext(EventContext.RENDER);

        static private Stage singelton = new Stage();
        static private RenderSupport support;
        static Stage _inst;

        //
        DisplayObject _touchTarget;
        TouchInfo[] _touches;
        List<DisplayObject> _rollOutChain;
        List<DisplayObject> _rollOverChain;
        int _lastHandleInputFrame;
        int _touchCount;
        //是否渲染在EditorWindow 上
        public static bool isRenderEditorWindow { get; set; }

        public static bool touchScreen { get; private set; }
        public static Vector2 touchPosition { get; private set; }
        internal static bool shiftDown { get; private set; }
        public EventListener onStageResized { get; private set; }
        public EventListener onTouchMove { get; private set; }
        public static Stage inst
        {
            get
            {
                if (_inst == null)
                    Instantiate();

                return _inst;
            }
        }
        public static void Instantiate()
        {
            if (_inst == null)
            {
                _inst = new Stage();
                //先理解差再改成这样
                //GRoot.inst = new GRoot();
                //_inst.AddChild(GRoot.inst.displayObject);
            }
        }
        public Stage()
            : base()
        {
            if (support == null)
                support = new RenderSupport();
            _stageWidth = Screen.width;
            _stageHeight = Screen.height;

            _lastMousePos = Vector2.zero;
            _lastMouseDownPos = Vector2.zero;
            _focused = this;

            //
            _lastHandleInputFrame = -1;

            if (Application.isMobilePlatform)
                touchScreen = true;

            _touches = new TouchInfo[5];
            for (int i = 0; i < _touches.Length; i++)
                _touches[i] = new TouchInfo();

            if (!touchScreen)
                _touches[0].touchId = 0;

            _rollOutChain = new List<DisplayObject>();
            _rollOverChain = new List<DisplayObject>();

            onStageResized = new EventListener(this, "onStageResized");
            onTouchMove = new EventListener(this, "onTouchMove");

        }
        public int stageHeight
        {
            get { return _stageHeight; }
            set { _stageHeight = value; }
        }
        public int stageWidth
        {
            get { return _stageWidth; }
            set { _stageWidth = value; }
        }

        public float mouseX
        {
            get { return _mouseX; }
        }
        public float mouseY
        {
            get { return _mouseY; }
        }

        public void Invalidate()
        {
            _invalidated = true;
        }

        public DisplayObject objectUnderMouse
        {
            get
            {
                if (_objectUnderMouse == this)
                    return null;
                else
                    return _objectUnderMouse;
            }
        }

        //public DisplayObject focus
        //{
        //    get {
        //        if (_focused == this)
        //            return null;
        //        else
        //            return _focused; 
        //    }
        //    set 
        //    {
        //        _focused = value;
        //        if (_focused == null)
        //            _focused = this;
        //    }
        //}
        public DisplayObject focus
        {
            get
            {
                if (_focused == this)
                    return null;
                else
                    return _focused;
            }
            set
            {
                if (_focused == value)
                    return;

                if (_focused != null)
                {
                    if ((_focused is TextField))
                        ((TextField)_focused).onFocusOut.Call();

                    _focused.onRemovedFromStage.RemoveCapture(OnFocusRemoved);
                }

                _focused = value;
                if (_focused == this)
                    _focused = null;
                if (_focused != null)
                {
                    if ((_focused is TextField))
                        ((TextField)_focused).onFocusIn.Call();

                    _focused.onRemovedFromStage.AddCapture(OnFocusRemoved);
                }
               
            }
        }

        void OnFocusRemoved(EventContext context)
        {
            if (_focused == null)
                return;

            if (context.sender == _focused)
                this.focus = null;
            else
            {
                DisplayObject currentObject = _focused.parent;
                while (currentObject != null)
                {
                    if (currentObject == context.sender)
                    {
                        this.focus = null;
                        break;
                    }
                    currentObject = currentObject.parent;
                }
            }
        }
        public void OnGUI()
        {
            Event evt = Event.current;

            int w, h;
            w = Screen.width;
            h = Screen.height;
            if (w != _stageWidth || h != _stageHeight)
            {
                _stageWidth = w;
                _stageHeight = h;
                DispatchEventObsolete(eResize);
            }

            //if(evt.type==EventType.Repaint)
            //Debug.Log(""+(Timers.time*1000) + evt.isMouse + "," + evt.type);
            bool hitTested = false;
            if (_mouseX != evt.mousePosition.x || _mouseY != evt.mousePosition.y)
            {
                _objectUnderMouse = HitTest(_lastMousePos, true);
                hitTested = true;

                _mouseX = evt.mousePosition.x;
                _mouseY = evt.mousePosition.y;

                eMouseMove.InitFrom(evt);
                DispatchEventObsolete(eMouseMove);

                _lastMousePos.x = _mouseX;
                _lastMousePos.y = _mouseY;
                if (_lastRollOver != _objectUnderMouse)
                {
                    if (_lastRollOver != null && _lastRollOver.stage != null)
                    {
                        //rollout
                        eMouseOut.InitFrom(evt);
                        _lastRollOver.DispatchEventObsolete(eMouseOut);
                    }

                    if (_objectUnderMouse != null)
                    {
                        _lastRollOver = _objectUnderMouse;
                        eMouseOver.InitFrom(evt);
                        _objectUnderMouse.DispatchEventObsolete(eMouseOver);
                    }
                }
            }

            if (evt.isMouse)
            {
                if (evt.button == 0)
                {
                    if (evt.type == EventType.MouseDown)
                    {
                        if (!_isMouseDown)
                        {
                            _isMouseDown = true;
                            if (!hitTested)
                                _objectUnderMouse = HitTest(_lastMousePos, true);

                            _clickCount = evt.clickCount;
                            _lastMouseDownPos.x = _mouseX;
                            _lastMouseDownPos.y = _mouseY;
                            if (_objectUnderMouse != null)
                            {
                                _focused = _objectUnderMouse;
                                eMouseDown.InitFrom(evt);
                                _objectUnderMouse.DispatchEventObsolete(eMouseDown);
                            }
                            else
                                _focused = this;
                        }
                    }
                    else if (evt.type == EventType.MouseUp)
                    {
                        if (_isMouseDown)
                        {
                            _isMouseDown = false;
                            if (!hitTested)
                                _objectUnderMouse = HitTest(_lastMousePos, true);

                            if (_objectUnderMouse != null)
                            {
                                eMouseUp.InitFrom(evt);
                                _objectUnderMouse.DispatchEventObsolete(eMouseUp);

                                if (Math.Abs(_mouseX - _lastMouseDownPos.x) < 10 && Math.Abs(_mouseY - _lastMouseDownPos.y) < 10)
                                {
                                    eMouseClick.InitFrom(evt);
                                    eMouseClick.ClickCount = _clickCount;
                                    _objectUnderMouse.DispatchEventObsolete(eMouseClick);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (evt.button == 1)
                    {
                        if (evt.type == EventType.MouseDown)
                        {
                            //ignore
                        }
                        else if (evt.type == EventType.MouseUp)
                        {
                            if (!hitTested)
                                _objectUnderMouse = HitTest(_lastMousePos, true);
                            if (_objectUnderMouse != null)
                            {
                                eMouseRightClick.InitFrom(evt);
                                _objectUnderMouse.DispatchEventObsolete(eMouseRightClick);
                            }
                        }
                    }
                }
            }
            else if (evt.type == EventType.scrollWheel)
            {
                if (!hitTested)
                    _objectUnderMouse = HitTest(_lastMousePos, true);
                if (_objectUnderMouse != null)
                {
                    eMouseWheel.InitFrom(evt);
                    _objectUnderMouse.DispatchEventObsolete(eMouseWheel);
                }
            }
            else if (evt.isKey)
            {
                if (evt.keyCode != KeyCode.None)
                {
                    if (evt.type == EventType.keyDown)
                    {
                        eKeyDown.InitFrom(evt);
                        _focused.HandleKeyEvent(eKeyDown);
                        if (eKeyDown.isDefaultPrevented)
                        {
                            _ignoreNextKeydown = true;
                            evt.Use();
                        }
                        else
                            _ignoreNextKeydown = false;
                    }
                    else
                    {
                        eKeyUp.InitFrom(evt);
                        _focused.HandleKeyEvent(eKeyUp);
                        _ignoreNextKeydown = false;
                    }
                }
                else
                {
                    if (_ignoreNextKeydown)
                        evt.Use();
                }
            }

            if (evt.type == EventType.Repaint || (_focused is TextField))
            {
                if (_invalidated)
                {
                    _invalidated = false;
                    DispatchEventObsolete(eRender);
                }
                if (_focused != null && _focused != this)
                    GUI.FocusControl(_focused.internalId);
                else
                    GUI.FocusControl(string.Empty);
                support.Reset();
                Render(support, 1);
            }
        }

        //
        void HandleRender()
        {
            //if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout
            //    || Event.current.type == EventType.MouseDown
            //    || Event.current.type == EventType.MouseUp
            //    || Event.current.type == EventType.MouseMove
            //    || Event.current.type == EventType.MouseDrag
            //    || Event.current.type == EventType.KeyDown
            //    || Event.current.type == EventType.KeyUp
            //    || Event.current.type == EventType.DragUpdated
            //    || Event.current.type == EventType.DragPerform
            //    || Event.current.type == EventType.DragExited
            //    || (_focused is TextField))
            if (Event.current.type == EventType.Ignore || Event.current.type == EventType.ignore)
                return;
            {
                if (_invalidated)
                {
                    _invalidated = false;
                    DispatchEventObsolete(eRender);
                }
                if (_focused != null && _focused != this)
                    GUI.FocusControl(_focused.internalId);
                else
                    GUI.FocusControl(string.Empty);

                support.Reset();
                Render(support, 1);
            }
        }
        public void HandleOnGUI()
        {
            HandleGUIEvents(Event.current);
            HandleInputEvents();
            HandleRender();
        }

        public void EditorHandleOnGUI()
        {
            HandleGUIEvents(Event.current);
            Vector2 mousePosition = Event.current.mousePosition;
            TouchInfo touch = _touches[0];
            MouseEvents(mousePosition, touch,
                Event.current.button == 0 && Event.current.type == EventType.MouseDown,
                Event.current.button == 0 && Event.current.type == EventType.mouseUp,
                Event.current.button == 1 && Event.current.type == EventType.MouseDown,
                Event.current.button == 1 && Event.current.type == EventType.mouseUp);
            HandleRender();
        }
        void HandleInputEvents()
        {
            if (_lastHandleInputFrame == Time.frameCount)
                return;

            _lastHandleInputFrame = Time.frameCount;

            //if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            //    shiftDown = false;
            //else if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            //    shiftDown = true;

            if (touchScreen)
                HandleTouchEvents();
            else
                HandleMouseEvents();
        }
        void HandleTouchEvents()
        {
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch uTouch = Input.GetTouch(i);

                if (uTouch.phase == TouchPhase.Stationary)
                    continue;

                bool hitTested = false;
                Vector2 pos = uTouch.position;
                pos.y = stageHeight - pos.y;
                TouchInfo touch = null;
                for (int j = 0; j < 5; j++)
                {
                    if (_touches[j].touchId == uTouch.fingerId)
                    {
                        touch = _touches[j];
                        break;
                    }

                    if (_touches[j].touchId == -1)
                        touch = _touches[j];
                }
                if (touch == null)
                    continue;

                touch.touchId = uTouch.fingerId;
                touchPosition = pos;

                if (touch.x != pos.x || touch.y != pos.y)
                {
                    touch.x = pos.x;
                    touch.y = pos.y;

                    _touchTarget = HitTest(pos, true);
                    hitTested = true;
                    touch.target = _touchTarget;

                    touch.UpdateEvent();
                    onTouchMove.Call(touch.evt);

                    //no rollover/rollout on mobile
                }

                if (uTouch.phase == TouchPhase.Began)
                {
                    if (!touch.began)
                    {
                        touch.began = true;
                        _touchCount++;
                        touch.clickCancelled = false;
                        touch.downX = touch.x;
                        touch.downY = touch.y;

                        if (!hitTested)
                        {
                            _touchTarget = HitTest(pos, true);
                            hitTested = true;
                            touch.target = _touchTarget;
                        }

                        this.focus = _touchTarget;

                        if (_touchTarget != null)
                        {
                            touch.UpdateEvent();
                            _touchTarget.onTouchBegin.BubbleCall(touch.evt);
                        }
                    }
                }
                else if (uTouch.phase == TouchPhase.Canceled || uTouch.phase == TouchPhase.Ended)
                {
                    if (touch.began)
                    {
                        touch.began = false;
                        _touchCount--;

                        if (!hitTested)
                        {
                            _touchTarget = HitTest(pos, true);
                            hitTested = true;
                            touch.target = _touchTarget;
                        }

                        if (_touchTarget != null)
                        {
                            touch.UpdateEvent();
                            _touchTarget.onTouchEnd.BubbleCall(touch.evt);

                            if (!touch.clickCancelled && Mathf.Abs(touch.x - touch.downX) < 50 && Mathf.Abs(touch.y - touch.downY) < 50)
                            {
                                touch.clickCount = uTouch.tapCount;
                                touch.UpdateEvent();
                                _touchTarget.onClick.BubbleCall(touch.evt);
                            }
                        }
                    }

                    touch.Reset();
                }
            }
        }

        void MouseEvents(Vector2 mousePosition, TouchInfo touch, bool mouseLeftDown, bool mouseLeftUp, bool mouseRightDown, bool mouseRightUp)
        {
            bool hitTested = false;
            if (mousePosition.x >= 0 && mousePosition.y >= 0)
            {
                if (touch.x != mousePosition.x || touch.y != mousePosition.y)
                {
                    touchPosition = mousePosition;
                    touch.x = mousePosition.x;
                    touch.y = mousePosition.y;

                    _touchTarget = HitTest(mousePosition, true);
                    hitTested = true;
                    touch.target = _touchTarget;

                    touch.UpdateEvent();
                    onTouchMove.Call(touch.evt);

                    if (touch.lastRollOver != _touchTarget)
                        HandleRollOver(touch);
                }
            }
            else
                mousePosition = touchPosition;

            if (mouseLeftDown)
            {
                if (!touch.began)
                {
                    touch.began = true;
                    _touchCount++;
                    touch.clickCancelled = false;
                    touch.downX = touch.x;
                    touch.downY = touch.y;

                    if (!hitTested)
                    {
                        _touchTarget = HitTest(mousePosition, true);
                        hitTested = true;
                        touch.target = _touchTarget;
                    }

                    this.focus = _touchTarget;

                    if (_touchTarget != null)
                    {
                        touch.UpdateEvent();
                        _touchTarget.onTouchBegin.BubbleCall(touch.evt);
                    }
                }
            }
            if (mouseLeftUp)
            {
                if (touch.began)
                {
                    touch.began = false;
                    _touchCount--;

                    if (!hitTested)
                    {
                        _touchTarget = HitTest(mousePosition, true);
                        hitTested = true;
                        touch.target = _touchTarget;
                    }

                    if (_touchTarget != null)
                    {
                        touch.UpdateEvent();
                        _touchTarget.onTouchEnd.BubbleCall(touch.evt);

                        if (!touch.clickCancelled && Mathf.Abs(touch.x - touch.downX) < 50 && Mathf.Abs(touch.y - touch.downY) < 50)
                        {
                            if (ToolSet.RealtimeSinceStartup() - touch.lastClickTime < 0.35f)
                            {
                                if (touch.clickCount == 2)
                                    touch.clickCount = 1;
                                else
                                    touch.clickCount++;
                            }
                            else
                                touch.clickCount = 1;
                            touch.lastClickTime = ToolSet.RealtimeSinceStartup();
                            touch.UpdateEvent();
                            _touchTarget.onClick.BubbleCall(touch.evt);
                        }
                    }
                }
            }
            if (mouseRightDown)
            {
                if (!hitTested)
                {
                    _touchTarget = HitTest(mousePosition, true);
                    hitTested = true;
                    touch.target = _touchTarget;
                }

                if (_touchTarget != null)
                {
                    touch.UpdateEvent();
                    _touchTarget.onRightClick.BubbleCall(touch.evt);
                }
            }
        }
        void HandleMouseEvents()
        {
            Vector2 mousePosition = Input.mousePosition;
            mousePosition.y = stageHeight - mousePosition.y;
            TouchInfo touch = _touches[0];
            MouseEvents(mousePosition, touch, Input.GetMouseButtonDown(0), Input.GetMouseButtonUp(0), Input.GetMouseButtonDown(1), Input.GetMouseButtonUp(1));
        }
        void HandleRollOver(TouchInfo touch)
        {
            DisplayObject element;
            element = touch.lastRollOver;
            while (element != null)
            {
                _rollOutChain.Add(element);
                element = element.parent;
            }

            touch.lastRollOver = touch.target;

            element = touch.target;
            int i;
            while (element != null)
            {
                i = _rollOutChain.IndexOf(element);
                if (i != -1)
                {
                    _rollOutChain.RemoveRange(i, _rollOutChain.Count - i);
                    break;
                }
                _rollOverChain.Add(element);

                element = element.parent;
            }

            int cnt = _rollOutChain.Count;
            if (cnt > 0)
            {
                for (i = 0; i < cnt; i++)
                {
                    element = _rollOutChain[i];
                    if (element.stage != null)
                        element.onRollOut.Call();
                }
                _rollOutChain.Clear();
            }

            cnt = _rollOverChain.Count;
            if (cnt > 0)
            {
                for (i = 0; i < cnt; i++)
                {
                    element = _rollOverChain[i];
                    if (element.stage != null)
                        element.onRollOver.Call();
                }
                _rollOverChain.Clear();
            }
        }

        void HandleGUIEvents(Event evt)
        {
            if (evt.rawType == EventType.KeyDown && evt.keyCode != KeyCode.None)
            {
                if (evt.keyCode == KeyCode.LeftShift || evt.keyCode == KeyCode.RightShift)
                    shiftDown = true;

                TouchInfo touch = _touches[0];
                touch.keyCode = evt.keyCode;
                touch.modifiers = evt.modifiers;

                touch.UpdateEvent();
                DisplayObject f = this.focus;
                if (f != null)
                    f.onKeyDown.BubbleCall(touch.evt);
                else
                    this.onKeyDown.Call(touch.evt);
            }
            else if (evt.rawType == EventType.KeyUp)
            {
                if (evt.keyCode == KeyCode.LeftShift || evt.keyCode == KeyCode.RightShift)
                    shiftDown = false;

                TouchInfo touch = _touches[0];
                touch.modifiers = evt.modifiers;
            }
            else if (evt.type == EventType.scrollWheel)
            {
                if (_touchTarget != null)
                {
                    TouchInfo touch = _touches[0];
                    touch.mouseWheelDelta = (int)evt.delta.y;
                    touch.UpdateEvent();
                    _touchTarget.onMouseWheel.BubbleCall(touch.evt);
                }
            }
        }
        public Vector2 GetTouchPosition(int touchId)
        {
            if (touchId < 0)
                return touchPosition;

            for (int j = 0; j < 5; j++)
            {
                TouchInfo touch = _touches[j];
                if (touch.touchId == touchId)
                    return new Vector2(touch.x, touch.y);
            }

            return touchPosition;
        }
        public int touchCount
        {
            get { return _touchCount; }
        }
        public void ResetInputState()
        {
            for (int j = 0; j < 5; j++)
            {
                TouchInfo touch = _touches[j];
                touch.Reset();
            }

            if (!touchScreen)
                _touches[0].touchId = 0;
        }
        public void CancelClick(int touchId)
        {
            for (int j = 0; j < 5; j++)
            {
                TouchInfo touch = _touches[j];
                if (touch.touchId == touchId)
                    touch.clickCancelled = true;
            }
        }

        public DisplayObject touchTarget
        {
            get
            {
                if (isRenderEditorWindow)
                {

                }
                else
                {
                    if (_lastHandleInputFrame != Time.frameCount)
                        HandleInputEvents();
                }

                if (_touchTarget == this)
                    return null;
                else
                    return _touchTarget;
            }
        }

    }
    class TouchInfo
    {
        public float x;
        public float y;
        public int touchId;
        public int clickCount;
        public KeyCode keyCode;
        public EventModifiers modifiers;
        public int mouseWheelDelta;

        public float downX;
        public float downY;
        public bool began;
        public bool clickCancelled;
        public float lastClickTime;
        public DisplayObject target;
        public DisplayObject lastRollOver;

        public InputEvent evt;

        public TouchInfo()
        {
            evt = new InputEvent();
            Reset();
        }

        public void Reset()
        {
            touchId = -1;
            x = 0;
            y = 0;
            clickCount = 0;
            keyCode = KeyCode.None;
            modifiers = 0;
            mouseWheelDelta = 0;
            lastClickTime = 0;
            began = false;
            target = null;
            lastRollOver = null;
            clickCancelled = false;
        }

        public void UpdateEvent()
        {
            evt.touchId = this.touchId;
            evt.x = this.x;
            evt.y = this.y;
            evt.clickCount = this.clickCount;
            evt.keyCode = this.keyCode;
            evt.modifiers = this.modifiers;
            evt.mouseWheelDelta = this.mouseWheelDelta;
        }
    }
}
