using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
    
    public DbSet<Journey> Journeys { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Reason> Reasons { get; set; }
}