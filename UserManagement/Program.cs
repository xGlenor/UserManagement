using System.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Helper;
using UserManagement.Models;
using UserManagement.Repository;
using UserManagement.Services;
using UserManagement.Validators;

var builder = WebApplication.CreateBuilder(args);

/*
|--------------------------------------------------------------------------
| Database Settings
|--------------------------------------------------------------------------
*/
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=UserManagement.db"));
    /*options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));*/

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

/*
|--------------------------------------------------------------------------
| Identity Settings
|--------------------------------------------------------------------------
*/

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 1;
        options.Password.RequiredUniqueChars = 1;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 3;
    }).AddPasswordValidator<CustomPasswordValidator>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
    options.SlidingExpiration = false;
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(5);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

/*
|--------------------------------------------------------------------------
| Email Settings
|--------------------------------------------------------------------------
*/
builder.Services.AddTransient<IEmailSender, EmailSender>(i => 
    new EmailSender(
        builder.Configuration["EmailSender:Host"],
        builder.Configuration.GetValue<int>("EmailSender:Port"),
        builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),
        builder.Configuration["EmailSender:UserName"],
        builder.Configuration["EmailSender:Password"]
    )
);

/*
|--------------------------------------------------------------------------
| General Settings
|--------------------------------------------------------------------------
*/
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoggRepository, LoggRepository>();
builder.Services.AddScoped<ILogService, LogService>();

/*
|--------------------------------------------------------------------------
| BUILD APP
|--------------------------------------------------------------------------
*/

var app = builder.Build();

using (var scopeRoles = app.Services.CreateScope())
{
    var serviceProvider = scopeRoles.ServiceProvider;
    await RoleInitializer.InitializeAsync(serviceProvider);

}

using (var scopeRoles = app.Services.CreateScope())
{
    var serviceProvider = scopeRoles.ServiceProvider;
    await AdminAndUserInitializer.InitializeAsync(serviceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.MapRazorPages();
//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

app.Run();