using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CoreLib.Models
{
    public class Column
    {
        private string _name = string.Empty;

        public string Name 
        { 
            get => _name;
            set
            {
                if (!IsValidColumnName(value))
                {
                    throw new ArgumentException(
                        "Column name can only contain letters, numbers, and underscores, " +
                        "cannot contain spaces or special characters, and cannot be empty.");
                }
                _name = value;
            }
        }
        
        public DataType Type { get; set; }
        public List<object?> Values { get; set; } = new List<object?>();
        public bool IsRequired { get; set; } = true;

        [JsonConstructor]
        public Column() { }

        public Column(string name, DataType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
        }

        /// <summary>
        /// Validates if a column name is safe for WPF usage
        /// </summary>
        public static bool IsValidColumnName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Allow only letters, numbers, and underscores
            // No spaces, no special characters that could cause WPF issues
            return Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
        }

        /// <summary>
        /// Sanitizes a column name for safe WPF usage
        /// Replaces invalid characters with underscores
        /// </summary>
        public static string SanitizeColumnName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Column";

            // Replace spaces and special characters with underscores
            string sanitized = Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
            
            // Ensure it starts with a letter or underscore
            if (!Regex.IsMatch(sanitized, @"^[a-zA-Z_]"))
            {
                sanitized = "_" + sanitized;
            }

            // Ensure it's not empty after sanitization
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "Column";
            }

            return sanitized;
        }

        public object? GetValue(int rowIndex)
        {
            return rowIndex >= 0 && rowIndex < Values.Count ? Values[rowIndex] : null;
        }

        public void SetValue(int rowIndex, object? value)
        {
            // Extend list if necessary
            while (Values.Count <= rowIndex)
            {
                Values.Add(null);
            }

            Values[rowIndex] = value;
        }

        public void RemoveValue(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < Values.Count)
            {
                Values.RemoveAt(rowIndex);
            }
        }

        public void InsertValue(int rowIndex, object? value)
        {
            if (rowIndex < 0) rowIndex = 0;
            if (rowIndex >= Values.Count)
            {
                Values.Add(value);
            }
            else
            {
                Values.Insert(rowIndex, value);
            }
        }

        public int GetRowCount()
        {
            return Values.Count;
        }

        public ValidationResult ValidateValue(object? value)
        {
            var result = new ValidationResult();

            if (IsRequired && value == null)
            {
                result.AddError($"Column '{Name}' is required and cannot be null");
                return result;
            }

            if (Type == DataType.TextFile && value is FileRecord fileRecord)
            {
                if (IsRequired && string.IsNullOrWhiteSpace(fileRecord.StoragePath) && 
                    (fileRecord.Content == null || fileRecord.Content.Length == 0))
                {
                    result.AddError($"You should select a file for column '{Name}'");
                    return result;
                }
            }

            if (value == null)
                return result; // Null is generally allowed

            // Type-specific validation
            switch (Type)
            {
                case DataType.Integer:
                    if (value is not int && !int.TryParse(value.ToString(), out _))
                        result.AddError($"Column '{Name}' must be an integer");
                    break;

                case DataType.Real:
                    if (value is not double && value is not float &&
                        !double.TryParse(value.ToString(), out _))
                        result.AddError($"Column '{Name}' must be a real number");
                    break;

                case DataType.Char:
                    var charStr = value.ToString();
                    if (string.IsNullOrEmpty(charStr) || charStr.Length != 1)
                        result.AddError($"Column '{Name}' must be a single character");
                    break;

                case DataType.String:
                    // Any object can be converted to string
                    break;

                case DataType.TextFile:
                    if (value is not FileRecord)
                        result.AddError($"Column '{Name}' must be a file record");
                    break;

                case DataType.IntegerInterval:
                    if (value is not IntegerInterval)
                    {
                        result.AddError($"Column '{Name}' must be a valid integer interval");
                    }
                    else
                    {
                        var interval = (IntegerInterval)value;
                        if (interval.Min > interval.Max)
                        {
                            result.AddError($"Column '{Name}': Min value cannot be greater than Max value");
                        }
                    }
                    break;
            }

            return result;
        }
    }
}