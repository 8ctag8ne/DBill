using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CoreLib.Models;
using CoreLib.Services;

namespace DBill.WpfApp
{
    public partial class RowDialog : Window
    {
        private readonly FileService _fileService;
        public Dictionary<string, object?> Values { get; private set; } = new();
        private readonly Dictionary<string, Column> _columnInfo;
        private readonly Dictionary<string, FileRecord?> _fileRecords = new();

        public RowDialog(List<Column> columns, FileService fileService)
        {
            InitializeComponent();
            _columnInfo = columns.ToDictionary(c => c.Name, c => c);
            _fileService = fileService;
            BuildFields(null);
        }

        public RowDialog(List<Column> columns, Dictionary<string, object?> values, FileService fileService)
        {
            InitializeComponent();
            _columnInfo = columns.ToDictionary(c => c.Name, c => c);
            _fileService = fileService;
            BuildFields(values);
        }

        private async Task OpenFile(FileRecord fileRecord)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{fileRecord.StoragePath}\"",
                    UseShellExecute = false
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відкриття файлу: {ex.Message}");
            }
        }

        private void BuildFields(Dictionary<string, object?>? values)
        {
            spFields.Children.Clear();

            foreach (var kvp in _columnInfo)
            {
                var colName = kvp.Key;
                var column = kvp.Value;

                var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
                panel.Children.Add(new TextBlock
                {
                    Text = colName + ":",
                    Width = 120,
                    VerticalAlignment = VerticalAlignment.Center
                });

                if (column.Type == DataType.IntegerInterval)
                {
                    // Інтервал: два TextBox для Min і Max
                    var innerPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    innerPanel.Children.Add(new TextBlock { Text = "Від:", Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center });
                    var tbMin = new TextBox { Name = "tbMin_" + colName, Width = 60, Margin = new Thickness(0, 0, 10, 0) };
                    innerPanel.Children.Add(tbMin);
                    innerPanel.Children.Add(new TextBlock { Text = "До:", Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center });
                    var tbMax = new TextBox { Name = "tbMax_" + colName, Width = 60 };
                    innerPanel.Children.Add(tbMax);

                    if (values?.ContainsKey(colName) == true && values[colName] is IntegerInterval interval)
                    {
                        tbMin.Text = interval.Min.ToString();
                        tbMax.Text = interval.Max.ToString();
                    }
                    panel.Children.Add(innerPanel);
                }
                else if (column.Type == DataType.TextFile)
                {
                    // Файл: кнопка вибору + назва файлу
                    var innerPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    var btnSelect = new Button { Content = "Вибрати файл...", Width = 120, Margin = new Thickness(0, 0, 10, 0) };
                    var tbFileName = new TextBlock
                    {
                        Name = "tbFileName_" + colName,
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = "(файл не вибрано)",
                        Foreground = System.Windows.Media.Brushes.Gray
                    };

                    if (values?.ContainsKey(colName) == true && values[colName] is FileRecord fileRecord)
                    {
                        _fileRecords[colName] = fileRecord;
                        tbFileName.Text = fileRecord.FileName;
                        tbFileName.Foreground = System.Windows.Media.Brushes.Blue;
                        tbFileName.TextDecorations = TextDecorations.Underline;
                        tbFileName.Cursor = System.Windows.Input.Cursors.Hand;
                        tbFileName.MouseLeftButtonUp += (s, e) => OpenFile(fileRecord);
                    }

                    btnSelect.Click += async (s, e) =>
                    {
                        var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Текстові файли (*.txt)|*.txt" };
                        if (dlg.ShowDialog() == true)
                        {
                            try
                            {
                                var fileContent = File.ReadAllBytes(dlg.FileName);
                                var fileName = Path.GetFileName(dlg.FileName);
                                
                                // Зберігаємо файл через FileService
                                var storagePath = await _fileService.SaveFileAsync(fileContent, fileName);
                                
                                var fr = new FileRecord(fileName, storagePath, fileContent.Length, "text/plain");
                                _fileRecords[colName] = fr;
                                
                                tbFileName.Text = fr.FileName;
                                tbFileName.Foreground = System.Windows.Media.Brushes.Blue;
                                tbFileName.TextDecorations = TextDecorations.Underline;
                                tbFileName.Cursor = System.Windows.Input.Cursors.Hand;
                                tbFileName.MouseLeftButtonUp += async (s2, e2) => await OpenFile(fr);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Помилка завантаження файлу: {ex.Message}");
                            }
                        }
                    };

                    innerPanel.Children.Add(btnSelect);
                    innerPanel.Children.Add(tbFileName);
                    panel.Children.Add(innerPanel);
                }
                else
                {
                    // Звичайний TextBox
                    var tb = new TextBox { Name = "tb_" + colName, Width = 250 };
                    if (values?.ContainsKey(colName) == true && values[colName] != null)
                        tb.Text = values[colName]?.ToString() ?? "";
                    panel.Children.Add(tb);
                }

                spFields.Children.Add(panel);
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Values = new Dictionary<string, object?>();
            
            foreach (var kvp in _columnInfo)
            {
                var colName = kvp.Key;
                var column = kvp.Value;

                try
                {
                    switch (column.Type)
                    {
                        case DataType.Integer:
                            var tbInt = FindControl<TextBox>("tb_" + colName);
                            if (tbInt != null && !string.IsNullOrWhiteSpace(tbInt.Text))
                            {
                                if (int.TryParse(tbInt.Text, out int intValue))
                                    Values[colName] = intValue; // ✅ Зберігаємо як int, не string
                                else
                                {
                                    MessageBox.Show($"Некоректне ціле число для '{colName}'.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                Values[colName] = null;
                            }
                            break;

                        case DataType.Real:
                            var tbReal = FindControl<TextBox>("tb_" + colName);
                            if (tbReal != null && !string.IsNullOrWhiteSpace(tbReal.Text))
                            {
                                if (double.TryParse(tbReal.Text, out double realValue))
                                    Values[colName] = realValue; // ✅ Зберігаємо як double
                                else
                                {
                                    MessageBox.Show($"Некоректне дійсне число для '{colName}'.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                Values[colName] = null;
                            }
                            break;

                        case DataType.Char:
                            var tbChar = FindControl<TextBox>("tb_" + colName);
                            if (tbChar != null && !string.IsNullOrWhiteSpace(tbChar.Text))
                            {
                                if (tbChar.Text.Length == 1)
                                    Values[colName] = tbChar.Text[0]; // ✅ Зберігаємо як char
                                else
                                {
                                    MessageBox.Show($"Поле '{colName}' має містити один символ.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                Values[colName] = null;
                            }
                            break;

                        case DataType.IntegerInterval:
                            var tbMin = FindControl<TextBox>("tbMin_" + colName);
                            var tbMax = FindControl<TextBox>("tbMax_" + colName);
                            
                            if (tbMin != null && tbMax != null && !string.IsNullOrWhiteSpace(tbMin.Text) && !string.IsNullOrWhiteSpace(tbMax.Text))
                            {
                                if (int.TryParse(tbMin.Text, out int min) && int.TryParse(tbMax.Text, out int max))
                                {
                                    Values[colName] = new IntegerInterval(min, max);
                                }
                                else
                                {
                                    MessageBox.Show($"Некоректні значення для інтервалу '{colName}'.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                Values[colName] = null;
                            }
                            break;
                        
                        case DataType.TextFile:
                            Values[colName] = _fileRecords.ContainsKey(colName) ? _fileRecords[colName] : null;
                            break;
                        
                        default: // String
                            var tb = FindControl<TextBox>("tb_" + colName);
                            Values[colName] = tb?.Text;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка обробки поля '{colName}': {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private T? FindControl<T>(string name) where T : FrameworkElement
        {
            foreach (var child in spFields.Children)
            {
                if (child is StackPanel sp)
                {
                    foreach (var c in sp.Children)
                    {
                        if (c is T ctrl && ctrl.Name == name)
                            return ctrl;
                        if (c is StackPanel inner)
                        {
                            foreach (var ic in inner.Children)
                                if (ic is T ictrl && ictrl.Name == name)
                                    return ictrl;
                        }
                    }
                }
            }
            return null;
        }
    }
}