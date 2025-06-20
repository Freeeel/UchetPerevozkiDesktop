using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UchetPerevozki.Windows;

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
                ReportsDataGrid.ItemsSource = reports;
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
                MessageBox.Show($"Ошибка при загрузке данных пользователя: {ex.Message}");
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

        private void RepairsButton_Click(object sender, RoutedEventArgs e)
        {
            RepairWindow repairWindow = new RepairWindow(_userData);
            repairWindow.Show();
            this.Close();
        }

        private void WorkersButton_Click(object sender, RoutedEventArgs e)
        {
            WorkersWindow workersWindow = new WorkersWindow(_userData);
            workersWindow.Show();
            this.Close();
        }

        private void ReportsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ReportsDataGrid.SelectedItem == null)
            {
                return;
            }

            if (ReportsDataGrid.SelectedItem is ReportResponse selectedReport)
            {
                // Создаем новое окно деталей и передаем выбранный отчет
                ReportsDetailsWindow detailsWindow = new ReportsDetailsWindow(selectedReport);
                detailsWindow.ShowDialog(); // ShowDialog() открывает модальное окно
            }

        }
    }
}
