using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace KR3
{
    public class Statistics
    {
        private readonly object _lock = new();
        private int _getCount;
        private int _postCount;
        private int _otherCount;
        private long _totalProcessingMs;
        private int _processedRequests;

        public DateTime StartTime { get; } = DateTime.Now;

        public int GetCount { get { lock (_lock) return _getCount; } }
        public int PostCount { get { lock (_lock) return _postCount; } }
        public int OtherCount { get { lock (_lock) return _otherCount; } }
        public int TotalCount => GetCount + PostCount + OtherCount;

        public double AverageProcessingMs
        {
            get
            {
                lock (_lock)
                {
                    if (_processedRequests == 0) return 0;
                    return (double)_totalProcessingMs / _processedRequests;
                }
            }
        }

        public TimeSpan Uptime => DateTime.Now - StartTime;

        private readonly ConcurrentBag<DateTime> _requestTimestamps = new();

        public void RegisterRequest(string method, long processingMs)
        {
            lock (_lock)
            {
                if (method.Equals("GET", StringComparison.OrdinalIgnoreCase)) _getCount++;
                else if (method.Equals("POST", StringComparison.OrdinalIgnoreCase)) _postCount++;
                else _otherCount++;

                _totalProcessingMs += processingMs;
                _processedRequests++;
            }

            _requestTimestamps.Add(DateTime.Now);
        }

        public List<(DateTime Bucket, int Count)> GetPerMinuteSeries()
        {
            var snapshot = _requestTimestamps.ToList();
            var now = DateTime.Now;
            var first = snapshot.Count > 0 ? snapshot.Min() : now;

            var bucketStart = new DateTime(first.Year, first.Month, first.Day, first.Hour, first.Minute, 0);
            var bucketEnd = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0).AddMinutes(1);

            var result = new List<(DateTime, int)>();
            for (var b = bucketStart; b < bucketEnd; b = b.AddMinutes(1))
            {
                var end = b.AddMinutes(1);
                result.Add((b, snapshot.Count(t => t >= b && t < end)));
            }
            return result;
        }

        public List<(DateTime Bucket, int Count)> GetPerHourSeries()
        {
            var snapshot = _requestTimestamps.ToList();
            var now = DateTime.Now;
            var first = snapshot.Count > 0 ? snapshot.Min() : now;

            var bucketStart = new DateTime(first.Year, first.Month, first.Day, first.Hour, 0, 0);
            var bucketEnd = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);

            var result = new List<(DateTime, int)>();
            for (var b = bucketStart; b < bucketEnd; b = b.AddHours(1))
            {
                var end = b.AddHours(1);
                result.Add((b, snapshot.Count(t => t >= b && t < end)));
            }
            return result;
        }
    }
}
