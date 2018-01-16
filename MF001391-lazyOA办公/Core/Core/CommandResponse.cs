namespace Core
{
    using System;
    using System.Text;

    public class CommandResponse : IRenderJson
    {
        public string CommandID;
        public string Data;

        public CommandResponse(string cmd, string data)
        {
            this.CommandID = cmd;
            this.Data = data;
        }

        void IRenderJson.RenderJson(StringBuilder builder)
        {
            Utility.RenderHashJson(builder, new object[] { "CommandID", this.CommandID, "Data", new JsonText(this.Data) });
        }
    }
}

