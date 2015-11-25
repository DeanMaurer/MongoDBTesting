using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMongoDB
{
    internal class DatabaseFactory
    {
        private IMongoClient _client;
        private IMongoDatabase _database;
        internal IMongoCollection<BsonDocument> Collection;
        private const int _recordsToCreate = 1000;
        private const string _collectionName = "Scans";
        private const string _databaseName = "ScansDB";

        internal long MillisecondsToCreateDatabase = 0;
        internal long RowsInserted = 0;

        internal static DatabaseFactory GetPopulatedDatabase()
        {
            var dbFactory = new DatabaseFactory();
            dbFactory.PopulateDatabase();

            return dbFactory;
        }

        private DatabaseFactory()
        {
            _client = new MongoClient();
            _database = CreateDatabase();
        }

        private IMongoDatabase CreateDatabase()
        {
            var t = _client.DropDatabaseAsync(_databaseName);
            t.Wait();
            return _client.GetDatabase(_databaseName);
        }

        private void PopulateDatabase()
        {
            TimedDatabaseCreation();
        }

        private void TimedDatabaseCreation()
        {
            var sw = new Stopwatch();
            sw.Start();
            var t = CreateDocumentCollection();
            t.Wait();
            sw.Stop();
            MillisecondsToCreateDatabase = sw.ElapsedMilliseconds;
        }

        private async Task CreateDocumentCollection()
        {
            Collection = _database.GetCollection<BsonDocument>(_collectionName);
            var options = new CreateIndexOptions();
            options.Sparse = true;
            options.Unique = true;

            Collection.Indexes.CreateOneAsync(GetIndexDefinition(), options);

            await LogIndexes();

            await CreateDocuments();
        }

        private async Task LogIndexes()
        {
            using (var cursor = await Collection.Indexes.ListAsync())
            {
                var indexes = await cursor.ToListAsync();

                int i = 0;
                foreach (var index in indexes)
                {
                    i++;
                    Console.WriteLine("Index Number {0} is: {1}", i, index);
                }
            }
        }

        private IndexKeysDefinition<BsonDocument> GetIndexDefinition()
        {
            return Builders<BsonDocument>.IndexKeys.Ascending("Ingress.Time").Ascending("Ingress.StationId").Ascending("Ingress.SessionId");
        }

        private async Task CreateDocuments()
        {
            for (int i = 1; i <= _recordsToCreate; i++)
            {
                var document = CreateDocument(10014, i);
                await Collection.InsertOneAsync(document);
                LogRecordCreationPercentComplete(i);
            }

            RowsInserted = Collection.CountAsync(new BsonDocument()).Result;
        }

        private BsonDocument CreateDocument(int stationId, int sessionId)
        {
            var document = new BsonDocument
            {
                { "Ingress", new BsonDocument
                    {
                        { "Time", DateTime.UtcNow },
                        { "StationId", stationId },
                        { "SessionId", sessionId }

                    }
                }
            };
            return document;
        }

        private static void LogRecordCreationPercentComplete(int i)
        {
            if (i % (_recordsToCreate / 10) == 0)
            {
                double percentComplete = ((double)i / _recordsToCreate) * 100;
                Console.WriteLine("ScansDB is {0}% complete.", percentComplete);
            }
        }
    }
}
