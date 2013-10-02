using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InnerDb.Core;
using InnerDb.Core.DataStore;
using InnerDb.Tests.TestHelpers;
using NUnit.Framework;
using System.IO;
using System.Threading;

namespace InnerDb.Tests
{
    [TestFixture]
    class InnerDbClientTest
    {        
	    // Helper objects
        private Sword masamune = new Sword() { Name = "Masamune", Cost = 10000 };
        private Sword murasame = new Sword { Name = "Murasame", Cost = 5000 };
        private dynamic lavos = new Creature() { Name = "Lavos", Disposition = Alignment.Evil };

		[SetUp]
		public void DeleteAllDatabases()
		{
			foreach (var dir in Directory.GetDirectories("."))
			{
				while (Directory.Exists(dir))
				{
					try
					{
						Directory.Delete(dir, true);
					}
					catch { }
				}
			}
		}

        [Test]
        public void TrueIsTrue()
        {
            Assert.IsTrue(true);
        }

        [Test]
        public void GetCollectionReturnsEmptyListIfNoItemsExist()
        {
			using (var client = new InnerDbClient("Empty"))
			{
				var actual = client.GetCollection<Sword>();
				Assert.IsNotNull(actual);
				Assert.AreEqual(0, actual.Count);
			}
        }

        [Test]
        public void PutObjectReturnsNonZeroId()
        {
			using (var client = new InnerDbClient("Empty"))
			{
				var actual = client.PutObject(masamune);
				Assert.Greater(actual, 0);
			}
        }

        [Test]
        public void GetByIdReturnsPutObject()
        {
			using (var client = new InnerDbClient("GetAndPut"))
			{
				var id = client.PutObject(masamune);
				Assert.Greater(id, 0);
				var actual = client.GetObject<Sword>(id);
				Assert.AreEqual(masamune, actual);
			}
        }

		[Test]
		public void GetByPredicateReturnsPutObject()
		{
			using (var client = new InnerDbClient("GetByPredicate"))
			{
				client.PutObject(masamune);
				client.PutObject(murasame);
				var actual = client.GetObject<Sword>(s => s.Name == "Masamune");
				Assert.AreEqual(masamune, actual);
			}
		}

        [Test]
        public void GetCollectionReturnsPutObjects()
        {
			using (var client = new InnerDbClient("GetCollection"))
			{
				var actual = client.GetCollection<Sword>();

				client.PutObject(masamune);
				client.PutObject(murasame);
				client.PutObject(lavos);

				actual = client.GetCollection<Sword>();
				Assert.AreEqual(2, actual.Count);
				Assert.AreEqual(masamune, actual.ElementAt(0));
				Assert.AreEqual(murasame, actual.ElementAt(1));
			}
        }

        [Test]
        public void PutObjectIncrementsIdRegardlessOfType()
        {
			using (var client = new InnerDbClient("AutoIncrementId"))
			{
				int first = client.PutObject(murasame);
				Assert.AreEqual(1, first);
				int second = client.PutObject(lavos);
				Assert.AreEqual(2, second);
				int third = client.PutObject(murasame);
				Assert.AreEqual(3, third);
			}
        }

        [Test]
        public void EnumsDeserializeCorrectly()
        {
			using (var client = new InnerDbClient("EnumSerialization"))
			{
				var expected = new Creature() { Name = "Chrono", Disposition = Alignment.Good };
				int id = client.PutObject(expected);
				var actual = client.GetObject<Creature>(c => c.Name == "Chrono");
				Assert.AreEqual(Alignment.Good, actual.Disposition);
			}
        }

        [Test]
        public void DataPersistsAcrossClients()
        {
			string dbName = "FileStorePersists";
			int swordId, creatureId = 0;

			using (var client = new InnerDbClient(dbName))
			{
				swordId = client.PutObject(murasame);
				creatureId = client.PutObject(lavos);
			}

			using (var client = new InnerDbClient(dbName))
			{
				var actualSword = client.GetObject<Sword>(swordId);
				Assert.AreEqual(murasame, actualSword);
				var actualCreature = client.GetObject<Creature>(creatureId);
				Assert.AreEqual(lavos, actualCreature);
			}
        }

		[Test]
		public void DeleteDeletesObject()
		{
			using (var client = new InnerDbClient("Delete"))
			{
				int swordId = client.PutObject(masamune);
				Assert.IsNotNull(client.GetObject<Sword>(s => s.Name == "Masamune"));
				client.Delete(swordId);
				Assert.IsNull(client.GetObject<Sword>(swordId));
			}
		}

		[Test]
		public void IdsAreNotReusedAfterDeletion()
		{
			using (var client = new InnerDbClient("IdReuse"))
			{
				int first = client.PutObject(masamune);
				int second = client.PutObject(murasame);
				client.Delete(second);
				int third = client.PutObject(lavos);
				Assert.AreNotEqual(second, third);
			}
		}

		[Test]
		public void PutObjectWithIdUpdatesObject()
		{
			string dbName = "PutOrEdit";
			var expected = new Sword() { Name = "Excalibur", Cost = 1 };

			using (var client = new InnerDbClient(dbName))
			{
				int id = client.PutObject(masamune);
				Assert.IsNotNull(client.GetObject<Sword>(id));
				client.PutObject(expected, id);
				var actual = client.GetObject<Sword>(id);
				Assert.IsNotNull(actual);
				Assert.AreEqual(expected, actual);
			}

			using (var client = new InnerDbClient(dbName))
			{
				var actual = client.GetObject<Sword>(s => s.Name == "Excalibur");
				Assert.AreEqual(expected, actual);
			}
		}

		[Test]
		public void PutObjectWithIdUpdatesObjectAndDoesntCrashIndex()
		{
			string dbName = "PutOrEditWithIndex";
			var expected = new Sword() { Name = "Excalibur", Cost = 1 };
			using (var client = new InnerDbClient(dbName))
			{
				int id = client.PutObject(masamune);
				Assert.IsNotNull(client.GetObject<Sword>(id));
				client.AddIndex<Sword>("Name"); 

				client.PutObject(expected, id);
				var actual = client.GetObject<Sword>(id);
				Assert.IsNotNull(actual);
				Assert.AreEqual(expected, actual);
			}

			using (var client = new InnerDbClient(dbName))
			{
				var actual = client.GetObject<Sword>(s => s.Name == "Excalibur");
				Assert.AreEqual(expected, actual);
			}
		}

		[Test]
		public void JournalWritesJournalFilesAndRunsThemOnStartup()
		{
			string dbName = "JournalingAndRecovery";
			string journalDir = string.Format(@"{0}\Journal", dbName);
			string dataDir = string.Format(@"{0}\Data", dbName);
			int id, secondId = 0;

			using (var client = new InnerDbClient(dbName))
			{
				// Believe me when I say this is very heavily tested already from
				// the above tests running really fast.
				client.SetJournalIntervalMilliseconds(10000);

				id = client.PutObject(murasame);
				client.Delete(id);
				secondId = client.PutObject(masamune);

				// White-box of sorts. Okay for now.
				string[] files = System.IO.Directory.GetFiles(journalDir);

				Assert.AreEqual(3, files.Length);
				Assert.IsTrue(files.Any(f => f.Contains(id + "-Put.json")));
				Assert.IsTrue(files.Any(f => f.Contains(id + "-Delete.json")));
				Assert.IsTrue(files.Any(f => f.Contains(secondId + "-Put.json")));				
			}

			using (var client = new InnerDbClient(dbName))
			{
				string[] data = new string[1];

				// Wait for victory.
				bool isDone = false;
				var start = DateTime.Now;

				while (!isDone && ((DateTime.Now - start).TotalSeconds <= 5))
				{
					var files = System.IO.Directory.GetFiles(journalDir);
					data = System.IO.Directory.GetFiles(dataDir);
					isDone = (files.Length == 0 && dataDir.Length == 1 && data.First().Contains("2.json"));
				}

				Assert.IsNull(client.GetObject<Sword>(id));
				Assert.AreEqual(masamune, client.GetObject<Sword>(secondId));
			}
		}
    }
}
