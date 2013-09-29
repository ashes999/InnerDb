using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InnerDb.Core.DataStore;

namespace InnerDb.Core
{
    public class InnerDbClient
    {
		// Cache-like device
        private InMemoryDataStore memoryStore;
		// Primary source of truth
        private FileDataStore fileStore;

        public InnerDbClient(string databaseName)
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
            // TODO: if not in memory, get from disk and put in memory
			if (memoryStore.HasObject(id)) {
				return memoryStore.GetObject<T>(id);
			} else {
				return fileStore.GetObject<T>(id);
			}
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
    }
}
