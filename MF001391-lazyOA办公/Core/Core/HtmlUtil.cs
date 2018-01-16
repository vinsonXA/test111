namespace Core
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    public static class HtmlUtil
    {
        private static Hashtable AllowHtmlTag = new Hashtable();
        private static Regex HtmlTagRegex = new Regex(@"<(\/|)([^ \f\n\r\t\v\<\>\/]+)(\s[^\<\>]*|)(\/|)>");
        private static Regex ImpermitWordRegex = new Regex("([^a-zA-Z1-9_])(on|expression|javascript)", RegexOptions.IgnoreCase);

        static HtmlUtil()
        {
            AllowHtmlTag.Add("I", "I");
            AllowHtmlTag.Add("B", "B");
            AllowHtmlTag.Add("U", "U");
            AllowHtmlTag.Add("P", "P");
            AllowHtmlTag.Add("TH", "TH");
            AllowHtmlTag.Add("TD", "TD");
            AllowHtmlTag.Add("TR", "TR");
            AllowHtmlTag.Add("OL", "OL");
            AllowHtmlTag.Add("UL", "UL");
            AllowHtmlTag.Add("LI", "LI");
            AllowHtmlTag.Add("BR", "BR");
            AllowHtmlTag.Add("H1", "H1");
            AllowHtmlTag.Add("H2", "H2");
            AllowHtmlTag.Add("H3", "H3");
            AllowHtmlTag.Add("H4", "H4");
            AllowHtmlTag.Add("H5", "H5");
            AllowHtmlTag.Add("H6", "H6");
            AllowHtmlTag.Add("H7", "H7");
            AllowHtmlTag.Add("EM", "EM");
            AllowHtmlTag.Add("PRE", "PRE");
            AllowHtmlTag.Add("DIV", "DIV");
            AllowHtmlTag.Add("IMG", "IMG");
            AllowHtmlTag.Add("CITE", "CITE");
            AllowHtmlTag.Add("SPAN", "SPAN");
            AllowHtmlTag.Add("FONT", "FONT");
            AllowHtmlTag.Add("CODE", "CODE");
            AllowHtmlTag.Add("TABLE", "TABLE");
            AllowHtmlTag.Add("TBODY", "TBODY");
            AllowHtmlTag.Add("SMALL", "SMALL");
            AllowHtmlTag.Add("THEAD", "THEAD");
            AllowHtmlTag.Add("CENTER", "CENTER");
            AllowHtmlTag.Add("STRONG", "STRONG");
            AllowHtmlTag.Add("BLOCKQUOTE", "BLOCKQUOTE");
        }

        public static string ReplaceHtml(string text)
        {
            return HtmlTagRegex.Replace(text, new MatchEvaluator(HtmlUtil.ReplaceHtmlTag));
        }

        public static string ReplaceHtmlTag(Match match)
        {
            if (match.Groups[1].Value == "/")
            {
                if (AllowHtmlTag.ContainsKey(match.Groups[2].Value.ToUpper()))
                {
                    return match.Value;
                }
                return ("&lt;/" + match.Groups[2].Value + "&gt;");
            }
            if (AllowHtmlTag.ContainsKey(match.Groups[2].Value.ToUpper()))
            {
                return ImpermitWordRegex.Replace(match.Value, new MatchEvaluator(HtmlUtil.ReplaceImpermitWord));
            }
            return ("&lt;" + match.Groups[2].Value + "&gt;");
        }

        public static string ReplaceImpermitWord(Match match)
        {
            return (match.Groups[1].Value + "_" + match.Groups[2].Value);
        }
    }
}

