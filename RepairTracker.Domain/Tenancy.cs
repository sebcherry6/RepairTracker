namespace RepairTracker.Domain;

// Links a tenant to a property for a period of time.
// Ended tenancies are kept, so there's a history of who lived where and when.
public class Tenancy
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string TenantId { get; set; } = "";
    public DateOnly StartDate { get; set; }

    // Null while the tenancy is ongoing
    public DateOnly? EndDate { get; private set; }

    // The end date is the tenant's last day, so it still counts as current on that day
    public bool IsCurrent(DateOnly today) =>
        StartDate <= today && (EndDate is null || today <= EndDate);

    public void End(DateOnly lastDay)
    {
        if (EndDate is not null)
            throw new InvalidOperationException("This tenancy has already ended.");
        if (lastDay < StartDate)
            throw new ArgumentException("A tenancy can't end before it starts.", nameof(lastDay));

        EndDate = lastDay;
    }
}