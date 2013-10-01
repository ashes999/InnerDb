using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using InnerDb.Core.Index;
using InnerDb.Tests.TestHelpers;

namespace InnerDb.Tests.Index
{
	[TestFixture]
	class IndexDataTest
	{
		private Sword excalibur = new Sword() { Name = "Excaliburz", Cost = 40000 };
		private Sword calendor = new Sword { Name = "Calendor", Cost = 0 };

		// The ultimate integration test. You can ignore everything else.
		[Test]
		public void GetObjectsWhereReturnsOnlyIndexedObjectsByFieldValue()
		{
			var i = new IndexData<Sword>();
			i.AddField("Name");
			i.IndexObject(excalibur, 1);
			Assert.Contains(excalibur, i.GetObjectsWhere("Name", excalibur.Name));

			i.AddField("Cost");
			Assert.Contains(excalibur, i.GetObjectsWhere("Cost", excalibur.Cost.ToString()));

			i.IndexObject(calendor, 2);
			Assert.Contains(calendor, i.GetObjectsWhere("Name", calendor.Name));
			Assert.Contains(calendor, i.GetObjectsWhere("Cost", calendor.Cost.ToString()));

			i.RemoveObject(excalibur);
			Assert.IsFalse(i.GetObjectsWhere("Name", excalibur.Name).Contains(excalibur));
			Assert.IsFalse(i.GetObjectsWhere("Cost", excalibur.Cost.ToString()).Contains(excalibur));
		}
	}
}
