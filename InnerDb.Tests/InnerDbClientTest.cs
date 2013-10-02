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

namespace InnerDb.Tests
{
    [TestFixture]
    class InnerDbClientTest
    {
        private readonly string TestDbName = "TestDB";
        private InnerDbClient client;
		private readonly uint JournalInterval = 100;

        // Helper objects
        private Sword masamune = new Sword() { Name = "Masamune", Cost = 10000 };
        private Sword murasame = new Sword { Name = "Murasame", Cost = 5000 };
        private dynamic lavos = new Creature() { Name = "Lavos", Disposition = Alignment.Evil };

        [SetUp]
        public void ResetClientAndDeleteDatabase()
        {
			if (client != null)
			{
				client.Dispose();
			}

			// Delete DB
			while (Directory.Exists(TestDbName))
			{
				try
				{
					Directory.Delete(TestDbName, true);
				}
				catch { }
			}

			this.ResetClient();
		}

        [Test]
        public void TrueIsTrue()
        {
            Assert.IsTrue(true);
        }

        [Test]
        public void GetCollectionReturnsEmptyListIfNoItemsExist()
        {
            var actual = client.GetCollection<Sword>();
            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Count);
        }

        [Test]
        public void PutObjectReturnsNonZeroId()
        {
            var actual = client.PutObject(masamune);
            Assert.Greater(actual, 0);
        }

        [Test]
        public void GetByIdReturnsPutObject()
        {
            var id = client.PutObject(masamune);
            Assert.Greater(id, 0);
            var actual = client.GetObject<Sword>(id);
            Assert.AreEqual(masamune, actual);
        }

		[Test]
		public void GetByPredicateReturnsPutObject()
		{
			client.PutObject(masamune);
			client.PutObject(murasame);
			var actual = client.GetObject<Sword>(s => s.Name == "Masamune");
			Assert.AreEqual(masamune, actual);
		}

        [Test]
        public void GetCollectionReturnsPutObjects()
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

        [Test]
        public void PutObjectIncrementsIdRegardlessOfType()
        {
            int first = client.PutObject(murasame);
            Assert.AreEqual(1, first);
            int second = client.PutObject(lavos);
            Assert.AreEqual(2, second);
            int third = client.PutObject(murasame);
            Assert.AreEqual(3, third);
        }

        [Test]
        public void EnumsDeserializeCorrectly()
        {
            var expected = new Creature() { Name = "Chrono", Disposition = Alignment.Good };
            int id = client.PutObject(expected);
            var actual = client.GetObject<Creature>(c => c.Name == "Chrono");
            Assert.AreEqual(Alignment.Good, actual.Disposition);
        }

        [Test]
        public void DataPersistsAcrossClients()
        {
            int swordId = client.PutObject(murasame);
            int creatureId = client.PutObject(lavos);

            this.ResetClient(); // Like reconnecting
            
            var actualSword = client.GetObject<Sword>(swordId);
            Assert.AreEqual(murasame, actualSword);
            var actualCreature = client.GetObject<Creature>(creatureId);
            Assert.AreEqual(lavos, actualCreature);
        }

		[Test]
		public void DeleteDeletesObject()
		{
			int swordId = client.PutObject(masamune);
			Assert.IsNotNull(client.GetObject<Sword>(s => s.Name == "Masamune"));
			client.Delete(swordId);
			Assert.IsNull(client.GetObject<Sword>(swordId));
		}

		[Test]
		public void IdsAreNotReusedAfterDeletion()
		{
			int first = client.PutObject(masamune);
			int second = client.PutObject(murasame);
			client.Delete(second);
			int third = client.PutObject(lavos);
			Assert.AreNotEqual(second, third);
		}

		[Test]
		public void PutObjectWithIdUpdatesObject()
		{
			int id = client.PutObject(masamune);
			var expected = new Sword() { Name = "Excalibur", Cost = 1 };
			client.PutObject(expected, id);
			var actual = client.GetObject<Sword>(id);
			Assert.AreEqual(expected, actual);

			this.ResetClient();
			actual = client.GetObject<Sword>(s => s.Name == "Excalibur");
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void EpicFail()
		{
			int id = client.PutObject(masamune);
			client.AddIndex<Sword>("Name"); // iQ

			var expected = new Sword() { Name = "Excalibur", Cost = 1 };
			client.PutObject(expected, id);
			var actual = client.GetObject<Sword>(id);
			Assert.AreEqual(expected, actual);

			this.ResetClient();
			actual = client.GetObject<Sword>(s => s.Name == "Excalibur");
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void JournalWritesJournalFilesAndRunsThemOnStartup()
		{
			// Believe me when I say this is very heavily tested already from
			// the above tests running really fast.
			this.AlmostDisableJournaling();

			int id = client.PutObject(murasame);
			client.Delete(id);
			int secondId = client.PutObject(masamune);

			// White-box of sorts. Okay for now.
			string journalDir = string.Format(@"{0}\Journal", TestDbName);
			string dataDir = string.Format(@"{0}\Data", TestDbName);

			string[] files = System.IO.Directory.GetFiles(journalDir);

			Assert.AreEqual(3, files.Length);
			Assert.IsTrue(files.Any(f => f.Contains(id + "-Put.json")));
			Assert.IsTrue(files.Any(f => f.Contains(id + "-Delete.json")));
			Assert.IsTrue(files.Any(f => f.Contains(secondId + "-Put.json")));

			client.SetJournalIntervalMilliseconds(JournalInterval);
			this.ResetClient();

			string[] data = new string[1];

			// Wait for victory.
			bool isDone = false;
			var start = DateTime.Now;

			while (!isDone && ((DateTime.Now - start).TotalSeconds <= 5))
			{
				files = System.IO.Directory.GetFiles(journalDir);
				data = System.IO.Directory.GetFiles(dataDir);
				isDone = (files.Length == 0 && dataDir.Length == 1 && data.First().Contains("2.json"));
			}

			Assert.IsNull(client.GetObject<Sword>(id));
			Assert.AreEqual(masamune, client.GetObject<Sword>(secondId));
		}

		private void ResetClient()
		{
			if (client != null)
			{
				client.Dispose();
			}
			client = new InnerDbClient(TestDbName);
			client.SetJournalIntervalMilliseconds(JournalInterval);
		}

		private void AlmostDisableJournaling()
		{			
			client.SetJournalIntervalMilliseconds(10000); // 10s is enough to test it...
		}
    }
}
