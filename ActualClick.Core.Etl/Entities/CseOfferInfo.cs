using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WinBuyer.B2B.CseToMongoEtl.Entities
{
    public class CseOfferInfo : ICloneable
    {
        [BsonElement("price")]
        public double Price
        {
            get;
            set;
        }
        [BsonElement("url")]
        public string Url
        {
            get;
            set;
        }
        [BsonElement("count")]
        public int Count
        {
            get;
            set;
        }
        [BsonElement("merchant_id")]
        public string MerchantId
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
        [BsonElement("brand")]
        public string Brand
        {
            get;
            set;
        }
        [BsonElement("model")]
        public string Model
        {
            get;
            set;
        }
        [BsonElement("cpc")]
        public double Cpc
        {
            get;
            set;
        }

        [BsonIgnore]
        public CseMerchantInfo MerchantInfo
        {
            get;
            set;
        }

        public object Clone()
        {
            return new CseOfferInfo()
            {
                Brand = this.Brand,
                Count = this.Count,
                Cpc = this.Cpc,
                MerchantId = this.MerchantId,
                MerchantInfo = (CseMerchantInfo)this.MerchantInfo.Clone(),
                Model = this.Model,
                Price = this.Price,
                Sku = this.Sku,
                Url = this.Url
            };
        }
    }
}
