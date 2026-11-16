using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairTracker.Domain;
using RepairTracker.Web.Data;
using RepairTracker.Web.Models;
using RepairTracker.Web.Security;

namespace RepairTracker.Web.Controllers;

[Authorize(Roles = Roles.Landlord)]
public class InvitesController(
    AppDbContext db,
    UserManager<IdentityUser> users,
    TimeProvider clock) : Controller
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(7);

    [HttpPost]
    public async Task<IActionResult> CreateForTenant(Guid propertyId)
    {
        var landlordId = users.GetUserId(User)!;

        // Same ownership rule as the property pages - can't invite people into someone else's property
        var property = await db.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.LandlordId == landlordId);
        if (property is null) return NotFound();

        var token = InviteTokens.Create();
        var now = clock.GetUtcNow().UtcDateTime;

        db.Invitations.Add(new Invitation
        {
            Id = Guid.NewGuid(),
            TokenHash = InviteTokens.Hash(token),
            Role = Roles.Tenant,
            PropertyId = property.Id,
            CreatedById = landlordId,
            CreatedAtUtc = now,
            ExpiresAtUtc = now + Lifetime
        });
        await db.SaveChangesAsync();

        var link = Url.Action(nameof(Accept), "Invites", new { token }, Request.Scheme)!;

        // Shown straight away rather than redirecting: the token isn't stored anywhere,
        // so this response is the only chance to see it
        return View("Created", new InviteCreatedViewModel(
            property.Nickname ?? property.AddressLine1, link, now + Lifetime));
    }

    // Placeholder so the link above has somewhere to point. Built properly in step 3.
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Accept(string token) => Content("Invite accepted page - coming in step 3.");
}