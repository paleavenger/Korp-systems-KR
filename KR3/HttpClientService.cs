using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KR3
{
    public class HttpClientService
    {
        private static readonly HttpClient _client = new();
        private readonly Logger _logger;
        private readonly Action<LogEntry> _uiLogCallback;

        public HttpClientService(Logger logger, Action<LogEntry> uiLogCallback)
        {
            _logger = logger;
            _uiLogCallback = uiLogCallback;
        }

        public async Task<(int StatusCode, string Body)> SendAsync(string method, string url, string? jsonBody)
        {
            var entry = new LogEntry
            {
                Direction = LogDirection.Outgoing,
                Method = method.ToUpperInvariant(),
                Url = url,
                Body = jsonBody ?? ""
            };

            var sw = Stopwatch.StartNew();
            try
            {
                using var request = new HttpRequestMessage(new HttpMethod(method.ToUpperInvariant()), url);

                if (!string.IsNullOrEmpty(jsonBody) && (method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                                                    || method.Equals("PUT", StringComparison.OrdinalIgnoreCase)))
                {
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }

                using var response = await _client.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                sw.Stop();
                entry.StatusCode = (int)response.StatusCode;
                entry.DurationMs = sw.ElapsedMilliseconds;

                foreach (var header in response.Headers)
                {
                    entry.Headers[header.Key] = string.Join(", ", header.Value);
                }

                _uiLogCallback(entry);
                return ((int)response.StatusCode, body);
            }
            catch (Exception ex)
            {
                sw.Stop();
                entry.StatusCode = 0;
                entry.DurationMs = sw.ElapsedMilliseconds;
                _uiLogCallback(entry);
                return (0, "Подключение не установлено");
            }
        }
    }
}
