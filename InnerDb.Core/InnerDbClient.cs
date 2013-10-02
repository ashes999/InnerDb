using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InnerDb.Core.DataStore;
using System.Collections.ObjectModel;

namespace InnerDb.Core
{
    public class InnerDbClient : IDisposable
    {
		private LocalDatabase database = LocalDatabase.Instance;

        public InnerDbClient(string databaseName)
        {
			this.database.OpenDatabase(databaseName);
        }

        public List<T> GetCollection<T>()
        {
			return this.database.GetCollection<T>();
        }

        public T GetObject<T>(int id)
        {
			return this.database.GetObject<T>(id);
        }

		public T GetObject<T>(Func<T, bool> predicate)
		{
			return this.database.GetObject<T>(predicate);
		}

        public int PutObject(object obj, int id = 0) {
			return this.database.PutObject(obj, id);
        }

		public void Delete(int id)
		{
			this.database.Delete(id);
		}

        public void DeleteDatabase()
        {
			this.database.DeleteDatabase();
        }

		public void SetJournalIntervalMilliseconds(uint milliseconds)
		{
			this.database.SetJournalIntervalMillseconds(milliseconds);
		}

		public void AddIndex<T>(string fieldName) {
			this.database.AddIndex<T>(fieldName);
		}

		public ReadOnlyCollection<T> GetCollectionFromIndex<T>(string fieldName, string value) where T : class
		{
			return this.database.GetCollectionFromIndex<T>(fieldName, value);
		}

		public void Dispose()
		{
			this.database.Stop();
		}
	}
}
