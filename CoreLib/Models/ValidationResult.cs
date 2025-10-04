//CoreLib/Models/ValidationResult.cs

namespace CoreLib.Models
{
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public List<string> Errors { get; set; } = new List<string>();

        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                Errors.Add(error);
            }
        }

        public void Merge(ValidationResult other)
        {
            if (other != null && other.Errors.Any())
            {
                Errors.AddRange(other.Errors);
            }
        }

        public static ValidationResult Success() => new ValidationResult();
        
        public static ValidationResult Failure(string error) => new ValidationResult { Errors = { error } };
    }
}