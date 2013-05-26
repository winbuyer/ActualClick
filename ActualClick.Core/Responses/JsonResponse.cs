using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using System.IO;
using System.Web;

namespace WinBuyer.B2B.Widget.DataService.Core.Responses
{
    public class JsonResponse : Response
    {
        public JsonResponse(object model)
        {
            GetResponse(model, true, null);
        }
        public JsonResponse(object model, bool enableCacheControl)
        {
            GetResponse(model, enableCacheControl, null);
        }
        public JsonResponse(object model, bool enableCacheControl, string contentType)
        {
            GetResponse(model, enableCacheControl, contentType);
        }

        private void GetResponse(object model, bool enableCacheControl, string contentType)
        {
            if (model == null)
            {
                this.StatusCode = HttpStatusCode.NoContent;

                return;
            }

            if (HttpContext.Current.Request.UrlReferrer != null)
            {
                this.Headers["Access-Control-Allow-Credentials"] = "true";
                this.Headers["Access-Control-Allow-Origin"] = string.Format("{0}://{1}",
                            HttpContext.Current.Request.UrlReferrer.Scheme,
                            HttpContext.Current.Request.UrlReferrer.Host);
            }
            else
            {
                this.Headers["Access-Control-Allow-Origin"] = "*";
            }

            this.Contents = GetJsonContents(model);
            this.ContentType = string.IsNullOrEmpty(contentType) ? "application/json" : contentType;
            this.StatusCode = HttpStatusCode.OK;

            if (enableCacheControl)
            {
                this.Headers["Cache-Control"] = "public";
                this.Headers["Expires"] = DateTime.UtcNow.AddMinutes(60).ToString("ddd, dd-MMM-yyyy HH:mm:ss 'GMT'");
            }
        }

        private Action<Stream> GetJsonContents(object model)
        {
            ISerializer jsonSerializer = new JsonSerializer();
            return stream => jsonSerializer.Serialize("application/json", model, stream);
        }
    }
}
