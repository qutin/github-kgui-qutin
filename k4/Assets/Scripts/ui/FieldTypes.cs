using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
 

namespace KGUI
{
    public enum AlignType
    {
        Left,
        Center,
        Right
    }

    public enum VertAlignType
    {
        Top,
        Middle,
        Bottom
    }

    public enum OverflowType
    {
        Visible,
        Hidden,
        Scroll,
        Scale,
        ScaleFree
    }

    public enum AutoSizeType
    {
        None,
        Both,
        Height
    }

    public enum ScrollType
    {
        Horizontal,
        Vertical,
        Both
    }

    public enum ScrollBarDisplayType
    {
        Default,
        Visible,
        Auto,
        Hidden
    }
    public enum ButtonMode
    {
        Common,
        Check,
        Radio
    }
    public enum RelationType
    {
        Left_Left,
		Left_Center,
		Left_Right,
		Center_Center,
		Right_Left,
		Right_Center,
		Right_Right,

		Top_Top,
		Top_Middle,
		Top_Bottom,
		Middle_Middle,
		Bottom_Top,
		Bottom_Middle,
		Bottom_Bottom,

		Width,
		Height,

		LeftExt_Left,
		LeftExt_Right,
		RightExt_Left,
		RightExt_Right,
		TopExt_Top,
		TopExt_Bottom,
		BottomExt_Top,
		BottomExt_Bottom,

		Size
    }
    public enum ListLayoutType
    {
        SingleColumn,
        SingleRow,
        FlowHorizontal,
        FlowVertical
    }

    public enum ListSelectionMode
    {
        Single,
        Multiple,
        Multiple_SingleClick,
        None
    }
    class FieldTypes
    {
        public static ButtonMode ParseButtonMode(string value)
        {
            switch (value)
            {
                case "Common":
                    return ButtonMode.Common;
                case "Check":
                    return ButtonMode.Check;
                case "Radio":
                    return ButtonMode.Radio;
                default:
                    return ButtonMode.Common;
            }
        }
        public static AlignType parseAlign(string value)
        {
            switch (value)
            {
                case "left":
                    return AlignType.Left;
                case "center":
                    return AlignType.Center;
                case "right":
                    return AlignType.Right;
                default:
                    return AlignType.Left;
            }
        }

        public static VertAlignType parseVerticalAlign(string value)
        {
            switch (value)
            {
                case "top":
                    return VertAlignType.Top;
                case "middle":
                    return VertAlignType.Middle;
                case "bottom":
                    return VertAlignType.Bottom;
                default:
                    return VertAlignType.Top;
            }
        }

        public static ScrollType parseScrollType(string value)
        {
            switch (value)
            {
                case "horizontal":
                    return ScrollType.Horizontal;
                case "vertical":
                    return ScrollType.Vertical;
                case "both":
                    return ScrollType.Both;
                default:
                    return ScrollType.Horizontal;
            }
        }

        public static ScrollBarDisplayType parseScrollBarDisplayType(string value)
        {
            switch (value)
            {
                case "default":
                    return ScrollBarDisplayType.Default;
                case "visible":
                    return ScrollBarDisplayType.Visible;
                case "auto":
                    return ScrollBarDisplayType.Auto;
                case "hidden":
                    return ScrollBarDisplayType.Hidden;
                default:
                    return ScrollBarDisplayType.Default;
            }
        }

        public static OverflowType parseOverflowType(string value)
        {
            switch (value)
            {
                case "visible":
                    return OverflowType.Visible;
                case "hidden":
                    return OverflowType.Hidden;
                case "scroll":
                    return OverflowType.Scroll;
                case "scale":
                    return OverflowType.Scale;
                case "scaleFree":
                    return OverflowType.ScaleFree;
                default:
                    return OverflowType.Visible;
            }
        }

        public static AutoSizeType parseAutoSizeType(string value)
        {
            switch (value)
            {
                case "none":
                    return AutoSizeType.None;
                case "both":
                    return AutoSizeType.Both;
                case "height":
                    return AutoSizeType.Height;
                default:
                    return AutoSizeType.None;
            }
        }
        static Dictionary<string, Ease> EaseTypeMap = new Dictionary<string, Ease>
			{
				{ "Linear", Ease.Linear },
				{ "Elastic.In", Ease.InElastic },
				{ "Elastic.Out", Ease.InOutElastic },
				{ "Elastic.InOut", Ease.InOutElastic },
				{ "Quad.In", Ease.InQuad },
				{ "Quad.Out", Ease.OutQuad },
				{ "Quad.InOut", Ease.InOutQuad },
				{ "Cube.In", Ease.InCubic },
				{ "Cube.Out", Ease.OutCubic },
				{ "Cube.InOut", Ease.InOutCubic },
				{ "Quart.In", Ease.InQuart },
				{ "Quart.Out", Ease.OutQuart },
				{ "Quart.InOut", Ease.InOutQuart },
				{ "Quint.In", Ease.InQuint },
				{ "Quint.Out", Ease.OutQuint },
				{ "Quint.InOut", Ease.InOutQuint },
				{ "Sine.In", Ease.InSine },
				{ "Sine.Out", Ease.OutSine },
				{ "Sine.InOut", Ease.InOutSine },
				{ "Bounce.In", Ease.InBounce },
				{ "Bounce.Out", Ease.OutBounce },
				{ "Bounce.InOut", Ease.InOutBounce },
				{ "Circ.In", Ease.InCirc },
				{ "Circ.Out", Ease.OutCirc },
				{ "Circ.InOut", Ease.InOutCirc },
				{ "Expo.In", Ease.InExpo },
				{ "Expo.Out", Ease.OutExpo },
				{ "Expo.InOut", Ease.InOutExpo },
				{ "Back.In", Ease.InBack },
				{ "Back.Out", Ease.OutBack },
				{ "Back.InOut", Ease.InOutBack }
			};
        public static Ease ParseEaseType(string value)
        {
            Ease type;
            if (!EaseTypeMap.TryGetValue(value, out type))
                type = Ease.OutExpo;

            return type;
        }
    }
}
