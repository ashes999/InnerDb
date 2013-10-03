using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InnerDb.Core.DataStore;
using InnerDb.Core.Journal;
using InnerDb.Core.Index;
using System.Collections.ObjectModel;
using InnerDb.Core.Compression;
using System.IO;

namespace InnerDb.Core
{
	class LocalDatabase : IDisposable
	{
		private FileDataStore fileStore; // Primary source of truth
		private FileJournal journal; 
		private InMemoryDataStore memoryStore;
		private IndexStore indexStore;
		private string databaseName;
		private bool isDisposed = false;

		public LocalDatabase(string databaseName)
        {
			this.databaseName = databaseName.SantizeForDatabaseName();

			string archiveName = GetDatabaseFileName(databaseName);
			if (File.Exists(archiveName))
			{
				GzipHelper.DecompressFiles(archiveName);
				File.Delete(archiveName);
			}

			indexStore = new IndexStore(databaseName);
            fileStore = new FileDataStore(databaseName, indexStore);
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
			indexStore.IndexObject(obj, id);

            return id;
        }

		public void Delete(int id)
		{
			memoryStore.Delete(id);
			journal.RecordDelete(id);
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

		public void Dispose()
		{
			if (!this.isDisposed)
			{
				this.isDisposed = true;
				this.journal.Stop();
				this.indexStore.SerializeIndexes();
				this.CreateDatabaseArchive();
			}
		}

		internal void SetJournalIntervalMillseconds(uint milliseconds)
		{
			this.journal.JournalIntervalSeconds = milliseconds;
		}

		internal void AddIndex<T>(string fieldName)
		{
			this.indexStore.AddField(typeof(T), fieldName);
		}

		private string GetDatabaseFileName(string databaseName)
		{
			return string.Format("{0}.innerdb", databaseName);
		}

		private void CreateDatabaseArchive()
		{
			var archiveName = this.GetDatabaseFileName(this.databaseName);
			GzipHelper.CompressFiles(this.databaseName, archiveName);
			Directory.Delete(this.databaseName, true);
		}
	}
}
