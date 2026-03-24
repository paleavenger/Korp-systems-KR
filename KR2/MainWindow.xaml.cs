using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KR2
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<string> _history = new();

        public MainWindow()
        {
            InitializeComponent();
            HistoryListBox.ItemsSource = _history;
            LoadInterfaces();
        }
        private void LoadInterfaces()
        {
            InterfaceListBox.Items.Clear();
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                InterfaceListBox.Items.Add(nic.Name);
        }

        private void RefreshInterfaces_Click(object sender, RoutedEventArgs e) => LoadInterfaces();

        private void InterfaceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var name = InterfaceListBox.SelectedItem as string;
            if (name == null) return;

            var nic = NetworkInterface.GetAllNetworkInterfaces()
                          .FirstOrDefault(n => n.Name == name);
            if (nic == null) return;

            var sb = new StringBuilder();
            var props = nic.GetIPProperties();

            bool hasIp = false;
            foreach (var uni in props.UnicastAddresses)
            {
                if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    sb.AppendLine($"IP: {uni.Address}");
                    sb.AppendLine($"Маска: {uni.IPv4Mask}");
                    hasIp = true;
                }
                else if (uni.Address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    sb.AppendLine($"IPv6: {uni.Address}");
                }
            }
            if (!hasIp) sb.AppendLine("IP: нет");

            var mac = nic.GetPhysicalAddress().ToString();
            if (mac.Length == 12)
                mac = string.Join(":", Enumerable.Range(0, 6).Select(i => mac.Substring(i * 2, 2)));
            sb.AppendLine($"MAC: {(string.IsNullOrEmpty(mac) ? "нет" : mac)}");

            sb.AppendLine($"Статус: {nic.OperationalStatus}");
            sb.AppendLine($"Тип: {nic.NetworkInterfaceType}");
            sb.AppendLine($"Скорость: {FormatSpeed(nic.Speed)}");

            foreach (var gw in props.GatewayAddresses.Take(2))
                sb.AppendLine($"Шлюз: {gw.Address}");

            foreach (var dns in props.DnsAddresses.Take(2))
                sb.AppendLine($"DNS: {dns}");

            InterfaceInfoText.Text = sb.ToString();
        }

        private static string FormatSpeed(long bps)
        {
            if (bps <= 0) return "н/д";
            if (bps >= 1_000_000_000) return $"{bps / 1_000_000_000.0:F1} Гбит/с";
            if (bps >= 1_000_000)     return $"{bps / 1_000_000.0:F0} Мбит/с";
            if (bps >= 1_000)         return $"{bps / 1_000.0:F0} Кбит/с";
            return $"{bps} бит/с";
        }
        private void UrlInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) AnalyzeUrl_Click(sender, e);
        }

        private void AnalyzeUrl_Click(object sender, RoutedEventArgs e)
        {
            var raw = UrlInputBox.Text.Trim();
            if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
            {
                MessageBox.Show("Неверный формат URL.", "Ошибка");
                return;
            }

            TbScheme.Text   = uri.Scheme;
            TbHost.Text     = uri.Host;
            TbPort.Text     = uri.IsDefaultPort ? $"{uri.Port} (по умолч.)" : uri.Port.ToString();
            TbPath.Text     = uri.AbsolutePath;
            TbQuery.Text    = string.IsNullOrEmpty(uri.Query) ? "—" : uri.Query;
            TbFragment.Text = string.IsNullOrEmpty(uri.Fragment) ? "—" : uri.Fragment;
            TbAddrType.Text = GetAddressType(uri.Host);

            AddHistory(raw);
            AppendDiagnostics($"[URL] Разобран: {raw}");
        }
        private async void PingHost_Click(object sender, RoutedEventArgs e)
        {
            var host = GetHost();
            if (host == null) return;

            AppendDiagnostics($"[PING] → {host}");

            try
            {
                using var ping = new Ping();
                for (int i = 0; i < 4; i++)
                {
                    var r = await ping.SendPingAsync(host, 2000);
                    AppendDiagnostics(r.Status == IPStatus.Success
                        ? $"  [{i+1}] {r.Address}  {r.RoundtripTime} мс  TTL={r.Options?.Ttl}"
                        : $"  [{i+1}] {r.Status}");
                    await Task.Delay(200);
                }
            }
            catch (Exception ex)
            {
                AppendDiagnostics($"  Ошибка: {ex.Message}");
            }
        }
        private async void DnsLookup_Click(object sender, RoutedEventArgs e)
        {
            var host = GetHost();
            if (host == null) return;

            AppendDiagnostics($"[DNS] → {host}");
            try
            {
                var entry = await Dns.GetHostEntryAsync(host);
                AppendDiagnostics($"  Имя: {entry.HostName}");
                foreach (var addr in entry.AddressList)
                    AppendDiagnostics($"  Адрес: {addr} ({addr.AddressFamily})");
                foreach (var alias in entry.Aliases)
                    AppendDiagnostics($"  Псевдоним: {alias}");
            }
            catch (Exception ex)
            {
                AppendDiagnostics($"  Ошибка: {ex.Message}");
            }
        }
        private string? GetHost()
        {
            var raw = UrlInputBox.Text.Trim();
            if (string.IsNullOrEmpty(raw)) { MessageBox.Show("Введите URL."); return null; }
            return Uri.TryCreate(raw, UriKind.Absolute, out var uri) ? uri.Host : raw;
        }

        private static string GetAddressType(string host)
        {
            if (host == "localhost" || host == "127.0.0.1" || host == "::1")
                return "Loopback";
            if (IPAddress.TryParse(host, out var ip))
            {
                if (IPAddress.IsLoopback(ip)) return "Loopback";
                var b = ip.GetAddressBytes();
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (b[0] == 10) return "Локальный (Class A)";
                    if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return "Локальный (Class B)";
                    if (b[0] == 192 && b[1] == 168) return "Локальный (Class C)";
                    if (b[0] == 169 && b[1] == 254) return "Link-local";
                }
                return "Публичный IP";
            }
            return "Публичный хост";
        }

        private void AppendDiagnostics(string text)
        {
            DiagnosticsOutput.Text += text + "\n";
            DiagnosticsOutput.ScrollToEnd();
        }

        private void AddHistory(string url)
        {
            _history.Remove(url);
            _history.Insert(0, url);
            if (_history.Count > 20) _history.RemoveAt(_history.Count - 1);
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e) => _history.Clear();

        private void HistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryListBox.SelectedItem is string url)
            {
                UrlInputBox.Text = url;
                HistoryListBox.SelectedItem = null;
            }
        }
    }
}