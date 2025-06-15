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
    /// Логика взаимодействия для RepairWindow.xaml
    /// </summary>
    public partial class RepairWindow : Window
    {
        private List<RepairWithUserName> _repairs;

        public RepairWindow()
        {
            InitializeComponent();
            LoadRepairsDataAsync();
        }

        private async Task LoadRepairsDataAsync()
        {
            try
            {
                _repairs = await GetRepairsWithUsersAsync(); // Получаем все данные
                                                             // Применяем фильтр по умолчанию (например, "Все заявки")
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task<List<RepairWithUserName>> GetRepairsWithUsersAsync()
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);
                try
                {
                    var response = await client.GetAsync("/repairs_with_users/");
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        List<RepairWithUserName> repairs = JsonConvert.DeserializeObject<List<RepairWithUserName>>(responseData);
                        return repairs;
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Ошибка при получении данных о ремонтах: {response.StatusCode} - {errorContent}");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    throw new Exception($"Ошибка HTTP запроса: {httpEx.Message}");
                }
                catch (JsonException jsonEx)
                {
                    throw new Exception($"Ошибка десериализации JSON: {jsonEx.Message}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Произошла ошибка: {ex.Message}");
                }
            }
        }
        // Обработчик события Checked для радиобаттонов
        private void StatusFilter_Checked(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }
        // Метод для применения фильтрации
        private void ApplyFilter()
        {
            if (_repairs == null)
            {
                return;
            }
            IEnumerable<RepairWithUserName> filteredRepairs = _repairs;
            // Проверяем, какой радиобаттон выбран, и применяем соответствующий фильтр
            if (FindRadioButton("Активные").IsChecked == true)
            {
                filteredRepairs = _repairs.Where(r => r.status_id == 1);
            }
            else if (FindRadioButton("Выполненные").IsChecked == true)
            {
                filteredRepairs = _repairs.Where(r => r.status_id == 2);
            }
            // Если ни "Активные", ни "Выполненные" не выбраны, остается фильтр по умолчанию ("Все заявки")
            DataGrid repairsDataGrid = this.FindName("RepairsDataGrid") as DataGrid;
            if (repairsDataGrid != null)
            {
                repairsDataGrid.ItemsSource = filteredRepairs.ToList();
            }
        }
        // Вспомогательный метод для поиска радиобаттона по его Content
        private RadioButton FindRadioButton(string content)
        {
            // Ищем радиобаттоны в визуальном дереве, например, в StackPanel
            // Вам может потребоваться адаптировать этот поиск в зависимости от вашей XAML структуры
            StackPanel filterPanel = this.FindName("FilterStackPanel") as StackPanel; // Добавьте x:Name="FilterStackPanel" к StackPanel
            if (filterPanel != null)
            {
                foreach (var child in filterPanel.Children)
                {
                    if (child is RadioButton radioButton && radioButton.Content.ToString() == content)
                    {
                        return radioButton;
                    }
                }
            }
            // Если радиобаттон не найден в StackPanel, попробуйте найти его в другом месте
            // Этот код может потребовать доработки в зависимости от вашей XAML структуры
            // Простой пример поиска по всему визуальному дереву (менее эффективно для больших деревьев)
            return FindVisualChild<RadioButton>(this, rb => rb.Content.ToString() == content && rb.GroupName == "StatusFilter");
        }
        // Вспомогательный метод для поиска визуального дочернего элемента (тот же, что и раньше)
        private T FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild && predicate(typedChild))
                {
                    return typedChild;
                }
                else
                {
                    T foundChild = FindVisualChild(child, predicate);
                    if (foundChild != null)
                    {
                        return foundChild;
                    }
                }
            }
            return null;
        }
    }
}