using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.IO;
using System.Windows;

namespace Service
{
    public partial class MainWindow : Window
    {
        private readonly string _cs;

        public MainWindow()
        {
            InitializeComponent();

            var dbExists = File.Exists("school.db");
            DatabaseHelper.InitializeDatabase();

            _cs = DatabaseHelper.ConnectionString;

            LoadAll();

            tbStatus.Text = dbExists ? "База данных загружена" : "Создана новая база данных";
        }

        private void LoadAll()
        {
            LoadStudents();
            LoadTeachers();
            LoadClasses();
            LoadSubjects();
            LoadGrades();
            LoadSchedule();
        }

        private DataTable Query(string sql)
        {
            using var con = new SqliteConnection(_cs);
            con.Open();
            using var cmd = new SqliteCommand(sql, con);
            using var r = cmd.ExecuteReader();
            var dt = new DataTable();
            dt.Load(r);
            return dt;
        }

        private void Exec(string sql, params (string name, object? value)[] p)
        {
            using var con = new SqliteConnection(_cs);
            con.Open();
            using var cmd = new SqliteCommand(sql, con);
            foreach (var (name, value) in p)
                cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        // -------------------- LOADERS --------------------
        private void LoadStudents()
        {
            dgStudents.ItemsSource = Query(
                @"SELECT s.УченикId, s.ФИО, s.Дата_рождения, s.Телефон_родителя,
                         COALESCE(k.Название,'') AS Класс
                  FROM Ученики s
                  LEFT JOIN Классы k ON k.КлассId = s.КлассId
                  ORDER BY s.ФИО").DefaultView;
        }

        private void LoadTeachers()
        {
            dgTeachers.ItemsSource = Query(
                "SELECT УчительId, ФИО, Телефон, Email, Кабинет FROM Учителя ORDER BY ФИО").DefaultView;
        }

        private void LoadClasses()
        {
            dgClasses.ItemsSource = Query(
                @"SELECT c.КлассId, c.Название, c.Год_обучения,
                         COALESCE(t.ФИО,'') AS Классный_руководитель
                  FROM Классы c
                  LEFT JOIN Учителя t ON t.УчительId = c.КлассныйРуководительId
                  ORDER BY c.Год_обучения, c.Название").DefaultView;
        }

        private void LoadSubjects()
        {
            dgSubjects.ItemsSource = Query(
                @"SELECT p.ПредметId, p.Название,
                         COALESCE(t.ФИО,'') AS Учитель
                  FROM Предметы p
                  LEFT JOIN Учителя t ON t.УчительId = p.УчительId
                  ORDER BY p.Название").DefaultView;
        }

        private void LoadGrades()
        {
            dgGrades.ItemsSource = Query(
                @"SELECT g.ОценкаId,
                         s.ФИО AS Ученик,
                         p.Название AS Предмет,
                         g.Дата,
                         g.Оценка,
                         COALESCE(g.Тема,'') AS Тема
                  FROM Оценки g
                  INNER JOIN Ученики s ON s.УченикId = g.УченикId
                  INNER JOIN Предметы p ON p.ПредметId = g.ПредметId
                  ORDER BY g.Дата DESC, s.ФИО, p.Название").DefaultView;
        }

        private void LoadSchedule()
        {
            dgSchedule.ItemsSource = Query(
                @"SELECT sch.УрокId,
                         c.Название AS Класс,
                         sch.ДеньНедели,
                         sch.НомерУрока,
                         p.Название AS Предмет,
                         COALESCE(t.ФИО,'') AS Учитель,
                         COALESCE(sch.Кабинет,'') AS Кабинет,
                         COALESCE(sch.Время,'') AS Время
                  FROM Расписание sch
                  INNER JOIN Классы c ON c.КлассId = sch.КлассId
                  INNER JOIN Предметы p ON p.ПредметId = sch.ПредметId
                  LEFT JOIN Учителя t ON t.УчительId = sch.УчительId
                  ORDER BY c.Название, sch.ДеньНедели, sch.НомерУрока").DefaultView;
        }

        private void RefreshAll_Click(object sender, RoutedEventArgs e)
        {
            LoadAll();
            tbStatus.Text = "Обновлено";
        }

        // -------------------- STUDENTS --------------------
        private void AddStudent_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new StudentDialog(_cs);
            if (dlg.ShowDialog() != true) return;

            Exec(@"INSERT INTO Ученики (ФИО, Дата_рождения, Телефон_родителя, КлассId)
                  VALUES (@fio,@birth,@phone,@class)",
                ("@fio", dlg.Fio),
                ("@birth", dlg.BirthDate),
                ("@phone", dlg.ParentPhone),
                ("@class", dlg.ClassId));

            LoadStudents();
        }

        private void EditStudent_Click(object sender, RoutedEventArgs e)
        {
            if (dgStudents.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["УченикId"]);

            var classId = GetNullableInt("SELECT КлассId FROM Ученики WHERE УченикId=@id", id);

            var dlg = new StudentDialog(_cs,
                fio: row["ФИО"].ToString() ?? "",
                birthDate: row["Дата_рождения"].ToString(),
                parentPhone: row["Телефон_родителя"].ToString(),
                classId: classId);

            if (dlg.ShowDialog() != true) return;

            Exec(@"UPDATE Ученики
                  SET ФИО=@fio, Дата_рождения=@birth, Телефон_родителя=@phone, КлассId=@class
                  WHERE УченикId=@id",
                ("@fio", dlg.Fio),
                ("@birth", dlg.BirthDate),
                ("@phone", dlg.ParentPhone),
                ("@class", dlg.ClassId),
                ("@id", id));

            LoadStudents();
        }

        private void DeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            if (dgStudents.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["УченикId"]);

            if (MessageBox.Show("Удалить ученика? Его оценки тоже удалятся.", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Exec("DELETE FROM Ученики WHERE УченикId=@id", ("@id", id));
            LoadStudents();
            LoadGrades();
        }

        // -------------------- TEACHERS --------------------
        private void AddTeacher_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new TeacherDialog();
            if (dlg.ShowDialog() != true) return;

            Exec(@"INSERT INTO Учителя (ФИО, Телефон, Email, Кабинет)
                  VALUES (@fio,@phone,@email,@room)",
                ("@fio", dlg.Fio),
                ("@phone", dlg.Phone),
                ("@email", dlg.Email),
                ("@room", dlg.Room));

            LoadTeachers();
            LoadClasses();
            LoadSubjects();
            LoadSchedule();
        }

        private void EditTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (dgTeachers.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["УчительId"]);

            var dlg = new TeacherDialog(
                fio: row["ФИО"].ToString() ?? "",
                phone: row["Телефон"].ToString(),
                email: row["Email"].ToString(),
                room: row["Кабинет"].ToString());

            if (dlg.ShowDialog() != true) return;

            Exec(@"UPDATE Учителя
                  SET ФИО=@fio, Телефон=@phone, Email=@email, Кабинет=@room
                  WHERE УчительId=@id",
                ("@fio", dlg.Fio),
                ("@phone", dlg.Phone),
                ("@email", dlg.Email),
                ("@room", dlg.Room),
                ("@id", id));

            LoadTeachers();
            LoadClasses();
            LoadSubjects();
            LoadSchedule();
        }

        private void DeleteTeacher_Click(object sender, RoutedEventArgs e)
        {
            if (dgTeachers.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["УчительId"]);

            if (MessageBox.Show("Удалить учителя? В классах/предметах/расписании он будет очищен.", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Exec("DELETE FROM Учителя WHERE УчительId=@id", ("@id", id));
            LoadTeachers();
            LoadClasses();
            LoadSubjects();
            LoadSchedule();
        }

        // -------------------- CLASSES --------------------
        private void AddClass_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ClassDialog(_cs);
            if (dlg.ShowDialog() != true) return;

            Exec(@"INSERT INTO Классы (Название, Год_обучения, КлассныйРуководительId)
                  VALUES (@name,@year,@head)",
                ("@name", dlg.Name),
                ("@year", dlg.Year),
                ("@head", dlg.HeadTeacherId));

            LoadClasses();
            LoadStudents();
        }

        private void EditClass_Click(object sender, RoutedEventArgs e)
        {
            if (dgClasses.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["КлассId"]);
            var headId = GetNullableInt("SELECT КлассныйРуководительId FROM Классы WHERE КлассId=@id", id);

            var dlg = new ClassDialog(_cs,
                name: row["Название"].ToString() ?? "",
                year: Convert.ToInt32(row["Год_обучения"]),
                headTeacherId: headId);

            if (dlg.ShowDialog() != true) return;

            Exec(@"UPDATE Классы
                  SET Название=@name, Год_обучения=@year, КлассныйРуководительId=@head
                  WHERE КлассId=@id",
                ("@name", dlg.Name),
                ("@year", dlg.Year),
                ("@head", dlg.HeadTeacherId),
                ("@id", id));

            LoadClasses();
            LoadStudents();
            LoadSchedule();
        }

        private void DeleteClass_Click(object sender, RoutedEventArgs e)
        {
            if (dgClasses.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["КлассId"]);

            if (MessageBox.Show("Удалить класс? Его расписание удалится, у учеников класс станет пустым.", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Exec("DELETE FROM Классы WHERE КлассId=@id", ("@id", id));
            LoadClasses();
            LoadStudents();
            LoadSchedule();
        }

        // -------------------- SUBJECTS --------------------
        private void AddSubject_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SubjectDialog(_cs);
            if (dlg.ShowDialog() != true) return;

            Exec("INSERT INTO Предметы (Название, УчительId) VALUES (@n,@t)",
                ("@n", dlg.Name), ("@t", dlg.TeacherId));

            LoadSubjects();
            LoadGrades();
            LoadSchedule();
        }

        private void EditSubject_Click(object sender, RoutedEventArgs e)
        {
            if (dgSubjects.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["ПредметId"]);
            var teacherId = GetNullableInt("SELECT УчительId FROM Предметы WHERE ПредметId=@id", id);

            var dlg = new SubjectDialog(_cs, name: row["Название"].ToString() ?? "", teacherId: teacherId);
            if (dlg.ShowDialog() != true) return;

            Exec("UPDATE Предметы SET Название=@n, УчительId=@t WHERE ПредметId=@id",
                ("@n", dlg.Name), ("@t", dlg.TeacherId), ("@id", id));

            LoadSubjects();
            LoadGrades();
            LoadSchedule();
        }

        private void DeleteSubject_Click(object sender, RoutedEventArgs e)
        {
            if (dgSubjects.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["ПредметId"]);

            if (MessageBox.Show("Удалить предмет? Его оценки и уроки в расписании удалятся.", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Exec("DELETE FROM Предметы WHERE ПредметId=@id", ("@id", id));
            LoadSubjects();
            LoadGrades();
            LoadSchedule();
        }

        // -------------------- GRADES --------------------
        private void AddGrade_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new GradeDialog(_cs);
            if (dlg.ShowDialog() != true) return;

            Exec(@"INSERT INTO Оценки (УченикId, ПредметId, Дата, Оценка, Тема)
                  VALUES (@s,@p,@d,@g,@t)",
                ("@s", dlg.StudentId), ("@p", dlg.SubjectId), ("@d", dlg.Date), ("@g", dlg.Grade), ("@t", dlg.Topic));
            LoadGrades();
        }

        private void EditGrade_Click(object sender, RoutedEventArgs e)
        {
            if (dgGrades.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["ОценкаId"]);

            // Вытаскиваем реальные FK
            using var con = new SqliteConnection(_cs);
            con.Open();
            using var cmd = new SqliteCommand("SELECT УченикId, ПредметId, Дата, Оценка, Тема FROM Оценки WHERE ОценкаId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return;
            var studentId = r.GetInt32(0);
            var subjectId = r.GetInt32(1);
            var date = r.GetString(2);
            var grade = r.GetInt32(3);
            var topic = r.IsDBNull(4) ? null : r.GetString(4);

            var dlg = new GradeDialog(_cs, studentId, subjectId, date, grade, topic);
            if (dlg.ShowDialog() != true) return;

            Exec(@"UPDATE Оценки
                  SET УченикId=@s, ПредметId=@p, Дата=@d, Оценка=@g, Тема=@t
                  WHERE ОценкаId=@id",
                ("@s", dlg.StudentId), ("@p", dlg.SubjectId), ("@d", dlg.Date), ("@g", dlg.Grade), ("@t", dlg.Topic), ("@id", id));

            LoadGrades();
        }

        private void DeleteGrade_Click(object sender, RoutedEventArgs e)
        {
            if (dgGrades.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["ОценкаId"]);

            if (MessageBox.Show("Удалить оценку?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Exec("DELETE FROM Оценки WHERE ОценкаId=@id", ("@id", id));
            LoadGrades();
        }

        // -------------------- SCHEDULE --------------------
        private void AddSchedule_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ScheduleDialog(_cs);
            if (dlg.ShowDialog() != true) return;

            try
            {
                Exec(@"INSERT INTO Расписание (КлассId, ДеньНедели, НомерУрока, ПредметId, УчительId, Кабинет, Время)
                      VALUES (@c,@d,@n,@p,@t,@r,@tm)",
                    ("@c", dlg.ClassId), ("@d", dlg.DayOfWeek), ("@n", dlg.LessonNumber), ("@p", dlg.SubjectId),
                    ("@t", dlg.TeacherId), ("@r", dlg.Room), ("@tm", dlg.Time));
            }
            catch (SqliteException ex)
            {
                MessageBox.Show($"Не удалось добавить урок.\n\nПричина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadSchedule();
        }

        private void EditSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (dgSchedule.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["УрокId"]);

            using var con = new SqliteConnection(_cs);
            con.Open();
            using var cmd = new SqliteCommand(@"SELECT КлассId, ДеньНедели, НомерУрока, ПредметId, УчительId, Кабинет, Время
                                                FROM Расписание WHERE УрокId=@id", con);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return;

            var classId = r.GetInt32(0);
            var dow = r.GetInt32(1);
            var num = r.GetInt32(2);
            var subjId = r.GetInt32(3);
            int? teachId = r.IsDBNull(4) ? null : r.GetInt32(4);
            var room = r.IsDBNull(5) ? null : r.GetString(5);
            var time = r.IsDBNull(6) ? null : r.GetString(6);

            var dlg = new ScheduleDialog(_cs, classId, dow, num, subjId, teachId, room, time);
            if (dlg.ShowDialog() != true) return;

            try
            {
                Exec(@"UPDATE Расписание
                      SET КлассId=@c, ДеньНедели=@d, НомерУрока=@n, ПредметId=@p, УчительId=@t, Кабинет=@r, Время=@tm
                      WHERE УрокId=@id",
                    ("@c", dlg.ClassId), ("@d", dlg.DayOfWeek), ("@n", dlg.LessonNumber), ("@p", dlg.SubjectId),
                    ("@t", dlg.TeacherId), ("@r", dlg.Room), ("@tm", dlg.Time), ("@id", id));
            }
            catch (SqliteException ex)
            {
                MessageBox.Show($"Не удалось изменить урок.\n\nПричина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadSchedule();
        }

        private void DeleteSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (dgSchedule.SelectedItem is not DataRowView row) return;
            var id = Convert.ToInt32(row["УрокId"]);

            if (MessageBox.Show("Удалить урок из расписания?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Exec("DELETE FROM Расписание WHERE УрокId=@id", ("@id", id));
            LoadSchedule();
        }

        // -------------------- HELPERS --------------------
        private int? GetNullableInt(string sql, int id)
        {
            using var con = new SqliteConnection(_cs);
            con.Open();
            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);
            var val = cmd.ExecuteScalar();
            if (val == null || val == DBNull.Value) return null;
            return Convert.ToInt32(val);
        }
    }
}
