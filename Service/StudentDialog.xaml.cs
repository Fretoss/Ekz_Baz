using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace Service
{
    public partial class StudentDialog : Window
    {
        private readonly string _cs;

        public string Fio => tbFio.Text.Trim();
        public string? BirthDate => string.IsNullOrWhiteSpace(tbBirth.Text) ? null : tbBirth.Text.Trim();
        public string? ParentPhone => string.IsNullOrWhiteSpace(tbParent.Text) ? null : tbParent.Text.Trim();
        public int? ClassId => cbClass.SelectedValue as int?;

        private sealed class Item
        {
            public int? Id { get; set; }
            public string Title { get; set; } = "";
        }

        public StudentDialog(string connectionString, string fio = "", string? birthDate = null, string? parentPhone = null, int? classId = null)
        {
            InitializeComponent();
            _cs = connectionString;

            tbFio.Text = fio;
            tbBirth.Text = birthDate ?? "";
            tbParent.Text = parentPhone ?? "";

            LoadClasses();
            cbClass.SelectedValue = classId;
        }

        private void LoadClasses()
        {
            var list = new List<Item> { new Item { Id = null, Title = "— не выбран —" } };

            using var con = new SqliteConnection(_cs);
            con.Open();
            using var cmd = new SqliteCommand("SELECT КлассId, Название FROM Классы ORDER BY Год_обучения, Название", con);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Item { Id = r.GetInt32(0), Title = r.GetString(1) });
            }
            cbClass.ItemsSource = list;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbFio.Text))
            {
                MessageBox.Show("Введите ФИО ученика.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(tbBirth.Text))
            {
                if (!DateTime.TryParseExact(tbBirth.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    MessageBox.Show("Дата рождения должна быть в формате YYYY-MM-DD (или оставьте пустой).", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
