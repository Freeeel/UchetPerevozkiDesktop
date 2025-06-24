using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using UchetPerevozki.Response;

namespace UchetPerevozki.Windows
{
    /// <summary>
    /// Логика взаимодействия для CarWindow.xaml
    /// </summary>
    public partial class CarWindow : Window
    {
        private ObservableCollection<CarResponse> _allCars;

        public CarWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string baseAddress = File.ReadAllText("ipAddress.txt").Trim();
                    client.BaseAddress = new Uri(baseAddress);

                    var response = await client.GetAsync("/car_parks/");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var cars = JsonConvert.DeserializeObject<List<CarResponse>>(responseData);
                        _allCars = new ObservableCollection<CarResponse>(cars);
                        CarsDataGrid.ItemsSource = _allCars;
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при загрузке данных", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                CarsDataGrid.ItemsSource = _allCars;
            }
            else
            {
                var filteredCars = _allCars.Where(car =>
                    (car.stamp?.ToLower().Contains(searchText) ?? false) ||
                    (car.model?.ToLower().Contains(searchText) ?? false) ||
                    (car.state_number?.ToLower().Contains(searchText) ?? false) ||
                    car.Id.ToString().Contains(searchText)
                ).ToList();

                CarsDataGrid.ItemsSource = filteredCars;
            }
        }

        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            var addCarWindow = new AddCarWindow();
            if (addCarWindow.ShowDialog() == true)
            {
                LoadData(); // Обновляем данные после добавления
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is CarResponse selectedCar)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить автомобиль {selectedCar.stamp} {selectedCar.model} ({selectedCar.state_number})?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            string baseAddress = File.ReadAllText("ipAddress.txt").Trim();
                            client.BaseAddress = new Uri(baseAddress);

                            var response = await client.DeleteAsync($"/car_parks/{selectedCar.Id}");
                            Console.WriteLine($"asa {selectedCar.Id}");
                            if (response.IsSuccessStatusCode)
                            {
                                _allCars.Remove(selectedCar);
                                MessageBox.Show("Автомобиль успешно удален", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Ошибка при удалении автомобиля", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}