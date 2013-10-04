using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using InnerDb.Core.Index;
using InnerDb.Tests.TestHelpers;
using InnerDb.Core;

namespace InnerDb.Tests.Index
{
	[TestFixture]
	class IndexStoreTest
	{
		private Car civic = new Car("Honda", "Civic", "White");
		private Car benz = new Car("Mercedes Bebz", "500SL", "Red");

		[Test]
		public void GetObjectsWhereReturnsOnlyIndexedObjectsByFieldValue()
		{
			var type = typeof(Car);
			var i = new IndexStore("TestDb");

			i.AddField(typeof(Car), "Make");
			i.IndexObject(civic, 1);
			Assert.Contains(civic, i.GetObjectsWhere(type, "Make", civic.Make));

			i.AddField(type, "Model");
			Assert.Contains(civic, i.GetObjectsWhere(type, "Model", civic.Model.ToString()));

			i.IndexObject(benz, 2);
			Assert.Contains(benz, i.GetObjectsWhere(type, "Make", benz.Make));
			Assert.Contains(benz, i.GetObjectsWhere(type, "Model", benz.Model.ToString()));

			i.RemoveObject(1);
			Assert.IsFalse(i.GetObjectsWhere(type, "Make", civic.Make).Contains(civic));
			Assert.IsFalse(i.GetObjectsWhere(type, "Model", civic.Model.ToString()).Contains(civic));

			Assert.Throws<ArgumentException>(() =>
			{
				i.GetObjectsWhere(type, "Colour", "Red");
			});
		}
	}
}
