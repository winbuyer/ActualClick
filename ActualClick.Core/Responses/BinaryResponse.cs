using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using System.IO;

namespace WinBuyer.B2B.Widget.DataService.Core.Responses
{
    public class BinaryResponse : Response
    {
        public BinaryResponse(object model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            this.Contents = GetBinaryContents(model);
            this.ContentType = "application/octet-stream";
            this.StatusCode = HttpStatusCode.OK;

            this.Headers["Cache-Control"] = "public";
            this.Headers["Expires"] = DateTime.UtcNow.AddMinutes(60).ToString("ddd, dd-MMM-yyyy HH:mm:ss 'GMT'");
        }

        private Action<Stream> GetBinaryContents(object model)
        {
            ISerializer binarySerializer = new BinarySerializer();
            return stream => binarySerializer.Serialize("application/octet-stream", model, stream);
        }
    }
}

