using System.Windows;

namespace DBill.WpfApp
{
    public partial class RenameColumnDialog : Window
    {
        public string NewColumnName => tbNewName.Text.Trim();

        public RenameColumnDialog(string oldName)
        {
            InitializeComponent();
            tbOldName.Text = oldName;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
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