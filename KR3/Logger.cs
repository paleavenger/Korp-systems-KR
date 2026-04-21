using System;
using System.Collections.ObjectModel;
using System.IO;

namespace KR3
{
    public class Logger
    {
        private readonly object _fileLock = new();
        private readonly string _filePath;

        public ObservableCollection<LogEntry> Entries { get; } = new();

        public event Action<LogEntry>? EntryAdded;

        public Logger(string filePath = "logs.txt")
        {
            _filePath = filePath;
        }

        public void Add(LogEntry entry)
        {
            Entries.Add(entry);

            lock (_fileLock)
            {
                try
                {
                    File.AppendAllText(_filePath, entry.FormatForFile());
                }
                catch { }
            }

            EntryAdded?.Invoke(entry);
        }
    }
}
