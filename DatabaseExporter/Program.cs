using System;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Text;
using System.Collections.Specialized;
using Microsoft.SqlServer.Management.Sdk.Sfc;

namespace DatabaseExporter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["Initial"].ConnectionString;
            string outputDirectory = ConfigurationManager.AppSettings["outputDirectory"];

            // Extract username and password from the connection string
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            string username = builder.UserID;
            string password = builder.Password;

            Server server = new Server(new ServerConnection(builder.DataSource, username, password));

            // Access the database from which you want to script the tables
            Database database = server.Databases[builder.InitialCatalog];

            // Set scripting options
            ScriptingOptions scriptingOptions = new ScriptingOptions
            {
                ScriptSchema = true,
                ScriptData = false,
                Indexes = true,
                Triggers = true
                // You can set more options as needed
            };

            //Set scriptiong options for population
            ScriptingOptions scriptingOptionsForPopulation = new ScriptingOptions
            {
                ScriptData = true,
                ScriptDrops = false,
                EnforceScriptingOptions = true,
                ScriptSchema = true,
                IncludeHeaders = true,
                AppendToFile = true,
                Indexes = true,
                WithDependencies = true
            };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Specify the path for the combined script file
                string combinedTablesFilePath = Path.Combine(outputDirectory, "Tables_Export.sql");
                string combinedProceduresFilePath = Path.Combine(outputDirectory, "Procedures_Export.sql");
                string combinedViewsFilePath = Path.Combine(outputDirectory, "Views_Export.sql");
                string combinedDataTypesFilePath = Path.Combine(outputDirectory, "DataTypes_Export.sql");

                // Create a "logs" folder within the output directory
                string logsFolderPath = Path.Combine(outputDirectory, "logs");
                Directory.CreateDirectory(logsFolderPath);

                // Specify the path for the log file within the "logs" folder
                string logFilePath = Path.Combine(logsFolderPath, "ExportLog.txt");

                using (StreamWriter logWriter = new StreamWriter(logFilePath, true))
                {
                    // Log the start of the export
                    logWriter.WriteLine($"Export started at {DateTime.Now}");

                    // Export tables
                    /*ExportTables(database, connection, combinedTablesFilePath, scriptingOptions, logWriter);
                    ExportStoredProcedures(database, connection, combinedProceduresFilePath, scriptingOptions, logWriter);
                    ExportViews(database, connection, combinedViewsFilePath, scriptingOptions, logWriter);
                    ExportCustomDataTypes(database, connection, combinedDataTypesFilePath, scriptingOptions, logWriter);*/
                    ExportTableData(database, connection, scriptingOptionsForPopulation, logWriter, outputDirectory);

                    // Log the completion of the export
                    logWriter.WriteLine($"Export completed at {DateTime.Now}");
                }

                Console.WriteLine("All exports completed.");
            }
        }

        static void ExportTables(Database database, SqlConnection connection, string combinedTablesFilePath, ScriptingOptions scriptingOptions, StreamWriter logWriter)
        {
            Console.WriteLine("Tables export");
            logWriter.WriteLine($"Tables export started");

            int totalTables = database.Tables.Count;
            int processedTables = 0;

            // Loop through the tables in the database and generate scripts
            foreach (Table table in database.Tables)
            {
                // Check if the table is a system table
                if (!table.IsSystemObject)
                {
                    string createTableScript = GetCreateTableScript(table, scriptingOptions);

                    // Append the create table script to the combined script file
                    using (StreamWriter writer = new StreamWriter(combinedTablesFilePath, true))
                    {
                        // Write the create table script to the file
                        writer.WriteLine(createTableScript);
                        writer.WriteLine(); // Add a blank line between tables for better readability
                        // Log the progress
                        processedTables++;
                        int progressPercentage = (int)((double)processedTables / totalTables * 100);
                        Console.WriteLine($"Tasks progress: {progressPercentage}%");
                        logWriter.WriteLine($"Table '{table.Name}' exported. Progress: {progressPercentage}%. [{DateTime.Now}]");
                    }
                }
            }
            Console.WriteLine("Tables export completed successfully.");
            logWriter.WriteLine($"Tables export completed");
        }

        static void ExportStoredProcedures(Database database, SqlConnection connection, string combinedProceduresFilePath, ScriptingOptions scriptingOptions, StreamWriter logWriter)
        {
            Console.WriteLine("Stored Procedures export");
            logWriter.WriteLine($"Stored Procedures export started");

            // Specify the path for the combined script file

            // Get the total number of stored procedures for calculating progress
            int totalProcedures = database.StoredProcedures.Count;
            int processedProcedures = 0;

            // Loop through the stored procedures in the database and generate scripts
            foreach (StoredProcedure procedure in database.StoredProcedures)
            {
                // Skip system procedures
                if (procedure.IsSystemObject)
                {
                    Console.WriteLine($"Skipping system procedure: {procedure.Name}");
                    continue;
                }

                string createProcedureScript = GetCreateProcedureScript(procedure, scriptingOptions);

                // Append the create procedure script to the combined script file
                using (StreamWriter writer = new StreamWriter(combinedProceduresFilePath, true))
                {
                    // Write the create procedure script to the file
                    writer.WriteLine(createProcedureScript);
                    writer.WriteLine(); // Add a blank line between procedures for better readability
                                        // Increment the processed procedures count
                    processedProcedures++;

                    // Calculate and display the progress percentage
                    int progressPercentage = (int)((double)processedProcedures / totalProcedures * 100);
                    Console.WriteLine($"Procedure progress: {progressPercentage}%");
                    logWriter.WriteLine($"Procedure '{procedure.Name}' exported. Progress: {progressPercentage}%. [{DateTime.Now}]");
                }
            }

            Console.WriteLine("Stored Procedures export completed successfully.");
            logWriter.WriteLine($"Stored Procedures export completed");
        }

        static void ExportViews(Database database, SqlConnection connection, string combinedViewsFilePath, ScriptingOptions scriptingOptions, StreamWriter logWriter)
        {
            Console.WriteLine("Views export");
            logWriter.WriteLine($"Views export started");

            int totalViews = database.Views.Count;
            int processedViews = 0;
            try
            {
                // Loop through the views in the database and generate scripts
                foreach (View view in database.Views)
                {
                    // Check if the view is a system object
                    if (!view.IsSystemObject)
                    {
                        string createViewScript = GetCreateViewScript(view, scriptingOptions);

                        // Append the create view script to the combined script file
                        using (StreamWriter writer = new StreamWriter(combinedViewsFilePath, true))
                        {
                            // Write the create view script to the file
                            writer.WriteLine(createViewScript);
                            writer.WriteLine(); // Add a blank line between views for better readability

                            // Log the progress
                            processedViews++;
                            int progressPercentage = (int)((double)processedViews / totalViews * 100);
                            Console.WriteLine($"View progress: {progressPercentage}%");
                            logWriter.WriteLine($"View '{view.Name}' exported. Progress: {progressPercentage}%. [{DateTime.Now}]");
                        }
                    }
                }

                Console.WriteLine("Views export completed successfully.");
                logWriter.WriteLine($"Views export completed");
            }
            catch (Exception ex)
            {
                logWriter.WriteLine($"An exception occured. Exception message:" + ex);
            }
        }

        static void ExportCustomDataTypes(Database database, SqlConnection connection, string combinedDataTypesFilePath, ScriptingOptions scriptingOptions, StreamWriter logWriter)
        {
            Console.WriteLine("Custom data types export");
            logWriter.WriteLine($"Custom data types export started");

            int totalDataTypes = database.UserDefinedDataTypes.Count;
            int processedDataTypes = 0;

            // Loop through the UserDefinedDataTypes in the database and generate scripts
            foreach (UserDefinedDataType dataType in database.UserDefinedDataTypes)
            {
                string createDataTypeScript = GetCreateDataTypeScript(dataType, scriptingOptions);

                // Append the create data type script to the combined script file
                using (StreamWriter writer = new StreamWriter(combinedDataTypesFilePath, true))
                {
                    // Write the create data type script to the file
                    writer.WriteLine(createDataTypeScript);
                    writer.WriteLine(); // Add a blank line for better readability
                                        // Log the progress
                    processedDataTypes++;
                    int progressPercentage = (int)((double)processedDataTypes / totalDataTypes * 100);
                    Console.WriteLine($"Costom data types progress: {progressPercentage}%");
                    logWriter.WriteLine($"Custom data type '{dataType.Name}' exported. [{DateTime.Now}]");
                }
            }

            Console.WriteLine("Custom data types export completed successfully.");
            logWriter.WriteLine($"Custom data types export completed");
        }

        static void ExportTableData(Database database, SqlConnection connection, ScriptingOptions scriptingOptionsForPopulation, StreamWriter logWriter, string dataPopulationScriptsDirectory)
        {
            Console.WriteLine("Table data export using SMO");
            logWriter.WriteLine($"Table data export started");

            int totalDataTypes = database.Tables.Count;
            int processedDataTypes = 0;

            foreach (Table table in database.Tables)
            {
                string insertScript = GetInsertScript(table, scriptingOptionsForPopulation);

                // Create a script file for each table
                string dataPopulationScriptsFilePath = Path.Combine(dataPopulationScriptsDirectory, $"{table.Name}_DataScript.sql");

                // Append the insert script to the individual script file
                using (StreamWriter writer = new StreamWriter(dataPopulationScriptsFilePath))
                {
                    // Write the insert script to the file
                    writer.WriteLine(insertScript);
                    // Log the progress
                    processedDataTypes++;
                    int progressPercentage = (int)((double)processedDataTypes / totalDataTypes * 100);
                    Console.WriteLine($"Population scripting progress: {progressPercentage}%");
                    logWriter.WriteLine($"Table data for '{table.Name}' exported. [{DateTime.Now}]");
                }
            }

            Console.WriteLine("Table data export completed successfully.");
            logWriter.WriteLine($"Table data export completed");
        }

        static string GetCreateProcedureScript(StoredProcedure procedure, ScriptingOptions scriptingOptions)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            // Generate the script for the procedure based on the specified options
            StringCollection script = procedure.Script(scriptingOptions);

            // Concatenate the lines from StringCollection to form the complete script
            foreach (string line in script)
            {
                scriptBuilder.AppendLine(line);
            }

            return scriptBuilder.ToString();
        }

        static string GetCreateTableScript(Table table, ScriptingOptions scriptingOptions)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            // Generate the script for the table based on the specified options
            StringCollection script = table.Script(scriptingOptions);

            // Concatenate the lines from StringCollection to form the complete script
            foreach (string line in script)
            {
                scriptBuilder.AppendLine(line);
            }

            return scriptBuilder.ToString();
        }

        static string GetCreateViewScript(View view, ScriptingOptions scriptingOptions)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            // Generate the script for the view based on the specified options
            StringCollection script = view.Script(scriptingOptions);

            // Concatenate the lines from StringCollection to form the complete script
            foreach (string line in script)
            {
                scriptBuilder.AppendLine(line);
            }

            return scriptBuilder.ToString();
        }

        static string GetCreateDataTypeScript(UserDefinedDataType dataType, ScriptingOptions scriptingOptions)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            // Generate the script for the user-defined data type based on the specified options
            StringCollection script = dataType.Script(scriptingOptions);

            // Concatenate the lines from StringCollection to form the complete script
            foreach (string line in script)
            {
                scriptBuilder.AppendLine(line);
            }

            return scriptBuilder.ToString();
        }

        static string GetInsertScript(Table table, ScriptingOptions scriptingOptionsForPopulation)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            // Retrieve data from the table and generate INSERT statements using SMO

            // Generate the script for the user-defined data type based on the specified options
            scriptingOptionsForPopulation.ScriptData = true;
            StringCollection script = table.Script(scriptingOptionsForPopulation);

            foreach (string line in script)
            {
                scriptBuilder.AppendLine(line);
            }

            return scriptBuilder.ToString();
        }
    }
}
