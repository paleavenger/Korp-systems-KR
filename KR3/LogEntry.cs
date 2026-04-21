using System;
using System.Collections.Generic;
using System.Text;

namespace KR3
{
    public enum LogDirection
    {
        Incoming,
        Outgoing
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public LogDirection Direction { get; set; }
        public string Method { get; set; } = "";
        public string Url { get; set; } = "";
        public Dictionary<string, string> Headers { get; set; } = new();
        public string Body { get; set; } = "";
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }

        public string FormatForDisplay()
        {
            var sb = new StringBuilder();
            sb.Append('[').Append(Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")).Append("] ");
            sb.Append(Direction == LogDirection.Incoming ? "IN  " : "OUT ");
            sb.Append(Method.PadRight(5)).Append(' ');
            sb.Append(Url).Append(' ');
            sb.Append("-> ").Append(StatusCode);
            sb.Append(" (").Append(DurationMs).Append(" ms)");
            return sb.ToString();
        }

        public string FormatForFile()
        {
            var sb = new StringBuilder();
            sb.AppendLine(FormatForDisplay());
            if (Headers.Count > 0)
            {
                sb.AppendLine("  Headers:");
                foreach (var kv in Headers)
                    sb.Append("    ").Append(kv.Key).Append(": ").AppendLine(kv.Value);
            }
            if (!string.IsNullOrEmpty(Body))
            {
                sb.AppendLine("  Body:");
                sb.Append("    ").AppendLine(Body);
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
