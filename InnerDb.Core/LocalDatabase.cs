using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InnerDb.Core.DataStore;
using InnerDb.Core.Journal;

namespace InnerDb.Core
{
	class LocalDatabase
	{
		private static LocalDatabase instance = new LocalDatabase();
		public static LocalDatabase Instance { get { return instance; } }
		
		private FileDataStore fileStore; // Primary source of truth
		private FileJournal journal; 
		private InMemoryDataStore memoryStore;

		private LocalDatabase() { }

		public void OpenDatabase(string databaseName)
        {
            fileStore = new FileDataStore(databaseName);
			journal = new FileJournal(databaseName, fileStore);
            memoryStore = new InMemoryDataStore(fileStore, journal.DirectoryPath);
        }

        public List<T> GetCollection<T>()
        {
            return memoryStore.GetCollection<T>();
        }

        public T GetObject<T>(int id)
        {
			if (memoryStore.HasObject(id))
			{
				return memoryStore.GetObject<T>(id);
			}
			else
			{
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
			journal.RecordWrite(obj, id);
            // TODO: verify journal here
            return id;
        }

		public void Delete(int id)
		{
			memoryStore.Delete(id);
			journal.RecordDelete(id);
			// TODO: verify journal
		}

        public void DeleteDatabase()
        {
            this.memoryStore.DeleteDatabase();
			this.journal.DeleteDatabase();
			this.fileStore.DeleteDatabase();
        }

		internal void StopJournal()
		{
			this.journal.Stop();
		}

		internal void SetJournalIntervalMillseconds(uint milliseconds)
		{
			this.journal.JournalIntervalSeconds = milliseconds;
		}
	}
}
