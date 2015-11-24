using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;
using System.IO;
using System.Diagnostics;

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

            CompareCountMethods();

            //BackupDbToFile();

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        private static void BackupDbToFile()
        {
            var sw = new Stopwatch();
            sw.Start();
            var s = WriteCollectionToFile(_database, "restaurants", "c:\\test\\backup.json");
            s.Wait();
            sw.Stop();
            Console.WriteLine("Output to file took {0} seconds", sw.ElapsedMilliseconds / 1000);
        }

        static void CompareCountMethods()
        {
            var filter = Builders<BsonDocument>.Filter.Eq("address.zipcode", "10075");

            var sw = new Stopwatch();
            sw.Start();
            var t = CountMongoMethod(filter);
            t.Wait();
            sw.Stop();
            Console.WriteLine("Time for Mongo Method: {0}", sw.ElapsedMilliseconds);

            var ss = new Stopwatch();
            ss.Start();
            var x = CountCustomMethod(filter);
            x.Wait();
            ss.Stop();
            Console.WriteLine("Time for Custom Method: {0}", ss.ElapsedMilliseconds);
        }

        static async Task CountMongoMethod(FilterDefinition<BsonDocument> filter)
        {
            var collection = _database.GetCollection<BsonDocument>("restaurants");
            
            var count = collection.CountAsync(filter);

            Console.WriteLine("The number of documents is: {0}", count.Result);
        }

        static async Task CountCustomMethod(FilterDefinition<BsonDocument> filter)
        {
            var collection = _database.GetCollection<BsonDocument>("restaurants");
            int count = 0;

            using (var cursor = await collection.FindAsync(filter))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        // process document
                        count++;
                    }
                }
            }
            Console.WriteLine("The number of documents is: {0}", count);
        }

        public static async Task WriteCollectionToFile(IMongoDatabase database, string collectionName, string fileName)
        {
            var collection = database.GetCollection<RawBsonDocument>(collectionName);

            // Make sure the file is empty before we start writing to it
            File.WriteAllText(fileName, string.Empty);

            using (var cursor = await collection.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    var batch = cursor.Current;
                    foreach (var document in batch)
                    {
                        File.AppendAllLines(fileName, new[] { document.ToString() });
                    }
                }
            }
        }

    }
}
