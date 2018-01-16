namespace Core
{
    using System;
    using System.Text;

    public class JsonText : IRenderJson
    {
        private static JsonText m_EmptyArray = new JsonText("[]");
        private static JsonText m_EmptyObject = new JsonText("{}");
        private static JsonText m_Null = new JsonText("null");
        private string m_Value;

        public JsonText(string value)
        {
            this.m_Value = value;
        }

        void IRenderJson.RenderJson(StringBuilder builder)
        {
            builder.Append((this.m_Value == null) ? "null" : this.m_Value);
        }

        public static JsonText EmptyArray
        {
            get
            {
                return m_EmptyArray;
            }
        }

        public static JsonText EmptyObject
        {
            get
            {
                return m_EmptyObject;
            }
        }

        public static JsonText Null
        {
            get
            {
                return m_Null;
            }
        }
    }
}

