using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinBuyer.B2B.CseToMongoEtl.Providers;
using WinBuyer.B2B.CseToMongoEtl.Entities;
using System.Threading;
using Nancy.Helpers;
using log4net;
using System.Reflection;
using WinBuyer.Optimization.Model;
using WinBuyer.B2B.CSEsConsumer;

namespace WinBuyer.B2B.DealFinder.Core.Providers.Market
{
    public class ProductsProvider
    {
        private DealFinderContext _context = null;

        private string _keyword = null;

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ProductsProvider(DealFinderContext context)
        {
            _context = context;
            _keyword = HttpUtility.UrlDecode(context.ProductName);
        }

        public List<CseProductInfo> GetProductsFromPcm(CsePcmInfo pcmInfo)
        {
            var products = GetProductsFromPcmAction(pcmInfo, _context.Op);

            if (products.Count == 0)
            {
                products = GetProductsFromCseAction(1);

                var pcmProvider = new PcmProvider(_context);
                pcmProvider.UpsertProductToPcm(products);

                return products;
            }

            var filterProvider = new FilterProvider(_context);

            products = filterProvider.FilterSameStore(products);
            products = filterProvider.FilterPriceRange(products);
            products = filterProvider.FilterDuplicateStores(products);
            products = filterProvider.FilterDuplicateProductId(products);

            return products;
        }
        public List<CseProductInfo> GetProductsFromCse(int minNumberOfOffers)
        {
            var result = GetProductsFromCseAction(minNumberOfOffers);

            return result;
        }

        private List<CseProductInfo> GetProductsFromPcmAction(CsePcmInfo pcmInfo, string op)
        {
            var matches = pcmInfo.Matches;
            var cseDataProvider = new CseToMongoEtlProvider();
            var products = new List<CseProductInfo>();

            foreach (var match in matches)
            {
                foreach (var pid in match.Pids)
                {
                    CseProductInfo product = null;

                    if (match.Cse == 9 || match.Cse == 10)
                        product = cseDataProvider.GetProduct(match.Cse, op + "_" + pid);
                    else
                        product = cseDataProvider.GetProduct(match.Cse, pid);

                    if (product == null)
                        return new List<CseProductInfo>();

                    foreach (var offer in product.Offers)
                        offer.MerchantInfo = cseDataProvider.GetMerchant(match.Cse, offer.MerchantId);

                    products.Add(product);
                }

                _context.AddCseOptimizationMapping(match.Cse.ToString(), match.ModifiedBy);
            }

            return products;
        }
        private List<CseProductInfo> GetProductsFromCseAction(int minNumberOfOffers)
        {
            var timer = new TimerProvider(_context, "get-campaign-apis-sql");
            var apis = GetCseApisAndCurrencyForCurrentCampaignId();
            timer.StopTimer();

            if (apis.Count == 0)
                return new List<CseProductInfo>();

            var cseDataProvider = new CseToMongoEtlProvider();

            var productsList = new List<CseProductInfo>();
            var rawProductsList = new List<CseProductInfo>();
            var merchantsList = new List<CseMerchantInfo>();

            int totalNumberOfOffers = 0;

            while (true)
            {
                for (int i = 0; i < apis.Count; i++)
                {
                    MatchingData matchingData = null;

                    try
                    {
                        matchingData = GetProtuctsFromCseByKeyword(apis[i]);

                        if (matchingData == null)
                            matchingData = new MatchingData();
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorFormat("failed get data from cse [{0}], ex: [{1}]", apis[i], ex.Message);
                        _context.AddCseExecutionFailure(apis[i].ToString(), "Failed");
                    }

                    _context.AddCseNumberOfRawResults(apis[i].ToString(), matchingData.Matches.Count.ToString());

                    if (matchingData.Matches.Count == 0)
                        continue;

                    _context.AddCseExecutionFailure(apis[i].ToString(), "OK");

                    List<CseProductInfo> products = null;
                    List<CseMerchantInfo> merchants = null;

                    cseDataProvider.ParseProductAndMerchants(apis[i], "widget", matchingData.ds, out products, out merchants);

                    if (products.Count == 0)
                        continue;

                    merchantsList.AddRange(merchants.Select(x => (CseMerchantInfo)x.Clone()));
                    rawProductsList.AddRange(products.Select(x => (CseProductInfo)x.Clone()));

                    var filterProvider = new FilterProvider(_context);

                    products = filterProvider.FilterSameStore(products);
                    products = filterProvider.FilterPriceRange(products);

                    if (products.Count == 0)
                        continue;

                    productsList.AddRange(products);

                    productsList = filterProvider.FilterDuplicateStores(productsList);
                    productsList = filterProvider.FilterDuplicateProductId(productsList);

                    if (productsList.Count == 0)
                        continue;

                    totalNumberOfOffers = CountProdcutOffers(productsList);

                    if (totalNumberOfOffers >= minNumberOfOffers)
                        break;
                }

                totalNumberOfOffers = CountProdcutOffers(productsList);

                if (totalNumberOfOffers >= minNumberOfOffers)
                    break;

                if (!_context.IsDefaultPrice)
                {
                    var filterProvider = new FilterProvider(_context);

                    rawProductsList = filterProvider.FilterDuplicateStores(rawProductsList);
                    rawProductsList = filterProvider.FilterDuplicateProductId(rawProductsList);

                    totalNumberOfOffers = CountProdcutOffers(rawProductsList);

                    if (totalNumberOfOffers >= minNumberOfOffers)
                    {
                        productsList = rawProductsList;
                        break;
                    }
                }

                if (!ShrinkKeywordStart())
                    break;
            }

            ThreadPool.QueueUserWorkItem(state =>
            {
                dynamic param = state as dynamic;

                cseDataProvider.AddCseResultToMongoDb("widget", _context.Op, param.products, param.merchants, false);

            }, new
            {
                products = new List<CseProductInfo>(rawProductsList.Select(x => (CseProductInfo)x.Clone())),
                merchants = new List<CseMerchantInfo>(merchantsList.Select(x => (CseMerchantInfo)x.Clone()))
            });

            return productsList;
        }

        private MatchingData GetProtuctsFromCseByKeyword(short api)
        {
            string encodedKeyword = HttpUtility.UrlEncode(_keyword);

            var timer = new TimerProvider(_context, string.Format("get-data-from-cse-[{0}]-api", api));
            var cseMatcher = new CseMatcher(api);
            var cseResult = cseMatcher.SearchKeyword(_context.Op.ToString(), _context.DomainName, encodedKeyword, 100);

            timer.StopTimer();

            return cseResult;
        }
        private bool ShrinkKeywordEnd()
        {
            var words = _keyword.Split(' ');

            if (words.Length == 1)
                return false;

            string shortKeyword = null;

            for (int i = 0; i < words.Length - 1; i++)
                shortKeyword += words[i] + " ";

            _keyword = shortKeyword.Trim();

            return true;
        }
        private bool ShrinkKeywordStart()
        {
            var words = _keyword.Split(' ');

            if (words.Length == 1)
                return false;

            string shortKeyword = null;

            for (int i = 1; i < words.Length; i++)
                shortKeyword += words[i] + " ";

            _keyword = shortKeyword.Trim();

            return true;
        }

        public static int CountProdcutOffers(List<CseProductInfo> products)
        {
            int counter = 0;

            foreach (var product in products)
            {
                foreach (var offer in product.Offers)
                    counter++;
            }

            return counter;
        }

        private List<short> GetCseApisAndCurrencyForCurrentCampaignId()
        {
            var dt = CseToMongoEtlProvider.GetDataTable("Get_Campaign_Apis",
              new Dictionary<string, object>
              {
                  {"CampaignID", string.IsNullOrEmpty(_context.Country) ? _context.CampaignId.ToString() : _context.Country}
              },
              "SQLCacheConnectionString", 24);

            if (dt == null || dt.Rows.Count == 0)
                return new List<short>();

            _context.Currency = dt.Rows[0][1].ToString();

            var apis = new List<short>();

            for (int i = 0; i < dt.Rows.Count; i++)
                apis.Add((short)dt.Rows[i][0]);

            return apis;
        }
    }
}
