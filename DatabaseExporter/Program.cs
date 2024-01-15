using System;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System.Text;
using System.Collections.Specialized;

namespace DatabaseExporter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["Initial"].ConnectionString;
            string outputDirectory = ConfigurationManager.AppSettings["outputDirectory"];

            string populationOutputDirectory = Path.Combine(outputDirectory, "DbPopulation");
            string schemaOutputDirectory = Path.Combine(outputDirectory, "DbSchema");

            if (!Directory.Exists(populationOutputDirectory))
            {
                Directory.CreateDirectory(populationOutputDirectory);
            }

            if (!Directory.Exists(schemaOutputDirectory))
            {
                Directory.CreateDirectory(schemaOutputDirectory);
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            string username = builder.UserID;
            string password = builder.Password;

            Server server = new Server(new ServerConnection(builder.DataSource, username, password));

            Database database = server.Databases[builder.InitialCatalog];

            ScriptingOptions scriptingOptions = new ScriptingOptions
            {
                ScriptSchema = true,
                ScriptData = false,
                Indexes = true,
                Triggers = true,
                NoCommandTerminator = false,
                ScriptBatchTerminator =true,
                ToFileOnly = true,
            };

            ScriptingOptions scriptingOptionsForPopulation = new ScriptingOptions
            {
                ScriptData = true,
                ScriptDrops = false,
                EnforceScriptingOptions = true,
                ScriptSchema = true,
                IncludeHeaders = true,
                AppendToFile = true,
                Indexes = true,
                WithDependencies = true,
                NoCommandTerminator = false,
                ScriptBatchTerminator = true,
                ToFileOnly= true,
                
            };

            ScriptingOptions scriptingOptionsForFunctions = new ScriptingOptions
            {
                ScriptSchema = true,
                ScriptData = false,
                Indexes = true,
                Triggers = true,
                ScriptDrops = false,
                IncludeIfNotExists = false,
                AnsiFile = false,
                NoCollation = true,
                SchemaQualify = false,
                NoCommandTerminator = false,
                ScriptBatchTerminator = true,
                ToFileOnly = true,

            };

            ScriptingOptions optionsWithoutTriggers = new ScriptingOptions
            {
                ScriptSchema = true,
                ScriptData = false,
                Indexes = true,
                NoCommandTerminator = false,
                ScriptBatchTerminator = true,
                ToFileOnly = true,
            };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string combinedTablesFilePath = Path.Combine(schemaOutputDirectory, "Tables_Export.sql");
                string combinedTriggersFilePath = Path.Combine(schemaOutputDirectory, "Triggers_Export.sql");
                string combinedProceduresFilePath = Path.Combine(schemaOutputDirectory, "Procedures_Export.sql");
                string combinedViewsFilePath = Path.Combine(schemaOutputDirectory, "Views_Export.sql");
                string combinedDataTypesFilePath = Path.Combine(schemaOutputDirectory, "DataTypes_Export.sql");
                string combinedUsersFilePath = Path.Combine(schemaOutputDirectory, "Users_Export.sql");
                string combinedRolesFilePath = Path.Combine(schemaOutputDirectory, "Roles_Export.sql");
                string combinedUserDefinedFunctionsFilePath = Path.Combine(schemaOutputDirectory, "UserDefinedFunctions_Export.sql");

                string logsFolderPath = Path.Combine(outputDirectory, "logs");

                Directory.CreateDirectory(logsFolderPath);

                string tableLogFilePath = Path.Combine(logsFolderPath, "TableExportLog.txt");
                string procedureLogFilePath = Path.Combine(logsFolderPath, "ProcedureExportLog.txt");
                string viewsLogFilePath = Path.Combine(logsFolderPath, "ViewsExportLog.txt");
                string customDataTypesLogFilePath = Path.Combine(logsFolderPath, "CustomDataTypesExportLog.txt");
                string usersLogFilePath = Path.Combine(logsFolderPath, "UsersExportLog.txt");
                string tablePopullationLogFilePath = Path.Combine(logsFolderPath, "PopulatingExportLog.txt");
                string rolesLogFilePath = Path.Combine(logsFolderPath, "RolesExportLog.txt");
                string userFunctionsLogFilePath = Path.Combine(logsFolderPath, "UserDefFunctionsExportLog.txt");
                string triggersFilePath = Path.Combine(logsFolderPath, "TriggersExportLog.txt");

                using (StreamWriter logWriter = new StreamWriter(tableLogFilePath, true))
                {
                    logWriter.WriteLine($"Export started at {DateTime.Now}");

                    ExportTables(database, connection, combinedTablesFilePath, optionsWithoutTriggers, logWriter);

                    logWriter.WriteLine($"Export completed. [{DateTime.Now}]");
                }

                using (StreamWriter logWriter = new StreamWriter(triggersFilePath, true))
                {
                    logWriter.WriteLine($"Export started at {DateTime.Now}");

                    ExportTriggers(database, connection, combinedTriggersFilePath, scriptingOptions, logWriter);

                    logWriter.WriteLine($"Export completed. [{DateTime.Now}]");
                }

                using (StreamWriter logWriter = new StreamWriter(procedureLogFilePath, true))
                {
                    logWriter.WriteLine($"Export started at {DateTime.Now}");

                    ExportStoredProcedures(database, connection, combinedProceduresFilePath, scriptingOptionsForFunctions, logWriter);

                    logWriter.WriteLine($"Export completed. [{DateTime.Now}]");
                }

                using (StreamWriter logWriter = new StreamWriter(viewsLogFilePath, true))
                {
                    logWriter.WriteLine($"Export started at {DateTime.Now}");

                    ExportViews(database, connection, tableLogFilePath, scriptingOptions, logWriter);

                    logWriter.WriteLine($"Export completed. [{DateTime.Now}]");
                }

                using (StreamWriter logWriter = new StreamWriter(customDataTypesLogFilePath, true))
                {
                    logWriter.WriteLine($"Export started at {DateTime.Now}");

                    ExportCustomDataTypes(database, connection, combinedDataTypesFilePath, scriptingOptions, logWriter);

                    logWriter.WriteLine($"Export completed. [{DateTime.Now}]");
                }

                using (StreamWriter logWriter = new StreamWriter(usersLogFilePath, true))
                {
                    logWriter.WriteLine($"Export started at {DateTime.Now}");

                    ExportUsers(database, connection, combinedUsersFilePath, scriptingOptions, logWriter);

                    logWriter.WriteLine($"Export completed. [{DateTime.Now}]");
                }

                using (StreamWriter logWriter = new StreamWriter(tablePopullationLogFilePath, true))
                {
                    logWriter.WriteLine($"Export started at {DateTime.Now}");

                    ExportTableData(database, connection, scriptingOptionsForPopulation, logWriter, populationOutputDirectory, server);

                    logWriter.WriteLine($"Export completed. [{DateTime.Now}]");
                }

                using (StreamWriter logWriter = new StreamWriter(rolesLogFilePath, true))
                {
                    logWriter.WriteLine($"Export started at {DateTime.Now}");

                    ExportRoles(database, connection, combinedRolesFilePath, scriptingOptions, logWriter);

                    logWriter.WriteLine($"Export completed. [{DateTime.Now}]");
                }

                using (StreamWriter logWriter = new StreamWriter(userFunctionsLogFilePath, true))
                {
                    logWriter.WriteLine($"Export started at {DateTime.Now}");

                    ExportUserDefFunctions(database, connection, userFunctionsLogFilePath, scriptingOptions, logWriter);

                    logWriter.WriteLine($"Export completed. [{DateTime.Now}]");
                }

                Console.WriteLine("All exports completed.");
            }
        }

        #region ExportRegion
        static void ExportTables(Database database, SqlConnection connection, string combinedTablesFilePath, ScriptingOptions optionsWithoutTriggers, StreamWriter logWriter)
        {
            try
            {
                Console.WriteLine("Tables export");
                logWriter.WriteLine($"Tables export started");

                int totalTables = database.Tables.Count;
                int processedTables = 0;

                foreach (Table table in database.Tables)
                {
                    string createTableScript = GetCreateTableScript(table, optionsWithoutTriggers);

                    using (StreamWriter writer = new StreamWriter(combinedTablesFilePath, true))
                    {

                        writer.WriteLine(createTableScript);
                        writer.WriteLine();
                        writer.WriteLine("GO");
                        writer.WriteLine();

                        processedTables++;
                        int progressPercentage = (int)((double)processedTables / totalTables * 100);
                        Console.WriteLine($"Tables progress: {progressPercentage}%");
                        logWriter.WriteLine($"Table '{table.Name}' exported. Progress: {progressPercentage}%. [{DateTime.Now}]");
                    }
                }

                Console.WriteLine("Tables export completed successfully.");
                logWriter.WriteLine($"Tables export completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured! {ex}");
                logWriter.WriteLine($"An exception occured! {ex}");
            }
        }

        static void ExportTriggers(Database database, SqlConnection connection, string combinedTriggersFilePath, ScriptingOptions scriptingOptions, StreamWriter logWriter)
        {
            try
            {
                Console.WriteLine("Triggers export");
                logWriter.WriteLine($"Triggers export started");

                int totalTables = database.Tables.Count;
                int processedTriggers = 0;

                foreach (Table table in database.Tables)
                {
                    foreach (Trigger trigger in table.Triggers)
                    {
                        string createTriggerScript = GetCreateTriggerScript(trigger, scriptingOptions);

                        using (StreamWriter writer = new StreamWriter(combinedTriggersFilePath, true))
                        {
                            writer.WriteLine(createTriggerScript);
                            writer.WriteLine();
                            writer.WriteLine("GO");
                            writer.WriteLine();

                            processedTriggers++;
                            int progressPercentage = (int)((double)processedTriggers / totalTables * 100);
                            Console.WriteLine($"Triggers progress: {progressPercentage}%");
                            logWriter.WriteLine($"Trigger '{trigger.Name}' exported. Progress: {progressPercentage}%. [{DateTime.Now}]");
                        }
                    }
                }

                Console.WriteLine("Triggers export completed successfully.");
                logWriter.WriteLine("Triggers export completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured! {ex}");
                logWriter.WriteLine($"An exception occured! {ex}");
            }
        }

        static void ExportStoredProcedures(Database database, SqlConnection connection, string combinedProceduresFilePath, ScriptingOptions scriptingOptionsForFunctions, StreamWriter logWriter)
        {
            try
            {
                Console.WriteLine("Stored Procedures export");
                logWriter.WriteLine($"Stored Procedures export started");

                int totalStoredProcedures = database.StoredProcedures.Count;
                int processedProcedures = 0;

                foreach (StoredProcedure procedure in database.StoredProcedures)
                {

                    if (procedure.IsSystemObject)
                    {
                        Console.WriteLine($"Skipping system procedure: {procedure.Name}");
                        continue;
                    }

                    string createProcedureScript = GetCreateProcedureScript(procedure, scriptingOptionsForFunctions);

                    using (StreamWriter writer = new StreamWriter(combinedProceduresFilePath, true))
                    {

                        writer.WriteLine(createProcedureScript);
                        writer.WriteLine();
                        writer.WriteLine("GO");
                        writer.WriteLine();

                        processedProcedures++;

                        int progressPercentage = (int)((double)processedProcedures / totalStoredProcedures * 100);
                        Console.WriteLine($"Procedure progress: {progressPercentage}%");
                        logWriter.WriteLine($"Procedure '{procedure.Name}' exported. Progress: {progressPercentage}%. [{DateTime.Now}]");
                    }
                }

                Console.WriteLine("Stored Procedures export completed successfully.");
                logWriter.WriteLine($"Stored Procedures export completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured! {ex}");
                logWriter.WriteLine($"An exception occured! {ex}");
            }
        }

        static void ExportViews(Database database, SqlConnection connection, string combinedViewsFilePath, ScriptingOptions scriptingOptions, StreamWriter logWriter)
        {
            try
            {
                Console.WriteLine("Views export");
                logWriter.WriteLine($"Views export started");

                int totalViews = database.Views.Count;
                int processedViews = 0;

                foreach (View view in database.Views)
                {

                    if (!view.IsSystemObject)
                    {
                        string createViewScript = GetCreateViewScript(view, scriptingOptions);

                        using (StreamWriter writer = new StreamWriter(combinedViewsFilePath, true))
                        {

                            writer.WriteLine(createViewScript);
                            writer.WriteLine();
                            writer.WriteLine("GO");
                            writer.WriteLine();

                            processedViews++;
                            int progressPercentage = (int)((double)processedViews / totalViews * 100);
                            Console.WriteLine($"View progress: {progressPercentage}%");
                            logWriter.WriteLine($"View '{view.Name}' exported. Progress: {progressPercentage}%. [{DateTime.Now}]");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Skipped system function {view}");
                        logWriter.WriteLine($"Skipped system function {view}");
                    }
                }

                Console.WriteLine("Views export completed successfully.");
                logWriter.WriteLine($"Views export completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured! {ex}");
                logWriter.WriteLine($"An exception occured! {ex}");
            }
        }

        static void ExportCustomDataTypes(Database database, SqlConnection connection, string combinedDataTypesFilePath, ScriptingOptions scriptingOptions, StreamWriter logWriter)
        {
            try
            {
                Console.WriteLine("Custom data types export");
                logWriter.WriteLine($"Custom data types export started");

                int totalDataTypes = database.UserDefinedDataTypes.Count;
                int processedDataTypes = 0;

                foreach (UserDefinedDataType dataType in database.UserDefinedDataTypes)
                {
                    string createDataTypeScript = GetCreateDataTypeScript(dataType, scriptingOptions);

                    using (StreamWriter writer = new StreamWriter(combinedDataTypesFilePath, true))
                    {

                        writer.WriteLine(createDataTypeScript);
                        writer.WriteLine();
                        writer.WriteLine("GO");
                        writer.WriteLine();

                        processedDataTypes++;
                        int progressPercentage = (int)((double)processedDataTypes / totalDataTypes * 100);
                        Console.WriteLine($"Costom data types progress: {progressPercentage}%");
                        logWriter.WriteLine($"Custom data type '{dataType.Name}' exported. [{DateTime.Now}]");
                    }
                }

                Console.WriteLine("Custom data types export completed successfully.");
                logWriter.WriteLine($"Custom data types export completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured! {ex}");
                logWriter.WriteLine($"An exception occured! {ex}");
            }
        }

        static void ExportTableData(Database database, SqlConnection connection, ScriptingOptions scriptingOptionsForPopulation, StreamWriter logWriter, string dataPopulationScriptsDirectory, Server server)
        {
            try
            {
                Console.WriteLine("Table data export using SMO");
                logWriter.WriteLine($"Table data export started");

                int totalTablesData = database.Tables.Count;
                int processedDataTypes = 0;

                foreach (Table table in database.Tables)
                {
                    string insertScript = GetInsertScript(table, scriptingOptionsForPopulation, server);

                    string dataPopulationScriptsFilePath = Path.Combine(dataPopulationScriptsDirectory, $"dbo.{table.Name}_DataScript.sql");

                    using (StreamWriter writer = new StreamWriter(dataPopulationScriptsFilePath))
                    {

                        writer.WriteLine(insertScript);
                        writer.WriteLine();
                        writer.WriteLine("GO");
                        writer.WriteLine();

                        processedDataTypes++;
                        int progressPercentage = (int)((double)processedDataTypes / totalTablesData * 100);
                        Console.WriteLine($"Population scripting progress: {progressPercentage}%");
                        logWriter.WriteLine($"Table data for '{table.Name}' exported. [{DateTime.Now}]");
                    }
                }

                Console.WriteLine("Table data export completed successfully.");
                logWriter.WriteLine($"Table data export completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured! {ex}");
                logWriter.WriteLine($"An exception occured! {ex}");
            }
        }

        static void ExportUsers(Database database, SqlConnection connection, string combinedUsersFilePath, ScriptingOptions scriptingOptions, StreamWriter logWriter)
        {
            try
            {
                Console.WriteLine("Users export");
                logWriter.WriteLine($"Users export started");

                int totalUsers = database.Users.Count;
                int processedUsers = 0;

                foreach (User user in database.Users)
                {

                    if (!user.IsSystemObject)
                    {
                        string createUserScript = GetCreateUserScript(user, scriptingOptions);

                        using (StreamWriter writer = new StreamWriter(combinedUsersFilePath, true))
                        {

                            writer.WriteLine(createUserScript);
                            writer.WriteLine();
                            writer.WriteLine("GO");
                            writer.WriteLine();

                            processedUsers++;
                            int progressPercentage = (int)((double)processedUsers / totalUsers * 100);
                            Console.WriteLine($"User progress: {progressPercentage}%");
                            logWriter.WriteLine($"User '{user.Name}' exported. Progress: {progressPercentage}%. [{DateTime.Now}]");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Skipped system function {user}");
                        logWriter.WriteLine($"Skipped system function {user}");
                    }
                }
                Console.WriteLine("Users export completed successfully.");
                logWriter.WriteLine($"Users export completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured! {ex}");
                logWriter.WriteLine($"An exception occured! {ex}");
            }
        }

        static void ExportRoles(Database database, SqlConnection connection, string combinedRolesFilePath, ScriptingOptions scriptingOptions, StreamWriter logWriter)
        {
            try
            {
                Console.WriteLine("Roles export");
                logWriter.WriteLine($"Roles export started");

                int totalRoles = database.Roles.Count;
                int processedRoles = 0;

                foreach (DatabaseRole role in database.Roles)
                {
                    string createRoleScript = GetCreateRoleScript(role, scriptingOptions);

                    using (StreamWriter writer = new StreamWriter(combinedRolesFilePath, true))
                    {

                        writer.WriteLine(createRoleScript);
                        writer.WriteLine();
                        writer.WriteLine("GO");
                        writer.WriteLine();

                        processedRoles++;
                        int progressPercentage = (int)((double)processedRoles / totalRoles * 100);
                        Console.WriteLine($"Roles progress: {progressPercentage}%");
                        logWriter.WriteLine($"Role '{role.Name}' exported. Progress: {progressPercentage}%. [{DateTime.Now}]");
                    }
                }

                Console.WriteLine("Roles export completed successfully.");
                logWriter.WriteLine($"Roles export completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured! {ex}");
                logWriter.WriteLine($"An exception occured! {ex}");
            }
        }

        static void ExportUserDefFunctions(Database database, SqlConnection connection, string combinedUserDefinedFunctionsFilePath, ScriptingOptions scriptingOptions, StreamWriter logWriter)
        {
            try
            {
                Console.WriteLine("Functions export");
                logWriter.WriteLine($"Functions export started");

                int totalFunctions = database.UserDefinedFunctions.Count;
                int processedFunctions = 0;

                foreach (UserDefinedFunction function in database.UserDefinedFunctions)
                {
                    if (!function.IsSystemObject)
                    {
                        string createFunctionScript = GetCreateFunctionScript(function, scriptingOptions);

                        using (StreamWriter writer = new StreamWriter(combinedUserDefinedFunctionsFilePath, true))
                        {

                            writer.WriteLine(createFunctionScript);
                            writer.WriteLine();
                            writer.WriteLine("GO");
                            writer.WriteLine();

                            processedFunctions++;
                            int progressPercentage = (int)((double)processedFunctions / totalFunctions * 100);
                            Console.WriteLine($"Functions progress: {progressPercentage}%");
                            logWriter.WriteLine($"Function '{function.Name}' exported. Progress: {progressPercentage}%. [{DateTime.Now}]");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Skipped system function {function}");
                        logWriter.WriteLine($"Skipped system function {function}");
                    }
                }

                Console.WriteLine("Functions export completed successfully.");
                logWriter.WriteLine($"Functions export completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occured! {ex}");
                logWriter.WriteLine($"An exception occured! {ex}");
            }
        }

        #endregion
        #region GetCreateRegion

        static string GetCreateTableScript(Table table, ScriptingOptions scriptingOptions)
        {
            StringBuilder scriptBuilder = new StringBuilder();
            StringCollection script = table.Script(scriptingOptions);

            for (int i = 0; i < script.Count; i++)
            {
                // Check if the line contains "CREATE TRIGGER"
                if (script[i].IndexOf("CREATE TRIGGER", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    // Add a "GO" statement on a new line before the line containing "CREATE TRIGGER"
                    scriptBuilder.AppendLine("GO");
                }

                scriptBuilder.AppendLine(script[i]);
            }

            return scriptBuilder.ToString();
        }



        static string GetCreateProcedureScript(StoredProcedure procedure, ScriptingOptions scriptingOptionsForFunctions)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            StringCollection script = procedure.Script(scriptingOptionsForFunctions);

            for (int i = 0; i < script.Count; i++)
            {
                // Check if the line is SET QUOTED_IDENTIFIER ON or SET QUOTED_IDENTIFIER OFF
                if (script[i].Trim().StartsWith("SET QUOTED_IDENTIFIER", StringComparison.OrdinalIgnoreCase))
                {
                    // Add a "GO" statement on a new line after the SET QUOTED_IDENTIFIER statement
                    scriptBuilder.AppendLine(script[i]);
                    scriptBuilder.AppendLine("GO");
                }
                else
                {
                    scriptBuilder.AppendLine(script[i]);
                }
            }

            return scriptBuilder.ToString();
        }


        static string GetCreateViewScript(View view, ScriptingOptions scriptingOptions)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            StringCollection script = view.Script(scriptingOptions);

            foreach (string line in script)
            {
                scriptBuilder.AppendLine(line);
            }

            return scriptBuilder.ToString();
        }

        static string GetCreateDataTypeScript(UserDefinedDataType dataType, ScriptingOptions scriptingOptions)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            StringCollection script = dataType.Script(scriptingOptions);

            foreach (string line in script)
            {
                scriptBuilder.AppendLine(line);
            }

            return scriptBuilder.ToString();
        }

        static string GetInsertScript(Table table, ScriptingOptions scriptingOptionsForPopulation, Server server)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            var scripter = new Scripter(server) { Options = { ScriptData = true, ScriptSchema = false } };
            var script = scripter.EnumScript(new SqlSmoObject[] { table });
            foreach (var line in script)
            {
                scriptBuilder.AppendLine(line);
            }
            return scriptBuilder.ToString();
        }

        static string GetCreateUserScript(User user, ScriptingOptions scriptingOptions)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            StringCollection script = user.Script(scriptingOptions);

            foreach (string line in script)
            {
                scriptBuilder.AppendLine(line);
            }

            return scriptBuilder.ToString();
        }

        static string GetCreateRoleScript(DatabaseRole role, ScriptingOptions scriptingOptions)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            StringCollection script = role.Script(scriptingOptions);

            foreach (string line in script)
            {
                scriptBuilder.AppendLine(line);
            }

            return scriptBuilder.ToString();
        }

        static string GetCreateFunctionScript(UserDefinedFunction function, ScriptingOptions scriptingOptions)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            StringCollection script = function.Script(scriptingOptions);

            foreach (string line in script)
            {
                scriptBuilder.AppendLine(line);
            }

            return scriptBuilder.ToString();
        }

        static string GetCreateTriggerScript(Trigger trigger, ScriptingOptions scriptingOptions)
        {
            StringBuilder scriptBuilder = new StringBuilder();

            StringCollection script = trigger.Script(scriptingOptions);
            for (int i = 0; i < script.Count; i++)
            {
                // Check if the line is SET QUOTED_IDENTIFIER ON or SET QUOTED_IDENTIFIER OFF
                if (script[i].Trim().StartsWith("SET QUOTED_IDENTIFIER", StringComparison.OrdinalIgnoreCase))
                {
                    // Add a "GO" statement on a new line after the SET QUOTED_IDENTIFIER statement
                    scriptBuilder.AppendLine(script[i]);
                    scriptBuilder.AppendLine("GO");
                }
                else
                {
                    scriptBuilder.AppendLine(script[i]);
                }
            }

            return scriptBuilder.ToString();
        }

        #endregion
    }
}