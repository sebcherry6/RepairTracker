using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RepairTracker.Domain;
using RepairTracker.Web.Data;

namespace RepairTracker.Tests;

// Shared bits for tests that act like a real user in a browser
public static partial class TestBrowser
{
    public const string Password = "purple kettle morning";

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenPattern();

    // Loads a form to get an anti-forgery token, then posts the fields with it.
    // The token belongs to the user, not the form, so it can be sent to a different URL.
    public static async Task<HttpResponseMessage> PostFormAsync(
        this HttpClient browser, string formUrl, string postUrl, Dictionary<string, string> fields)
    {
        var page = await browser.GetStringAsync(formUrl);
        fields["__RequestVerificationToken"] = TokenPattern().Match(page).Groups[1].Value;
        return await browser.PostAsync(postUrl, new FormUrlEncodedContent(fields));
    }

    // Registers a fresh landlord through the real form. The client keeps the sign-in cookie.
    public static async Task<(HttpClient Browser, string UserId)> RegisterLandlordAsync(this TestApp app)
    {
        var browser = app.CreateBrowser();
        var email = $"landlord-{Guid.NewGuid():N}@example.test";

        var response = await browser.PostFormAsync("/Auth/Register", "/Auth/Register", new()
        {
            ["Email"] = email,
            ["Password"] = Password,
            ["ConfirmPassword"] = Password
        });
        response.EnsureRedirect();

        return (browser, await app.UserIdForAsync(email));
    }

    public static async Task<string> UserIdForAsync(this TestApp app, string email)
    {
        using var scope = app.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await users.FindByEmailAsync(email);
        return user!.Id;
    }

    // Puts a property straight into the database, skipping the UI, to set up a test quickly
    public static async Task<Guid> AddPropertyAsync(this TestApp app, string landlordId, string nickname)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var property = new Property
        {
            Id = Guid.NewGuid(),
            LandlordId = landlordId,
            AddressLine1 = "1 Test Street",
            Town = "Edinburgh",
            Postcode = "EH1 1YZ",
            Nickname = nickname,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Properties.Add(property);
        await db.SaveChangesAsync();
        return property.Id;
    }

    public static void EnsureRedirect(this HttpResponseMessage response) =>
        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
}