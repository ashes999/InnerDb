using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using InnerDb.Core.DataStore;
using Newtonsoft.Json;
using System.IO;

namespace InnerDb.Core.Journal
{
	class FileJournal
	{
		internal string DirectoryPath { get { return this.directoryName; } }

		private uint journalIntervalMilliseconds = 100;
		private IList<JournalEntry> entries = new List<JournalEntry>();
		private string databaseName;
		private Timer queueTimer = new Timer(100);
		private FileDataStore fileStore;
		private string directoryName;
		private bool isRunning = false;
		private string journalDir;
		private static int nextJournalId = 1;
		private static readonly int MaxJournalId = 1000000;

		internal static readonly string PutEntryPrefix = "Put";
		private static readonly string DeleteEntryPrefix = "Delete";

		public FileJournal(string databaseName, FileDataStore fileStore)
		{
			this.databaseName = databaseName;
			this.directoryName = databaseName.SantizeForDatabaseName();

			this.journalDir = string.Format(@"{0}\Journal", directoryName);
			if (!Directory.Exists(journalDir))
			{
				Directory.CreateDirectory(journalDir);
			}

			this.fileStore = fileStore;
			this.LoadUncommittedEntries();

			queueTimer.Elapsed += (sender, eventArgs) =>
			{
				if (this.isRunning)
				{
					this.ProcessRecords();
				}
			};
			queueTimer.Start();

			this.isRunning = true;
		}

		public uint JournalIntervalSeconds
		{
			get
			{
				return this.journalIntervalMilliseconds;
			}
			set
			{
				if (value < 100 || value > 10000)
				{
					throw new ArgumentException("Journal interval must be between 100 and 10000 milliseconds inclusively.");
				}

				this.queueTimer.Stop();
				this.journalIntervalMilliseconds = value;
				this.queueTimer.Interval = this.journalIntervalMilliseconds;
				this.queueTimer.Start();
			}
		}

		public void RecordWrite(object data, int id = 0)
		{
			this.entries.Add(new PutObjectEntry(data, id));
			string fileName = GetPathFor(id, PutEntryPrefix);
			File.WriteAllText(fileName, data.Serialize());
		}

		public void RecordDelete(int id)
		{
			this.entries.Add(new DeleteObjectEntry(id));
			string fileName = GetPathFor(id, DeleteEntryPrefix);
			File.Create(fileName);
		}

		internal void DeleteDatabase()
		{
			this.Stop();
			while (Directory.Exists(this.journalDir))
			{
				try
				{
					Directory.Delete(this.journalDir, true);
				}
				catch
				{
					// Listen, doc, the background thread spins real fast in a test environment
					// where we're adding/deleting tons of stuff every second. It's almost impossible
					// to avoid concurrent file access in that (production-artificial) enviornment.
					// Just play it cool, and delete atomicly.
				}
			}
		}

		internal void Stop()
		{
			this.queueTimer.Stop();
			this.isRunning = false;
		}
		private string GetPathFor(int id, string journalSuffix)
		{
			// Include integer so multiple simultaneous entries are executed in order
			var timestamp = string.Format("J{0}", nextJournalId);
			nextJournalId = nextJournalId + 1 % MaxJournalId;
			return string.Format(@"{0}\Journal\{1}_{2}-{3}.json", this.directoryName, timestamp, id, journalSuffix);
		}

		private void ProcessRecords()
		{
			// No locking. Copying.
			IList<JournalEntry> copy = new List<JournalEntry>(this.entries);
			foreach (var entry in copy)
			{
				this.entries.Remove(entry);
				entry.Execute(this.fileStore);
				string fileName = GetPathFor(entry.Id, entry.Suffix);
				File.Delete(fileName);
			}
		}

		private void LoadUncommittedEntries()
		{			
			// Order by creation date. That's important if we have, say, multiple files for the same ID.
			var dir = new DirectoryInfo(string.Format(@"{0}\Journal", this.directoryName));
			var files = dir.GetFiles().OrderBy(f => f.Name); // Timestamp isn't granular enough for tests.
			foreach (var file in files) {
				
				int id = 0;
				string filename = file.FullName;

				if (filename.Contains('-'))
				{
					id = DatabaseHelper.GetIdFromFilename(filename);
				}

				// Add entries, instead of executing, to avoid contention on files
				// eg. as we're deleting this record, it's being executed.
				if (filename.EndsWith(string.Format("-{0}.json", PutEntryPrefix)))
				{
					var data = DatabaseHelper.Deserialize(filename);
					new PutObjectEntry(data, id).Execute(this.fileStore);
				}
				else if (filename.EndsWith(string.Format("-{0}.json", DeleteEntryPrefix)))
				{
					new DeleteObjectEntry(id).Execute(fileStore);
				}
				else
				{
					throw new InvalidOperationException("Unexpected journal file: " + filename);
				}
			}
		}

		private abstract class JournalEntry
		{
			public int Id { get; protected set; }
			public string Suffix { get; protected set; }
			public abstract void Execute(FileDataStore fileStore);
		}

		private class PutObjectEntry : JournalEntry
		{
			public object Data { get; private set; }

			public PutObjectEntry(object data, int id = 0)
			{
				this.Id = id;
				this.Data = data;
				this.Suffix = PutEntryPrefix;
			}

			public override void Execute(FileDataStore fileStore)
			{
				fileStore.PutObject(this.Data, this.Id);
			}
		}

		private class DeleteObjectEntry : JournalEntry
		{
			public DeleteObjectEntry(int id)
			{
				this.Id = id;
				this.Suffix = DeleteEntryPrefix;
			}

			public override void Execute(FileDataStore fileStore)
			{
				fileStore.Delete(this.Id);
			}
		}
	}
}
