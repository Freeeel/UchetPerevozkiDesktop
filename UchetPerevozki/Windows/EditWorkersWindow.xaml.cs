using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

using UchetPerevozki.Response;

namespace UchetPerevozki.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditWorkersWindow.xaml
    /// </summary>
    public partial class EditWorkersWindow : Window
    {
        private readonly int _workerId; // Поле для хранения ID работника
        public EditWorkersWindow(WorkerResponse worker)
        {
            InitializeComponent();
            _workerId = worker.Id;
            // Загружаем общие данные работника
            IdTB.Text = worker.Id.ToString();
            SurnameTB.Text = worker.surname;
            NameTB.Text = worker.name;
            PatronymicTB.Text = worker.patronymic;
            PhoneTB.Text = worker.phone;
            AddressTB.Text = worker.address_residential;
            BankAccountNumberTB.Text = worker.bank_account_number.ToString();
            LoginTB.Text = worker.login;
            PasswordPB.Password = worker.password;
            LoadAndSetCarData(worker.Id);
        }

        private async Task LoadCarData()
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);
                try
                {
                    var response = await client.GetAsync("/car_parks");
                    response.EnsureSuccessStatusCode();
                    var responseData = await response.Content.ReadAsStringAsync();
                    List<CarResponse> carResponses = JsonSerializer.Deserialize<List<CarResponse>>(responseData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    CarBrandTextBox.Items.Clear();
                    foreach (var car in carResponses)
                    {
                        string displayText = $"{car.model} - {car.stamp} - {car.state_number}";
                        CarBrandTextBox.Items.Add(displayText);
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Ошибка при получении списка машин: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    MessageBox.Show($"Ошибка при десериализации JSON машин: {ex.Message}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }

        private async void LoadAndSetCarData(int workerId)
        {
            await LoadCarData();
            try
            {
                CarResponse currentCar = await GetCarByWorkerIdAsync(workerId);
                if (currentCar != null)
                {

                    string currentCarDisplay = $"{currentCar.model} - {currentCar.stamp} - {currentCar.state_number}";
                    CarBrandTextBox.SelectedItem = CarBrandTextBox.Items.Cast<string>().FirstOrDefault(item => item == currentCarDisplay);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке текущего автомобиля работника: {ex.Message}");
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            string name = NameTB.Text;
            string surname = SurnameTB.Text;
            string patronymic = PatronymicTB.Text;
            string phone = PhoneTB.Text;
            string address_residential = AddressTB.Text;
            string login = LoginTB.Text;
            string password = PasswordPB.Password;
            string bankAccountNumberText = BankAccountNumberTB.Text;
            string selectedCar = CarBrandTextBox.SelectedItem?.ToString();
            
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(surname) ||
                string.IsNullOrEmpty(patronymic) || string.IsNullOrEmpty(phone) ||
                string.IsNullOrEmpty(address_residential) || string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(bankAccountNumberText) || string.IsNullOrEmpty(selectedCar))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if (!int.TryParse(bankAccountNumberText, out int bank_account_number))
            {
                MessageBox.Show("Пожалуйста, введите корректный номер банковского счета!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int carId = 0;
            if (CarBrandTextBox.SelectedItem != null)
            {
                string[] carParts = selectedCar.Split(new string[] { " - " }, StringSplitOptions.None);
                if (carParts.Length == 3)
                {
                    string model = carParts[0];
                    string stamp = carParts[1];
                    string stateNumber = carParts[2];
                    carId = await GetCarIdFromAPI(model, stamp, stateNumber);
                    if (carId == 0)
                    {
                        MessageBox.Show("Не удалось найти car_id для выбранного автомобиля.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Неверный формат данных автомобиля в ComboBox.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Выберите автомобиль из ComboBox.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var updatedData = new
            {
                name,
                surname,
                patronymic,
                phone,
                address_residential,
                bank_account_number,
                login,
                password,
                car_id = carId 
            };

            bool success = await UpdateWorkerAsync(_workerId, updatedData);
            if (success)
            {
                MessageBox.Show("Данные работника успешно обновлены!", "Успешно!", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Не удалось обновить данные работника", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> UpdateWorkerAsync(int workerId, object updatedData)
        {
            string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);
                string json = JsonSerializer.Serialize(updatedData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync($"/users/{workerId}", content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private async Task<int> GetCarIdFromAPI(string model, string stamp, string stateNumber)
        {
            string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);
                try
                {
                    string apiUrl = $"/car_parks?model={Uri.EscapeDataString(model)}&stamp={Uri.EscapeDataString(stamp)}&state_number={Uri.EscapeDataString(stateNumber)}";
                    var response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    string responseData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(responseData))
                    {
                        return 0;
                    }
                    JsonDocument jsonDocument = JsonDocument.Parse(responseData);
                    JsonElement root = jsonDocument.RootElement;
                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement element in root.EnumerateArray())
                        {
                            if (element.TryGetProperty("model", out JsonElement modelElement) &&
                                element.TryGetProperty("stamp", out JsonElement stampElement) &&
                                element.TryGetProperty("state_number", out JsonElement stateNumberElement))
                            {
                                if (modelElement.GetString() == model &&
                                    stampElement.GetString() == stamp &&
                                    stateNumberElement.GetString() == stateNumber)
                                {
                                    if (element.TryGetProperty("id", out JsonElement idElement))
                                    {
                                        return idElement.GetInt32();
                                    }
                                }
                            }
                        }
                    }
                    else if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("model", out JsonElement modelElement) &&
                            root.TryGetProperty("stamp", out JsonElement stampElement) &&
                            root.TryGetProperty("state_number", out JsonElement stateNumberElement))
                        {
                            if (modelElement.GetString() == model &&
                                stampElement.GetString() == stamp &&
                                stateNumberElement.GetString() == stateNumber)
                            {
                                if (root.TryGetProperty("id", out JsonElement idElement))
                                {
                                    return idElement.GetInt32();
                                }
                            }
                        }
                    }
                    return 0;
                }
                catch (JsonException) // Используйте System.Text.Json.JsonException
                {
                    return 0;
                }
                catch (HttpRequestException)
                {
                    return 0;
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
                    return System.Text.Json.JsonSerializer.Deserialize<CarResponse>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {

                    throw new Exception($"Ошибка при получении машины для пользователя {userId}: {response.StatusCode}");
                }
            }
        }



        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}