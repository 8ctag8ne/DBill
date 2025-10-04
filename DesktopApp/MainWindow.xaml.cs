using CoreLib.Services;
using CoreLib.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DataGrid = System.Windows.Controls.DataGrid;

namespace DBill.WpfApp
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _databaseService;
        private readonly TableService _tableService;
        private readonly FileService _fileService;

        protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // Очищаємо тимчасові файли при закритті
            try
            {
                await _databaseService.CloseDatabase();
            }
            catch
            {
                // Ігноруємо помилки очищення
            }
        }

        // Активує кнопку перейменування при кліку на хедер
        private void DgRows_Sorting(object sender, DataGridSortingEventArgs e)
        {
            // Вимикаємо сортування
            e.Handled = true;
            // Активуємо кнопку і зберігаємо вибрану колонку
            dgRows.CurrentCell = new DataGridCellInfo(dgRows.Items[0], e.Column);
            UpdateRenameColumnButtonState();
        }
        private void DgRows_HeaderClick(object sender, RoutedEventArgs e)
        {
            // MessageBox.Show("HeaderClick event triggered");
            if (e.OriginalSource is DataGridColumnHeader header && header.Column != null)
            {
                // MessageBox.Show($"Column header clicked: {header.Column.Header}");
                dgRows.CurrentCell = new DataGridCellInfo(dgRows.Items.Count > 0 ? dgRows.Items[0] : null, header.Column);
                UpdateRenameColumnButtonState();
            }
            else
            {
                // MessageBox.Show("Clicked, but not on column header");
            }
        }

        private void UpdateDatabaseButtonsState()
        {
            bool hasDatabaseLoaded = _databaseService.CurrentDatabase != null;

            btnAddTable.IsEnabled = hasDatabaseLoaded;
            btnSaveDb.IsEnabled = hasDatabaseLoaded;
        }

        private void UpdateTableButtonsState()
        {
            bool hasTableSelected = lstTables.SelectedItem != null;
            btnDeleteTable.IsEnabled = hasTableSelected;
        }

        public MainWindow(DatabaseService dbService, TableService tableService, FileService fileService)
        {
            InitializeComponent();
            _databaseService = dbService;
            _tableService = tableService;
            _fileService = fileService;

            // Додаємо прямий обробник кліку на хедер
            dgRows.LoadingRow += (s, e) =>
            {
                // MessageBox.Show($"Loading row {e.Row.GetIndex()}");
                if (e.Row.Header is DataGridRowHeader header)
                {
                    header.Click += DgRows_HeaderClick;
                }
            };

            // Додаємо обробник кліку безпосередньо на DataGrid
            dgRows.PreviewMouseLeftButtonUp += (s, e) =>
            {
                var originalSource = e.OriginalSource as DependencyObject;
                while (originalSource != null && !(originalSource is DataGridColumnHeader))
                {
                    originalSource = VisualTreeHelper.GetParent(originalSource);
                }

                if (originalSource is DataGridColumnHeader columnHeader)
                {
                    // MessageBox.Show($"Column header clicked via PreviewMouseLeftButtonUp: {columnHeader.Column.Header}");
                    dgRows.CurrentCell = new DataGridCellInfo(dgRows.Items.Count > 0 ? dgRows.Items[0] : null, columnHeader.Column);
                    UpdateRenameColumnButtonState();
                }
            };

            dgRows.SelectedCellsChanged += DgRows_SelectedCellsChanged;
            dgRows.SelectionChanged += DgRows_SelectionChanged;
            dgRows.ColumnReordered += DgRows_ColumnReordered;

            btnCreateDb.Click += BtnCreateDb_Click;
            btnOpenDb.Click += BtnOpenDb_Click;
            btnSaveDb.Click += BtnSaveDb_Click;
            btnAddTable.Click += BtnAddTable_Click;
            btnDeleteTable.Click += BtnDeleteTable_Click;
            btnAddRow.Click += BtnAddRow_Click;
            btnEditRow.Click += BtnEditRow_Click;
            btnDeleteRow.Click += BtnDeleteRow_Click;
            lstTables.SelectionChanged += LstTables_SelectionChanged;
            dgRows.Sorting += DgRows_Sorting;

            UpdateCurrentDbName();
            UpdateDatabaseButtonsState();
            UpdateTableButtonsState();
        }

        private void DgRows_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            if (lstTables.SelectedItem is not string tableName) return;

            // Зберігаємо новий порядок після перестановки
            var newOrder = dgRows.Columns
                .OrderBy(c => c.DisplayIndex)
                .Select(c => c.Header?.ToString()?.Split('(')[0].Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();

            // Зберігаємо в таблиці через сервіс
            _tableService.ReorderColumns(tableName, newOrder);
        }

        private async void BtnCreateDb_Click(object sender, RoutedEventArgs e)
        {
            // Очищаємо файли попередньої бази
            await _databaseService.CloseDatabase();

            var name = Microsoft.VisualBasic.Interaction.InputBox("Введіть назву бази:", "Створення бази");
            if (string.IsNullOrWhiteSpace(name)) return;

            _databaseService.CreateDatabase(name);
            ClearTableUI();
            UpdateTablesList();
            UpdateCurrentDbName();
            UpdateDatabaseButtonsState();
            MessageBox.Show($"Базу '{name}' створено.");
        }

        private async void BtnOpenDb_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "JSON файли (*.json)|*.json|Всі файли|*.*" };
            if (dlg.ShowDialog() != true) return;

            try
            {
                
                // ОЧИЩАЄМО UI ПЕРЕД ЗАВАНТАЖЕННЯМ
                ClearTableUI();
                
                await _databaseService.LoadDatabaseAsync(dlg.FileName);

                UpdateTablesList();
                UpdateCurrentDbName();
                UpdateDatabaseButtonsState();
                
                // ЯКЩО Є ТАБЛИЦІ - БУДУЄМО UI ПЕРШОЇ ТАБЛИЦІ
                if (lstTables.Items.Count > 0 && lstTables.SelectedItem is string firstTable)
                {
                    BuildTableUI(firstTable);
                }
                MessageBox.Show("Базу завантажено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Помилка завантаження бази:\n{ex.Message}\n\nПопередня база залишилась відкритою.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void UpdateRenameColumnButtonState()
        {
            if (lstTables.SelectedItem is not string tableName)
            {
                // MessageBox.Show("UpdateRenameColumnButtonState: No table selected");
                btnRenameColumn.IsEnabled = false;
                return;
            }

            var currentCell = dgRows.CurrentCell;
            if (currentCell.Column == null)
            {
                // MessageBox.Show("UpdateRenameColumnButtonState: No column selected");
                btnRenameColumn.IsEnabled = false;
                return;
            }

            var columns = _tableService.GetColumnNames(tableName);
            // MessageBox.Show($"UpdateRenameColumnButtonState: Table: {tableName}, Column: {currentCell.Column.Header}, Columns count: {columns.Count}");
            btnRenameColumn.IsEnabled = columns.Count > 0;
        }

        private async void BtnSaveDb_Click(object sender, RoutedEventArgs e)
        {
            if (_databaseService.CurrentDatabase == null)
            {
                MessageBox.Show("Немає відкритої бази для збереження.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "JSON файли (*.json)|*.json|Всі файли|*.*" };
            if (dlg.ShowDialog() != true) return;

            try
            {
                await _databaseService.SaveDatabaseAsync(dlg.FileName);
                MessageBox.Show("Базу збережено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Помилка збереження бази:\n{ex.Message}\n\nБаза залишилась у пам'яті без змін.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void BtnAddTable_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new TableDialog(_tableService); // Передаємо TableService
            if (dlg.ShowDialog() == true)
            {
                var tableName = dlg.TableName;
                var columns = dlg.Columns;
                
                try
                {
                    _tableService.CreateTable(tableName, columns);
                    UpdateTablesList();
                    MessageBox.Show($"Таблицю '{tableName}' створено.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка створення таблиці: {ex.Message}");
                }
            }
        }

        private void BtnDeleteTable_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is string tableName)
            {
                if (MessageBox.Show($"Видалити таблицю '{tableName}'?", "Підтвердження", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _tableService.DeleteTable(tableName);
                    UpdateTablesList();
                }
            }
        }

        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is not string tableName) return;
            var table = _tableService.GetTable(tableName);
            if (table == null) return;
            
            var rowDialog = new RowDialog(tableName, table.Columns, _fileService, _tableService);
            if (rowDialog.ShowDialog() == true)
            {
                var values = rowDialog.Values;
                _tableService.AddRow(tableName, values);
                BuildTableUI(tableName);
            }
        }


        private void BtnEditRow_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is not string tableName) return;
            if (dgRows.SelectedItem is not System.Collections.IDictionary row) return;
            int rowIndex = dgRows.SelectedIndex;
            
            var table = _tableService.GetTable(tableName);
            if (table == null) return;
            
            var columns = table.Columns;
            var oldValues = columns.ToDictionary(c => c.Name, c => row[c.Name]);
            
            var rowDialog = new RowDialog(tableName, columns, oldValues, _fileService, _tableService);
            if (rowDialog.ShowDialog() == true)
            {
                var values = rowDialog.Values;
                _tableService.UpdateRow(tableName, rowIndex, values);
                BuildTableUI(tableName);
            }
        }

        private void BtnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is not string tableName) return;
            if (dgRows.SelectedIndex < 0) return;
            _tableService.DeleteRow(tableName, dgRows.SelectedIndex);
            BuildTableUI(tableName);
        }

        private void LstTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstTables.SelectedItem is string tableName)
            {
                // ВИКОРИСТОВУЄМО НОВИЙ МЕТОД ДЛЯ ПОБУДОВИ UI
                BuildTableUI(tableName);
            }
            else
            {
                ClearTableUI();
                UpdateRenameColumnButtonState();
            }
            UpdateTableButtonsState();
        }

        // Додаткові функції для перейменування та перестановки колонок
        private void BtnRenameColumn_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is not string tableName) return;
            if (dgRows.CurrentCell == null || dgRows.CurrentCell.Column == null) return;

            var header = dgRows.CurrentCell.Column.Header?.ToString();
            if (string.IsNullOrWhiteSpace(header)) return;

            var oldName = header.Split('(')[0].Trim();

            var dlg = new RenameColumnDialog(oldName);
            if (dlg.ShowDialog() == true)
            {
                var newName = dlg.NewColumnName;
                if (string.IsNullOrWhiteSpace(newName)) return;

                // ✅ БЕРЕМО ПОТОЧНИЙ ПОРЯДОК З ТАБЛИЦІ (він вже оновлений через ColumnReordered)
                var currentOrder = _tableService.GetColumnNames(tableName);

                var result = _tableService.RenameColumn(tableName, oldName, newName);
                if (!result)
                {
                    MessageBox.Show("Не вдалося перейменувати колонку.");
                }
                else
                {
                    // Оновлюємо порядок з новою назвою
                    var newOrder = currentOrder.Select(n => n == oldName ? newName : n).ToList();
                    // _tableService.ReorderColumns(tableName, newOrder);
                    BuildTableUI(tableName);
                }
            }
        }

        private void DgRows_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            UpdateRenameColumnButtonState();
        }

        // Оновлення списку таблиць
        private void UpdateTablesList()
        {
            lstTables.ItemsSource = _databaseService.GetTableNames();
            if (lstTables.Items.Count > 0)
                lstTables.SelectedIndex = 0;
            UpdateCurrentDbName();
            UpdateTableButtonsState();
        }

        private void UpdateCurrentDbName()
        {
            if (tbCurrentDbName == null) return;
            var db = _databaseService.CurrentDatabase;
            tbCurrentDbName.Text = db != null ? $"База: {db.Name}" : "(База не вибрана)";
        }

        private void OpenFileFromGrid(FileRecord fileRecord)
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
                MessageBox.Show($"Помилка відкриття файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DgRows_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnEditRow.IsEnabled = dgRows.SelectedIndex >= 0;
            btnDeleteRow.IsEnabled = dgRows.SelectedIndex >= 0;
        }

        private void dgRows_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (btnEditRow.IsEnabled)
                BtnEditRow_Click(sender, e);
        }
        private void ClearTableUI()
        {
            dgRows.Columns.Clear();
            dgRows.ItemsSource = null;
            
            if (tbNoTable != null) 
                tbNoTable.Visibility = Visibility.Visible;
            if (tbEmptyTable != null) 
                tbEmptyTable.Visibility = Visibility.Collapsed;
                
            btnAddRow.IsEnabled = false;
            btnEditRow.IsEnabled = false;
            btnDeleteRow.IsEnabled = false;
        }

        // Створює/оновлює UI таблиці з заданою назвою
        private void BuildTableUI(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                ClearTableUI();
                return;
            }

            // Повністю очищаємо перед побудовою
            dgRows.Columns.Clear();
            dgRows.ItemsSource = null;

            var columnNames = _tableService.GetColumnNames(tableName);
            
            foreach (var colName in columnNames)
            {
                var column = _tableService.GetColumn(tableName, colName);
                if (column != null)
                {
                    var colType = column.Type.ToString();
                    
                    if (column.Type == DataType.TextFile)
                    {
                        // Для файлів - гіперпосилання
                        var dgCol = new DataGridTemplateColumn
                        {
                            Header = $"{colName} ({colType})",
                            CanUserSort = false
                        };
                        
                        var cellTemplate = new DataTemplate();
                        var factory = new FrameworkElementFactory(typeof(TextBlock));
                        factory.SetValue(TextBlock.ForegroundProperty, System.Windows.Media.Brushes.Blue);
                        factory.SetValue(TextBlock.TextDecorationsProperty, TextDecorations.Underline);
                        factory.SetValue(TextBlock.CursorProperty, System.Windows.Input.Cursors.Hand);
                        factory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding($"[{colName}].FileName"));
                        factory.SetValue(TextBlock.MarginProperty, new Thickness(5));
                        
                        factory.AddHandler(TextBlock.MouseLeftButtonUpEvent, new System.Windows.Input.MouseButtonEventHandler((s, e) =>
                        {
                            if (s is TextBlock tb && tb.DataContext is System.Collections.IDictionary row)
                            {
                                if (row[colName] is FileRecord fileRecord)
                                {
                                    OpenFileFromGrid(fileRecord);
                                }
                            }
                        }));
                        
                        cellTemplate.VisualTree = factory;
                        dgCol.CellTemplate = cellTemplate;
                        dgRows.Columns.Add(dgCol);
                    }
                    else
                    {
                        var dgCol = new DataGridTextColumn
                        {
                            Header = $"{colName} ({colType})",
                            Binding = new System.Windows.Data.Binding($"[{colName}]"),
                            CanUserSort = false
                        };
                        dgRows.Columns.Add(dgCol);
                    }
                }
            }
            
            var rows = _tableService.GetAllRows(tableName);
            dgRows.ItemsSource = rows;
            
            // Оновлюємо стан UI
            if (rows.Count == 0)
            {
                if (tbEmptyTable != null) tbEmptyTable.Visibility = Visibility.Visible;
            }
            else
            {
                if (tbEmptyTable != null) tbEmptyTable.Visibility = Visibility.Collapsed;
            }
            
            if (tbNoTable != null) tbNoTable.Visibility = Visibility.Collapsed;
            btnAddRow.IsEnabled = true;
            btnEditRow.IsEnabled = dgRows.SelectedIndex >= 0;
            btnDeleteRow.IsEnabled = dgRows.SelectedIndex >= 0;
            UpdateRenameColumnButtonState();
        }
        }
    }