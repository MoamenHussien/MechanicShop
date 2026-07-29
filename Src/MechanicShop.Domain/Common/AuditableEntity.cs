public abstract class AuditableEntity : Entity
{
    protected AuditableEntity(Guid id) : base(id)
    {

    }

    protected AuditableEntity()
    {

    }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset LastModifiedUtc { get; set; }
    public string? LastModifiedBy { get; set; }

}
