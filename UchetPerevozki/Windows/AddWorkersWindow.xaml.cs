using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UchetPerevozki.Response;
using System.Text.Json;

namespace UchetPerevozki.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddWorkersWindow.xaml
    /// </summary>
    public partial class AddWorkersWindow : Window
    {
        public AddWorkersWindow()
        {
            InitializeComponent();
            LoadCarData();
        }
        private async Task LoadCarData()
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);
                try
                {
                    var response = await client.GetAsync($"/car_parks");
                    response.EnsureSuccessStatusCode();
                    var responseData = await response.Content.ReadAsStringAsync();
                    // Используем System.Text.Json для десериализации
                    List<CarResponse> carResponses = JsonSerializer.Deserialize<List<CarResponse>>(responseData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Учитываем регистр свойств при десериализации
                    });
                    // Заполняем ComboBox данными
                    foreach (var car in carResponses)
                    {
                        string displayText = $"{car.model} - {car.stamp} - {car.state_number}";
                        CarBrandTextBox.Items.Add(displayText); //CarBrandTextBox - имя вашего ComboBox
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Ошибка при получении списка машин: {ex.Message}");
                }
                // Теперь используем System.Text.Json.JsonException
                catch (System.Text.Json.JsonException ex)
                {
                    MessageBox.Show($"Ошибка при десериализации JSON: {ex.Message}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }
        private void DateOfBirthDatePicker_CalendarClosed(object sender, RoutedEventArgs e)
        {
            if (DateOfBirthDatePicker.SelectedDate.HasValue)
            {
                DateOfBirthDatePicker.Text = DateOfBirthDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
            }
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем значения из полей
            string surname = SurnameTextBox.Text;
            string name = NameTextBox.Text;
            string patronymic = PatronymicTextBox.Text;
            string dateOfBirth = DateOfBirthDatePicker.SelectedDate?.ToString("yyyy-MM-dd"); // Получаем дату из DatePicker
            string phone = PhoneTextBox.Text;
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Text;
            string addressResidential = AddressResidentialTextBox.Text;
            string bankAccountNumberText = BankAccountNumberTextBox.Text;
            string selectedCar = CarBrandTextBox.SelectedItem?.ToString(); // Получаем выбранный автомобиль
            // Проверка на заполненность полей
            if (string.IsNullOrEmpty(surname) || string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(patronymic) || string.IsNullOrEmpty(dateOfBirth) ||
                string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(addressResidential) ||
                string.IsNullOrEmpty(bankAccountNumberText) || string.IsNullOrEmpty(selectedCar))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Прерываем выполнение метода, если поля не заполнены
            }
            // Проверка на корректность введенного номера банковского счета
            if (!int.TryParse(bankAccountNumberText, out int bank_account_number))
            {
                MessageBox.Show("Пожалуйста, введите корректный номер банковского счета!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Прерываем выполнение метода, если номер банковского счета некорректен
            }
            // Получаем выбранный автомобиль из ComboBox
            if (CarBrandTextBox.SelectedItem != null)
            {
                // Предполагаем, что displayText в ComboBox имеет формат "Model - Stamp - State_number"
                string[] carParts = selectedCar.Split(new string[] { " - " }, StringSplitOptions.None);
                if (carParts.Length == 3)
                {
                    string model = carParts[0];
                    string stamp = carParts[1];
                    string stateNumber = carParts[2];
                    // Находим car_id по model, stamp и stateNumber (нужно будет запросить API для этого)
                    int carId = await GetCarIdFromAPI(model, stamp, stateNumber);
                    if (carId != 0)
                    {
                        // Создаем объект с данными пользователя
                        var userData = new
                        {
                            surname,
                            name,
                            patronymic,
                            date_of_birthday = dateOfBirth,
                            phone,
                            login,
                            password,
                            address_residential = addressResidential,
                            bank_account_number = bank_account_number,
                            car_id = carId
                        };
                        // Используем System.Text.Json для сериализации
                        Console.WriteLine($"Данные для API: {JsonSerializer.Serialize(userData)}");
                        // Вызываем API для добавления нового работника
                        bool success = await AddWorkerAsync(userData);
                        if (success)
                        {
                            MessageBox.Show("Работник успешно добавлен!");
                            this.Close(); // Закрываем окно после успешного добавления
                        }
                        else
                        {
                            MessageBox.Show("Не удалось добавить работника.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти car_id для выбранного автомобиля.");
                    }
                }
                else
                {
                    MessageBox.Show("Неверный формат данных автомобиля в ComboBox.");
                }
            }
            else
            {
                MessageBox.Show("Выберите автомобиль из ComboBox.");
            }
        }
        private async Task<bool> AddWorkerAsync(object userData)
        {
            string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);
                // Используем System.Text.Json для сериализации
                string json = JsonSerializer.Serialize(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                // Отправляем POST-запрос
                HttpResponseMessage response = await client.PostAsync("/users/", content);
                // Проверяем статус ответа
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    // Логируем ошибку для отладки
                    Console.WriteLine($"Ошибка при добавлении работника: {response.StatusCode}");
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Содержимое ошибки: {errorContent}"); // Выводим содержимое ошибки
                    return false;
                }
            }
        }
        // Метод для получения car_id из API
        private async Task<int> GetCarIdFromAPI(string model, string stamp, string stateNumber)
        {
            string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);
                try
                {
                    // Логируем параметры, которые передаются в API
                    Console.WriteLine($"API Request: /car_parks?model={model}&stamp={stamp}&state_number={stateNumber}");
                    // Запрос к API для получения car_id по model, stamp и stateNumber
                    // Возможно, вам нужно URL-кодировать параметры модели, марки и номера
                    string apiUrl = $"/car_parks?model={Uri.EscapeDataString(model)}&stamp={Uri.EscapeDataString(stamp)}&state_number={Uri.EscapeDataString(stateNumber)}";
                    var response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();
                    string responseData = await response.Content.ReadAsStringAsync();
                    // Логируем ответ от API для отладки
                    Console.WriteLine($"API Response: {responseData}");
                    // Проверяем, что ответ не пустой
                    if (string.IsNullOrEmpty(responseData))
                    {
                        Console.WriteLine("API вернул пустой ответ.");
                        return 0;
                    }
                    // Используем System.Text.Json для разбора JSON
                    JsonDocument jsonDocument = JsonDocument.Parse(responseData);
                    JsonElement root = jsonDocument.RootElement;
                    // Если API возвращает массив объектов
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
                    // Если API возвращает один объект (в зависимости от реализации вашего API)
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
                    Console.WriteLine("Не удалось найти car_id для выбранного автомобиля.");
                    return 0;
                }
                // Теперь используем System.Text.Json.JsonException
                catch (System.Text.Json.JsonException ex)
                {
                    Console.WriteLine($"Ошибка при разборе JSON: {ex.Message}");
                    return 0;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Ошибка при получении car_id: {ex.Message}");
                    return 0;
                }
            }
        }
    }
}