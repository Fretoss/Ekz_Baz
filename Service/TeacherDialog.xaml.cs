using System.Windows;

namespace Service
{
    public partial class TeacherDialog : Window
    {
        public string Fio => tbFio.Text.Trim();
        public string? Phone => string.IsNullOrWhiteSpace(tbPhone.Text) ? null : tbPhone.Text.Trim();
        public string? Email => string.IsNullOrWhiteSpace(tbEmail.Text) ? null : tbEmail.Text.Trim();
        public string? Room => string.IsNullOrWhiteSpace(tbRoom.Text) ? null : tbRoom.Text.Trim();

        public TeacherDialog(string fio = "", string? phone = null, string? email = null, string? room = null)
        {
            InitializeComponent();
            tbFio.Text = fio;
            tbPhone.Text = phone ?? "";
            tbEmail.Text = email ?? "";
            tbRoom.Text = room ?? "";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbFio.Text))
            {
                MessageBox.Show("Введите ФИО учителя.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
