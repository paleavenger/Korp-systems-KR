using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace KR3
{
    public class StoredMessage
    {
        public string Id { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime ReceivedAt { get; set; }
    }

    public class MessageStore
    {
        private readonly ConcurrentDictionary<string, StoredMessage> _messages = new();

        public string AddMessage(string text, string? customId = null)
        {
            var id = string.IsNullOrWhiteSpace(customId) ? Guid.NewGuid().ToString() : customId;
            _messages[id] = new StoredMessage
            {
                Id = id,
                Text = text,
                ReceivedAt = DateTime.Now
            };
            return id;
        }

        public StoredMessage? GetById(string id) =>
            _messages.TryGetValue(id, out var msg) ? msg : null;

        public int Count => _messages.Count;

        public IReadOnlyCollection<StoredMessage> All => _messages.Values.ToArray();
    }
}
