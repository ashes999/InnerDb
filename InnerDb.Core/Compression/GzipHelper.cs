using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;

namespace InnerDb.Core.Compression
{
	static class GzipHelper
	{
		public static void CompressFiles(string rootDirectory, string archiveName)
		{
			string[] files = Directory.GetFiles(rootDirectory, "*.*", SearchOption.AllDirectories);
			using (var outputStream = new ZipOutputStream(File.Create(archiveName)))
			{
				var crc = new Crc32();
				var archive = new ZipEntry(archiveName);

				foreach (var filename in files)
				{
					var entry = new ZipEntry(filename);
					bool isProcessed = false;

					while (!isProcessed)
					{
						try
						{
							using (var stream = File.OpenRead(filename))
							{
								crc.Reset();
								outputStream.PutNextEntry(entry);

								var buffer = new byte[stream.Length];
								var bytesRead = stream.Read(buffer, 0, (int)stream.Length);
								outputStream.Write(buffer, 0, bytesRead);
								crc.Update(buffer, 0, bytesRead);

								entry.Crc = crc.Value;
								entry.DateTime = File.GetCreationTime(filename);
								entry.Size = stream.Length;
								stream.Close();
							}

							isProcessed = true;
						}
						catch { }
					}
				}
			}
		}

		internal static void DecompressFiles(string archiveName)
		{
			using (var inputStream = new ZipInputStream(File.OpenRead(archiveName)))
			{
				var entry = inputStream.GetNextEntry();
				while (entry != null)
				{
					// Make sure the dir exists first
					string fileName = entry.Name;
					var fileInfo = new FileInfo(fileName);
					fileInfo.Directory.Create();

					using (var outStream = File.Create(fileName))
					{
						var buffer = new byte[inputStream.Length];
						var bytesRead = inputStream.Read(buffer, 0, buffer.Length);
						outStream.Write(buffer, 0, bytesRead);
						outStream.Close();
					}

					entry = inputStream.GetNextEntry();
				}
			}
		}
	}
}
