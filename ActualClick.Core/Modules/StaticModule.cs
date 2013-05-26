using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Reflection;
using System.Configuration;
using System.IO;
using Nancy;
using System.Web;
using ActualClick.Core.Extensions;
using Extentions.DataService.Providers;

namespace Extentions.DataService.Modules
{
    public class StaticModule : NancyModule
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public StaticModule(StaticProvider provider)
            : base("/static")
        {
            Get["/get-js"] = p =>
            {
                dynamic result = null;

                try
                {
                    string staticCacheKey = ConfigurationManager.AppSettings["StaticCacheKey"];
                    string requestSaticCacheKey = Request.Headers.IfNoneMatch.Count() == 0 ? null : Request.Headers.IfNoneMatch.ToList()[0];

                    if (requestSaticCacheKey == staticCacheKey)
                        return new Response().WithStatusCode(HttpStatusCode.NotModified);

                    string fileName = Request.Query.name;

                    result = provider.GetJs(fileName);

                    string campaignId = Request.Query.campaignid;
                    string sid = Request.Query.sid;
                    string qaMode = Request.Query.qamode;

                    result = result.Replace("$campaignId$", campaignId);
                    result = result.Replace("$sid$", sid);
                    result = result.Replace("$qaMode$", qaMode);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);

                    result = ex.Message;
                }

                return Response.AsText((string)result, "application/javascript")
                       .WithHeader("Etag", ConfigurationManager.AppSettings["StaticCacheKey"]);
            };
            Get["/get-image"] = p =>
            {
                dynamic result = null;

                try
                {
                    string staticCacheKey = ConfigurationManager.AppSettings["StaticCacheKey"];
                    string requestSaticCacheKey = Request.Headers.IfNoneMatch.Count() == 0 ? null : Request.Headers.IfNoneMatch.ToList()[0];

                    if (requestSaticCacheKey == staticCacheKey)
                        return new Response().WithStatusCode(HttpStatusCode.NotModified);

                    string fileName = Request.Query.name;

                    result = provider.GetImage(fileName);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);

                    result = ex.Message;

                    return Response.AsText((string)result, "text/html")
                       .WithHeader("Etag", ConfigurationManager.AppSettings["StaticCacheKey"]);
                }

                return Response.FromByteArray((byte[])result.content, (string)result.content_type)
                       .WithHeader("Etag", ConfigurationManager.AppSettings["StaticCacheKey"]);
            };
            Get["/get-css"] = p =>
            {
                dynamic result = null;

                try
                {
                    string staticCacheKey = ConfigurationManager.AppSettings["StaticCacheKey"];
                    string requestSaticCacheKey = Request.Headers.IfNoneMatch.Count() == 0 ? null : Request.Headers.IfNoneMatch.ToList()[0];

                    if (requestSaticCacheKey == staticCacheKey)
                        return new Response().WithStatusCode(HttpStatusCode.NotModified);

                    string fileName = Request.Query.name;

                    result = provider.GetCss(fileName);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);

                    result = ex.Message;
                }

                return Response.AsText((string)result, "text/css")
                       .WithHeader("Etag", ConfigurationManager.AppSettings["StaticCacheKey"]);
            };
        }
    }
}
