using ClosedXML.Excel;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UchetPerevozki.Windows;

namespace UchetPerevozki
{
    /// <summary>
    /// Логика взаимодействия для HistoryReportsWindow.xaml
    /// </summary>
    public partial class HistoryReportsWindow : Window
    {
        private int _userId; // Это будет ID пользователя
        private UserResponse _userData;
        private List<ReportResponse> _allReports;

        public HistoryReportsWindow(int userId)
        {
            InitializeComponent();
            _userId = userId; // Сохраняем ID пользователя
        }
        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _userData = await GetUserDataAsync(_userId);
                userNameTextBlock.Text = $"{_userData.Name} {_userData.Surname}";
                _allReports = await GetReportsAsync();
                ReportsDataGrid.ItemsSource = _allReports;
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Ошибка при обращении к API: {httpEx.Message}");
            }
            catch (JsonException jsonEx)
            {
                MessageBox.Show($"Ошибка десериализации: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных пользователя: {ex.Message}");
            }
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

            string searchText = SearchTextBox.Text.ToLower().Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {

                ReportsDataGrid.ItemsSource = _allReports;
            }
            else
            {
                var filteredReports = _allReports.Where(report =>

                    (report.point_departure?.ToLower().Contains(searchText) ?? false) ||
                    (report.type_point_departure?.ToLower().Contains(searchText) ?? false) ||
                    (report.sender?.ToLower().Contains(searchText) ?? false) ||
                    (report.point_destination?.ToLower().Contains(searchText) ?? false) ||
                    (report.type_point_destination?.ToLower().Contains(searchText) ?? false) ||
                    (report.recipient?.ToLower().Contains(searchText) ?? false) ||
                    (report.view_wood?.ToLower().Contains(searchText) ?? false) ||
                    (report.assortment_wood_type?.ToLower().Contains(searchText) ?? false) ||
                    (report.variety_wood_type?.ToLower().Contains(searchText) ?? false) ||
                    (report.user_name?.ToLower().Contains(searchText) ?? false) ||
                    (report.user_surname?.ToLower().Contains(searchText) ?? false) ||
                    (report.user_patronymic?.ToLower().Contains(searchText) ?? false) ||
                    (report.user_full_name?.ToLower().Contains(searchText) ?? false) ||
                    (report.cargo?.ToLower().Contains(searchText) ?? false) ||
                    report.Id.ToString().ToLower().Contains(searchText) ||
                    report.length_wood.ToString().ToLower().Contains(searchText) ||
                    report.volume_wood.ToString().Replace(',', '.').ToLower().Contains(searchText) ||
                    report.user_id.ToString().ToLower().Contains(searchText) ||
                    report.report_date_time.ToString("dd.MM.yyyy HH:mm").ToLower().Contains(searchText)       
                ).ToList();
                ReportsDataGrid.ItemsSource = filteredReports;
            }
        }

        private async Task<List<ReportResponse>> GetReportsAsync()
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);
                var response = await client.GetAsync($"/reports/all");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<ReportResponse>>(responseData);
                }
                else
                {
                    throw new Exception($"Ошибка при получении отчетов: {response.StatusCode}");
                }
            }
        }

        private async Task<UserResponse> GetUserDataAsync(int userId)
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);

                var response = await client.GetAsync($"/getuser/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UserResponse>(responseData);
                }
                else
                {
                    throw new Exception($"Ошибка при получении данных о пользователе: {response.StatusCode}");
                }
            }
        }

        private async Task DeleteReportAsync(int reportId)
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);
                var response = await client.DeleteAsync($"/reports/{reportId}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ошибка API: {response.StatusCode} - {errorContent}");
                }
            }
        }

        private void RepairsButton_Click(object sender, RoutedEventArgs e)
        {
            RepairWindow repairWindow = new RepairWindow(_userData);
            repairWindow.Show();
            this.Close();
        }

        private void WorkersButton_Click(object sender, RoutedEventArgs e)
        {
            WorkersWindow workersWindow = new WorkersWindow(_userData);
            workersWindow.Show();
            this.Close();
        }

        private void ReportsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ReportsDataGrid.SelectedItem == null)
            {
                return;
            }

            if (ReportsDataGrid.SelectedItem is ReportResponse selectedReport)
            {
                // Создаем новое окно деталей и передаем выбранный отчет
                ReportsDetailsWindow detailsWindow = new ReportsDetailsWindow(selectedReport);
                detailsWindow.ShowDialog(); // ShowDialog() открывает модальное окно
            }

        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is ReportResponse selectedReport)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить отчет №{selectedReport.Id}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await DeleteReportAsync(selectedReport.Id);
                        MessageBox.Show("Отчет успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Обновляем список отчетов после удаления
                        _allReports = await GetReportsAsync();
                        ReportsDataGrid.ItemsSource = _allReports;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ReportsDataGrid.SelectedItem == null)
            {
                return;
            }

            if (ReportsDataGrid.SelectedItem is ReportResponse selectedReport)
            {
                // Создаем новое окно деталей и передаем выбранный отчет
                ReportsDetailsWindow detailsWindow = new ReportsDetailsWindow(selectedReport);
                detailsWindow.ShowDialog(); // ShowDialog() открывает модальное окно
            }
        }

        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем данные для экспорта (все или отфильтрованные)
                var reportsToExport = ReportsDataGrid.ItemsSource as IEnumerable<ReportResponse> ?? _allReports;

                if (reportsToExport == null || !reportsToExport.Any())
                {
                    MessageBox.Show("Нет данных для экспорта", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Создаем новую книгу Excel
                using (var workbook = new XLWorkbook())
                {
                    // Добавляем лист
                    var worksheet = workbook.Worksheets.Add("Отчеты о перевозках");

                    // Создаем стиль для заголовков
                    var headerStyle = workbook.Style;
                    headerStyle.Font.Bold = true;
                    headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerStyle.Fill.BackgroundColor = XLColor.LightGray;
                    headerStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // Заголовки столбцов
                    worksheet.Cell(1, 1).Value = "ID отчета";
                    worksheet.Cell(1, 2).Value = "Дата и время";
                    worksheet.Cell(1, 3).Value = "Пункт отправления";
                    worksheet.Cell(1, 4).Value = "Тип пункта отправления";
                    worksheet.Cell(1, 5).Value = "Отправитель";
                    worksheet.Cell(1, 6).Value = "Пункт назначения";
                    worksheet.Cell(1, 7).Value = "Тип пункта назначения";
                    worksheet.Cell(1, 8).Value = "Получатель";
                    worksheet.Cell(1, 9).Value = "Вид древесины";
                    worksheet.Cell(1, 10).Value = "Длина (м)";
                    worksheet.Cell(1, 11).Value = "Объем (куб.м)";
                    worksheet.Cell(1, 12).Value = "Тип асортимента";
                    worksheet.Cell(1, 13).Value = "Разновидность";
                    worksheet.Cell(1, 14).Value = "ID перевозчика";
                    worksheet.Cell(1, 15).Value = "ФИО перевозчика";
                   

                    // Применяем стиль к заголовкам
                    worksheet.Range("A1:P1").Style = headerStyle;

                    // Заполняем данные
                    int row = 2;
                    foreach (var report in reportsToExport)
                    {
                        worksheet.Cell(row, 1).Value = report.Id;
                        worksheet.Cell(row, 2).Value = report.report_date_time;
                        worksheet.Cell(row, 3).Value = report.point_departure;
                        worksheet.Cell(row, 4).Value = report.type_point_departure;
                        worksheet.Cell(row, 5).Value = report.sender;
                        worksheet.Cell(row, 6).Value = report.point_destination;
                        worksheet.Cell(row, 7).Value = report.type_point_destination;
                        worksheet.Cell(row, 8).Value = report.recipient;
                        worksheet.Cell(row, 9).Value = report.view_wood;
                        worksheet.Cell(row, 10).Value = report.length_wood;
                        worksheet.Cell(row, 11).Value = report.volume_wood;
                        worksheet.Cell(row, 12).Value = report.assortment_wood_type;
                        worksheet.Cell(row, 13).Value = report.variety_wood_type;
                        worksheet.Cell(row, 14).Value = report.user_id;
                        worksheet.Cell(row, 15).Value = report.user_full_name;
                        

                        row++;
                    }

                    // Форматирование столбцов
                    worksheet.Column(2).Style.NumberFormat.Format = "dd.MM.yyyy HH:mm";
                    worksheet.Column(10).Style.NumberFormat.Format = "0";
                    worksheet.Column(11).Style.NumberFormat.Format = "0.00";

                    // Автоподбор ширины столбцов
                    worksheet.Columns().AdjustToContents();

                    // Диалог сохранения файла (WPF версия)
                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = "Excel файлы (*.xlsx)|*.xlsx",
                        FileName = $"Отчеты_перевозок_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                        Title = "Сохранить отчет в Excel"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        workbook.SaveAs(saveFileDialog.FileName);
                        MessageBox.Show($"Успешно экспортировано {reportsToExport.Count()} записей\nФайл сохранен: {saveFileDialog.FileName}",
                            "Экспорт завершен",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel:\n{ex.Message}",
                    "Ошибка экспорта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}
