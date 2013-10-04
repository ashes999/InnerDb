using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using InnerDb.Core.DataStore;
using InnerDb.Tests.TestHelpers;
using InnerDb.Core.Journal;
using InnerDb.Core.Index;
using System.IO;

namespace InnerDb.Tests.DataStore
{
	[TestFixture]
	class InMemoryDataStoreTest
	{
		[Test]
		public void InMemoryStoreCanPut1MillionObjectsWithoutCrashing()
		{
			var index = new IndexStore("1M");
			var fs = new FileDataStore("1M", index);
			var journal = new FileJournal("1M", fs);
			var store = new InMemoryDataStore(fs, journal.DirectoryPath);

			try
			{
				Assert.DoesNotThrow(() =>
				{
					for (int i = 0; i < 1000000; i++)
					{
						store.PutObject(new Sword("Exo-Sword " + i, 1), i);
					}
				});
			}
			finally
			{
				Directory.Delete("1M", true);
			}
		}

		[Test]
		public void GetCollectionGetsObjectsOfType()
		{
			var index = new IndexStore("GetAll");
			var fs = new FileDataStore("GetAll", index);
			var journal = new FileJournal("GetAll", fs);
			var store = new InMemoryDataStore(fs, journal.DirectoryPath);

			try
			{
				var sword = new Sword("Lego Sword", 13);
				var katana = new Sword("Lego Katana", 27);

				store.PutObject(sword, 1);
				store.PutObject(katana, 2);
				store.PutObject(new Car("Kia", "Aieeee", "Black"), 3);

				var actual = store.GetCollection<Sword>();
				Assert.AreEqual(2, actual.Count);
				Assert.AreEqual(sword, actual.ElementAt(0));
				Assert.AreEqual(katana, actual.ElementAt(1));
			}
			finally
			{
				Directory.Delete("GetAll", true);
			}
		}

		[Test]
		public void GetReturnsPutObjects()
		{
			var index = new IndexStore("GetAndPut");
			var fs = new FileDataStore("GetAndPut", index);
			var journal = new FileJournal("GetAndPut", fs);
			var store = new InMemoryDataStore(fs, journal.DirectoryPath);

			try
			{
				var sword = new Sword("Lego Sword", 13);
				var car = new Car("Kia", "Eeep", "Black");

				store.PutObject(sword, 1);
				store.PutObject(car, 3);

				Assert.AreEqual(sword, store.GetObject<Sword>(1));
				Assert.AreEqual(car, store.GetObject<Car>(3));
			}
			finally
			{
				Directory.Delete("GetAndPut", true);
			}
		}
	}
}
