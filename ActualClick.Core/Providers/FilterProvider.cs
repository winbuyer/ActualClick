using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinBuyer.B2B.CseToMongoEtl.Entities;
using MoreLinq;
using WinBuyer.B2B.CseToMongoEtl.Providers;

namespace WinBuyer.B2B.DealFinder.Core.Providers.Market
{
    public class FilterProvider
    {
        private DealFinderContext _context = null;

        public FilterProvider(DealFinderContext context)
        {
            _context = context;
        }

        public List<CseProductInfo> FilterPriceRange(List<CseProductInfo> products)
        {
            if (_context.IsDefaultPrice)
                return products;

            dynamic domainPriceInterval = GetDomainPriceInterval();

            double lowerPrice = _context.Price - (_context.Price * domainPriceInterval.min);
            double higherPrice = (_context.Price * domainPriceInterval.max) + _context.Price;

            _context.DomainPriceRule = string.Format("{0}%-{1}%", domainPriceInterval.min * 100, domainPriceInterval.max * 100);

            for (int i = products.Count - 1; i > -1; i--)
            {
                for (int z = products[i].Offers.Count - 1; z > -1; z--)
                {
                    if (products[i].Offers[z].Price > higherPrice ||
                        products[i].Offers[z].Price < lowerPrice)
                    {
                        products[i].Offers.RemoveAt(z);
                    }
                }

                if (products[i].Offers.Count == 0)
                    products.RemoveAt(i);
            }

            return products;
        }
        public List<CseProductInfo> FilterSameStore(List<CseProductInfo> products)
        {
            for (int i = products.Count - 1; i > -1; i--)
            {
                for (int z = products[i].Offers.Count - 1; z > -1; z--)
                {
                    string midToFilter = GetDomainToMidMapping(products[i].ShoppingEngine);

                    if (products[i].Offers[z].MerchantId == midToFilter)
                        products[i].Offers.RemoveAt(z);
                }

                if (products[i].Offers.Count == 0)
                    products.RemoveAt(i);
            }

            return products;
        }
        public List<CseProductInfo> FilterDuplicateStores(List<CseProductInfo> products)
        {
            for (int i = products.Count - 1; i > -1; i--)
            {
                for (int z = products[i].Offers.Count - 1; z > -1; z--)
                {
                    if (z > products[i].Offers.Count - 1)
                        z = products[i].Offers.Count - 1;

                    string globalMid = products[i].Offers[z].MerchantInfo.GlobalId;

                    FilterDuplicateStores(globalMid, products);
                }

                if (products[i].Offers.Count == 0)
                    products.RemoveAt(i);
            }

            return products;
        }
        public List<CseProductInfo> FilterDuplicateProductId(List<CseProductInfo> products)
        {
            var result = products.DistinctBy(x => new
            {
                x.ProductId,
                x.ShoppingEngine
            }).ToList();

            return result;
        }

        private void FilterDuplicateStores(string globalMid, List<CseProductInfo> products)
        {
            bool foundFirst = false;

            for (int i = products.Count - 1; i > -1; i--)
            {
                for (int z = products[i].Offers.Count - 1; z > -1; z--)
                {
                    if (globalMid == products[i].Offers[z].MerchantInfo.GlobalId)
                    {
                        if (!foundFirst)
                        {
                            foundFirst = true;
                            continue;
                        }
                        else
                        {
                            products[i].Offers.RemoveAt(z);
                        }
                    }
                }
            }
        }
        private dynamic GetDomainPriceInterval()
        {
            var dt = CseToMongoEtlProvider.GetDataTable("Get_DomainPriceRange",
            new Dictionary<string, object>
              {
                  {"@domain", _context.DomainName}
              },
            "SQLCacheConnectionString", 24);

            if (dt == null || dt.Rows.Count == 0)
            {
                return new
                {
                    min = 0,
                    max = 0
                };
            }
            else
            {
                return new
                {
                    min = (double)((int)dt.Rows[0]["min"]) / 100,
                    max = (double)((int)dt.Rows[0]["max"]) / 100
                };
            }
        }
        private string GetDomainToMidMapping(int cse)
        {
            var dt = CseToMongoEtlProvider.GetDataTable("Get_Domain_Mids",
            new Dictionary<string, object>
              {
                  {"@domain", _context.DomainName}
              },
            "SQLCacheConnectionString", 24);

            if (dt == null || dt.Rows.Count == 0)
                return "undefined";

            var rows = dt.Select("api = " + cse.ToString());

            if (rows.Length == 0)
                return "undefined";

            string mid = rows[0][0] as string;

            return mid;
        }
    }
}
