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
        private const int _recordsToCreate = 10000;
        private const string _collectionName = "Scans";
        private const string _databaseName = "ScansDB";

        internal IMongoCollection<BsonDocument> Collection;
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
            Collection.Indexes.CreateOneAsync(GetIndexDefinition());
            await CreateDocuments();
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
