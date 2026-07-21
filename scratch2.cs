using System;
using System.Collections.Generic;

public sealed record CreateCustomerCommand(
    string name,
    string email,
    string PhoneNumber,
    List<string> Vehicles);

class Program
{
    static void Main()
    {
        var command = new CreateCustomerCommand("Moamen", "test@gmail.com", "123", null);
        Console.WriteLine($"Is Null? {command.Vehicles == null}");
    }
}
