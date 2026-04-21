using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KR3
{
    public class HttpServer
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly int _port;
        private readonly Logger _logger;
        private readonly Statistics _statistics;
        private readonly MessageStore _messageStore;

        private HttpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _listenerTask;

        public event Action? RequestProcessed;

        public Statistics Stats => _statistics;
        public MessageStore Store => _messageStore;

        private readonly Action<LogEntry> _uiLogCallback;

        public HttpServer(int port, Logger logger, Statistics stats, MessageStore store, Action<LogEntry> uiLogCallback)
        {
            _port = port;
            _logger = logger;
            _statistics = stats;
            _messageStore = store;
            _uiLogCallback = uiLogCallback;
        }

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{_port}/");
            _listener.Start();

            _cts = new CancellationTokenSource();
            _listenerTask = Task.Run(() => ListenLoop(_cts.Token));
        }

        public void Stop()
        {
            try
            {
                _cts?.Cancel();
                _listener?.Stop();
                _listener?.Close();
            }
            catch { }
        }

        private async Task ListenLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _listener != null && _listener.IsListening)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (InvalidOperationException) { break; }

                _ = Task.Run(() => HandleRequestSafe(context));
            }
        }

        private async Task HandleRequestSafe(HttpListenerContext context)
        {
            var sw = Stopwatch.StartNew();
            var entry = new LogEntry
            {
                Direction = LogDirection.Incoming,
                Method = context.Request.HttpMethod,
                Url = context.Request.Url?.ToString() ?? ""
            };

            foreach (string? key in context.Request.Headers.AllKeys)
            {
                if (key != null)
                    entry.Headers[key] = context.Request.Headers[key] ?? "";
            }

            string body = "";
            if (context.Request.HasEntityBody)
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                body = await reader.ReadToEndAsync();
            }
            entry.Body = body;

            try
            {
                await RouteRequest(context, body);
                entry.StatusCode = context.Response.StatusCode;
            }
            catch (Exception ex)
            {
                entry.StatusCode = 500;
                await WriteResponse(context, 500, "text/plain", $"Internal error: {ex.Message}");
            }
            finally
            {
                sw.Stop();
                entry.DurationMs = sw.ElapsedMilliseconds;
                _statistics.RegisterRequest(entry.Method, entry.DurationMs);

                _uiLogCallback(entry);
                RequestProcessed?.Invoke();

                try { context.Response.OutputStream.Close(); } catch { }
            }
        }

        private async Task RouteRequest(HttpListenerContext context, string body)
        {
            var method = context.Request.HttpMethod.ToUpperInvariant();
            var path = context.Request.Url?.AbsolutePath ?? "/";

            if (method == "GET" && path == "/messages")
                await HandleGetMessages(context);
            else if (method == "GET" && path == "/")
                await HandleGet(context);
            else if (method == "GET" && path == "/error")
                throw new Exception("Internal Server Error");
            else if (method == "POST" && path == "/messages")
                await HandlePost(context, body);
            else
                await WriteResponse(context, 404, "text/plain", "Not Found");
        }

        private async Task HandleGet(HttpListenerContext context)
        {
            var payload = new
            {
                status = "running",
                getCount = _statistics.GetCount,
                postCount = _statistics.PostCount,
                uptime = _statistics.Uptime.ToString(@"hh\:mm\:ss"),
                serverTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            var json = JsonSerializer.Serialize(payload, _jsonOptions);
            await WriteResponse(context, 200, "application/json", json);
        }

        private async Task HandleGetMessages(HttpListenerContext context)
        {
            var idParam = context.Request.QueryString["id"];

            if (!string.IsNullOrEmpty(idParam))
            {
                var msg = _messageStore.GetById(idParam);
                if (msg == null)
                {
                    await WriteResponse(context, 404, "application/json", "{\"error\":\"Message not found\"}");
                    return;
                }

                var single = new { id = msg.Id, message = msg.Text, receivedAt = msg.ReceivedAt.ToString("yyyy-MM-dd HH:mm:ss") };
                var singleJson = JsonSerializer.Serialize(single, _jsonOptions);
                await WriteResponse(context, 200, "application/json", singleJson);
                return;
            }

            var messages = _messageStore.All.Select(m => new
            {
                id = m.Id,
                message = m.Text,
                receivedAt = m.ReceivedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });

            var json = JsonSerializer.Serialize(messages, _jsonOptions);
            await WriteResponse(context, 200, "application/json", json);
        }

        private async Task HandlePost(HttpListenerContext context, string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                await WriteResponse(context, 400, "application/json", "{\"error\":\"Empty body\"}");
                return;
            }

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("message", out var messageElement))
                {
                    await WriteResponse(context, 400, "application/json",
                        "{\"error\":\"Field 'message' is required\"}");
                    return;
                }

                var messageText = messageElement.GetString() ?? "";
                string? customId = null;
                if (doc.RootElement.TryGetProperty("id", out var idElement))
                    customId = idElement.GetString();
                var id = _messageStore.AddMessage(messageText, customId);

                var response = JsonSerializer.Serialize(new { id, status = "stored", message = messageText }, _jsonOptions);
                await WriteResponse(context, 200, "application/json", response);
            }
            catch (JsonException ex)
            {
                await WriteResponse(context, 400, "application/json",
                    $"{{\"error\":\"Invalid JSON: {ex.Message}\"}}");
            }
        }

        private static async Task WriteResponse(HttpListenerContext context, int statusCode, string contentType, string body)
        {
            var buffer = Encoding.UTF8.GetBytes(body);
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = $"{contentType}; charset=utf-8";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
