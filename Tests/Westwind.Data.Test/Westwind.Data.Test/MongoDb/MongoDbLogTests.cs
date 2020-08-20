using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Driver.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities.Logging;

namespace Westwind.BusinessFramework.Test.MongoDb
{
    [TestClass]
    public class MongoDbLogTests
    {
        [TestMethod]
        public void CreateTableTest()
        {
            var log = CreateLogManager();
            log.CreateLog();
        }

        [TestMethod]
        public void WriteEntry()
        {
            var entry = new WebLogEntry()
            {
                Message = "Test at " + DateTime.Now,
                ErrorLevel = ErrorLevels.Info
            };

            var log = CreateLogManager();

            Assert.IsTrue(log.WriteEntry(entry));
            Assert.IsTrue(LogManager.Current.WriteEntry(entry));
        }

        [TestMethod]
        public void LoadEntryById()
        {
            var log = CreateLogManager();
            var db = log.LogAdapter as MongoDbLogAdapter;

            var entry = db.MongoBusiness.Collection.AsQueryable().FirstOrDefault();
            Assert.IsNotNull(entry, "No entries in table.");

            int id = entry.Id;

            entry = log.GetWebLogEntry(id);

            Assert.AreEqual(entry.Id, id);
            Console.WriteLine(entry.Message);
        }

        [TestMethod]
        public void GetEntriesTest()
        {
            var log = CreateLogManager();
            var db = log.LogAdapter as MongoDbLogAdapter;

            var entries = log.GetEntries();

            Assert.IsNotNull(entries);
            Assert.IsTrue(entries.Any());
            foreach (var entry in entries.OrderByDescending(wl => wl.Entered).Take(20))
                Console.WriteLine(entry.Message + " - " + entry.Id);
        }

        /// <summary>
        /// Creates a log manager instance as well as LogManager.Current
        /// instance.
        /// </summary>
        /// <returns></returns>
        private LogManager CreateLogManager()
        {
            return LogManager.Create(new MongoDbLogAdapter("MongoTestContext"));
        }

        


    }
}

