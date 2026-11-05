namespace RepairTracker.Domain;

// Every role name in one place, so a typo like "Landlrod" can't sneak in.
public static class Roles
{
    public const string Landlord = "Landlord";
    public const string Tenant = "Tenant";
    public const string Contractor = "Contractor";

    public static readonly string[] All = [Landlord, Tenant, Contractor];
}