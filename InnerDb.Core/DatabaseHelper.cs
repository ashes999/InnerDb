using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace InnerDb.Core
{
	static class DatabaseHelper
	{
		public static string SantizeForDatabaseName(this string source)
		{
			return source.Replace(" ", "")
				.Replace("!", "").Replace("@", "").Replace("#", "")
				.Replace("{", "").Replace("}", "").Replace("+", "");
		}

		public static string Serialize(this object source)
		{
			var json = JsonConvert.SerializeObject(source);
			string content = string.Format("{0}\n{1}", source.GetType().FullName, json);
			return content;
		}

		public static int GetIdFromFilename(string filename)
		{
			int id = 0;

			filename = filename.Substring(filename.LastIndexOf('\\') + 1);

			if (filename.Contains('_'))
			{
				int start = filename.LastIndexOf('_') + 1;
				int stop = filename.LastIndexOf('-');
				id = int.Parse(filename.Substring(start, stop - start));
			}
			else
			{
				int start = filename.LastIndexOf('\\') + 1;
				int stop = filename.LastIndexOf('-');
				id = int.Parse(filename.Substring(start, stop - start));
			}
			return id;
		}

		public static object Deserialize(string filename)
		{
			string content = File.ReadAllText(filename);

			int pos = content.IndexOf('\n');
			string json = content.Substring(pos).Trim();
			string typeName = content.Substring(0, pos);

			var type = LoadType(typeName);
			var value = JsonConvert.DeserializeObject(json, type);
			return value;
		}

		public static Type LoadType(string typeName)
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
	}
}
