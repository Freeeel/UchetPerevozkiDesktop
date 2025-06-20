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
        public RepairDetailsWindow(RepairWithUserName repair)
        {
            InitializeComponent();
            _currentRepair = repair; // Сохраняем переданный объект
            DisplayRepairDetails(repair);
        }
        private void DisplayRepairDetails(RepairWithUserName repair)
        {
            if (repair != null)
            {
                IdTextBlock.Text = repair.Id.ToString();
                DescriptionTextBlock.Text = repair.description_breakdown;
                StatusTextBox.Text = repair.status_text;
                DateTimeTextBlock.Text = repair.date_and_time_repair.ToString();
                UserNameTextBlock.Text = repair.user_name;
                // Показываем кнопку, если статус "Активна"
                // Убедитесь, что у вас есть кнопка с именем CompleteRepairButton в XAML
                if (repair.status_id == 1) // 1 соответствует "Активна"
                {
                    CompleteRepairButton.Visibility = Visibility.Visible;
                }
                else
                {
                    CompleteRepairButton.Visibility = Visibility.Collapsed;
                }
            }
        }
        private async void CompleteRepairButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentRepair == null)
            {
                MessageBox.Show("Данные о ремонте отсутствуют.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Проверяем, что ремонт действительно активен перед отправкой запроса
            if (_currentRepair.status_id != 1)
            {
                MessageBox.Show("Ремонт уже не активен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                CompleteRepairButton.Visibility = Visibility.Collapsed; // Скрываем кнопку
                return;
            }
            var updatePayload = new { status_id = 2 };

            var jsonPayload = JsonSerializer.Serialize(updatePayload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            string baseUrl;
            try
            {
                baseUrl = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось прочитать адрес API из файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        // Обновляем локальный статус и UI
                        _currentRepair.status_id = 2;
                        StatusTextBox.Text = _currentRepair.status_text; // Обновит текст на "Выполнена"
                        CompleteRepairButton.Visibility = Visibility.Collapsed; // Скрываем кнопку
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка при обновлении статуса ремонта: {response.StatusCode}\n{errorContent}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    MessageBox.Show($"Ошибка HTTP запроса: {httpEx.Message}", "Ошибка сети", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (JsonException jsonEx) // Исключение для System.Text.Json
                {
                    MessageBox.Show($"Ошибка сериализации JSON: {jsonEx.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла непредвиденная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}