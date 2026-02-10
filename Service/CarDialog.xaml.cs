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
    /// Логика взаимодействия для CarDialog.xaml
    /// </summary>
    public partial class CarDialog : Window
    {
        public int CarId { get; set; }

        // ИЗМЕНИТЕ тип
        public long? ClientId { get; set; }  // Было: public int ClientId { get; set; }

        public string Number { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; } = DateTime.Now.Year;
        public int Mileage { get; set; }
        public string Color { get; set; }

        private string connectionString;

        public CarDialog(string connString)
        {
            connectionString = connString;
            InitializeComponent();
            DataContext = this;
            LoadClients();
        }

        private void LoadClients()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqliteCommand("SELECT Код_клиента, ФИО FROM Клиенты ORDER BY ФИО", connection);

                    using (var reader = command.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);

                        // Добавляем пустой элемент
                        var newRow = dt.NewRow();
                        newRow["Код_клиента"] = DBNull.Value;
                        newRow["ФИО"] = "(Выберите клиента)";
                        dt.Rows.InsertAt(newRow, 0);

                        cbClient.ItemsSource = dt.DefaultView;
                        cbClient.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!ClientId.HasValue || ClientId.Value <= 0)
            {
                MessageBox.Show("Выберите клиента", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(Number))
            {
                MessageBox.Show("Введите государственный номер", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(Brand))
            {
                MessageBox.Show("Введите марку автомобиля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(Model))
            {
                MessageBox.Show("Введите модель автомобиля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Year < 1900 || Year > DateTime.Now.Year + 1)
            {
                MessageBox.Show("Введите корректный год выпуска", "Ошибка",
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
    }
}
