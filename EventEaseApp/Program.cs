using System.Text.RegularExpressions;
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

// Use a real SQL Server (Azure SQL or on-prem) whenever a connection string is
// configured for a non-LocalDB host. Falling back to InMemory only when no
// connection string is present or it explicitly points at LocalDB keeps the
// local override file (appsettings.Development.local.json) authoritative —
// if a developer puts an Azure SQL connection string there, it is used.
// Set "Database:ForceInMemory" to true in any settings file to override.
bool forceInMemory = builder.Configuration.GetValue<bool>("Database:ForceInMemory");
bool useRealDatabase = !forceInMemory
    && !string.IsNullOrWhiteSpace(connectionString)
    && (connectionString.Contains("database.windows.net", StringComparison.OrdinalIgnoreCase)
        || connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase)
        || (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase)
            && !connectionString.Contains("localdb", StringComparison.OrdinalIgnoreCase)));

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
builder.Services.AddSingleton<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IEmailService, DevEmailService>();

var maxIncomingBytes = builder.Configuration.GetValue<long?>("ImageProcessing:MaxIncomingBytes") ?? 20_971_520L;
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxIncomingBytes;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxIncomingBytes;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventEaseContext>();
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    if (context.Database.IsRelational())
        await EnsureRelationalSchemaAsync(context, startupLogger);
    else
        await context.Database.EnsureCreatedAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var configuredEmail = app.Configuration["AdminSeed:Email"];
    var configuredPassword = app.Configuration["AdminSeed:Password"];
    var adminEmail = !string.IsNullOrWhiteSpace(configuredEmail)
        ? configuredEmail
        : "admin@eventease.co.za";
    var adminPassword = !string.IsNullOrWhiteSpace(configuredPassword)
        ? configuredPassword
        : "Admin123";

    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Admin",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(admin, adminPassword);
    }

    if (!await userManager.IsInRoleAsync(admin, "Admin"))
        await userManager.AddToRoleAsync(admin, "Admin");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

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

static async Task EnsureRelationalSchemaAsync(EventEaseContext context, ILogger logger)
{
    string[] identityTables =
    [
        "AspNetRoles",
        "AspNetUsers",
        "AspNetUserRoles",
        "AspNetUserClaims",
        "AspNetUserLogins",
        "AspNetUserTokens",
        "AspNetRoleClaims"
    ];

    if (!await context.Database.CanConnectAsync())
    {
        throw new InvalidOperationException(
            "Cannot connect to the configured SQL database. Check DefaultConnection and Azure SQL firewall rules.");
    }

    var tableList = string.Join(", ", identityTables.Select(name => $"'{name}'"));
    var existingIdentityTables = await context.Database
        .SqlQueryRaw<int>($"SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE name IN ({tableList})")
        .SingleAsync();

    if (existingIdentityTables == identityTables.Length)
        return;

    logger.LogInformation(
        "ASP.NET Identity schema incomplete ({Existing}/{Total}) — creating missing tables.",
        existingIdentityTables,
        identityTables.Length);

    // script.sql creates domain tables, but EnsureCreated() is skipped when the database
    // already exists. Generate and run only the Identity DDL from the EF model.
    var script = context.Database.GenerateCreateScript();
    var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

    foreach (var batch in batches)
    {
        var sql = batch.Trim();
        if (string.IsNullOrWhiteSpace(sql))
            continue;
        if (!sql.Contains("AspNet", StringComparison.OrdinalIgnoreCase))
            continue;

        try
        {
            await context.Database.ExecuteSqlRawAsync(sql);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2714 or 1913 or 2705)
        {
            // Object/index/column already exists from a previous partial startup attempt.
            logger.LogWarning("Skipped identity DDL batch: {Message}", ex.Message);
        }
    }
}
