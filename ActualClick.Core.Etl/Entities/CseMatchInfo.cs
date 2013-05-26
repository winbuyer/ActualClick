using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace WinBuyer.B2B.CseToMongoEtl.Entities
{
    public class CseMatchInfo
    {
        [BsonElement("status")]
        public int Status
        {
            get;
            set;
        }
        [BsonElement("modified_by")]
        public string ModifiedBy
        {
            get;
            set;
        }
        [BsonElement("cse")]
        public int Cse
        {
            get;
            set;
        }
        [BsonElement("match_type")]
        public int MatchType
        {
            get;
            set;
        }
        [BsonElement("pids")]
        public List<string> Pids
        {
            get;
            set;
        }
        [BsonElement("modified_at")]
        public DateTime DateModified
        {
            get;
            set;
        }
    }
}
