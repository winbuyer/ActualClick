using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using WinBuyer.B2B.CseToMongoEtl.Entities;
using MongoDB.Bson;

namespace WinBuyer.B2B.CseToMongoEtl.Providers
{
    public class PcmToMongoEtlProvider
    {
        private const string PCM_COLLECTION = "pcm";
        private const string PCM_DB = "catalogs";

        private MongoDBServerConnectionWrapper _server = MongoDBServerConnectionWrapper.GetInstance();

        public void UpsertPcm(List<CsePcmInfo> products)
        {
            if (products == null || products.Count == 0)
                return;

            for (int i = 0; i < products.Count; i++)
            {
                var product = GetProduct(products[i].Domain, products[i].Sku, "_id");

                if (product == null)
                    InsertProduct(products[i]);
                else
                    UpdateProduct(products[i]);
            }
        }
        public void UpsertPcm(CsePcmInfo product)
        {
            if (product == null)
                return;

            var result = GetProduct(product.Domain, product.Sku, "_id");

            if (result == null)
                InsertProduct(product);
            else
                UpdateProduct(product);
        }
        public void UpdateProduct(CsePcmInfo product)
        {
            var dataBase = _server.ServerConnection.GetDatabase(PCM_DB);
            var collection = dataBase.GetCollection<CseProductInfo>(PCM_COLLECTION);

            var updateBuilder = new UpdateBuilder();

            if (product.Brand != null)
                updateBuilder.Set("brand", product.Brand);
            if (product.BrandOrigin != null)
                updateBuilder.Set("brand_origin", product.BrandOrigin);
            if (product.ImageUrl != null)
                updateBuilder.Set("image_url", product.ImageUrl);
            if (product.Isbn != null)
                updateBuilder.Set("isbn", product.Isbn);
            if (product.Mpn != null)
                updateBuilder.Set("mpn", product.Mpn);
            if (product.MpnOrigin != null)
                updateBuilder.Set("mpn_origin", product.MpnOrigin);
            if (product.Matches != null)
                updateBuilder.SetWrapped("matches", product.Matches);
            if (product.ProductDisplayName != null)
                updateBuilder.Set("product_display_name", product.ProductDisplayName);
            if (product.ProductName != null)
                updateBuilder.Set("product_name", product.ProductName);
            if (product.Sku != null)
                updateBuilder.Set("sku", product.Sku);
            if (product.Upc != null)
                updateBuilder.Set("upc", product.Upc);
            if (product.Price != null)
                updateBuilder.Set("price", product.Price);
            if (product.ShippingPrice != null)
                updateBuilder.Set("shipping_price", product.ShippingPrice);
            if (product.Status != null)
                updateBuilder.Set("status", product.Status);
            if (product.DomainPriceRule != null)
                updateBuilder.Set("domain_price_rule", product.DomainPriceRule);

            updateBuilder.Set("date_modified", DateTime.Now);

            collection.Update(Query.And(Query.EQ("domain", product.Domain), Query.EQ("sku", product.Sku)), updateBuilder);
        }
        public CsePcmInfo InsertProduct(CsePcmInfo product)
        {
            var dataBase = _server.ServerConnection.GetDatabase(PCM_DB);
            var collection = dataBase.GetCollection<CseProductInfo>(PCM_COLLECTION);

            product.DateCreated = DateTime.Now;
            product.DateModified = DateTime.Now;

            collection.Insert(product, new MongoInsertOptions()
            {
                SafeMode = SafeMode.False
            });

            return product;
        }
        public void DeleteProduct(CsePcmInfo product)
        {
            var dataBase = _server.ServerConnection.GetDatabase(PCM_DB);
            var collection = dataBase.GetCollection<CseProductInfo>(PCM_COLLECTION);

            collection.Remove(Query.And(Query.EQ("sku", product.Sku), Query.EQ("domain", product.Domain)));
        }
        public CsePcmInfo GetProduct(string domain, string sku, params string[] fields)
        {
            var dataBase = _server.ServerConnection.GetDatabase(PCM_DB);
            var collection = dataBase.GetCollection<CsePcmInfo>(PCM_COLLECTION);

            var result = collection
                      .Find(Query.And(Query.EQ("domain", domain), Query.EQ("sku", sku)))
                      .SetFields(Fields.Include(fields))
                      .SetLimit(1)
                      .SingleOrDefault();

            return result;
        }
        public CsePcmInfo GetProduct(ObjectId productId, params string[] fields)
        {
            var dataBase = _server.ServerConnection.GetDatabase(PCM_DB);
            var collection = dataBase.GetCollection<CsePcmInfo>(PCM_COLLECTION);

            var result = collection
                      .Find(Query.EQ("_id", productId))
                      .SingleOrDefault();

            return result;
        }
    }
}
