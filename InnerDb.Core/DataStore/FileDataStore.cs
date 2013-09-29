using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace InnerDb.Core.DataStore
{
    class FileDataStore : IDataStore
    {
        private string directoryName = "";

        private int nextId = 1;

        public FileDataStore(string databaseName)
        {
            // TODO: stop cluttering mah file system
            this.directoryName = databaseName.Replace(" ", "")
                .Replace("!", "").Replace("@", "").Replace("#", "")
                .Replace("{", "").Replace("}", "").Replace("+", "");

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        public List<T> GetCollection<T>()
        {
            throw new NotImplementedException();
        }

        public T GetObject<T>(int id)
        {
			if (!Directory.Exists(this.directoryName) || !File.Exists(this.GetPathFor(id)))
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
            if (!Directory.Exists(this.directoryName))
            {
                Directory.CreateDirectory(this.directoryName);
            }

            var json = JsonConvert.SerializeObject(obj);
            string content = string.Format("{0}\n{1}", obj.GetType().FullName, json);
            File.WriteAllText(this.GetPathFor(id), content);
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
                    string[] files = Directory.GetFiles(this.directoryName);

                    foreach (string filename in files)
                    {
                        int start = filename.LastIndexOf('\\') + 1;
                        int stop = filename.LastIndexOf(".json");
                        int id = int.Parse(filename.Substring(start, stop - start));
                        string content = File.ReadAllText(filename);
                        
                        int pos = content.IndexOf('\n');
                        string json = content.Substring(pos).Trim();
                        string typeName = content.Substring(0, pos);

                        var type = this.LoadType(typeName);
                        var value = JsonConvert.DeserializeObject(json, type);
                        toReturn[id] = value;
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

        private Type LoadType(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (assembly.GetType(typeName) != null)
                {
                    return assembly.GetType(typeName);
                }
            }

            throw new InvalidOperationException("Can't deserialize type of " + typeName + ". The assembly is not loaded into the current app domain.");
        }

		private bool ObjectExists(int id)
		{
			return File.Exists(this.GetPathFor(id));
		}

		private string GetPathFor(int id)
		{
			return string.Format(@"{0}\{1}.json", this.directoryName, id);
		}
	}
}
