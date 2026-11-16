namespace RepairTracker.Domain;

public class Property
{
    public Guid Id { get; set; }

    // The landlord's user ID. Kept as a plain string so Domain doesn't depend on Identity.
    public string LandlordId { get; set; } = "";

    public string AddressLine1 { get; set; } = "";
    public string? AddressLine2 { get; set; }
    public string Town { get; set; } = "";
    public string Postcode { get; set; } = "";

    // Optional short name like "Flat 3" so landlords can tell properties apart
    public string? Nickname { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}