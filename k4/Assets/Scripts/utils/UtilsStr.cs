using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGUI
{
    class UtilsStr
    {

        public static Boolean StartsWith(string source, string str, Boolean ignoreCase = false)
        {
            if (source == null)
                return false;
            else if (source.Length < str.Length)
                return false;
            else
            {
                source = source.Substring(0, str.Length);
                if (!ignoreCase)
                    return source == str;
                else
                    return source.ToLower() == str.ToLower();
            }
        }

        public static Boolean EndsWith(string source, string str, Boolean ignoreCase = false)
        {
            if (source == null)
                return false;
            else if (source.Length < str.Length)
                return false;
            else
            {
                source = source.Substring(source.Length - str.Length);
                if (!ignoreCase)
                    return source == str;
                else
                    return source.ToLower() == str.ToLower();
            }
        }
        public static string ConvertToHtmlColor(uint argb, bool hasAlpha=false) 
        {
	       	string alpha;
	    	if(hasAlpha)
	    		alpha = (argb >> 24 & 0xFF).ToString("X");
	    	else
	    		alpha = "";
            string red = (argb >> 16 & 0xFF).ToString("X");
            string green = (argb >> 8 & 0xFF).ToString("X");
            string blue = (argb & 0xFF).ToString("X");
            if (alpha.Length == 1)
                alpha = "0" + alpha;
    		if(red.Length==1)
 	    		red = "0" + red;
 	    	if(green.Length==1)
 		    	green = "0" + green;
  		    if(blue.Length==1)
 			    blue = "0" + blue;
 		    return "#" + alpha + red +  green + blue;
 	    }
        public static uint ConvertFromHtmlColor(string str, bool hasAlpha = false)
        {
            if (str.Length < 1 || str[0] != '#')
                return 0;

            if (str.Length == 9)
                return (uint)((Convert.ToUInt32(str.Substring(1, 2), 16 ) << 24) + Convert.ToUInt32(str.Substring(3), 16 ));
            else if (hasAlpha)
                return (uint)(0xFF000000 + Convert.ToUInt32(str.Substring(1), 16 ));
            else
                return Convert.ToUInt32(str.Substring(1) ,16);
        }
        public static string GetFileExt(string source, bool keepCase = false)
        {
            int i = source.LastIndexOf('?');
            int j;
            string ret;
            if (i != -1)
            {
                j = source.LastIndexOf('.', i);
                if (j != -1)
                    ret = source.Substring(j + 1, i);
                else
                    ret = "";
            }
            else
            {
                j = source.LastIndexOf('.');
                if (j != -1)
                    ret = source.Substring(j + 1);
                else
                    ret = "";
            }
            if (!keepCase)
                ret = ret.ToLower();
            return ret;
        }

        public static string EncodeHTML(string str) {
		    if(str==null || str.Length==0)
			    return "";
		    else
                return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;");
	    }

        public static UBBParser DefaultUBBParser = new UBBParser();
	    public static string ParseUBB(string text) {
		    return DefaultUBBParser.Parse(text);
	    }
    }
}
