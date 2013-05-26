using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace WinBuyer.B2B.CseToMongoEtl.Entities
{
    public class CseProductInfo : ICloneable
    {
        [BsonId]
        public ObjectId ObjectId
        {
            get;
            set;
        }
        [BsonElement("product_id")]
        public string ProductId
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
        [BsonElement("description")]
        public string Description
        {
            get;
            set;
        }
        [BsonElement("category_id")]
        public string CategoryId
        {
            get;
            set;
        }
        [BsonElement("category")]
        public string Category
        {
            get;
            set;
        }
        [BsonElement("min_price")]
        public double MinPrice
        {
            get;
            set;
        }
        [BsonElement("max_price")]
        public double MaxPrice
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
        [BsonElement("image")]
        public string Image
        {
            get;
            set;
        }
        [BsonElement("offers")]
        public List<CseOfferInfo> Offers
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

        [BsonIgnore]
        public string Id
        {
            get
            {
                return ObjectId == null ? null : ObjectId.ToString();
            }
        }
        [BsonIgnore]
        public string CampaignId
        {
            get;
            set;
        }

        public object Clone()
        {
            return new CseProductInfo()
            {
                Category = this.Category,
                CategoryId = this.CategoryId,
                DateCreated = this.DateCreated,
                DateModified = this.DateModified,
                Description = this.Description,
                DisplayName = this.DisplayName,
                Image = this.Image,
                MaxPrice = this.MaxPrice,
                MinPrice = this.MinPrice,
                Name = this.Name,
                ObjectId = this.ObjectId,
                Offers = new List<CseOfferInfo>(this.Offers.Select(x => (CseOfferInfo)x.Clone())),
                ProductId = this.ProductId,
                ShoppingEngine = this.ShoppingEngine,
                Source = this.Source
            };
        }
    }
}
