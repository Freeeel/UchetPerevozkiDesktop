﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

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

                var response = await client.PostAsync("/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync(); 
                    return JsonConvert.DeserializeObject<UserResponse>(responseData);
                }
                else
                {
                    throw new Exception($"Ошибка: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                }
            }
        }

        private async void btnLoginClick(object sender, RoutedEventArgs e)
        {
            string Login = loginTextBox.Text;
            string Password = passwordTextBox.Password;
            var userResponse = await LoginAsync(Login, Password);
            int userId = userResponse.Id;
            HistoryReportsWindow historyReportsWindow = new HistoryReportsWindow(userId);
            historyReportsWindow.Show();
            this.Close();
        }
    }
}
