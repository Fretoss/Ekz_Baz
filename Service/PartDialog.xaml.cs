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
    /// Логика взаимодействия для PartDialog.xaml
    /// </summary>
    public partial class PartDialog : Window
    {
        public int PartId { get; set; }
        public string PartName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // ИЗМЕНИТЕ тип с int на long
        public long SupplierId { get; set; }  // Было: public int SupplierId { get; set; }

        private string connectionString;

        public PartDialog(string connString)
        {
            connectionString = connString;
            InitializeComponent();
            DataContext = this;
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = new SqliteCommand(
                        "SELECT Код_поставщика, Название_компании FROM Поставщики ORDER BY Название_компании",
                        connection);

                    using (var reader = command.ExecuteReader())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);

                        // Добавляем пустую строку в начало
                        var newRow = dt.NewRow();
                        newRow["Код_поставщика"] = 0;
                        newRow["Название_компании"] = "(Не выбран)";
                        dt.Rows.InsertAt(newRow, 0);

                        cbSupplier.ItemsSource = dt.DefaultView;
                        cbSupplier.SelectedIndex = 0; // Выбираем первый элемент
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnNewSupplier_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SupplierDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var connection = new SqliteConnection(connectionString))
                    {
                        connection.Open();
                        var command = new SqliteCommand(
                            @"INSERT INTO Поставщики (Название_компании, Контактный_телефон) 
                      VALUES (@название, @телефон);
                      SELECT last_insert_rowid();",
                            connection);

                        command.Parameters.AddWithValue("@название", dialog.CompanyName);
                        command.Parameters.AddWithValue("@телефон", dialog.Phone);

                        var newId = command.ExecuteScalar();

                        // Обновляем список поставщиков
                        LoadSuppliers();

                        // Выбираем нового поставщика
                        if (newId != null)
                        {
                            SupplierId = Convert.ToInt32(newId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления поставщика: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            // ИЗМЕНИТЕ проверку с Name на PartName
            if (string.IsNullOrWhiteSpace(PartName))  // Было: Name
            {
                MessageBox.Show("Введите наименование запчасти", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Price <= 0)
            {
                MessageBox.Show("Введите цену запчасти", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Quantity < 0)
            {
                MessageBox.Show("Количество не может быть отрицательным", "Ошибка",
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
