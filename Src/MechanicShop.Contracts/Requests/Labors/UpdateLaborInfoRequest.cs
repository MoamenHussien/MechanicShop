using FluentValidation;

public class UpdateLaborInfoRequest
{
    public string FirstName { get;  set; } =null!;
    public string LastName { get;  set; } =null!;
    public bool IsActive { get;  set; } 
}

public class UpdateLaborInfoCommandValidatorContract : AbstractValidator<UpdateLaborInfoRequest>
{
    public UpdateLaborInfoCommandValidatorContract()
    {
        RuleFor(n => n.FirstName).NotEmpty().WithMessage("First Name is required").MinimumLength(2).WithMessage("First Name must be at least 2 characters").MaximumLength(50).WithMessage("First Name cannot exceed 50 characters");
        RuleFor(n => n.LastName).NotEmpty().WithMessage("Last Name is required").MinimumLength(2).WithMessage("Last Name must be at least 2 characters").MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters");
        RuleFor(n => n.IsActive).NotNull().WithMessage("The Labors Status Is Required");
    }
}