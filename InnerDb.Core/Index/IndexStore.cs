using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace InnerDb.Core.Index
{
	public class IndexStore
	{
		// indexes[Type] = indexes!
		private Dictionary<Type, IndexData<object>> indexes = new Dictionary<Type, IndexData<object>>();
		private string directoryName;

		public IndexStore(string databaseName)
		{
			this.directoryName = databaseName.SantizeForDatabaseName();
			Directory.CreateDirectory(string.Format(@"{0}\Indexes", this.directoryName));
			this.DeserializeIndexes();
		}

		public void AddField(Type type, string fieldName)
		{			
			if (!this.indexes.ContainsKey(type))
			{
				this.indexes[type] = new IndexData<object>();
			}

			this.indexes[type].AddField(fieldName);
		}

		public void IndexObject(object o, int id)
		{
			var type = o.GetType();
			if (!this.indexes.ContainsKey(type))
			{
				this.indexes[type] = new IndexData<object>();
			}

			var index = this.indexes[type];
			// Remove from all indicies
			index.RemoveObject(o);
			// Index all fields that have an index
			index.IndexObject(o, id);
		}

		public void RemoveObject(int id)
		{
			//this.indexes[o.GetType()].RemoveObject(o);
			// Not knowing the type, we must brute-force search :(
			foreach (var index in this.indexes.Values) {
				// TODO: if IDs are not unique *across* objects, this will fail
				if (index.GetObjectIds().Any(i => i == id))
				{
					index.RemoveObject(id);
					break;
				}
			}
		}

		public ReadOnlyCollection<object> GetObjectsWhere(Type type, string fieldName, string value)
		{
			if (this.indexes[type] != null)
			{
				return this.indexes[type].GetObjectsWhere(fieldName, value);
			}
			else
			{
				throw new ArgumentException("There are no indexes for " + type.FullName + ".");
			}
		}

		public ReadOnlyCollection<int> GetObjectsOfType<T>()
		{
			var type = typeof(T);
			if (this.indexes.ContainsKey(type))
			{
				return this.indexes[type].GetObjectIds();
			}
			else
			{
				return new ReadOnlyCollection<int>(new List<int>());
			}
		}

		internal void SerializeIndexes()
		{
			foreach (var index in this.indexes)
			{
				string indexName = index.Key.FullName;
				string json = JsonConvert.SerializeObject(index);
				File.WriteAllText(string.Format(@"{0}\Indexes\{1}.index", this.directoryName, indexName), json);
			}
		}

		internal bool HasObject(int id)
		{
			foreach (var index in this.indexes.Values)
			{
				// TODO: if IDs are not unique *across* objects, this will fail
				if (index.GetObjectIds().Any(i => i == id))
				{
					return true;
				}
			}

			return false;
		}

		private void DeserializeIndexes()
		{
			foreach (var indexFile in Directory.GetFiles(string.Format(@"{0}\Indexes", this.directoryName)))
			{
				string contents = File.ReadAllText(indexFile);
				var index = (KeyValuePair<Type, IndexData<object>>)
					JsonConvert.DeserializeObject(contents,
					typeof(KeyValuePair<Type, IndexData<object>>));
				this.indexes[index.Key] = index.Value;
			}
		}
	}
}
