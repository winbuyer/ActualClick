using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using ActualClick.Core.Responses;

namespace ActualClick.Core.Extensions
{
    public static class Extensions
    {
        public static Response FromByteArray(this IResponseFormatter formatter, byte[] body, string contentType = null)
        {
            return new ByteArrayResponse(body, contentType);
        }
    }
}
