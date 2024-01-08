using System;
using System.Data.SqlClient;
using System.IO;

namespace DatabaseExporter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=censored;Database=censored;User Id=censored;Password=censored;";
            string outputDirectory = @"censored";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                ExportTables(connection, outputDirectory);
                ExportViews(connection, outputDirectory);
                ExportProcedures(connection, outputDirectory);
                ExportFunctions(connection, outputDirectory);

                Console.WriteLine("Export completed successfully.");
            }
        }

        static void ExportTables(SqlConnection connection, string outputDirectory)
        {
            // Fetch tables
            using (SqlCommand command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                // Create or overwrite the tables.txt file
                using (StreamWriter writer = new StreamWriter(Path.Combine(outputDirectory, "tables.txt")))
                {
                    while (reader.Read())
                    {
                        string tableName = reader["TABLE_NAME"].ToString();
                        // Write the table name to the file
                        writer.WriteLine(tableName);
                    }
                }
            }
        }

        static void ExportViews(SqlConnection connection, string outputDirectory)
        {
            // Fetch views
            using (SqlCommand command = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.VIEWS", connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                // Create or overwrite the views.txt file
                using (StreamWriter writer = new StreamWriter(Path.Combine(outputDirectory, "views.txt")))
                {
                    while (reader.Read())
                    {
                        string viewName = reader["TABLE_NAME"].ToString();
                        // Write the view name to the file
                        writer.WriteLine(viewName);
                    }
                }
            }
        }

        static void ExportProcedures(SqlConnection connection, string outputDirectory)
        {
            // Fetch procedures
            using (SqlCommand command = new SqlCommand("SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE'", connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                // Create or overwrite the procedures.txt file
                using (StreamWriter writer = new StreamWriter(Path.Combine(outputDirectory, "procedures.txt")))
                {
                    while (reader.Read())
                    {
                        string procedureName = reader["ROUTINE_NAME"].ToString();
                        // Write the procedure name to the file
                        writer.WriteLine(procedureName);
                    }
                }
            }
        }

        static void ExportFunctions(SqlConnection connection, string outputDirectory)
        {
            // Fetch functions
            using (SqlCommand command = new SqlCommand("SELECT ROUTINE_NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION'", connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                // Create or overwrite the functions.txt file
                using (StreamWriter writer = new StreamWriter(Path.Combine(outputDirectory, "functions.txt")))
                {
                    while (reader.Read())
                    {
                        string functionName = reader["ROUTINE_NAME"].ToString();
                        // Write the function name to the file
                        writer.WriteLine(functionName);
                    }
                }
            }
        }
    }
}
