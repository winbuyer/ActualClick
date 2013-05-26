using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Nancy.Helpers;
using WinBuyer.B2B.CseToMongoEtl.Providers;

namespace WinBuyer.B2B.DealFinder.Core.Providers
{
    public class RetargetingProvider
    {
        private DealFinderContext _context = null;

        public RetargetingProvider(DealFinderContext context)
        {
            _context = context;
        }

        public dynamic GetRetargetingInfo()
        {
            if (string.IsNullOrEmpty(_context.RtStack))
                return null;

            dynamic rtSettings = GetRtSettings();

            if (rtSettings == null)
                return null;

            if (!rtSettings.is_active)
                return null;

            bool isEcommerceDomain = IsEcommerceDomain();
            int showCount = int.Parse(_context.RtShowCount);

            if (!isEcommerceDomain && showCount > rtSettings.freq)
                return null;

            var lastShownTime = new DateTime(long.Parse(_context.RtLastShown));
            var currentTime = DateTime.Now;

            var span = currentTime - lastShownTime;

            int rec = isEcommerceDomain ? rtSettings.e_com_rec : rtSettings.non_e_com_rec;

            if (rec == 0)
                return null;

            if (span.TotalMinutes < rec)
                return null;

            dynamic stack = JObject.Parse(HttpUtility.UrlDecode(_context.RtStack));
            dynamic list = new List<JToken>();

            if (stack.stack != null)
                list = ((JArray)stack.stack).ToList();

            if (list == null || list.Count == 0)
                return null;

            int pointer = int.Parse(_context.RtPointer);

            var product = ObjectId.Parse(list[pointer].ToString());
            pointer--;

            if (pointer == -1)
                pointer = list.Count - 1;

            if (!isEcommerceDomain)
                showCount++;

            return new
            {
                campaign_id = rtSettings.rt_campaign_id,
                product_id = product,
                pointer = pointer.ToString(),
                show_count = showCount,
                type = isEcommerceDomain ? "30K" : "non_30K"
            };
        }
        public string UpdateProductIdsStack(string rtStack, string productId, out int stackCount)
        {
            dynamic stack = JObject.Parse(HttpUtility.UrlDecode(rtStack));
            dynamic list = new List<JToken>();
            string response = null;

            if (stack.stack != null)
                list = ((JArray)stack.stack).ToList();

            if (((List<JToken>)list).FirstOrDefault(x => x.ToString() == productId) != null)
            {
                response = JsonConvert.SerializeObject(stack);
                stackCount = list.Count;

                return response;
            }

            if (list.Count >= 5)
                list.RemoveAt(0);

            list.Add(productId);

            stack.stack = JArray.FromObject(list);

            response = JsonConvert.SerializeObject(stack);

            stackCount = list.Count;

            return response;
        }
        public dynamic GetRtSettings()
        {
            var dataTable = CseToMongoEtlProvider.GetDataTable("sp_GET_DealFinder_RT_Settings",
            new Dictionary<string, object>
            {
                {"@CampaignId", _context.CampaignId}
            },
            "SQLCacheConnectionString", 60);

            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                return new
                {
                    rt_campaign_id = "",
                    is_active = false,
                    freq = 0,
                    non_e_com_rec = 0,
                    e_com_rec = 0
                };
            }

            return new
            {
                rt_campaign_id = (string)dataTable.Rows[0]["CampaignID_RT"],
                is_active = ((byte)dataTable.Rows[0]["active"]) == 1 ? true : false,
                freq = (short)dataTable.Rows[0]["Frequency_Not30K"],
                non_e_com_rec = (short)dataTable.Rows[0]["Reccency_Not30K"],
                e_com_rec = (short)dataTable.Rows[0]["Reccency_30K"]
            };
        }
        public bool IsEcommerceDomain()
        {
            var dataTable = CseToMongoEtlProvider.GetDataTable("sp_Domain_in30K",
            new Dictionary<string, object>
            {
                {"@Domain", _context.DomainName}
            },
            "SQLCacheConnectionString", 60);

            if (dataTable == null || dataTable.Rows.Count == 0)
                return false;

            return ((int)dataTable.Rows[0][0]) == 0 ? false : true;
        }
    }
}
