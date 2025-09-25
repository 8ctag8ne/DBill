using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace CoreLib.Models
{
    public class Database
    {
        public string Name { get; set; } = string.Empty;
        public List<Table> Tables { get; set; } = new List<Table>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.0";

        [JsonConstructor]
        public Database() { }

        public Database(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public void AddTable(Table table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (Tables.Any(t => t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Table '{table.Name}' already exists in database");

            Tables.Add(table);
            ModifiedAt = DateTime.UtcNow;
        }

        public bool RemoveTable(string tableName)
        {
            var table = GetTable(tableName);
            if (table == null)
                return false;

            Tables.Remove(table);
            ModifiedAt = DateTime.UtcNow;
            return true;
        }

        public Table? GetTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return null;

            return Tables.FirstOrDefault(t => 
                t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        }

        public List<Table> GetAllTables()
        {
            return Tables.ToList();
        }

        public List<string> GetTableNames()
        {
            return Tables.Select(t => t.Name).OrderBy(n => n).ToList();
        }

        public bool TableExists(string tableName)
        {
            return GetTable(tableName) != null;
        }

        public ValidationResult Validate()
        {
            var result = new ValidationResult();

            // Check database name
            if (string.IsNullOrWhiteSpace(Name))
                result.AddError("Database name is required");

            // Check for duplicate table names
            var tableNames = Tables.Select(t => t.Name.ToLowerInvariant()).ToList();
            var duplicates = tableNames.GroupBy(n => n)
                                     .Where(g => g.Count() > 1)
                                     .Select(g => g.Key);

            foreach (var duplicate in duplicates)
            {
                result.AddError($"Duplicate table name found: '{duplicate}'");
            }

            // Validate each table
            foreach (var table in Tables)
            {
                if (string.IsNullOrWhiteSpace(table.Name))
                {
                    result.AddError("Table name is required");
                    continue;
                }

                // Check for duplicate column names within table
                var columnNames = table.Columns.Select(c => c.Name.ToLowerInvariant()).ToList();
                var columnDuplicates = columnNames.GroupBy(n => n)
                                                 .Where(g => g.Count() > 1)
                                                 .Select(g => g.Key);

                foreach (var duplicate in columnDuplicates)
                {
                    result.AddError($"Duplicate column name in table '{table.Name}': '{duplicate}'");
                }

                // Validate data consistency across columns
                var rowCount = table.GetRowCount();
                foreach (var column in table.Columns)
                {
                    if (column.GetRowCount() != rowCount)
                    {
                        result.AddError($"Column '{column.Name}' in table '{table.Name}' has inconsistent row count");
                    }

                    // Validate each value in the column
                    for (int i = 0; i < column.GetRowCount(); i++)
                    {
                        var value = column.GetValue(i);
                        var valueValidation = column.ValidateValue(value);
                        if (!valueValidation.IsValid)
                        {
                            result.AddError($"Invalid value in column '{column.Name}', row {i}: {string.Join(", ", valueValidation.Errors)}");
                        }
                    }
                }
            }

            return result;
        }

        public DatabaseStatistics GetStatistics()
        {
            return new DatabaseStatistics
            {
                DatabaseName = Name,
                TableCount = Tables.Count,
                TotalRows = Tables.Sum(t => t.GetRowCount()),
                TotalColumns = Tables.Sum(t => t.Columns.Count),
                CreatedAt = CreatedAt,
                ModifiedAt = ModifiedAt,
                TableStatistics = Tables.Select(t => new TableStatistics
                {
                    TableName = t.Name,
                    RowCount = t.GetRowCount(),
                    ColumnCount = t.Columns.Count,
                    CreatedAt = t.CreatedAt,
                    ModifiedAt = t.ModifiedAt
                }).ToList()
            };
        }
    }

    public class DatabaseStatistics
    {
        public string DatabaseName { get; set; } = string.Empty;
        public int TableCount { get; set; }
        public int TotalRows { get; set; }
        public int TotalColumns { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public List<TableStatistics> TableStatistics { get; set; } = new List<TableStatistics>();
    }

    public class TableStatistics
    {
        public string TableName { get; set; } = string.Empty;
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}