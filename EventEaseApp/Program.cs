using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;
using EventEaseApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString) && connectionString != "Server=(localdb)\\mssqllocaldb;Database=EventEaseDB;Trusted_Connection=True;MultipleActiveResultSets=true")
{
    builder.Services.AddDbContext<EventEaseContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Fallback to InMemory for local development without SQL Server
    builder.Services.AddDbContext<EventEaseContext>(options =>
        options.UseInMemoryDatabase("EventEaseDB"));
}

builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

var app = builder.Build();

// Seed the database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EventEaseContext>();
    context.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
