using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using InnerDb.Core.DataStore;
using InnerDb.Tests.TestHelpers;
using InnerDb.Core.Index;

namespace InnerDb.Tests.DataStore
{
	[TestFixture]
	class FileDataStoreTest
	{
		private Car fit = new Car("Honda", "Fit", "Silver");
		private Car prius = new Car("Toyota", "Prius", "Red");
		private Creature rat = new Creature("Rat", Alignment.Evil);

		public void GetGetsPutObjects()
		{
			var fs = new FileDataStore("GetAndPut", null);
			try
			{
				fs.PutObject(fit, 1);
				fs.PutObject(prius, 2);
				var actual = fs.GetObject<Car>(1);
				Assert.AreEqual(fit, actual);
			}
			finally
			{
				fs.DeleteDatabase();
			}
		}

		public void DeleteDeletesObjects()
		{
			var fs = new FileDataStore("Deletez", null);
			try
			{
				fs.PutObject(fit, 1);
				fs.Delete(1);
				var actual = fs.GetObject<Car>(1);
				Assert.IsNull(actual);
			}
			finally
			{
				fs.DeleteDatabase();
			}
		}

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
