using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;

public abstract class Entity {
    public Guid Id { get; }

    [NotMapped]
    private readonly List<DomainEvents> _DomainEvents = [];
    public ICollection<DomainEvents> DomainEvents => _DomainEvents.AsReadOnly();
    protected Entity()
    {
        
    }
    protected Entity( Guid id)
    {
        this.Id = id == Guid.Empty ? Guid.NewGuid() : id;
    }
    
    public void AddDomainEvent (DomainEvents domainEvents) => _DomainEvents.Add(domainEvents);
    public void DeleteDomainEvent (DomainEvents domainEvents)=> _DomainEvents.Remove(domainEvents);

    public void ClearDomainEvent () => _DomainEvents.Clear();

}