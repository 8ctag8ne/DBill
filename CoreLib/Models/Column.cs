//CoreLib/Models/Column.cs
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoreLib.Models
{
    public class Column
    {
        public string Name { get; set; } = string.Empty;
        public DataType Type { get; set; }
        public List<object?> Values { get; set; } = new List<object?>();

        [JsonConstructor]
        public Column() { }

        public Column(string name, DataType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
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

            if (Type == DataType.TextFile && value is FileRecord fileRecord)
            {
                if (string.IsNullOrWhiteSpace(fileRecord.StoragePath) && 
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
                    }
                    break;
            }

            return result;
        }
        
        // public (object? Value, string Error) ParseValueFromString(string input)
        // {
        //     if (string.IsNullOrWhiteSpace(input))
        //         return (null, "Value cannot be null or empty");

        //     try
        //         {
        //             switch (Type)
        //             {
        //                 case DataType.Integer:
        //                     if (int.TryParse(input, out int intValue))
        //                         return (intValue, null);
        //                     else
        //                         return (null, $"Incorrect integer number: {input}");

        //                 case DataType.Real:
        //                     if (double.TryParse(input, out double realValue))
        //                         return (realValue, null);
        //                     else
        //                         return (null, $"Incorrect real number: {input}");

        //                 case DataType.Char:
        //                     if (input.Length == 1)
        //                         return (input[0], null);
        //                     else
        //                         return (null, $"Should be a single character: {input}");

        //                 case DataType.String:
        //                     return (input, null);

        //                 case DataType.IntegerInterval:
        //                     // Логіка парсингу інтервалу
        //                     var parts = input.Split('-');
        //                     if (parts.Length == 2 &&
        //                         int.TryParse(parts[0], out int min) &&
        //                         int.TryParse(parts[1], out int max))
        //                         return (new IntegerInterval(min, max), null);
        //                     else
        //                         return (null, $"Incorrect integer interval: {input}");

        //                 default:
        //                     return (input, null);
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             return (null, $"Parsing error: {ex.Message}");
        //         }
        // }
    }
}