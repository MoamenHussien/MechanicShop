using System.Text.RegularExpressions;
using FluentValidation;

// Application/Validators/CommonRules.cs

public static class CommonRules
{
    private static readonly Regex EmailRegex = new(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$",
    RegexOptions.Compiled);

    private static readonly Regex EgyptPhoneRegex = new(
        @"^(\+20|0020|0)?1[0125]\d{8}$",
        RegexOptions.Compiled);

    public static IRuleBuilderOptions<T, string> MustBeValidEmail<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Email Is Required")
            .MaximumLength(256)
            .WithMessage("Email Length From 3 To 256 Char")
            .Must(email => EmailRegex.IsMatch(email))
            .WithMessage("Email Not Valid");
    }

    public static IRuleBuilderOptions<T, string> MustBeValidPhone<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Phone Is Required")
            .Must(phone => EgyptPhoneRegex.IsMatch(phone))
            .WithMessage("Phone Is Not Valid");
    }

    public static IRuleBuilderOptions<T, Guid> IdRequired<T>(
        this IRuleBuilder<T, Guid> ruleBuilder, string TypeName)
    {
        return ruleBuilder
            .NotEmpty().WithErrorCode($"{TypeName}_Id_Is_Required").WithMessage($"{TypeName} Id Is Required");

    }
}
