using System.Dynamic;

public sealed class RepairTask : AuditableEntity
{
   public string Name { get; private set; }
   public decimal LaborCost { get; private set; }
   public RepairDurationInMinute EstimatedDuration { get; private set; }

   private readonly List<Part> _Parts = [];
   public IReadOnlyList<Part> Parts => _Parts;
   public decimal TotalPartsCost => _Parts.Sum(n => n.PartFinalCosts);
   public decimal TotalCost => TotalPartsCost + LaborCost;

#pragma warning disable CS8618

   private RepairTask()
   {

   }

#pragma warning restore CS8618

   private RepairTask(Guid id, string name, decimal LaborCost,RepairDurationInMinute repairDuration, List<Part> parts) : base(id)
   {
      this.Name = name;
      this.LaborCost = LaborCost;
      this._Parts = parts;
   }

   public static Result<RepairTask> Create(Guid id, string name, decimal LaborCost,RepairDurationInMinute repairDuration, List<Part> parts)
   {
      if (id == Guid.Empty)
      {
         id=Guid.NewGuid();
      }

      if (string.IsNullOrWhiteSpace(name))
      {
         return RepairTaskErrors.NameRequired;
      }

      if(LaborCost <=0)
      {
         return RepairTaskErrors.LaborCostInvalid;
      }

      if (!Enum.IsDefined(typeof(RepairDurationInMinute),repairDuration))
      {
         return RepairTaskErrors.DurationInvalid;
      }

      if (parts is null || !parts.Any())
      {
         return RepairTaskErrors.AtLeastOneRepairTaskPartIsRequired;
      }

      return new RepairTask(id,name.CapitalizeFirstLetter(),LaborCost,repairDuration,parts);
   }

   public Result<Updated> Update (string name, decimal LaborCost,RepairDurationInMinute repairDuration)
   {
      if (string.IsNullOrWhiteSpace(name))
      {
         return RepairTaskErrors.NameRequired;
      }

      if(LaborCost <= 0)
      {
         return RepairTaskErrors.LaborCostInvalid;
      }

      if (!Enum.IsDefined(typeof(RepairDurationInMinute),repairDuration))
      {
         return RepairTaskErrors.DurationInvalid;
      }

      this.Name =name.CapitalizeFirstLetter();
      this.LaborCost=LaborCost;
      this.EstimatedDuration=repairDuration;

      return Result.Updated;
   }

   public Result<Updated> UpSert(List<Part> parts)
   {
      var ids = parts.Select(n=> n.Id).ToHashSet();

      _Parts.RemoveAll(n=> !ids.Contains(n.Id));

      var Dic = _Parts.ToDictionary(n=>n.Id);

       foreach(var part in parts)
      {
         if(Dic.TryGetValue(part.Id,out Part? TempPart))
         {
            var UpdatePartStatus = TempPart.Updated(part.Costs,part.Name,part.Quantity);

            if (UpdatePartStatus.IsError)
            {
               return UpdatePartStatus.Errors;
            }
         }
         else
         {
            _Parts.Add(part);
         }
      }

      return Result.Updated;
   }



}