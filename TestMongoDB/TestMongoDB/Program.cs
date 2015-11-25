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

        static void Main(string[] args)
        {
            var db = DatabaseFactory.GetPopulatedDatabase();
            Console.WriteLine("Inserted {0} rows.", db.RowsInserted);
            Console.WriteLine("It took {0} seconds to create and populate the database.", db.MillisecondsToCreateDatabase / 1000);


            var cm = new CollectionManager(db.Collection);
            Console.WriteLine("The first record is: {0}", cm.GetFirstRecord());


            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        public static async Task WriteCollectionToFile(IMongoDatabase database, string collectionName, string fileName)
        {
            var collection = database.GetCollection<RawBsonDocument>(collectionName);

            // Make sure the file is empty before we start writing to it
            File.WriteAllText(fileName, string.Empty);

            var sw = new Stopwatch();
            sw.Start();

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
            sw.Stop();
            Console.WriteLine("Writing the collection to a file took {0} seconds", sw.ElapsedMilliseconds / 1000);
        }

        

    }
}
