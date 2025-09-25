using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreLib.Models;

namespace CoreLib.Common
{
    public static class DatabaseUtilities
    {
        public static string GenerateUniqueTableName(Database database, string baseName = "Table")
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            var existingNames = database.GetTableNames().ToHashSet(StringComparer.OrdinalIgnoreCase);
            
            if (!existingNames.Contains(baseName))
                return baseName;

            var counter = 1;
            string newName;
            do
            {
                newName = $"{baseName}{counter}";
                counter++;
            } while (existingNames.Contains(newName));

            return newName;
        }

        public static string GenerateUniqueColumnName(Table table, string baseName = "Column")
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var existingNames = table.GetColumnNames().ToHashSet(StringComparer.OrdinalIgnoreCase);
            
            if (!existingNames.Contains(baseName))
                return baseName;

            var counter = 1;
            string newName;
            do
            {
                newName = $"{baseName}{counter}";
                counter++;
            } while (existingNames.Contains(newName));

            return newName;
        }

        public static bool IsValidDatabaseFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return false;

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == Constants.DefaultDatabaseExtension ||
                   extension == Constants.DefaultJsonExtension ||
                   extension == ".json";
        }

        public static string GetDatabaseFileFilter()
        {
            return "Database Files|*.dbm;*.json|" +
                   "Database Manager Files (*.dbm)|*.dbm|" +
                   "JSON Database Files (*.json)|*.json|" +
                   "All Files|*.*";
        }

        public static DataType GetRecommendedDataType(object? value)
        {
            return value switch
            {
                null => DataType.String,
                int => DataType.Integer,
                double or float or decimal => DataType.Real,
                char => DataType.Char,
                string s when s.Length == 1 => DataType.Char,
                string => DataType.String,
                FileRecord => DataType.TextFile,
                IntegerInterval => DataType.IntegerInterval,
                _ => DataType.String
            };
        }

        public static string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";

            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:F2} {suffixes[suffixIndex]}";
        }

        public static Table CreateSampleTable(string tableName)
        {
            var columns = new List<Column>
            {
                new Column("ID", DataType.Integer),
                new Column("Name", DataType.String),
                new Column("Age", DataType.Integer),
                new Column("Score", DataType.Real)
            };

            var table = new Table(tableName, columns);
            
            // Add sample data
            table.AddRow(new Dictionary<string, object?> 
            { 
                { "ID", 1 }, 
                { "Name", "John Doe" }, 
                { "Age", 25 }, 
                { "Score", 95.5 } 
            });
            table.AddRow(new Dictionary<string, object?> 
            { 
                { "ID", 2 }, 
                { "Name", "Jane Smith" }, 
                { "Age", 30 }, 
                { "Score", 87.2 } 
            });

            return table;
        }
    }
}