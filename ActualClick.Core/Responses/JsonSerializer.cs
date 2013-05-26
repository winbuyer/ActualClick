using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Nancy;

namespace WinBuyer.B2B.Widget.DataService.Core.Responses
{
    public class JsonSerializer : ISerializer
    {
        public bool CanSerialize(string contentType)
        {
            if (contentType == "application/json")
                return true;
            return false;
        }
        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            if (contentType != "application/json")
                throw new NotSupportedException(contentType);

            string serialized = null;

            if (model is string)
                serialized = model as string;
            else
                serialized = JsonConvert.SerializeObject(model, Formatting.None);

            StreamWriter streamWriter = new StreamWriter(outputStream, new UTF8Encoding(false));
            streamWriter.AutoFlush = true;
            streamWriter.Write(serialized);
        }
        public IEnumerable<string> Extensions
        {
            get
            {
                return null;
            }
        }
    }
}
