using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using EventEaseApp.Data;

namespace EventEaseApp.Data;

public static class RelationalSchemaBootstrapper
{
    private static readonly string[] IdentityTables =
    [
        "AspNetRoles",
        "AspNetUsers",
        "AspNetUserRoles",
        "AspNetUserClaims",
        "AspNetUserLogins",
        "AspNetUserTokens",
        "AspNetRoleClaims"
    ];

    public static async Task<DatabaseBootstrapResult> TryEnsureAsync(
        EventEaseContext context,
        IWebHostEnvironment environment,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!context.Database.IsRelational())
            {
                await context.Database.EnsureCreatedAsync(cancellationToken);
                return DatabaseBootstrapResult.Available();
            }

            if (!await CanConnectQuicklyAsync(context, cancellationToken))
            {
                return DatabaseBootstrapResult.Unavailable(
                    "The application cannot connect to Azure SQL. On Azure for Students, free databases pause when usage limits are reached or after inactivity. Resume the database in Azure Portal, create a new free database if needed, update DefaultConnection, then restart the web app.");
            }

            if (!await IdentityTablesExistAsync(context, cancellationToken))
            {
                logger.LogInformation("ASP.NET Identity tables missing — applying IdentityTables.sql.");
                await ExecuteSqlScriptAsync(context, environment, "Data/IdentityTables.sql", logger, cancellationToken);
            }

            if (!await IdentityTablesExistAsync(context, cancellationToken))
            {
                return DatabaseBootstrapResult.Unavailable(
                    "Login tables are missing in Azure SQL. Run EventEaseApp/Data/IdentityTables.sql in Azure SQL Query Editor, then restart the web app.");
            }

            return DatabaseBootstrapResult.Available();
        }
        catch (FileNotFoundException ex)
        {
            logger.LogError(ex, "Identity bootstrap script was not found.");
            return DatabaseBootstrapResult.Unavailable(
                "A required database setup file is missing from the deployed application. Redeploy the web app and try again.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database bootstrap failed.");
            return DatabaseBootstrapResult.Unavailable(DatabaseAvailabilityState.ResolveMessage(ex));
        }
    }

    private static async Task<bool> CanConnectQuicklyAsync(
        EventEaseContext context,
        CancellationToken cancellationToken)
    {
        var connectionString = context.Database.GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
            return false;

        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            ConnectTimeout = 5
        };

        try
        {
            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            return true;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    private static async Task<bool> IdentityTablesExistAsync(
        EventEaseContext context,
        CancellationToken cancellationToken)
    {
        var tableList = string.Join(", ", IdentityTables.Select(name => $"'{name}'"));
        var count = await context.Database
            .SqlQueryRaw<int>($"SELECT CAST(COUNT(*) AS int) AS [Value] FROM sys.tables WHERE name IN ({tableList})")
            .SingleAsync(cancellationToken);

        return count == IdentityTables.Length;
    }

    private static async Task ExecuteSqlScriptAsync(
        EventEaseContext context,
        IWebHostEnvironment environment,
        string relativePath,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var scriptPath = Path.Combine(environment.ContentRootPath, relativePath);
        if (!File.Exists(scriptPath))
        {
            logger.LogError("Database bootstrap script not found: {ScriptPath}", scriptPath);
            return;
        }

        var script = await File.ReadAllTextAsync(scriptPath, cancellationToken);
        var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        foreach (var batch in batches)
        {
            var sql = batch.Trim();
            if (string.IsNullOrWhiteSpace(sql))
                continue;

            try
            {
                await context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            }
            catch (SqlException ex) when (ex.Number is 2714 or 1913 or 2705 or 4922)
            {
                logger.LogWarning("Skipped SQL batch: {Message}", ex.Message);
            }
        }
    }
}
