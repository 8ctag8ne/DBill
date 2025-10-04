// using CoreLib.Services;
// using CoreLib.Models;
// using System.IO;
// using System.Windows;
// using System.Windows.Controls;
// using System.Windows.Controls.Primitives;
// using System.Windows.Media;
// using DataGrid = System.Windows.Controls.DataGrid;

// namespace DBill.WpfApp
// {
//     public partial class MainWindow : Window
//     {
//         private readonly DatabaseService _databaseService;
//         private readonly TableService _tableService;
//         private readonly FileService _fileService;

//         protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
//         {
//             base.OnClosing(e);

//             // Очищаємо тимчасові файли при закритті
//             try
//             {
//                 await _databaseService.CloseDatabase();
//             }
//             catch
//             {
//                 // Ігноруємо помилки очищення
//             }
//         }

//         // Активує кнопку перейменування при кліку на хедер
//         private void DgRows_Sorting(object sender, DataGridSortingEventArgs e)
//         {
//             // Вимикаємо сортування
//             e.Handled = true;
//             // Активуємо кнопку і зберігаємо вибрану колонку
//             dgRows.CurrentCell = new DataGridCellInfo(dgRows.Items[0], e.Column);
//             UpdateRenameColumnButtonState();
//         }
//         private void DgRows_HeaderClick(object sender, RoutedEventArgs e)
//         {
//             // MessageBox.Show("HeaderClick event triggered");
//             if (e.OriginalSource is DataGridColumnHeader header && header.Column != null)
//             {
//                 // MessageBox.Show($"Column header clicked: {header.Column.Header}");
//                 dgRows.CurrentCell = new DataGridCellInfo(dgRows.Items.Count > 0 ? dgRows.Items[0] : null, header.Column);
//                 UpdateRenameColumnButtonState();
//             }
//             else
//             {
//                 // MessageBox.Show("Clicked, but not on column header");
//             }
//         }

//         private void UpdateDatabaseButtonsState()
//         {
//             bool hasDatabaseLoaded = _databaseService.CurrentDatabase != null;

//             btnAddTable.IsEnabled = hasDatabaseLoaded;
//             btnSaveDb.IsEnabled = hasDatabaseLoaded;
//         }

//         private void UpdateTableButtonsState()
//         {
//             bool hasTableSelected = lstTables.SelectedItem != null;
//             btnDeleteTable.IsEnabled = hasTableSelected;
//         }

//         public MainWindow(DatabaseService dbService, TableService tableService, FileService fileService)
//         {
//             InitializeComponent();
//             _databaseService = dbService;
//             _tableService = tableService;
//             _fileService = fileService;

//             // Додаємо прямий обробник кліку на хедер
//             dgRows.LoadingRow += (s, e) =>
//             {
//                 // MessageBox.Show($"Loading row {e.Row.GetIndex()}");
//                 if (e.Row.Header is DataGridRowHeader header)
//                 {
//                     header.Click += DgRows_HeaderClick;
//                 }
//             };

//             // Додаємо обробник кліку безпосередньо на DataGrid
//             dgRows.PreviewMouseLeftButtonUp += (s, e) =>
//             {
//                 var originalSource = e.OriginalSource as DependencyObject;
//                 while (originalSource != null && !(originalSource is DataGridColumnHeader))
//                 {
//                     originalSource = VisualTreeHelper.GetParent(originalSource);
//                 }

//                 if (originalSource is DataGridColumnHeader columnHeader)
//                 {
//                     // MessageBox.Show($"Column header clicked via PreviewMouseLeftButtonUp: {columnHeader.Column.Header}");
//                     dgRows.CurrentCell = new DataGridCellInfo(dgRows.Items.Count > 0 ? dgRows.Items[0] : null, columnHeader.Column);
//                     UpdateRenameColumnButtonState();
//                 }
//             };

//             dgRows.ColumnReordered += DgRows_ColumnReordered;

//             btnCreateDb.Click += BtnCreateDb_Click;
//             btnOpenDb.Click += BtnOpenDb_Click;
//             btnSaveDb.Click += BtnSaveDb_Click;
//             btnAddTable.Click += BtnAddTable_Click;
//             btnDeleteTable.Click += BtnDeleteTable_Click;
//             btnAddRow.Click += BtnAddRow_Click;
//             btnEditRow.Click += BtnEditRow_Click;
//             btnDeleteRow.Click += BtnDeleteRow_Click;
//             lstTables.SelectionChanged += LstTables_SelectionChanged;
//             dgRows.Sorting += DgRows_Sorting;

//             UpdateCurrentDbName();
//             UpdateDatabaseButtonsState();
//             UpdateTableButtonsState();
//         }

//         ...

//         private async void BtnCreateDb_Click(object sender, RoutedEventArgs e)
//         {
//             // Очищаємо файли попередньої бази
//             await _databaseService.CloseDatabase();

//             var name = Microsoft.VisualBasic.Interaction.InputBox("Введіть назву бази:", "Створення бази");
//             if (string.IsNullOrWhiteSpace(name)) return;

//             _databaseService.CreateDatabase(name);
//             UpdateTablesList();
//             UpdateCurrentDbName();
//             UpdateDatabaseButtonsState();
//             MessageBox.Show($"Базу '{name}' створено.");
//         }

//         private async void BtnOpenDb_Click(object sender, RoutedEventArgs e)
//         {
//             var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "JSON файли (*.json)|*.json|Всі файли|*.*" };
//             if (dlg.ShowDialog() == true)
//             {
//                 try
//                 {
//                     // Очищаємо файли попередньої бази
//                     await _databaseService.CloseDatabase();

//                     await _databaseService.LoadDatabaseAsync(dlg.FileName);
//                     UpdateTablesList();
//                     UpdateCurrentDbName();
//                     UpdateDatabaseButtonsState();
//                     MessageBox.Show("Базу завантажено.");
//                 }
//                 catch (Exception ex)
//                 {
//                     MessageBox.Show($"Помилка: {ex.Message}");
//                 }
//             }
//         }

//         ...

//         private async void BtnSaveDb_Click(object sender, RoutedEventArgs e)
//         {
//             var dlg = new Microsoft.Win32.SaveFileDialog { Filter = "JSON файли (*.json)|*.json|Всі файли|*.*" };
//             if (dlg.ShowDialog() == true)
//             {
//                 try
//                 {
//                     await _databaseService.SaveDatabaseAsync(dlg.FileName);
//                     MessageBox.Show("Базу збережено.");
//                 }
//                 catch (Exception ex)
//                 {
//                     MessageBox.Show($"Помилка: {ex.Message}");
//                 }
//             }
//         }

//         // Оновлення списку таблиць
//         private void UpdateTablesList()
//         {
//             lstTables.ItemsSource = _databaseService.GetTableNames();
//             if (lstTables.Items.Count > 0)
//                 lstTables.SelectedIndex = 0;
//             UpdateCurrentDbName();
//             UpdateTableButtonsState();
//         }

//         private void UpdateCurrentDbName()
//         {
//             if (tbCurrentDbName == null) return;
//             var db = _databaseService.CurrentDatabase;
//             tbCurrentDbName.Text = db != null ? $"База: {db.Name}" : "(База не вибрана)";
//         }


//         private void OpenFileFromGrid(FileRecord fileRecord)
//         {
//             try
//             {
//                 System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
//                 {
//                     FileName = "notepad.exe",
//                     Arguments = $"\"{fileRecord.StoragePath}\"",
//                     UseShellExecute = false
//                 });
//             }
//             catch (Exception ex)
//             {
//                 MessageBox.Show($"Помилка відкриття файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
//             }
//         }
//         ...
//         }
//     }