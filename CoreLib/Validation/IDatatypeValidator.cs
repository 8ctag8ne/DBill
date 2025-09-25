using CoreLib.Models;

namespace CoreLib.Validation
{
    public interface IDataTypeValidator
    {
        ValidationResult ValidateValue(object? value, DataType dataType);
        bool CanConvert(object? value, DataType targetType);
        object? ConvertValue(object? value, DataType targetType);
    }
}