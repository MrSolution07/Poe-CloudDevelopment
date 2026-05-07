using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;
using EventEaseApp.Models;
using EventEaseApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Load optional local overrides (e.g. Azure connection string) — appsettings.Development.local.json is gitignored
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.local.json", optional: true);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Use Azure SQL (or any real SQL Server) when a connection string pointing to a SQL Server host is present.
// In Development, skip Azure SQL so "dotnet run" works without reaching Azure (avoids connection timeout).
// Use LocalDB or InMemory when running locally.
bool useRealDatabase = !string.IsNullOrWhiteSpace(connectionString) &&
    !(builder.Environment.IsDevelopment() && connectionString.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase)) &&
    (connectionString.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase) ||
     connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
     (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) &&
      !connectionString.Contains("localdb", StringComparison.OrdinalIgnoreCase)));

if (useRealDatabase)
{
    builder.Services.AddDbContext<EventEaseContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null)));
}
else
{
    builder.Services.AddDbContext<EventEaseContext>(options =>
        options.UseInMemoryDatabase("EventEaseDB"));
}

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<EventEaseContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
});

builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IEmailService, DevEmailService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventEaseContext>();
    context.Database.EnsureCreated();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var admin = await userManager.FindByEmailAsync("admin@eventease.co.za");
    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = "admin@eventease.co.za",
            Email = "admin@eventease.co.za",
            FullName = "Admin User",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(admin, "Admin123");
    }

    if (!await userManager.IsInRoleAsync(admin, "Admin"))
        await userManager.AddToRoleAsync(admin, "Admin");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Set Permissions-Policy without browsing-topics to avoid browser console warning (e.g. on Azure)
app.Use(async (context, next) =>
{
    context.Response.Headers["Permissions-Policy"] = "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";
    await next();
});
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
