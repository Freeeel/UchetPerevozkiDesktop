using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UchetPerevozki
{
    /// <summary>
    /// Логика взаимодействия для HistoryReportsWindow.xaml
    /// </summary>
    public partial class HistoryReportsWindow : Window
    {
        private int _userId; // Это будет ID пользователя
        private UserResponse _userData;
        public HistoryReportsWindow(int userId)
        {
            InitializeComponent();
            _userId = userId; // Сохраняем ID пользователя
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _userData = await GetUserDataAsync(_userId);
                
                userNameTextBlock.Text = $"{_userData.Name} {_userData.Surname}";
                var reports = await GetReportsAsync();

                // Создаем WrapPanel с карточками
                WrapPanel wrapPanel = CreateReportCards(reports);

                // Очищаем ReportsStackPanel и добавляем WrapPanel
                ReportsStackPanel.Children.Clear();
                ReportsStackPanel.Children.Add(wrapPanel);

            }
            
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Ошибка при обращении к API: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                MessageBox.Show($"Ошибка десериализации: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрзке данных пользователя: {ex.Message}");
            }
        }

        private async Task<List<ReportResponse>> GetReportsAsync()
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);
                var response = await client.GetAsync($"/reports/all");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ReportResponse>>(responseData);
                }
                else
                {
                    throw new Exception($"Ошибка при получении отчетов: {response.StatusCode}");
                }
            }
        }

        private async Task<UserResponse> GetUserDataAsync(int userId)
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);

                var response = await client.GetAsync($"/getuser/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UserResponse>(responseData);
                }
                else
                {
                    throw new Exception($"Ошибка при получении данных о пользователе: {response.StatusCode}");
                }
            }
        }

        private WrapPanel CreateReportCards(List<ReportResponse> reports)
        {
            WrapPanel wrapPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Width = 960, // 320 * 3
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            foreach (var report in reports)
            {
                StackPanel card = CreateReportCard(report);
                card.Width = 300;
                card.Height = 254;
                wrapPanel.Children.Add(card);
            }

            return wrapPanel;
        }

        private StackPanel CreateReportCard(ReportResponse report)
        {
            StackPanel card = new StackPanel
            {
                Margin = new Thickness(10),
                Width = 320,
                Height = 254
            };

            Border border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Background = Brushes.White,
                Padding = new Thickness(10)
            };

            StackPanel innerStack = new StackPanel();
            innerStack.Children.Add(new TextBlock { Text = $"Отчёт № {report.Id}", FontSize = 16, FontWeight = FontWeights.Bold });
            innerStack.Children.Add(new TextBlock { Text = report.report_date_time.ToString("dd.MM.yyyy"), FontWeight = FontWeights.Normal });
            innerStack.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = 1, Y2 = 0, Stroke = Brushes.Black, StrokeThickness = 1 });
            innerStack.Children.Add(new TextBlock { Text = $"Откуда: {report.point_departure}", FontWeight = FontWeights.Bold });
            innerStack.Children.Add(new TextBlock { Text = $"Куда: {report.point_destination}", FontWeight = FontWeights.Bold });
            innerStack.Children.Add(new TextBlock { Text = $"Перевозчик: {report.sender}" });
            innerStack.Children.Add(new TextBlock { Text = $"Вид древесины: {report.view_wood}" });
            innerStack.Children.Add(new TextBlock { Text = $"Длина: {report.length_wood} метров" });
            innerStack.Children.Add(new TextBlock { Text = $"Объём: {report.volume_wood}" });
            innerStack.Children.Add(new TextBlock { Text = $"Ассортимент: {report.assortment_wood_type}" });
            innerStack.Children.Add(new TextBlock { Text = $"Сорт: {report.variety_wood_type}" });

            border.Child = innerStack;
            card.Children.Add(border);

            return card;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WorkersWindow workersWindow = new WorkersWindow(_userData);
            workersWindow.Show();
            this.Close();
        }


    }
}
