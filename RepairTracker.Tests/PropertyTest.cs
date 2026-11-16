using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RepairTracker.Domain;
using RepairTracker.Web.Data;

namespace RepairTracker.Tests;

public class PropertyTests(TestApp app) : IClassFixture<TestApp>
{
    [Fact]
    public async Task Landlord_only_sees_their_own_properties()
    {
        var alice = await app.RegisterLandlordAsync();
        var bob = await app.RegisterLandlordAsync();
        await app.AddPropertyAsync(alice.UserId, "Alpha Flat");
        await app.AddPropertyAsync(bob.UserId, "Bravo House");

        var page = await bob.Browser.GetStringAsync("/Properties");

        Assert.Contains("Bravo House", page);
        Assert.DoesNotContain("Alpha Flat", page);
    }

    [Fact]
    public async Task Landlord_cannot_open_another_landlords_property()
    {
        var alice = await app.RegisterLandlordAsync();
        var bob = await app.RegisterLandlordAsync();
        var alicesProperty = await app.AddPropertyAsync(alice.UserId, "Alpha Flat");

        var response = await bob.Browser.GetAsync($"/Properties/Edit/{alicesProperty}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Landlord_cannot_change_another_landlords_property()
    {
        var alice = await app.RegisterLandlordAsync();
        var bob = await app.RegisterLandlordAsync();
        var alicesProperty = await app.AddPropertyAsync(alice.UserId, "Alpha Flat");

        // Bob can't load Alice's edit form, so he posts straight at it with a token from his own page
        var response = await bob.Browser.PostFormAsync("/Properties/Create", $"/Properties/Edit/{alicesProperty}", new()
        {
            ["Nickname"] = "Hijacked",
            ["AddressLine1"] = "1 Test Street",
            ["Town"] = "Edinburgh",
            ["Postcode"] = "EH1 1YZ"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        // The real proof: the database still has the original
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await db.Properties.AsNoTracking().SingleAsync(p => p.Id == alicesProperty);
        Assert.Equal("Alpha Flat", stored.Nickname);
    }

    [Fact]
    public async Task Posted_landlord_id_is_ignored()
    {
        var alice = await app.RegisterLandlordAsync();
        var bob = await app.RegisterLandlordAsync();

        // Bob tries to sneak a property into Alice's account by adding LandlordId to the form
        await bob.Browser.PostFormAsync("/Properties/Create", "/Properties/Create", new()
        {
            ["LandlordId"] = alice.UserId,
            ["Nickname"] = "Sneaky Flat",
            ["AddressLine1"] = "1 Test Street",
            ["Town"] = "Edinburgh",
            ["Postcode"] = "EH1 1YZ"
        });

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await db.Properties.AsNoTracking().SingleAsync(p => p.Nickname == "Sneaky Flat");
        Assert.Equal(bob.UserId, stored.LandlordId);
    }

    [Fact]
    public async Task Tenant_cannot_open_the_properties_page()
    {
        // Tenants only join by invite (Milestone 3), so make one directly
        var email = $"tenant-{Guid.NewGuid():N}@example.test";
        using (var scope = app.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var tenant = new IdentityUser { UserName = email, Email = email };
            await users.CreateAsync(tenant, TestBrowser.Password);
            await users.AddToRoleAsync(tenant, Roles.Tenant);
        }

        var browser = app.CreateBrowser();
        var signIn = await browser.PostFormAsync("/Auth/Login", "/Auth/Login", new()
        {
            ["Email"] = email,
            ["Password"] = TestBrowser.Password
        });
        signIn.EnsureRedirect();   // signed in fine - a failed sign-in shows the form again with a 200

        var response = await browser.GetAsync("/Properties");

        // Access denied sends them to the sign-in page rather than showing the list
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Auth/Login", response.Headers.Location!.PathAndQuery);
    }
}