using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Service
{
    public partial class ClassDialog : Window
    {
        private readonly string _cs;

        public string Name => tbName.Text.Trim();
        public int Year => int.TryParse(tbYear.Text, out var y) ? y : 0;
        public int? HeadTeacherId => cbHead.SelectedValue as int?;

        private sealed class Item
        {
            public int? Id { get; set; }
            public string Title { get; set; } = "";
        }

        public ClassDialog(string connectionString, string name = "", int year = 1, int? headTeacherId = null)
        {
            InitializeComponent();
            _cs = connectionString;

            tbName.Text = name;
            tbYear.Text = year.ToString();

            LoadTeachers();
            cbHead.SelectedValue = headTeacherId;
        }

        private void LoadTeachers()
        {
            var list = new List<Item> { new Item { Id = null, Title = "— не выбран —" } };

            using var con = new SqliteConnection(_cs);
            con.Open();
            using var cmd = new SqliteCommand("SELECT УчительId, ФИО FROM Учителя ORDER BY ФИО", con);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Item { Id = r.GetInt32(0), Title = r.GetString(1) });

            cbHead.ItemsSource = list;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Введите название класса.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(tbYear.Text, out var y) || y < 1 || y > 11)
            {
                MessageBox.Show("Год обучения должен быть числом 1..11.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
