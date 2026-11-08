using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepairTracker.Web.Data;

namespace RepairTracker.Tests;

// Starts the real app with its own throwaway database, so tests never touch my dev data
public class TestApp : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TestApp()
    {
        // CI sets REPAIRTRACKER_TEST_SQL; locally it falls back to LocalDB
        var server = Environment.GetEnvironmentVariable("REPAIRTRACKER_TEST_SQL")
            ?? @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True";

        _connectionString = new SqlConnectionStringBuilder(server)
        {
            InitialCatalog = $"RepairTracker_Test_{Guid.NewGuid():N}"
        }.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
    }

    // HTTPS client that doesn't follow redirects, so tests can check where they point
    public HttpClient CreateBrowser() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        // Drop the test database once the run is finished
        SqlConnection.ClearAllPools();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(_connectionString).Options;
        using var db = new AppDbContext(options);
        db.Database.EnsureDeleted();
    }
}