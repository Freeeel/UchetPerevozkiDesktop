using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
    /// Логика взаимодействия для RepairDetailsWindow.xaml
    /// </summary>
    public partial class RepairDetailsWindow : Window
    {
        private RepairWithUserName _currentRepair; // Для хранения текущего объекта ремонта
        // Конструктор: принимает ссылку на объект, который нужно редактировать
        public RepairDetailsWindow(RepairWithUserName repair)
        {
            InitializeComponent();
            _currentRepair = repair;

            this.DataContext = _currentRepair;

            UpdateCompleteButtonVisibility();
        }

        private void UpdateCompleteButtonVisibility()
        {
            if (_currentRepair != null && _currentRepair.status_id == 1) // 1 соответствует "Активна"
            {
                CompleteRepairButton.Visibility = Visibility.Visible;
            }
            else
            {
                CompleteRepairButton.Visibility = Visibility.Collapsed;
            }
        }



        private async void CompleteRepairButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRepair == null)
            {
                MessageBox.Show("Данные о ремонте отсутствуют.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_currentRepair.status_id != 1)
            {
                MessageBox.Show("Ремонт уже не активен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                UpdateCompleteButtonVisibility();
                this.DialogResult = false;
                return;
            }
            var updatePayload = new { status_id = 2 };
            var jsonPayload = JsonSerializer.Serialize(updatePayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            string baseUrl;
            try
            {
                baseUrl = File.ReadAllText("ipAddress.txt").Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось прочитать адрес API из файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
                return;
            }
            string apiUrl = $"{baseUrl}/repairs/{_currentRepair.Id}";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.PutAsync(apiUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Статус ремонта успешно обновлен на 'Выполнена'.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        _currentRepair.status_id = 2;
                        UpdateCompleteButtonVisibility();
                        this.DialogResult = true;
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка при обновлении статуса ремонта: {response.StatusCode}\n{errorContent}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        this.DialogResult = false;
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    MessageBox.Show($"Ошибка HTTP запроса: {httpEx.Message}", "Ошибка сети", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DialogResult = false;
                }
                catch (JsonException jsonEx)
                {
                    MessageBox.Show($"Ошибка десериализации JSON: {jsonEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DialogResult = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла непредвиденная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.DialogResult = false;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}