using System.Text.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UchetPerevozki.Response;
using Newtonsoft.Json.Serialization;

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
                    var response = await client.GetAsync($"/car_parks"); // Получаем список машин
                    response.EnsureSuccessStatusCode(); // Выбрасываем исключение в случае неуспешного 
                    var responseData = await response.Content.ReadAsStringAsync();
                    List<CarResponse> carResponses = JsonSerializer.Deserialize<List<CarResponse>>(responseData);
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
                catch (JsonException ex)
                {
                    MessageBox.Show($"Ошибка при десериализации JSON: {ex.Message}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                }
            }
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранный автомобиль из ComboBox
            if (CarBrandTextBox.SelectedItem != null)
            {
                string selectedCar = CarBrandTextBox.SelectedItem.ToString();
                Console.WriteLine($"Combo: {selectedCar}");
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
                            surname = SurnameTextBox.Text,
                            name = NameTextBox.Text,
                            patronymic = PatronymicTextBox.Text,
                            date_of_birthday = DateOfBirthTextBox.Text,
                            phone = PhoneTextBox.Text,
                            login = LoginTextBox.Text,
                            password = PasswordTextBox.Text,
                            address_residential = "unknown",  // или любое другое значение по умолчанию
                            bank_account_number = 0, //
                            car_id = carId
                        };
                        Console.WriteLine($"Данные для API: {JsonSerializer.Serialize(userData)}");
                        // Вызываем API для добавления нового работника
                        bool success = await AddWorkerAsync(userData);
                        if (success)
                        {
                            MessageBox.Show("Работник успешно добавлен!");
                            DialogResult = true;
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
                // Сериализуем данные в JSON
                string json = JsonSerializer.Serialize(userData); // Используем JsonSerializer.Serialize
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
        // Добавляем метод для получения car_id из API
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
                    string apiUrl = $"/car_parks?model={model}&stamp={stamp}&state_number={stateNumber}";
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

                    //  Предполагаем, что API возвращает JSON с полем "id"
                    JsonDocument jsonDocument = JsonDocument.Parse(responseData);
                    JsonElement root = jsonDocument.RootElement;

                    //  Если API возвращает массив объектов
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

                    Console.WriteLine("Не удалось найти car_id для выбранного автомобиля.");
                    return 0;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Ошибка при получении car_id: {ex.Message}");
                    return 0;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"Ошибка при разборе JSON: {ex.Message}");
                    return 0;
                }
            }
        }

    }
}