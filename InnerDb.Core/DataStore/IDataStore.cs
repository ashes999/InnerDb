using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnerDb.Core.DataStore
{
    interface IDataStore
    {
        List<T> GetCollection<T>();
        T GetObject<T>(int id);
        void PutObject(object obj, int id);
		void Delete(int id);
        void DeleteDatabase();
    }
}
