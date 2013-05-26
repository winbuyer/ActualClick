using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Configuration;

namespace WinBuyer.B2B.CseToMongoEtl
{
    public class MongoDBServerConnectionWrapper
    {
        private readonly MongoServer _mongoServer = null;
        private static MongoDBServerConnectionWrapper _instance = null;
        private static object _locker = new object();

        private MongoDBServerConnectionWrapper()
        {
            string connectionString = ConfigurationManager.AppSettings["MongoDBConnectionStringCse"];

            _mongoServer = new MongoClient(connectionString).GetServer();
        }

        public static MongoDBServerConnectionWrapper GetInstance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    if (_instance == null)
                    {
                        _instance = new MongoDBServerConnectionWrapper();
                    }
                }
            }

            return _instance;
        }

        public MongoServer ServerConnection
        {
            get
            {
                return _mongoServer;
            }
        }
    }
}
