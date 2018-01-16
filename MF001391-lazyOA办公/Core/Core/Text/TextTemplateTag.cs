namespace Core.Text
{
    using System;

    internal class TextTemplateTag
    {
        private int _length;
        private string _name;
        private int _position;

        public TextTemplateTag(string name, int pos, int len)
        {
            this._name = name;
            this._position = pos;
            this._length = len;
        }

        public int Length
        {
            get
            {
                return this._length;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public int Position
        {
            get
            {
                return this._position;
            }
        }
    }
}

