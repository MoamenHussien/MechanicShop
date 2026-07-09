using System.Net.Cache;

public static class CustomerErrors
{
    public static readonly Error NameRequired = Error.Validation("Customer.Name.Required","You Must Enter Customer Name");
    public static readonly Error EmailRequired = Error.Validation("Customer.Email.Required","You Must Enter Customer Email");
    public static readonly Error VehiclesRequired = Error.Validation("Customer.Vehicles.Required","You Must Enter At Least One Vehicle To Customer");
    public static readonly Error PhoneRequired = Error.Validation("Customer.Phone.Required","You Must Enter Customer Phone");
    
}