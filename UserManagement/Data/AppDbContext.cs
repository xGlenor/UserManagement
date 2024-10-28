﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data;

public class AppDbContext: IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
        
    }
    
    public DbSet<UserPasswordHistory> UserPasswordHistories { get; set; }
    public DbSet<ApplicationUserSettings> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<UserPasswordHistory>().ToTable("UserPasswordHistories");
        builder.Entity<ApplicationUserSettings>().ToTable("UserSettings");
        
        base.OnModelCreating(builder);
    }
}