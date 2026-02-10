using Microsoft.Data.Sqlite;
using System;

namespace Service
{
    public static class DatabaseHelper
    {
        public static string ConnectionString = "Data Source=school.db";

        public static void InitializeDatabase()
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                ExecuteNonQuery(connection, "PRAGMA foreign_keys = ON;");

                // Учителя
                ExecuteNonQuery(connection,
                    @"CREATE TABLE IF NOT EXISTS Учителя (
                        УчительId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ФИО TEXT NOT NULL,
                        Телефон TEXT NULL,
                        Email TEXT NULL,
                        Кабинет TEXT NULL
                    );");

                // Классы
                ExecuteNonQuery(connection,
                    @"CREATE TABLE IF NOT EXISTS Классы (
                        КлассId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Название TEXT NOT NULL UNIQUE,
                        Год_обучения INTEGER NOT NULL,
                        КлассныйРуководительId INTEGER NULL,
                        FOREIGN KEY (КлассныйРуководительId) REFERENCES Учителя(УчительId)
                            ON UPDATE CASCADE ON DELETE SET NULL
                    );");

                // Ученики
                ExecuteNonQuery(connection,
                    @"CREATE TABLE IF NOT EXISTS Ученики (
                        УченикId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ФИО TEXT NOT NULL,
                        Дата_рождения TEXT NULL,
                        Телефон_родителя TEXT NULL,
                        КлассId INTEGER NULL,
                        FOREIGN KEY (КлассId) REFERENCES Классы(КлассId)
                            ON UPDATE CASCADE ON DELETE SET NULL
                    );");

                // Предметы
                ExecuteNonQuery(connection,
                    @"CREATE TABLE IF NOT EXISTS Предметы (
                        ПредметId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Название TEXT NOT NULL,
                        УчительId INTEGER NULL,
                        FOREIGN KEY (УчительId) REFERENCES Учителя(УчительId)
                            ON UPDATE CASCADE ON DELETE SET NULL
                    );");

                // Оценки (журнал)
                ExecuteNonQuery(connection,
                    @"CREATE TABLE IF NOT EXISTS Оценки (
                        ОценкаId INTEGER PRIMARY KEY AUTOINCREMENT,
                        УченикId INTEGER NOT NULL,
                        ПредметId INTEGER NOT NULL,
                        Дата TEXT NOT NULL,
                        Оценка INTEGER NOT NULL CHECK(Оценка BETWEEN 1 AND 5),
                        Тема TEXT NULL,
                        FOREIGN KEY (УченикId) REFERENCES Ученики(УченикId)
                            ON UPDATE CASCADE ON DELETE CASCADE,
                        FOREIGN KEY (ПредметId) REFERENCES Предметы(ПредметId)
                            ON UPDATE CASCADE ON DELETE CASCADE
                    );");

                // Расписание
                ExecuteNonQuery(connection,
                    @"CREATE TABLE IF NOT EXISTS Расписание (
                        УрокId INTEGER PRIMARY KEY AUTOINCREMENT,
                        КлассId INTEGER NOT NULL,
                        ДеньНедели INTEGER NOT NULL CHECK(ДеньНедели BETWEEN 1 AND 7),
                        НомерУрока INTEGER NOT NULL CHECK(НомерУрока BETWEEN 1 AND 10),
                        ПредметId INTEGER NOT NULL,
                        УчительId INTEGER NULL,
                        Кабинет TEXT NULL,
                        Время TEXT NULL,
                        UNIQUE(КлассId, ДеньНедели, НомерУрока),
                        FOREIGN KEY (КлассId) REFERENCES Классы(КлассId)
                            ON UPDATE CASCADE ON DELETE CASCADE,
                        FOREIGN KEY (ПредметId) REFERENCES Предметы(ПредметId)
                            ON UPDATE CASCADE ON DELETE CASCADE,
                        FOREIGN KEY (УчительId) REFERENCES Учителя(УчительId)
                            ON UPDATE CASCADE ON DELETE SET NULL
                    );");

                InsertTestDataIfEmpty(connection);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка инициализации базы данных: {ex.Message}");
            }
        }

        private static void ExecuteNonQuery(SqliteConnection connection, string sql)
        {
            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private static void InsertTestDataIfEmpty(SqliteConnection connection)
        {
            long teachersCount;
            using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Учителя", connection))
                teachersCount = Convert.ToInt64(cmd.ExecuteScalar());

            if (teachersCount > 0) return;

            ExecuteNonQuery(connection,
                @"INSERT INTO Учителя (ФИО, Телефон, Email, Кабинет) VALUES
                  ('Иванова Анна Сергеевна', '+79990001122', 'ivanova@school.ru', '101'),
                  ('Петров Михаил Андреевич', '+79990002233', 'petrov@school.ru', '205'),
                  ('Сидорова Ольга Викторовна', '+79990003344', 'sidorova@school.ru', '302');");

            ExecuteNonQuery(connection,
                @"INSERT INTO Классы (Название, Год_обучения, КлассныйРуководительId) VALUES
                  ('7А', 7, 1),
                  ('7Б', 7, 2),
                  ('8А', 8, 3);");

            ExecuteNonQuery(connection,
                @"INSERT INTO Ученики (ФИО, Дата_рождения, Телефон_родителя, КлассId) VALUES
                  ('Смирнов Денис Павлович', '2012-05-14', '+79991112233', 1),
                  ('Кузнецова Мария Игоревна', '2012-11-02', '+79992223344', 1),
                  ('Ахметов Тимур Русланович', '2011-02-20', '+79993334455', 2),
                  ('Волкова Алина Сергеевна', '2010-09-10', '+79994445566', 3);");

            ExecuteNonQuery(connection,
                @"INSERT INTO Предметы (Название, УчительId) VALUES
                  ('Математика', 2),
                  ('Русский язык', 1),
                  ('История', 3);");

            ExecuteNonQuery(connection,
                @"INSERT INTO Оценки (УченикId, ПредметId, Дата, Оценка, Тема) VALUES
                  (1, 1, '2026-02-10', 5, 'Контрольная'),
                  (1, 2, '2026-02-10', 4, 'Диктант'),
                  (2, 1, '2026-02-11', 3, 'Самостоятельная');");

            ExecuteNonQuery(connection,
                @"INSERT INTO Расписание (КлассId, ДеньНедели, НомерУрока, ПредметId, УчительId, Кабинет, Время) VALUES
                  (1, 1, 1, 2, 1, '101', '08:30-09:15'),
                  (1, 1, 2, 1, 2, '205', '09:25-10:10'),
                  (1, 1, 3, 3, 3, '302', '10:20-11:05');");
        }
    }
}
