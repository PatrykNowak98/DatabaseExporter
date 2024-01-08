using System;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

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
            Server server = new Server(new ServerConnection(connection.ConnectionString));

            // Fetch tables
            foreach (Table table in server.Databases[connection.Database].Tables)
            {
                string tableName = table.Name;
                string createTableScript = GetCreateTableScript(table);

                // Create or append to the CreateTableScript.sql file
                string scriptFilePath = Path.Combine(outputDirectory, "CreateTableScript.sql");
                using (StreamWriter writer = new StreamWriter(scriptFilePath, true))
                {
                    // Write the create table script to the file
                    writer.WriteLine(createTableScript);
                    writer.WriteLine(); // Add a blank line between tables for better readability
                }
            }
        }

        static string GetCreateTableScript(Table table)
        {
            // Concatenate the lines from StringCollection to form the complete script
            string createTableScript = string.Join(Environment.NewLine, table.Script());

            return createTableScript;
        }
    }
}
