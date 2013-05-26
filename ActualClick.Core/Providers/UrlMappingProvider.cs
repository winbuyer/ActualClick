using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Nancy.Helpers;
using WinBuyer.B2B.DealFinder.Core.Providers.Market;
using WinBuyer.B2B.CseToMongoEtl.Providers;

namespace WinBuyer.B2B.DealFinder.Core.Providers
{
    public class UrlMappingProvider
    {
        public dynamic GetUrlMappingData(string url, string cachedValue, string domain)
        {
            var debugDictionary = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(url))
            {
                debugDictionary.Add("missing parameter", "url");

                return GetResponse(null, debugDictionary);
            }

            url = HttpUtility.UrlDecode(url);
            debugDictionary.Add("url", url);
            var domainName = ExtentionsProvider.GetDomainName(url);
            debugDictionary.Add("domain", domainName);

            string pageType = GetPageTypeByUrl(debugDictionary, domainName, url);

            if (string.IsNullOrEmpty(pageType))
                return GetResponse(null, debugDictionary);

            string result = null;

            switch (pageType.ToLower())
            {
                case "search":
                    {
                        result = GetSearchKeyword(debugDictionary, domainName, url);
                        return GetResponse(new
                        {
                            pagetype = pageType.ToLower(),
                            keyword = result
                        },
                        debugDictionary);
                    }
                case "product":
                    {
                        result = GetProductName(debugDictionary, domainName, url);
                        return GetResponse(new
                        {
                            pagetype = pageType.ToLower(),
                            keyword = result
                        },
                        debugDictionary);
                    }
                case "category":
                    {
                        result = GetProductName(debugDictionary, domainName, url);
                        return GetResponse(new
                        {
                            pagetype = pageType.ToLower(),
                            keyword = result
                        },
                        debugDictionary);
                    }
                case "product-category":
                    {
                        result = GetProductName(debugDictionary, domainName, url);
                        return GetResponse(new
                        {
                            pagetype = pageType.ToLower(),
                            keyword = result
                        },
                        debugDictionary);
                    }
                case "offerlisting":
                    {
                        result = GetSearchKeyword(debugDictionary, domainName, url);
                        return GetResponse(new
                        {
                            pagetype = pageType.ToLower(),
                            keyword = result
                        },
                        debugDictionary);
                    }
            }

            return GetResponse(pageType, debugDictionary);
        }
        public dynamic GetUrlMappingData(string url)
        {
            return GetUrlMappingData(url, null, null);
        }

        private dynamic GetResponse(dynamic data, dynamic debug)
        {
            return new
            {
                data = data,
                debug = debug
            };
        }
        private string GetSearchKeyword(Dictionary<string, string> debug, string domain, string url)
        {
            var searchParamsDt = CseToMongoEtlProvider.GetDataTable("Get_Domain_SearchParam",
                new Dictionary<string, object> { { "@domain", domain } },
                "SQLCacheConnectionString", 60);

            if (searchParamsDt == null || searchParamsDt.Rows.Count == 0)
            {
                debug.Add("could not find search params for", domain);
                return null;
            }

            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);

            for (int i = 0; i < searchParamsDt.Rows.Count; i++)
            {
                var param = (string)searchParamsDt.Rows[i]["SearchParam"];
                var searchKeyword = query[param];

                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    debug.Add("search keyword", searchKeyword);
                    return searchKeyword;
                }
            }

            debug.Add("could not find search params in", url);
            return null;
        }
        private string GetProductName(Dictionary<string, string> debug, string domain, string url)
        {
            var urlKeywordMappingsDt = CseToMongoEtlProvider.GetDataTable("Get_Domain_UrlProduct",
               new Dictionary<string, object> { { "@domain", domain } },
               "SQLCacheConnectionString", 60);

            if (urlKeywordMappingsDt == null || urlKeywordMappingsDt.Rows.Count == 0)
            {
                return null;

                //debug.Add("could not find url mapping for domain, executing get site data", domain);

                //try
                //{
                //    var siteData = MarketProvider.GetSiteData(debug, "6000666", url, false);

                //    return siteData.productName;
                //}
                //catch
                //{

                //}
            }
            else
            {
                debug.Add("url mapping found", domain);

                short level = (short)urlKeywordMappingsDt.Rows[0]["Level"];
                string stopString = (string)urlKeywordMappingsDt.Rows[0]["StopString"];
                string divider = (string)urlKeywordMappingsDt.Rows[0]["Divider"];

                var uri = new Uri(url);

                string keyword = uri.Segments[level - 1];
                keyword = keyword.Replace(stopString, "");
                keyword = keyword.Replace(divider, " ");

                return keyword;
            }

            return null;
        }
        private string GetPageTypeByUrl(Dictionary<string, string> debug, string domain, string url)
        {
            var pageTypeMappingDt = CseToMongoEtlProvider.GetDataTable("Get_Domain_PageTypes",
                new Dictionary<string, object> { { "@domain", domain } },
                "SQLCacheConnectionString", 60);

            if (pageTypeMappingDt == null || pageTypeMappingDt.Rows.Count == 0)
            {
                debug.Add("could not find page type mappings for", domain);
                return null;
            }

            for (int i = 0; i < pageTypeMappingDt.Rows.Count; i++)
            {
                if (Regex.IsMatch(url, (string)pageTypeMappingDt.Rows[i]["Regex"]))
                {
                    string pageType = (string)pageTypeMappingDt.Rows[i]["pagetype"];
                    debug.Add("pagetype", pageType.ToLower());
                    return pageType;
                }
            }

            debug.Add("could determin page type for", url);

            return null;
        }
    }
}
