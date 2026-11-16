using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YMH219_KeyEventLogger
{
    /// <summary>
    /// Klavye olay kayıtlarını bellek üzerinde yöneten, istatistik üreten ve dosyaya aktaran sınıf.
    /// </summary>
    public class KeyLogManager
    {
        private readonly List<KeyLogEntry> _entries;

        public bool IsRecording { get; set; } = true;

        public IReadOnlyList<KeyLogEntry> Entries => _entries.AsReadOnly();

        public int TotalCount => _entries.Count;
        public int KeyDownCount => _entries.Count(e => e.EventType == "KeyDown");
        public int KeyPressCount => _entries.Count(e => e.EventType == "KeyPress");
        public int KeyUpCount => _entries.Count(e => e.EventType == "KeyUp");

        public KeyLogManager()
        {
            _entries = new List<KeyLogEntry>();
        }

        /// <summary>
        /// Kayıt aktifse yeni bir tuş olayını listeye ekler.
        /// </summary>
        public KeyLogEntry? AddEntry(string eventType, string keyName, string keyCode, int keyValue, string modifiers)
        {
            if (!IsRecording)
                return null;

            int nextSequence = _entries.Count + 1;
            var entry = new KeyLogEntry(nextSequence, eventType, keyName, keyCode, keyValue, modifiers);
            _entries.Add(entry);
            return entry;
        }

        /// <summary>
        /// Tüm kayıt geçmişini temizler.
        /// </summary>
        public void Clear()
        {
            _entries.Clear();
        }

        /// <summary>
        /// Kayıtları düz metin (.txt) dosyası olarak dışa aktarır.
        /// </summary>
        public void ExportToTxt(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== YMH 219 - KEY EVENT LOGGER KAYIT RAPORU ===");
            sb.AppendLine($"Oluşturulma Tarihi: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Toplam Olay: {TotalCount} (KeyDown: {KeyDownCount}, KeyPress: {KeyPressCount}, KeyUp: {KeyUpCount})");
            sb.AppendLine(new string('-', 95));

            foreach (var entry in _entries)
            {
                sb.AppendLine(entry.ToTextLine());
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Kayıtları CSV (.csv) tablo dosyası olarak dışa aktarır.
        /// </summary>
        public void ExportToCsv(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SiraNo,ZamanDamgasi,OlayTuru,TusAdi,KeyCode,KeyValue,Degistiriciler");

            foreach (var entry in _entries)
            {
                sb.AppendLine(entry.ToCsvLine());
            }

            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }
    }
}