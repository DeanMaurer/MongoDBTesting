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

        static void Main(string[] args)
        {
            _client = new MongoClient();

            var t = BuildDatabase();
            t.Wait();

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
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

        public static async Task BuildDatabase()
        {
            var t = _client.DropDatabaseAsync("ScanDB");
            t.Wait();
            var db = _client.GetDatabase("ScanDB");
            var collection = db.GetCollection<BsonDocument>("Scans");

            var sw = new Stopwatch();
            sw.Start();
            for (int i = 1; i <= 10000; i++)
            {
                var document = CreateDocument(10014, i);
                await collection.InsertOneAsync(document);
                if (i % 1000 == 0)
                {
                    double percentComplete = ((double)i / 10000) * 100;
                    Console.WriteLine("ScansDB is {0}% complete.", percentComplete);
                }
            }
            sw.Stop();

            Console.WriteLine("Inserted {0} rows", collection.CountAsync(new BsonDocument()).Result);
            Console.WriteLine("The first record is: {0}", collection.Find(new BsonDocument()).FirstAsync().Result);
            Console.WriteLine("Inserting {0} records took {1} seconds", collection.CountAsync(new BsonDocument()).Result, sw.ElapsedMilliseconds / 1000);
        }

        private static BsonDocument CreateDocument(int stationId, int sessionId)
        {
            var document = new BsonDocument
            {
                { "ingress", new BsonDocument
                    {
                        { "Time", DateTime.UtcNow },
                        { "StationId", stationId },
                        { "SessionId", sessionId }

                    }
                }
            };
            return document;
        }

    }
}
