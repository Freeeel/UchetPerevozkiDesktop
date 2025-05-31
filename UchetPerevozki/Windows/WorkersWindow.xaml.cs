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
using UchetPerevozki.Windows;



namespace UchetPerevozki
{
    /// <summary>
    /// Логика взаимодействия для WorkersWindow.xaml
    /// </summary>
    public partial class WorkersWindow : Window
    {
        private UserResponse _userData;
        public WorkersWindow(UserResponse userData)
        {
            InitializeComponent();
            LoadData();
            _userData = userData;
        }
        private async void LoadData()
        {
            try
            {
                var workers = await GetWorkersAsync();
                WrapPanel wrapPanel = await CreateWorkerCards(workers);
                WorkersStackPanel.Children.Clear();
                WorkersStackPanel.Children.Add(wrapPanel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async Task<List<WorkerResponse>> GetWorkersAsync()
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress); // Заменить на отдельный класс
                var response = await client.GetAsync("/users/role/1"); // Получаем работников с role_id 1
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<WorkerResponse>>(responseData);
                }
                else
                {
                    throw new Exception($"Ошибка при получении работников: {response.StatusCode}");
                }
            }
        }

        private async Task<CarResponse> GetCarByWorkerIdAsync(int userId)
        {
            using (var client = new HttpClient())
            {
                string baseAddress = File.ReadAllText("C:\\Users\\Дмитрий\\source\\repos\\UchetPerevozki\\UchetPerevozki\\ipAddress.txt").Trim();
                client.BaseAddress = new Uri(baseAddress);
                var response = await client.GetAsync($"/user/{userId}/car");
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<CarResponse>(responseData);
                }
                else
                {
                    throw new Exception($"Ошибка при получении машины для пользователя {userId}: {response.StatusCode}");
                }
            }
        }

        private async Task<WrapPanel> CreateWorkerCards(List<WorkerResponse> workers)
        {
            WrapPanel wrapPanel = new WrapPanel
            {
                Orientation = Orientation.Horizontal,
                Width = 960,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            foreach (var worker in workers)
            {
                StackPanel card = await CreateWorkerCard(worker);
                wrapPanel.Children.Add(card);
            }

            return wrapPanel;
        }

        private async Task<StackPanel> CreateWorkerCard(WorkerResponse worker)
        {
            // Создание основной карточки
            StackPanel card = new StackPanel
            {
                Margin = new Thickness(10),
                Width = 300,
            };

            Border border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Background = Brushes.White,
                Padding = new Thickness(10),
            };

            StackPanel innerStack = new StackPanel
            {
                Margin = new Thickness(-11, 0, -11, 0)
            };

            // Заголовок карточки с данными о работнике
            StackPanel headerStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20, 10, 20, 10)
            };

            Image avatarImage = new Image
            {
                Source = new BitmapImage(new Uri("C:\\Users\\Дмитрий\\Desktop\\иконки\\avaWork.png")),
                Height = 72,
                Width = 72
            };

            StackPanel workerInfoStack = new StackPanel
            {
                Margin = new Thickness(10, 0, 0, 0)
            };

            workerInfoStack.Children.Add(CreateTextBlock("ID:", worker.Id.ToString()));
            workerInfoStack.Children.Add(CreateTextBlock("Дата рождения:", worker.date_of_birthday.ToString("dd/MM/yyyy")));
            workerInfoStack.Children.Add(CreateTextBlock("ФИО:", $"{worker.surname} {worker.name} {worker.patronymic}"));

            headerStack.Children.Add(avatarImage);
            headerStack.Children.Add(workerInfoStack);

            innerStack.Children.Add(headerStack);

            // Разделитель
            innerStack.Children.Add(new Line { X1 = 0, Y1 = 0, X2 = 334, Y2 = 0, Stroke = Brushes.Black, StrokeThickness = 1, Margin = new Thickness(0, 10, 0, 10) });

            // Получаем машину для работника
            var car = await GetCarByWorkerIdAsync(worker.Id);

            // Убедитесь, что вы добавляете свои метки
            innerStack.Children.Add(CreateLabel("Марка ТС:", car?.model));
            innerStack.Children.Add(CreateLabel("Модель ТС:", car?.stamp));
            innerStack.Children.Add(CreateLabel("Номер ТС:", car?.state_number));
            innerStack.Children.Add(CreateLabel("Телефон:", worker.phone));
            innerStack.Children.Add(CreateLabel("Адрес:", worker.address_residential));
            innerStack.Children.Add(CreateLabel("Номер банковского счёта:", worker.bank_account_number));

            Button editButton = new Button
            {
                Content = "Редактировать",
                Margin = new Thickness(10, 20, 10, 0),
                Height = 30,
                Width = 150,
            };

            // Обработчик клика по кнопке "Редактировать"
            editButton.Click += (sender, e) => OnEditButtonClick(worker);

            innerStack.Children.Add(editButton);
            border.Child = innerStack;
            card.Children.Add(border);

            return card;


        }

        private StackPanel CreateTextBlock(string label, string value)
        {
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            stackPanel.Children.Add(new TextBlock { Text = label, FontSize = 14, FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = value, FontSize = 14, Margin = new Thickness(5, 0, 0, 0) });

            return stackPanel;
        }

        private StackPanel CreateLabel(string label, string value)
        {
            StackPanel stackPanel = new StackPanel { Margin = new Thickness(0, 5, 0, 0) };
            TextBlock labelBlock = new TextBlock { Text = label, FontSize = 16, Margin = new Thickness(25, 10, 0, 0) };
            Border valueBorder = new Border
            {
                Width = 280,
                Height = 32,
                CornerRadius = new CornerRadius(5),
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                Margin = new Thickness(0, 5, 0, 0)
            };

            Label innerLabel = new Label
            {
                Content = value ?? string.Empty,
                Padding = new Thickness(5),
                BorderThickness = new Thickness(0),
                Width = 270,
                Height = 26
            };

            valueBorder.Child = innerLabel;
            stackPanel.Children.Add(labelBlock);
            stackPanel.Children.Add(valueBorder);

            return stackPanel;
        }

        private Border CreateDataBorder(string content, string label)
        {
            Border border = new Border
            {
                Width = 280,
                Height = 32,
                CornerRadius = new CornerRadius(5),
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
            };

            Label innerLabel = new Label
            {
                Content = $"{label}: {content}",
                Padding = new Thickness(5),
                BorderThickness = new Thickness(0),
                Width = 270,
                Height = 26
            };

            border.Child = innerLabel;
            return border;
        }

        private void OnEditButtonClick(WorkerResponse worker)
        {

            EditWorkersWindow editWindow = new EditWorkersWindow(worker);
            editWindow.Show();
        }

        private void btnAddWorker(object sender, RoutedEventArgs e)
        {
            AddWorkersWindow addWorkersWindow = new AddWorkersWindow();
            addWorkersWindow.Show();
        }

        private void BtnHistoryOpenWindow(object sender, RoutedEventArgs e)
        {
            HistoryReportsWindow historyReportsWindow = new HistoryReportsWindow(_userData.Id);
            historyReportsWindow.Show();
            this.Close();
        }
    }
}
