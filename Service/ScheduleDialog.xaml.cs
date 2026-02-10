using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Windows;

namespace Service
{
    public partial class ScheduleDialog : Window
    {
        private readonly string _cs;

        public int ClassId => (int)(cbClass.SelectedValue ?? 0);
        public int DayOfWeek => int.TryParse(tbDow.Text, out var d) ? d : 0;
        public int LessonNumber => int.TryParse(tbNum.Text, out var n) ? n : 0;
        public int SubjectId => (int)(cbSubject.SelectedValue ?? 0);
        public int? TeacherId => cbTeacher.SelectedValue as int?;
        public string? Room => string.IsNullOrWhiteSpace(tbRoom.Text) ? null : tbRoom.Text.Trim();
        public string? Time => string.IsNullOrWhiteSpace(tbTime.Text) ? null : tbTime.Text.Trim();

        private sealed class Item
        {
            public int? Id { get; set; }
            public string Title { get; set; } = "";
        }

        public ScheduleDialog(string connectionString,
            int? classId = null, int dayOfWeek = 1, int lessonNumber = 1,
            int? subjectId = null, int? teacherId = null, string? room = null, string? time = null)
        {
            InitializeComponent();
            _cs = connectionString;

            LoadClasses();
            LoadSubjects();
            LoadTeachers();

            if (classId.HasValue) cbClass.SelectedValue = classId.Value;
            tbDow.Text = dayOfWeek.ToString();
            tbNum.Text = lessonNumber.ToString();
            if (subjectId.HasValue) cbSubject.SelectedValue = subjectId.Value;
            cbTeacher.SelectedValue = teacherId;
            tbRoom.Text = room ?? "";
            tbTime.Text = time ?? "";
        }

        private void LoadClasses()
        {
            var list = new List<Item>();
            using var con = new SqliteConnection(_cs);
            con.Open();
            using var cmd = new SqliteCommand("SELECT КлассId, Название FROM Классы ORDER BY Год_обучения, Название", con);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Item { Id = r.GetInt32(0), Title = r.GetString(1) });
            cbClass.ItemsSource = list;
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
            if (ClassId == 0 || SubjectId == 0)
            {
                MessageBox.Show("Выберите класс и предмет.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (DayOfWeek < 1 || DayOfWeek > 7)
            {
                MessageBox.Show("День недели должен быть 1..7.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (LessonNumber < 1 || LessonNumber > 10)
            {
                MessageBox.Show("Номер урока должен быть 1..10.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
