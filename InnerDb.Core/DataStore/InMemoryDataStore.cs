using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnerDb.Core.DataStore
{
    class InMemoryDataStore : IDataStore
    {
        // TODO: dictionary<type, ilist<t>> doesn't work well :(
        private Dictionary<int, object> data = new Dictionary<int, object>();
        private int nextId = 1;
        private FileDataStore fileStore;

        public InMemoryDataStore(FileDataStore fileStore)
        {
            this.fileStore = fileStore;

            // Seed from the file store, preserving keys
            foreach (var kvp in this.fileStore.DataById)
            {
                this.data[kvp.Key] = kvp.Value;
            }
        }
        
        public List<T> GetCollection<T>()
        {
            var toReturn = new List<T>();
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

        public int PutObject(object obj)
        {
            int toReturn = this.nextId;

            // TODO: if RAM is limited, dump something
            this.data[this.nextId] = obj;
            nextId = this.data.Keys.Max() + 1;
            return toReturn;
        }

        public void DeleteDatabase()
        {
            this.data.Clear();
            this.nextId = 1;
        }
    }
}
