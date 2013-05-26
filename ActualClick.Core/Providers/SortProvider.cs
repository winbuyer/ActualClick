using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace WinBuyer.B2B.DealFinder.Core.Providers.Market
{
    public class SortProvider
    {
        private DealFinderContext _context = null;

        public SortProvider(DealFinderContext context)
        {
            _context = context;
        }

        public List<dynamic> SortProducts(List<dynamic> products)
        {
            products = products.OrderBy(x => (double)x.product_price_raw).ToList();

            return products;
        }
        public List<dynamic> RandomizeProducts(List<dynamic> products, int limit)
        {
            products.Shuffle();

            return products.Take(limit).ToList();
        }
    }

    public static class ListExtentions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do
                    provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
