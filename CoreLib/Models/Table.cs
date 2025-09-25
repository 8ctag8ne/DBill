using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace CoreLib.Models
{
    public class Table
    {
        public string Name { get; set; } = string.Empty;
        public List<Column> Columns { get; set; } = new List<Column>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        [JsonConstructor]
        public Table() { }

        public Table(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public Table(string name, List<Column> columns) : this(name)
        {
            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        }

        public void AddColumn(Column column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (Columns.Any(c => c.Name.Equals(column.Name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Column '{column.Name}' already exists in table");

            // Extend new column to match existing row count
            var rowCount = GetRowCount();
            while (column.Values.Count < rowCount)
            {
                column.Values.Add(null);
            }

            Columns.Add(column);
            ModifiedAt = DateTime.UtcNow;
        }

        public bool RemoveColumn(string columnName)
        {
            var column = GetColumn(columnName);
            if (column == null)
                return false;

            Columns.Remove(column);
            ModifiedAt = DateTime.UtcNow;
            return true;
        }

        public Column? GetColumn(string columnName)
        {
            return Columns.FirstOrDefault(c => 
                c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }

        public int GetColumnIndex(string columnName)
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        public List<string> GetColumnNames()
        {
            return Columns.Select(c => c.Name).ToList();
        }

        public int GetRowCount()
        {
            return Columns.Count > 0 ? Columns.Max(c => c.GetRowCount()) : 0;
        }

        public void AddRow(Dictionary<string, object?> values)
        {
            var rowIndex = GetRowCount();
            
            foreach (var column in Columns)
            {
                var value = values.TryGetValue(column.Name, out var val) ? val : null;
                column.SetValue(rowIndex, value);
            }

            ModifiedAt = DateTime.UtcNow;
        }

        public void UpdateRow(int rowIndex, Dictionary<string, object?> values)
        {
            if (rowIndex < 0 || rowIndex >= GetRowCount())
                throw new ArgumentOutOfRangeException(nameof(rowIndex));

            foreach (var column in Columns)
            {
                if (values.TryGetValue(column.Name, out var value))
                {
                    column.SetValue(rowIndex, value);
                }
            }

            ModifiedAt = DateTime.UtcNow;
        }

        public bool DeleteRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= GetRowCount())
                return false;

            foreach (var column in Columns)
            {
                column.RemoveValue(rowIndex);
            }

            ModifiedAt = DateTime.UtcNow;
            return true;
        }

        public Dictionary<string, object?> GetRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= GetRowCount())
                return new Dictionary<string, object?>();

            var row = new Dictionary<string, object?>();
            foreach (var column in Columns)
            {
                row[column.Name] = column.GetValue(rowIndex);
            }
            return row;
        }

        public List<Dictionary<string, object?>> GetAllRows()
        {
            var rows = new List<Dictionary<string, object?>>();
            var rowCount = GetRowCount();
            
            for (int i = 0; i < rowCount; i++)
            {
                rows.Add(GetRow(i));
            }
            
            return rows;
        }

        public bool RenameColumn(string oldName, string newName)
        {
            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
                return false;

            if (oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
                return true; // No change needed

            var column = GetColumn(oldName);
            if (column == null)
                return false;

            // Check if new name already exists
            if (Columns.Any(c => c.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Simply change the column name - O(1) operation!
            column.Name = newName;
            ModifiedAt = DateTime.UtcNow;
            return true;
        }

        public bool ReorderColumns(List<string> newOrder)
        {
            if (newOrder == null || newOrder.Count != Columns.Count)
                return false;

            // Verify all columns exist
            var currentColumnNames = Columns.Select(c => c.Name).OrderBy(n => n).ToList();
            var newOrderNames = newOrder.OrderBy(n => n).ToList();
            
            if (!currentColumnNames.SequenceEqual(newOrderNames, StringComparer.OrdinalIgnoreCase))
                return false;

            // Create new column list in the specified order - O(n) operation!
            var reorderedColumns = new List<Column>();
            foreach (var columnName in newOrder)
            {
                var column = GetColumn(columnName);
                if (column != null)
                {
                    reorderedColumns.Add(column);
                }
            }

            Columns = reorderedColumns;
            ModifiedAt = DateTime.UtcNow;
            return true;
        }

        public ValidationResult ValidateRow(Dictionary<string, object?> rowValues)
        {
            var result = new ValidationResult();

            if (rowValues == null)
            {
                result.AddError("Row values cannot be null");
                return result;
            }

            // Validate each column
            foreach (var column in Columns)
            {
                var value = rowValues.TryGetValue(column.Name, out var val) ? val : null;
                var columnValidation = column.ValidateValue(value);
                result.Merge(columnValidation);
            }

            return result;
        }
    }
}