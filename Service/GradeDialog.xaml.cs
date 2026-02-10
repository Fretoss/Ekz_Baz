using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace Service
{
    public partial class GradeDialog : Window
    {
        private readonly string _cs;

        public int StudentId => (int)(cbStudent.SelectedValue ?? 0);
        public int SubjectId => (int)(cbSubject.SelectedValue ?? 0);
        public string Date => tbDate.Text.Trim();
        public int Grade => int.TryParse(tbGrade.Text, out var g) ? g : 0;
        public string? Topic => string.IsNullOrWhiteSpace(tbTopic.Text) ? null : tbTopic.Text.Trim();

        private sealed class Item
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
        }

        public GradeDialog(string connectionString, int? studentId = null, int? subjectId = null,
            string? date = null, int grade = 5, string? topic = null)
        {
            InitializeComponent();
            _cs = connectionString;

            LoadStudents();
            LoadSubjects();

            if (studentId.HasValue) cbStudent.SelectedValue = studentId.Value;
            if (subjectId.HasValue) cbSubject.SelectedValue = subjectId.Value;

            tbDate.Text = date ?? DateTime.Today.ToString("yyyy-MM-dd");
            tbGrade.Text = grade.ToString();
            tbTopic.Text = topic ?? "";
        }

        private void LoadStudents()
        {
            var list = new List<Item>();
            using var con = new SqliteConnection(_cs);
            con.Open();
            using var cmd = new SqliteCommand(
                @"SELECT s.УченикId, s.ФИО, COALESCE(k.Название,'')
                  FROM Ученики s LEFT JOIN Классы k ON k.КлассId=s.КлассId
                  ORDER BY s.ФИО", con);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var id = r.GetInt32(0);
                var fio = r.GetString(1);
                var cls = r.GetString(2);
                list.Add(new Item { Id = id, Title = string.IsNullOrWhiteSpace(cls) ? fio : $"{fio} ({cls})" });
            }
            cbStudent.ItemsSource = list;
        }

        private void LoadSubjects()
        {
            var list = new List<Item>();
            using var con = new SqliteConnection(_cs);
            con.Open();
            using var cmd = new SqliteCommand("SELECT ПредметId, Название FROM Предметы ORDER BY Название", con);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Item { Id = r.GetInt32(0), Title = r.GetString(1) });

            cbSubject.ItemsSource = list;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (StudentId == 0 || SubjectId == 0)
            {
                MessageBox.Show("Выберите ученика и предмет.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DateTime.TryParseExact(tbDate.Text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                MessageBox.Show("Дата должна быть в формате YYYY-MM-DD.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(tbGrade.Text, out var g) || g < 1 || g > 5)
            {
                MessageBox.Show("Оценка должна быть числом 1..5.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
