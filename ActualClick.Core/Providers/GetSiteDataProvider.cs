using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WidgetSettings;
using Widget.logic.Classes;
using Nancy.Helpers;
using System.Dynamic;

namespace WinBuyer.B2B.DealFinder.Core.Providers.Market
{
    public class GetSiteDataProvider
    {
        private DealFinderContext _context = null;
        private readonly bool _getDebugParams = false;

        public GetSiteDataProvider(DealFinderContext context, bool getDebugParams)
        {
            _context = context;
            _getDebugParams = getDebugParams;
        }

        public dynamic GetSiteData()
        {
            var result = GetSiteDataAction();

            return result;
        }

        private dynamic GetSiteDataAction()
        {
            var data = new Settings.datastruct()
            {
                kw = "",
                AffiliateID = int.Parse(_context.CampaignId),
                RefUrl = HttpUtility.UrlEncode(_context.Url),
                debug = _getDebugParams
            };

            var siteData = new DataUtils().Get_SiteData(data);

            if (siteData == null || string.IsNullOrEmpty(siteData[0]))
                return null;

            string upc = null;
            string isbn = null;

            if (!string.IsNullOrEmpty(siteData[4]))
            {
                if (siteData[4] == "0")
                    upc = siteData[3];
                else
                    isbn = siteData[3];
            }

            dynamic result = new ExpandoObject();

            result.productName = siteData[0];
            result.mfg = siteData[1];
            result.price = siteData[2];
            result.upc = upc;
            result.isbn = isbn;
            result.productNameMatchBy = siteData[5];
            result.productPriceMatchBy = siteData[6];

            _context.ProductName = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(siteData[0].Replace("+", " ")));
            _context.Mfg = siteData[1];
            _context.Price = double.Parse(siteData[2]);
            _context.Upc = upc;
            _context.Isbn = isbn;
            _context.ProductNameMatchedBy = siteData[5];
            _context.ProductPriceMatchedBy = siteData[6];
            _context.IsDefaultPrice = siteData[6] == "defaultprice" ? true : false;

            return result;
        }
    }
}
