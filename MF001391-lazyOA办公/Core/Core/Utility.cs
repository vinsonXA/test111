namespace Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Net.Json;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Configuration;
    using WC.Tool;

    public class Utility
    {
        private static DateTime BaseDateTime = new DateTime(0x7b2, 1, 1, 0, 0, 0);

        public static string DataTableToJSON(DataTable dt, string Name)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            if (dt.Rows.Count > 0)
            {
                builder.AppendFormat("\"{0}\":", Name);
                builder.Append("[");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(",");
                    }
                    RenderJson(builder, dt.Rows[i]);
                }
                builder.Append("]");
            }
            builder.Append("}");
            return builder.ToString();
        }

        public static string Decode(string s)
        {
            Regex regex = new Regex(@"\\u[0-9a-fA-F]{4}|\\x[0-9a-fA-F]{2}");
            MatchEvaluator evaluator = new MatchEvaluator(Utility.ReplaceChar);
            return regex.Replace(s, evaluator);
        }

        public static System.Configuration.Configuration GetConfig()
        {
            return WebConfigurationManager.OpenWebConfiguration((HttpContext.Current.Request.ApplicationPath == "/") ? "/Lesktop" : (HttpContext.Current.Request.ApplicationPath + "/Lesktop"));
        }

        public static string MD5(string str)
        {
            return Encrypt.MD5_32(str);
        }

        private static object ParseJson(JsonObject jsonObject)
        {
            Type type = jsonObject.GetType();
            if (type == typeof(JsonObjectCollection))
            {
                Hashtable hashtable = new Hashtable();
                foreach (JsonObject obj2 in jsonObject as JsonObjectCollection)
                {
                    hashtable.Add(obj2.Name, ParseJson(obj2));
                }
                if (hashtable.ContainsKey("__DataType"))
                {
                    if ((hashtable["__DataType"] as string) == "Date")
                    {
                        return BaseDateTime.AddMilliseconds((double) hashtable["__Value"]);
                    }
                    if ((hashtable["__DataType"] as string) == "Exception")
                    {
                        return new Exception((hashtable["__Value"] as Hashtable)["Message"] as string);
                    }
                }
                return hashtable;
            }
            if (type == typeof(JsonArrayCollection))
            {
                List<object> list = new List<object>();
                foreach (JsonObject obj2 in jsonObject as JsonArrayCollection)
                {
                    list.Add(ParseJson(obj2));
                }
                return list.ToArray();
            }
            if (type == typeof(JsonBooleanValue))
            {
                return jsonObject.GetValue();
            }
            if (type == typeof(JsonStringValue))
            {
                return jsonObject.GetValue();
            }
            if (type == typeof(JsonNumericValue))
            {
                return jsonObject.GetValue();
            }
            return null;
        }

        public static object ParseJson(string jsonText)
        {
            if (jsonText == "{}")
            {
                return new Hashtable();
            }
            if (!string.IsNullOrEmpty(jsonText))
            {
                JsonTextParser parser = new JsonTextParser();
                return ParseJson(parser.Parse(jsonText));
            }
            return null;
        }

        public static string RenderHashJson(params object[] ps)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            for (int i = 0; i < ps.Length; i += 2)
            {
                if (i > 0)
                {
                    builder.Append(",");
                }
                builder.AppendFormat("\"{0}\":", ps[i].ToString());
                RenderJson(builder, ps[i + 1]);
            }
            builder.Append("}");
            return builder.ToString();
        }

        public static void RenderHashJson(StringBuilder builder, params object[] ps)
        {
            builder.Append("{");
            for (int i = 0; i < ps.Length; i += 2)
            {
                if (i > 0)
                {
                    builder.Append(",");
                }
                builder.AppendFormat("\"{0}\":", ps[i].ToString());
                RenderJson(builder, ps[i + 1]);
            }
            builder.Append("}");
        }

        public static string RenderJson(object obj)
        {
            StringBuilder builder = new StringBuilder();
            RenderJson(builder, obj);
            return builder.ToString();
        }

        public static void RenderJson(StringBuilder builder, object obj)
        {
            if (obj == null)
            {
                builder.Append("null");
            }
            else if (obj is IRenderJson)
            {
                (obj as IRenderJson).RenderJson(builder);
            }
            else if (obj is Exception)
            {
                builder.AppendFormat("{{\"__DataType\":\"Exception\",\"__Value\":{{\"Name\":\"{0}\",\"Message\":\"{1}\"}}}}", obj.GetType().Name, TransferCharJavascript((obj as Exception).Message));
            }
            else if (obj.GetType() == typeof(DateTime))
            {
                DateTime time = (DateTime) obj;
                object[] ps = new object[4];
                ps[0] = "__DataType";
                ps[1] = "Date";
                ps[2] = "__Value";
                TimeSpan span = (TimeSpan) (time - BaseDateTime);
                ps[3] = span.TotalMilliseconds;
                RenderHashJson(builder, ps);
            }
            else
            {
                int num;
                if (obj is IDictionary)
                {
                    num = 0;
                    builder.Append("{");
                    foreach (DictionaryEntry entry in obj as IDictionary)
                    {
                        if (num > 0)
                        {
                            builder.Append(",");
                        }
                        builder.AppendFormat("\"{0}\":", TransferCharJavascript(entry.Key.ToString()));
                        RenderJson(builder, entry.Value);
                        num++;
                    }
                    builder.Append("}");
                }
                else if (obj is IList)
                {
                    IList list = obj as IList;
                    builder.Append("[");
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (i > 0)
                        {
                            builder.Append(",");
                        }
                        RenderJson(builder, list[i]);
                    }
                    builder.Append("]");
                }
                else if (obj is ICollection)
                {
                    ICollection is2 = obj as ICollection;
                    builder.Append("[");
                    num = 0;
                    foreach (object obj2 in is2)
                    {
                        if (num > 0)
                        {
                            builder.Append(",");
                        }
                        RenderJson(builder, obj2);
                        num++;
                    }
                    builder.Append("]");
                }
                else if (obj is DataRow)
                {
                    DataRow row = obj as DataRow;
                    builder.Append("{");
                    num = 0;
                    foreach (DataColumn column in row.Table.Columns)
                    {
                        if (num > 0)
                        {
                            builder.Append(",");
                        }
                        builder.AppendFormat("\"{0}\":", column.ColumnName);
                        RenderJson(builder, row[column.ColumnName]);
                        num++;
                    }
                    builder.Append("}");
                }
                else if (obj is Rectangle)
                {
                    Rectangle rectangle = (Rectangle) obj;
                    RenderHashJson(builder, new object[] { "Left", rectangle.Left, "Top", rectangle.Top, "Width", rectangle.Width, "Height", rectangle.Height });
                }
                else if (((((obj is ushort) || (obj is uint)) || ((obj is ulong) || (obj is short))) || (((obj is int) || (obj is long)) || ((obj is double) || (obj is decimal)))) || (obj is long))
                {
                    builder.Append(obj.ToString());
                }
                else if (obj is bool)
                {
                    builder.Append(((bool) obj) ? "true" : "false");
                }
                else
                {
                    builder.Append("\"");
                    builder.Append(TransferCharJavascript(obj.ToString()));
                    builder.Append("\"");
                }
            }
        }

        public static string ReplaceChar(Match match)
        {
            ushort num;
            if (match.Length == 4)
            {
                num = ushort.Parse(match.Value.Substring(2, 2), NumberStyles.HexNumber);
            }
            else
            {
                num = ushort.Parse(match.Value.Substring(2, 4), NumberStyles.HexNumber);
            }
            return Convert.ToChar(num).ToString();
        }

        public static unsafe Bitmap ToGray(Bitmap bmp, int mode)
        {
            if (bmp == null)
            {
                return null;
            }
            int width = bmp.Width;
            int height = bmp.Height;
            try
            {
                byte num3 = 0;
                BitmapData bitmapdata = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                byte* numPtr = (byte*) bitmapdata.Scan0.ToPointer();
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        if (mode == 0)
                        {
                            num3 = (byte) (((numPtr[0] * 0.114f) + (numPtr[1] * 0.587f)) + (numPtr[2] * 0.299f));
                        }
                        else
                        {
                            num3 = (byte) (((float) ((numPtr[0] + numPtr[1]) + numPtr[2])) / 3f);
                        }
                        numPtr[0] = num3;
                        numPtr[1] = num3;
                        numPtr[2] = num3;
                        numPtr += 3;
                    }
                    numPtr += bitmapdata.Stride - (width * 3);
                }
                bmp.UnlockBits(bitmapdata);
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        public static string TransferCharForXML(string s)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char ch in s)
            {
                switch (ch)
                {
                    case '<':
                    case '>':
                    case '\\':
                    case '\'':
                    case '\t':
                    case '\n':
                    case '\v':
                    case '\f':
                    case '\r':
                    case '"':
                    {
                        builder.AppendFormat("&#{0};", (int) ch);
                        continue;
                    }
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static string TransferCharJavascript(string s)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char ch in s)
            {
                switch (ch)
                {
                    case '<':
                    case '>':
                    case '\\':
                    case '\'':
                    case '\t':
                    case '\n':
                    case '\v':
                    case '\f':
                    case '\r':
                    case '"':
                    case '\0':
                    {
                        builder.AppendFormat(@"\u{0:X4}", (int) ch);
                        continue;
                    }
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }
    }
}

