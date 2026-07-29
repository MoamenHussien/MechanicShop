using FluentValidation;

public class RegisterLaborRequestContract
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public List<string> Roles { get; set; } = [];
}

public class RegisterLaborCommandValidatorContract : AbstractValidator<RegisterLaborRequestContract>
{
    public RegisterLaborCommandValidatorContract()
    {
        RuleFor(n => n.Email).MustBeValidEmail();
        RuleFor(n => n.Password).NotEmpty().WithMessage("Password Is Required").Must(n => !string.IsNullOrWhiteSpace(n)).WithMessage("Enter Valid Password").Length(8, 30).WithMessage("Password must be between 8 and 30 characters");
        RuleFor(n => n.FirstName).NotEmpty().WithMessage("First Name is required").MinimumLength(2).WithMessage("First Name must be at least 2 characters").MaximumLength(50).WithMessage("First Name cannot exceed 50 characters");
        RuleFor(n => n.LastName).NotEmpty().WithMessage("Last Name is required").MinimumLength(2).WithMessage("Last Name must be at least 2 characters").MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters");
        RuleForEach(n => n.Roles).IsInEnum().WithMessage("Roles must be a valid Enum Value");
    }
}
