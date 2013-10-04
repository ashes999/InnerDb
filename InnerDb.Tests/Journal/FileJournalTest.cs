using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using InnerDb.Core.Journal;
using InnerDb.Tests.TestHelpers;
using System.IO;
using System.Threading;
using InnerDb.Core.DataStore;

namespace InnerDb.Tests.Journal
{
	[TestFixture]
	class FileJournalTest
	{
		[Test]
		public void IntervalThrowsOutsideOf100Or10000()
		{
			var journal = new FileJournal("Interval", null);
			try
			{
				Assert.Throws<ArgumentException>(() =>
				{
					journal.JournalIntervalSeconds = 17;
				});
				Assert.Throws<ArgumentException>(() =>
				{
					journal.JournalIntervalSeconds = 10003;
				});
				Assert.DoesNotThrow(() =>
				{
					journal.JournalIntervalSeconds = 259;
				});
			}
			finally
			{
				journal.DeleteDatabase();
			}
		}

		[Test]
		public void RecordWriteCreatesJournalEntry()
		{
			var journal = new FileJournal("Wrote", null);
			journal.JournalIntervalSeconds = 10000;

			var gates = new Creature("Bill Gates", Alignment.Good);
			string dir = string.Format(@"{0}\Journal", journal.DirectoryPath);

			try
			{
				Assert.AreEqual(0, Directory.GetFiles(dir).Length);
				journal.RecordWrite(gates);
				Assert.AreEqual(1, Directory.GetFiles(dir).Length);
			}
			finally
			{
				journal.DeleteDatabase();
				Directory.Delete("Wrote");
			}
		}

		[Test]
		public void RecordDeleteCreatesJournalEntry()
		{
			var journal = new FileJournal("Wrote", null);
			journal.JournalIntervalSeconds = 10000;

			string dir = string.Format(@"{0}\Journal", journal.DirectoryPath);

			try
			{
				Assert.AreEqual(0, Directory.GetFiles(dir).Length);
				journal.RecordDelete(1);
				Assert.AreEqual(1, Directory.GetFiles(dir).Length);
			}
			finally
			{
				journal.DeleteDatabase();
				Directory.Delete("Wrote");
			}
		}
	}
}
