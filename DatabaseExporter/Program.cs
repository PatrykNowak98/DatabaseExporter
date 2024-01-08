using System;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;

namespace DatabaseExporter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["Initial"].ConnectionString;
            string outputDirectory = ConfigurationManager.AppSettings["outputDirectory"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                ExportTables(connection, outputDirectory);

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
    }
}
