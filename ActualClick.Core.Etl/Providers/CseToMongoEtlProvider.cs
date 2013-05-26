using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using WinBuyer.B2B.CseToMongoEtl.Entities;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Web;
using System.Web.Caching;
using System.Data.SqlClient;
using System.Configuration;

namespace WinBuyer.B2B.CseToMongoEtl.Providers
{
    public class CseToMongoEtlProvider
    {
        private const string PRODUCTS_COLLECTION = "products";
        private const string MERCHANTS_COLLECTION = "merchants";

        private MongoDBServerConnectionWrapper _server = MongoDBServerConnectionWrapper.GetInstance();

        public List<CseProductInfo> AddCseResultToMongoDb(int cse, string source, DataSet dataSet, bool upadteIfExist = true)
        {
            if (dataSet == null || dataSet.Tables.Count == 0)
                return null;

            List<CseProductInfo> productsList = null;
            List<CseMerchantInfo> merchantsList = null;

            switch (cse)
            {
                case 2:
                    productsList = ParseShopZillaProducts(source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParseShopZillaMerchants(source, dataSet.Tables["Merchants"]);
                    break;
                case 3:
                    productsList = ParseShoppingProducts(3, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParseShoppingMerchants(3, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    break;
                case 4:
                    productsList = ParseShoppingProducts(4, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParseShoppingMerchants(4, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    break;
                case 6:
                    productsList = ParseAffiliateWindowProducts(source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParseAffiliateWindowMerchants(source, dataSet.Tables["Merchants"]);
                    break;
                case 9:
                    productsList = ParsePriceGrabberProducts(9, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParsePriceGrabberMerchants(9, source, dataSet.Tables["Merchants"]);
                    break;
                case 10:
                    productsList = ParsePriceGrabberProducts(10, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParsePriceGrabberMerchants(10, source, dataSet.Tables["Merchants"]);
                    break;
            }

            UpsertProducts(productsList, upadteIfExist);
            UpsertMerchants(merchantsList, upadteIfExist);

            for (int i = 0; i < productsList.Count; i++)
            {
                for (int z = 0; z < productsList[i].Offers.Count; z++)
                {
                    var merchantInfo = merchantsList.Find(mid => mid.MerchantId == productsList[i].Offers[z].MerchantId);
                    productsList[i].Offers[z].MerchantInfo = merchantInfo;
                }
            }

            return productsList;
        }
        public void AddCseResultToMongoDb(string source, string op, List<CseProductInfo> products, List<CseMerchantInfo> merchants, bool upadteIfExist = true)
        {
            products
                .Where(i => i.ShoppingEngine == 9 || i.ShoppingEngine == 10).ToList()
                .ForEach(i => i.ProductId = op + "_" + i.ProductId);

            UpsertProducts(products, upadteIfExist);
            UpsertMerchants(merchants, upadteIfExist);
        }
        public void ParseProductAndMerchants(int cse, string source, DataSet dataSet, out List<CseProductInfo> products, out List<CseMerchantInfo> merchants)
        {
            products = null;
            merchants = null;

            if (dataSet == null || dataSet.Tables.Count == 0)
                return;

            List<CseProductInfo> productsList = null;
            List<CseMerchantInfo> merchantsList = null;

            switch (cse)
            {
                case 2:
                    productsList = ParseShopZillaProducts(source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParseShopZillaMerchants(source, dataSet.Tables["Merchants"]);
                    break;
                case 3:
                    productsList = ParseShoppingProducts(3, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParseShoppingMerchants(3, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    break;
                case 4:
                    productsList = ParseShoppingProducts(4, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParseShoppingMerchants(4, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    break;
                case 6:
                    productsList = ParseAffiliateWindowProducts(source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParseAffiliateWindowMerchants(source, dataSet.Tables["Merchants"]);
                    break;
                case 9:
                    productsList = ParsePriceGrabberProducts(9, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParsePriceGrabberMerchants(9, source, dataSet.Tables["Merchants"]);
                    break;
                case 10:
                    productsList = ParsePriceGrabberProducts(10, source, dataSet.Tables["Products"], dataSet.Tables["ProductMerchants"]);
                    merchantsList = ParsePriceGrabberMerchants(10, source, dataSet.Tables["Merchants"]);
                    break;
            }

            foreach (var merchant in merchantsList)
                merchant.GlobalId = GetMerchantGlobalId(merchant.MerchantId, merchant.ShoppingEngine);

            for (int i = 0; i < productsList.Count; i++)
            {
                for (int z = 0; z < productsList[i].Offers.Count; z++)
                {
                    var merchantInfo = merchantsList.Find(mid => mid.MerchantId == productsList[i].Offers[z].MerchantId);
                    productsList[i].Offers[z].MerchantInfo = merchantInfo;
                }
            }

            products = productsList;
            merchants = merchantsList;
        }

        private List<CseProductInfo> ParseShoppingProducts(int cse, string source, DataTable products, DataTable productMerchants)
        {
            if (products == null || products.Rows.Count == 0)
                return null;

            var productsList = new List<CseProductInfo>();

            for (int i = 0; i < products.Rows.Count; i++)
            {
                var product = new CseProductInfo();

                product.ShoppingEngine = cse;
                product.Source = source;
                product.ProductId = products.Rows[i]["PID"] as string;
                product.CategoryId = products.Rows[i]["categoryID"] as string;
                product.Name = products.Rows[i]["name"] != DBNull.Value ? ((string)products.Rows[i]["name"]).ToLower() : null;
                product.DisplayName = products.Rows[i]["name"] as string;
                product.Description = products.Rows[i]["fullDescription"] as string;
                product.Image = products.Rows[i]["image"] as string;

                product.Offers = new List<CseOfferInfo>();

                if (productMerchants == null || productMerchants.Rows.Count == 0)
                    continue;

                var rows = productMerchants.Select("PID = '" + product.ProductId + "'");

                if (rows.Length == 0)
                    continue;

                for (int x = 0; x < rows.Length; x++)
                {
                    var offer = new CseOfferInfo();

                    offer.Url = rows[x]["offerUrl"] as string;
                    offer.Price = double.Parse((string)rows[x]["basePrice"]);
                    offer.Brand = rows[x]["brand"] != DBNull.Value ? (string)rows[x]["brand"] : null;
                    offer.Sku = rows[x]["sku"] != DBNull.Value ? (string)rows[x]["sku"] : null;
                    offer.Model = rows[x]["model"] != DBNull.Value ? (string)rows[x]["model"] : null;
                    offer.MerchantId = rows[x]["mid"] != DBNull.Value ? (string)rows[x]["mid"] : null;
                    offer.Cpc = rows[x]["cpc"] != DBNull.Value ? double.Parse((string)rows[x]["cpc"]) : 0;

                    product.Offers.Add(offer);
                }

                product.Offers = product.Offers.OrderBy(x => x.Price).ToList();
                product.MinPrice = product.Offers[0].Price;
                product.MaxPrice = product.Offers[product.Offers.Count - 1].Price;

                productsList.Add(product);
            }

            return productsList;
        }
        private List<CseMerchantInfo> ParseShoppingMerchants(int cse, string source, DataTable products, DataTable productMerchants)
        {
            if (productMerchants == null || productMerchants.Rows.Count == 0)
                return null;

            var merchants = new List<CseMerchantInfo>();

            for (int i = 0; i < productMerchants.Rows.Count; i++)
            {
                var merchant = new CseMerchantInfo();

                merchant.ShoppingEngine = cse;
                merchant.Source = source;
                merchant.MerchantId = productMerchants.Rows[i]["mid"] as string;
                merchant.Name = ((string)productMerchants.Rows[i]["name"]).ToLower();
                merchant.DisplayName = productMerchants.Rows[i]["name"] as string;
                merchant.Logo = productMerchants.Rows[i]["logo"] as string;
                merchant.Source = source;
                merchant.UseLogo = true;

                var rating = products.Select("PID = '" + (string)productMerchants.Rows[i]["PID"] + "'")[0]["rating"];
                merchant.Rating = rating != DBNull.Value ? Convert.ToDouble(rating) : 0;

                merchants.Add(merchant);
            }

            return merchants;
        }

        private List<CseProductInfo> ParsePriceGrabberProducts(int cse, string source, DataTable products, DataTable productMerchants)
        {
            if (products == null || products.Rows.Count == 0)
                return null;

            var productsList = new List<CseProductInfo>();

            for (int i = 0; i < products.Rows.Count; i++)
            {
                var product = new CseProductInfo();

                product.ShoppingEngine = cse;
                product.Source = source;
                product.ProductId = products.Rows[i]["pid"] as string;
                product.CategoryId = products.Rows[i]["category_id"] as string;
                product.Name = ((string)products.Rows[i]["name"]).ToLower();
                product.DisplayName = products.Rows[i]["name"] as string;
                product.Description = products.Rows[i]["long_Desc"] as string;
                product.Image = products.Rows[i]["imageURL_med"] as string;

                product.Offers = new List<CseOfferInfo>();

                if (productMerchants == null || productMerchants.Rows.Count == 0)
                    continue;

                var rows = productMerchants.Select("mPID = '" + product.ProductId + "'");

                if (rows.Length == 0)
                    continue;

                for (int x = 0; x < rows.Length; x++)
                {
                    var offer = new CseOfferInfo();

                    offer.Url = rows[x]["URL"] as string;
                    offer.Price = double.Parse((string)rows[x]["price"]);
                    offer.MerchantId = rows[x]["mid"] != DBNull.Value ? (string)rows[x]["mid"] : null;

                    product.Offers.Add(offer);
                }

                product.Offers = product.Offers.OrderBy(x => x.Price).ToList();
                product.MinPrice = product.Offers[0].Price;
                product.MaxPrice = product.Offers[product.Offers.Count - 1].Price;

                productsList.Add(product);
            }

            return productsList;
        }
        private List<CseMerchantInfo> ParsePriceGrabberMerchants(int cse, string source, DataTable merchantsTable)
        {
            if (merchantsTable == null || merchantsTable.Rows.Count == 0)
                return null;

            var merchants = new List<CseMerchantInfo>();

            for (int i = 0; i < merchantsTable.Rows.Count; i++)
            {
                var merchant = new CseMerchantInfo();

                merchant.ShoppingEngine = cse;
                merchant.Source = source;
                merchant.MerchantId = merchantsTable.Rows[i]["merchant_id"] as string;
                merchant.Name = ((string)merchantsTable.Rows[i]["merchant_name"]).ToLower();
                merchant.DisplayName = merchantsTable.Rows[i]["merchant_name"] as string;
                merchant.UseLogo = true;

                merchants.Add(merchant);
            }

            return merchants;
        }

        private List<CseProductInfo> ParseShopZillaProducts(string source, DataTable products, DataTable productMerchants)
        {
            if (products == null || products.Rows.Count == 0)
                return null;

            var productsList = new List<CseProductInfo>();

            for (int i = 0; i < products.Rows.Count; i++)
            {
                var product = new CseProductInfo();

                product.ShoppingEngine = 2;
                product.Source = source;
                product.ProductId = products.Rows[i]["pid"] as string;
                product.CategoryId = products.Rows[i]["category_id"] as string;
                product.Name = ((string)products.Rows[i]["name"]).ToLower();
                product.DisplayName = products.Rows[i]["name"] as string;
                product.Image = products.Rows[i]["imageURL_small"] as string;
                product.Description = products.Rows[i]["long_Desc"] != DBNull.Value ? (string)products.Rows[i]["long_Desc"] : null;

                product.Offers = new List<CseOfferInfo>();

                if (productMerchants == null || productMerchants.Rows.Count == 0)
                    continue;

                var rows = productMerchants.Select("mPID = '" + product.ProductId + "'");

                if (rows.Length == 0)
                    continue;

                for (int x = 0; x < rows.Length; x++)
                {
                    var offer = new CseOfferInfo();

                    offer.Url = rows[x]["URL"] as string;
                    offer.Price = (double)rows[x]["price"];
                    offer.MerchantId = rows[x]["mid"] != DBNull.Value ? (string)rows[x]["mid"] : null;

                    product.Offers.Add(offer);
                }

                product.Offers = product.Offers.OrderBy(x => x.Price).ToList();
                product.MinPrice = product.Offers[0].Price;
                product.MaxPrice = product.Offers[product.Offers.Count - 1].Price;

                productsList.Add(product);
            }

            return productsList;
        }
        private List<CseMerchantInfo> ParseShopZillaMerchants(string source, DataTable merchantsTable)
        {
            if (merchantsTable == null || merchantsTable.Rows.Count == 0)
                return null;

            var merchants = new List<CseMerchantInfo>();

            for (int i = 0; i < merchantsTable.Rows.Count; i++)
            {
                var merchant = new CseMerchantInfo();

                merchant.ShoppingEngine = 2;
                merchant.Source = source;
                merchant.MerchantId = merchantsTable.Rows[i]["merchant_id"] as string;
                merchant.Name = ((string)merchantsTable.Rows[i]["merchant_name"]).ToLower();
                merchant.DisplayName = merchantsTable.Rows[i]["merchant_name"] as string;
                merchant.Logo = merchantsTable.Rows[i]["merchant_logo"] as string;
                merchant.UseLogo = true;

                merchants.Add(merchant);
            }

            return merchants;
        }

        private List<CseProductInfo> ParseAffiliateWindowProducts(string source, DataTable products, DataTable productMerchants)
        {
            if (products == null || products.Rows.Count == 0)
                return null;

            var productsList = new List<CseProductInfo>();

            for (int i = 0; i < products.Rows.Count; i++)
            {
                var product = new CseProductInfo();

                product.ShoppingEngine = 6;
                product.Source = source;
                product.ProductId = products.Rows[i]["pid"] as string;
                product.Name = ((string)products.Rows[i]["name"]).ToLower();
                product.DisplayName = products.Rows[i]["name"] as string;
                product.Image = (string)productMerchants.Select("iId = '" + product.ProductId + "'")[0]["sAwThumbUrl"];
                product.Description = products.Rows[i]["short_Desc"] != DBNull.Value ? (string)products.Rows[i]["short_Desc"] : null;

                product.Offers = new List<CseOfferInfo>();

                if (productMerchants == null || productMerchants.Rows.Count == 0)
                    continue;

                var rows = productMerchants.Select("iId = '" + product.ProductId + "'");

                if (rows.Length == 0)
                    continue;

                for (int x = 0; x < rows.Length; x++)
                {
                    var offer = new CseOfferInfo();

                    offer.Url = rows[x]["sAwDeepLink"] as string;
                    offer.Price = (double)(float)rows[x]["fPrice"];
                    offer.MerchantId = rows[x]["iMerchantId"] != DBNull.Value ? ((int)rows[x]["iMerchantId"]).ToString() : null;

                    product.Offers.Add(offer);
                }

                product.Offers = product.Offers.OrderBy(x => x.Price).ToList();
                product.MinPrice = product.Offers[0].Price;
                product.MaxPrice = product.Offers[product.Offers.Count - 1].Price;

                productsList.Add(product);
            }

            return productsList;
        }
        private List<CseMerchantInfo> ParseAffiliateWindowMerchants(string source, DataTable merchantsTable)
        {
            if (merchantsTable == null || merchantsTable.Rows.Count == 0)
                return null;

            var merchants = new List<CseMerchantInfo>();

            for (int i = 0; i < merchantsTable.Rows.Count; i++)
            {
                var merchant = new CseMerchantInfo();

                merchant.ShoppingEngine = 6;
                merchant.Source = source;
                merchant.MerchantId = merchantsTable.Rows[i]["merchant_id"] as string;
                merchant.Name = ((string)merchantsTable.Rows[i]["merchant_name"]).ToLower();
                merchant.DisplayName = merchantsTable.Rows[i]["merchant_name"] as string;
                merchant.Logo = merchantsTable.Rows[i]["merchant_logo"] as string;
                merchant.UseLogo = true;

                merchants.Add(merchant);
            }

            return merchants;
        }

        private void UpsertProducts(List<CseProductInfo> products, bool upadteIfExist)
        {
            if (products == null || products.Count == 0)
                return;

            for (int i = 0; i < products.Count; i++)
            {
                products[i].Offers = products[i].Offers.OrderBy(x => x.Price).ToList();
                products[i].MinPrice = products[i].Offers[0].Price;
                products[i].MaxPrice = products[i].Offers[products[i].Offers.Count - 1].Price;

                var product = GetProduct(products[i].ShoppingEngine, products[i].ProductId, "_id");

                if (product == null)
                    InsertProduct(products[i]);
                else if (upadteIfExist == true)
                    UpdateProduct(products[i]);
            }
        }
        private void UpsertMerchants(List<CseMerchantInfo> merchants, bool upadteIfExist)
        {
            if (merchants == null || merchants.Count == 0)
                return;

            for (int i = 0; i < merchants.Count; i++)
            {
                var merchant = GetMerchant(merchants[i].ShoppingEngine, merchants[i].MerchantId, "_id");

                if (merchant == null)
                    InsertMerchant(merchants[i]);
                else if (upadteIfExist == true)
                    UpdateMerchant(merchants[i]);
            }
        }
        private string GetMerchantGlobalId(string mid, int cse)
        {
            string cacheKey = string.Format("GetMerchantGlobalId:{0}:{1}", mid, cse);

            if (HttpContext.Current.Cache[cacheKey] != null)
                return (string)HttpContext.Current.Cache[cacheKey];

            var dt = GetDataTable("Get_MidApi_Domain",
                new Dictionary<string, object>
                {
                  {"@Mid", mid},
                  {"@API", cse}
                },
               "SQLCacheConnectionString");

            if (dt == null || dt.Rows.Count == 0)
                return "undefined";

            string merchandGlobalId = dt.Rows[0][0] as string;
            merchandGlobalId = merchandGlobalId.ToLower();

            HttpContext.Current.Cache.Insert
                (cacheKey, merchandGlobalId, null, DateTime.Now.AddHours(24), Cache.NoSlidingExpiration);

            return merchandGlobalId;
        }

        //DAL
        public void UpdateMerchant(CseMerchantInfo merchant)
        {
            var dataBase = _server.ServerConnection.GetDatabase(string.Format("cse_caching_{0}", merchant.ShoppingEngine));
            var collection = dataBase.GetCollection<CseMerchantInfo>(MERCHANTS_COLLECTION);

            var updateBuilder = new UpdateBuilder();

            if (merchant.Name != null)
                updateBuilder.Set("name", merchant.Name);
            if (merchant.DisplayName != null)
                updateBuilder.Set("display_name", merchant.DisplayName);
            if (merchant.Logo != null)
                updateBuilder.Set("logo", merchant.Logo);
            if (merchant.Source != null)
                updateBuilder.Set("source", merchant.Source);

            updateBuilder.Set("use_logo", merchant.UseLogo);
            updateBuilder.Set("rating", merchant.Rating);
            updateBuilder.Set("shopping_engine", merchant.ShoppingEngine);
            updateBuilder.Set("date_modified", DateTime.Now);

            collection.Update(Query.EQ("merchant_id", merchant.MerchantId), updateBuilder);
        }
        public CseMerchantInfo InsertMerchant(CseMerchantInfo merchant)
        {
            var dataBase = _server.ServerConnection.GetDatabase(string.Format("cse_caching_{0}", merchant.ShoppingEngine));
            var collection = dataBase.GetCollection<CseMerchantInfo>(MERCHANTS_COLLECTION);

            merchant.DateCreated = DateTime.Now;
            merchant.DateModified = DateTime.Now;

            collection.Insert(merchant);

            return merchant;
        }
        public CseMerchantInfo GetMerchant(int cse, string merchantId, params string[] fields)
        {
            var dataBase = _server.ServerConnection.GetDatabase(string.Format("cse_caching_{0}", cse));
            var collection = dataBase.GetCollection<CseMerchantInfo>(MERCHANTS_COLLECTION);

            var result = collection
                      .Find(Query.EQ("merchant_id", merchantId))
                      .SetFields(Fields.Include(fields))
                      .SetLimit(1)
                      .SingleOrDefault();

            return result;
        }

        public void UpdateProduct(CseProductInfo product)
        {
            var dataBase = _server.ServerConnection.GetDatabase(string.Format("cse_caching_{0}", product.ShoppingEngine));
            var collection = dataBase.GetCollection<CseProductInfo>(PRODUCTS_COLLECTION);

            var updateBuilder = new UpdateBuilder();

            if (product.Name != null)
                updateBuilder.Set("name", product.Name);
            if (product.DisplayName != null)
                updateBuilder.Set("display_name", product.DisplayName);
            if (product.Description != null)
                updateBuilder.Set("description", product.Description);
            if (product.CategoryId != null)
                updateBuilder.Set("category_id", product.CategoryId);
            if (product.CategoryId != null)
                updateBuilder.Set("category", product.CategoryId);
            if (product.Source != null)
                updateBuilder.Set("source", product.Source);
            if (product.Image != null)
                updateBuilder.Set("image", product.Image);
            if (product.Offers != null)
                updateBuilder.SetWrapped("offers", product.Offers);

            updateBuilder.Set("min_price", product.MinPrice);
            updateBuilder.Set("max_price", product.MaxPrice);
            updateBuilder.Set("shopping_engine", product.ShoppingEngine);
            updateBuilder.Set("date_modified", DateTime.Now);

            collection.Update(Query.EQ("product_id", product.ProductId), updateBuilder);
        }
        public CseProductInfo InsertProduct(CseProductInfo product)
        {
            var dataBase = _server.ServerConnection.GetDatabase(string.Format("cse_caching_{0}", product.ShoppingEngine));
            var collection = dataBase.GetCollection<CseProductInfo>(PRODUCTS_COLLECTION);

            product.DateCreated = DateTime.Now;
            product.DateModified = DateTime.Now;

            collection.Insert(product, new MongoInsertOptions()
            {
                SafeMode = SafeMode.False
            });

            return product;
        }
        public CseProductInfo GetProduct(int cse, string productId, params string[] fields)
        {
            var dataBase = _server.ServerConnection.GetDatabase(string.Format("cse_caching_{0}", cse));
            var collection = dataBase.GetCollection<CseProductInfo>(PRODUCTS_COLLECTION);

            var result = collection
                      .Find(Query.EQ("product_id", productId))
                      .SetFields(Fields.Include(fields))
                      .SetLimit(1)
                      .SingleOrDefault();

            return result;
        }

        public string GetUrlId(string url)
        {
            var dataBase = _server.ServerConnection.GetDatabase("catalogs");
            var collection = dataBase.GetCollection<BsonDocument>("url_mapping");

            var document = new BsonDocument(new BsonElement("url", url), new BsonElement("date_created", DateTime.Now));

            collection.Insert(document);

            return ((ObjectId)document["_id"]).ToString();
        }

        public static DataTable GetDataTable(string procedureName, Dictionary<string, object> parameters, string connectionString)
        {
            var dataTable = GetDsBySp(procedureName, connectionString, parameters).Tables[0];

            return dataTable;
        }
        public static DataTable GetDataTable(string procedureName, Dictionary<string, object> parameters, string connectionString, int cache)
        {
            var dataTable = GetDsBySp(procedureName, connectionString, parameters).Tables[0];

            return dataTable;
        }
        public static DataTable GetDataTable(string tableName, string parameters, string connectionString, int cache)
        {
            DataTable dataTable = new DataTable();
            string cacheKey = string.Format("{0}:{1}", tableName, parameters);

            try
            {
                if (HttpContext.Current.Cache[cacheKey] == null)
                {
                    dataTable = GetCacheTableFromDb(tableName, connectionString);

                    HttpContext.Current.Cache.Insert(
                        cacheKey,
                        dataTable,
                        null,
                        DateTime.Now.AddHours(cache),
                        Cache.NoSlidingExpiration);
                }
                else
                {
                    dataTable = (DataTable)HttpContext.Current.Cache[cacheKey];
                }
            }
            catch
            {
            }

            return dataTable;
        }

        private static DataTable GetCacheTableFromDb(string TableName, string ConnectionString)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            parameters.Add("@TABNAME", TableName);

            var ds = GetDsBySp("SP_CLEARCACHE", ConnectionString, parameters);

            if (ds.Tables.Count == 0)
                throw new Exception("Cannot get " + TableName + " data from sql");
            var dt = ds.Tables[0];
            if (dt.Rows.Count == 0)
                throw new Exception("table named " + TableName + " has no data");
            return dt;
        }

        private static DataSet GetDsBySp(string SP, string strConnectionString, Dictionary<string, object> parameters)
        {
            DataSet ds = new DataSet();
            SqlConnection connShop = new SqlConnection();
            string StrConnShop = ConfigurationManager.AppSettings[strConnectionString];
            SqlCommand cmd = new SqlCommand();
            try
            {
                if (connShop.State != System.Data.ConnectionState.Open)
                {
                    connShop.ConnectionString = StrConnShop;
                    connShop.Open();
                }
            }
            catch
            {
            }
            cmd.CommandTimeout = 2400;
            cmd.Connection = connShop;
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = SP;

            foreach (var pair in parameters)
            {
                cmd.Parameters.AddWithValue(pair.Key, pair.Value);
            }

            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;

            try
            {
                da.Fill(ds);
            }
            catch (Exception ex)
            {
                string strEx = ex.Message;
            }
            finally
            {
                da.Dispose();
                cmd.Dispose();
                connShop.Close();
            }
            return ds;

        }
        private static DataTable Get_AllData(string table, string parameters, string strConnectionString)
        {
            SqlConnection connShop;
            string StrConnShop;
            StrConnShop = ConfigurationManager.AppSettings[strConnectionString];
            connShop = new SqlConnection();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            try
            {
                if (connShop.State != System.Data.ConnectionState.Open)
                {
                    connShop.ConnectionString = StrConnShop;
                    connShop.Open();
                }
            }
            catch
            {
            }
            cmd.Connection = connShop;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = "SELECT * FROM " + table + " with(nolock) " + parameters;
            //  SqlDataAdapter da = new SqlDataAdapter();
            // da.SelectCommand = cmd;
            try
            {
                //      da.Fill(dt);
                SqlDataReader sqlDataReader = cmd.ExecuteReader();
                dt.Load(sqlDataReader);
                sqlDataReader.Close();
            }
            catch
            {
            }
            finally
            {
                //   da.Dispose();
                cmd.Dispose();
                connShop.Close();
            }

            return dt;
        }
    }
}
