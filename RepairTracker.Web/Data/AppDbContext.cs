using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RepairTracker.Web.Data;

// Only the Identity tables for now. Repair tables get added milestone by milestone.
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
{
}