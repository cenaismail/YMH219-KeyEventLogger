using System;

namespace YMH219_KeyEventLogger
{
    /// <summary>Tek bir klavye olayı kaydını temsil eder.</summary>
    public class KeyLogEntry
    {
        public DateTime Timestamp { get; }
        public string KeyName { get; }
        public string EventType { get; }   // "KeyDown" veya "KeyUp"

        public KeyLogEntry(string keyName, string eventType)
        {
            Timestamp = DateTime.Now;
            KeyName = keyName;
            EventType = eventType;
        }

        public override string ToString()
            => $"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} | {EventType,-7} | {KeyName}";
    }
}