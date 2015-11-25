using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMongoDB
{
    internal static class CollectionToFileWriter
    {
        public static void WriteCollectionToFile(string collectionName, string databaseName)
        {
            var t = WriteCollectionToFileAsync(collectionName, databaseName);
            t.Wait();
        }

        private static async Task WriteCollectionToFileAsync(string collectionName, string databaseName)
        {
            string fileName = ".\\" + collectionName + ".json";

            // Make sure the file is empty before we start writing to it
            File.WriteAllText(fileName, string.Empty);

            IMongoClient client = new MongoClient();
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

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
