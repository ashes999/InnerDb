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
        private InMemoryDataStore memoryStore;
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
            return memoryStore.GetObject<T>(id);
        }

        public int PutObject(object obj) {
            int toReturn = memoryStore.PutObject(obj);
            // TODO: write to journal here
            fileStore.PutObject(obj);
            // TODO: verify journal here
            return toReturn;
        }

        public void DeleteDatabase()
        {
            this.memoryStore.DeleteDatabase();
            this.fileStore.DeleteDatabase();
        }
    }
}
