using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using InnerDb.Core.Journal;

namespace InnerDb.Core.DataStore
{
    class InMemoryDataStore : IDataStore
    {
        // TODO: dictionary<type, ilist<t>> doesn't work well :(
        private Dictionary<int, object> data = new Dictionary<int, object>();
        private FileDataStore fileStore;

		// Do we have the latest and greatest of these objects? If so, they're here.
		private List<Type> synchedWithFileStore = new List<Type>();

		public InMemoryDataStore()
		{
		}

        public InMemoryDataStore(FileDataStore fileStore, string journalDirectory)
        {
            this.fileStore = fileStore;

            // Seed from the file store, preserving keys
            foreach (var kvp in this.fileStore.AllData)
            {
                this.data[kvp.Key] = kvp.Value;
            }

			// Seed any non-persisted puts from our journal
			IEnumerable<string> putEntries = Directory.GetFiles(journalDirectory)
				.Where(s => s.EndsWith(FileJournal.PutEntryPrefix));

			foreach (string filename in putEntries)
			{
				int id = DatabaseHelper.GetIdFromFilename(filename);
				var data = DatabaseHelper.Deserialize(filename);
				this.data[id] = data;

				var type = data.GetType();
				if (!this.synchedWithFileStore.Contains(type))
				{
					this.synchedWithFileStore.Add(type);
				}
			}
        }
        
        public List<T> GetCollection<T>()
        {
            var toReturn = new List<T>();

			if (!this.synchedWithFileStore.Contains(typeof(T)))
			{
				this.SynchWithFileStore<T>();
			}

            foreach (var value in data.Values)
            {
                if (value is T)
                {
                    toReturn.Add((T)value);
                }
            }

            return toReturn;
        }

        public T GetObject<T>(int id)
        {
			if (!this.synchedWithFileStore.Contains(typeof(T)))
			{
				this.SynchWithFileStore<T>();
			}

            var toReturn = data[id];
            if (toReturn is T)
            {
                return (T)toReturn;
            }
            else
            {
                throw new ArgumentException("Item with ID=" + id + " isn't a " + typeof(T).FullName + ", it's a " + toReturn.GetType().FullName);
            }
        }

        public void PutObject(object obj, int id)
        {
            // TODO: if RAM is limited, dump something
            this.data[id] = obj;
        }

        public void DeleteDatabase()
        {
            this.data.Clear();
        }

		internal bool HasObject(int id)
		{
			return this.data.ContainsKey(id);
		}


		public void Delete(int id)
		{
			if (this.HasObject(id))
			{
				this.data.Remove(id);
			}
			else
			{
				throw new ArgumentException("There's no object with ID " + id + " to delete.");
			}
		}

		private void SynchWithFileStore<T>()
		{
			Dictionary<int, T> found = this.fileStore.GetCollectionWithId<T>();
			foreach (var kvp in found)
			{
				this.data[kvp.Key] = kvp.Value;
			}
			
			var type = typeof(T);
			if (!this.synchedWithFileStore.Contains(type))
			{
				this.synchedWithFileStore.Add(type);
			}
		}
	}
}
