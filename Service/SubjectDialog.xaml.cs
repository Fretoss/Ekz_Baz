using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Windows;

namespace Service
{
    public partial class SubjectDialog : Window
    {
        private readonly string _cs;

        public string Name => tbName.Text.Trim();
        public int? TeacherId => cbTeacher.SelectedValue as int?;

        private sealed class Item
        {
            public int? Id { get; set; }
            public string Title { get; set; } = "";
        }

        public SubjectDialog(string connectionString, string name = "", int? teacherId = null)
        {
            InitializeComponent();
            _cs = connectionString;

            tbName.Text = name;

            LoadTeachers();
            cbTeacher.SelectedValue = teacherId;
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
            cbTeacher.ItemsSource = list;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Введите название предмета.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
