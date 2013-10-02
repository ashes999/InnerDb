﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InnerDb.Core.DataStore;
using InnerDb.Core.Journal;
using InnerDb.Core.Index;
using System.Collections.ObjectModel;

namespace InnerDb.Core
{
	class LocalDatabase
	{
		private static LocalDatabase instance = new LocalDatabase();
		public static LocalDatabase Instance { get { return instance; } }
		
		private FileDataStore fileStore; // Primary source of truth
		private FileJournal journal; 
		private InMemoryDataStore memoryStore;
		private IndexStore indexStore;

		private LocalDatabase() { }

		public void OpenDatabase(string databaseName)
        {
            fileStore = new FileDataStore(databaseName);
			journal = new FileJournal(databaseName, fileStore);
            memoryStore = new InMemoryDataStore(fileStore, journal.DirectoryPath);
			indexStore = new IndexStore(databaseName);
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

		public ReadOnlyCollection<T> GetCollectionFromIndex<T>(string fieldName, string value) where T : class
		{
			return new ReadOnlyCollection<T>(
				this.indexStore.GetObjectsWhere(typeof(T), fieldName, value)
				.Select(o => o as T).ToList());
		}

		internal void StopJournal()
		{
			this.journal.Stop();
		}

		internal void SetJournalIntervalMillseconds(uint milliseconds)
		{
			this.journal.JournalIntervalSeconds = milliseconds;
		}

		internal void AddIndex<T>(string fieldName)
		{
			this.indexStore.AddField(typeof(T), fieldName);
		}		
	}
}
