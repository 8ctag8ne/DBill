using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CoreLib.Models;

namespace DBill.WpfApp
{
    public partial class TableDialog : Window
    {
        public string TableName => tbTableName.Text.Trim();
        public List<Column> Columns { get; } = new List<Column>();

        public TableDialog()
        {
            InitializeComponent();
            cbColumnType.SelectedIndex = 0;
            lbColumns.ItemsSource = ColumnsDisplay;
        }

        private List<ColumnDisplay> ColumnsDisplay => Columns.Select(c => new ColumnDisplay { Name = c.Name, Type = c.Type.ToString(), Display = $"{c.Name} ({c.Type})" }).ToList();

        private void BtnAddColumn_Click(object sender, RoutedEventArgs e)
        {
            var name = tbColumnName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;
            if (Columns.Any(c => c.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Колонка з такою назвою вже існує.");
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
            lbColumns.ItemsSource = ColumnsDisplay;
            lbColumns.Items.Refresh();
            tbColumnName.Clear();
        }

        private void BtnDeleteColumn_Click(object sender, RoutedEventArgs e)
        {
            if (lbColumns.SelectedIndex >= 0 && lbColumns.SelectedIndex < Columns.Count)
            {
                Columns.RemoveAt(lbColumns.SelectedIndex);
                lbColumns.ItemsSource = ColumnsDisplay;
                lbColumns.Items.Refresh();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TableName) || Columns.Count == 0)
            {
                MessageBox.Show("Вкажіть назву таблиці та додайте хоча б одну колонку.");
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

        private class ColumnDisplay
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Display { get; set; }
        }
    }
}
