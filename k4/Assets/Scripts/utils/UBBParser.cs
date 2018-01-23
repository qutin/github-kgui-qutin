using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGUI
{
    public class UBBParser
    {
        public static UBBParser inst = new UBBParser();

        private string iText;
		private int iReadPos;

        protected Dictionary<string, handler> iHandlers;
		
		public int smallFontSize = 12;
		public int normalFontSize = 14;
		public int largeFontSize = 16;
		
		public int defaultImgWidth = 0;
		public int defaultImgHeight = 0;

        protected delegate string handler(string tagName, bool end, string attr);
		
		public UBBParser()
		{
            iHandlers = new Dictionary<string, handler>();
			iHandlers["URL"] = onTag_URL;
			iHandlers["IMG"] = onTag_IMG;
			iHandlers["B"] = onTag_Simple;
			iHandlers["I"] = onTag_Simple;
			iHandlers["U"] = onTag_Simple;
			iHandlers["SUP"] = onTag_Simple;
			iHandlers["SUB"] = onTag_Simple;
			iHandlers["COLOR"] = onTag_COLOR;
			iHandlers["FONT"] = onTag_FONT;
			iHandlers["SIZE"] = onTag_SIZE;
			iHandlers["MOVE"] = onTag_MOVE;
			iHandlers["FLY"] = onTag_FLY;
		}
		
		protected string onTag_URL(string tagName, bool end, string attr) {
			if(!end) {
				if(attr!=null)
					return "<a href=\"" + attr + "\" target=\"_blank\">";
				else {
					var href = getTagText();
					return "<a href=\"" + href + "\" target=\"_blank\">";
				}
			}
			else
				return "</a>";
		}
		
		protected string onTag_IMG(string tagName, bool end, string attr) {
			if(!end) {
				var src = getTagText(true);
				if(src==null || src.Length==0)
					return null;
				
				if(defaultImgWidth!=0)
					return "<img src=\"" + src + "\" width=\"" + defaultImgWidth + "\" height=\"" + defaultImgHeight + "\"/>";
				else
					return "<img src=\"" + src + "\"/>";
			}
			else
				return null;
		}
		
		protected string onTag_Simple(string tagName, bool end, string attr) {
			return end?("</"+tagName+">"):("<" + tagName + ">");
		}
		
		protected string onTag_COLOR(string tagName, bool end, string attr) {
			if(!end)
				return "<font color=\"" + attr + "\">";
			else
				return "</font>";
		}
		
		protected string onTag_FONT(string tagName, bool end, string attr) {
			if(!end)
				return "<font face=\"" + attr + "\">";
			else
				return "</font>";
		}
		
		protected string onTag_SIZE(string tagName, bool end, string attr) {
            if (!end)
            {
                if (attr == "normal")
                    attr = "" + normalFontSize;
                else if (attr == "small")
                    attr = "" + smallFontSize;
                else if (attr == "large")
                    attr = "" + largeFontSize;
                else if (attr.Length > 0 && attr[0] == '+')
                    attr = "" + (smallFontSize + int.Parse(attr.Substring(1)));
                else if (attr.Length > 0 && attr[0] == '-')
                    attr = "" + (smallFontSize - int.Parse(attr.Substring(1)));
                return "<font size=\""+ attr + "\">";
            }
            else
                return "</font>";
		}
		
		protected string onTag_MOVE(string tagName, bool end, string attr) {
            if (!end)
                return "<marquee scrollamount=\"3\">";
            else
                return "</marquee>";
		}
		
		protected string onTag_FLY(string tagName, bool end, string attr) {
			if(!end)
				return "<marquee behavior=\"alternate\" scrollamount=\"3\">";
			else
				return "</marquee>";
		}

        private string asSubstring(string str, int fromIndex, int toIndex)
        {
            return str.Substring(fromIndex, toIndex - fromIndex);
        }
		
		protected string getTagText(bool remove=false) {
			int pos = iText.IndexOf("[", iReadPos);
			if(pos==-1)
				return null;

            var ret = asSubstring(iText,iReadPos, pos);
			if(remove)
				iReadPos = pos;
			return ret;
		}		
		
		public string Parse(string text) {
			iText = text;
			int pos1 = 0, pos2, pos3;
			bool end;
			string tag, attr;
			string repl;
			handler func;
			while((pos2=iText.IndexOf("[", pos1))!=-1) {
				pos1 = pos2;
                pos2 = iText.IndexOf("]", pos1);
				if(pos2==-1)
					break;
				
				end = iText[pos1+1]=='/';
                tag = asSubstring(iText, end ? pos1 + 2 : pos1 + 1, pos2);
				pos2++;
				iReadPos = pos2;
				attr = null;
				repl = null;
				pos3 = tag.IndexOf("=");
				if(pos3!=-1) {
                    attr = tag.Substring(pos3 + 1);
                    tag = asSubstring(tag, 0, pos3);
				}
                tag = tag.ToUpper();
				func = iHandlers[tag];
				if(func!=null) {
					repl = func(tag, end, attr);
					if(repl==null)
						repl = "";
				}
				else {
					pos1 = pos2;
					continue;
				}
                iText = asSubstring(iText, 0, pos1) + repl + iText.Substring(iReadPos);
			}
			return iText;
		}
    }
}
