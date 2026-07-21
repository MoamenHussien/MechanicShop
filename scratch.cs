using System;

class Program
{
    static void Main()
    {
        var dt = DateTime.UtcNow.Date;
        var startAt = new DateTimeOffset(dt).AddDays(1).AddHours(15);
        var opening = startAt.Date.Add(new TimeSpan(9, 0, 0));
        var closing = startAt.Date.Add(new TimeSpan(18, 0, 0));
        var endAt = startAt + TimeSpan.FromMinutes(30);

        Console.WriteLine($"startAt: {startAt}");
        Console.WriteLine($"opening: {opening}");
        Console.WriteLine($"closing: {closing}");
        Console.WriteLine($"endAt: {endAt}");
        Console.WriteLine($"IsOutside: {startAt < opening || endAt > closing}");
    }
}
