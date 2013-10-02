using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using InnerDb.Core.Index;

namespace InnerDb.Core.DataStore
{
    class FileDataStore : IDataStore
    {
        private string directoryName = "";
		private IndexStore indexStore;

        private int nextId = 1;

        public FileDataStore(string databaseName, IndexStore indexStore)
        {
			this.directoryName = databaseName.SantizeForDatabaseName();
			this.indexStore = indexStore;

			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}

			if (!Directory.Exists(string.Format(@"{0}\Data", directoryName)))
			{
				Directory.CreateDirectory(string.Format(@"{0}\Data", directoryName));
			}
        }

        public List<T> GetCollection<T>()
        {
            throw new NotImplementedException();
        }

		public Dictionary<int, T> GetCollectionWithId<T>()
		{
			var ids = this.indexStore.GetObjectsOfType<T>();
			Dictionary<int, T> toReturn = new Dictionary<int, T>();

			foreach (var id in ids)
			{
				toReturn[id] = this.GetObject<T>(id);
			}

			return toReturn;
		}

        public T GetObject<T>(int id)
        {
			if (!File.Exists(this.GetPathFor(id)))
            {
                return default(T);                        
            } else {
                string json = File.ReadAllText(this.GetPathFor(id));
                json = json.Substring(json.IndexOf('\n')).Trim();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public void PutObject(object obj, int id)
        {            
            File.WriteAllText(this.GetPathFor(id), obj.Serialize());
        }

        public void DeleteDatabase()
        {
            if (Directory.Exists(this.directoryName))
            {
                Directory.Delete(this.directoryName, true);
                this.nextId = 1;
            }
        }

		public void Delete(int id)
		{
			if (this.ObjectExists(id))
			{
				File.Delete(this.GetPathFor(id));
			}
		}

		internal int GetKeyForNewObject(object obj)
		{
			int toReturn = this.nextId;
			this.nextId++;
			return toReturn;
		}

        // This function should NEVER be called, except when seeding the memory DB
        // from the file system (we don't have anything except type names).
        internal Dictionary<int, object> AllData
        {
            get
            {
                Dictionary<int, object> toReturn = new Dictionary<int,object>();

                if (Directory.Exists(this.directoryName))
                {
                    string[] files = Directory.GetFiles(string.Format(@"{0}\Data", this.directoryName));

                    foreach (string filename in files)
                    {
                        int start = filename.LastIndexOf('\\') + 1;
                        int stop = filename.LastIndexOf(".json");
                        int id = int.Parse(filename.Substring(start, stop - start));

						toReturn[id] = DatabaseHelper.Deserialize(filename);
                        if (id >= this.nextId)
                        {
                            this.nextId = id + 1;
                        }
                    }

                    return toReturn;
                }

                return toReturn;
            }
        }

		private bool ObjectExists(int id)
		{
			return File.Exists(this.GetPathFor(id));
		}

		private string GetPathFor(int id)
		{
			return string.Format(@"{0}\Data\{1}.json", this.directoryName, id);
		}
	}
}
