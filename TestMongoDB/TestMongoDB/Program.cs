using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TestMongoDB
{
    class Program
    {

        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
        

        static void Main(string[] args)
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("test");

        }
    }
}
