using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

public sealed class Invoice : AuditableEntity
{

    public DateTimeOffset IssuedAtUtc { get; init; }
    public InvoiceStatus Status { get; private set; }

    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    private readonly List<InvoiceLineItem> _InvoiceLineItems = [];
    public IEnumerable<InvoiceLineItem> InvoiceLineItems => _InvoiceLineItems.AsReadOnly();

    public WorkOrder WorkOrder { get; init; } = null!;
    public Guid WorkOrderId { get; init; }

    public decimal Subtotal => _InvoiceLineItems.Sum(n => n.LineTotal);

    public decimal Total => Subtotal + TaxAmount - DiscountAmount;

#pragma warning disable CS8618
    private Invoice()
    {

    }
#pragma warning restore CS8618

    private Invoice(Guid id, DateTimeOffset IssuedAtUtc,
                   decimal TaxAmount, decimal DiscountAmount,
                   List<InvoiceLineItem> InvoiceLineItems, Guid workOrderid) : base(id)
    {
        this.IssuedAtUtc = IssuedAtUtc;
        this.Status = InvoiceStatus.Unpaid;
        this.TaxAmount = TaxAmount;
        this.DiscountAmount = DiscountAmount;
        this._InvoiceLineItems = InvoiceLineItems;
        this.WorkOrderId = workOrderid;
    }

    public static Result<Invoice> Create(Guid id, TimeProvider time,
                   decimal TaxAmount, decimal DiscountAmount,
                   List<InvoiceLineItem> InvoiceLineItems, Guid WorkOrderid)
    {
        if (id == Guid.Empty)
        {
            id = Guid.NewGuid();
        }

        if (WorkOrderid == Guid.Empty)
        {
            return InvoiceErrors.WorkOrderIdInvalid;
        }

        if (InvoiceLineItems is null || !InvoiceLineItems.Any())
        {
            return InvoiceErrors.LineItemsEmpty;
        }

        return new Invoice(id, time.GetUtcNow(), TaxAmount, DiscountAmount, InvoiceLineItems, WorkOrderid);
    }

    public Result<Updated> ApplyDiscount(decimal DiscountAmount)
    {
        if (this.Status is not (InvoiceStatus.Unpaid))
        {
            return InvoiceErrors.InvoiceLocked;
        }

        if (DiscountAmount >= Subtotal)
        {
            return InvoiceErrors.DiscountExceedsSubtotal;
        }

        if (DiscountAmount < 0)
        {
            return InvoiceErrors.DiscountNegative;
        }

        this.DiscountAmount = DiscountAmount;

        return Result.Updated;
    }

    public Result<Updated> MarkAsPaid(TimeProvider timeProvider)
    {
        if (this.Status is not (InvoiceStatus.Unpaid))
        {
            return InvoiceErrors.InvoiceLocked;
        }

        this.Status = InvoiceStatus.Paid;
        this.PaidAt = timeProvider.GetUtcNow();

        return Result.Updated;
    }


}