using System.Text.RegularExpressions;

namespace SFA.DAS.EmployerFinance.Validation;

public static class EmployerReferenceValidation
{
    private const int MaximumLength = 50;

    private static readonly Regex EmployerReferencePattern = new(@"^[0-9]{3}/[A-Z0-9]{1,7}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool TryNormalise(string? employerReference, out string normalisedEmployerReference)
    {
        normalisedEmployerReference = string.Empty;

        if (string.IsNullOrWhiteSpace(employerReference))
        {
            return false;
        }

        // Reject hostile input before trimming or transforming it.
        var trimmed = employerReference.Trim();

        if (trimmed.Length > MaximumLength || trimmed.Any(char.IsControl))
        {
            return false;
        }

        normalisedEmployerReference = trimmed.ToUpperInvariant();        

        if (normalisedEmployerReference.Length > 3 && normalisedEmployerReference[3] != '/')
        {
            normalisedEmployerReference = normalisedEmployerReference.Insert(3, "/");
        }

        return EmployerReferencePattern.IsMatch(normalisedEmployerReference);
    }
}