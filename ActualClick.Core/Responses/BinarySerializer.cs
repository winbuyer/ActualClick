using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using System.IO;
using WinBuyer.Infrastructure.Common.Serialization;

namespace WinBuyer.B2B.Widget.DataService.Core.Responses
{
    public class BinarySerializer : ISerializer
    {
        public bool CanSerialize(string contentType)
        {
            if (contentType == "application/octet-stream")
                return true;
            return false;
        }
        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            if (contentType != "application/octet-stream")
                throw new NotSupportedException(contentType);

            byte[] serialized = Binary.Serialize(model);

            outputStream.Write(serialized, 0, serialized.Length);
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
