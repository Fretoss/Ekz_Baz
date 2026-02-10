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
    /// Логика взаимодействия для OrderDialog.xaml
    /// </summary>
    public partial class OrderDialog : Window
    {
        public long? CarId { get; set; }          // Было: public int CarId { get; set; }
        public long? ManagerId { get; set; }      // Было: public int ManagerId { get; set; }
        public long? WorkerId { get; set; }       // Было: public int WorkerId { get; set; }
        public string Problem { get; set; }

        private string connectionString;

        public OrderDialog(string connString)
        {
            connectionString = connString;
            InitializeComponent();
            DataContext = this;
            LoadCars();
            LoadManagers();
            LoadWorkers();
        }

        private void LoadCars()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqliteCommand(@"
                SELECT a.Код_автомобиля, 
                       a.Государственный_номер || ' ' || a.Марка || ' ' || a.Модель as Автомобиль,
                       k.ФИО
                FROM Автомобили a
                LEFT JOIN Клиенты k ON a.Код_клиента = k.Код_клиента
                ORDER BY a.Марка, a.Модель",
                        connection);

                    using (var reader = command.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        cbCar.ItemsSource = dt.DefaultView;

                        // Автоматически выбираем первый элемент, если он есть
                        if (dt.Rows.Count > 0)
                        {
                            CarId = Convert.ToInt64(dt.Rows[0]["Код_автомобиля"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки автомобилей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadManagers()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqliteCommand(
                        "SELECT Код_менеджера, ФИО FROM Менеджеры ORDER BY ФИО",
                        connection);

                    using (var reader = command.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        cbManager.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки менеджеров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                        // Добавляем пустой элемент
                        var newRow = dt.NewRow();
                        newRow["Код_работника"] = DBNull.Value;
                        newRow["ФИО"] = "(Не обязательно)";
                        dt.Rows.InsertAt(newRow, 0);

                        cbWorker.ItemsSource = dt.DefaultView;
                        cbWorker.SelectedIndex = 0;
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
            // Проверяем, что выбраны обязательные поля
            if (!CarId.HasValue || CarId.Value <= 0)
            {
                MessageBox.Show("Выберите автомобиль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Подсвечиваем поле
                if (cbCar != null)
                    cbCar.Focus();
                return;
            }

            if (!ManagerId.HasValue || ManagerId.Value <= 0)
            {
                MessageBox.Show("Выберите менеджера", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                if (cbManager != null)
                    cbManager.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Problem))
            {
                MessageBox.Show("Введите описание проблемы", "Ошибка",
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
