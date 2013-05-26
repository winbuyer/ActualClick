using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WinBuyer.B2B.CseToMongoEtl.Entities
{
    public class CseMerchantInfo : ICloneable
    {
        [BsonId]
        public ObjectId ObjectId
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
        [BsonElement("name")]
        public string Name
        {
            get;
            set;
        }
        [BsonElement("display_name")]
        public string DisplayName
        {
            get;
            set;
        }
        [BsonElement("logo")]
        public string Logo
        {
            get;
            set;
        }
        [BsonElement("use_logo")]
        public bool UseLogo
        {
            get;
            set;
        }
        [BsonElement("rating")]
        public double Rating
        {
            get;
            set;
        }
        [BsonElement("source")]
        public string Source
        {
            get;
            set;
        }
        [BsonElement("global_id")]
        public string GlobalId
        {
            get;
            set;
        }
        [BsonElement("shopping_engine")]
        public int ShoppingEngine
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

        public object Clone()
        {
            return new CseMerchantInfo()
            {
                DateCreated = this.DateCreated,
                DateModified = this.DateModified,
                DisplayName = this.DisplayName,
                Logo = this.Logo,
                MerchantId = this.MerchantId,
                Name = this.Name,
                ObjectId = this.ObjectId,
                Rating = this.Rating,
                ShoppingEngine = this.ShoppingEngine,
                Source = this.Source,
                UseLogo = this.UseLogo,
                GlobalId = this.GlobalId
            };
        }
    }
}
