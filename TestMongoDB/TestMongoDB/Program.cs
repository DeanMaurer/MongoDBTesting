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
            Console.WriteLine();
            //Console.WriteLine("The first record is: {0}", db.GetFirstRecord());

            //LogDuplicateRecordInsertionFailure(db);

            Console.WriteLine("Updating half the documents.");
            db.UpdateSomeDocuments();
            Console.WriteLine("{0} documents were updated.", db.GetNumberOfUpdatedDocuments());

            WaitForUserToCloseConsole();
        }        

        private static void LogDuplicateRecordInsertionFailure(DatabaseFactory db)
        {
            try
            {
                db.InsertDuplicateRecord();
            }
            catch
            {
                Console.WriteLine();
                Console.WriteLine("Failed to insert a duplicate record!");
            }
        }

        private static void WaitForUserToCloseConsole()
        {
            Console.WriteLine();
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        

    }
}
