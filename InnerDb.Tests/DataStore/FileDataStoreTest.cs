using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using InnerDb.Core.DataStore;
using InnerDb.Tests.TestHelpers;
using InnerDb.Core.Index;
using log4net;

namespace InnerDb.Tests.DataStore
{
	[TestFixture]
	class FileDataStoreTest
	{
		private Car fit = new Car("Honda", "Fit", "Silver");
		private Car prius = new Car("Toyota", "Prius", "Red");
		private Creature rat = new Creature("Rat", Alignment.Evil);

        static FileDataStoreTest()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        [Test]
		public void GetGetsPutObjects()
		{
            var indexStore = new IndexStore("GetAndPut");
			var fs = new FileDataStore("GetAndPut", indexStore);
			try
			{
				fs.PutObject(fit, 1);
                indexStore.IndexObject(fit, 1);

				fs.PutObject(prius, 2);
                indexStore.IndexObject(prius, 2);

				var actual = fs.GetObject<Car>(1);
				Assert.AreEqual(fit, actual);
			}
			finally
			{
				fs.DeleteDatabase();
			}
		}

        [Test]
        public void DeleteDeletesObjects()
		{
            var indexStore = new IndexStore("GetAndPut");
			var fs = new FileDataStore("Deletez", indexStore);

			try
			{
				fs.PutObject(fit, 1);
                indexStore.IndexObject(fit, 1);

				fs.Delete(1);
                indexStore.RemoveObject(1);

                var actual = fs.GetObject<Car>(1);
				Assert.IsNull(actual);
			}
			finally
			{
				fs.DeleteDatabase();
			}
		}

        [Test]
        public void GetCollectionWithIdReturnsObjectsByType()
		{
			var index = new IndexStore("GetByType");
			var fs = new FileDataStore("GetByType", index);
			
			try
			{
				fs.PutObject(fit, 1);
				index.IndexObject(fit, 1);

				fs.PutObject(rat, 2);
				index.IndexObject(rat, 2);

				fs.PutObject(prius, 3);
				index.IndexObject(prius, 3);

				var actual = fs.GetCollectionWithId<Car>();
				Assert.AreEqual(2, actual.Count);
				var first = actual.ElementAt(0);
				Assert.AreEqual(1, first.Key);
				Assert.AreEqual(fit, first.Value);
				var second = actual.ElementAt(1);
				Assert.AreEqual(3, second.Key);
				Assert.AreEqual(prius, second.Value);
			}
			finally
			{
				fs.DeleteDatabase();
			}
		}
	}
}
