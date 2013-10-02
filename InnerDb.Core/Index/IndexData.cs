using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace InnerDb.Core.Index
{
	public class IndexData<T>
	{
		// Outer: data[fieldName] => Inner
		// Inner: data[fieldValue] => IDs
		[JsonPropertyAttribute(DefaultValueHandling = DefaultValueHandling.Include)]
		private Dictionary<string, Dictionary<string, List<int>>> indexedData = new Dictionary<string, Dictionary<string, List<int>>>();

		[JsonPropertyAttribute(DefaultValueHandling = DefaultValueHandling.Include)]
		private Dictionary<T, int> indexedObjectIds = new Dictionary<T, int>();
		
		public ReadOnlyCollection<T> GetObjectsWhere(string fieldName, string value)
		{
			List<T> toReturn = new List<T>();
			if (indexedData.ContainsKey(fieldName))
			{
				var outer = indexedData[fieldName];
				if (outer.ContainsKey(value))
				{
					var inner = outer[value];
					toReturn.AddRange(this.indexedObjectIds.Where(kvp => inner.Contains(kvp.Value)).Select(k => k.Key));
				}
			}
			else
			{
				throw new ArgumentException(fieldName + " is not indexed for " + typeof(T).FullName);
			}
			return new ReadOnlyCollection<T>(toReturn);
		}

		public void IndexObject(T o, int id)
		{
			this.indexedObjectIds[o] = id;

			foreach (string fieldName in this.indexedData.Keys)
			{
				IndexField(o, id, fieldName);
			}
		}

		public void AddField(string fieldName)
		{
			// Create index for this field
			this.indexedData[fieldName] = new Dictionary<string, List<int>>();
			// Reindex all objects on this field
			this.Reindex(fieldName);
		}

		private void Reindex(string fieldName)
		{
			foreach (var o in this.indexedObjectIds.Keys)
			{
				this.IndexField(o, this.indexedObjectIds[o], fieldName);
			}
		}

		public void RemoveObject(T o)
		{
			if (this.indexedObjectIds.ContainsKey(o))
			{
				int id = this.indexedObjectIds[o];

				foreach (var fieldName in this.indexedData.Keys)
				{
					string value = o.GetType().GetProperty(fieldName).GetValue(o, null).ToString();
					if (this.indexedData[fieldName].ContainsKey(value))
					{
						var index = this.indexedData[fieldName][value];
						index.Remove(id);
					}
				}

				// Forget it exists (so it can be GCed)
				this.indexedObjectIds.Remove(o);
			}
		}

		private void IndexField(T o, int id, string fieldName)
		{
			var fieldInfo = o.GetType().GetProperty(fieldName);
			if (fieldInfo != null)
			{
				string value = fieldInfo.GetValue(o, null).ToString();
				if (value != null)
				{
					// Are we indexing this field?
					if (!this.indexedData.ContainsKey(fieldName))
					{
						this.indexedData[fieldName] = new Dictionary<string, List<int>>();
					}
					// Does the index for this field, index this value?
					if (!this.indexedData[fieldName].ContainsKey(value))
					{
						this.indexedData[fieldName][value] = new List<int>();
					}
					// Does thie list of ints with this value, have this ID?
					if (!this.indexedData[fieldName][value].Contains(id))
					{
						this.indexedData[fieldName][value].Add(id);
					}
				}
			}
		}

	}
}
