using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ScottPlot;

namespace KR3
{
    public partial class MainWindow : Window
    {
        private readonly Logger _logger;
        private readonly Statistics _statistics;
        private readonly MessageStore _messageStore;
        private readonly HttpClientService _clientService;
        private HttpServer? _server;

        private readonly ObservableCollection<LogEntry> _allEntries = new();

        private readonly DispatcherTimer _uiTimer;


        public MainWindow()
        {
            InitializeComponent();

            _logger = new Logger("logs.txt");
            _statistics = new Statistics();
            _messageStore = new MessageStore();
            _clientService = new HttpClientService(_logger, OnLogEntryReceived);

            InitPlot();

            _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _uiTimer.Tick += (_, _) => UpdateStatsUi();
            _uiTimer.Start();
        }

        private void InitPlot()
        {
            LoadPlotView.Plot.Title("Запросы в единицу времени");
            LoadPlotView.Plot.Axes.DateTimeTicksBottom();
            LoadPlotView.Plot.YLabel("Запросов");
            LoadPlotView.Plot.XLabel("Время");
            LoadPlotView.Plot.Axes.AutoScale();

            var tickGen = new ScottPlot.TickGenerators.NumericAutomatic();
            tickGen.LabelFormatter = v => ((int)Math.Round(v)).ToString();
            LoadPlotView.Plot.Axes.Left.TickGenerator = tickGen;
        }

        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(PortTextBox.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Порт должен быть числом от 1 до 65535",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _server = new HttpServer(port, _logger, _statistics, _messageStore, OnLogEntryReceived);
                _server.Start();

                StatusTextBlock.Text = $"Запущен на порту {port}";
                StatusTextBlock.Foreground = Brushes.Green;
                StartServerButton.IsEnabled = false;
                StopServerButton.IsEnabled = true;
                PortTextBox.IsEnabled = false;
            }
            catch
            {
                _server = null;
            }
        }

        private void StopServerButton_Click(object sender, RoutedEventArgs e)
        {
            _server?.Stop();
            _server = null;

            StatusTextBlock.Text = "Остановлен";
            StatusTextBlock.Foreground = Brushes.Gray;
            StartServerButton.IsEnabled = true;
            StopServerButton.IsEnabled = false;
            PortTextBox.IsEnabled = true;
        }

        private async void SendRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var url = ClientUrlTextBox.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Введите URL", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var method = ((ComboBoxItem)ClientMethodComboBox.SelectedItem).Content.ToString() ?? "GET";
            var body = ClientBodyTextBox.Text;

            SendRequestButton.IsEnabled = false;

            try
            {
                var (statusCode, responseBody) = await _clientService.SendAsync(method, url, body);
                ClientResponseTextBox.Text =
                    $"HTTP {statusCode}\n" +
                    responseBody;
            }
            catch (Exception ex)
            {
                ClientResponseTextBox.Text = $"Ошибка: {ex.Message}";
            }
            finally
            {
                SendRequestButton.IsEnabled = true;
            }
        }

        private void OnLogEntryReceived(LogEntry entry)
        {
            Dispatcher.Invoke(() =>
            {
                _logger.Add(entry);
                _allEntries.Add(entry);
                if (PassesFilter(entry))
                {
                    LogListBox.Items.Add(entry.FormatForDisplay());
                    LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
                }
            });
        }

        private bool PassesFilter(LogEntry e)
        {
            var methodFilter = ((ComboBoxItem)FilterMethodComboBox.SelectedItem).Content.ToString();
            if (methodFilter != "Все" && !e.Method.Equals(methodFilter, StringComparison.OrdinalIgnoreCase))
                return false;

            var statusFilter = ((ComboBoxItem)FilterStatusComboBox.SelectedItem).Content.ToString();
            if (statusFilter != "Все")
            {
                var prefix = statusFilter![0];
                if (e.StatusCode < 100 || e.StatusCode.ToString()[0] != prefix)
                    return false;
            }

            var directionFilter = ((ComboBoxItem)FilterDirectionComboBox.SelectedItem).Content.ToString();
            if (directionFilter == "Входящие" && e.Direction != LogDirection.Incoming) return false;
            if (directionFilter == "Исходящие" && e.Direction != LogDirection.Outgoing) return false;

            return true;
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (LogListBox == null) return;
            RebuildLogList();
        }

        private void RebuildLogList()
        {
            LogListBox.Items.Clear();
            foreach (var entry in _allEntries)
            {
                if (PassesFilter(entry))
                    LogListBox.Items.Add(entry.FormatForDisplay());
            }
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            _allEntries.Clear();
            LogListBox.Items.Clear();
        }

        private void SaveLogs_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"logs_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
                DefaultExt = ".txt",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                var lines = _allEntries.Select(e => e.FormatForFile());
                System.IO.File.WriteAllLines(dialog.FileName, lines);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatsUi()
        {
            StatTotalTextBlock.Text = _statistics.TotalCount.ToString();
            StatGetTextBlock.Text = _statistics.GetCount.ToString();
            StatPostTextBlock.Text = _statistics.PostCount.ToString();
            StatOtherTextBlock.Text = _statistics.OtherCount.ToString();
            StatAvgTextBlock.Text = $"{_statistics.AverageProcessingMs:F1} ms";
            StatUptimeTextBlock.Text = _server == null ? "00:00:00" : _statistics.Uptime.ToString(@"hh\:mm\:ss");
            StatMessagesTextBlock.Text = _messageStore.Count.ToString();

            UpdatePlot();
        }

        private void UpdatePlot()
        {
            List<(DateTime Bucket, int Count)> data = ChartHourRadio.IsChecked == true
                ? _statistics.GetPerHourSeries()
                : _statistics.GetPerMinuteSeries();

            double[] xs = data.Select(d => d.Bucket.ToOADate()).ToArray();
            double[] ys = data.Select(d => (double)d.Count).ToArray();

            LoadPlotView.Plot.Clear();
            LoadPlotView.Plot.Axes.DateTimeTicksBottom();

            if (xs.Length > 0)
            {
                var scatter = LoadPlotView.Plot.Add.Scatter(xs, ys);
                scatter.LineWidth = 2;
                scatter.MarkerSize = 6;
            }

            LoadPlotView.Refresh();
        }

        private void ChartMode_Changed(object sender, RoutedEventArgs e)
        {
            if (LoadPlotView == null) return;
            UpdatePlot();
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            LoadPlotView.Plot.Axes.AutoScale();
            LoadPlotView.Refresh();
        }

        protected override void OnClosed(EventArgs e)
        {
            _uiTimer.Stop();
            _server?.Stop();
            base.OnClosed(e);
        }
    }
}
