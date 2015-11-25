using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMongoDB
{
    internal class CollectionManager
    {
        private IMongoCollection<BsonDocument> _collection;

        internal CollectionManager(IMongoCollection<BsonDocument> mongoCollection)
        {
            _collection = mongoCollection;
        }

        internal GetFirstRecord()
        {
            
            return _collection.FindAsync()
        }
    }
}
