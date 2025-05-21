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

            _workerId = worker.Id; // Сохраняем ID работника
            IdTB.Text = worker.Id.ToString();
            SurnameTB.Text = worker.surname;
            NameTB.Text = worker.name;
            PatronymicTB.Text = worker.patronymic;
            PhoneTB.Text = worker.phone;
            AddressTB.Text = worker.address_residential;
            BankAccountNumberTB.Text = worker.bank_account_number.ToString();
            LoginTB.Text = worker.login;
            PasswordPB.Password = worker.password;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем обновленные данные из формы
            var updatedData = new
            {
                name = NameTB.Text,
                surname = SurnameTB.Text,
                patronymic = PatronymicTB.Text,
                phone = PhoneTB.Text,
                address_residential = AddressTB.Text,
                bank_account_number = int.Parse(BankAccountNumberTB.Text),
                login = LoginTB.Text,
                password = PasswordPB.Password
            };

            // Вызываем API для обновления данных работника
            bool success = await UpdateWorkerAsync(_workerId, updatedData);

            if (success)
            {
                MessageBox.Show("Данные работника успешно обновлены!");
                DialogResult = true; // Устанавливаем DialogResult в true, если обновление прошло успешно
                
            }
            else
            {
                MessageBox.Show("Не удалось обновить данные работника.");
            }
        }

        private async Task<bool> UpdateWorkerAsync(int workerId, object updatedData)
        {
            string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseAddress);

                // Сериализуем обновленные данные в JSON
                string json = JsonConvert.SerializeObject(updatedData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Отправляем PUT-запрос
                HttpResponseMessage response = await client.PutAsync($"/users/{workerId}", content);

                // Проверяем статус ответа
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    // Логируем ошибку для отладки
                    Console.WriteLine($"Ошибка при обновлении работника: {response.StatusCode}");
                    return false;
                }
            }
        }
    }
}