using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InnerDb.Core.DataStore;
using System.Timers;

namespace InnerDb.Core
{
	class LocalDatabase
	{
		private static LocalDatabase instance = new LocalDatabase();
		public static LocalDatabase Instance { get { return instance; } }

        private InMemoryDataStore memoryStore;
		// Primary source of truth
        private FileDataStore fileStore;
		private Timer journalTimer = new Timer(TimeSpan.FromMilliseconds(100).TotalMilliseconds);

		private LocalDatabase() {

			this.journalTimer.Elapsed += (sender, args) =>
			{
				this.ProcessJournalEntries();
			};

			this.journalTimer.Start();
		}

		public void OpenDatabase(string databaseName)
        {
            fileStore = new FileDataStore(databaseName);
            memoryStore = new InMemoryDataStore(fileStore);
        }

        public List<T> GetCollection<T>()
        {
            return memoryStore.GetCollection<T>();
        }

        public T GetObject<T>(int id)
        {
			if (memoryStore.HasObject(id)) {
				return memoryStore.GetObject<T>(id);
			} else {
				return fileStore.GetObject<T>(id);
			}
        }

		public T GetObject<T>(Func<T, bool> predicate)
		{
			return this.GetCollection<T>().First(obj => predicate.Invoke(obj) == true);
		}

        public int PutObject(object obj, int id = 0) {
			if (id == 0)
			{
				id = fileStore.GetKeyForNewObject(obj);
			}
			memoryStore.PutObject(obj, id);
            // TODO: write to journal here
            fileStore.PutObject(obj, id);
            // TODO: verify journal here
            return id;
        }

		public void Delete(int id)
		{
			memoryStore.Delete(id);
			// TODO: journal: +delete
			fileStore.Delete(id);
			// TODO: verify journal
		}

        public void DeleteDatabase()
        {
            this.memoryStore.DeleteDatabase();
            this.fileStore.DeleteDatabase();
        }

		private void ProcessJournalEntries()
		{
		}
	}
}
