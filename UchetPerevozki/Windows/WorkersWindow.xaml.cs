using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        private ObservableCollection<WorkerResponse> _allWorkers;

        public WorkersWindow(UserResponse userData)
        {
            InitializeComponent();
            _userData = userData;
            userNameTextBlock.Text = $"{_userData.Name} {_userData.Surname}";
            LoadData();
            
            
        }

        private async void LoadData()
        {
            try
            {
                var workers = await GetWorkersAsync();
                _allWorkers = new ObservableCollection<WorkerResponse>(workers);
                WorkersDataGrid.ItemsSource = _allWorkers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сети");
            }
        }

        private async Task<List<WorkerResponse>> GetWorkersAsync()
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);
                var response = await client.GetAsync("/users/role/1");
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<WorkerResponse>>(responseData);
                }
                else
                {
                    throw new Exception($"Ошибка при получении работников!");
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                WorkersDataGrid.ItemsSource = _allWorkers;
            }
            else
            {
                var filteredWorkers = _allWorkers.Where(worker =>
                    (worker.surname?.ToLower().Contains(searchText) ?? false) ||
                    (worker.name?.ToLower().Contains(searchText) ?? false) ||
                    (worker.patronymic?.ToLower().Contains(searchText) ?? false) ||
                    (worker.phone?.ToLower().Contains(searchText) ?? false) ||
                    (worker.login?.ToLower().Contains(searchText) ?? false) ||
                    worker.Id.ToString().Contains(searchText) ||
                    worker.date_of_birthday.ToString("dd.MM.yyyy").Contains(searchText)
                ).ToList();

                WorkersDataGrid.ItemsSource = filteredWorkers;
            }
        }

        private async Task DeleteWorkerAsync(int userId)
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);
                var response = await client.DeleteAsync($"/users/{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ошибка сети!");
                }
            }
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
            if ((sender as Button).DataContext is WorkerResponse selectedWorker)
            {
                EditWorkersWindow editWindow = new EditWorkersWindow(selectedWorker);
                editWindow.ShowDialog();
                LoadData();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is WorkerResponse selectedWorker)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить работника: {selectedWorker.surname} {selectedWorker.name}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await DeleteWorkerAsync(selectedWorker.Id);
                        MessageBox.Show("Работник успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении работника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void AddWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            AddWorkersWindow addWorkersWindow = new AddWorkersWindow();
            if (addWorkersWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            CarWindow carWindow = new CarWindow();
            if (carWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

    }
}