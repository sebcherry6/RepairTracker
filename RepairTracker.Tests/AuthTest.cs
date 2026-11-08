using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RepairTracker.Domain;

namespace RepairTracker.Tests;

public class AuthTests(TestApp app) : IClassFixture<TestApp>
{
    [Fact]
    public async Task Anonymous_user_is_sent_to_sign_in()
    {
        var response = await app.CreateBrowser().GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Auth/Login", response.Headers.Location!.PathAndQuery);
    }

    [Fact]
    public async Task Registering_makes_you_a_landlord()
    {
        var browser = app.CreateBrowser();
        var email = $"landlord-{Guid.NewGuid():N}@example.test";

        // Load the form first, like a real browser, to get its anti-forgery token
        var formPage = await browser.GetStringAsync("/Auth/Register");
        var token = Regex.Match(formPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;

        var response = await browser.PostAsync("/Auth/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = "purple kettle morning",
            ["ConfirmPassword"] = "purple kettle morning",
            ["__RequestVerificationToken"] = token
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        // Check the database directly
        using var scope = app.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.True(await users.IsInRoleAsync(user, Roles.Landlord));
    }
}