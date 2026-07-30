using System.Data;
using Microsoft.Data.SqlClient;
using Playground.Util;

namespace Playground.Data;

public class User
{
    public int UserId { get; set; }
    public DateTimeOffset Created { get; set; }
    public int? CreatedBy { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public int? LastModifiedBy { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// When set, the user is inactive as of that timestamp. Null means active.
    /// </summary>
    public DateTimeOffset? InActive { get; set; }

    public bool IsInactive => InActive is not null;

    public User()
    {
    }

    public User(
        int userId,
        DateTimeOffset created,
        int? createdBy,
        DateTimeOffset? lastModified,
        int? lastModifiedBy,
        string displayName,
        string email,
        DateTimeOffset? inActive)
    {
        UserId = userId;
        Created = created;
        CreatedBy = createdBy;
        LastModified = lastModified;
        LastModifiedBy = lastModifiedBy;
        DisplayName = displayName;
        Email = email;
        InActive = inActive;
    }

    public static List<User> List(bool includeInactive = true)
    {
        var sql = includeInactive
            ? "SELECT * FROM Users ORDER BY UserID"
            : "SELECT * FROM Users WHERE InActive IS NULL ORDER BY UserID";

        var table = DB.ExecuteQuery(sql);
        return table.Rows.Cast<DataRow>().Select(FromRow).ToList();
    }

    public static User? ReadSingle(int userId)
    {
        var table = DB.ExecuteQuery(
            "SELECT * FROM Users WHERE UserID = @UserID",
            DB.Param("@UserID", userId));

        return table.Rows.Count == 0 ? null : FromRow(table.Rows[0]);
    }

    public static User? ReadSingleByEmail(string email)
    {
        var table = DB.ExecuteQuery(
            "SELECT * FROM Users WHERE Email = @Email",
            DB.Param("@Email", email));

        return table.Rows.Count == 0 ? null : FromRow(table.Rows[0]);
    }

    public static User? ReadSingleByDisplayName(string displayName)
    {
        var table = DB.ExecuteQuery(
            "SELECT * FROM Users WHERE DisplayName = @DisplayName",
            DB.Param("@DisplayName", displayName));

        return table.Rows.Count == 0 ? null : FromRow(table.Rows[0]);
    }

    public static User Create(int createdBy, string displayName, string email, bool inActive = false)
    {
        var now = DateTimeOffset.UtcNow;
        var table = DB.ExecuteQuery(
            """
            INSERT INTO Users (Created, CreatedBy, LastModified, LastModifiedBy, DisplayName, Email, InActive)
            OUTPUT INSERTED.*
            VALUES (@Created, @CreatedBy, @LastModified, @LastModifiedBy, @DisplayName, @Email, @InActive)
            """,
            DB.Param("@Created", now),
            DB.Param("@CreatedBy", createdBy),
            DB.Param("@LastModified", now),
            DB.Param("@LastModifiedBy", createdBy),
            DB.Param("@DisplayName", displayName),
            DB.Param("@Email", email),
            DB.Param("@InActive", inActive ? now : null));

        if (table.Rows.Count == 0)
        {
            throw new InvalidOperationException("Insert succeeded but no row was returned.");
        }

        return FromRow(table.Rows[0]);
    }

    public static User? Update(int userId, int lastModifiedBy, string displayName, string email, bool inActive)
    {
        var existing = ReadSingle(userId);
        if (existing is null)
        {
            return null;
        }

        DateTimeOffset? inActiveValue = inActive
            ? (existing.InActive ?? DateTimeOffset.UtcNow)
            : null;

        var table = DB.ExecuteQuery(
            """
            UPDATE Users
            SET LastModified = @LastModified,
                LastModifiedBy = @LastModifiedBy,
                DisplayName = @DisplayName,
                Email = @Email,
                InActive = @InActive
            OUTPUT INSERTED.*
            WHERE UserID = @UserID
            """,
            DB.Param("@UserID", userId),
            DB.Param("@LastModified", DateTimeOffset.UtcNow),
            DB.Param("@LastModifiedBy", lastModifiedBy),
            DB.Param("@DisplayName", displayName),
            DB.Param("@Email", email),
            DB.Param("@InActive", inActiveValue));

        return table.Rows.Count == 0 ? null : FromRow(table.Rows[0]);
    }

    public static User? Inactivate(int userId, int lastModifiedBy)
    {
        var table = DB.ExecuteQuery(
            """
            UPDATE Users
            SET LastModified = @LastModified,
                LastModifiedBy = @LastModifiedBy,
                InActive = @InActive
            OUTPUT INSERTED.*
            WHERE UserID = @UserID
            """,
            DB.Param("@UserID", userId),
            DB.Param("@LastModified", DateTimeOffset.UtcNow),
            DB.Param("@LastModifiedBy", lastModifiedBy),
            DB.Param("@InActive", DateTimeOffset.UtcNow));

        return table.Rows.Count == 0 ? null : FromRow(table.Rows[0]);
    }

    public static bool Delete(int userId)
    {
        var rows = DB.ExecuteNonQuery(
            "DELETE FROM Users WHERE UserID = @UserID",
            DB.Param("@UserID", userId));
        return rows > 0;
    }

    private static User FromRow(DataRow row) => new(
        Convert.ToInt32(row["UserID"]),
        (DateTimeOffset)row["Created"],
        row["CreatedBy"] is DBNull ? null : Convert.ToInt32(row["CreatedBy"]),
        row["LastModified"] is DBNull ? null : (DateTimeOffset)row["LastModified"],
        row["LastModifiedBy"] is DBNull ? null : Convert.ToInt32(row["LastModifiedBy"]),
        row["DisplayName"]?.ToString() ?? string.Empty,
        row["Email"]?.ToString() ?? string.Empty,
        row["InActive"] is DBNull ? null : (DateTimeOffset)row["InActive"]);
}
