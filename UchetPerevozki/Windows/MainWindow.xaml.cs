using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace UchetPerevozki
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;
        public MainWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        public async Task<UserResponse> LoginAsync(string Login, string Password)
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));



                var userLogin = new UserLogin
                {
                    login = Login,
                    password = Password
                };

                var json = JsonConvert.SerializeObject(userLogin);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync("/login", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<UserResponse>(responseData);
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при подключении к серверу: {ex.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
        }

        private async void btnLoginClick(object sender, RoutedEventArgs e)
        {
            string Login = loginTextBox.Text;
            string Password = passwordTextBox.Password;

            if (string.IsNullOrEmpty(Login))
            {
                MessageBox.Show("Пожалуйста, заполните поле логин!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Прерываем выполнение метода, если поля не заполнены
            }
            if (string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Пожалуйста, заполните поле пароль!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Прерываем выполнение метода, если поля не заполнены
            }


            int userId = 0; // Инициализируем userId значением по умолчанию
            var userResponse = await LoginAsync(Login, Password);
            if (userResponse != null)
            {
                userId = userResponse.Id; // Получаем userId из ответа
                HistoryReportsWindow historyReportsWindow = new HistoryReportsWindow(userId);
                historyReportsWindow.Show();
                this.Close();
            }
        }
    }
}
