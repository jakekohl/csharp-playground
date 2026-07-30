using System.Data;
using Microsoft.Data.SqlClient;

namespace Playground.Util;

/// <summary>
/// Helper for running queries and transactions against Azure SQL / SQL Server.
/// Connection settings come from environment variables (see .env).
/// </summary>
public static class DB
{
    private static string? _connectionString;

    public static string ConnectionString =>
        _connectionString ??= BuildConnectionString();

    /// <summary>
    /// Opens a new connection. Prefer the Execute* helpers; use this when you need
    /// fine-grained control (e.g. streaming a reader yourself).
    /// </summary>
    public static async Task<SqlConnection> OpenConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public static SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    /// <summary>
    /// Runs a SELECT (or any result-producing) query and returns a DataTable.
    /// </summary>
    public static async Task<DataTable> ExecuteQueryAsync(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, sql, parameters);

        var table = new DataTable();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        table.Load(reader);
        return table;
    }

    public static DataTable ExecuteQuery(
        string sql,
        params SqlParameter[] parameters)
    {
        using var connection = OpenConnection();
        using var command = CreateCommand(
            connection,
            sql,
            parameters.Length > 0 ? parameters : null);

        var table = new DataTable();
        using var reader = command.ExecuteReader();
        table.Load(reader);
        return table;
    }

    /// <summary>
    /// Runs INSERT / UPDATE / DELETE (or other non-query) SQL. Returns rows affected.
    /// </summary>
    public static async Task<int> ExecuteNonQueryAsync(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, sql, parameters);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public static int ExecuteNonQuery(
        string sql,
        params SqlParameter[] parameters)
    {
        using var connection = OpenConnection();
        using var command = CreateCommand(
            connection,
            sql,
            parameters.Length > 0 ? parameters : null);
        return command.ExecuteNonQuery();
    }

    /// <summary>
    /// Runs a query expected to return a single value (e.g. COUNT(*), SCOPE_IDENTITY()).
    /// </summary>
    public static async Task<T?> ExecuteScalarAsync<T>(
        string sql,
        IEnumerable<SqlParameter>? parameters = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, sql, parameters);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return ConvertScalar<T>(result);
    }

    public static T? ExecuteScalar<T>(
        string sql,
        params SqlParameter[] parameters)
    {
        using var connection = OpenConnection();
        using var command = CreateCommand(
            connection,
            sql,
            parameters.Length > 0 ? parameters : null);
        var result = command.ExecuteScalar();
        return ConvertScalar<T>(result);
    }

    /// <summary>
    /// Runs work inside a SQL transaction. Commits on success; rolls back on exception.
    /// Use the provided connection/transaction for all commands in <paramref name="work"/>.
    /// </summary>
    public static async Task ExecuteInTransactionAsync(
        Func<SqlConnection, SqlTransaction, Task> work,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await work(connection, transaction);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Same as <see cref="ExecuteInTransactionAsync"/> but returns a value from the work delegate.
    /// </summary>
    public static async Task<T> ExecuteInTransactionAsync<T>(
        Func<SqlConnection, SqlTransaction, Task<T>> work,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction =
            (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await work(connection, transaction);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public static void ExecuteInTransaction(Action<SqlConnection, SqlTransaction> work)
    {
        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            work(connection, transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>
    /// Builds a parameter. Prefer this over concatenating values into SQL.
    /// </summary>
    public static SqlParameter Param(string name, object? value) =>
        new(name, value ?? DBNull.Value);

    /// <summary>
    /// Quick connectivity check against Azure SQL (SELECT 1).
    /// </summary>
    public static async Task<bool> CanConnectAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ExecuteScalarAsync<int>(
                "SELECT 1",
                cancellationToken: cancellationToken);
            return result == 1;
        }
        catch
        {
            return false;
        }
    }

    private static SqlCommand CreateCommand(
        SqlConnection connection,
        string sql,
        IEnumerable<SqlParameter>? parameters,
        SqlTransaction? transaction = null)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        if (transaction is not null)
        {
            command.Transaction = transaction;
        }

        if (parameters is not null)
        {
            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }
        }

        return command;
    }

    private static T? ConvertScalar<T>(object? result)
    {
        if (result is null or DBNull)
        {
            return default;
        }

        return (T)Convert.ChangeType(result, typeof(T));
    }

    private static string BuildConnectionString()
    {
        // Prefer a full connection string if provided
        var full = Env("SQL_CONNECTION_STRING") ?? Env("sql_connection_string");
        if (!string.IsNullOrWhiteSpace(full))
        {
            return full;
        }

        var server = RequireEnv("SQL_SERVER", "sql_server");
        var database = RequireEnv("SQL_DATABASE", "sql_database");
        var user = RequireEnv("SQL_USER", "sql_user");
        var password = RequireEnv("SQL_PASSWORD", "sql_password");

        // SqlConnectionStringBuilder correctly escapes special characters in passwords
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            UserID = user,
            Password = password,
            Encrypt = true,
            TrustServerCertificate = false,
            ConnectTimeout = 30,
            MultipleActiveResultSets = false,
        };

        return builder.ConnectionString;
    }

    private static string RequireEnv(params string[] names)
    {
        var value = Env(names);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing database setting. Set one of: {string.Join(", ", names)} " +
                "(in .env or the process environment).");
        }

        return value;
    }

    private static string? Env(params string[] names)
    {
        foreach (var name in names)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
}
