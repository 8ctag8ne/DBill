using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}