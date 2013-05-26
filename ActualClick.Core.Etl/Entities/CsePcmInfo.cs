using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WinBuyer.B2B.CseToMongoEtl.Entities
{
    [BsonIgnoreExtraElements]
    public class CsePcmInfo
    {
        [BsonId]
        public ObjectId ObjectId
        {
            get;
            set;
        }
        [BsonElement("sku")]
        public string Sku
        {
            get;
            set;
        }
        [BsonElement("status")]
        public int Status
        {
            get;
            set;
        }
        [BsonElement("domain")]
        public string Domain
        {
            get;
            set;
        }
        [BsonElement("isbn")]
        public string Isbn
        {
            get;
            set;
        }
        [BsonElement("upc")]
        public string Upc
        {
            get;
            set;
        }
        [BsonElement("mid")]
        public string Mid
        {
            get;
            set;
        }
        [BsonElement("mpn")]
        public string Mpn
        {
            get;
            set;
        }
        [BsonElement("imageurl")]
        public string ImageUrl
        {
            get;
            set;
        }
        [BsonElement("mpn_origin")]
        public string MpnOrigin
        {
            get;
            set;
        }
        [BsonElement("product_name")]
        public string ProductName
        {
            get;
            set;
        }
        [BsonElement("price")]
        public double Price
        {
            get;
            set;
        }
        [BsonElement("is_default_price")]
        public bool IsDefaultPrice
        {
            get;
            set;
        }
        [BsonElement("product_display_name")]
        public string ProductDisplayName
        {
            get;
            set;
        }
        [BsonElement("shipping")]
        public string ShippingPrice
        {
            get;
            set;
        }
        [BsonElement("brand_origin")]
        public string BrandOrigin
        {
            get;
            set;
        }
        [BsonElement("brand")]
        public string Brand
        {
            get;
            set;
        }
        [BsonElement("currency")]
        public string Currency
        {
            get;
            set;
        }
        [BsonElement("product_url")]
        public string ProductUrl
        {
            get;
            set;
        }
        [BsonElement("page_type")]
        public string PageType
        {
            get;
            set;
        }
        [BsonElement("domain_price_rule")]
        public string DomainPriceRule
        {
            get;
            set;
        }
        [BsonElement("matches")]
        public List<CseMatchInfo> Matches
        {
            get;
            set;
        }
        [BsonElement("date_created")]
        public DateTime DateCreated
        {
            get;
            set;
        }
        [BsonElement("date_modified")]
        public DateTime DateModified
        {
            get;
            set;
        }

        public CsePcmInfo()
        {
            ProductName = "";
            ProductDisplayName = "";
            Domain = "";
            Matches = new List<CseMatchInfo>();
        }
    }
}
