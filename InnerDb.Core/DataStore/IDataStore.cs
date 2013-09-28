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
        int PutObject(object obj);
        void DeleteDatabase();
    }
}
