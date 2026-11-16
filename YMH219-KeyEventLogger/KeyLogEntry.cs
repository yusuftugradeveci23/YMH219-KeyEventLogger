using System;

namespace YMH219_KeyEventLogger
{
    /// <summary>
    /// Yakalanan her bir klavye olayını (KeyDown, KeyPress, KeyUp) temsil eden veri modeli sınıfı.
    /// </summary>
    public class KeyLogEntry
    {
        public int SequenceNumber { get; set; }
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; }      // KeyDown, KeyPress, KeyUp
        public string KeyName { get; set; }        // Tuş adı (Örn: A, Enter, Space) veya karakter
        public string KeyCode { get; set; }        // Keys enum karşılığı
        public int KeyValue { get; set; }          // Sayısal ASCII / Sanal tuş kodu
        public string Modifiers { get; set; }      // Shift, Ctrl, Alt durumu

        public KeyLogEntry(int sequenceNumber, string eventType, string keyName, string keyCode, int keyValue, string modifiers)
        {
            SequenceNumber = sequenceNumber;
            Timestamp = DateTime.Now;
            EventType = eventType;
            KeyName = keyName;
            KeyCode = keyCode;
            KeyValue = keyValue;
            Modifiers = string.IsNullOrWhiteSpace(modifiers) ? "Yok" : modifiers;
        }

        /// <summary>
        /// Kaydı okunabilir düz metin (.txt) satırına dönüştürür.
        /// </summary>
        public string ToTextLine()
        {
            return $"[{Timestamp:HH:mm:ss.fff}] #{SequenceNumber:D4} | Olay: {EventType,-8} | Tuş: {KeyName,-12} | Kod: {KeyCode,-12} | Değer: {KeyValue,-4} | Değiştiriciler: {Modifiers}";
        }

        /// <summary>
        /// Kaydı CSV formatına uygun satıra dönüştürür.
        /// </summary>
        public string ToCsvLine()
        {
            string safeKeyName = $"\"{KeyName.Replace("\"", "\"\"")}\"";
            string safeModifiers = $"\"{Modifiers.Replace("\"", "\"\"")}\"";
            return $"{SequenceNumber},{Timestamp:yyyy-MM-dd HH:mm:ss.fff},{EventType},{safeKeyName},{KeyCode},{KeyValue},{safeModifiers}";
        }
    }
}