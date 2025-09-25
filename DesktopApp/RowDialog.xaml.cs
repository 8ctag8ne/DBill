using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DBill.WpfApp
{
    public partial class RowDialog : Window
    {
        public Dictionary<string, object?> Values { get; private set; } = new();
        private readonly List<string> _columns;

        public RowDialog(List<string> columns)
        {
            InitializeComponent();
            _columns = columns;
            BuildFields(null);
        }

        public RowDialog(List<string> columns, Dictionary<string, object?> values)
        {
            InitializeComponent();
            _columns = columns;
            BuildFields(values);
        }

        private void BuildFields(Dictionary<string, object?>? values)
        {
            spFields.Children.Clear();
            foreach (var col in _columns)
            {
                var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
                panel.Children.Add(new TextBlock { Text = col + ":", Width = 120, VerticalAlignment = VerticalAlignment.Center });
                var tb = new TextBox { Name = "tb_" + col, Width = 250 };
                if (values != null && values.ContainsKey(col) && values[col] != null)
                    tb.Text = values[col]?.ToString() ?? "";
                panel.Children.Add(tb);
                spFields.Children.Add(panel);
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Values = new Dictionary<string, object?>();
            foreach (var col in _columns)
            {
                var tb = FindNameInStackPanel(spFields, "tb_" + col) as TextBox;
                Values[col] = tb?.Text;
            }
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private Control? FindNameInStackPanel(Panel parent, string name)
        {
            foreach (var child in parent.Children)
            {
                if (child is StackPanel sp)
                {
                    foreach (var c in sp.Children)
                        if (c is Control ctrl && ctrl.Name == name)
                            return ctrl;
                }
            }
            return null;
        }
    }
}
