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

        // For deserialization, we have to be able to get an object back from a type
        private Dictionary<string, Type> knownTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Creates a new file data store.
        /// </summary>
        /// <param name="databaseName">The name of the database</param>
        /// <param name="typeAssemblies">A list of assemblies. Used when seeding a memory store from a file store.</param>
        public FileDataStore(string databaseName, string[] typeAssemblies)
        {
            // TODO: stop cluttering mah file system
            this.directoryName = databaseName.Replace(" ", "")
                .Replace("!", "").Replace("@", "").Replace("#", "")
                .Replace("{", "").Replace("}", "").Replace("+", "");

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            // Load types. Used to get values back once persisted, when we only have type names.
            if (typeAssemblies != null)
            {
                foreach (var assembly in typeAssemblies)
                {
                    foreach (Type t in Assembly.Load(assembly).GetTypes())
                    {
                        this.knownTypes[t.FullName] = t;
                    }
                }
            }
        }

        public List<T> GetCollection<T>()
        {
            throw new NotImplementedException();
        }

        public T GetObject<T>(int id)
        {
            if (!Directory.Exists(this.directoryName))
            {
                return default(T);
            }
            else
            {
                string json = File.ReadAllText(this.getPathFor(id));
                json = json.Substring(json.IndexOf('\n')).Trim();
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public int PutObject(object obj)
        {
            if (!Directory.Exists(this.directoryName))
            {
                Directory.CreateDirectory(this.directoryName);
            }

            int id = Directory.GetFiles(this.directoryName).Length + 1;
            var json = JsonConvert.SerializeObject(obj);
            string content = string.Format("{0}\n{1}", obj.GetType().FullName, json);
            File.WriteAllText(this.getPathFor(id), content);
            return id;
        }

        public void DeleteDatabase()
        {
            if (Directory.Exists(this.directoryName))
            {
                Directory.Delete(this.directoryName, true);
            }
        }

        // This horrible, terrible function should NEVER be called, except when seeding
        // the memory DB from the file system (we don't have anything except type names).
        // If you can ditch this, ditch the knownTypes dictionary, and remove it from
        // the constructor.
        internal Dictionary<int, object> DataById
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

                        if (this.knownTypes.ContainsKey(typeName))
                        {
                            var type = this.knownTypes[typeName];
                            var value = JsonConvert.DeserializeObject(json, type);
                            toReturn[id] = value;
                        }
                        else
                        {
                            throw new InvalidOperationException("Can't deserialize type of " + typeName + ". Please pass the assembly name into the client constructor.");
                        }
                    }

                    return toReturn;
                }

                return toReturn;
            }
        }



        private string getPathFor(int id)
        {
            return string.Format(@"{0}\{1}.json", this.directoryName, id);
        }
    }
}
