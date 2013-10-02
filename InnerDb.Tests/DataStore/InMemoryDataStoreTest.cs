using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using InnerDb.Core.DataStore;
using InnerDb.Tests.TestHelpers;

namespace InnerDb.Tests.DataStore
{
	[TestFixture]
	class InMemoryDataStoreTest
	{
		[Test]
		public void InMemoryStoreCanPut1MillionObjectsWithoutCrashing()
		{
			var store = new InMemoryDataStore();

			Assert.DoesNotThrow(() =>
			{
				for (int i = 0; i < 1000000; i++)
				{
					store.PutObject(new Sword("Exo-Sword " + i, 1), i);
				}
			});
		}
	}
}
