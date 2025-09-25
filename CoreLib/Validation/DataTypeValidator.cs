using System;
using System.Globalization;
using CoreLib.Models;

namespace CoreLib.Validation
{
    public class DataTypeValidator : IDataTypeValidator
    {
        public ValidationResult ValidateValue(object? value, DataType dataType)
        {
            var result = new ValidationResult();

            // Null values are generally allowed
            if (value == null)
                return result;

            switch (dataType)
            {
                case DataType.Integer:
                    if (value is not int && !int.TryParse(value.ToString(), out _))
                        result.AddError($"Value '{value}' is not a valid integer");
                    break;

                case DataType.Real:
                    if (value is not double && value is not float && 
                        !double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                        result.AddError($"Value '{value}' is not a valid real number");
                    break;

                case DataType.Char:
                    var charStr = value.ToString();
                    if (string.IsNullOrEmpty(charStr) || charStr.Length != 1)
                        result.AddError($"Value '{value}' is not a single character");
                    break;

                case DataType.String:
                    // Any object can be converted to string
                    break;

                case DataType.TextFile:
                    if (value is not FileRecord)
                        result.AddError("Value must be a FileRecord for TextFile data type");
                    break;

                case DataType.IntegerInterval:
                    if (value is not IntegerInterval)
                    {
                        try
                        {
                            IntegerInterval.Parse(value.ToString() ?? string.Empty);
                        }
                        catch (Exception ex)
                        {
                            result.AddError($"Value '{value}' is not a valid integer interval: {ex.Message}");
                        }
                    }
                    break;
            }

            return result;
        }

        public bool CanConvert(object? value, DataType targetType)
        {
            if (value == null)
                return true;

            try
            {
                ConvertValue(value, targetType);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public object? ConvertValue(object? value, DataType targetType)
        {
            if (value == null)
                return null;

            return targetType switch
            {
                DataType.Integer => ConvertToInteger(value),
                DataType.Real => ConvertToReal(value),
                DataType.Char => ConvertToChar(value),
                DataType.String => value.ToString(),
                DataType.TextFile => value as FileRecord ?? throw new InvalidCastException("Cannot convert to FileRecord"),
                DataType.IntegerInterval => value is IntegerInterval ii ? ii : IntegerInterval.Parse(value.ToString() ?? string.Empty),
                _ => throw new ArgumentException($"Unknown data type: {targetType}")
            };
        }

        private int ConvertToInteger(object value)
        {
            return value switch
            {
                int i => i,
                string s => int.Parse(s),
                double d when d == Math.Floor(d) => (int)d,
                float f when f == Math.Floor(f) => (int)f,
                long l when l >= int.MinValue && l <= int.MaxValue => (int)l,
                _ => throw new InvalidCastException($"Cannot convert '{value}' to integer")
            };
        }

        private double ConvertToReal(object value)
        {
            return value switch
            {
                double d => d,
                float f => f,
                int i => i,
                long l => l,
                decimal dec => (double)dec,
                string s => double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture),
                _ => throw new InvalidCastException($"Cannot convert '{value}' to real number")
            };
        }

        private char ConvertToChar(object value)
        {
            return value switch
            {
                char c => c,
                string s when s.Length == 1 => s[0],
                _ => throw new InvalidCastException($"Cannot convert '{value}' to char")
            };
        }
    }
}