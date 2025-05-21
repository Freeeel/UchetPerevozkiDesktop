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
    /// Логика взаимодействия для AddWorkersWindow.xaml
    /// </summary>
    public partial class AddWorkersWindow : Window
    {
        public AddWorkersWindow()
        {
            InitializeComponent();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            
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
                bank_account_number = 0 //
            };
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
        private async Task<bool> AddWorkerAsync(object userData)
        {
            string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);

                // Сериализуем данные в JSON
                string json = JsonConvert.SerializeObject(userData);
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
    }
}