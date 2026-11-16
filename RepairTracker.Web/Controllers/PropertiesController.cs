using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairTracker.Domain;
using RepairTracker.Web.Data;
using RepairTracker.Web.Models;

namespace RepairTracker.Web.Controllers;

// Landlords only. Tenants and contractors get Access Denied here.
[Authorize(Roles = Roles.Landlord)]
public class PropertiesController(AppDbContext db, UserManager<IdentityUser> users) : Controller
{
    // Signed-in user's id, straight from their auth cookie - never from the form
    private string LandlordId => users.GetUserId(User)!;

    // Every query starts here, so a landlord can only ever touch their own rows
    private IQueryable<Property> MyProperties =>
        db.Properties.Where(p => p.LandlordId == LandlordId);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var properties = await MyProperties
            .AsNoTracking()
            .OrderBy(p => p.Nickname ?? p.AddressLine1)
            .ToListAsync();

        return View(properties);
    }

    [HttpGet]
    public IActionResult Create() => View(new PropertyInput());

    [HttpPost]
    public async Task<IActionResult> Create(PropertyInput input)
    {
        if (!ModelState.IsValid) return View(input);

        var property = new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = LandlordId,
            CreatedAtUtc = DateTime.UtcNow
        };
        input.ApplyTo(property);

        db.Properties.Add(property);
        await db.SaveChangesAsync();

        TempData["Status"] = "Property added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var property = await MyProperties.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

        // Someone else's property looks exactly like one that doesn't exist
        if (property is null) return NotFound();

        ViewData["PropertyId"] = id;
        return View(PropertyInput.From(property));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, PropertyInput input)
    {
        if (!ModelState.IsValid)
        {
            ViewData["PropertyId"] = id;
            return View(input);
        }

        // Load with the owner check again - the GET check doesn't protect the POST
        var property = await MyProperties.FirstOrDefaultAsync(p => p.Id == id);
        if (property is null) return NotFound();

        input.ApplyTo(property);
        await db.SaveChangesAsync();

        TempData["Status"] = "Changes saved.";
        return RedirectToAction(nameof(Index));
    }
}