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
            string name = NameTB.Text;
            string surname = SurnameTB.Text;
            string patronymic = PatronymicTB.Text;
            string phone = PhoneTB.Text;
            string address_residential = AddressTB.Text;
            string login = LoginTB.Text;
            string password = PasswordPB.Password;
            string bankAccountNumberText = BankAccountNumberTB.Text;

            // Проверка на заполненность полей
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(surname) ||
                string.IsNullOrEmpty(patronymic) || string.IsNullOrEmpty(phone) ||
                string.IsNullOrEmpty(address_residential) || string.IsNullOrEmpty(login) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(bankAccountNumberText))
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
            var updatedData = new
            {
                name,
                surname,
                patronymic,
                phone,
                address_residential,
                bank_account_number,
                login,
                password
            };
            // Вызываем API для обновления данных работника
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
                string json = JsonConvert.SerializeObject(updatedData);
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
    }
}