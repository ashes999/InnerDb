using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InnerDb.Core.Index
{
	public class IndexStore
	{
		// indexes[Type] = indexes!
		private IDictionary<Type, IndexData> indexes = new Dictionary<Type, IndexData>();

		//public bool HasIndex<T>(string fieldName)
		//{
		//    var type = typeof(T);
		//    return this.indexes[type] != null && this.indexes[type].IndexesField(fieldName);
		//}

		public void AddIndex(Type type, string fieldName)
		{			
			if (!this.indexes.ContainsKey(type))
			{
				this.indexes[type] = new IndexData();
			}

			this.indexes[type].AddField(fieldName);
		}

		public void IndexObject(object o, int id)
		{
			var type = o.GetType();
			if (!this.indexes.ContainsKey(type))
			{
				throw new ArgumentException("Not indexing " + type.FullName + " yet. Call AddIndex first.");
			}
			else
			{
				var index = this.indexes[type];
				// Remove from all indicies
				index.RemoveObject(o);
				// Index all fields that have an index
				index.IndexObject(o, id);
			}
		}

		public void RemoveObject(object o)
		{
			this.indexes[o.GetType()].RemoveObject(o);
		}

		class IndexData
		{
			// Outer: data[fieldName] => Inner
			// Inner: data[fieldValue] => IDs
			private Dictionary<string, Dictionary<string, List<int>>> indexdata = new Dictionary<string, Dictionary<string, List<int>>>();
			private Dictionary<object, int> indexedObjects = new Dictionary<object, int>();

			//public bool IndexesField(string fieldName)
			//{
			//    return this.indexdata.ContainsKey(fieldName);
			//}

			public void IndexObject(object o, int id)
			{
				this.indexedObjects[o] = id;

				foreach (string fieldName in this.indexdata.Keys)
				{
					IndexField(o, id, fieldName);
				}
			}

			public void AddField(string fieldName)
			{
				// Create index for this field
				this.indexdata[fieldName] = new Dictionary<string, List<int>>();
				// Reindex all objects on this field
				foreach (var o in this.indexedObjects.Keys)
				{
					this.IndexField(o, this.indexedObjects[o], fieldName);
				}
			}

			private void IndexField(object o, int id, string fieldName)
			{
				var fieldInfo = o.GetType().GetProperty(fieldName);
				if (fieldInfo != null)
				{
					string value = fieldInfo.GetValue(o, null).ToString();
					if (value != null)
					{
						// Are we indexing this field?
						if (!this.indexdata.ContainsKey(fieldName))
						{
							this.indexdata[fieldName] = new Dictionary<string, List<int>>();
						}
						// Does the index for this field, index this value?
						if (!this.indexdata[fieldName].ContainsKey(value))
						{
							this.indexdata[fieldName][value] = new List<int>();
						}
						// Does thie list of ints with this value, have this ID?
						if (!this.indexdata[fieldName][value].Contains(id))
						{
							this.indexdata[fieldName][value].Add(id);
						}
					}
				}
			}

			public void AddIndex(string fieldName)
			{
				foreach (var o in this.indexedObjects.Keys)
				{
					this.IndexField(o, this.indexedObjects[o], fieldName);
				}
			}

			public void RemoveObject(object o)
			{
				if (this.indexedObjects.ContainsKey(o))
				{
					int id = this.indexedObjects[o];

					foreach (var fieldName in this.indexdata.Keys)
					{
						string value = o.GetType().GetProperty(fieldName).GetValue(o, null).ToString();
						if (this.indexdata[fieldName].ContainsKey(value))
						{
							var index = this.indexdata[fieldName][value];
							index.Remove(id);
						}
					}

					// Forget it exists (so it can be GCed)
					this.indexedObjects.Remove(o);
				}
			}
		}
	}
}
