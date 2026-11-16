using RepairTracker.Domain;

namespace RepairTracker.Tests;

public class InvitationTests
{
    private static readonly DateTime Now = new(2026, 11, 16, 12, 0, 0, DateTimeKind.Utc);

    private static Invitation NewInvite() => new()
    {
        CreatedAtUtc = Now,
        ExpiresAtUtc = Now.AddDays(7)
    };

    [Fact]
    public void Fresh_invite_can_be_used()
    {
        var invite = NewInvite();

        invite.MarkUsed("tenant-1", Now);

        Assert.Equal("tenant-1", invite.UsedById);
        Assert.Equal(Now, invite.UsedAtUtc);
    }

    [Fact]
    public void Invite_cannot_be_used_twice()
    {
        var invite = NewInvite();
        invite.MarkUsed("tenant-1", Now);

        Assert.Throws<InvalidOperationException>(() => invite.MarkUsed("tenant-2", Now));
        Assert.Equal("tenant-1", invite.UsedById);
    }

    [Fact]
    public void Expired_invite_cannot_be_used()
    {
        var invite = NewInvite();

        Assert.False(invite.IsUsable(Now.AddDays(8)));
        Assert.Throws<InvalidOperationException>(() => invite.MarkUsed("tenant-1", Now.AddDays(8)));
    }
}