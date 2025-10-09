// CoreLib/Services/TableService.cs
using CoreLib.Models;

namespace CoreLib.Services
{
    public class TableService
    {
        private readonly DatabaseService _databaseService;

        public TableService(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        public Table? GetTable(string tableName)
        {
            return _databaseService.GetTable(tableName);
        }

        public void CreateTable(string tableName, List<Column> columns)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            if (tableName.Contains(' ') || tableName.Contains(',') || tableName.Contains(';') || tableName.Contains(':'))
                throw new ArgumentException("Table name cannot contains symbols like ' ', ',', ';', ':'", nameof(tableName));

            if (columns == null || !columns.Any())
                throw new ArgumentException("Table must have at least one column", nameof(columns));

            var table = new Table(tableName, columns);
            _databaseService.CreateTable(table);
        }

        public bool DeleteTable(string tableName)
        {
            return _databaseService.DeleteTable(tableName);
        }

        public void AddRow(string tableName, Dictionary<string, object?> values)
        {
            var table = GetTableOrThrow(tableName);
            table.AddRow(values);
        }

        public void UpdateRow(string tableName, int rowIndex, Dictionary<string, object?> values)
        {
            var table = GetTableOrThrow(tableName);
            table.UpdateRow(rowIndex, values);
        }

        public bool DeleteRow(string tableName, int rowIndex)
        {
            var table = GetTableOrThrow(tableName);
            return table.DeleteRow(rowIndex);
        }

        public Dictionary<string, object?> GetRow(string tableName, int rowIndex)
        {
            var table = GetTable(tableName);
            return table?.GetRow(rowIndex) ?? new Dictionary<string, object?>();
        }

        public List<Dictionary<string, object?>> GetAllRows(string tableName)
        {
            var table = GetTable(tableName);
            return table?.GetAllRows() ?? new List<Dictionary<string, object?>>();
        }

        public bool RenameColumn(string tableName, string oldColumnName, string newColumnName)
        {
            var table = GetTable(tableName);
            return table?.RenameColumn(oldColumnName, newColumnName) ?? false;
        }

        public bool ReorderColumns(string tableName, List<string> newOrder)
        {
            var table = GetTable(tableName);
            return table?.ReorderColumns(newOrder) ?? false;
        }

        public List<string> GetColumnNames(string tableName)
        {
            var table = GetTable(tableName);
            return table?.GetColumnNames() ?? new List<string>();
        }

        public Column? GetColumn(string tableName, string columnName)
        {
            var table = GetTable(tableName);
            return table?.GetColumn(columnName);
        }

        public ValidationResult ValidateRow(string tableName, Dictionary<string, object?> values)
        {
            var table = GetTable(tableName);
            if (table == null)
                return ValidationResult.Failure($"Table '{tableName}' not found");

            return table.ValidateRow(values);
        }

        public ValidationResult ValidateColumnOperation(string tableName, string operationType,
            string? oldColumnName = null, string? newColumnName = null, List<string>? newOrder = null)
        {
            var result = new ValidationResult();
            var table = GetTable(tableName);

            if (table == null)
            {
                result.AddError($"Table '{tableName}' not found");
                return result;
            }

            switch (operationType.ToLowerInvariant())
            {
                case "rename":
                    if (string.IsNullOrWhiteSpace(oldColumnName))
                        result.AddError("Old column name is required for rename operation");
                    else if (table.GetColumn(oldColumnName) == null)
                        result.AddError($"Column '{oldColumnName}' not found in table");

                    if (string.IsNullOrWhiteSpace(newColumnName))
                        result.AddError("New column name is required for rename operation");
                    else if (table.GetColumn(newColumnName) != null)
                        result.AddError($"Column '{newColumnName}' already exists in table");
                    break;

                case "reorder":
                    if (newOrder == null || !newOrder.Any())
                    {
                        result.AddError("New column order is required for reorder operation");
                    }
                    else
                    {
                        var currentColumns = table.GetColumnNames();
                        if (newOrder.Count != currentColumns.Count)
                        {
                            result.AddError("New order must contain all existing columns");
                        }
                        else
                        {
                            var missingColumns = currentColumns.Except(newOrder, StringComparer.OrdinalIgnoreCase);
                            foreach (var missing in missingColumns)
                            {
                                result.AddError($"Column '{missing}' is missing from new order");
                            }
                        }
                    }
                    break;

                default:
                    result.AddError($"Unknown operation type: {operationType}");
                    break;
            }

            return result;
        }

        private Table GetTableOrThrow(string tableName)
        {
            var table = GetTable(tableName);
            if (table == null)
                throw new ArgumentException($"Table '{tableName}' not found");
            return table;
        }

        public ValidationResult ValidateTableCreation(string tableName, List<Column> columns)
        {
            var result = new ValidationResult();
            
            if (string.IsNullOrWhiteSpace(tableName))
                result.AddError("Column name cannot be null or empty");
            
            if (columns == null || columns.Count == 0)
                result.AddError("Column should contain at least one column");
            
            // Перевірка валідності імен колонок
            foreach (var column in columns)
            {
                try
                {
                    // Це викличе виняток, якщо ім'я невалідне
                    var testColumn = new Column(column.Name, column.Type);
                }
                catch (ArgumentException ex)
                {
                    result.AddError($"Incorrect column name '{column.Name}': {ex.Message}");
                }
            }
            
            // Перевірка унікальності назв колонок
            var duplicateColumns = columns
                .GroupBy(c => c.Name.ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
                
            if (duplicateColumns.Any())
                result.AddError($"Duplicate column names found: {string.Join(", ", duplicateColumns)}");
            
            return result;
        }

        public ValidationResult ValidateRowData(string tableName, Dictionary<string, object?> rowData)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(tableName))
            {
                result.AddError("Table name is required");
                return result;
            }

            var table = GetTable(tableName);
            if (table == null)
            {
                result.AddError($"Table '{tableName}' not found");
                return result;
            }

            return table.ValidateRow(rowData);
        }

        public ValidationResult ValidateColumnName(string columnName)
        {
            var result = new ValidationResult();
            
            try
            {
                var testColumn = new Column(columnName, DataType.String); // Тип не має значення для валідації імені
            }
            catch (ArgumentException ex)
            {
                result.AddError(ex.Message);
            }
            
            return result;
        }

        public (bool Success, string Error) TryRenameColumn(string tableName, string oldName, string newName)
        {
            try
            {
                var table = GetTable(tableName);
                if (table == null)
                    return (false, $"Table '{tableName}' not found");

                // Спочатку перевіряємо нове ім'я
                var nameValidation = ValidateColumnName(newName);
                if (!nameValidation.IsValid)
                    return (false, string.Join(", ", nameValidation.Errors));

                // Потім виконуємо перейменування
                var result = table.RenameColumn(oldName, newName);
                if (!result)
                    return (false, "Couldn't rename the column.");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Renaming column failed: {ex.Message}");
            }
        }

        public (Dictionary<string, object?> Values, ValidationResult Validation) ParseAndValidateRowData(
        string tableName,
        Dictionary<string, object?> rawData,
        Dictionary<string, FileRecord?> fileRecords)
        {
            var values = new Dictionary<string, object?>();
            var validation = new ValidationResult();

            var table = GetTable(tableName);
            if (table == null)
            {
                validation.AddError($"Table '{tableName}' isn't found");
                return (values, validation);
            }

            foreach (var column in table.Columns)
            {
                try
                {
                    object? value = null;

                    if (column.Type == DataType.TextFile)
                    {
                        value = fileRecords.ContainsKey(column.Name) ? fileRecords[column.Name] : null;
                        if (value == null)
                        {
                            throw new ArgumentException($"You should select a file for column '{column.Name}'");
                        }
                        if (value is FileRecord fileRecord)
                        {
                            if (string.IsNullOrWhiteSpace(fileRecord.StoragePath) &&
                            (fileRecord.Content == null || fileRecord.Content.Length == 0))
                            {
                                throw new ArgumentException($"You should select a file for column '{column.Name}'");
                            }
                        }
                    }
                    else if (column.Type == DataType.IntegerInterval)
                    {
                        // Спеціальна обробка для IntegerInterval з двох полів
                        value = ParseIntegerIntervalValue(rawData, column.Name);
                    }
                    else if (rawData.ContainsKey(column.Name))
                    {
                        value = ParseSingleValue(column, rawData[column.Name]);
                    }

                    values[column.Name] = value;
                }
                catch (Exception ex)
                {
                    validation.AddError($"Error parsing value for column '{column.Name}': {ex.Message}");
                }
            }
            if (validation.IsValid)
            {
                var rowValidation = table.ValidateRow(values);
                validation.Merge(rowValidation);
            }

            return (values, validation);
        }
        
        private object? ParseIntegerIntervalValue(Dictionary<string, object?> rawData, string columnName)
        {
            // Шукаємо дані для IntegerInterval у форматі { Min: "...", Max: "..." }
            if (rawData.TryGetValue(columnName, out var intervalData) && intervalData != null)
            {
                // Використовуємо рефлексію для отримання значень Min і Max
                var type = intervalData.GetType();
                var minProperty = type.GetProperty("Min");
                var maxProperty = type.GetProperty("Max");

                if (type == typeof(IntegerInterval))
                {
                    return intervalData as IntegerInterval;
                }

                if (minProperty != null && maxProperty != null)
                {
                    var minStr = minProperty.GetValue(intervalData) as string;
                    var maxStr = maxProperty.GetValue(intervalData) as string;

                    if (!string.IsNullOrWhiteSpace(minStr) && !string.IsNullOrWhiteSpace(maxStr))
                    {
                        if (int.TryParse(minStr, out int min) && int.TryParse(maxStr, out int max))
                        {
                            return new IntegerInterval(min, max);
                        }
                        else
                        {
                            throw new ArgumentException($"Incorrect values for integer interval. Integer numbers expected.");
                        }
                    }
                    else
                    if (string.IsNullOrWhiteSpace(minStr) && string.IsNullOrWhiteSpace(maxStr))
                    {
                        throw new ArgumentException($"Min and max values cannot be empty");
                    }
                    else if (string.IsNullOrWhiteSpace(minStr))
                    {
                        throw new ArgumentException($"Min value cannot be empty");
                    }
                    else if (string.IsNullOrWhiteSpace(maxStr))
                    {
                        throw new ArgumentException($"Max value cannot be empty");
                    }
                }
            }
            return null;
        }

        private object? ParseSingleValue(Column column, object? rawValue)
        {
            if (rawValue == null)
            {
                throw new ArgumentException($"Value cannot be null");
            }

            var stringValue = rawValue.ToString();
            if (string.IsNullOrWhiteSpace(stringValue) && (column.Type != DataType.String))
            {
                throw new ArgumentException($"Value cannot be empty");
            }

            switch (column.Type)
            {
                case DataType.Integer:
                    if (int.TryParse(stringValue, out int intValue))
                        return intValue;
                    else
                        throw new ArgumentException($"Incorrect integer number: {stringValue}");

                case DataType.Real:
                    if (double.TryParse(stringValue, out double realValue))
                        return realValue;
                    else
                        throw new ArgumentException($"Incorrect real number: {stringValue}");

                case DataType.Char:
                    if (stringValue.Length == 1)
                        return stringValue[0];
                    else
                        throw new ArgumentException($"Should be a single character: {stringValue}");

                case DataType.String:
                    return stringValue;

                default:
                    return stringValue;
            }
        }
    }
}