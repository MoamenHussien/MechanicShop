public sealed class Part : AuditableEntity
{
    public decimal Costs { get;private set; }
    public string Name { get;private set; }
    public int Quantity { get;private set; }
    public RepairTask RepairTask { get; init;} = null!;
    public Guid RepairTaskId { get;init; }

    public decimal PartFinalCosts => Costs*Quantity;

    #pragma warning disable CS8618
    private Part()
    {
        
    }
    #pragma warning restore CS8618

   private Part(Guid id,decimal Costs,string Name,int Quantity) :base(id)
   {
    this.Costs=Costs;
    this.Name=Name;
    this.Quantity=Quantity;
   }

   public static Result<Part> Create(Guid id,decimal Costs,string Name,int Quantity)
    {
        if(id == Guid.Empty)
        {
            id = Guid.NewGuid();
        }

        if (Costs <= 0)
        {
           return PartsErrors.partCostLowerThanZero;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            return PartsErrors.ValidPartName;
        }

        if (Quantity <= 0)
        {
            return PartsErrors.PartQuantityLowerThanZero;
        }

        return new Part(id,Costs,Name.CapitalizeFirstLetter(),Quantity);
    }

    public  Result<Updated> Updated(decimal Costs,string Name,int Quantity)
    {
        if (Costs <= 0)
        {
           return PartsErrors.partCostLowerThanZero;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            return PartsErrors.ValidPartName;
        }

        if (Quantity <= 0)
        {
            return PartsErrors.PartQuantityLowerThanZero;
        }

        this.Costs=Costs;
        this.Name=Name.CapitalizeFirstLetter();
        this.Quantity=Quantity;

        return Result.Updated;
    }

}