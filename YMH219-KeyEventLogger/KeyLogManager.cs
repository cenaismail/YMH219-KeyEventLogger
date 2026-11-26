using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace YMH219_KeyEventLogger
{
    /// <summary>Kayıt durumunu ve kayıt listesini yönetir (arayüzden bağımsız).</summary>
    public class KeyLogManager
    {
        private readonly List<KeyLogEntry> _entries = new List<KeyLogEntry>();

        public bool IsRecording { get; private set; }
        public int Count => _entries.Count;
        public IReadOnlyList<KeyLogEntry> Entries => _entries;

        /// <summary>Yeni kayıt eklendiğinde tetiklenir.</summary>
        public event Action<KeyLogEntry> EntryAdded;

        public void Start() => IsRecording = true;
        public void Stop() => IsRecording = false;

        public void Add(string keyName, string eventType)
        {
            if (!IsRecording) return;

            var entry = new KeyLogEntry(keyName, eventType);
            _entries.Add(entry);
            EntryAdded?.Invoke(entry);
        }

        public void Clear() => _entries.Clear();

        /// <summary>Kayıtları yerel .txt dosyasına yazar.</summary>
        public void SaveToFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Dosya yolu boş olamaz.");

            var lines = new List<string>();
            foreach (var e in _entries) lines.Add(e.ToString());

            File.WriteAllLines(path, lines, Encoding.UTF8);
        }
    }
}