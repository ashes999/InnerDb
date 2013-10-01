using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InnerDb.Core.Index
{
	public class IndexStore
	{
		// indexes[Type] = indexes!
		private IDictionary<Type, IndexData<object>> indexes = new Dictionary<Type, IndexData<object>>();

		//public bool HasIndex<T>(string fieldName)
		//{
		//    var type = typeof(T);
		//    return this.indexes[type] != null && this.indexes[type].IndexesField(fieldName);
		//}

		public void AddIndex(Type type, string fieldName)
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
	}
}
