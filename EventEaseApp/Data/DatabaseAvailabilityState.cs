using Microsoft.Data.SqlClient;

namespace EventEaseApp.Data;

public sealed class DatabaseAvailabilityState
{
    public bool IsAvailable { get; private set; } = true;

    public string Message { get; private set; } =
        "Our cloud database is temporarily unavailable. Please try again later.";

    public void MarkUnavailable(string message)
    {
        IsAvailable = false;
        if (!string.IsNullOrWhiteSpace(message))
            Message = message;
    }

    public static string ResolveMessage(Exception exception)
    {
        for (var ex = exception; ex is not null; ex = ex.InnerException)
        {
            if (ex is SqlException sqlEx)
                return ResolveSqlMessage(sqlEx);
        }

        return "Our cloud database is temporarily unavailable. On Azure for Students, free SQL databases may pause when usage limits are reached or after a period of inactivity. Resume the database in Azure Portal, update the connection string if you created a new database, or try again later.";
    }

    private static string ResolveSqlMessage(SqlException sqlEx)
    {
        if (sqlEx.Number is 40613 or 40671)
        {
            return "The Azure SQL database is paused or not currently available. On Azure for Students, free databases pause when usage limits are reached or after inactivity. Open Azure Portal, resume the database or create a new free database, update the App Service connection string, then restart the web app.";
        }

        if (sqlEx.Number is 18456 or 40532)
        {
            return "The application could not sign in to Azure SQL. Check the DefaultConnection username and password in App Service configuration.";
        }

        if (sqlEx.Number is 4060)
        {
            return "The configured Azure SQL database could not be found. If you created a new database, update Initial Catalog in the DefaultConnection connection string.";
        }

        return "Our cloud database is temporarily unavailable. On Azure for Students, free SQL databases may pause when usage limits are reached or after a period of inactivity. Resume the database in Azure Portal, update the connection string if you created a new database, or try again later.";
    }
}
