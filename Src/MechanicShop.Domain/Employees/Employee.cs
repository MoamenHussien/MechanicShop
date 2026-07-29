using System.Security.Cryptography;

public sealed class Employee : AuditableEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public bool IsActive { get; private set; }
    public string FullName => FirstName + " " + LastName;

#pragma warning disable CS8618
    private Employee()
    {

    }
#pragma warning restore CS8618

    private Employee(Guid id, string FirstName, string LastName) : base(id)
    {
        this.FirstName = FirstName;
        this.LastName = LastName;
        this.IsActive = true;
    }

    public static Result<Employee> Create(Guid id, string FirstName, string LastName)
    {
        if (id == Guid.Empty)
        {
            id = Guid.NewGuid();
        }

        if (string.IsNullOrWhiteSpace(FirstName))
        {
            return EmployeeErrors.FirstNameRequired;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            return EmployeeErrors.LastNameRequired;
        }

        return new Employee(id, FirstName.CapitalizeFirstLetter(), LastName.CapitalizeFirstLetter());
    }
    public Result<Updated> Update(string FirstName, string LastName, bool IsActive)
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            return EmployeeErrors.FirstNameRequired;
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            return EmployeeErrors.LastNameRequired;
        }

        this.FirstName = FirstName.CapitalizeFirstLetter();
        this.LastName = LastName.CapitalizeFirstLetter();
        this.IsActive = IsActive;

        return Result.Updated;
    }



}
