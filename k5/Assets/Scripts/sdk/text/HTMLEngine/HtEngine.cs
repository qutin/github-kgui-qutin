/// The modified version of this software is Copyright (C) 2013 ZHing.
/// The original version's copyright as below.

/* Copyright (C) 2012 Ruslan A. Abdrashitov

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions 
of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE. */

using System;

namespace HTMLEngine
{
    public class HtEngine
    {
        internal static HtDevice Device = new GenericDevice();
        internal static HtLogger Logger = new ConsoleLogger();

        /// <summary>
        /// Change this to some other HtLogLevel value, if do not need debug messages
        /// </summary>
        public static HtLogLevel LogLevel = HtLogLevel.Debug;

        /// <summary>
        /// Default text color
        /// </summary>
        public static HtColor DefaultColor = HtColor.white;

				/// <summary>
				/// Link hover color
				/// </summary>
        public static HtColor LinkHoverColor = HtColor.blue;

				/// <summary>
				/// Link pressed factor.
				/// </summary>
        public static float LinkPressedFactor = 0.6f;

				/// <summary>
				/// Link function name.
				/// </summary>
        public static string LinkFunctionName = "onLinkClicked";

        /// <summary>
        /// Default font face
        /// </summary>
        public static string DefaultFontFace = "default";

        /// <summary>
        /// Default font size
        /// </summary>
        public static int DefaultFontSize = 16;

        /// <summary>
        /// Color of the &lt;a href...&gt; tags
        /// </summary>
        public static HtColor DefaultLinkColor = HtColor.yellow;

        public static void RegisterDevice(HtDevice device)
        {
						if (Device != null) Device.OnRelease();
            Device = device;
        }
        public static void RegisterLogger(HtLogger logger)
        {
            Logger = logger;
        }

        public static HtCompiler GetCompiler() { return OP<HtCompiler>.Acquire(); }

        internal static void Log(HtLogLevel level, string format, params object[] args) { Logger.Log(level, string.Format(format, args)); }

        internal class GenericFont : HtFont
        {
            public GenericFont(string face, int size, bool bold, bool italic) : base(face, size, bold, italic) { }
            public override int LineSpacing { get { return this.Size; } }
            public override int WhiteSize { get { return this.Size/2; } }
            public override HtSize Measure(string text) { return new HtSize(text.Length*this.WhiteSize, this.Size); }
            public override void Draw(string id, HtRect rect, HtColor color, string text, bool isEffect, Core.DrawTextEffect effect, HtColor effectColor, int effectAmount, string linkText, object userData) {
              Console.WriteLine("DrawText: {0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}", this, id, rect, color, text, isEffect, effect, effectColor, effectAmount, linkText, userData);
            }
        }

        internal class GenericImage : HtImage
        {
            public override int Width { get { return 32; } }
            public override int Height { get { return 32; } }
            public override void Draw(string id, HtRect rect, HtColor color, string linkText, object userData) {
              Console.WriteLine("DrawImage {0} {1} {2} {3} {4} {5}", this, id, rect, color, linkText, userData);
            }
        }

        internal class GenericDevice : HtDevice
        {
            public override HtFont LoadFont(string face, int size, bool bold, bool italic) { return new GenericFont(face, size, bold, italic); }
            public override HtImage LoadImage(string src, int fps) { return new GenericImage(); }
            public override void FillRect(HtRect rect, HtColor color, object userData) { Console.WriteLine("FillRect {0} {1} {2}", rect, color, userData); }
            public override void OnRelease() { Console.WriteLine("OnRelease"); }
        }

        internal class ConsoleLogger : HtLogger
        {
            public override void Log(HtLogLevel level, string message) { Console.WriteLine("{0} : {1}", level, message); }
        }
    }
}