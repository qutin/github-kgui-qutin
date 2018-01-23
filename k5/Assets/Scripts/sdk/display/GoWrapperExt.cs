using UnityEngine;

namespace KGUI
{
    public class GoWrapperExt : GoWrapper
    {
        public GoWrapperExt(RenderCallback onGUI = null, DisposeCallback onDispose = null) : base(onGUI, onDispose)
        { }
        public override void Render(RenderSupport support, float parentAlpha)
        {
           
            Rect rect = support.GetNativeDrawRect(this.width, this.height);
            GUI.BeginGroup(rect);
            if (Event.current.type == EventType.Ignore)
            {
                GUI.EndGroup();
                return;
            }
            if (this._onGUI != null)
                this._onGUI(this, rect);
            GUI.EndGroup();
        }
    }
}
