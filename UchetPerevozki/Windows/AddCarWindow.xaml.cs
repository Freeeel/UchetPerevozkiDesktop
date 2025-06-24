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

namespace UchetPerevozki.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddCarWindow.xaml
    /// </summary>
    public partial class AddCarWindow : Window
    {
        public AddCarWindow()
        {
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения полей
            if (string.IsNullOrWhiteSpace(StampTextBox.Text) ||
                string.IsNullOrWhiteSpace(ModelTextBox.Text) ||
                string.IsNullOrWhiteSpace(StateNumberTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Создаем объект для отправки
                var carData = new
                {
                    state_number = StateNumberTextBox.Text,
                    model = ModelTextBox.Text,
                    stamp = StampTextBox.Text
                };

                // Отправляем данные на сервер
                using (var client = new HttpClient())
                {
                    string baseAddress = File.ReadAllText("ipAddress.txt").Trim();
                    client.BaseAddress = new Uri(baseAddress);

                    var content = new StringContent(
                        JsonConvert.SerializeObject(carData),
                        System.Text.Encoding.UTF8,
                        "application/json");

                    var response = await client.PostAsync("/car_parks/", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Транспортное средство успешно добавлено!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка при добавлении ТС: {errorContent}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

