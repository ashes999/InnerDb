using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace InnerDb.Core.Index
{
	public class IndexData<T>
	{
		// Outer: data[fieldName] => Inner
		// Inner: data[fieldValue] => IDs
		private Dictionary<string, Dictionary<string, List<int>>> indexdata = new Dictionary<string, Dictionary<string, List<int>>>();
		private Dictionary<T, int> indexedObjectIds = new Dictionary<T, int>();

		//public ReadOnlyCollection<string> IndexedFields
		//{
		//    get
		//    {
		//        return new ReadOnlyCollection<string>(this.indexdata.Keys.ToList());
		//    }
		//}

		public ReadOnlyCollection<T> GetObjectsWhere(string fieldName, string value)
		{
			List<T> toReturn = new List<T>();
			if (indexdata.ContainsKey(fieldName))
			{
				var outer = indexdata[fieldName];
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

		//public bool IndexesField(string fieldName)
		//{
		//    return this.indexdata.ContainsKey(fieldName);
		//}

		public void IndexObject(T o, int id)
		{
			this.indexedObjectIds[o] = id;

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
			foreach (var o in this.indexedObjectIds.Keys)
			{
				this.IndexField(o, this.indexedObjectIds[o], fieldName);
			}
		}

		public void Reindex(string fieldName)
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

	}
}
