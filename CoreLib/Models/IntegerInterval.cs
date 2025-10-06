//CoreLib/Models/IntegerInterval.cs
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
    }
}