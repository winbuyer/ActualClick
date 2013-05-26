using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinBuyer.Infrastructure.BusinessMessages.Interfaces.DealFinder;
using WinBuyer.Infrastructure.BusinessMessages.Core;
using System.Web;
using Newtonsoft.Json.Linq;

namespace WinBuyer.B2B.DealFinder.Core.Providers
{
    public class BusinessLoggerProvider
    {
        public void LogClick(string prodPrice, string shownStores, string shownProducts, string storeApi,
                       string storeMid, string storePid, string storePos, string storePrice, string pageId,
                       string prodSku, string minStorePrice, string maxStorePrice, string clickType, string productUrl, string op,
                       DealFinderContext dealFinderContext)
        {
            DfClickBusinessMessage click = new DfClickBusinessMessage("df", "df_click", DateTime.Now);

            click.ClickType = clickType;
            click.CampaignID = dealFinderContext.CampaignId.ToString();
            click.SubCampaignID = dealFinderContext.Country;
            click.DomainName = dealFinderContext.DomainName;
            click.PageID = pageId;
            click.Query = op;
            click.RequestIP = dealFinderContext.Ip;
            click.MachineIP = dealFinderContext.HostName;
            click.UserAgent = dealFinderContext.UserAgent;
            click.URL = productUrl;
            click.Application = "DealFinder";
            click.IMPID = dealFinderContext.ImpressionId;
            click.ShownOffers = shownStores;
            click.ShownProducts = shownProducts;
            click.ProdPrice = prodPrice;
            click.API = storeApi;
            click.MID = storeMid;
            click.PID = storePid;
            click.StorePos = storePos;
            click.StorePrice = storePrice;
            click.Rendered = "0";
            click.GUID = dealFinderContext.UserId;
            click.Currency = dealFinderContext.Category;
            click.SKU = prodSku;
            click.MinPrice = minStorePrice;
            click.MaxPrice = maxStorePrice;

            BusinessMessageWriter writer = new BusinessMessageWriter();

            //writer.WriteMessageToQueue(click);
        }

        public void LogImpression(string shownProducts, string shownStores, string minStoresPrice, string maxStoresPrice,
                           string prodPrice, string prodSku, string storePos, string storePrice, string storeMid,
                           string storeApi, string storePid, string query, DealFinderContext dealFinderContext)
        {
            DfImpressionBusinessMessage impression = new DfImpressionBusinessMessage("df", "df_impression", DateTime.Now);

            impression.AbTestId = dealFinderContext.AbTestGroupId;
            impression.AbTestStatus = dealFinderContext.AbTestShowWidget;
            impression.Application = "DealFinder";
            impression.CampaignID = dealFinderContext.CampaignId.ToString();
            impression.CategoryID = dealFinderContext.Category;
            impression.Currency = dealFinderContext.Currency;
            impression.DomainName = dealFinderContext.DomainName;
            impression.GUID = dealFinderContext.UserId;
            impression.IMPID = dealFinderContext.ImpressionId;
            impression.MachineIP = dealFinderContext.HostName;
            impression.RequestIP = dealFinderContext.Ip;
            impression.MessageVersion = "1";
            impression.PageID = dealFinderContext.Sid;
            impression.PageType = string.IsNullOrEmpty(dealFinderContext.PageType) ? "category" : dealFinderContext.PageType;
            impression.Query = query;
            impression.Rendered = dealFinderContext.CookiesEnabled ? "0" : "-1";
            impression.RequestRaw = dealFinderContext.RequestUrl;
            impression.SubCampaignID = dealFinderContext.Country;
            impression.URL = HttpUtility.UrlDecode(dealFinderContext.Referrer);
            impression.UserAgent = dealFinderContext.UserAgent;
            impression.WidgetAlgorithm = "default";

            impression.SKU = prodSku;
            impression.MID = storeMid;
            impression.PID = storePid;
            impression.API = storeApi;

            impression.ProdPrice = prodPrice;
            impression.ShownOffers = shownStores;
            impression.ShownProducts = shownProducts;
            impression.MinPrice = minStoresPrice;
            impression.MaxPrice = maxStoresPrice;
            impression.StorePos = storePos;
            impression.StorePrice = storePrice;

            BusinessMessageWriter writer = new BusinessMessageWriter();

            //writer.WriteMessageToQueue(impression);
        }

        public void LogPerformance(string domain, string campaignId, string guid, string impId, string url, string sku, dynamic timers)
        {
            DfPerfomanceCountersBusinessMessage performance = new DfPerfomanceCountersBusinessMessage("df", "df_performance_counters", DateTime.Now);

            performance.CampaignID = campaignId;
            performance.DomainName = domain;
            performance.GUID = guid;
            performance.IMPID = impId;
            performance.URL = url;
            performance.Res1 = sku;

            var timersList = (timers as JArray).ToList();

            foreach (var timer in timersList)
            {
                switch (timer["name"].ToString())
                {
                    case "get-domain-triggers":
                        performance.GetDomainTriggers = string.Format("{0:0}", timer["time"]);
                        break;
                    case "domain-triggers-match-time":
                        performance.DomainTriggersMatchTime = string.Format("{0:0}", timer["time"]);
                        break;
                    case "extract-cache-value-and-domain":
                        performance.ExtractCacheValueAndDomain = string.Format("{0:0}", timer["time"]);
                        break;
                    case "get-url-mapping-data":
                        performance.GetUrlMappingData = string.Format("{0:0}", timer["time"]);
                        break;
                    case "global-server-time":
                        performance.GlobalServerTime = string.Format("{0:0}", timer["time"]);
                        break;
                    case "global-client-time":
                        performance.GlobalClientTime = string.Format("{0:0}", timer["time"]);
                        break;
                    case "get-data-from-pcm":
                        performance.GetDataFromPcm = string.Format("{0:0}", timer["time"]);
                        break;
                    case "insert-product-to-pcm":
                        performance.InsertProductToPcm = string.Format("{0:0}", timer["time"]);
                        break;
                    case "get-site-data":
                        performance.GetSiteData = string.Format("{0:0}", timer["time"]);
                        break;
                    case "get-campaign-apis-sql":
                        performance.GetCampaignApisFromSQL = string.Format("{0:0}", timer["time"]);
                        break;


                    case "get-data-from-cse-[2]-api":
                        performance.GetDataFromCse2 = string.Format("{0:0}", timer["time"]);
                        break;
                    case "get-data-from-cse-[3]-api":
                        performance.GetDataFromCse3 = string.Format("{0:0}", timer["time"]);
                        break;
                    case "get-data-from-cse-[4]-api":
                        performance.GetDataFromCse4 = string.Format("{0:0}", timer["time"]);
                        break;
                    case "get-data-from-cse-[6]-api":
                        performance.GetDataFromCse6 = string.Format("{0:0}", timer["time"]);
                        break;
                    case "get-data-from-cse-[9]-api":
                        performance.GetDataFromCse9 = string.Format("{0:0}", timer["time"]);
                        break;
                    case "get-data-from-cse-[10]-api":
                        performance.GetDataFromCse10 = string.Format("{0:0}", timer["time"]);
                        break;

                    //case "insert-data-to-cse-[2]":
                    //    performance.InsertDataToCse2 = string.Format("{0:0}", timer["time"]);
                    //    break;
                    //case "insert-data-to-cse-[3]":
                    //    performance.GetDataFromCse3 = string.Format("{0:0}", timer["time"]);
                    //    break;
                    //case "insert-data-to-cse-[4]":
                    //    performance.GetDataFromCse4 = string.Format("{0:0}", timer["time"]);
                    //    break;
                    //case "insert-data-to-cse-[6]":
                    //    performance.GetDataFromCse6 = string.Format("{0:0}", timer["time"]);
                    //    break;
                    //case "insert-data-to-cse-[9]":
                    //    performance.GetDataFromCse9 = string.Format("{0:0}", timer["time"]);
                    //    break;
                    //case "insert-data-to-cse-[10]":
                    //    performance.GetDataFromCse10 = string.Format("{0:0}", timer["time"]);
                    //    break;

                    //case "update-cse-[2]-mongo":
                    //    performance.UpdateCse2 = string.Format("{0:0}", timer["time"]);
                    //    break;
                    //case "update-cse-[3]-mongo":
                    //    performance.UpdateCse3 = string.Format("{0:0}", timer["time"]);
                    //    break;
                    //case "update-cse-[4]-mongo":
                    //    performance.UpdateCse4 = string.Format("{0:0}", timer["time"]);
                    //    break;
                    //case "update-cse-[6]-mongo":
                    //    performance.UpdateCse6 = string.Format("{0:0}", timer["time"]);
                    //    break;
                    //case "update-cse-[9]-mongo":
                    //    performance.UpdateCse9 = string.Format("{0:0}", timer["time"]);
                    //    break;
                    //case "update-cse-[10]-mongo":
                    //    performance.UpdateCse10 = string.Format("{0:0}", timer["time"]);
                    //    break;
                }
            }

            BusinessMessageWriter writer = new BusinessMessageWriter();

            //writer.WriteMessageToQueue(performance);
        }
        public void LogPerformance(string url, dynamic context, dynamic timers)
        {
            LogPerformance(context.DomainName, context.CampaignId, context.UserId, context.ImpressionId, url, context.CachedValue, timers);
        }
    }
}
