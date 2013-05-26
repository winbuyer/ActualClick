using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Reflection;
using System.Web;
using WinBuyer.B2B.CseToMongoEtl.Entities;
using System.Runtime.Caching;
using System.Dynamic;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.Data;
using System.Configuration;
using WinBuyer.B2B.CseToMongoEtl.Providers;
using WinBuyer.B2B.Common.Implementation;

namespace WinBuyer.B2B.DealFinder.Core.Providers.Market
{
    public class ExtentionsProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public dynamic GetProductsByTrigger(string url, string campaignId, string sid, dynamic cookies, bool qaMode)
        {
            var context = new DealFinderContext(HttpContext.Current)
            {
                Url = HttpUtility.UrlDecode(url),
                DomainName = GetDomainName(HttpUtility.UrlDecode(url)),
                CampaignId = campaignId,
                QaMode = qaMode,
                RtPointer = cookies.rtPointer,
                RtLastShown = cookies.rtLastShown,
                RtShowCount = cookies.rtShowCount,
                RtStack = cookies.rtStack,
                NewUserId = cookies.userId == "" ? Guid.NewGuid().ToString() : cookies.userId,
                NewImpressionId = Guid.NewGuid().ToString(),
                CookiesEnabled = cookies.cookiesEnabled,
                Sid = sid,
                Nid = campaignId,
                Op = GetOP(campaignId, sid, campaignId)
            };

            var timer = new TimerProvider(context, "global-server-time");

            context.UserId = context.NewUserId;
            context.ImpressionId = context.NewImpressionId;

            dynamic result = null;

            try
            {
                result = GetProductsByTriggerAction(context);
            }
            catch (Exception ex)
            {
                context.Status = 3;

                var pcmInfo = new CsePcmInfo();
                var products = new List<CseProductInfo>();

                result = GetResult(context, pcmInfo, products);
            }

            result = LogResult(context, result);

            AddDebugParameters(context);

            timer.StopTimer();

            return result;
        }
        public dynamic GetProductsByUrl(string url, string campaignId, dynamic cookies, bool qaMode)
        {
            var context = new DealFinderContext(HttpContext.Current)
            {
                Url = HttpUtility.UrlDecode(url),
                DomainName = GetDomainName(HttpUtility.UrlDecode(url)),
                CampaignId = campaignId,
                QaMode = qaMode,
                NewUserId = cookies.userId == "" ? Guid.NewGuid().ToString() : cookies.userId,
                NewImpressionId = Guid.NewGuid().ToString(),
            };

            context.UserId = context.NewUserId;
            context.ImpressionId = context.NewImpressionId;

            dynamic result = null;

            try
            {
                result = GetProductsByUrlOrKeywordAction(context);
            }
            catch (Exception ex)
            {
                context.Status = 3;

                var pcmInfo = new CsePcmInfo();
                var products = new List<CseProductInfo>();

                result = GetResult(context, pcmInfo, products);
            }

            result = LogResult(context, result);

            return result;
        }
        public dynamic LogTimers(string url, string campaignId, string userId, string impressionId, string cachedValue, dynamic timers)
        {
            string domain = GetDomainName(url);

            new BusinessLoggerProvider().LogPerformance(domain, campaignId, userId, impressionId, url, cachedValue, timers);

            return new
            {
                logged = "OK"
            };
        }

        private dynamic GetProductsByUrlOrKeywordAction(DealFinderContext context)
        {
            dynamic result = null;

            var timer = new TimerProvider(context, "get-url-mapping-data");

            var urlMappingProvider = new UrlMappingProvider();
            var urlMapping = urlMappingProvider.GetUrlMappingData(context.Url);

            timer.StopTimer();

            if (urlMapping != null && urlMapping.data != null)
            {
                context.ProductName = urlMapping.data.pagetype == "search" ? urlMapping.data.keyword : null;
                context.PageType = urlMapping.data.pagetype == "search" ? "search" : "product";
                context.IsDefaultPrice = urlMapping.data.pagetype == "search" ? true : false;
                context.CachedValue = urlMapping.data.pagetype == "search" ? context.ProductName : null;
                context.SearchDomainName = urlMapping.data.pagetype == "search" ? string.Format("ac.search.{0}", context.Country.ToLower()) : null;
            }

            if (context.ProductName == null)
                result = GetProductsByUrlAction(context);
            else
                result = GetProductsByKeywordAction(context);

            return result;
        }
        private dynamic GetProductsByUrlAction(DealFinderContext context)
        {
            var appCachedValue = string.Format("GetProductsByUrlAction:{0}:{1}", context.Url, context.CampaignId);
            var products = new List<CseProductInfo>();
            var pcmInfo = new CsePcmInfo();

            if (MemoryCache.Default.Get(appCachedValue) == null || context.QaMode == true)
            {
                products = RetrieveProducts(context, out pcmInfo);

                dynamic cacheObject = new
                {
                    products = products,
                    pcm = pcmInfo
                };

                MemoryCache.Default.Add(appCachedValue, cacheObject, DateTimeOffset.Now.AddHours(4));
            }
            else
            {
                context.Status = 1;

                var cacheOject = (dynamic)MemoryCache.Default[appCachedValue];

                products = cacheOject.products;
                pcmInfo = cacheOject.pcm;
            }

            context.Currency = pcmInfo.Currency;

            var result = GetResult(context, pcmInfo, products);

            return result;
        }
        private dynamic GetProductsByKeywordAction(DealFinderContext context)
        {
            var appCachedValue = string.Format("GetProductsByKeyword:{0}:{1}", context.Url, context.CampaignId);
            var products = new List<CseProductInfo>();
            var pcmInfo = new CsePcmInfo();

            if (MemoryCache.Default.Get(appCachedValue) == null || context.QaMode == true)
            {
                products = RetrieveProducts(context, out pcmInfo);

                dynamic cacheObject = new
                {
                    products = products,
                    pcm = pcmInfo
                };

                MemoryCache.Default.Add(appCachedValue, cacheObject, DateTimeOffset.Now.AddHours(4));
            }
            else
            {
                context.Status = 1;

                var cacheOject = (dynamic)MemoryCache.Default[appCachedValue];

                products = cacheOject.products;
                pcmInfo = cacheOject.pcm;
            }

            context.Currency = pcmInfo.Currency;

            var result = GetResult(context, pcmInfo, products);

            return result;
        }
        private dynamic GetProductsByTriggerAction(DealFinderContext context)
        {
            var domainTriggers = GetDomainTriggersAndCountry(context);
            bool hasMatch = CheckIfCurrentDomainHasMatchForCurrentUrl(context, domainTriggers);

            var retargetingProvider = new RetargetingProvider(context);
            var retargetingInfo = retargetingProvider.GetRetargetingInfo();
            var retargetingSettings = retargetingProvider.GetRtSettings();
            bool isEcommerceDomain = retargetingProvider.IsEcommerceDomain();

            context.IsRetargetingEnabled = retargetingSettings.is_active;
            context.IsEcommerceDomain = isEcommerceDomain;
            context.EcommerceRecency = retargetingSettings.e_com_rec.ToString() + " Minutes";
            context.NonEcommerceRecency = retargetingSettings.non_e_com_rec.ToString() + " Minutes";
            context.NonEcommerceFrequency = retargetingSettings.freq.ToString();

            dynamic result = null;

            if (hasMatch)
            {
                result = GetProductsByUrlOrKeywordAction(context);

                int stackCount = 0;
                var stack = retargetingProvider.UpdateProductIdsStack(context.RtStack, (string)result.product_id, out stackCount);

                result.rt_stack = stack;
                result.rt_pointer = (stackCount - 1).ToString();
                result.rt_last_shown = DateTime.Now.Ticks.ToString();
            }
            else
            {
                context.Status = -1;
                context.PageType = "No Trigger";
                context.IsDomainInDataBase = GetDomainCache(context.DomainName).Length > 0;

                if (retargetingInfo == null)
                {
                    var pcmInfo = new CsePcmInfo();
                    var products = new List<CseProductInfo>();

                    result = GetResult(context, pcmInfo, products);
                }
                else
                {
                    context.IsRetargetingResult = true;
                    context.PageType = isEcommerceDomain ? "Retargeting eCom" : "Retargeting non eCom";

                    result = GetProductsByPcmIdAction(context, (ObjectId)retargetingInfo.product_id);

                    result.rt_pointer = retargetingInfo.pointer;
                    result.rt_show_count = retargetingInfo.show_count.ToString();
                    result.rt_last_shown = DateTime.Now.Ticks.ToString();
                }
            }

            return result;
        }
        private dynamic GetProductsByPcmIdAction(DealFinderContext context, ObjectId pcmId)
        {
            var appCachedValue = string.Format("GetProductsByPcmId:{0}", pcmId);
            var products = new List<CseProductInfo>();
            var pcmInfo = new CsePcmInfo();
            context.Status = 1;

            if (MemoryCache.Default.Get(appCachedValue) == null || context.QaMode == true)
            {
                var pcmProvider = new PcmProvider(context);
                pcmInfo = pcmProvider.GetProductFromPcmById(pcmId);

                context.IsExistInPcm = true;
                context.ProductName = pcmInfo.ProductName;
                context.Price = pcmInfo.Price;
                context.IsDefaultPrice = pcmInfo.IsDefaultPrice;

                var productsProvider = new ProductsProvider(context);
                products = productsProvider.GetProductsFromPcm(pcmInfo);

                dynamic cacheObject = new
                {
                    products = products,
                    pcm = pcmInfo
                };

                MemoryCache.Default.Add(appCachedValue, cacheObject, DateTimeOffset.Now.AddHours(4));
            }
            else
            {
                var cacheOject = (dynamic)MemoryCache.Default[appCachedValue];

                products = cacheOject.products;
                pcmInfo = cacheOject.pcm;
            }

            context.Currency = pcmInfo.Currency;

            var result = GetResult(context, pcmInfo, products);

            return result;
        }

        private List<CseProductInfo> RetrieveProducts(DealFinderContext context, out CsePcmInfo pcmInfo)
        {
            var timer = new TimerProvider(context, "extract-cache-value-and-domain");
            var domains = GetDomainCache(context.DomainName);

            if (domains.Length > 0)
                context.IsDomainInDataBase = true;

            if (context.PageType == "product")
            {
                int priority = 0;

                context.CachedValue = GetCachedValue(domains, context.Url, context.DomainName, out priority);
                context.CachedValuePriority = priority == 0 ? "" : priority.ToString();
            }

            timer.StopTimer();

            var pcmProvider = new PcmProvider(context);
            pcmInfo = pcmProvider.GetProductFromPcmByCacheValueAndDomain();
            var products = new List<CseProductInfo>();

            if (pcmInfo == null)
            {
                if (context.PageType == "product")
                {
                    timer = new TimerProvider(context, "get-site-data");

                    var getSiteDataProvider = new GetSiteDataProvider(context, true);
                    var siteData = getSiteDataProvider.GetSiteData();

                    timer.StopTimer();

                    context.IsGetSiteDataExecuted = true;

                    if (siteData == null)
                    {
                        context.IsGetSiteDataFailed = true;

                        pcmInfo = new CsePcmInfo();

                        return products;
                    }
                }

                var productsProvider = new ProductsProvider(context);

                products = productsProvider.GetProductsFromCse(1);

                timer = new TimerProvider(context, "insert-product-to-pcm");
                pcmInfo = pcmProvider.UpsertProductToPcm(products);
                timer.StopTimer();
            }
            else
            {
                context.IsExistInPcm = true;
                context.Status = 1;
                context.ProductName = pcmInfo.ProductName;
                context.Price = pcmInfo.Price;
                context.IsDefaultPrice = pcmInfo.IsDefaultPrice;

                timer = new TimerProvider(context, "get-data-from-pcm");

                var productsProvider = new ProductsProvider(context);
                products = productsProvider.GetProductsFromPcm(pcmInfo);

                timer.StopTimer();
            }

            context.ProductName = pcmInfo.ProductDisplayName;
            context.Price = pcmInfo.Price;
            context.DomainPriceRule = pcmInfo.DomainPriceRule;

            return products;
        }

        private string CalculateWidth(List<dynamic> products)
        {
            int totalNumberOfOffers = products.Count;

            switch (totalNumberOfOffers)
            {
                case 1:
                    return "410px";
                case 2:
                    return "650px";
                case 3:
                    return "895px";
            }

            return "0px";
        }
        private string GetMoreDealsUrl(DealFinderContext context, string productName)
        {
            productName = productName.Replace(" ", "+");

            string result = null;

            if (context.Currency == "$")
                result = context.ProductName != "" ? @"http://www.shoppingedge.com/keyword-" + productName + "/search.html?campaign=5900007" : "";
            else
                result = context.ProductName != "" ? @"http://www.uk.cybercie.com/keyword-" + productName + "/search.html?campaign=5900007" : "";

            return result;
        }
        public static string GetDomainName(string url)
        {
            string DomainName = "";
            string[] arr;
            try
            {
                Uri uri = new Uri(url);
                DomainName = uri.Host;
                arr = DomainName.Split('.');
                if (arr.Length > 2)
                {
                    try
                    {
                        int Ext = Int32.Parse(arr[arr.Length - 1]);
                    }
                    catch (Exception)
                    {
                        if (DomainName.Contains(".co.uk") || DomainName.Contains(".com.au") || DomainName.Contains(".uk.com"))
                            DomainName = arr[arr.Length - 3] + "." + arr[arr.Length - 2] + "." + arr[arr.Length - 1];
                        else
                            DomainName = arr[arr.Length - 2] + "." + arr[arr.Length - 1];
                    }
                }
            }
            catch (Exception)
            {

                DomainName = url;
            }

            return DomainName;
        }

        private dynamic LogResult(DealFinderContext context, dynamic result)
        {
            int index = 0;
            int totalOffers = result.products.Count;
            double minOfferPrice = totalOffers == 0 ? 0 : result.products[0].product_price_raw;
            double maxOfferPrice = totalOffers == 0 ? 0 : result.products[result.products.Count - 1].product_price_raw;

            var businessLogerProvider = new BusinessLoggerProvider();

            foreach (var offer in result.products)
            {
                context.Category = offer.category;

                businessLogerProvider.LogImpression(totalOffers.ToString(),
                    "0", minOfferPrice.ToString(),
                    maxOfferPrice.ToString(), result.price.ToString(),
                    context.CachedValue, index.ToString(), offer.product_price_raw.ToString(),
                    offer.merchant_id, offer.cse.ToString(),
                    offer.product_id, result.keyword, context);

                index++;
            }

            businessLogerProvider.LogImpression(totalOffers.ToString(), totalOffers.ToString(),
                "0", "0", result.price.ToString(), context.CachedValue, context.Status == -1 ? "-2" : "-1", "0",
                context.Status.ToString(), "0", "0", result.keyword, context);

            return result;
        }
        private dynamic GetResult(DealFinderContext context, CsePcmInfo pcmInfo, List<CseProductInfo> products)
        {
            dynamic result = new ExpandoObject();

            result.html_template = new MarketTemplateProvider().GetTemplate();
            result.keyword = pcmInfo.ProductDisplayName;
            result.encoded_keyword = pcmInfo.ProductName;
            result.height = "90px";
            result.more_deals_url = GetMoreDealsUrl(context, pcmInfo.ProductName);
            result.sku = context.CachedValue;
            result.product_id = pcmInfo.ObjectId.ToString();
            result.rt_pointer = null;
            result.rt_stack = null;
            result.rt_show_count = null;
            result.rt_last_shown = null;
            result.price = pcmInfo.Price;
            result.debug = context.Debug;
            result.timers = context.Timers;
            result.user_id = context.NewUserId;
            result.impression_id = context.NewImpressionId;

            var resultProductList = new List<dynamic>();

            foreach (var product in products)
            {
                foreach (var offer in product.Offers)
                {
                    var productUrl = GenerateOuterClickUrl(offer.Url,
                     context.PageType, 3, 3, 0, offer.Price.ToString(), offer.Price.ToString(),
                     context.CachedValue, offer.Price.ToString(), offer.Price.ToString(),
                     context, product.ShoppingEngine.ToString(), true);

                    dynamic productResult = new ExpandoObject();

                    productResult.category = product.CategoryId;
                    productResult.product_id = product.ProductId;
                    productResult.product_name = product.Name.Length > 35 ? product.Name.Remove(35, product.Name.Length - 35) + "..." : product.Name;
                    productResult.product_image = product.Image;
                    productResult.product_url = productUrl;
                    productResult.product_price_raw = offer.Price;
                    productResult.product_price = string.Format("{0:0.00}", offer.Price);
                    productResult.merchant_image = offer.MerchantInfo.UseLogo ? offer.MerchantInfo.Logo : "";
                    productResult.merchant_name = offer.MerchantInfo.Name;
                    productResult.merchant_id = offer.MerchantInfo.MerchantId;
                    productResult.country = context.Country;
                    productResult.cse = product.ShoppingEngine;

                    resultProductList.Add(productResult);
                }
            }

            context.TotalNumberOfOffersToDisplay = resultProductList.Count;

            var sortProvider = new SortProvider(context);

            if (context.PageType == "search")
                resultProductList = sortProvider.RandomizeProducts(resultProductList, 3);

            resultProductList = sortProvider.SortProducts(resultProductList);

            result.products = resultProductList.Take(3).ToList();
            result.width = CalculateWidth(result.products);

            if (context.Status == 0 || context.Status == 1)
            {
                if (result.products.Count == 0)
                    context.Status = 2;

                if (result.products.Count > 0 && result.products.Count < 3)
                    context.Status = 4;
            }

            return result;
        }

        private void AddDebugParameters(DealFinderContext context)
        {
            context.AddDebug("Time Stamp", DateTime.Now.ToString());
            context.AddDebug("Url", context.Url);
            context.AddDebug("Trigger", context.CurrentTrigger);
            context.AddDebug("Campaign ID", context.CampaignId);
            context.AddDebug("GUID", context.NewUserId);
            context.AddDebug("Impression ID", context.NewImpressionId);
            context.AddDebug("Domain", context.DomainName);
            context.AddDebug("Is Domain In DataBase?", context.IsDomainInDataBase ? "Yes" : "No");
            context.AddDebug("Page Type", context.PageType);
            context.AddDebug("Country", context.Country);
            context.AddDebug("SKU", context.CachedValue);
            context.AddDebug("SKU in PCM?", context.IsExistInPcm ? "Yes" : "No");
            context.AddDebug("Is Get Site Data Executed?", context.IsGetSiteDataExecuted ? "Yes" : "No");
            context.AddDebug("Is Get Site Data Failed?", context.IsGetSiteDataFailed ? "Yes" : "No");
            context.AddDebug("Product Name", context.ProductName);
            context.AddDebug("Product Price", context.Price.ToString());
            context.AddDebug("Domain Price Rule", context.DomainPriceRule);
            context.AddDebug("Currency", context.Currency);
            context.AddDebug("SKU Optimized By", JsonConvert.SerializeObject(context.CseOptimizationMapping));
            context.AddDebug("Pre Filtered Products", JsonConvert.SerializeObject(context.CseNumberOfRawResults));
            context.AddDebug("Total Offers Available To Display", context.TotalNumberOfOffersToDisplay.ToString());
            context.AddDebug("CSE Call Status", JsonConvert.SerializeObject(context.CseExecutionFailure));

            context.AddDebug("Is Retargeting Enabled", context.IsRetargetingEnabled ? "Yes" : "No");
            context.AddDebug("Is Retargeting Result", context.IsRetargetingResult ? "Yes" : "No");
            context.AddDebug("Is eCommerce Domain", context.IsEcommerceDomain ? "Yes" : "No");
            context.AddDebug("eCommerce Recency", context.EcommerceRecency);
            context.AddDebug("Non eCommerce Recency", context.NonEcommerceRecency);
            context.AddDebug("Non eCommerce Frequency", context.NonEcommerceFrequency);

        }
        private List<string> GetDomainTriggersAndCountry(DealFinderContext context)
        {
            var timer = new TimerProvider(context, "get-domain-triggers");
            var dataTable = CseToMongoEtlProvider.GetDataTable("sp_Get_DomainTrigger",

            new Dictionary<string, object>
            {
                {"Domain", context.DomainName}
            },
            "SQLCacheConnectionString", 60);

            timer.StopTimer();

            timer = new TimerProvider(context, "domain-triggers-match-time");

            var domainTriggers = new List<string>();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                domainTriggers.Add((string)dataTable.Rows[i][0]);
                context.Country = dataTable.Rows[i][1] == DBNull.Value ? "US" : (string)dataTable.Rows[i][1];
            }

            timer.StopTimer();

            return domainTriggers;
        }
        private bool CheckIfCurrentDomainHasMatchForCurrentUrl(DealFinderContext context, List<string> domainTriggers)
        {
            foreach (dynamic trigger in domainTriggers)
            {
                Match match = Match.Empty;

                try
                {
                    match = Regex.Match(context.Url, trigger);
                }
                catch (Exception ex)
                {
                    context.AddDebug("Bad Trigger !", trigger);
                }

                if (match.Success)
                {
                    context.CurrentTrigger = trigger;
                    return true;
                }
            }

            return false;
        }
        private string GetOP(string campaignId, string sid, string nid)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("AID", campaignId);
            parameters.Add("PID", nid == null ? "" : nid);
            parameters.Add("SE", 0);
            parameters.Add("SID", sid == null ? "" : sid);

            DataTable aidMapping = CseToMongoEtlProvider.GetDataTable("SP_DF_GET_MappedCampaign", parameters, "SQLCacheConnectionString", 24);

            string op = aidMapping.Rows[0]["OP"].ToString();

            return op;
        }


        public static string GenerateOuterClickUrl(string offerUrl, string pageId, int shownProducts, int shownStores, int storePos,
                      string prodPrice, string storePrice, string prodSku, string minStorePrice,
                      string maxStorePrice, DealFinderContext dealFinderContext, string api,
                      bool encode)
        {
            var queryParams = new StringBuilder();

            queryParams.Append(string.Format("productURL={0}&", HttpUtility.UrlEncode(offerUrl)));
            queryParams.Append(string.Format("domainName={0}&", dealFinderContext.DomainName));
            queryParams.Append(string.Format("pageId={0}&", pageId));
            queryParams.Append(string.Format("currency={0}&", dealFinderContext.Currency));
            queryParams.Append(string.Format("category={0}&", dealFinderContext.Category));
            queryParams.Append(string.Format("country={0}&", dealFinderContext.Country));
            queryParams.Append(string.Format("impressionId={0}&", dealFinderContext.ImpressionId));
            queryParams.Append(string.Format("userId={0}&", dealFinderContext.UserId));
            queryParams.Append(string.Format("campaignId={0}&", dealFinderContext.CampaignId));
            queryParams.Append(string.Format("shownProducts={0}&", shownProducts));
            queryParams.Append(string.Format("shownStores={0}&", shownStores));
            queryParams.Append(string.Format("storeApi={0}&", api));
            queryParams.Append(string.Format("storePos={0}&", storePos));
            queryParams.Append(string.Format("storePrice={0}&", storePrice));
            queryParams.Append(string.Format("productPrice={0}&", prodPrice));
            queryParams.Append(string.Format("minStorePrice={0}&", minStorePrice));
            queryParams.Append(string.Format("maxStorePrice={0}&", maxStorePrice));
            queryParams.Append(string.Format("clickType={0}&", 1));
            queryParams.Append(string.Format("prodSku={0}", prodSku));

            string url = queryParams.ToString();

            string clickUrl = null;

            if (!encode)
            {
                clickUrl = string.Format("{0}/?{1}", ConfigurationManager.AppSettings["ClickUrl"], url);
            }
            else
            {
                var query = new StringBuilder();
                var mongoProvider = new CseToMongoEtlProvider();
                var offerUrlId = mongoProvider.GetUrlId(url);

                query.Append(string.Format("data={0}", offerUrlId));
                query.Append(string.Format("&nid={0}", dealFinderContext.Nid));
                query.Append(string.Format("&sid={0}", dealFinderContext.Sid == null ? "0" : dealFinderContext.Sid));

                clickUrl = string.Format("{0}/?{1}", ConfigurationManager.AppSettings["ClickUrl"], query.ToString());
            }

            return clickUrl;
        }


        private DataRow[] GetDomainCache(string strDomain)
        {
            string exp = "DomainName = '" + strDomain + "'";
            string sortOrder = "Priority DESC,isPrefixCacheRegex DESC";
            DataTable dtCacheDomainsCache = CseToMongoEtlProvider.GetDataTable("dtCacheDomains", "", "SQLCacheConnectionString", 60);

            //  DataTable dtCacheDomainsCache = CU.GetCacheTable("dtCacheDomains", "SQLCacheConnectionString", 11);
            DataRow[] drw = new DataRow[0];
            try
            {
                //drw = dtCacheDomainsCache.Select(exp, sortOrder);
                DataView dv;
                lock (dtCacheDomainsCache)
                {
                    dv = new DataView(dtCacheDomainsCache, exp, sortOrder, DataViewRowState.Added);

                    if (dv.Count == 0)
                    {
                        dv = new DataView(dtCacheDomainsCache, exp, sortOrder, DataViewRowState.OriginalRows);
                    }

                    drw = dv.ToTable().Select();
                }
            }
            catch
            {
            }
            return drw;
        }
        private string GetCachedValue(DataRow[] dr, string FullURL, string strDomain, out int priority)
        {
            priority = 0;
            int idx = -1;
            int idx2 = -1;
            string lcURL = FullURL;
            string strCacheVal = "";
            string strCacheParam = "";
            string Suffix_Cut_String = "";
            string Suffix = "";
            int intSuffix_Cut_String = 0;
            DataRow tmpdr = null;
            byte caseSensitive = 1;
            int isRegEx = 0;
            Regex re = null;
            Match m = null;
            string strPrefixCache = "";
            string strSuffix = "";
            int isPrefixCacheRegex = 0;

            dr = dr.OrderBy(x => x["Priority"]).ToArray();

            for (int i = 0; i < dr.Length; i++)
            {
                priority = i + 1;

                Suffix = dr[i]["Suffix"].ToString();
                Suffix_Cut_String = "";
                intSuffix_Cut_String = 0;
                try
                {
                    caseSensitive = (byte)dr[i]["caseSensitive"];
                }
                catch
                {
                    caseSensitive = 1;
                }

                try
                {
                    isRegEx = (int)dr[i]["isRegEx"];
                }
                catch
                {
                    isRegEx = 0;
                }

                try
                {
                    isPrefixCacheRegex = (int)dr[i]["isPrefixCacheRegex"];
                }
                catch
                {
                    isPrefixCacheRegex = 0;
                }

                if (caseSensitive == 0)
                {
                    strDomain = strDomain.ToLower();
                    FullURL = FullURL.ToLower();
                    lcURL = FullURL;
                    for (int y = 0; y < dr[i].ItemArray.Length; y++)
                    {
                        try
                        {
                            dr[i][y] = dr[i][y].ToString().ToLower().Trim();
                        }
                        catch
                        {
                        }
                    }
                }
                try
                {
                    //Is use Regular Expression for Prefix Cache
                    strPrefixCache = dr[i]["PrefixCache"].ToString();
                    if (isPrefixCacheRegex == 1)
                    {
                        re = new Regex(strPrefixCache, RegexOptions.Compiled);
                        if (re.IsMatch(lcURL))
                        {
                            m = re.Match(lcURL);
                            strPrefixCache = m.Value;
                        }
                        else
                            continue;

                    }

                    //Is use Regular Expression
                    strSuffix = dr[i]["Suffix"].ToString();
                    if (isRegEx == 1)
                    {
                        re = new Regex(strSuffix, RegexOptions.Compiled);
                        if (re.IsMatch(lcURL))
                        {
                            m = re.Match(lcURL);
                            strSuffix = m.Value;
                        }
                        else
                            continue;

                    }
                    //Check Prefix
                    if (dr[i].ItemArray[1] != null && dr[i].ItemArray[1].ToString() != "" && dr[i].ItemArray[1].ToString() != "null")
                    {
                        idx = lcURL.IndexOf(strDomain);
                        idx2 = lcURL.IndexOf(dr[i].ItemArray[1].ToString());
                        if (idx2 < 0 || idx2 > idx)
                            continue;
                    }
                    //Check Suffix
                    if (strSuffix != null && strSuffix != "" && strSuffix != "null")
                    {
                        idx = lcURL.IndexOf(strDomain);
                        if (isRegEx == 1)
                        {
                            idx = idx2 = lcURL.IndexOf(strSuffix) + strSuffix.Length;
                            idx--;
                        }
                        else
                        {
                            idx = lcURL.IndexOf("/", idx);

                            if (idx < 0)
                                continue;
                            idx2 = lcURL.IndexOf(strSuffix, idx);
                        }
                        if (idx2 < 0 || idx2 != idx + 1)
                            continue;
                    }


                    if (dr[i].ItemArray[5] != null && dr[i].ItemArray[5].ToString() != "" && dr[i].ItemArray[5].ToString() != "null")
                    {
                        strCacheParam = "&" + dr[i].ItemArray[5].ToString() + "=";
                        idx = lcURL.IndexOf(strCacheParam);
                        if (idx < 0)
                        {
                            strCacheParam = "?" + dr[i].ItemArray[5].ToString() + "=";
                            idx = lcURL.IndexOf(strCacheParam);
                        }
                        if (idx >= 0)
                        {
                            if (FullURL.IndexOf("&", idx + 1) > 0)
                                strCacheVal = lcURL.Substring(idx + strCacheParam.Length, FullURL.IndexOf("&", idx + 1) - idx - strCacheParam.Length);
                            else
                                strCacheVal = lcURL.Substring(idx + strCacheParam.Length, FullURL.Length - idx - strCacheParam.Length);
                            strCacheVal = strCacheVal.Replace("=", "");
                            strCacheVal = strCacheVal.Replace("?", "");
                            strCacheVal = strCacheVal.Replace("&", "");
                            break;
                        }
                    }
                    else
                        //Check start and end parameter
                        if (strPrefixCache != null && strPrefixCache != "" && strPrefixCache != "null" &&
                            dr[i].ItemArray[4] != null && dr[i].ItemArray[4].ToString() != "null")
                        {
                            int t = 0;
                            if (isRegEx == 1)
                            {
                                t = strSuffix.Length - 1;
                                if (t < 0)
                                    t = 0;
                            }

                            idx = lcURL.IndexOf(strPrefixCache) + t;
                            idx2 = lcURL.IndexOf(dr[i].ItemArray[4].ToString(), idx + strPrefixCache.Length + 1);
                            if (dr[i].ItemArray[4].ToString() == "")
                            {
                                idx2 = lcURL.Length;
                            }
                            if (tmpdr == null && idx > 0)
                                tmpdr = dr[i];
                            if (idx > 0 && idx < idx2)
                            {
                                strCacheVal = lcURL.Substring(idx + strPrefixCache.Length, idx2 - idx - strPrefixCache.Length);
                                Suffix_Cut_String = dr[i]["Suffix_Cut_String"].ToString().Trim();
                                intSuffix_Cut_String = strCacheVal.IndexOf(dr[i]["Suffix_Cut_String"].ToString().Trim());
                                if (Suffix_Cut_String != "" && intSuffix_Cut_String > 0)
                                    strCacheVal = strCacheVal.Substring(0, intSuffix_Cut_String);
                                break;
                            }
                        }
                }
                catch (Exception ex)
                {
                    strCacheVal = "op_error";
                }

            }

            if (strCacheVal == "" && tmpdr != null && tmpdr.ItemArray[3].ToString() != "")
            {
                idx = lcURL.IndexOf(tmpdr.ItemArray[3].ToString());
                strCacheVal = lcURL.Substring(idx + tmpdr.ItemArray[3].ToString().Length);

                Suffix_Cut_String = tmpdr["Suffix_Cut_String"].ToString().Trim();
                intSuffix_Cut_String = strCacheVal.IndexOf(tmpdr["Suffix_Cut_String"].ToString().Trim());
                if (Suffix_Cut_String != "" && intSuffix_Cut_String > 0)
                    strCacheVal = strCacheVal.Substring(0, intSuffix_Cut_String);

            }
            return strCacheVal;
        }
    }
}
