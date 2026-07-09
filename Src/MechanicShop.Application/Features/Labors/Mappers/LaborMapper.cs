public static class LaborMapper
{
    public static List<LaborDto> ToDto(this IList<Employee> employee)
    {
        return employee.Select(n=> ToDto(n)).ToList();
    }

    public static LaborDto ToDto(this Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);
        return new LaborDto(employee.Id,employee.FullName);
    }
}