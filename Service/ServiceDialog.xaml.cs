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
    /// Логика взаимодействия для ServiceDialog.xaml
    /// </summary>
    public partial class ServiceDialog : Window
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Hours { get; set; }

        // ИЗМЕНИТЕ тип с int на long
        public long? WorkerId { get; set; }  // Было: public int WorkerId { get; set; }

        private string connectionString;

        public ServiceDialog(string connString)
        {
            connectionString = connString;
            InitializeComponent();
            DataContext = this;
            LoadWorkers();
        }

        private void LoadWorkers()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqliteCommand(
                        "SELECT Код_работника, ФИО FROM Работники ORDER BY ФИО",
                        connection);

                    using (var reader = command.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);

                        // Добавляем пустой элемент в начало
                        var newRow = dt.NewRow();
                        newRow["Код_работника"] = DBNull.Value;
                        newRow["ФИО"] = "(Не выбран)";
                        dt.Rows.InsertAt(newRow, 0);

                        cbWorker.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки работников: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            // ИЗМЕНЕНИЕ: проверяйте ServiceName вместо Name
            if (string.IsNullOrWhiteSpace(ServiceName))
            {
                MessageBox.Show("Введите наименование услуги", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Price <= 0)
            {
                MessageBox.Show("Введите цену услуги", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Hours <= 0)
            {
                MessageBox.Show("Введите норму часов", "Ошибка",
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
