using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using System.IO;

namespace ActualClick.Core.Responses
{
    public class ByteArrayResponse : Response
    {
        /// <summary>
        /// Byte array response
        /// </summary>
        /// <param name="body">Byte array to be the body of the response</param>
        /// <param name="contentType">Content type to use</param>
        public ByteArrayResponse(byte[] body, string contentType = null)
        {
            if (body == null)
                return;

            this.ContentType = contentType ?? "application/octet-stream";

            this.Contents = stream =>
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(body);
                }
            };
        }
    }
}
