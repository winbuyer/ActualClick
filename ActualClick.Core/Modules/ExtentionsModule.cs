using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using log4net;
using System.Reflection;
using WinBuyer.B2B.DealFinder.Core.Providers.Market;
using WinBuyer.B2B.Widget.DataService.Core.Responses;
using System.Configuration;
using System.IO;
using System.Dynamic;
using System.Collections;
using System.Web;
using System.Runtime.Caching;
using Newtonsoft.Json.Linq;

namespace ActualClick.Core
{
    public class ExtentionsModule : NancyModule
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ExtentionsModule(ExtentionsProvider provider)
            : base("/extentions")
        {
            Get["/get-products-by-url"] = p =>
            {
                dynamic result = null;

                string url = Request.Query.url;
                string campaignId = Request.Query.campaignid;
                string responseType = Request.Query.responsetype == null ? "html" : Request.Query.responsetype;
                bool qaMode = Request.Query.qamode == "true" ? true : false;

                dynamic cookies = new ExpandoObject();

                cookies.userId = Request.Cookies.ContainsKey("user_id") ? Request.Cookies["user_id"] : "";

                try
                {
                    result = provider.GetProductsByUrl(url, campaignId, cookies, qaMode);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);

                    result = ex;

                    return new JsonResponse(result);
                }

                switch (responseType)
                {
                    case "json":
                        return new JsonResponse(result);
                    case "html":
                        return View["market", result];
                    default:
                        return View["market", result];
                }
            };
            Get["/get-products-by-trigger"] = p =>
            {
                dynamic result = null;

                string url = Request.Query.url;
                string campaignId = Request.Query.campaignid;
                string sid = Request.Query.sid;
                string responseType = Request.Query.responsetype == null ? "html" : Request.Query.responsetype;
                bool qaMode = Request.Query.qamode == "true" ? true : false;

                dynamic cookies = new ExpandoObject();

                cookies.cookiesEnabled = Request.Cookies.ContainsKey("test_cookie") ? true : false;
                cookies.rtPointer = Request.Cookies.ContainsKey("rt_pointer") ? Request.Cookies["rt_pointer"] : "0";
                cookies.rtStack = Request.Cookies.ContainsKey("rt_stack") ? Request.Cookies["rt_stack"] : "{}";
                cookies.rtShowCount = Request.Cookies.ContainsKey("rt_show_count") ? Request.Cookies["rt_show_count"] : "0";
                cookies.rtLastShown = Request.Cookies.ContainsKey("rt_last_shown") ? Request.Cookies["rt_last_shown"] : "0";
                cookies.userId = Request.Cookies.ContainsKey("user_id") ? Request.Cookies["user_id"] : "";

                try
                {
                    result = provider.GetProductsByTrigger(url, campaignId, sid, cookies, qaMode);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);

                    result = ex;

                    return new JsonResponse(result);
                }

                switch (responseType)
                {
                    case "json":
                        {
                            var response = new JsonResponse(result);

                            if (result.rt_pointer != null)
                                response.AddCookie("rt_pointer", (string)result.rt_pointer, DateTime.Now.AddYears(2));

                            if (result.rt_stack != null)
                                response.AddCookie("rt_stack", (string)result.rt_stack, DateTime.Now.AddYears(2));

                            if (result.rt_show_count != null)
                                response.AddCookie("rt_show_count", (string)result.rt_show_count, DateTime.Now.AddDays(1));

                            if (result.rt_last_shown != null)
                                response.AddCookie("rt_last_shown", (string)result.rt_last_shown, DateTime.Now.AddYears(2));

                            response.AddCookie("user_id", (string)result.user_id, DateTime.Now.AddYears(2));
                            response.AddCookie("impression_id", (string)result.impression_id, DateTime.Now.AddYears(2));

                            return response;
                        }
                    case "html":
                        return View["market", result];
                    default:
                        return View["market", result];
                }
            };
            Get["/get-js"] = p =>
            {
                string result = null;

                try
                {
                    string guid = ConfigurationManager.AppSettings["GUID"];
                    string requsetGuid = Request.Headers.IfNoneMatch.Count() == 0 ?
                        null : Request.Headers.IfNoneMatch.ToList()[0];

                    if (requsetGuid == guid)
                        return new Response().StatusCode = HttpStatusCode.NotModified;

                    string campaignId = Request.Query.campaignId;
                    string sid = Request.Query.sid;
                    string qaMode = Request.Query.qaMode == "true" ? "true" : "false";

                    string jsFilePath = string.Format(@"{0}\js\actualclick\actualclick-gateway.js",
                        ConfigurationManager.AppSettings["DealFinderCdnFolder"]);

                    string jsFile = File.ReadAllText(jsFilePath);

                    jsFile = jsFile.Replace("$campaignId$", campaignId);
                    jsFile = jsFile.Replace("$sid$", sid);
                    jsFile = jsFile.Replace("$qaMode$", qaMode);
                    jsFile = jsFile.Replace("$version$", guid);

                    result = jsFile;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);

                    result = ex.Message;
                }

                return Response.AsText(result, "application/javascript")
                    .WithHeader("Etag", ConfigurationManager.AppSettings["GUID"])
                    .AddCookie("test_cookie", "test_cookie", DateTime.Now.AddYears(2));
            };
            Get["/get-html"] = p =>
            {
                return View["extentions"];
            };
            Get["/get-about"] = p =>
            {
                return View["market-about"];
            };
            Get["/log-timers"] = p =>
            {
                dynamic result = null;

                try
                {
                    string decodedData = Base64Decode(Request.Query.data);
                    dynamic requestJson = JObject.Parse(decodedData);

                    string campaignId = requestJson.campaignId;
                    string url = requestJson.url;
                    string cachedValue = requestJson.cachedValue;
                    string userId = Request.Cookies.ContainsKey("user_id") ? Request.Cookies["user_id"] : "";
                    string imressionId = Request.Cookies.ContainsKey("impression_id") ? Request.Cookies["impression_id"] : "";

                    provider.LogTimers(url, campaignId, userId, imressionId, cachedValue, requestJson.timers);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                return new JsonResponse(result);
            };
            Get["/clear-cache"] = p =>
            {
                dynamic result = null;

                try
                {
                    var enumerator = HttpContext.Current.Cache.GetEnumerator();

                    result = new List<dynamic>();

                    while (enumerator.MoveNext())
                    {
                        HttpContext.Current.Cache.Remove((string)enumerator.Key);

                        result.Add("removed cached key: " + (string)enumerator.Key);
                        result.Add("</br>");
                    }

                    var keys = MemoryCache.Default.Select(x => x.Key).ToList();

                    foreach (var key in keys)
                    {
                        MemoryCache.Default.Remove(key);

                        result.Add("removed cached key: " + key);
                        result.Add("</br>");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);

                    result = ex;
                }

                return new JsonResponse(result, false, "text/html");
            };
        }

        private string Base64Encode(string str)
        {
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));

            return base64;
        }
        private string Base64Decode(string str)
        {
            byte[] result = Convert.FromBase64String(str);
            string decodedString = Encoding.UTF8.GetString(result);

            return decodedString;
        }
    }
}
