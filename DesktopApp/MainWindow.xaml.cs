using CoreLib.Services;
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
        }

        private void BtnCreateDb_Click(object sender, RoutedEventArgs e)
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("Введіть назву бази:", "Створення бази");
            if (string.IsNullOrWhiteSpace(name)) return;
            _databaseService.CreateDatabase(name);
            UpdateTablesList();
            UpdateCurrentDbName();
            MessageBox.Show($"Базу '{name}' створено.");
        }

        private async void BtnOpenDb_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "JSON файли (*.json)|*.json|Всі файли|*.*" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    await _databaseService.LoadDatabaseAsync(dlg.FileName);
                    UpdateTablesList();
                    UpdateCurrentDbName();
                    MessageBox.Show("Базу завантажено.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
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
            var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "JSON файли (*.json)|*.json|Всі файли|*.*" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    await _databaseService.SaveDatabaseAsync(dlg.FileName);
                    MessageBox.Show("Базу збережено.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            }
        }

        private void BtnAddTable_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new TableDialog();
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
                    MessageBox.Show($"Помилка: {ex.Message}");
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
            var columns = _tableService.GetColumnNames(tableName);
            var rowDialog = new RowDialog(columns);
            if (rowDialog.ShowDialog() == true)
            {
                var values = rowDialog.Values;
                var validation = _tableService.ValidateRow(tableName, values);
                if (!validation.IsValid)
                {
                    MessageBox.Show($"Помилка: {string.Join(", ", validation.Errors)}");
                    return;
                }
                _tableService.AddRow(tableName, values);
                UpdateRowsGrid(tableName);
            }
        }

        private void BtnEditRow_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is not string tableName) return;
            if (dgRows.SelectedItem is not System.Collections.IDictionary row) return;
            int rowIndex = dgRows.SelectedIndex;
            var columns = _tableService.GetColumnNames(tableName);
            var oldValues = columns.ToDictionary(c => c, c => row[c]);
            var rowDialog = new RowDialog(columns, oldValues);
            if (rowDialog.ShowDialog() == true)
            {
                var values = rowDialog.Values;
                var validation = _tableService.ValidateRow(tableName, values);
                if (!validation.IsValid)
                {
                    MessageBox.Show($"Помилка: {string.Join(", ", validation.Errors)}");
                    return;
                }
                _tableService.UpdateRow(tableName, rowIndex, values);
                UpdateRowsGrid(tableName);
            }
        }

        private void BtnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is not string tableName) return;
            if (dgRows.SelectedIndex < 0) return;
            _tableService.DeleteRow(tableName, dgRows.SelectedIndex);
            UpdateRowsGrid(tableName);
        }

        private void LstTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstTables.SelectedItem is string tableName)
            {
                UpdateRowsGrid(tableName);
                UpdateRenameColumnButtonState();
            }
            else
            {
                UpdateRowsGrid(null);
                UpdateRenameColumnButtonState();
            }
        }

        // Додаткові функції для перейменування та перестановки колонок
        private void BtnRenameColumn_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is not string tableName) return;
            if (dgRows.CurrentCell == null || dgRows.CurrentCell.Column == null) return;
            
            var header = dgRows.CurrentCell.Column.Header?.ToString();
            if (string.IsNullOrWhiteSpace(header)) return;
            
            var oldName = header.Split('(')[0].Trim();
            
            // ✅ ЗБЕРІГАЄМО ПОТОЧНИЙ ПОРЯДОК КОЛОНОК
            var currentOrder = dgRows.Columns
                .Select(c => c.Header?.ToString()?.Split('(')[0].Trim())
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .ToList();
                
            var dlg = new RenameColumnDialog(oldName);
            if (dlg.ShowDialog() == true)
            {
                var newName = dlg.NewColumnName;
                if (string.IsNullOrWhiteSpace(newName)) return;
                
                
                var result = _tableService.RenameColumn(tableName, oldName, newName);
                if (!result)
                {
                    MessageBox.Show("Не вдалося перейменувати колонку.");
                }
                else
                {
                    // ✅ ОНОВЛЮЄМО ПОРЯДОК (замінюємо стару назву на нову)
                    var newOrder = currentOrder.Select(n => n == oldName ? newName : n).ToList();
                    _tableService.ReorderColumns(tableName, newOrder);
                    
                    UpdateRowsGrid(tableName);
                }
            }
        }

       private void DgRows_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            UpdateRenameColumnButtonState();
        }

        private void BtnReorderColumns_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is not string tableName) return;
            var columns = _tableService.GetColumnNames(tableName);
            var newOrder = Microsoft.VisualBasic.Interaction.InputBox($"Введіть новий порядок колонок через кому:\n{string.Join(", ", columns)}", "Перестановка колонок");
            if (string.IsNullOrWhiteSpace(newOrder)) return;
            var orderList = newOrder.Split(',').Select(s => s.Trim()).ToList();
            var result = _tableService.ReorderColumns(tableName, orderList);
            if (!result)
                MessageBox.Show("Не вдалося змінити порядок колонок.");
            else
                UpdateRowsGrid(tableName);
        }

        // Оновлення списку таблиць
        private void UpdateTablesList()
        {
            lstTables.ItemsSource = _databaseService.GetTableNames();
            if (lstTables.Items.Count > 0)
                lstTables.SelectedIndex = 0;
            UpdateCurrentDbName();
        }

        private void UpdateCurrentDbName()
        {
            if (tbCurrentDbName == null) return;
            var db = _databaseService.CurrentDatabase;
            tbCurrentDbName.Text = db != null ? $"База: {db.Name}" : "(База не вибрана)";
        }

        // Оновлення DataGrid з рядками
        private void UpdateRowsGrid(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                dgRows.Columns.Clear();
                dgRows.ItemsSource = null;
                if (tbNoTable != null) tbNoTable.Visibility = Visibility.Visible;
                if (tbEmptyTable != null) tbEmptyTable.Visibility = Visibility.Collapsed;
                btnAddRow.IsEnabled = false;
                btnEditRow.IsEnabled = false;
                btnDeleteRow.IsEnabled = false;
                return;
            }
            // Формуємо колонки згідно з таблицею
            dgRows.Columns.Clear();
            var columns = _tableService.GetColumnNames(tableName);
            var table = _tableService.GetTable(tableName);
            if (table != null)
            {
                foreach (var col in table.Columns)
                {
                    var colName = col.Name;
                    var colType = col.Type.ToString();
                    var dgCol = new System.Windows.Controls.DataGridTextColumn
                    {
                        Header = $"{colName} ({colType})",
                        Binding = new System.Windows.Data.Binding($"[{colName}]"),
                        CanUserSort = false
                    };
                    dgRows.Columns.Add(dgCol);
                }
            }
            var rows = _tableService.GetAllRows(tableName);
            dgRows.ItemsSource = rows;
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
        }
    }