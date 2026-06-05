using System.Data;
using Microsoft.Data.SqlClient;

namespace MuzikPlatformuApp;

internal static class Db
{
    public const string ConnectionString =
        @"Server=PC-A4H34625\SQLEXPRESS;Database=MuzikPlatformuDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";

    public static DataTable Query(string sql, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);

        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static DataTable StoredProcedure(string procedureName, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddRange(parameters);

        using var adapter = new SqlDataAdapter(command);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    public static void ExecuteStoredProcedure(string procedureName, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(ConnectionString);
        using var command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddRange(parameters);

        connection.Open();
        command.ExecuteNonQuery();
    }
}
