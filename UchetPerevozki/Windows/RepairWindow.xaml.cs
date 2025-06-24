
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        private ObservableCollection<RepairWithUserName> _repairs;
        private UserResponse _userData;
        public RepairWindow(UserResponse userData)
        {
            InitializeComponent();
            _repairs = new ObservableCollection<RepairWithUserName>();
            RepairsDataGrid.ItemsSource = _repairs;

            LoadRepairsDataAsync();
            _userData = userData;
            userNameTextBlock.Text = $"{_userData.Name} {_userData.Surname}";
        }
        private async Task LoadRepairsDataAsync()
        {
            try
            {
                // Очищаем коллекцию перед добавлением новых данных
                _repairs.Clear();
                List<RepairWithUserName> fetchedRepairs = await GetRepairsWithUsersAsync();
                foreach (var repair in fetchedRepairs)
                {
                    _repairs.Add(repair); // Добавляем данные в ObservableCollection
                }
                ApplyFilter(); // Применяем фильтр к загруженным данным
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task<List<RepairWithUserName>> GetRepairsWithUsersAsync()
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("ipAddress.txt").Trim();
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
                        throw new Exception($"Ошибка при получении данных о ремонтах.");
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
            IEnumerable<RepairWithUserName> filteredRepairs = _repairs; // Начинаем с ObservableCollection
            // Проверяем, какой радиобаттон выбран, и применяем соответствующий фильтр
            // Убедитесь, что RadioButton'ы имеют x:Name и GroupName,
            // а FindRadioButton() корректно их находит.
            if (FindRadioButton("Активные")?.IsChecked == true) // Используем безопасный вызов ?.
            {
                filteredRepairs = _repairs.Where(r => r.status_id == 1);
            }
            else if (FindRadioButton("Выполненные")?.IsChecked == true)
            {
                filteredRepairs = _repairs.Where(r => r.status_id == 2);
            }
            // Если ни "Активные", ни "Выполненные" не выбраны, остается фильтр по умолчанию ("Все заявки")
            // **ВАЖНОЕ ИЗМЕНЕНИЕ:** Присваиваем отфильтрованный список свойству ItemsSource DataGrid.
            // DataGrid repairsDataGrid = this.FindName("RepairsDataGrid") as DataGrid; // Эта строка не нужна, если RepairsDataGrid доступен напрямую
            // if (repairsDataGrid != null) { ... }
            RepairsDataGrid.ItemsSource = filteredRepairs.ToList(); // ItemsSource будет перепривязан к новому List
        }
        // Вспомогательный метод для поиска радиобаттона по его Content
        // (Предполагается, что FilterStackPanel имеет x:Name="FilterStackPanel" в XAML)
        private RadioButton FindRadioButton(string content)
        {
            // Пример: Ищем радиобаттоны в StackPanel, если FilterStackPanel доступен напрямую
            if (FilterStackPanel != null) // Замените FilterStackPanel на имя вашего StackPanel
            {
                foreach (var child in FilterStackPanel.Children)
                {
                    if (child is RadioButton radioButton && radioButton.Content.ToString() == content)
                    {
                        return radioButton;
                    }
                }
            }
            // Если не нашли или FilterStackPanel не существует, попробуйте FindVisualChild
            return FindVisualChild<RadioButton>(this, rb => rb.Content.ToString() == content); // Убрал GroupName для гибкости
        }
        // Вспомогательный метод для поиска визуального дочернего элемента
        private T FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate) where T : DependencyObject
        {
            if (parent == null) return null;
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
        // Обработчик двойного клика по строке DataGrid
        private void RepairsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, что есть выбранный элемент и это RepairWithUserName
            if (RepairsDataGrid.SelectedItem is RepairWithUserName selectedRepair)
            {
                // Создаем и открываем окно деталей, передавая ему ссылку на выбранный объект.
                RepairDetailsWindow detailsWindow = new RepairDetailsWindow(selectedRepair);
                // **ВАЖНО:** Используем ShowDialog(), чтобы заблокировать родительское окно
                // и получить результат после закрытия окна деталей.
                bool? dialogResult = detailsWindow.ShowDialog();
                // Проверяем DialogResult из RepairDetailsWindow
                if (dialogResult == true) // Если RepairDetailsWindow сообщило об успешном изменении
                {
                    // Так как _repairs (ObservableCollection) содержит ссылку на тот же объект,
                    // и объект RepairWithUserName реализует INotifyPropertyChanged,
                    // DataGrid уже обновил значения в строке.
                    // Однако, если изменение статуса привело к тому, что строка
                    // должна исчезнуть/появиться из-за фильтрации, нужно заново применить фильтр.
                    ApplyFilter(); // Повторно применяем текущий фильтр к обновленным данным
                }
            }
        }
        // Обработчики кнопок навигации
        private void WorkersButton_Click(object sender, RoutedEventArgs e)
        {
            WorkersWindow workersWindow = new WorkersWindow(_userData);
            workersWindow.Show();
            this.Close();
        }
        private void HistoryReportsButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryReportsWindow historyReportsWindow = new HistoryReportsWindow(_userData.Id);
            historyReportsWindow.Show();
            this.Close();
        }
    }
}