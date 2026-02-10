using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

namespace Service
{
    /// <summary>
    /// Логика взаимодействия для CompleteOrderDialog.xaml
    /// </summary>
    public partial class CompleteOrderDialog : Window
    {
        public DateTime CompletionDate { get; set; } = DateTime.Today;
        public decimal TotalCost { get; set; }
        public int ServiceId { get; set; }
        public int PartId { get; set; }
        public int PartQuantity { get; set; } = 1;
        public string Problem { get; set; }

        private string connectionString = "Data Source=autoservice.db";

        public CompleteOrderDialog(string connectionString)
        {
            this.connectionString = connectionString;
            InitializeComponent();
            DataContext = this;
            LoadServices();
            LoadParts();

            if (!string.IsNullOrEmpty(Problem))
            {
                tbProblem.Text = Problem;
            }
        }

        private void LoadServices()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqliteCommand(
                        "SELECT Код_услуги, Наименование_услуги, Цена FROM Услуги ORDER BY Наименование_услуги",
                        connection);

                    using (var reader = command.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        cbService.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки услуг: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadParts()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqliteCommand(
                        "SELECT Код_запчасти, Наименование_запчасти, Цена, Количество_на_складе FROM Запчасти WHERE Количество_на_складе > 0 ORDER BY Наименование_запчасти",
                        connection);

                    using (var reader = command.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        cbPart.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки запчастей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (TotalCost <= 0)
            {
                MessageBox.Show("Введите общую стоимость", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PartId > 0 && PartQuantity <= 0)
            {
                MessageBox.Show("Введите количество запчастей", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CbService_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbService.SelectedItem is DataRowView row)
            {
                // Можно автоматически рассчитать стоимость
            }
        }

        private void CbPart_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPart.SelectedItem is DataRowView row)
            {
                // Можно показать текущее количество на складе
            }
        }
    }
}
