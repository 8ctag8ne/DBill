//DesktopApp/TableDialog.xaml.cs
using System.Windows;
using System.Windows.Controls;
using CoreLib.Models;
using CoreLib.Services;

namespace DBill.WpfApp
{
    public partial class TableDialog : Window
    {
        private readonly TableService _tableService;
        public string TableName => tbTableName.Text.Trim();
        public List<Column> Columns { get; } = new List<Column>();

        public TableDialog(TableService tableService)
        {
            InitializeComponent();
            _tableService = tableService;
            cbColumnType.SelectedIndex = 0;
            lbColumns.ItemsSource = ColumnsDisplay;
        }

        private List<ColumnDisplay> ColumnsDisplay => Columns.Select(c => new ColumnDisplay { 
            Name = c.Name, 
            Type = c.Type.ToString(), 
            Display = $"{c.Name} ({c.Type})" 
        }).ToList();

        private void BtnAddColumn_Click(object sender, RoutedEventArgs e)
        {
            var name = tbColumnName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) 
            {
                ShowError("Введіть назву колонки");
                return;
            }

            if (Columns.Any(c => c.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)))
            {
                ShowError("Колонка з такою назвою вже існує");
                return;
            }

            var typeStr = (cbColumnType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "string";
            DataType type = typeStr.ToLower() switch
            {
                "integer" => DataType.Integer,
                "real" => DataType.Real,
                "char" => DataType.Char,
                "string" => DataType.String,
                "integerinvl" => DataType.IntegerInterval,
                "file" => DataType.TextFile,
                _ => DataType.String
            };

            Columns.Add(new Column(name, type));
            RefreshColumnsList();
            tbColumnName.Clear();
            ClearError();
        }

        private void BtnDeleteColumn_Click(object sender, RoutedEventArgs e)
        {
            if (lbColumns.SelectedIndex >= 0 && lbColumns.SelectedIndex < Columns.Count)
            {
                Columns.RemoveAt(lbColumns.SelectedIndex);
                RefreshColumnsList();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            // Використовуємо валідацію з бібліотеки
            var validation = _tableService.ValidateTableCreation(TableName, Columns);
            
            if (!validation.IsValid)
            {
                ShowError(string.Join("\n", validation.Errors));
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

        private void RefreshColumnsList()
        {
            lbColumns.ItemsSource = ColumnsDisplay;
            lbColumns.Items.Refresh();
        }

        private void ShowError(string message)
        {
            // Можна додати спеціальний контрол для відображення помилок
            MessageBox.Show(message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ClearError()
        {
            // Можна очистити відображення помилок, якщо використовуєте спеціальний контрол
        }

        private class ColumnDisplay
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Display { get; set; }
        }
    }
}
