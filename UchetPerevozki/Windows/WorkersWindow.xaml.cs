using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UchetPerevozki.Response;
using UchetPerevozki.Windows;



namespace UchetPerevozki
{
    /// <summary>
    /// Логика взаимодействия для WorkersWindow.xaml
    /// </summary>
    public partial class WorkersWindow : Window
    {
        private UserResponse _userData;
        public WorkersWindow(UserResponse userData)
        {
            InitializeComponent();
            LoadData();
            _userData = userData;
            userNameTextBlock.Text = $"{_userData.Name} {_userData.Surname}";
        }

        private async void LoadData()
        {
            try
            {
                var workers = await GetWorkersAsync();
                WorkersDataGrid.ItemsSource = workers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async Task<List<WorkerResponse>> GetWorkersAsync()
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress); // Заменить на отдельный класс
                var response = await client.GetAsync("/users/role/1"); // Получаем работников с role_id 1
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<WorkerResponse>>(responseData);
                }
                else
                {
                    throw new Exception($"Ошибка при получении работников: {response.StatusCode}");
                }
            }
        }

        private async Task<CarResponse> GetCarByWorkerIdAsync(int userId)
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);
                var response = await client.GetAsync($"/user/{userId}/car");
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<CarResponse>(responseData);
                }
                else
                {
                    throw new Exception($"Ошибка при получении машины для пользователя {userId}: {response.StatusCode}");
                }
            }
        }

        private void btnAddWorker(object sender, RoutedEventArgs e)
        {
            AddWorkersWindow addWorkersWindow = new AddWorkersWindow();
            addWorkersWindow.Show();
        }

        private void HistoryReportsButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryReportsWindow historyReportsWindow = new HistoryReportsWindow(_userData.Id);
            historyReportsWindow.Show();
            this.Close();
        }

        private void RepairsButton_Click(object sender, RoutedEventArgs e)
        {
            RepairWindow repairWindow = new RepairWindow(_userData);
            repairWindow.Show();
            this.Close();
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            WorkerResponse selectedWorker = (sender as Button).DataContext as WorkerResponse;
            if (selectedWorker != null)
            {
                // Открыть окно редактирования для selectedWorker
                EditWorkersWindow editWindow = new EditWorkersWindow(selectedWorker);
                editWindow.ShowDialog(); // ShowDialog для блокировки родительского окна
                LoadData(); // Обновить данные после редактирования
            }

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            WorkerResponse selectedWorker = (sender as Button).DataContext as WorkerResponse;
            if (selectedWorker != null)
            {
                // Логика удаления работника, например, запрос подтверждения
                MessageBox.Show($"Удалить работника: {selectedWorker.surname}");
                // Вызвать метод API для удаления, затем обновить DataGrid
            }
        }

        private void AddWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            AddWorkersWindow addWorkersWindow = new AddWorkersWindow();
            addWorkersWindow.ShowDialog();
        }
    }
}
