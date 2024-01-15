# ​DatabaseExporter Documentation

## ​Overview

The DatabaseExporter is a .NET application designed to export various database objects and data, such as tables, triggers, stored procedures, views, custom data types, user information, and more. The application utilizes SQL Server Management Objects (SMO) to script and export SQL Server database components.

## ​Components

### ​1. DatabaseExporter Application

The DatabaseExporter application is the main executable responsible for orchestrating the export process. It uses SQL Server Management Objects (SMO) to connect to a SQL Server database and script its objects.

### ​2. ExporterMethods Classses

The `ExporterMethods` classes contain methods for exporting different types of database objects, including tables, triggers, stored procedures, views, custom data types, user information, roles, and user-defined functions.

### ​3. SMO Helper Methods

Several helper methods are implemented to generate appropriate SQL scripts for different database objects. These methods include:

- `GetCreateTableScript`: Generates a script for creating a table, including handling triggers.
- `GetCreateProcedureScript`: Generates a script for creating stored procedures, considering the `SET QUOTED_IDENTIFIER` statement.
- `GetCreateViewScript`: Generates a script for creating views.
- `GetCreateDataTypeScript`: Generates a script for creating user-defined data types.
- `GetInsertScript`: Generates a script for inserting data into tables.
- `GetCreateUserScript`: Generates a script for creating database users.
- `GetCreateRoleScript`: Generates a script for creating database roles.
- `GetCreateFunctionScript`: Generates a script for creating user-defined functions.
- `GetCreateTriggerScript`: Generates a script for creating triggers, including handling the `SET QUOTED_IDENTIFIER` statement.

### ​4. app.config

The `app.config` file stores configuration settings for the DatabaseExporter application. Key components include:

- **Startup** : Specifies the target .NET runtime version for the application.
- **Connection Strings** : Defines connection strings for connecting to the SQL Server database.
- **App Settings** : Contains application-specific settings, such as the output directory for exported data.
- **Runtime Assembly Bindings** : Provides binding redirects for dependent assemblies to ensure compatibility.

## ​Configuration

### ​Connection Strings

Modify the connection string in the `app.config` file under the `<connectionStrings>` section to connect to the desired SQL Server database.

```
xml<add name="Initial"connectionString="Data Source=your_server;Initial Catalog=your_database;User Id=your_username;Password=your_password;"providerName="System.Data.SqlClient" />
```

### ​App Settings

Adjust the output directory in the `app.config` file under the `<appSettings>` section to specify the location where exported data will be saved.

```
xml<add key="outputDirectory" value="C:\Your\Output\Directory" />
```

## ​Instructions

**Download and Install .NET Runtime:** Ensure that you have the appropriate version of the .NET runtime installed on your machine. You can download it from the official .NET website.

1. **Compile the Application:**

- Open the solution in Visual Studio or any other preferred C# development environment.
- Build the solution to compile the DatabaseExporter application.

2. **Configure Database Connection:**

- Open the `app.config` file in the DatabaseExporter project.
- Locate the `<add>` element under the `<connectionStrings>` section.
- Replace the placeholder values in the `connectionString` attribute with your SQL Server connection details (server, database, username, and password).

3. **Set Output Directory (Optional):**

- In the `app.config` file, find the `<appSettings>` section.
- Modify the `value` attribute of the `<add>` element with the key `"outputDirectory"` to specify the directory where the exported data will be saved.

4. **Run the Application:**

- Start the DatabaseExporter application by clicking a Start button on the solution explorer.
- The application will connect to the specified SQL Server database and export the configured components.

5. **Monitor Progress:**

- The application will display progress messages in the console, indicating the export progress of different database components.

6. **Review Exported Data:**

- Once the export process is complete, check the specified output directory for the exported SQL scripts and data.

7. **Review Log Files:**

- Check the log files generated during the export process for any error messages or additional information.

## ​Usage

1. Build and run the DatabaseExporter application.
2. Monitor the console output for progress updates and completion messages.
3. Exported scripts and data will be saved in the specified output directory.

## ​Dependencies

- SQL Server Management Objects (SMO)
- .NET Framework 4.7.2
- Dependent assemblies with binding redirects as specified in the `app.config` file.

## ​Notes

- Ensure that the required assemblies are available in the application's runtime environment.
- Adjust connection strings and app settings according to your database and export preferences.
