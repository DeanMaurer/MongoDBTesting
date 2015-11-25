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
        private IMongoCollection<BsonDocument> _collection;
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
            _collection = _database.GetCollection<BsonDocument>(_collectionName);
            var options = new CreateIndexOptions();
            options.Sparse = true;
            options.Unique = true;

            _collection.Indexes.CreateOneAsync(GetIndexDefinition(), options);

            await LogIndexes();

            await CreateDocuments();
        }

        private async Task LogIndexes()
        {
            using (var cursor = await _collection.Indexes.ListAsync())
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
            for (int record = 1; record <= _recordsToCreate; record++)
            {
                var document = CreateDocument(10014, record, DateTime.UtcNow);
                await _collection.InsertOneAsync(document);
                LogRecordCreationPercentComplete(record);
            }

            RowsInserted = _collection.CountAsync(new BsonDocument()).Result;
        }

        private BsonDocument CreateDocument(int stationId, int sessionId, DateTime time)
        {
            var document = new BsonDocument
            {
                { "Ingress", new BsonDocument
                    {
                        { "Time", time },
                        { "StationId", stationId },
                        { "SessionId", sessionId }
                    }
                }
            };
            return document;
        }

        private static void LogRecordCreationPercentComplete(int lastRecordCreated)
        {
            if (LastRecordCreatedIsMultipleOfTen(lastRecordCreated))
            {
                double percentComplete = ((double)lastRecordCreated / _recordsToCreate) * 100;
                Console.WriteLine("ScansDB is {0}% complete.", percentComplete);
            }
        }

        private static bool LastRecordCreatedIsMultipleOfTen(int lastRecordCreated)
        {
            return lastRecordCreated % (_recordsToCreate / 10) == 0;
        }

        internal void InsertDuplicateRecord()
        {
            var t = InsertDuplicateRecordAsync();
            t.Wait();
        }

        private async Task InsertDuplicateRecordAsync()
        {
            var document = GetFirstRecordAsync();
            await _collection.InsertOneAsync(document.Result);
        }

        internal string GetFirstRecord()
        {
            var recordThread = GetFirstRecordAsync();
            recordThread.Wait();
            return recordThread.Result.ToString();
        }

        private async Task<BsonDocument> GetFirstRecordAsync()
        {
            return _collection.Find(new BsonDocument()).FirstAsync().Result;
        }

        internal void WriteCollectionToFile()
        {
            CollectionToFileWriter.WriteCollectionToFile(_collectionName, _databaseName);
        }
    }
}
