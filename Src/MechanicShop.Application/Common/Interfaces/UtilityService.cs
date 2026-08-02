using System.Security.Cryptography.X509Certificates;

public static class UtilityService
{
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return string.Empty;
        }

        int index = email.IndexOf('@');

        if (index <= 1)
        {
            return $"*****{email.AsSpan(index)}";
        }

        return email[0] + "*****" + email[index - 1] + email[index..];
    }

    // public static Result<Guid> ToGuid(this string stringGuidId)
    // {
    //     if (string.IsNullOrWhiteSpace(stringGuidId))
    //     {
    //         return Error.Failure("Invalid_Guid","Guid value is null or empty");
    //     }

    // if (Guid.TryParse(stringGuidId, out Guid guidId))
    //     {
    //         return guidId;
    //     }
    //     else
    //     {
    //         return Error.Failure("The Guid is null or invalid");
    //     }
    // }
    public static Result<Guid> ToGuid(this string? stringGuidId)
    {
        if (string.IsNullOrWhiteSpace(stringGuidId))
        {
            return Error.Failure("Invalid_Guid", "Guid value is null or empty");
        }

        if (Guid.TryParse(stringGuidId, out var guidId))
        {
            return guidId;
        }

        return Error.Failure("Invalid_Guid", $"'{stringGuidId}' is not a valid Guid");
    }

    public static DateTime ToUtc(this DateTime date, TimeZoneInfo zone)
    {
        return TimeZoneInfo.ConvertTimeToUtc(date, zone);
    }

    public static DateTime ToLocal(this DateTime date, TimeZoneInfo zone)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(date, zone);
    }

    public static DateTimeOffset ToUtc(this DateTimeOffset date)
    {
        return date.ToUniversalTime();
    }

    public static DateTimeOffset ToLocal(this DateTimeOffset date, TimeZoneInfo zone)
    {
        return TimeZoneInfo.ConvertTime(date, zone);
    }
}
