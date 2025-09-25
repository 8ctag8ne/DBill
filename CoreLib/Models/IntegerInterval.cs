using System;
using System.Text.Json.Serialization;

namespace CoreLib.Models
{
    public class IntegerInterval
    {
        public int Min { get; set; }
        public int Max { get; set; }

        [JsonConstructor]
        public IntegerInterval(int min, int max)
        {
            if (min > max)
                throw new ArgumentException("Min value cannot be greater than Max value");
            
            Min = min;
            Max = max;
        }

        public bool Contains(int value)
        {
            return value >= Min && value <= Max;
        }

        public override string ToString()
        {
            return $"[{Min}, {Max}]";
        }

        public static IntegerInterval Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or empty");

            // Expected format: [min, max] or (min, max)
            value = value.Trim();
            if ((!value.StartsWith("[") && !value.StartsWith("(")) || 
                (!value.EndsWith("]") && !value.EndsWith(")")))
                throw new FormatException("Invalid interval format. Expected: [min, max]");

            var content = value.Substring(1, value.Length - 2);
            var parts = content.Split(',');
            
            if (parts.Length != 2)
                throw new FormatException("Invalid interval format. Expected: [min, max]");

            if (!int.TryParse(parts[0].Trim(), out int min))
                throw new FormatException("Invalid min value");
            
            if (!int.TryParse(parts[1].Trim(), out int max))
                throw new FormatException("Invalid max value");

            return new IntegerInterval(min, max);
        }
    }
}