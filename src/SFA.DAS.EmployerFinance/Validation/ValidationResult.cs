namespace SFA.DAS.EmployerFinance.Validation;

public class ValidationResult
{
    public bool IsUnauthorized { get; set; }
    public Dictionary<string, string> ValidationDictionary { get; set; }

    public ValidationResult()
    {
        ValidationDictionary = new Dictionary<string, string>();
    }

    public void AddError(string propertyName)
    {
        AddError(propertyName, $"{propertyName} has not been supplied");
    }

    public void AddError(string propertyName, string validationError)
    {
        if (ValidationDictionary.TryGetValue(propertyName, out var existing))
        {
            ValidationDictionary[propertyName] = $"{existing} {validationError}";
            return;
        }

        ValidationDictionary[propertyName] = validationError;
    }

    public List<string> ErrorList => ValidationDictionary.Select(c => c.Key + "|" + c.Value).ToList();
        

    public bool IsValid()
    {
        if (ValidationDictionary == null)
        {
            return false;
        }

        return !ValidationDictionary.Any();
    }
}