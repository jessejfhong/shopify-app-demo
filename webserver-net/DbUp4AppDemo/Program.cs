using DbUp;
using Microsoft.Extensions.Configuration;
using System.Reflection;

var arg = args.FirstOrDefault() ?? string.Empty;

var configBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json");

if (arg.ToLower() == "dev")
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Running in Development mode!");
    Console.ResetColor();

    configBuilder.AddJsonFile($"appsettings.Development.json");
}

var config = configBuilder.Build();

var connStr = config.GetConnectionString("postgres");

EnsureDatabase.For.PostgresqlDatabase(connStr);

var upgrader =
    DeployChanges.To
        .PostgresqlDatabase(connStr)
        .WithScriptsAndCodeEmbeddedInAssembly(Assembly.GetExecutingAssembly())
        .LogToConsole()
        .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();

    return -1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Success!");
Console.ResetColor();

if (arg.ToLower() == "dev")
{
    Console.ReadLine();
}

return 0;
