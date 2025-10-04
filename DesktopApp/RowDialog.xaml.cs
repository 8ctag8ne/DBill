//DesktopApp/RowDialog.xaml.cs
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CoreLib.Models;
using CoreLib.Services;

namespace DBill.WpfApp
{
    public partial class RowDialog : Window
    {
        private readonly TableService _tableService;
        private readonly FileService _fileService;
        private readonly string _tableName;
        public Dictionary<string, object?> Values { get; private set; } = new();
        private readonly Dictionary<string, Column> _columnInfo;
        private readonly Dictionary<string, FileRecord?> _fileRecords = new();

        public RowDialog(string tableName, List<Column> columns, FileService fileService, TableService tableService)
        {
            InitializeComponent();
            _tableName = tableName;
            _columnInfo = columns.ToDictionary(c => c.Name, c => c);
            _fileService = fileService;
            _tableService = tableService;
            BuildFields(null);
        }

        public RowDialog(string tableName, List<Column> columns, Dictionary<string, object?> values, 
                    FileService fileService, TableService tableService)
        {
            InitializeComponent();
            _tableName = tableName;
            _columnInfo = columns.ToDictionary(c => c.Name, c => c);
            _fileService = fileService;
            _tableService = tableService;
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

        private async void btnOk_Click(object sender, RoutedEventArgs e)
        {
            // Збираємо "сирі" дані з форми
            var rawData = CollectRawDataFromForm();
            
            // Використовуємо бібліотеку для парсингу та валідації
            var (parsedValues, validation) = _tableService.ParseAndValidateRowData(
                _tableName, rawData, _fileRecords);

            if (!validation.IsValid)
            {
                ShowError(string.Join("\n", validation.Errors));
                return;
            }

            Values = parsedValues;
            DialogResult = true;
            Close();
        }

        private Dictionary<string, object?> CollectRawDataFromForm()
        {
            var rawData = new Dictionary<string, object?>();
            
            foreach (var kvp in _columnInfo)
            {
                var colName = kvp.Key;
                var column = kvp.Value;

                try
                {
                    switch (column.Type)
                    {
                        case DataType.Integer:
                        case DataType.Real:
                        case DataType.Char:
                        case DataType.String:
                            var tb = FindControl<TextBox>("tb_" + colName);
                            rawData[colName] = tb?.Text;
                            break;

                        case DataType.IntegerInterval:
                            // Збираємо дані з двох полів у спеціальний об'єкт
                            var tbMin = FindControl<TextBox>("tbMin_" + colName);
                            var tbMax = FindControl<TextBox>("tbMax_" + colName);
                            rawData[colName] = new { 
                                Min = tbMin?.Text ?? "", 
                                Max = tbMax?.Text ?? "" 
                            };
                            break;

                        case DataType.TextFile:
                            // Файли вже зберігаються в _fileRecords
                            break;

                        default:
                            var defaultTb = FindControl<TextBox>("tb_" + colName);
                            rawData[colName] = defaultTb?.Text;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    rawData[colName] = null;
                }
            }
            
            return rawData;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Error);
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