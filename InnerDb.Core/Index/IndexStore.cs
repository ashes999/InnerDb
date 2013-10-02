using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace InnerDb.Core.Index
{
	public class IndexStore
	{
		// indexes[Type] = indexes!
		private IDictionary<Type, IndexData<object>> indexes = new Dictionary<Type, IndexData<object>>();
		private string directoryName;

		public IndexStore(string databaseName)
		{
			this.directoryName = databaseName.SantizeForDatabaseName();
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
	}
}
