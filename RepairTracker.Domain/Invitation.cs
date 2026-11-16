namespace RepairTracker.Domain;

public class Invitation
{
    public Guid Id { get; set; }

    // SHA-256 of the token in the link, not the token itself.
    // If the database ever leaked, nobody could rebuild working invite links from it.
    public string TokenHash { get; set; } = "";

    // Tenant for now; Contractor invites come in Milestone 6
    public string Role { get; set; } = Roles.Tenant;

    // Which property a tenant invite is for. Contractor invites won't have one.
    public Guid? PropertyId { get; set; }

    public string CreatedById { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

    // Private setters: the only way to use up an invite is MarkUsed, so the rules can't be skipped
    public DateTime? UsedAtUtc { get; private set; }
    public string? UsedById { get; private set; }

    public bool IsUsable(DateTime nowUtc) => UsedAtUtc is null && nowUtc < ExpiresAtUtc;

    public void MarkUsed(string userId, DateTime nowUtc)
    {
        if (!IsUsable(nowUtc))
            throw new InvalidOperationException("This invitation has already been used or has expired.");

        UsedAtUtc = nowUtc;
        UsedById = userId;
    }
}