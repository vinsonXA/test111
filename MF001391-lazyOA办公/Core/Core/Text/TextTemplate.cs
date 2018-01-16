namespace Core.Text
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    public class TextTemplate
    {
        private string[] _contentParts;
        private int _tagCount;
        private TextTemplateTag[] _tags;

        private TextTemplate()
        {
            this._tagCount = 0;
            this._tags = null;
            this._contentParts = null;
        }

        public TextTemplate(string content)
        {
            this.FromString(content);
        }

        public TextTemplate(string file, Encoding encoding)
        {
            StreamReader reader = new StreamReader(file, encoding);
            try
            {
                string content = reader.ReadToEnd();
                this.FromString(content);
            }
            catch (Exception)
            {
                reader.Close();
                throw;
            }
            reader.Close();
        }

        private void FromString(string content)
        {
            MatchCollection matchs = Regex.Matches(content, @"{\w+}");
            this._tagCount = matchs.Count;
            this._tags = new TextTemplateTag[matchs.Count];
            this._contentParts = new string[matchs.Count + 1];
            int index = 0;
            foreach (Match match in matchs)
            {
                this._tags[index++] = new TextTemplateTag(match.Value.Substring(1, match.Value.Length - 2), match.Index, match.Length);
            }
            int startIndex = 0;
            index = 0;
            foreach (TextTemplateTag tag in this._tags)
            {
                this._contentParts[index] = content.Substring(startIndex, tag.Position - startIndex);
                startIndex = tag.Position + tag.Length;
                index++;
            }
            if (startIndex < content.Length)
            {
                this._contentParts[index] = content.Substring(startIndex);
            }
        }

        public string Render(Hashtable values)
        {
            StringBuilder builder = new StringBuilder(0x2000);
            int index = 0;
            index = 0;
            while (index < this._tagCount)
            {
                builder.Append(this._contentParts[index]);
                if (values[this._tags[index].Name] != null)
                {
                    builder.Append(values[this._tags[index].Name]);
                }
                else
                {
                    builder.Append("{" + this._tags[index].Name + "}");
                }
                index++;
            }
            builder.Append(this._contentParts[index]);
            return builder.ToString();
        }

        public string Render(params object[] args)
        {
            StringBuilder builder = new StringBuilder(0x800);
            int index = 0;
            index = 0;
            while (index < this._tagCount)
            {
                builder.Append(this._contentParts[index]);
                builder.Append(args[index].ToString());
                index++;
            }
            builder.Append(this._contentParts[index]);
            return builder.ToString();
        }

        public void SaveAs(string file, Encoding encoding, Hashtable values)
        {
            StreamWriter writer = new StreamWriter(file, false, encoding);
            try
            {
                string str = this.Render(values);
                writer.Write(str);
            }
            catch (Exception)
            {
                writer.Close();
                throw;
            }
            writer.Close();
        }

        public void SaveAs(string file, Encoding encoding, params object[] args)
        {
            StreamWriter writer = new StreamWriter(file, false, encoding);
            try
            {
                string str = this.Render(args);
                writer.Write(str);
            }
            catch (Exception)
            {
                writer.Close();
                throw;
            }
            writer.Close();
        }

        public TextTemplate[] Split(string splitTag)
        {
            List<TextTemplate> list = new List<TextTemplate>();
            List<string> list2 = new List<string>();
            List<TextTemplateTag> list3 = new List<TextTemplateTag>();
            int index = 0;
            foreach (string str in this._contentParts)
            {
                list2.Add(str);
                if ((index >= this._tags.Length) || (this._tags[index].Name == splitTag))
                {
                    TextTemplate item = new TextTemplate {
                        _contentParts = list2.ToArray(),
                        _tags = list3.ToArray(),
                        _tagCount = list3.Count
                    };
                    list.Add(item);
                    list2.Clear();
                    list3.Clear();
                }
                else
                {
                    list3.Add(new TextTemplateTag(this._tags[index].Name, this._tags[index].Position, this._tags[index].Length));
                }
                index++;
            }
            return list.ToArray();
        }
    }
}

