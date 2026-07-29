public static class PartsErrors
{
    public static readonly Error ValidPartName = Error.Validation("You Must Enter Valid Part Name");
    public static readonly Error PartQuantityLowerThanZero = Error.Validation("You Must Enter Part Quantity Greater Than Zero");
    public static readonly Error partCostLowerThanZero = Error.Validation("You Must Enter Part Cost Greater Than Zero");
}
