using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinBuyer.B2B.CseToMongoEtl.Entities;
using WinBuyer.B2B.CseToMongoEtl.Providers;
using MongoDB.Bson;

namespace WinBuyer.B2B.DealFinder.Core.Providers.Market
{
    public class PcmProvider
    {
        private DealFinderContext _context = null;
        private PcmToMongoEtlProvider _pcmDataProvider = null;

        public PcmProvider(DealFinderContext context)
        {
            _context = context;
            _pcmDataProvider = new PcmToMongoEtlProvider();
        }

        public CsePcmInfo UpsertProductToPcm(List<CseProductInfo> products)
        {
            var result = UpsertProductToPcmAction(products);

            return result;
        }
        public CsePcmInfo GetProductFromPcmByCacheValueAndDomain()
        {
            var result = GetProductFromPcmByCacheValueAndDomainAction();

            return result;
        }
        public CsePcmInfo GetProductFromPcmById(ObjectId pcmId)
        {
            var result = GetProductFromPcmByIdAction(pcmId);

            return result;
        }

        private CsePcmInfo UpsertProductToPcmAction(List<CseProductInfo> products)
        {
            var pcmInfo = new CsePcmInfo();

            pcmInfo.DomainPriceRule = _context.DomainPriceRule;
            pcmInfo.PageType = _context.PageType;
            pcmInfo.Domain = _context.PageType == "search" ? _context.SearchDomainName : _context.DomainName;
            pcmInfo.Sku = _context.CachedValue;
            pcmInfo.Isbn = _context.Isbn;
            pcmInfo.Upc = _context.Upc;
            pcmInfo.IsDefaultPrice = _context.IsDefaultPrice;
            pcmInfo.Price = _context.Price;
            pcmInfo.ProductDisplayName = _context.ProductName;
            pcmInfo.ProductName = _context.ProductName.ToLower();
            pcmInfo.ProductUrl = _context.Url;
            pcmInfo.Currency = _context.Currency;
            pcmInfo.Status = 1;
            pcmInfo.Matches = new List<CseMatchInfo>();

            var matchGroup2 = products.Where(x => x.ShoppingEngine == 2).ToList();
            if (matchGroup2.Count > 0)
                pcmInfo.Matches.Add(ExtractMatchInfoFromProducts(matchGroup2));

            var matchGroup3 = products.Where(x => x.ShoppingEngine == 3).ToList();
            if (matchGroup3.Count > 0)
                pcmInfo.Matches.Add(ExtractMatchInfoFromProducts(matchGroup3));

            var matchGroup4 = products.Where(x => x.ShoppingEngine == 4).ToList();
            if (matchGroup4.Count > 0)
                pcmInfo.Matches.Add(ExtractMatchInfoFromProducts(matchGroup4));

            var matchGroup6 = products.Where(x => x.ShoppingEngine == 6).ToList();
            if (matchGroup6.Count > 0)
                pcmInfo.Matches.Add(ExtractMatchInfoFromProducts(matchGroup6));

            var matchGroup9 = products.Where(x => x.ShoppingEngine == 9).ToList();
            if (matchGroup9.Count > 0)
                pcmInfo.Matches.Add(ExtractMatchInfoFromProducts(matchGroup9));

            var matchGroup10 = products.Where(x => x.ShoppingEngine == 10).ToList();
            if (matchGroup10.Count > 0)
                pcmInfo.Matches.Add(ExtractMatchInfoFromProducts(matchGroup10));

            var timerStart = DateTime.Now;

            if (pcmInfo.Matches != null && pcmInfo.Matches.Count > 0)
                _pcmDataProvider.UpsertPcm(pcmInfo);

            var opTime = DateTime.Now - timerStart;

            return pcmInfo;
        }
        private CsePcmInfo GetProductFromPcmByCacheValueAndDomainAction()
        {
            string domainName = _context.PageType == "search" ? _context.SearchDomainName : _context.DomainName;

            var result = _pcmDataProvider.GetProduct(domainName, _context.CachedValue);

            return result;
        }
        private CsePcmInfo GetProductFromPcmByIdAction(ObjectId pcmId)
        {
            var result = _pcmDataProvider.GetProduct(pcmId);

            return result;
        }

        private CseMatchInfo ExtractMatchInfoFromProducts(List<CseProductInfo> products)
        {
            var productIds = products.Select(x => x.ProductId).ToList();

            var matchInfo = new CseMatchInfo()
            {
                Cse = products[0].ShoppingEngine,
                DateModified = DateTime.Now,
                MatchType = 3,
                ModifiedBy = "widget",
                Status = 1,
                Pids = productIds
            };

            return matchInfo;
        }
    }
}
